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
    public LocalizationManager localizationManager; // 引入 LocalizationManager

    [Header("UI Components for Different Confirmation Dialogs")]
    public Text conflictInstallConfirmationText; // 用于命名空间冲突提示的 InstallConfirmation 文本组件
    public Text upgradeInstallConfirmationText; // 用于升级或降级提示的 InstallConfirmation 文本组件

    [Header("Panels for Different Confirmation Dialogs")]
    public GameObject conflictInstallConfirmationPanel; // 用于命名空间冲突提示的面板
    public GameObject upgradeInstallConfirmationPanel; // 用于升级或降级提示的面板

    [Header("ModConflictWarning (Panel) - Bind these inside the Conflict panel")]
    public Text conflict_TitleText;          // 顶部标题（例如：警告：存在命名空间冲突）
    public Text conflict_InstallConfirm;     // InstallConfirm 文本（行 1）
    public Text conflict_InstallConfirm2;    // InstallConfirm2 文本（行 2）
    public Text conflict_InstallConfirm3;    // InstallConfirm3 文本（行 3 / 说明）
    public Image conflict_ArchiveImage;      // 归档缩略图
    public Text conflict_ConflictModName;    // 显示冲突的已安装 Mod 名称
    public Text conflict_CancelButtonText;   // 取消按钮上的文本（面板内的按钮子组件）
    public Text conflict_InstallButtonText;  // 确认安装按钮上的文本（面板内的按钮子组件）
    public Button conflict_InstallButton;  // 确认安装按钮组件
    public Button conflict_CancelButton;  // 取消按钮组件

    [Header("ModUpdateConfirm (Panel) - Bind these inside the Update panel")]
    public Text update_TitleText;            // 顶部标题（例如：即将更新 Mod）
    public Text update_InstallConfirm;       // InstallConfirm 文本（行 1）
    public Text update_InstallConfirm2;      // InstallConfirm2 文本（行 2）
    public Text update_InstallConfirm3;      // InstallConfirm3 文本（行 3 / 说明）
    public Image update_ArchiveImage;        // 归档缩略图
    public Text update_VersionChangeText;    // 版本变化显示（例如：0.0.1 -> 0.0.2）
    public Text update_CancelButtonText;     // 取消按钮上的文本（面板内的按钮子组件）
    public Text update_InstallButtonText;    // 确认安装按钮上的文本（面板内的按钮子组件）
    public Button update_InstallButton;    // 确认安装按钮组件
    public Button update_CancelButton;     // 取消按钮组件

    private string selectedFilePath;
    private string pluginName;
    private string version;
    private string dirPath;

    void Start()
    {
        dirPath = Path.Combine(Application.dataPath, "..", "Mods");

        var fadeController = GetComponent<UIElementFadeController>();

        // 绑定按钮以控制窗口显示
        if (selectFileButton != null)
        {
            if (fadeController != null)
            {
                selectFileButton.onClick.RemoveAllListeners();
                selectFileButton.onClick.AddListener(fadeController.ActivateComponent);
            }
            selectFileButton.onClick.AddListener(OpenFileSelector);
        }

        // 绑定按钮事件
        if (confirmButton != null)
            confirmButton.onClick.AddListener(InstallConfirmed);

        if (cancelButton != null)
            cancelButton.onClick.AddListener(CancelInstallation);

        if (conflict_InstallButtonText != null)
            conflict_InstallButtonText.GetComponentInParent<Button>()?.onClick.AddListener(InstallConfirmed);

        if (conflict_CancelButtonText != null)
            conflict_CancelButtonText.GetComponentInParent<Button>()?.onClick.AddListener(CancelInstallation);

        // 注册拖放事件（使用自定义 FileDragHandler）
        // 注意：此功能仅在 Windows 构建版本中有效，编辑器中无效
        var dragHandler = gameObject.AddComponent<FileDragHandler>();
        dragHandler.OnFilesDropped += OnFilesDropped;

        HideConfirmDialog();
    }

    void OnDestroy()
    {
        // FileDragHandler 会在 OnDisable 中自动清理 Windows 钩子
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

        var dialogTitle = localizationManager.GetLocalizedText("SelectFile", "Select File");

        var paths = StandaloneFileBrowser.OpenFilePanel(dialogTitle, "", extensions, false);
        if (paths != null && paths.Length > 0)
        {
            selectedFilePath = paths[0];
            if (ValidateZipContents(selectedFilePath))
            {
                Debug.Log(localizationManager.GetLocalizedText("InstallNewMod", "Install New Mod"));
            }
            else
            {
                Debug.LogError(localizationManager.GetLocalizedText("CompatibilityWarning", "Due to compatibility issues, this feature is no longer provided"));
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

    // 处理拖放文件事件
    private void OnFilesDropped(System.Collections.Generic.List<string> paths)
    {
        if (paths != null && paths.Count > 0)
        {
            HandleDroppedFile(paths[0]); // 处理第一个文件
        }
    }

    private bool CheckVersionCompatibility(string pluginName, int newVersionCode)
    {
        // 检查版本号是否兼容
        string installedManifestPath = Path.Combine(dirPath, pluginName + ".ini");
        if (!File.Exists(installedManifestPath))
        {
            // 如果未安装，则视为兼容
            return true;
        }

        using var iniReader = new IniFileReader(installedManifestPath);
        int installedVersionCode = int.Parse(iniReader.GetValue("Plugin", "VersionCode") ?? "0");

        if (newVersionCode < installedVersionCode)
        {
            Debug.LogWarning($"Attempting to downgrade {pluginName}: Installed version code {installedVersionCode}, new version code {newVersionCode}.");
            Logger.LogWarning($"Attempting to downgrade {pluginName}: Installed version code {installedVersionCode}, new version code {newVersionCode}.");
            return false;
        }

        return true;
    }

    private bool CheckNamespaceConflict(string namespaceName)
    {
        // 检查是否存在命名空间冲突
        string[] existingNamespaces = Directory.GetFiles(dirPath, "*.ini")
            .Select(file => new IniFileReader(file).GetValue("Plugin", "Namespace"))
            .Where(ns => !string.IsNullOrEmpty(ns))
            .ToArray();

        if (existingNamespaces.Contains(namespaceName))
        {
            Debug.LogWarning($"Namespace conflict detected: {namespaceName} already exists.");
            Logger.LogWarning($"Namespace conflict detected: {namespaceName} already exists.");
            return true;
        }

        return false;
    }

    void ShowConfirmDialog()
    {
        if (confirmDialogText != null)
            confirmDialogText.text = $"{Path.GetFileName(selectedFilePath)} - {pluginName}";

        if (previewNameText != null)
            previewNameText.text = $"{pluginName} - {version}";

        if (confirmDialog != null)
            confirmDialog.SetActive(true);
    }

    void HideConfirmDialog()
    {
        confirmDialog?.SetActive(false);
        conflictInstallConfirmationPanel?.SetActive(false);
        upgradeInstallConfirmationPanel?.SetActive(false);
    }

    void ShowConfirmDialogForUpdate(string oldVersion, string newVersion)
    {
        if (upgradeInstallConfirmationText != null)
        {
            string baseText = string.Format(
                "{0}\n{1} -> {2}",
                localizationManager.GetLocalizedText("InstallConfirmaction1", "You are about to install the following mod"),
                oldVersion,
                newVersion
            );

            // 检测降级安装
            if (string.Compare(newVersion, oldVersion) < 0)
            {
                string downgradeWarning = localizationManager.GetLocalizedText(
                    "DowngradeWarning",
                    "Warning: Downgrading may cause issues."
                );
                string downgradeAction = localizationManager.GetLocalizedText(
                    "DowngradeAction",
                    "If you still want to downgrade, click \"Continue\"."
                );
                baseText += $"\n\n{downgradeWarning}\n{downgradeAction}";
            }

            upgradeInstallConfirmationText.text = baseText;
        }

        if (upgradeInstallConfirmationPanel != null)
        {
            upgradeInstallConfirmationPanel.SetActive(true);
        }
    }

    void ShowConfirmDialogForNamespaceConflict(string namespaceName)
    {
        if (conflictInstallConfirmationText != null)
        {
            conflictInstallConfirmationText.text = string.Format(
                "{0}\n{1}",
                localizationManager.GetLocalizedText("NamespaceConflict", "Warning: Namespace Conflict"),
                namespaceName
            );
        }

        if (conflictInstallConfirmationPanel != null)
        {
            conflictInstallConfirmationPanel.SetActive(true);
        }
    }

    // 在 ValidateZipContents 中调用更新和命名空间冲突的确认对话框
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
                    pluginName = iniReader.GetValue("Plugin", "PluginName");
                    version = iniReader.GetValue("Plugin", "Version");
                    int newVersionCode = int.Parse(iniReader.GetValue("Plugin", "VersionCode") ?? "0");
                    string namespaceName = iniReader.GetValue("Plugin", "Namespace");

                    string installedManifestPath = Path.Combine(dirPath, pluginName + ".ini");
                    if (File.Exists(installedManifestPath))
                    {
                        using var installedIniReader = new IniFileReader(installedManifestPath);
                        int installedVersionCode = int.Parse(installedIniReader.GetValue("Plugin", "VersionCode") ?? "0");
                        string installedVersion = installedIniReader.GetValue("Plugin", "Version");

                        if (newVersionCode < installedVersionCode)
                        {
                            ShowConfirmDialogForUpdate(installedVersion, version);
                            confirmDialog?.SetActive(true);
                            File.Delete(tempPath);
                            return false;
                        }
                    }

                    if (CheckNamespaceConflict(namespaceName))
                    {
                        ShowConfirmDialogForNamespaceConflict(namespaceName);
                        confirmDialog?.SetActive(true);
                        File.Delete(tempPath);
                        return false;
                    }
                }

                File.Delete(tempPath);
            }

            if (string.IsNullOrEmpty(pluginName) || string.IsNullOrEmpty(version))
            {
                Debug.LogError("Manifest.ini file is missing required fields.");
                Logger.LogError($"Try to install {pluginName} but Manifest.ini file is missing required fields.");
                return false;
            }

            // 如果没有问题，显示安装确认窗口
            ShowConfirmDialog();
            confirmDialog?.SetActive(true);

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

    public void InstallConfirmed()
    {
        try
        {
            string modFolderPath = Path.Combine(dirPath, pluginName);
            if (!Directory.Exists(modFolderPath))
                Directory.CreateDirectory(modFolderPath);

            // 检查目标文件是否存在
            string manifestPath = Path.Combine(modFolderPath, "Manifest.ini");
            if (File.Exists(manifestPath))
            {
                Debug.LogWarning($"File already exists: {manifestPath}. Overwriting...");
                Logger.LogWarning($"File already exists: {manifestPath}. Overwriting...");
                File.Delete(manifestPath); // 删除旧文件
            }

            // 解压文件到目标目录
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

    public void OnDrop(PointerEventData eventData)
    {
        
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        dropArea?.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        dropArea?.SetActive(false);
    }
}
