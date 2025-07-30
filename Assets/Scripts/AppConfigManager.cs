using UnityEngine;
using System.IO;

public class AppConfigManager : MonoBehaviour
{
    private static AppConfigManager instance;
    public static AppConfigManager Instance => instance;

    private AppConfig config;
    private const string configFile = "app_config.json";

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            LoadConfig();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadConfig()
    {
        string filePath = Path.Combine(Application.dataPath, configFile);
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            config = JsonUtility.FromJson<AppConfig>(json);
        }
        else
        {
            // Create default config if file doesn't exist
            config = new AppConfig
            {
                appName = "TMI Mod Manager",
                version = "1.0.0",
                author = "Unknown",
                description = "Mod management tool"
            };
            SaveConfig();
        }
    }

    private void SaveConfig()
    {
        string filePath = Path.Combine(Application.dataPath, configFile);
        string json = JsonUtility.ToJson(config, true);
        File.WriteAllText(filePath, json);
    }

    // Public methods to access config
    public string GetAppName() => config.appName;
    public string GetVersion() => config.version;
    public string GetAuthor() => config.author;
    public string GetDescription() => config.description;

    // Public methods to update config
    public void UpdateAppName(string name) { config.appName = name; SaveConfig(); }
    public void UpdateVersion(string version) { config.version = version; SaveConfig(); }
    public void UpdateAuthor(string author) { config.author = author; SaveConfig(); }
    public void UpdateDescription(string desc) { config.description = desc; SaveConfig(); }
}

[System.Serializable]
public class AppConfig
{
    public string appName;
    public string version;
    public string author;
    public string description;
}
