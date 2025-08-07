using System;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Diagnostics;
using System.IO;

public class WindowTitleChanger : MonoBehaviour
{
    public string windowTitle;
    public string processName;

#if UNITY_STANDALONE_WIN
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr FindWindow(string className, string windowName);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern bool SetWindowText(IntPtr hwnd, string title);

    [DllImport("shell32.dll", SetLastError = true)]
    private static extern bool IsUserAnAdmin();
#endif

    private static string CONFIG_SECTION = "Title";
    private static string DEFAULT_TITLE = Application.productName;
    private const string WINDOW_CLASS = "UnityWndClass";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        try
        {
            var title = LoadTitleFromConfig();

            // 检查是否为管理员权限
            if (IsRunningAsAdmin())
            {
                title += " (Admin)";
            }

            if (!string.IsNullOrEmpty(title))
            {
                ApplyWindowTitle(title);
                Debug.Log($"成功设置窗口标题: {title}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"窗口标题设置失败: {ex.Message}");
            ApplyWindowTitle(DEFAULT_TITLE);
        }
#endif
    }

    // 使用 Win32 API 检查管理员权限
    public static bool IsRunningAsAdmin()
    {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        return IsUserAnAdmin();
#else
        return false;
#endif
    }

    private static string LoadTitleFromConfig()
    {
        string configPath = Path.Combine(
            Application.dataPath,
            "..",
            "Definitions.ini"
        );

        if (!File.Exists(configPath)) return DEFAULT_TITLE;

        using var reader = new IniFileReader(configPath);
        return reader.GetValue(CONFIG_SECTION, "ThisWindowTitle")
            ?? reader.GetValue(CONFIG_SECTION, "MainGameWindowTitle")
            ?? DEFAULT_TITLE;
    }

    private static void ApplyWindowTitle(string title)
    {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        IntPtr hwnd = FindWindow(WINDOW_CLASS, null);
        if (hwnd != IntPtr.Zero)
        {
            // 确保使用 Unicode 编码
            SetWindowText(hwnd, title);
        }
#endif


    }

    private bool shouldCheckProcess = false;
    private Process targetProcess = null;

    // 可由按钮调用的方法
    public void StartCheckingProcess()
    {
        shouldCheckProcess = true;
        targetProcess = null;
    }

    void Update()
    {
        if (!shouldCheckProcess) return;

        try
        {
            if (targetProcess == null || targetProcess.HasExited)
            {
                // 查找目标进程
                Process[] processes = Process.GetProcessesByName("Touhou Mystia Izakaya.exe");
                if (processes.Length == 0)
                {
                    shouldCheckProcess = false;
                    return;
                }
                targetProcess = processes[0];
            }

            if (targetProcess != null && !targetProcess.HasExited)
            {
                IntPtr hwnd = targetProcess.MainWindowHandle;
                if (hwnd != IntPtr.Zero && targetProcess.MainWindowTitle == windowTitle)
                {
                    SetWindowText(hwnd, windowTitle);
                    shouldCheckProcess = false; // 修改成功后停止检测
                }
            }
        }
        catch
        {
            shouldCheckProcess = false;
        }
    }
}