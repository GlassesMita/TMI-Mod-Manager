using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class UserSettings : MonoBehaviour
{
    public GameObject settingPanel;
    public GameObject exePathInputField;
    public Toggle toggle; // 假设你有一个 Toggle 组件用于控制显示

    // 假设 JSON 文件的路径
    private string jsonFilePath = @"";

    // Start is called before the first frame update
    void Start()
    {
        jsonFilePath = Application.dataPath + @"/Config/UserSettings.json";
    // 加载并解析 JSON 文件
        LoadUserSettings();
        Debug.Log("User Preferences JSON file: " + jsonFilePath);
        toggle.enabled = true;
    }

    public void OpenSettingPanel()
    {
        settingPanel.SetActive(true);
    }

    public void CloseSettingPanel()
    {
        settingPanel.SetActive(false);
    }

    public void DisplayExePathInputField()
    {
        // 使用从 JSON 文件中解析的布尔值
        if (isSteamReleaseData.isSteamRelease == true)
        {
            exePathInputField.SetActive(false);
        }
        else
        {
            exePathInputField.SetActive(true);
        }
    }

    private void LoadUserSettings()
    {
        if (File.Exists(jsonFilePath))
        {
            string jsonData = File.ReadAllText(jsonFilePath);
            UserPreferences data = JsonUtility.FromJson<UserPreferences>(jsonData);
            isSteamReleaseData = data;
        }
    }

    private UserPreferences isSteamReleaseData;
}