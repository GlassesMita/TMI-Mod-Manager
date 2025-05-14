using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class WindowTitleChanger : MonoBehaviour
{
    public string windowTitle;
    // ����Windows API����
    [DllImport("user32.dll", EntryPoint = "FindWindow")]
    public static extern IntPtr FindWindow(string className, string windowName);

    [DllImport("user32.dll", EntryPoint = "SetWindowText")]
    public static extern bool SetWindowText(IntPtr hwnd, string lpString);

    void Update()
    {
        // �����������Ҫ���ҵĴ��ڱ�����"Old Window Title"
        // �������뽫�����޸�Ϊ"New Window Title"
        IntPtr windowPtr = FindWindow(null, windowTitle);
        if (windowPtr != IntPtr.Zero)
        {
            SetWindowText(windowPtr, "Modded " + windowTitle);
        }
    }
}