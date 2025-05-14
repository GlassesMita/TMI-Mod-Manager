using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class UserSettings : MonoBehaviour
{
    public GameObject settingPanel;
    public GameObject exePathInputField;
    public Toggle toggle; // ��������һ�� Toggle ������ڿ�����ʾ

    // ���� JSON �ļ���·��
    private string jsonFilePath = @"";

    // Start is called before the first frame update
    void Start()
    {
        jsonFilePath = Application.dataPath + @"/Config/UserSettings.json";
    // ���ز����� JSON �ļ�
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
        // ʹ�ô� JSON �ļ��н����Ĳ���ֵ
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