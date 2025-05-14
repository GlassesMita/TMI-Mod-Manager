using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;

public class SceneLoader : MonoBehaviour
{
    public Dropdown sceneDropdown;
    public Button loadButton;

    private void Start()
    {
        // 获取 Assets/Scenes 下的所有场景文件
        string[] scenePaths = GetScenePaths();
        // 将场景文件名添加到 Dropdown 中
        PopulateDropdown(scenePaths);

        // 设置按钮的点击事件
        loadButton.onClick.AddListener(OnLoadButtonClicked);
    }

    // 获取 Assets/Scenes 下的所有场景文件路径
    private string[] GetScenePaths()
    {
        // 获取 "Assets/Scenes" 文件夹下的所有场景文件
        string[] sceneFiles = Directory.GetFiles("Assets/Scenes", "*.unity");
        string[] sceneNames = new string[sceneFiles.Length];

        // 提取文件名（去除路径和扩展名）
        for (int i = 0; i < sceneFiles.Length; i++)
        {
            sceneNames[i] = Path.GetFileNameWithoutExtension(sceneFiles[i]);
        }

        return sceneNames;
    }

    // 将场景名称添加到 Dropdown 中
    private void PopulateDropdown(string[] sceneNames)
    {
        sceneDropdown.ClearOptions(); // 清空现有选项
        sceneDropdown.AddOptions(new System.Collections.Generic.List<string>(sceneNames)); // 添加新选项
    }

    // 按钮点击事件：加载所选场景
    private void OnLoadButtonClicked()
    {
        string selectedScene = sceneDropdown.options[sceneDropdown.value].text;
        SceneManager.LoadScene(selectedScene); // 加载选中的场景
    }
}
