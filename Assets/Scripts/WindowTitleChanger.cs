using System;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Diagnostics;

public class WindowTitleChanger : MonoBehaviour
{
    public string windowTitle;
    public string processName;

    [DllImport("user32.dll", EntryPoint = "SetWindowText")]
    public static extern bool SetWindowText(IntPtr hwnd, string lpString);

    void Update()
    {
        // 查找所有指定进程名的进程
        Process[] processes = Process.GetProcessesByName(processName);
        foreach (Process proc in processes)
        {
            IntPtr hwnd = proc.MainWindowHandle;
            if (hwnd != IntPtr.Zero && proc.MainWindowTitle == windowTitle)
            {
                SetWindowText(hwnd, "Modded " + windowTitle);
                return; // 修改后立即跳出方法体
            }
        }
    }
}
