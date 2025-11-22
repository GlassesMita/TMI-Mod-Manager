using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

public class JsonFileParser : IDisposable
{
    private Dictionary<string, Dictionary<string, string>> data;

    public JsonFileParser(string filePath)
    {
        data = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        LoadJson(filePath);
    }

    private void LoadJson(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");

        string jsonContent = File.ReadAllText(filePath);
        var jsonData = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(jsonContent);

        if (jsonData != null)
        {
            foreach (var section in jsonData)
            {
                data[section.Key] = new Dictionary<string, string>(section.Value, StringComparer.OrdinalIgnoreCase);
            }
        }
    }

    public List<string> GetSections()
    {
        return new List<string>(data.Keys);
    }

    public List<string> GetKeys(string section)
    {
        if (data.TryGetValue(section, out var sectionData))
        {
            return new List<string>(sectionData.Keys);
        }
        return new List<string>();
    }

    public bool TryGetValue(string section, string key, out string value)
    {
        value = null;
        if (data.TryGetValue(section, out var sectionData) && sectionData.TryGetValue(key, out value))
        {
            return true;
        }
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

    public string GetValue(string section, string key)
    {
        if (data.TryGetValue(section, out var sectionData) && sectionData.TryGetValue(key, out var value))
        {
            return value;
        }
        return null;
    }

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

    public List<string> GetValueList(string section, string key)
    {
        return new List<string>(GetValues(section, key));
    }

    public void Dispose()
    {
        data?.Clear();
    }
}