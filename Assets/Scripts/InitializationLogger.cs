using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Diagnostics;

public class InitializationLogger : MonoBehaviour
{
    public void Start()
    {
        IniFileReader reader = new IniFileReader(Path.Combine(Application.dataPath, "..", "AppConfig.ini"));
        string appVersion = reader.GetValue("Config", "Version");

        Logger.Log("*** Initialized Application ***");
        Logger.Log("Application Version: " + appVersion);
        Logger.Log("Build with Unity " + Application.unityVersion);
        Logger.Log("Running From: " + Process.GetCurrentProcess().MainModule.FileName);
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
        Logger.Log("Current logged user: " + Environment.UserName);
        Logger.Log("===================================");
        Logger.Log("\t");
    }
}