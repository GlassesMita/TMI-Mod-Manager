using UnityEngine;
using System.IO;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq; // ����� System.Linq �����ռ���ʹ�� Where ����

public class FileLoader : MonoBehaviour
{
    public string directoryPath = "";
    public GameObject statusTextPrefab;
    public Text jsonContentText;
    public UIManager uiManager;
    public CurrentSceneName currentSceneName;

    void Start()
    {
        directoryPath = Application.dataPath + directoryPath;
        string[] jsonFiles = Directory.GetFiles(directoryPath, "*.json");

        // ���˵� Data Ŀ¼�µ��ļ�
        jsonFiles = jsonFiles.Where(file => !file.Contains("./" + Application.productName +"/")).ToArray();

        if (uiManager != null)
        {
            uiManager.UpdateUIWithFiles(jsonFiles);
        }
        else
        {
            Debug.LogError("UIManager ������δ����");
        }

        // ����Ƿ��ҵ� JSON �ļ�
        if (jsonFiles.Length == 0)
        {
            // ����Ԥ�Ƽ�
            GameObject statusTextObject = Instantiate(statusTextPrefab);
            // ��ȡԤ�Ƽ��ϵ� Text ���
            Text statusText = statusTextObject.GetComponent<Text>();
            if (statusText != null)
            {
                // ���� Text ��ֵ
                statusText.text = "�տ���Ҳ";
                // ��� Text �� Canvas �£������������������λ��
                statusText.transform.SetParent(uiManager.fileListContainer, false);

            }
            else
            {
                Debug.LogError("Ԥ�Ƽ���δ���ҵ� Text ���");
            }
        }
    }

    public void ReLoad()
    {
        foreach (Transform child in uiManager.fileListContainer)
        {
            Destroy(child.gameObject);
        }
        jsonContentText.text = "";
        SceneManager.LoadScene(currentSceneName.sceneName);
    }
}