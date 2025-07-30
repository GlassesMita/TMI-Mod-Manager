using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
public class QuitApp : MonoBehaviour
{
    public void Quit()
    {
        Process.GetCurrentProcess().Kill();
    }

    public void 退出应用()
    {
        Process.GetCurrentProcess().Kill();
    }
}
