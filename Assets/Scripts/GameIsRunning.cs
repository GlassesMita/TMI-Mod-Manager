using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Linq;
using System.IO;

public class GameIsRunning : MonoBehaviour
{
    public GameObject coverPanel;
    // Update is called once per frame
    void Update()
    {
        Process[] processes = Process.GetProcessesByName("Touhou Mystia Izakaya");
        if (processes.Length > 0)
        {
            coverPanel.gameObject.SetActive(true);
        }
        else
        {
            coverPanel.gameObject.SetActive(false);
        }
    }
}
