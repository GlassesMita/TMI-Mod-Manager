using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Diagnostics;

public class InitializationLogger : MonoBehaviour
{
    private static bool initialized = false;

    public void Start()
    {
        // 作为后备，如果尚未由静态入口执行，则在 Start 时执行一次
        if (!initialized)
        {
            PerformInitialization();
        }
    }

    // 确保在播放器创建并在场景加载之前执行
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void RunBeforeSceneLoad()
    {
        PerformInitialization();
    }

    private static void PerformInitialization()
    {
        if (initialized) return;
        initialized = true;

        try
        {
            IniFileReader reader = new IniFileReader(Path.Combine(Application.dataPath, "..", "AppConfig.Schale"));
            string appVersion = reader.GetValue("Config", "Version");

            Logger.Log("*** Initialized Application ***");
            Logger.Log("Application Version: " + appVersion);
            Logger.Log("Build with Unity " + Application.unityVersion);
            Logger.Log("Running From: " + Process.GetCurrentProcess().MainModule.FileName);

    // 检测是否为爆出 CVE-2025-59489 漏洞的 Unity 2021.3.28f1 Mono Non-development x64 播放器版本，通过 SHA-1 和 MD5 进行检验，原始版本的 SHA-1 值为 f533ffe6a197876244aed60fe1c2070def962c73, MD5 值为 3efb0fce3c5c6b33d399172b6d366596
#if UNITY_STANDALONE_WIN
            try
            {
                Logger.Log("\t");
                // 在下面获取 UnityPlayer.dll 的 SHA1 & MD5
                string unityPlayerPath = Path.Combine(Application.dataPath, "..", "UnityPlayer.dll");
                if (File.Exists(unityPlayerPath))
                {
                    using var sha1 = System.Security.Cryptography.SHA1.Create();
                    using var md5 = System.Security.Cryptography.MD5.Create();
                    using var stream = File.OpenRead(unityPlayerPath);
                    var sha1Hash = BitConverter.ToString(sha1.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
                    stream.Position = 0; // 重置流位置以重新计算 MD5
                    var md5Hash = BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();

                    if (sha1Hash == "f533ffe6a197876244aed60fe1c2070def962c73" && md5Hash == "3efb0fce3c5c6b33d399172b6d366596")
                    {
                        Logger.Log(Logger.LogLevel.Warning, " ! Detected vulnerable UnityPlayer.dll version (CVE-2025-59489). Please update to a patched version.");
                        Logger.Log(Logger.LogLevel.Warning, " ! UnityPlayer.dll SHA1: " + sha1Hash);
                        Logger.Log(Logger.LogLevel.Warning, " ! UnityPlayer.dll MD5: " + md5Hash);
                        Logger.Log(Logger.LogLevel.Warning, " You can download the patcher(version 1.2.0) from the link below:");
                        Logger.Log(Logger.LogLevel.Warning, " \thttps://security-patches.unity.com/bc0977e0-21a9-4f6e-9414-4f44b242110a/unity-patcher/UnityApplicationPatcher-1.2.0-Win.zip");
                        Logger.Log("\t");
                    }
                }
                else
                {
#if UNITY_EDITOR
                    Logger.Log("Running in Editor, skipping UnityPlayer.dll hash check.");
#elif UNITY_STANDALONE_WIN
                    Logger.Log(Logger.LogLevel.Warning, "UnityPlayer.dll not found at expected path: " + unityPlayerPath);
#else
                    Logger.Log(Logger.LogLevel.Warning, "Current we cannot support this platform, so skipping UnityPlayer.dll hash check.");
#endif
                }
            }
            catch (Exception ex)
            {
                Logger.Log(Logger.LogLevel.Error, "Error computing UnityPlayer.dll hashes: " + ex.Message);
            }
#endif
#if UNITY_EDITOR
            Logger.Log("--- Running in Editor Environment ---");
#endif
            Logger.Log("\t");
            Logger.Log("========== Hardware Info ==========");
            Logger.Log("CPU: " + SystemInfo.processorType + " (" + SystemInfo.processorCount + " cores)");
            Logger.Log("RAM (excluding the portion reserved for hardware): " + SystemInfo.systemMemorySize / 1024 + "GB");
            Logger.Log("GPU: " + SystemInfo.graphicsDeviceName + " (" + SystemInfo.graphicsMemorySize / 1024 + "GB), Vendor: " + SystemInfo.graphicsDeviceVendor + "");
            Logger.Log("===================================");
            Logger.Log("\t");
            Logger.Log("=========== Device Info ===========");
            Logger.Log("OS: " + SystemInfo.operatingSystem);
            Logger.Log("Device Model: " + SystemInfo.deviceModel);
            Logger.Log("Device Name: " + SystemInfo.deviceName);
            Logger.Log("Device Type: " + SystemInfo.deviceType);
            if (Environment.UserName == SystemInfo.deviceName + "$")
            {
                Logger.Log(Logger.LogLevel.Warning, "Current logged user: " + Environment.UserName + "\n\t\t\t(Note: if username is device name, it may this instance is running under system account.)");
            }
            else
            {
                Logger.Log(Logger.LogLevel.Info, "Current logged user: " + Environment.UserName);
            }
            Logger.Log("===================================");
            Logger.Log("\t");
        }
        catch (Exception ex)
        {
            // 如果初始化过程中出现任何异常，记录错误并继续，不要阻塞加载
            Logger.Log(Logger.LogLevel.Error, "InitializationLogger failed: " + ex.Message);
        }
    }
}