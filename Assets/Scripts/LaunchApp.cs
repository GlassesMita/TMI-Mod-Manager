using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System;

public class LaunchApp : MonoBehaviour
{
    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    public static extern int ShellExecute(IntPtr hwnd, string lpszOp, string lpszFile,
                                    string lpszParams, string lpszDir, int FsShowCmd);

    public void LaunchApplication(string exePath)
    {
        ShellExecute(IntPtr.Zero, "open", exePath, "", "", 1);
    }
}
