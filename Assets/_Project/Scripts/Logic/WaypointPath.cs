using UnityEngine;

public class WaypointPath : MonoBehaviour
{
    // Статичний масив, до якого звертаються компоненти під час руху
    public static Transform[] Points;

    void Awake()
    {
        Points = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            Points[i] = transform.GetChild(i);
        }
    }
}