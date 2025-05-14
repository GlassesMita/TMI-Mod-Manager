using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Profiling;

public class ResourceMonitor : MonoBehaviour
{
    public Text memoryUsageText;     // 显示内存使用的文本
    public Text cpuUsageText;        // 显示CPU使用率的文本（间接）
    public Text fpsText;             // 显示FPS的文本

    private void Update()
    {
        // 获取并显示内存信息
        DisplayMemoryUsage();
        
        // 显示CPU信息（通过FPS间接反映）
        DisplayCPUUsage();

        // 显示帧率
        DisplayFPS();
    }

    // 显示内存占用
    private void DisplayMemoryUsage()
    {
        long totalMemory = Profiler.GetTotalAllocatedMemoryLong(); // 获取已分配的内存
        long usedMemory = Profiler.GetTotalReservedMemoryLong();  // 获取已保留的内存
        long systemMemory = SystemInfo.systemMemorySize * 1024 * 1024; // 系统总内存

        // 更新 UI 上的内存文本
        memoryUsageText.text = $"Used Memory: {FormatMemory(totalMemory)}\n" +
                               $"Reserved Memory: {FormatMemory(usedMemory)}\n" +
                               $"System Memory: {FormatMemory(systemMemory)}";
    }

    // 显示 CPU 使用率（通过间接方式）
    private void DisplayCPUUsage()
    {
        // 通过间接方法（帧率）估算 CPU 使用情况
        // 较低的帧率意味着 CPU 使用较高
        float cpuUsage = (1.0f / Time.deltaTime) / 60.0f; // 通过帧率反推

        cpuUsageText.text = $"CPU Usage (approx): {cpuUsage * 100f}%";
    }

    // 显示 FPS
    private void DisplayFPS()
    {
        float fps = 1.0f / Time.deltaTime;  // 每帧时间倒数获取FPS
        fpsText.text = $"FPS: {fps:F2}";
    }

    // 格式化内存数据为可读的格式
    private string FormatMemory(long bytes)
    {
        if (bytes >= 1024 * 1024 * 1024)
        {
            return (bytes / (1024f * 1024f * 1024f)).ToString("F2") + " GB";
        }
        else if (bytes >= 1024 * 1024)
        {
            return (bytes / (1024f * 1024f)).ToString("F2") + " MB";
        }
        else if (bytes >= 1024)
        {
            return (bytes / 1024f).ToString("F2") + " KB";
        }
        else
        {
            return bytes + " B";
        }
    }
}
