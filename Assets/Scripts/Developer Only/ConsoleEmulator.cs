using UnityEngine;
using UnityEngine.UI;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading;

public class ConsoleEmulator : MonoBehaviour
{
    [SerializeField] private Text consoleOutput;  // 显示输出的Text组件
    [SerializeField] private InputField commandInput;  // 输入命令的InputField
    [SerializeField] private ScrollRect scrollRect;  // 用于自动滚动

    private string currentDirectory;  // 当前工作目录
    private string userName;  // 用户名
    private string computerName;  // 计算机名
    private StringBuilder outputBuilder = new StringBuilder();

    // 命令提示符样式
    private string prompt => $"┌──({userName}@{computerName})-[{currentDirectory}]\n└─PS> ";

    void Start()
    {
        // 初始化信息
        currentDirectory = Environment.CurrentDirectory;
        userName = Environment.UserName;
        computerName = Environment.MachineName;

        // 初始欢迎信息
        AppendOutput("Praying for you 🕯️ O Great Mita 💝\n");
        AppendOutput($"{Environment.OSVersion}\n");
        AppendOutput($"PowerShell {GetPowerShellVersion()}\n\n");
        AppendOutput(prompt);

        // 输入框提交事件
        commandInput.onEndEdit.AddListener(OnCommandSubmitted);
        
        // 自动聚焦输入框
        commandInput.ActivateInputField();
    }

    // 获取PowerShell版本
    private string GetPowerShellVersion()
    {
        try
        {
            using Process process = new Process();
            process.StartInfo.FileName = "pwsh.exe";
            process.StartInfo.Arguments = "$PSVersionTable.PSVersion | Select-Object -ExpandProperty Version";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.CreateNoWindow = true;

            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return output.Trim();
        }
        catch
        {
            return "7.5.3";  // 默认版本
        }
    }

    // 处理提交的命令
    private void OnCommandSubmitted(string command)
    {
        if (string.IsNullOrEmpty(command) || !Input.GetKeyDown(KeyCode.Return))
            return;

        // 添加命令到输出
        AppendOutput(command + "\n");
        
        // 执行命令
        ExecuteCommand(command);
        
        // 清空输入框并重新聚焦
        commandInput.text = "";
        commandInput.ActivateInputField();
    }

    // 执行系统命令
    private void ExecuteCommand(string command)
    {
        // 特殊命令处理（如切换目录）
        if (command.StartsWith("cd ", StringComparison.OrdinalIgnoreCase))
        {
            string path = command.Substring(3).Trim();
            try
            {
                currentDirectory = System.IO.Path.GetFullPath(path);
                AppendOutput(prompt);
            }
            catch (Exception ex)
            {
                AppendOutput($"错误: {ex.Message}\n");
                AppendOutput(prompt);
            }
            return;
        }
        else if (command.Equals("cls", StringComparison.OrdinalIgnoreCase))
        {
            outputBuilder.Clear();
            consoleOutput.text = "";
            AppendOutput(prompt);
            return;
        }

        // 在新线程中执行命令，避免阻塞UI
        Thread thread = new Thread(() =>
        {
            try
            {
                using Process process = new Process();
                process.StartInfo.FileName = "pwsh.exe";
                process.StartInfo.Arguments = $"-Command \"{command}\"";
                process.StartInfo.WorkingDirectory = currentDirectory;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
                process.StartInfo.StandardErrorEncoding = Encoding.UTF8;

                // 输出数据接收事件
                process.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        AppendOutput(e.Data + "\n");
                    }
                };

                // 错误数据接收事件
                process.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        AppendOutput($"错误: {e.Data}\n");
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
            }
            catch (Exception ex)
            {
                AppendOutput($"执行命令时出错: {ex.Message}\n");
            }
            finally
            {
                // 命令执行完成后显示新的提示符
                AppendOutput(prompt);
            }
        });
        
        thread.Start();
    }

    // 添加内容到输出
    private void AppendOutput(string text)
    {
        // 在主线程中更新UI
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            outputBuilder.Append(text);
            consoleOutput.text = outputBuilder.ToString();
            
            // 自动滚动到底部
            scrollRect.verticalNormalizedPosition = 0;
        });
    }
}
