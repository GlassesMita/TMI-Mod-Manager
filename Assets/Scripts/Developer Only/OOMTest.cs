using UnityEngine;

public class OOMTest : MonoBehaviour
{
    public int numberOfObjects = 100000; // 创建对象的数量
    public Vector3 objectSize = new Vector3(0.5f, 0.5f, 0.5f); // 对象大小
    public GameObject objectPrefab; // 你要使用的对象预制件

    private void Start()
    {
        if (objectPrefab == null)
        {
            Debug.LogError("Object prefab not assigned!");
            return;
        }

        // 启动创建对象的过程
        CreateObjects();
    }

    // 创建大量的 GameObject
    private void CreateObjects()
    {
        for (int i = 0; i < numberOfObjects; i++)
        {
            // 通过预制件创建新对象
            GameObject newObject = Instantiate(objectPrefab);

            // 随机化对象的位置
            newObject.transform.position = new Vector3(Random.Range(-50f, 50f), Random.Range(-50f, 50f), Random.Range(-50f, 50f));
            newObject.transform.localScale = objectSize;
        }

        Debug.Log($"Created {numberOfObjects} objects!");
    }
}
