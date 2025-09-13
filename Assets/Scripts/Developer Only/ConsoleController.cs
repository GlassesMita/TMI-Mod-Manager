using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConsoleController : MonoBehaviour
{
    public GameObject consoleWindow;

    public Text consoleTextArea;

    public void ToggleConsole()
    {
        if (consoleWindow.activeInHierarchy == false)
        {
            consoleWindow.SetActive(true);
        }
        else
        {
            consoleWindow.SetActive(false);
        }
    }

    public void ClearConsole()
    {
        consoleTextArea.text = "";
    }
}
