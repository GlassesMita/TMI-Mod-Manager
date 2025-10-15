using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.SceneManagement;
using System.Linq;

public class UIManager : MonoBehaviour
{
    public GameObject fileButtonPrefab;
    public Transform fileListContainer;
    public Text jsonContentText;
    public Text modNameText;
    public Text authorText;
    public Text versionText;
    public Text requiredFilesText;
    public Text filePathText;
    public Button deleteButton; // 独立的删除按钮
    public Button refreshButton; // 独立的刷新按钮
    public GameObject confirmDialog; // 确认弹窗
    public Text confirmDialogText; // 确认弹窗文本
    public Button confirmButton; // 确认按钮
    public Button cancelButton; // 取消按钮
    public CurrentSceneName currentSceneName;

    private string filePathToDelete; // 用于存储待删除的文件路径
    private string[] filesToIncludeToDelete; // 用于存储待删除的关联文件
    private string disableFilePathToDelete; // 用于存储待删除的 .DISABLE 文件路径

    void Start()
    {
        if (refreshButton == null || deleteButton == null || confirmButton == null || cancelButton == null || confirmDialog == null || confirmDialogText == null)
        {
            Debug.LogError("UIManager: 有一个或多个实例未设置");
            return;
        }

        refreshButton.onClick.AddListener(RefreshFileList); // 确保这个按钮仅用于刷新功能
    // 兼容旧绑定：RefreshFileList 调用新方法 RefreshMods
    confirmButton.onClick.AddListener(DeleteConfirmed);
        cancelButton.onClick.AddListener(HideConfirmDialog);
        // 初始时禁用删除按钮，直到用户选择某个文件
        if (deleteButton != null) deleteButton.interactable = false;
        RefreshMods(); // 初始化时刷新一次文件列表
    }

    public void UpdateUIWithMods(string[] iniFiles)
    {
        foreach (string file in iniFiles)
        {
            CreateModButton(file);
        }
    }

    // 兼容旧代码调用：FileLoader 等处可能调用 UpdateUIWithFiles
    public void UpdateUIWithFiles(string[] iniFiles)
    {
        UpdateUIWithMods(iniFiles);
    }

    void CreateModButton(string filePath)
    {
        // 使用 INI 读取插件元信息（期望 [Plugin] 节）
        IniFileReader reader = null;
        try
        {
            reader = new IniFileReader(filePath);
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"无法读取 INI 文件: {filePath} - {ex.Message}");
            return;
        }

    string pluginName = reader.GetValue("Plugin", "PluginName") ?? Path.GetFileNameWithoutExtension(filePath);
    string author = reader.GetValue("Plugin", "Author") ?? "Unknown";
    string version = reader.GetValue("Plugin", "Version") ?? "1.0";
    // 使用新的 INI 节 [RequiedList] 下的 List 键
    string[] requiredList = reader.GetValues("RequiedList", "List");

        GameObject buttonObj = Instantiate(fileButtonPrefab, fileListContainer);
        Text buttonText = buttonObj.GetComponentInChildren<Text>();
        Toggle toggle = buttonObj.GetComponentInChildren<Toggle>();

        if (buttonText == null || toggle == null)
        {
            Debug.LogError("UIManager: 缺失预制件上的 UI 控件. 请确保在 Button 中设置了 Text 和 Toggle 控件");
            return;
        }

        buttonText.text = pluginName; // 设置按钮文本为 pluginName

        // 检查目录下是否存在同名 .Disabled 文件
        string disableFilePath = filePath + ".Disabled";
        toggle.isOn = !File.Exists(disableFilePath); // 如果存在 .Disabled 文件，Toggle 设置为 Off，否则设置为 On

        // 点击按钮时显示详情并将当前选择设置为该文件（使删除按钮可用）
        buttonObj.GetComponent<Button>().onClick.AddListener(() =>
        {
            // 把 INI 字段的内容填入不同的 Text 控件
            modNameText.text = pluginName;
            if (authorText != null) authorText.text = author;
            if (versionText != null) versionText.text = version;
            if (filePathText != null) filePathText.text = filePath;

            // 依赖显示：不存在或为空时隐藏；否则显示本地化前缀和逗号分隔的依赖列表
            if (requiredFilesText != null)
            {
                if (requiredList == null || requiredList.Length == 0)
                    requiredFilesText.gameObject.SetActive(false);
                else
                {
                    string prefix = GetLocalizationValue("Dependencies") ?? "Dependencies:";
                    requiredFilesText.gameObject.SetActive(true);
                    requiredFilesText.text = prefix + " " + string.Join(", ", requiredList);
                }
            }

            // 兼容性检查并设置颜色
            bool ok = CheckCompatibility(filePath, requiredList);
            SetTextColor(requiredFilesText, ok);

            // 设置待删除信息并启用删除按钮
            filePathToDelete = filePath;
            filesToIncludeToDelete = requiredList;
            disableFilePathToDelete = disableFilePath;
            if (deleteButton != null) deleteButton.interactable = true;
        });

        // 添加 Toggle 事件处理，创建/删除 .Disabled 文件
        toggle.onValueChanged.AddListener((bool isOn) =>
        {
            if (isOn)
            {
                if (File.Exists(disableFilePath))
                {
                    File.Delete(disableFilePath);
                }
            }
            else
            {
                if (!File.Exists(disableFilePath))
                {
                    File.Create(disableFilePath).Close();
                }
            }
            // 修改后立即刷新兼容性状态（简单方案：刷新列表）
            RefreshMods();
        });

        // 初始时根据兼容性设置字体颜色
        bool initialOk = CheckCompatibility(filePath, requiredList);
        SetTextColor(buttonText, initialOk);
    }

    void ShowConfirmDialog(string filePath, string[] filesToInclude, string disableFilePath)
    {
        filePathToDelete = filePath;
        filesToIncludeToDelete = filesToInclude;
        disableFilePathToDelete = disableFilePath;
        confirmDialogText.text = "你确认要删除这个文件吗？";
        confirmDialog.SetActive(true);
    }

    public void HideConfirmDialog()
    {
        confirmDialog.SetActive(false);
    }

    public void ConfirmDelete()
    {
        RemoveModAndDependencies(filePathToDelete, filesToIncludeToDelete, disableFilePathToDelete);
        HideConfirmDialog();
    }

    // 保持兼容：旧的 DeleteConfirmed 名称继续工作
    public void DeleteConfirmed()
    {
        ConfirmDelete();
    }

    // 保持兼容：旧的 RefreshFileList 名称继续工作
    public void RefreshFileList()
    {
        RefreshMods();
    }

    // 简单的兼容性检查：验证 requiredList 中的每个文件是否存在于 Mods 目录
    private bool CheckCompatibility(string iniPath, string[] requiredList)
    {
        if (requiredList == null || requiredList.Length == 0) return true;
        string dir = Path.GetDirectoryName(iniPath);
        foreach (var req in requiredList)
        {
            if (string.IsNullOrWhiteSpace(req)) continue;
            string p = Path.Combine(dir, req.Trim());
            if (!File.Exists(p)) return false;
        }
        return true;
    }

    private void SetTextColor(Text txt, bool ok)
    {
        if (txt == null) return;
        txt.color = ok ? Color.black : Color.red;
    }

    void RemoveModAndDependencies(string jsonFilePath, string[] filesToInclude, string disableFilePath)
    {
        // 删除 JSON 文件
        if (File.Exists(jsonFilePath))
        {
            File.Delete(jsonFilePath);
        }

        // 删除 RequiredList 中列出的关联文件（位于同一目录）
        if (filesToInclude != null)
        {
            foreach (string includedFile in filesToInclude)
            {
                if (string.IsNullOrWhiteSpace(includedFile)) continue;
                string includedFilePath = Path.Combine(Path.GetDirectoryName(jsonFilePath), includedFile.Trim());
                if (File.Exists(includedFilePath))
                {
                    File.Delete(includedFilePath);
                }
            }
        }

        // 删除 .DISABLE 文件
        if (File.Exists(disableFilePath))
        {
            File.Delete(disableFilePath);
        }

        // 刷新文件列表并检查兼容性
        RefreshMods();
    }

    public void RefreshMods()
    {
        // 清空当前内容
        foreach (Transform child in fileListContainer)
        {
            Destroy(child.gameObject);
        }

        // 重新读取 INI 文件列表，目录为 Application.dataPath/../Mods
        string modsDir = Path.Combine(Application.dataPath, "..", "Mods");
        if (!Directory.Exists(modsDir)) Directory.CreateDirectory(modsDir);

        // 递归扫描 Mods 目录及其子目录中的所有 .ini 文件，保证结果按路径排序以便 UI 稳定
        string[] iniFiles = Directory.GetFiles(modsDir, "*.ini", SearchOption.AllDirectories)
            .OrderBy(p => p, System.StringComparer.OrdinalIgnoreCase)
            .ToArray();

        UpdateUIWithMods(iniFiles);
    }

    void ShowModDetails(string filePath)
    {
        // Deprecated: keep signature for compatibility but prefer button's inline handler
        if (!File.Exists(filePath)) return;
        try
        {
            using var reader = new IniFileReader(filePath);
            string pluginName = reader.GetValue("Plugin", "PluginName") ?? Path.GetFileNameWithoutExtension(filePath);
            string author = reader.GetValue("Plugin", "Author") ?? "Unknown";
            string version = reader.GetValue("Plugin", "Version") ?? "1.0";
            string[] requiredList = reader.GetValues("RequiedList", "List");
            string includedFiles = (requiredList != null && requiredList.Length > 0) ? string.Join(", ", requiredList) : "None";
            modNameText.text = pluginName;
            if (authorText != null) authorText.text = author;
            if (versionText != null) versionText.text = version;
            if (requiredFilesText != null) requiredFilesText.text = includedFiles;
            if (filePathText != null) filePathText.text = filePath;

            // also set selection info for delete flow
            filePathToDelete = filePath;
            filesToIncludeToDelete = requiredList;
            disableFilePathToDelete = filePath + ".Disabled";
            if (deleteButton != null) deleteButton.interactable = true;
            // 兼容性检查
            bool ok = CheckCompatibility(filePath, requiredList);
            SetTextColor(requiredFilesText, ok);
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"ShowJsonContent failed for {filePath}: {ex.Message}");
        }
    }

    // 从 StreamingAssets/Localization/{lang}.ini 读取 [Localization] 节的键值
    private string GetLocalizationValue(string key)
    {
        try
        {
            // 首先读取 AppConfig.Schale 中记录的 DisplayLanguage（如果存在）
            string appConfigPath = Path.Combine(Application.dataPath, "..", "AppConfig.Schale");
            string langCode = null;
            if (File.Exists(appConfigPath))
            {
                try
                {
                    var cfg = new IniFileReader(appConfigPath);
                    langCode = cfg.GetValue("Localization", "DisplayLanguage");
                }
                catch { langCode = null; }
            }

            // 构造本地化文件路径
            string localizationPath = Path.Combine(Application.streamingAssetsPath, "Localization");
            if (string.IsNullOrEmpty(langCode))
            {
                // 尝试选第一个可用的语言文件
                if (Directory.Exists(localizationPath))
                {
                    var files = Directory.GetFiles(localizationPath, "*.ini");
                    if (files.Length > 0)
                        langCode = Path.GetFileNameWithoutExtension(files[0]);
                }
            }

            if (string.IsNullOrEmpty(langCode)) return null;

            string locFile = Path.Combine(localizationPath, langCode + ".ini");
            if (!File.Exists(locFile)) return null;

            var reader = new IniFileReader(locFile);
            return reader.GetValue("Localization", key);
        }
        catch
        {
            return null;
        }
    }
}
