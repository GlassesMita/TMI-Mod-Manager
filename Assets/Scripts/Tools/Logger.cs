using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.InputSystem.EnhancedTouch;
using System.Runtime.InteropServices;
using System.Diagnostics;

public class Logger : MonoBehaviour
{
    private static string logFilePath;

    // 静态构造函数，确保在类加载时初始化 logFilePath
    static Logger()
    {
        InitializeLogFilePath();
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

    private static void InitializeLogFilePath()
    {
        logFilePath = Path.Combine(Application.dataPath, "..", "Logs", "Latest.Log");
#if UNITY_EDITOR
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
}
