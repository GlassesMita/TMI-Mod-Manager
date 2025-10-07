using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.InputSystem.EnhancedTouch;
using System.Runtime.InteropServices;
using System.Diagnostics;

public class Logger : MonoBehaviour
{
    public enum LogLevel
    {
        Info,
        Warning,
        Error
    }
    private static string logFilePath;

    // 静态构造函数，确保在类加载时初始化 logFilePath
    static Logger()
    {
        InitializeLogFilePath();
    }

    public static void Log(string message)
    {
        // 兼容旧版，仅传 message 时视为 Info
        Log(message, LogLevel.Info);
    }

    // 写入消息并指定等级（不使用默认参数，以免与 Log(string) 冲突）
    public static void Log(string message, LogLevel level)
    {
        if (string.IsNullOrEmpty(logFilePath))
        {
            InitializeLogFilePath();
        }

        string tag = GetLevelShortTag(level);
        string logMessage = $"[{System.DateTime.Now:yyyy/MM/dd HH:mm:ss}][{tag}] {message}";
        File.AppendAllText(logFilePath, logMessage + System.Environment.NewLine);
    }

    // 显式传入 (LogLevel, message) 的重载，方便调用方以不同顺序传参
    public static void Log(LogLevel level, string message)
    {
        Log(message, level);
    }

    // 支持格式化字符串并指定等级
    public static void Log(string format, LogLevel level, params object[] args)
    {
        string message = args != null && args.Length > 0 ? string.Format(format, args) : format;
        Log(message, level);
    }

    private static string GetLevelShortTag(LogLevel level)
    {
        switch (level)
        {
            case LogLevel.Warning: return "W";
            case LogLevel.Error: return "E";
            case LogLevel.Info:
            default:
                return "I";
        }
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
