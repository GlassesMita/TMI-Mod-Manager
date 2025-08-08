using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.IO.Compression;
using System.Linq;
using SFB; // 使用 StandaloneFileBrowser 命名空间

public class ModInstaller : MonoBehaviour
{
    private string dirPath; // 目标目录路径
    public Button selectFileButton;
    public GameObject confirmDialog; // 确认弹窗
    public Text confirmDialogText; // 确认弹窗文本
    public Button confirmButton; // 确认按钮
    public Button cancelButton; // 取消按钮

    public UIManager uiManager;

    private string selectedFilePath;
    private string pluginName;
    private string version;
    private IniFileReader localizationManager;
    public string languageCode;

    void Start()
    {
        localizationManager = new IniFileReader(Path.Combine(Application.dataPath, "..", "AppConfig.ini"));
        dirPath = Path.Combine(Application.dataPath, "..", "Mods");
        selectFileButton.onClick.AddListener(OpenFileSelector);
        confirmButton.onClick.AddListener(InstallConfirmed);
        cancelButton.onClick.AddListener(HideConfirmDialog);
        languageCode = localizationManager.GetValue("Language", "DisplayLanguage");
    }

    void OpenFileSelector()
    {
        // 使用 StandaloneFileBrowser 打开文件选择对话框
        var extensions = new[] {
            new ExtensionFilter( "Izakaya File", "izakaya" ),
            new ExtensionFilter( "ZIP File", "zip" ),
            new ExtensionFilter( "All Files", "*" )
        };
        string localizationPath = Path.Combine(Application.streamingAssetsPath, "Localization", $"{languageCode}.ini");
        string dialogTitle = "Select File"; // 默认值
        if (File.Exists(localizationPath))
        {
            using (var iniReader = new IniFileReader(localizationPath))
            {
                dialogTitle = iniReader.GetValue("Localization", "SelectFile") ?? dialogTitle;
            }
        }
        var paths = StandaloneFileBrowser.OpenFilePanel(dialogTitle, "", extensions, true);

        if (paths.Length > 0)
        {
            selectedFilePath = paths[0];
            if (ValidateZipContents(selectedFilePath))
            {
                ShowConfirmDialog();
            }
            else
            {
                Debug.LogError("Archive file validation failed.");
            }
        }
    }

    bool ValidateZipContents(string zipFilePath)
    {
        try
        {
            using (ZipArchive archive = ZipFile.OpenRead(zipFilePath))
            {
                // 检查是否存在 Manifest.ini 文件
                var manifestEntry = archive.GetEntry("Manifest.ini");
                if (manifestEntry == null)
                {
                    Debug.LogError("Manifest.ini file not found in the archive.");
                    return false;
                }

                // 读取 Manifest.ini 文件内容
                using (var tempStream = new MemoryStream())
                {
                    manifestEntry.Open().CopyTo(tempStream);
                    tempStream.Position = 0;
                    
                    using (var tempFile = new StreamReader(tempStream))
                    {
                        var tempPath = Path.GetTempFileName();
                        File.WriteAllText(tempPath, tempFile.ReadToEnd());
                        
                        using (var iniReader = new IniFileReader(tempPath))
                        {
                            pluginName = iniReader.GetValue("Plugin", "pluginName");
                            version = iniReader.GetValue("Plugin", "version");

                            // 验证 INI 内容是否非空
                            if (string.IsNullOrEmpty(pluginName) || string.IsNullOrEmpty(version))
                            {
                                Debug.LogError("Manifest.ini file is missing required fields.");
                                return false;
                            }
                        }
                        
                        File.Delete(tempPath);
                    }
                }

                // 校验逻辑示例：确保所有文件都存在
                return archive.Entries.Any(entry => !string.IsNullOrEmpty(entry.Name));
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error validating archive contents: " + ex.Message);
            return false;
        }
    }

    void ShowConfirmDialog()
    {
        confirmDialogText.text = $"{pluginName} ({version})";
        confirmDialog.SetActive(true);
    }

    void HideConfirmDialog()
    {
        confirmDialog.SetActive(false);
    }

    void InstallConfirmed()
    {
        try
        {
            ZipFile.ExtractToDirectory(selectedFilePath, dirPath);

            string manifestPath = Path.Combine(dirPath, "Manifest.ini");
            if (File.Exists(manifestPath))
            {
                string newManifestPath = Path.Combine(dirPath, $"{pluginName}.ini");
                File.Move(manifestPath, newManifestPath);
                Debug.Log($"Manifest.ini file renamed to {pluginName}.ini");
            }
            uiManager.RefreshFileList();
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error installing files: " + ex.Message);
        }
        HideConfirmDialog();
    }
}
