using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Instance;
    private Dictionary<string, Queue<GameObject>> poolDictionary = new Dictionary<string, Queue<GameObject>>();

    void Awake()
    {
        Instance = this;
    }

    public GameObject SpawnFromPool(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        string key = prefab.name;
        if (!poolDictionary.ContainsKey(key))
            poolDictionary.Add(key, new Queue<GameObject>());

        if (poolDictionary[key].Count == 0)
        {
            GameObject obj = Instantiate(prefab, position, rotation);
            obj.name = key;
            obj.SetActive(true);
            return obj;
        }

        GameObject objectToSpawn = poolDictionary[key].Dequeue();
        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;
        return objectToSpawn;
    }

    public void ReturnToPool(GameObject obj)
    {
        obj.SetActive(false);
        poolDictionary[obj.name].Enqueue(obj);
    }
}