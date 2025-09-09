using UnityEngine;
using System.IO;
using System.Collections.Generic;
using UnityEngine.UI;

public class LocalizationManager : MonoBehaviour
{
    [Header("This script is only for Text(Old)'s localization\nNo effects on TextMeshPro or Image components")]
    [Header("仅对 Text(Old) 组件的本地化有效\n对 TextMeshPro 或 Image 组件无效")]
    [Tooltip("The language code used by the app, as defined in AppConfig.ini\nBut can be overridden by string forceLanguageCode")]
    public string languageCode;

    [Tooltip("Force the language code to be used regardless of AppConfig.ini")]
    public bool forceLanguageCode = false;

    [Tooltip("The final language code used by the app, after all checks have been made\nJust for testing purposes, do not use in production")]
    public string finalLanguageCode;

    [Tooltip("The name of the value to be localized")]
    public string value;

    [Tooltip("The alternative text to use if the localization fails")]
    public string alterText;

    public Text textComponent;

    void Start()
    {
        if (forceLanguageCode)
        {
            languageCode = finalLanguageCode;
        }
        else
        {
            IniFileReader languageCodeLoader = new IniFileReader(Path.Combine(Application.dataPath, "..", "AppConfig.ini"));
            languageCode = languageCodeLoader.GetValue("Localization", "DisplayLanguage");
        }

        Localization();
    }

    public void Localization()
    {
        if (alterText == null)
        {
            Debug.LogError("Alternative text cannot be null! Please provide a valid string.");
        }
        else if (value == null)
        {
            Debug.LogError("Value cannot be null! Please provide a valid string.");
        }
        else
        {
            IniFileReader localizedManager = new IniFileReader(Path.Combine(Application.streamingAssetsPath, "Localization", languageCode + ".ini"));
            string textContent = localizedManager.GetValue("Localization", value);
            if (textContent == null || textContent == "")
            {
                Debug.LogError("Text content is null! Using alternative text instead.\nNull or empty text's kvp is: " + value);
                textComponent.text = alterText;
            }
            else
            {
                textComponent.text = textContent;
            }
        }

    }
}
