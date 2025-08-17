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

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];

                if (IsValidSectionLine(line, section))
                {
                    sectionFound = true;
                    sb.AppendLine(line);
                    continue;
                }

                // 如果进入了目标 section
                if (sectionFound)
                {
                    // 如果遇到下一个 section，且还没写 key，则插入 key
                    if (IsValidSectionLine(line))
                    {
                        if (!keyWritten)
                        {
                            sb.AppendLine($"{key}={value}");
                            keyWritten = true;
                        }
                        sb.AppendLine(line);
                        sectionFound = false;
                        continue;
                    }

                    // 如果是目标 key，则替换
                    if (line.TrimStart().StartsWith($"{key}=", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!keyWritten)
                        {
                            sb.AppendLine($"{key}={value}");
                            keyWritten = true;
                        }
                        else
                        {
                            // 跳过重复的 key
                        }
                        continue;
                    }
                }

                sb.AppendLine(line);
            }

            // 如果文件结尾还在目标 section 且没写 key，则追加
            if (sectionFound && !keyWritten)
            {
                sb.AppendLine($"{key}={value}");
                keyWritten = true;
            }

            // 如果整个文件都没有目标 section，则追加
            if (!sectionFound && !keyWritten)
            {
                sb.AppendLine($"[{section}]");
                sb.AppendLine($"{key}={value}");
            }
        }

        File.WriteAllText(filePath, sb.ToString());
    }
}