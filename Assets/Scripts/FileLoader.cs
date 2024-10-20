using UnityEngine;
using System.IO;
using UnityEngine.UI;

public class FileLoader : MonoBehaviour
{
    public string directoryPath = "";
    public Text jsonContentText;
    public UIManager uiManager;

    void Start()
    {
        string[] jsonFiles = Directory.GetFiles(directoryPath, "*.json");

        if (uiManager != null)
        {
            uiManager.UpdateUIWithFiles(jsonFiles);
        }
        else
        {
            Debug.LogError("UIManager reference is not set.");
        }
    }
    public void ReLoad()
    {
        foreach (Transform child in uiManager.fileListContainer)
        {
            Destroy(child.gameObject);
        }
        Start();
        jsonContentText.text = "";
    }
}
