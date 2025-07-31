using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

public class IniFileWriter
{
    private string filePath;

    public IniFileWriter(string path)
    {
        filePath = path;
    }

    private bool IsValidSectionLine(string line, string targetSection = null)
    {
        var trimmedLine = line.Trim();
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
        StringBuilder sb = new StringBuilder();
        if (!File.Exists(filePath))
        {
            sb.AppendLine($"[{section}]");
            sb.AppendLine($"{key}={value}");
        }
        else
        {
            var lines = File.ReadAllLines(filePath);
            bool sectionFound = false;
            bool keyWritten = false;

            foreach (var line in lines)
            {
                if (IsValidSectionLine(line, section))
                {
                    sectionFound = true;
                    sb.AppendLine(line);
                }
                else if (sectionFound && IsValidSectionLine(line))
                {
                    if (!keyWritten)
                    {
                        sb.AppendLine($"{key}={value}");
                        keyWritten = true;
                    }
                    sb.AppendLine(line);
                    sectionFound = false;
                }
                else if (sectionFound && line.TrimStart().StartsWith($"{key}=", StringComparison.OrdinalIgnoreCase))
                {
                    sb.AppendLine($"{key}={value}");
                    keyWritten = true;
                }
                else
                {
                    sb.AppendLine(line);
                }
            }

            if (sectionFound && !keyWritten)
            {
                sb.AppendLine($"{key}={value}");
            }

            if (!sectionFound)
            {
                sb.AppendLine($"[{section}]");
                sb.AppendLine($"{key}={value}");
            }
        }

        File.WriteAllText(filePath, sb.ToString());
    }
}