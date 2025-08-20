using System;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace WindowTitleTool
{
    class Program
    {
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr FindWindow(string className, string windowName);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern bool SetWindowText(IntPtr hwnd, string title);

        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("用法: WindowTitleTool.exe <进程名> <新标题>");
                return;
            }
            string processName = args[0];
            string newTitle = args[1];
            Process[] processes = Process.GetProcessesByName(processName);
            if (processes.Length == 0)
            {
                Console.WriteLine($"未找到进程: {processName}");
                return;
            }
            foreach (var proc in processes)
            {
                IntPtr hwnd = proc.MainWindowHandle;
                if (hwnd == IntPtr.Zero)
                {
                    Console.WriteLine($"进程 {proc.Id} 没有主窗口句柄");
                    continue;
                }
                SetWindowText(hwnd, newTitle);
                Console.WriteLine($"已设置进程 {proc.Id} 标题为: {newTitle}");
            }
        }
    }
}
