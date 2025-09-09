using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
public class QuitApp : MonoBehaviour
{
    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        Debug.Log("Quitting Play Mode - This is not Playback Engine environment.");
#else
        Process.GetCurrentProcess().Kill();
#endif
    }

    public void 退出应用()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        Debug.Log("Quitting Play Mode - This is not Playback Engine environment.");
#else
        Process.GetCurrentProcess().Kill();
#endif
    }
}
