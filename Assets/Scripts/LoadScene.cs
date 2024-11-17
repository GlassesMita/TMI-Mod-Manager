using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading;

public class LoadScene : MonoBehaviour
{
    public int sceneFileID;
    public int WaitSecond;
    public void LoadSceneByFile()
    {
        SceneManager.LoadScene(sceneFileID);
    }

    public void LoadSceneByWaitTime()
    {
        Thread.Sleep(WaitSecond * 1000);
        SceneManager.LoadScene(sceneFileID);
    }

    private void LoadSceneAuto()
    {
        Thread.Sleep(WaitSecond * 1000);
        SceneManager.LoadScene(sceneFileID);
    }
}
