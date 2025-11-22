using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class FileDragHandler : MonoBehaviour
{
    public event Action<List<string>> OnFilesDropped;

#if UNITY_STANDALONE_WIN
    private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
    private IntPtr _oldWndProcPtr;
    private IntPtr _newWndProcPtr;
    private IntPtr _unityWindowHandle;
    private WndProcDelegate _newWndProc;

    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
    private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
    private static extern IntPtr SetWindowLong32(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    [DllImport("shell32.dll")]
    private static extern void DragAcceptFiles(IntPtr hWnd, bool fAccept);

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern uint DragQueryFile(IntPtr hDrop, uint iFile, System.Text.StringBuilder lpszFile, uint cch);

    [DllImport("shell32.dll")]
    private static extern void DragFinish(IntPtr hDrop);

    private const int GWL_WNDPROC = -4;
    private const uint WM_DROPFILES = 0x0233;

    private static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
    {
        if (IntPtr.Size == 8)
            return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
        else
            return SetWindowLong32(hWnd, nIndex, dwNewLong);
    }

    void OnEnable()
    {
#if !UNITY_EDITOR
        _unityWindowHandle = GetActiveWindow();
        if (_unityWindowHandle != IntPtr.Zero)
        {
            DragAcceptFiles(_unityWindowHandle, true);
            _newWndProc = new WndProcDelegate(WndProc);
            _newWndProcPtr = Marshal.GetFunctionPointerForDelegate(_newWndProc);
            _oldWndProcPtr = SetWindowLongPtr(_unityWindowHandle, GWL_WNDPROC, _newWndProcPtr);
        }
#endif
    }

    void OnDisable()
    {
#if !UNITY_EDITOR
        if (_unityWindowHandle != IntPtr.Zero && _oldWndProcPtr != IntPtr.Zero)
        {
            SetWindowLongPtr(_unityWindowHandle, GWL_WNDPROC, _oldWndProcPtr);
            DragAcceptFiles(_unityWindowHandle, false);
            _oldWndProcPtr = IntPtr.Zero;
            _unityWindowHandle = IntPtr.Zero;
        }
#endif
    }

    private IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        if (msg == WM_DROPFILES)
        {
            IntPtr hDrop = wParam;
            uint count = DragQueryFile(hDrop, 0xFFFFFFFF, null, 0);
            var files = new List<string>();
            var sb = new System.Text.StringBuilder(1024);

            for (uint i = 0; i < count; i++)
            {
                if (DragQueryFile(hDrop, i, sb, 1024) > 0)
                {
                    files.Add(sb.ToString());
                }
            }

            DragFinish(hDrop);

            if (files.Count > 0)
            {
                OnFilesDropped?.Invoke(files);
            }
            return IntPtr.Zero;
        }

        return CallWindowProc(_oldWndProcPtr, hWnd, msg, wParam, lParam);
    }
#endif
}
