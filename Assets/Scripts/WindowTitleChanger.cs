using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class WindowTitleChanger : MonoBehaviour
{
    public string windowTitle;
    // 导入Windows API函数
    [DllImport("user32.dll", EntryPoint = "FindWindow")]
    public static extern IntPtr FindWindow(string className, string windowName);

    [DllImport("user32.dll", EntryPoint = "SetWindowText")]
    public static extern bool SetWindowText(IntPtr hwnd, string lpString);

    void Update()
    {
        // 这里假设你想要查找的窗口标题是"Old Window Title"
        // 并且你想将标题修改为"New Window Title"
        IntPtr windowPtr = FindWindow(null, windowTitle);
        if (windowPtr != IntPtr.Zero)
        {
            SetWindowText(windowPtr, "Modded " + windowTitle);
        }
    }
}