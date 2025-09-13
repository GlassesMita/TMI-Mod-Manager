using UnityEngine;
using UnityEngine.UI;
using System;
using System.Diagnostics;
using System.Text;
using System.Collections.Generic;
using System.Threading;
using System.Text.RegularExpressions; // 新增：用于处理正则表达式
using Debug = UnityEngine.Debug;

public class PowerShellInterceptor : MonoBehaviour
{
    [SerializeField] private Text consoleText;       // 显示输出的Text控件
    [SerializeField] private InputField commandInput; // 输入命令的InputField
    [SerializeField] private ScrollRect scrollRect;   // 用于自动滚动的ScrollRect
    [SerializeField] private Font consoleFont;        // 新增：指定支持非ASCII字符的字体

    private string currentDirectory;                  // 当前工作目录
    private StringBuilder outputBuffer = new StringBuilder(); // 输出缓存
    private Process powershellProcess;                // PowerShell进程实例
    private Queue<string> outputQueue = new Queue<string>();   // 线程安全的输出队列
    private object queueLock = new object();          // 队列同步锁
    // 新增：匹配ANSI转义序列的正则表达式（用于过滤格式）
    private static readonly Regex AnsiRegex = new Regex(@"\x1B(?:[@-Z\\-_]|\[[0-?]*[ -/]*[@-~])", RegexOptions.Compiled);


    private void Start()
    {
        // 初始化工作目录（默认程序运行目录）
        currentDirectory = Environment.CurrentDirectory;

        // 新增：确保使用支持非ASCII字符的字体
        if (consoleFont != null)
        {
            consoleText.font = consoleFont;
        }
        else
        {
            Debug.LogWarning("请指定支持非ASCII字符的字体（如Arial Unicode MS）以避免乱码");
        }

        // 初始化控制台欢迎信息
        AppendOutput("Praying for you 🕯️ O Great Mita 💝\n");
        AppendOutput($"Microsoft Windows {Environment.OSVersion.VersionString}\n");
        AppendOutput($"PowerShell {GetPowerShellVersion()}\n\n");
        ShowPrompt();

        // 绑定输入提交事件（按Enter执行命令）
        commandInput.onEndEdit.AddListener(OnCommandSubmitted);
    }

    // 每帧检查队列，在主线程处理输出
    private void Update()
    {
        // 锁定队列，避免多线程冲突
        lock (queueLock)
        {
            // 处理队列中所有待显示的内容
            while (outputQueue.Count > 0)
            {
                string output = outputQueue.Dequeue();
                AppendOutput(output);
            }
        }
    }

    // 获取PowerShell版本
    private string GetPowerShellVersion()
    {
        try
        {
            using (Process process = new Process())
            {
                process.StartInfo.FileName = "pwsh.exe";
                // 新增：强制使用UTF-8编码输出
                process.StartInfo.Arguments = "-Command [Console]::OutputEncoding = [System.Text.Encoding]::UTF8; $PSVersionTable.PSVersion";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
                process.StartInfo.StandardErrorEncoding = Encoding.UTF8;

                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                // 新增：清理输出中的格式字符
                return StripAnsiCodes(output).Trim();
            }
        }
        catch (Exception ex)
        {
            return $"Unknown version (错误: {ex.Message})";
        }
    }

    // 处理用户输入的命令
    private void OnCommandSubmitted(string command)
    {
        if (string.IsNullOrEmpty(command) || !Input.GetKeyDown(KeyCode.Return))
            return;

        string trimmedCommand = command.Trim();
        AppendOutput(trimmedCommand + "\n"); // 显示用户输入的命令

        ProcessCommand(trimmedCommand);      // 执行命令
        commandInput.text = "";              // 清空输入框
    }

    // 执行命令（核心：拦截PowerShell输出）
    private void ProcessCommand(string command)
    {
        // 处理内置命令（cls清空、cd切换目录）
        if (string.Equals(command, "cls", StringComparison.OrdinalIgnoreCase))
        {
            ClearConsole();
            ShowPrompt();
            return;
        }
        if (command.StartsWith("cd ", StringComparison.OrdinalIgnoreCase))
        {
            string path = command.Substring(3).Trim();
            TryChangeDirectory(path);
            ShowPrompt();
            return;
        }

        // 执行PowerShell命令
        try
        {
            // 终止之前的进程（避免冲突）
            if (powershellProcess != null && !powershellProcess.HasExited)
            {
                powershellProcess.Kill();
                powershellProcess.Dispose();
            }

            powershellProcess = new Process();
            // 执行 PowerShell 命令的代码段中，修改 StartInfo 和命令参数
            string safeCommand = $"[Console]::OutputEncoding = [System.Text.Encoding]::UTF8; {command} | Out-String -Width 1000";

            powershellProcess.StartInfo = new ProcessStartInfo
            {
                FileName = "pwsh.exe",
                Arguments = $"-Command {safeCommand}",
                WorkingDirectory = currentDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                // 关键：强制 PowerShell 输出为 UTF-8 编码
                StandardOutputEncoding = System.Text.Encoding.UTF8,
                StandardErrorEncoding = System.Text.Encoding.UTF8
            };
            // 异步捕获输出（非UI线程）
            powershellProcess.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    // 新增：移除格式字符
                    string cleanedOutput = StripAnsiCodes(e.Data);
                    lock (queueLock) // 线程安全地加入队列
                    {
                        outputQueue.Enqueue(cleanedOutput + "\n");
                    }
                }
            };

            // 异步捕获错误输出
            powershellProcess.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    // 新增：移除格式字符
                    string cleanedError = StripAnsiCodes(e.Data);
                    lock (queueLock)
                    {
                        outputQueue.Enqueue($"错误: {cleanedError}\n");
                    }
                }
            };

            // 进程退出时显示命令提示符
            powershellProcess.Exited += (sender, e) =>
            {
                lock (queueLock)
                {
                    outputQueue.Enqueue("\n");
                    outputQueue.Enqueue(ShowPrompt(false)); // 不直接显示，加入队列
                }
                powershellProcess.Dispose(); // 释放资源
            };

            // 启动进程并开始异步读取
            powershellProcess.Start();
            powershellProcess.BeginOutputReadLine();
            powershellProcess.BeginErrorReadLine();
        }
        catch (Exception ex)
        {
            lock (queueLock)
            {
                outputQueue.Enqueue($"执行失败: {ex.Message}\n");
                outputQueue.Enqueue(ShowPrompt(false));
            }
        }
    }

    // 新增：移除ANSI转义序列（格式控制字符）
    private string StripAnsiCodes(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return AnsiRegex.Replace(input, string.Empty);
    }

    // 切换目录
    private void TryChangeDirectory(string path)
    {
        try
        {
            string newPath = System.IO.Path.GetFullPath(path, currentDirectory);
            if (System.IO.Directory.Exists(newPath))
            {
                currentDirectory = newPath;
            }
            else
            {
                lock (queueLock)
                {
                    outputQueue.Enqueue($"目录不存在: {path}\n");
                }
            }
        }
        catch (Exception ex)
        {
            lock (queueLock)
            {
                outputQueue.Enqueue($"切换目录失败: {ex.Message}\n");
            }
        }
    }

    // 显示命令提示符（如：└─PS> ）
    private string ShowPrompt(bool appendDirectly = true)
    {
        string userName = Environment.UserName;
        string machineName = Environment.MachineName;
        string prompt = $"┌──({userName}@{machineName})-[{currentDirectory}]\n└─PS> ";

        if (appendDirectly)
        {
            AppendOutput(prompt);
        }
        return prompt;
    }

    // 追加内容到UI
    private void AppendOutput(string text)
    {
        outputBuffer.Append(text);
        consoleText.text = outputBuffer.ToString();
        ScrollToBottom(); // 自动滚动到底部
    }

    // 清空控制台
    private void ClearConsole()
    {
        outputBuffer.Clear();
        consoleText.text = "";
    }

    // 滚动到底部
    private void ScrollToBottom()
    {
        // 延迟一帧确保滚动生效
        StartCoroutine(ScrollCoroutine());
    }

    private System.Collections.IEnumerator ScrollCoroutine()
    {
        yield return null;
        scrollRect.verticalNormalizedPosition = 0;
    }

    // 退出时清理进程
    private void OnDestroy()
    {
        if (powershellProcess != null && !powershellProcess.HasExited)
        {
            powershellProcess.Kill();
            powershellProcess.Dispose();
        }
    }
}