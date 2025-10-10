using System;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Diagnostics;
using System.IO;
using Debug = UnityEngine.Debug;

public class WindowTitleChanger : MonoBehaviour
{
    public string windowTitle;
    public string processName;

#if UNITY_STANDALONE_WIN
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern bool SetWindowText(IntPtr hwnd, string lpString);

    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);
    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hWnd);
#endif

    private static string CONFIG_SECTION = "Title";
    private static string DEFAULT_TITLE = null;
    private const string WINDOW_CLASS = "UnityWndClass";

    // 读取KVP配置并返回标题
    private static string GetTitleFromKvp()
    {
        string configPath = Path.Combine(Application.dataPath, "..", "AppConfig.ini");
        string fallback = DEFAULT_TITLE ?? "UnityApp";
        if (!File.Exists(configPath)) return fallback;
        using var reader = new IniFileReader(configPath);
        // 优先取Title节下的ThisWindowTitle，否则取MainGameWindowTitle，否则默认
        string title = reader.GetValue(CONFIG_SECTION, "ThisWindowTitle")
            ?? reader.GetValue(CONFIG_SECTION, "MainGameWindowTitle")
            ?? fallback;
        return title;
    }
    public static void Awake()
    {
        if (DEFAULT_TITLE == null)
        {
            DEFAULT_TITLE = Application.productName;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        try
        {
            string title = GetTitleFromKvp();
            // 检查是否为管理员权限
            if (IsRunningAsAdmin())
            {
                string langCode;
                using var languageCodeMemeLoader = new IniFileReader(Path.Combine(Application.dataPath, "..", "AppConfig.ini"));
                langCode = languageCodeMemeLoader.GetValue("Localization", "DisplayLanguage");
                if (langCode == "zh_MEMES")
                {
                    title += " (Admin) (未响应)";
                }
                else
                {
                    title += " (Admin)";
                }
            }
            else
            {
                string langCode;
                using var languageCodeMemeLoader = new IniFileReader(Path.Combine(Application.dataPath, "..", "AppConfig.ini"));
                langCode = languageCodeMemeLoader.GetValue("Localization", "DisplayLanguage");
                if (langCode == "zh_MEMES")
                {
                    title += " (未响应)";
                }
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
        try
        {
            // 更可靠地使用 WindowsPrincipal 检测管理员权限
            var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
            var principal = new System.Security.Principal.WindowsPrincipal(identity);
            return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
        }
        catch
        {
            return false;
        }
#else
        return false;
#endif
    }

    private static void ApplyWindowTitle(string title)
    {
#if UNITY_STANDALONE_WIN
        // 尝试通过当前进程的主窗口句柄设置（通常在非 Editor 下可用）
        IntPtr hwnd = Process.GetCurrentProcess().MainWindowHandle;
        if (hwnd == IntPtr.Zero)
        {
            // 如果主窗口句柄为空，枚举顶层窗口以找到匹配的进程窗口
            hwnd = FindMainWindowHandleOfProcess(Process.GetCurrentProcess());
        }
        if (hwnd != IntPtr.Zero)
        {
            SetWindowText(hwnd, title);
        }
        else
        {
            Debug.LogWarning("未找到自身窗口句柄，标题未能设置。");
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
                if (string.IsNullOrEmpty(processName))
                {
                    Debug.LogWarning("processName未设置");
                    shouldCheckProcess = false;
                    return;
                }
                Process[] processes = Process.GetProcessesByName(processName);
                if (processes.Length == 0)
                {
                    shouldCheckProcess = false;
                    Debug.LogWarning($"未找到目标进程: {processName}");
                    return;
                }
                targetProcess = processes[0];
            }

            if (targetProcess != null && !targetProcess.HasExited)
            {
                // 尝试通过进程主窗口句柄或枚举窗口找到目标窗口
                IntPtr hwnd = FindMainWindowHandleOfProcess(targetProcess);
                if (hwnd != IntPtr.Zero)
                {
                    string title = GetTitleFromKvp();
                    string langCode;
                    using var languageCodeMemeLoader = new IniFileReader(Path.Combine(Application.dataPath, "..", "AppConfig.ini"));
                    langCode = languageCodeMemeLoader.GetValue("Localization", "DisplayLanguage");
                    if (langCode == "zh_MEMES")
                    {
                        title += " (未响应)";
                    }
                    SetWindowText(hwnd, title);
                    shouldCheckProcess = false;
                }
                else
                {
                    Debug.LogWarning("未找到目标进程窗口句柄。");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("修改外部窗口标题失败: " + ex.Message);
            shouldCheckProcess = false;
        }
    }

    // 寻找指定进程的主窗口句柄：优先使用 Process.MainWindowHandle，否则枚举顶层窗口匹配 PID
    private static IntPtr FindMainWindowHandleOfProcess(Process proc)
    {
        if (proc == null) return IntPtr.Zero;
        try
        {
            if (proc.MainWindowHandle != IntPtr.Zero) return proc.MainWindowHandle;

            IntPtr found = IntPtr.Zero;
            uint targetPid = (uint)proc.Id;
            EnumWindows((hWnd, lParam) =>
            {
                if (!IsWindowVisible(hWnd)) return true; // continue
                GetWindowThreadProcessId(hWnd, out uint pid);
                if (pid == targetPid)
                {
                    // 确保窗口标题非空（避免托盘/隐藏窗口）
                    var sb = new System.Text.StringBuilder(256);
                    GetWindowText(hWnd, sb, sb.Capacity);
                    if (!string.IsNullOrWhiteSpace(sb.ToString()))
                    {
                        found = hWnd;
                        return false; // stop enumeration
                    }
                }
                return true; // continue enumeration
            }, IntPtr.Zero);
            return found;
        }
        catch
        {
            return IntPtr.Zero;
        }
    }
}