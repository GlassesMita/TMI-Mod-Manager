using UnityEngine;
using System.IO;

public class FileLoader : MonoBehaviour
{
    public string directoryPath = "";
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

}
