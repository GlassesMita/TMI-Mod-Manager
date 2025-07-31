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

    public void Dispose()
    {
        data?.Clear();
    }
}