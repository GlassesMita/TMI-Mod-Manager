using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class LocalizationManager : MonoBehaviour
{
    public string languageCode;

    
    void Start()
    {
        INIParser languageCode = new INIParser();
        languageCode.Open(Path.Combine(Application.dataPath, "..", "AppConfig.ini"));
        languageCode.ReadValue("Language", "languageCode", "en_US");
        languageCode.Close();
    }

    public void Localization(string value)
    {
        INIParser localizedManager = new INIParser();
        localizedManager.Open(Path.Combine(Application.streamingAssetsPath, languageCode + ".ini"));
        localizedManager.ReadValue("Localization", value, "localizedText");
        localizedManager.Close();
    }
}
