using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class UIManager : MonoBehaviour
{
    public GameObject fileButtonPrefab;
    public Transform fileListContainer;
    public Text jsonContentText;

    public void UpdateUIWithFiles(string[] jsonFiles)
    {
        foreach (string file in jsonFiles)
        {
            CreateFileButton(file);
        }
    }

    void CreateFileButton(string filePath)
    {
        string jsonContent = File.ReadAllText(filePath);
        PluginInfo pluginInfo = JsonUtility.FromJson<PluginInfo>(jsonContent);

        GameObject buttonObj = Instantiate(fileButtonPrefab, fileListContainer);
        Button button = buttonObj.GetComponent<Button>();
        Text buttonText = button.GetComponentInChildren<Text>();
        buttonText.text = pluginInfo.pluginName; // 设置按钮文本为 pluginName

        button.onClick.AddListener(() => ShowJsonContent(filePath));
    }

    void ShowJsonContent(string filePath)
    {
        string jsonContent = File.ReadAllText(filePath);
        PluginInfo pluginInfo = JsonUtility.FromJson<PluginInfo>(jsonContent);
        jsonContentText.text = $"Plugin Name: {pluginInfo.pluginName}\nAuthor: {pluginInfo.author}\nVersion: {pluginInfo.version}\nFile Name: {pluginInfo.fileName}\n\n{pluginInfo.pluginInfo}";
    }
}