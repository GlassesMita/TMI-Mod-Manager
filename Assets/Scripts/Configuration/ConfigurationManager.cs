using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using Shortcut;

public class ConfigurationManager : MonoBehaviour
{
    [Header("Configuration Settings")]
    [Tooltip("Path to the configuration file")]
    public string configFilePath;
    private IniFileReader configFileReader;

    public Dropdown languageDropdown;

    // 新增：分辨率下拉框和全屏切换
    public Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;

    public Toggle shortcutToggle;
    // 用于控制 OsuCursor 指针基准高度（像素），UI 上的 Slider
    public Slider cursorSizeSlider;
    // 可选：显示当前像素值的文本（实时显示 40 + slider.value）
    public Text cursorSizeLabel;

    // 存储语言名与文件名的映射
    private List<(string languageName, string fileName)> languageList = new();

    // 存储分辨率字符串列表
    private List<string> availableResolutions = new();

    void Start()
    {
        configFilePath = Path.Combine(Application.dataPath, "..", "AppConfig.ini");
        LoadConfiguration();

        // 填充下拉框
        PopulateLanguageDropdown();
        PopulateResolutionDropdown();
        PopulateFullscreenToggle();
        PopulateShortcutToggle();

        // 自动根据ini配置选择下拉框和Label的值
        // 语言下拉框
        string iniLang = configFileReader?.GetValue("Localization", "DisplayLanguage");
        if (!string.IsNullOrEmpty(iniLang) && languageDropdown != null && languageList.Count > 0)
        {
            int idx = languageList.FindIndex(x => Path.GetFileNameWithoutExtension(x.fileName) == iniLang);
            if (idx >= 0)
            {
                languageDropdown.value = idx;
                // 自动填充到Label（假设Label为Dropdown的CaptionText）
                var label = languageDropdown.transform.Find("Label");
                if (label != null)
                {
                    var labelText = label.GetComponent<Text>();
                    if (labelText != null)
                        labelText.text = languageList[idx].languageName;
                }
            }
        }

        // 分辨率下拉框
        string iniRes = configFileReader?.GetValue("Display", "ScreenResolution");
        if (!string.IsNullOrEmpty(iniRes) && resolutionDropdown != null && availableResolutions.Count > 0)
        {
            int idx = availableResolutions.FindIndex(r => r == iniRes);
            if (idx >= 0)
            {
                resolutionDropdown.value = idx;
                var label = resolutionDropdown.transform.Find("Label");
                if (label != null)
                {
                    var labelText = label.GetComponent<Text>();
                    if (labelText != null)
                        labelText.text = availableResolutions[idx];
                }
            }
        }

        // 全屏Toggle
        string iniFullscreen = configFileReader?.GetValue("Display", "Fullscreen");
        if (!string.IsNullOrEmpty(iniFullscreen) && fullscreenToggle != null)
        {
            bool isFullscreen = false;
            bool.TryParse(iniFullscreen, out isFullscreen);
            fullscreenToggle.isOn = isFullscreen;
        }

        string iniShortcut = configFileReader?.GetValue("Shortcut", "ShowIndicator");
        if (!string.IsNullOrEmpty(iniShortcut) && shortcutToggle != null)
        {
            bool isShowIndicator = false;
            bool.TryParse(iniShortcut, out isShowIndicator);
            shortcutToggle.isOn = isShowIndicator;
        }

        // 读取并初始化 Cursor 大小 Slider（Slider value = 0..20，对应实际像素 40..60）
        if (cursorSizeSlider != null)
        {
            cursorSizeSlider.minValue = 0f;
            cursorSizeSlider.maxValue = 20f;
            string iniCursorSize = configFileReader?.GetValue("Cursor", "CursorBaseHeight");
            float cursorBase = 40f;
            if (!string.IsNullOrEmpty(iniCursorSize)) float.TryParse(iniCursorSize, out cursorBase);
            cursorBase = Mathf.Clamp(cursorBase, 40f, 60f);
            // 将实际像素值转换为 Slider 的 0..20 范围
            float sliderValue = cursorBase - 40f;
            cursorSizeSlider.value = sliderValue;
            cursorSizeSlider.onValueChanged.AddListener(OnCursorSizeSliderChanged);

            // 更新 label 并将初始值应用到 OsuCursor（如果存在）
            float displayValue = 40f + cursorSizeSlider.value;
            if (cursorSizeLabel != null)
                cursorSizeLabel.text = Mathf.RoundToInt(displayValue).ToString() + " px";

            var osu = FindObjectOfType<OsuCursor>();
            if (osu != null)
            {
                osu.SetBaseHeight(displayValue);
            }
        }
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

    // 自动识别本地化文件并填充下拉框
    private void PopulateLanguageDropdown()
    {
        languageDropdown.ClearOptions();
        languageList = GetLanguageList();

        List<string> options = new();
        foreach (var (languageName, _) in languageList)
        {
            options.Add(languageName);
        }
        languageDropdown.AddOptions(options);

        // 可选：设置默认选中项
        string currentLanguage = configFileReader?.GetValue("Localization", "DisplayLanguage");
        if (!string.IsNullOrEmpty(currentLanguage))
        {
            int idx = languageList.FindIndex(x => x.fileName == currentLanguage);
            if (idx >= 0)
                languageDropdown.value = idx;
        }

        languageDropdown.onValueChanged.AddListener(OnLanguageDropdownChanged);
    }

    // 新增：填充分辨率下拉框
    private void PopulateResolutionDropdown()
    {
        if (resolutionDropdown == null) return;

        resolutionDropdown.ClearOptions();
        availableResolutions.Clear();

        // 从配置文件读取分辨率
        string[] resolutions = configFileReader?.GetValues("Display", "AvailableScreenResolutions");
        if (resolutions != null && resolutions.Length > 0)
        {
            availableResolutions.AddRange(resolutions);
            resolutionDropdown.AddOptions(availableResolutions);

            // 读取当前分辨率
            string currentRes = configFileReader.GetValue("Display", "ScreenResolution");
            int idx = availableResolutions.FindIndex(r => r == currentRes);
            if (idx >= 0)
                resolutionDropdown.value = idx;
        }

        resolutionDropdown.onValueChanged.AddListener(OnResolutionDropdownChanged);
    }

    // 新增：填充全屏Toggle
    private void PopulateFullscreenToggle()
    {
        if (fullscreenToggle == null) return;

        string fullscreenValue = configFileReader?.GetValue("Display", "Fullscreen");
        bool isFullscreen = false;
        bool.TryParse(fullscreenValue, out isFullscreen);
        fullscreenToggle.isOn = isFullscreen;

        fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggleChanged);
    }

    private void PopulateShortcutToggle()
    {
        if (shortcutToggle == null) return;

        string showIndicatorValue = configFileReader?.GetValue("Shortcut", "ShowIndicator");
        bool showIndicator = true;
        bool.TryParse(showIndicatorValue, out showIndicator);
        shortcutToggle.isOn = showIndicator;

        var shortcutManager = FindObjectOfType<ShortcutManager>();
        if (shortcutManager != null)
        {
            shortcutManager.showShortcutIndicator = showIndicator;
        }
    }

    // 新增：分辨率下拉框变化时
    private void OnResolutionDropdownChanged(int index)
    {
        if (index >= 0 && index < availableResolutions.Count)
        {
            string selectedRes = availableResolutions[index];
            Debug.Log($"Selected resolution: {selectedRes}");

            // 可选：立即应用分辨率
            ApplyResolution(selectedRes, fullscreenToggle != null && fullscreenToggle.isOn);
        }
    }

    // 新增：全屏Toggle变化时
    private void OnFullscreenToggleChanged(bool isOn)
    {
        Debug.Log($"Fullscreen toggled: {isOn}");

        // 可选：立即应用分辨率
        if (resolutionDropdown != null && resolutionDropdown.value >= 0 && resolutionDropdown.value < availableResolutions.Count)
        {
            string selectedRes = availableResolutions[resolutionDropdown.value];
            ApplyResolution(selectedRes, isOn);
        }
    }

    // 新增：应用分辨率和全屏设置
    private void ApplyResolution(string resolution, bool fullscreen)
    {
        // 解析分辨率字符串（如 "1920x1080"）
        var parts = resolution.Split('x');
        if (parts.Length == 2 && int.TryParse(parts[0], out int width) && int.TryParse(parts[1], out int height))
        {
            Screen.SetResolution(width, height, fullscreen);
        }
    }

    // 获取所有本地化文件的语言名和文件名
    private List<(string languageName, string fileName)> GetLanguageList()
    {
        var result = new List<(string, string)>();
        string localizationPath = Path.Combine(Application.streamingAssetsPath, "Localization");

        if (!Directory.Exists(localizationPath))
        {
            Debug.LogWarning($"Localization directory not found: {localizationPath}");
            return result;
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
                            string fileName = Path.GetFileName(filePath);
                            result.Add((languageName, fileName));
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

        return result;
    }

    // 下拉框选项变化时调用
    private void OnLanguageDropdownChanged(int index)
    {
        if (index >= 0 && index < languageList.Count)
        {
            string selectedFileName = languageList[index].fileName;
            Debug.Log($"Selected language file: {selectedFileName}");
        }

        // 只为选中的元素启用 Checkmark
        // Dropdown 展开时，选项在 Dropdown 下的 Template/Viewport/Content 下
        Transform template = languageDropdown.transform.Find("Template");
        if (template == null) return;
        Transform content = template.Find("Viewport/Content");
        if (content == null) return;

        for (int i = 0; i < content.childCount; i++)
        {
            var item = content.GetChild(i);
            var toggle = item.GetComponent<Toggle>();
            if (toggle != null)
            {
                // 只为选中的元素启用 Checkmark
                toggle.isOn = (i == languageDropdown.value);
            }
        }
    }

    // 按钮点击时调用，保存所选语言到 AppConfig.ini
    public void SaveConfiguration()
    {
        if (languageDropdown.value >= 0 && languageDropdown.value < languageList.Count)
        {
            string selectedFileName = languageList[languageDropdown.value].fileName;
            string configPath = configFilePath;

            // 去除 .ini 后缀，仅保存文件名
            string languageCode = Path.GetFileNameWithoutExtension(selectedFileName);

            // 使用 IniFileWriter 写入 DisplayLanguage
            var iniWriter = new IniFileWriter(configPath);
            iniWriter.WriteValue("Localization", "DisplayLanguage", languageCode);

            Debug.Log($"Saved language: {languageCode} to {configPath}");
        }

        // 保存分辨率和全屏设置
        if (resolutionDropdown != null && availableResolutions.Count > 0 && resolutionDropdown.value >= 0 && resolutionDropdown.value < availableResolutions.Count)
        {
            string selectedRes = availableResolutions[resolutionDropdown.value];
            var iniWriter = new IniFileWriter(configFilePath);
            iniWriter.WriteValue("Display", "ScreenResolution", selectedRes);
        }
        if (fullscreenToggle != null)
        {
            var iniWriter = new IniFileWriter(configFilePath);
            iniWriter.WriteValue("Display", "IsFullScreen", fullscreenToggle.isOn.ToString());
        }

        if (shortcutToggle != null)
        {
            var iniWriter = new IniFileWriter(configFilePath);
            iniWriter.WriteValue("Shortcut", "ShowIndicator", shortcutToggle.isOn.ToString());
        }

        // 保存 Cursor 大小到 INI
        if (cursorSizeSlider != null)
        {
            // 保存实际像素高度（40 + slider.value）
            float displayValue = 40f + cursorSizeSlider.value;
            var iniWriter = new IniFileWriter(configFilePath);
            iniWriter.WriteValue("Cursor", "CursorBaseHeight", displayValue.ToString());
        }
    }

    // Slider 回调：实时更新 OsuCursor 的基准高度
    private void OnCursorSizeSliderChanged(float val)
    {
        float displayValue = 40f + val;
        if (cursorSizeLabel != null)
            cursorSizeLabel.text = Mathf.RoundToInt(displayValue).ToString() + " px";

        var osu = FindObjectOfType<OsuCursor>();
        if (osu != null)
        {
            osu.SetBaseHeight(displayValue);
        }
    }
}