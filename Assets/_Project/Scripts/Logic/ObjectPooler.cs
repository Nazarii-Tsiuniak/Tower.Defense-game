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

        GameObject obj;
        if (poolDictionary[key].Count == 0)
        {
            obj = Instantiate(prefab, position, rotation);
            obj.name = key;
        }
        else
        {
            obj = poolDictionary[key].Dequeue();
            obj.transform.position = position;
            obj.transform.rotation = rotation;
        }

        obj.SetActive(true);
        return obj;
    }

    public void ReturnToPool(GameObject obj)
    {
        obj.SetActive(false);
        if (!poolDictionary.ContainsKey(obj.name))
            poolDictionary.Add(obj.name, new Queue<GameObject>());
        poolDictionary[obj.name].Enqueue(obj);
    }
}