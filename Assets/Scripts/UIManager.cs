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
        confirmButton.onClick.AddListener(DeleteConfirmed);
        cancelButton.onClick.AddListener(HideConfirmDialog);
        RefreshFileList(); // 初始化时刷新一次文件列表
    }

    public void UpdateUIWithFiles(string[] jsonFiles)
    {
        foreach (string file in jsonFiles)
        {
            CreateFileButton(file);
        }
        // 过滤掉 Data 目录下的文件
        jsonFiles = jsonFiles.Where(file => !file.Contains("./" + Application.productName + "/")).ToArray();
    }

    void CreateFileButton(string filePath)
    {
        string jsonContent = File.ReadAllText(filePath);
        PluginInfo pluginInfo = JsonUtility.FromJson<PluginInfo>(jsonContent);

        GameObject buttonObj = Instantiate(fileButtonPrefab, fileListContainer);
        Text buttonText = buttonObj.GetComponentInChildren<Text>();
        Toggle toggle = buttonObj.GetComponentInChildren<Toggle>();

        if (buttonText == null || toggle == null)
        {
            Debug.LogError("UIManager: 缺失预制件上的 UI 控件. 请确保在 Button 中设置了 Text 和 Toggle 控件");
            return;
        }

        buttonText.text = pluginInfo.pluginName; // 设置按钮文本为 pluginName

        // 检查目录下是否存在同名 .DISABLE 文件
        string disableFilePath = filePath + ".Disabled";
        toggle.isOn = !File.Exists(disableFilePath); // 如果存在 .DISABLE 文件，Toggle 设置为 Off，否则设置为 On

        buttonObj.GetComponent<Button>().onClick.AddListener(() => ShowJsonContent(filePath));

        // 添加 Toggle 事件处理
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
        });

        // 添加删除文件按钮事件
        deleteButton.onClick.AddListener(() => ShowConfirmDialog(filePath, pluginInfo.fileInclude, disableFilePath));
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

    public void DeleteConfirmed()
    {
        DeleteJsonFile(filePathToDelete, filesToIncludeToDelete, disableFilePathToDelete);
        HideConfirmDialog();
    }

    void DeleteJsonFile(string jsonFilePath, string[] filesToInclude, string disableFilePath)
    {
        // 删除 JSON 文件
        if (File.Exists(jsonFilePath))
        {
            File.Delete(jsonFilePath);
        }

        // 删除包含的文件
        if (filesToInclude != null)
        {
            foreach (string includedFile in filesToInclude)
            {
                string includedFilePath = Path.Combine(Path.GetDirectoryName(jsonFilePath), includedFile);
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

        // 刷新文件列表
        // RefreshFileList();
        SceneManager.LoadScene(currentSceneName.sceneName);
        RefreshFileList();
    }

    public void RefreshFileList()
    {
        // 清空当前内容
        foreach (Transform child in fileListContainer)
        {
            Destroy(child.gameObject);
        }

        // 重新读取文件列表
        string[] jsonFiles = Directory.GetFiles(Application.dataPath, "*.json");
        UpdateUIWithFiles(jsonFiles);
    }

    void ShowJsonContent(string filePath)
    {
        string jsonContent = File.ReadAllText(filePath);
        PluginInfo pluginInfo = JsonUtility.FromJson<PluginInfo>(jsonContent);
        string includedFiles = pluginInfo.fileInclude != null ? string.Join(", ", pluginInfo.fileInclude) : "None";
        modNameText.text = $"{pluginInfo.pluginName}";
        jsonContentText.text = $"Author: {pluginInfo.author}\nVersion: {pluginInfo.version}\nIncluded Files: {includedFiles}\nFile Path: {filePath}";
    }
}
