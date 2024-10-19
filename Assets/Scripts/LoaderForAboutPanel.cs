using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoaderForAboutPanel : MonoBehaviour
{
    public GameObject PanelContainer;
    public void EasterEggActive()
    {
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            PanelContainer.SetActive(true);
        }
    }

    public void EasterEggInactive()
    {
        PanelContainer.SetActive(false);
    }
}
