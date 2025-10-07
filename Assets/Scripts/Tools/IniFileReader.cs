using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class IniFileReader : IDisposable
{
    private Dictionary<string, Dictionary<string, string>> data;

    public IniFileReader(string filePath)
    {
        data = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        LoadWithEncoding(filePath);
    }

    // 返回所有节名（不含方括号）
    public List<string> GetSections()
    {
        return new List<string>(data.Keys);
    }

    // 返回指定节下的所有键名
    public List<string> GetKeys(string section)
    {
        if (data.TryGetValue(section, out var sectionData))
        {
            return new List<string>(sectionData.Keys);
        }
        return new List<string>();
    }

    // TryGet 风格安全读取
    public bool TryGetValue(string section, string key, out string value)
    {
        value = null;
        if (data.TryGetValue(section, out var sectionData) && sectionData.TryGetValue(key, out value))
        {
            return true;
        }
        value = null;
        return false;
    }

    public bool HasSection(string section)
    {
        return data.ContainsKey(section);
    }

    public bool HasKey(string section, string key)
    {
        return data.TryGetValue(section, out var sectionData) && sectionData.ContainsKey(key);
    }

    private void LoadWithEncoding(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"文件未找到: {filePath}");

        // 使用 UTF-8 编码读取（自动检测 BOM）
        string[] lines = File.ReadAllLines(filePath, Encoding.UTF8);
        string currentSection = null;

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();

            if (string.IsNullOrEmpty(line) || line.StartsWith(";"))
                continue;

            if (line.StartsWith("[") && line.EndsWith("]"))
            {
                currentSection = line.Substring(1, line.Length - 2);
                data[currentSection] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }
            else if (currentSection != null)
            {
                var separatorIndex = line.IndexOf('=');
                if (separatorIndex > 0)
                {
                    var key = line.Substring(0, separatorIndex).Trim();
                    var value = line.Substring(separatorIndex + 1).Trim();
                    data[currentSection][key] = DecodeString(value);
                }
            }
        }
    }

    // 处理转义字符（可选）
    private string DecodeString(string input)
    {
        return input
            .Replace("\\n", "\n")
            .Replace("\\t", "\t");
    }

    public string GetValue(string section, string key)
    {
        if (data.TryGetValue(section, out var sectionData) &&
            sectionData.TryGetValue(key, out var value))
        {
            return value;
        }
        return null;
    }

    /// <summary>
    /// 获取以逗号分隔的值，返回 string[]
    /// </summary>
    public string[] GetValues(string section, string key)
    {
        var value = GetValue(section, key);
        if (string.IsNullOrEmpty(value))
            return Array.Empty<string>();
        var parts = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < parts.Length; i++)
        {
            parts[i] = parts[i].Trim();
        }
        return parts;
    }

    /// <summary>
    /// 获取以逗号分隔的值，返回 List<string>;
    /// </summary>
    public List<string> GetValueList(string section, string key)
    {
        return new List<string>(GetValues(section, key));
    }

    public void Dispose()
    {
        data?.Clear();
    }
}