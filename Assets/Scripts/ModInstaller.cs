using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.IO.Compression;
using System.Linq;
using SFB; // 使用 StandaloneFileBrowser 命名空间

public class ModInstaller : MonoBehaviour
{
    public string dirPath; // 目标目录路径
    public Button selectFileButton;
    public GameObject confirmDialog; // 确认弹窗
    public Text confirmDialogText; // 确认弹窗文本
    public Button confirmButton; // 确认按钮
    public Button cancelButton; // 取消按钮

    public UIManager uiManager;

    private string selectedFilePath;
    private string pluginName;
    private string version;

    void Start()
    {
        selectFileButton.onClick.AddListener(OpenFileSelector);
        confirmButton.onClick.AddListener(InstallConfirmed);
        cancelButton.onClick.AddListener(HideConfirmDialog);
    }

    void OpenFileSelector()
    {
        // 使用 StandaloneFileBrowser 打开文件选择对话框
        var extensions = new[] {
            new ExtensionFilter( "Izakaya File", "izakaya" ),
            new ExtensionFilter( "ZIP File", "zip" ),
            new ExtensionFilter( "All Files", "*" )
        };
        var paths = StandaloneFileBrowser.OpenFilePanel("Select File", "", extensions, true);

        if (paths.Length > 0)
        {
            selectedFilePath = paths[0];
            if (ValidateZipContents(selectedFilePath))
            {
                ShowConfirmDialog();
            }
            else
            {
                Debug.LogError("Zip file validation failed.");
            }
        }
    }

    bool ValidateZipContents(string zipFilePath)
    {
        try
        {
            using (ZipArchive archive = ZipFile.OpenRead(zipFilePath))
            {
                // 检查是否存在 Manifest.json 文件
                var manifestEntry = archive.GetEntry("Manifest.json");
                if (manifestEntry == null)
                {
                    Debug.LogError("Manifest.json file not found in the archive.");
                    return false;
                }

                // 读取 Manifest.json 文件内容
                using (var reader = new StreamReader(manifestEntry.Open()))
                {
                    string manifestContent = reader.ReadToEnd();
                    // 解析 JSON 内容
                    var manifest = JsonUtility.FromJson<PluginInfo>(manifestContent);
                    pluginName = manifest.pluginName;
                    version = manifest.version;

                    // 验证 JSON 内容是否非空
                    if (string.IsNullOrEmpty(pluginName) || string.IsNullOrEmpty(version))
                    {
                        Debug.LogError("Manifest.json file is missing required fields.");
                        return false;
                    }
                }

                // 校验逻辑示例：确保所有文件都存在
                return archive.Entries.Any(entry => !string.IsNullOrEmpty(entry.Name));
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error validating zip contents: " + ex.Message);
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

            string manifestPath = Path.Combine(dirPath, "Manifest.json");
            if (File.Exists(manifestPath))
            {
                string newManifestPath = Path.Combine(dirPath, $"{pluginName}.json");
                File.Move(manifestPath, newManifestPath);
                Debug.Log($"Manifest.json file renamed to {pluginName}.json");
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
