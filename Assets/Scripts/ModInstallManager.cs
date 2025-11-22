using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using SFB;
using static Logger;

public class ModInstallManager : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI References")]
    public Button selectFileButton;
    public GameObject dropArea; // 可拖拽区域（用于高亮等）
    public GameObject confirmDialog; // 确认弹窗
    public Text confirmDialogText; // 确认弹窗文本（显示插件名/版本）
    public Button confirmButton; // 确认按钮
    public Button cancelButton; // 取消按钮
    public Image previewIcon;
    public Text previewNameText;

    public UIManager uiManager;

    private string selectedFilePath;
    private string pluginName;
    private string version;
    private string dirPath;
    private IniFileReader localizationManager;
    public string languageCode;

    void Start()
    {
        dirPath = Path.Combine(Application.dataPath, "..", "Mods");

        // 绑定按钮以控制窗口显示
        if (selectFileButton != null)
        {
            var fadeController = GetComponent<UIElementFadeController>();
            if (fadeController != null)
            {
                selectFileButton.onClick.RemoveAllListeners();
                selectFileButton.onClick.AddListener(fadeController.ActivateComponent);
            }
        }

        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(OpenFileSelector);
        }

        if (cancelButton != null)
            cancelButton.onClick.AddListener(HideConfirmDialog);

        // 读取本地化（可选）
        try
        {
            var appConfigPath = Path.Combine(Application.dataPath, "..", "AppConfig.Schale");
            if (File.Exists(appConfigPath))
            {
                localizationManager = new IniFileReader(appConfigPath);
                languageCode = localizationManager.GetValue("Localization", "DisplayLanguage");
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("Failed to load AppConfig.Schale: " + ex.Message);
            Logger.LogWarning("Failed to load AppConfig.Schale: " + ex.Message);
        }

        HideConfirmDialog();
    }

    // 打开文件选择对话框（StandaloneFileBrowser）
    public void OpenFileSelector()
    {
        var extensions = new[] {
            new ExtensionFilter( "Izakaya File", "izakaya" ),
            new ExtensionFilter( "ZIP File", "zip" ),
            new ExtensionFilter( "SchalePack File", "schalepack"),
            new ExtensionFilter( "All Files", "*" )
        };

        string dialogTitle = "Select File";
        string localizationPath = Path.Combine(Application.streamingAssetsPath, "Localization", $"{languageCode}.ini");
        if (File.Exists(localizationPath))
        {
            using var ini = new IniFileReader(localizationPath);
            dialogTitle = ini.GetValue("Localization", "SelectFile") ?? dialogTitle;
        }

        var paths = StandaloneFileBrowser.OpenFilePanel(dialogTitle, "", extensions, false);
        if (paths != null && paths.Length > 0)
        {
            selectedFilePath = paths[0];
            if (ValidateZipContents(selectedFilePath))
            {
                ShowConfirmDialog();
            }
            else
            {
                Debug.LogError("Archive file validation failed.");
                Logger.LogError("Archive file validation failed.");
            }
        }
    }

    // 外部拖放或插件调用可以用此方法传入文件路径
    public void HandleDroppedFile(string path)
    {
        if (string.IsNullOrEmpty(path) || !File.Exists(path)) return;
        selectedFilePath = path;
        if (ValidateZipContents(selectedFilePath))
            ShowConfirmDialog();
        else
            Debug.LogError("Dropped archive validation failed.");
    }

    // 验证 ZIP 内容并读取 Manifest.ini 中的必要字段
    bool ValidateZipContents(string zipFilePath)
    {
        try
        {
            using ZipArchive archive = ZipFile.OpenRead(zipFilePath);
            var manifestEntry = archive.GetEntry("Manifest.ini");
            if (manifestEntry == null)
            {
                Debug.LogError("Manifest.ini file not found in the archive.");
                Logger.LogError($"Try to install {pluginName} but Manifest.ini file not found in the archive.");
                return false;
            }

            using (var tempStream = new MemoryStream())
            {
                manifestEntry.Open().CopyTo(tempStream);
                tempStream.Position = 0;

                using var reader = new StreamReader(tempStream);
                var tempPath = Path.GetTempFileName();
                File.WriteAllText(tempPath, reader.ReadToEnd());

                using (var iniReader = new IniFileReader(tempPath))
                {
                    pluginName = iniReader.GetValue("Plugin", "pluginName");
                    version = iniReader.GetValue("Plugin", "version");
                }

                File.Delete(tempPath);
            }

            if (string.IsNullOrEmpty(pluginName) || string.IsNullOrEmpty(version))
            {
                Debug.LogError("Manifest.ini file is missing required fields.");
                Logger.LogError($"Try to install {pluginName} but Manifest.ini file is missing required fields.");
                return false;
            }

            // 简单校验：至少包含一个条目
            return archive.Entries.Any(e => !string.IsNullOrEmpty(e.Name));
        }
        catch (Exception ex)
        {
            Debug.LogError("Error validating archive contents: " + ex.Message);
            Logger.LogError($"Try to install {pluginName} but encountered error validating archive contents: {ex.Message}");
            return false;
        }
    }

    void ShowConfirmDialog()
    {
        if (confirmDialogText != null)
            confirmDialogText.text = $"{Path.GetFileName(selectedFilePath)} - {pluginName}";

        if (previewNameText != null)
            previewNameText.text = pluginName;

        if (confirmDialog != null)
            confirmDialog.SetActive(true);
    }

    void HideConfirmDialog()
    {
        if (confirmDialog != null)
            confirmDialog.SetActive(false);
    }

    public void InstallConfirmed()
    {
        try
        {
            string modFolderPath = Path.Combine(dirPath, pluginName);
            if (!Directory.Exists(modFolderPath))
                Directory.CreateDirectory(modFolderPath);

            ZipFile.ExtractToDirectory(selectedFilePath, modFolderPath);

            Debug.Log($"Mod installed to {modFolderPath}");
            Logger.Log(LogLevel.Info, $"Mod {pluginName} installed to {modFolderPath}");

            uiManager?.RefreshFileList();

        }
        catch (Exception ex)
        {
            Debug.LogError("Error installing files: " + ex.Message);
            Logger.LogError($"Try to install {pluginName} but encountered error during installation: {ex.Message}");
        }
        HideConfirmDialog();
    }

    public void CancelInstallation()
    {
        HideConfirmDialog();
    }

    // IDropHandler 接口（当 UI 元素内接收到 Unity 内部拖放事件时）
    public void OnDrop(PointerEventData eventData)
    {
        // Unity 的 PointerEventData 不包含 OS 文件路径；如果使用外部插件或自定义实现，请调用 HandleDroppedFile
        // 此处仅作为占位以便在未来扩展
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (dropArea != null)
            dropArea.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (dropArea != null)
            dropArea.SetActive(false);
    }
}
