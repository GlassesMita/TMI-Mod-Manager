using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;

public class SceneLoader : MonoBehaviour
{
    public Dropdown sceneDropdown;
    public Button loadButton;

    private void Start()
    {
        // ��ȡ Assets/Scenes �µ����г����ļ�
        string[] scenePaths = GetScenePaths();
        // �������ļ�����ӵ� Dropdown ��
        PopulateDropdown(scenePaths);

        // ���ð�ť�ĵ���¼�
        loadButton.onClick.AddListener(OnLoadButtonClicked);
    }

    // ��ȡ Assets/Scenes �µ����г����ļ�·��
    private string[] GetScenePaths()
    {
        // ��ȡ "Assets/Scenes" �ļ����µ����г����ļ�
        string[] sceneFiles = Directory.GetFiles("Assets/Scenes", "*.unity");
        string[] sceneNames = new string[sceneFiles.Length];

        // ��ȡ�ļ�����ȥ��·������չ����
        for (int i = 0; i < sceneFiles.Length; i++)
        {
            sceneNames[i] = Path.GetFileNameWithoutExtension(sceneFiles[i]);
        }

        return sceneNames;
    }

    // ������������ӵ� Dropdown ��
    private void PopulateDropdown(string[] sceneNames)
    {
        sceneDropdown.ClearOptions(); // �������ѡ��
        sceneDropdown.AddOptions(new System.Collections.Generic.List<string>(sceneNames)); // �����ѡ��
    }

    // ��ť����¼���������ѡ����
    private void OnLoadButtonClicked()
    {
        string selectedScene = sceneDropdown.options[sceneDropdown.value].text;
        SceneManager.LoadScene(selectedScene); // ����ѡ�еĳ���
    }
}
