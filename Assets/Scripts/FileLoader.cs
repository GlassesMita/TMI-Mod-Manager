using UnityEngine;
using System.IO;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq; // 添加了 System.Linq 命名空间以使用 Where 方法

public class FileLoader : MonoBehaviour
{
    public string directoryPath = "";
    public GameObject statusTextPrefab;
    public Text jsonContentText;
    public UIManager uiManager;
    public CurrentSceneName currentSceneName;

    void Start()
    {
        directoryPath = Application.dataPath + directoryPath;
        string[] jsonFiles = Directory.GetFiles(directoryPath, "*.json");

        // 过滤掉 Data 目录下的文件
        jsonFiles = jsonFiles.Where(file => !file.Contains("./" + Application.productName +"/")).ToArray();

        if (uiManager != null)
        {
            uiManager.UpdateUIWithFiles(jsonFiles);
        }
        else
        {
            Debug.LogError("UIManager 的引用未设置");
        }

        // 检查是否找到 JSON 文件
        if (jsonFiles.Length == 0)
        {
            // 创建预制件
            GameObject statusTextObject = Instantiate(statusTextPrefab);
            // 获取预制件上的 Text 组件
            Text statusText = statusTextObject.GetComponent<Text>();
            if (statusText != null)
            {
                // 设置 Text 的值
                statusText.text = "空空如也";
                // 添加 Text 到 Canvas 下，或者你可以设置它的位置
                statusText.transform.SetParent(uiManager.fileListContainer, false);

            }
            else
            {
                Debug.LogError("预制件上未能找到 Text 组件");
            }
        }
    }

    public void ReLoad()
    {
        foreach (Transform child in uiManager.fileListContainer)
        {
            Destroy(child.gameObject);
        }
        jsonContentText.text = "";
        SceneManager.LoadScene(currentSceneName.sceneName);
    }
}