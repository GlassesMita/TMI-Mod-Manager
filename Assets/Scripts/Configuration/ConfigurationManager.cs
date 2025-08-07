using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;

public class ConfigurationManager : MonoBehaviour
{
    [Header("Configuration Settings")]
    [Tooltip("Path to the configuration file")]
    public string configFilePath;
    private IniFileReader configFileReader;

    public Dropdown languageDropdown;


    void Start()
    {
        string[] languageCodes = GetLanguageCodes();
        configFilePath = Path.Combine(Application.dataPath, "..", "AppConfig.ini");
        LoadConfiguration();
    }

    private void LoadConfiguration()
    {
        if (File.Exists(configFilePath))
        {
            configFileReader = new IniFileReader(configFilePath);
            string languageCode = configFileReader.GetValue("Localization", "DisplayLanguage");
            Debug.Log($"Loaded language code: {languageCode}");
        }
        else
        {
            Debug.LogError($"Configuration file not found at {configFilePath}");
        }
    }
    


    public void SaveConfiguration()
    {

    }

    private string[] GetLanguageCodes()
    {
        var languageNames = new List<string>();
        string localizationPath = Path.Combine(Application.streamingAssetsPath, "Localization");
        
        if (!Directory.Exists(localizationPath))
        {
            Debug.LogWarning($"Localization directory not found: {localizationPath}");
            return new string[0];
        }

        try
        {
            foreach (string filePath in Directory.GetFiles(localizationPath, "*.ini"))
            {
                try
                {
                    using (var iniReader = new IniFileReader(filePath))
                    {
                        string languageName = iniReader.GetValue("Localization", "Language");
                        if (!string.IsNullOrEmpty(languageName))
                        {
                            languageNames.Add(languageName);
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"Failed to parse localization file {filePath}: {ex.Message}");
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error reading localization files: {ex.Message}");
        }
        
        return languageNames.ToArray();
    }
}