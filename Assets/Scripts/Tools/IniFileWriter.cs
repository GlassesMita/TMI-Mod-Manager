using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

// Single, clean implementation of IniFileWriter
public class IniFileWriter
{
    private readonly string filePath;

    public IniFileWriter(string path)
    {
        filePath = path;
    }

    private bool IsValidSectionLine(string line, string targetSection = null)
    {
        var trimmedLine = line?.Trim();
        if (string.IsNullOrEmpty(trimmedLine)) return false;
        if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
        {
            if (targetSection != null)
            {
                var section = trimmedLine.Substring(1, trimmedLine.Length - 2);
                return string.Equals(section, targetSection, StringComparison.OrdinalIgnoreCase);
            }
            return true;
        }
        return false;
    }

    public void WriteValue(string section, string key, string value)
    {
        WriteValues(section, key, new[] { value });
    }

    public void WriteValues(string section, string key, IEnumerable<string> values)
    {
        EnsureDirectoryExists();
        string serialized = string.Join(",", values ?? Array.Empty<string>());

    string tempPath = filePath + ".tmp";

        var output = new List<string>();

        if (!File.Exists(filePath))
        {
            output.Add($"[{section}]");
            output.Add($"{key}={serialized}");
            File.WriteAllLines(tempPath, output, Encoding.UTF8);
            ReplaceFile(tempPath, filePath);
            return;
        }

        var lines = File.ReadAllLines(filePath, Encoding.UTF8);

        bool inSection = false;
        bool wroteKey = false;
        bool sectionExists = false;

        for (int i = 0; i < lines.Length; i++)
        {
            var raw = lines[i];
            var trimmed = raw?.Trim();

            if (IsValidSectionLine(raw, section))
            {
                sectionExists = true;
                inSection = true;
                output.Add(raw);
                continue;
            }

            if (inSection)
            {
                if (IsValidSectionLine(raw))
                {
                    if (!wroteKey)
                    {
                        output.Add($"{key}={serialized}");
                        wroteKey = true;
                    }
                    output.Add(raw);
                    inSection = false;
                    continue;
                }

                if (!string.IsNullOrEmpty(trimmed) && trimmed.StartsWith(key + "=", StringComparison.OrdinalIgnoreCase))
                {
                    if (!wroteKey)
                    {
                        output.Add($"{key}={serialized}");
                        wroteKey = true;
                    }
                    continue; // skip original key line(s)
                }
            }

            output.Add(raw);
        }

        if (inSection && !wroteKey)
        {
            output.Add($"{key}={serialized}");
            wroteKey = true;
        }

        if (!sectionExists)
        {
            output.Add($"[{section}]");
            output.Add($"{key}={serialized}");
        }

        File.WriteAllLines(tempPath, output, Encoding.UTF8);
        ReplaceFile(tempPath, filePath);
    }

    private void EnsureDirectoryExists()
    {
        var dir = Path.GetDirectoryName(filePath);
        if (string.IsNullOrEmpty(dir)) return;
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
    }

    private void ReplaceFile(string tempPath, string targetPath)
    {
        try
        {
            if (File.Exists(targetPath)) File.Delete(targetPath);
            File.Move(tempPath, targetPath);
        }
        catch (Exception)
        {
            try
            {
                File.Copy(tempPath, targetPath, true);
                File.Delete(tempPath);
            }
            catch { }
        }
    }

    public void DeleteKey(string section, string key)
    {
        if (!File.Exists(filePath)) return;

    string tempPath = filePath + ".tmp";

        var lines = File.ReadAllLines(filePath, Encoding.UTF8);
        var output = new List<string>();

        bool inSection = false;
        for (int i = 0; i < lines.Length; i++)
        {
            var raw = lines[i];
            if (IsValidSectionLine(raw, section))
            {
                inSection = true;
                output.Add(raw);
                continue;
            }
            if (inSection)
            {
                if (IsValidSectionLine(raw))
                {
                    inSection = false;
                    output.Add(raw);
                    continue;
                }

                if (!string.IsNullOrEmpty(raw) && raw.TrimStart().StartsWith(key + "=", StringComparison.OrdinalIgnoreCase))
                {
                    continue; // skip this key line
                }
            }
            output.Add(raw);
        }

        File.WriteAllLines(tempPath, output, Encoding.UTF8);
        ReplaceFile(tempPath, filePath);
    }

    public void DeleteSection(string section)
    {
        if (!File.Exists(filePath)) return;

    string tempPath = filePath + ".tmp";

        var lines = File.ReadAllLines(filePath, Encoding.UTF8);
        var output = new List<string>();

        bool inSection = false;
        for (int i = 0; i < lines.Length; i++)
        {
            var raw = lines[i];
            if (IsValidSectionLine(raw, section))
            {
                inSection = true;
                continue;
            }
            if (inSection)
            {
                if (IsValidSectionLine(raw))
                {
                    inSection = false;
                    output.Add(raw);
                }
                continue;
            }
            output.Add(raw);
        }

        File.WriteAllLines(tempPath, output, Encoding.UTF8);
        ReplaceFile(tempPath, filePath);
    }

}