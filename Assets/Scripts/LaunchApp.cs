using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System;

public class LaunchApp : MonoBehaviour
{
    public bool isSteamRelease = true;
    public string steamUrl;
    public string exePath = @"";

    public void LaunchApplication()
    {
        if(isSteamRelease == true)
        {
            Application.OpenURL(steamUrl);
        }
        else
        {
            Process.Start(exePath);
        }
    }
}
