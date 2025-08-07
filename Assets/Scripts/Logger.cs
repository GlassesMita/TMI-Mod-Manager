using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Logger : MonoBehaviour
{
    private static string logFilePath;

    // 静态构造函数，确保在类加载时初始化 logFilePath
    static Logger()
    {
        InitializeLogFilePath();
    }

    private static void InitializeLogFilePath()
    {
#if UNITY_EDITOR && !UNITY_STANDALONE_WIN
        logFilePath = Path.Combine(Application.dataPath, "..", "Logs", "Latest.Log");
#else
        logFilePath = Path.Combine(Application.dataPath, "..", "EditorRuntimeLogs", "Latest.Log");
#endif
        // 检查日志文件是否存在
        if (File.Exists(logFilePath))
        {
            // 清空文件内容
            File.WriteAllText(logFilePath, string.Empty);
        }
        else
        {
            // 创建日志文件
            Directory.CreateDirectory(Path.GetDirectoryName(logFilePath));
            File.Create(logFilePath).Dispose();
        }
    }

    public static void Log(string message)
    {
        if (string.IsNullOrEmpty(logFilePath))
        {
            InitializeLogFilePath();
        }

        string logMessage = $"[{System.DateTime.Now:yyyy/MM/dd HH:mm:ss}] {message}";
        File.AppendAllText(logFilePath, logMessage + System.Environment.NewLine);
    }
}
