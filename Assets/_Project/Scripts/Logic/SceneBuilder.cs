using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

public class SceneBuilder : MonoBehaviour
{
    void Awake()
    {
        EnsureEventSystem();
        EnsureObjectPooler();
        CreateWaypointPath();
    }

    void EnsureEventSystem()
    {
        if (FindAnyObjectByType<EventSystem>() != null) return;

        var go = new GameObject("EventSystem");
        go.AddComponent<EventSystem>();
        go.AddComponent<InputSystemUIInputModule>();
    }

    void EnsureObjectPooler()
    {
        if (ObjectPooler.Instance != null) return;

        var go = new GameObject("ObjectPooler");
        go.AddComponent<ObjectPooler>();
    }

    void CreateWaypointPath()
    {
        if (WaypointPath.Points != null && WaypointPath.Points.Length > 0) return;

        var pathGO = new GameObject("WaypointPath");

        Vector3[] waypoints = new Vector3[]
        {
            new Vector3(-8f, -3f, 0f),
            new Vector3(-4f, -3f, 0f),
            new Vector3(-4f,  1f, 0f),
            new Vector3( 0f,  1f, 0f),
            new Vector3( 0f, -2f, 0f),
            new Vector3( 4f, -2f, 0f),
            new Vector3( 4f,  3f, 0f),
            new Vector3( 8f,  3f, 0f)
        };

        for (int i = 0; i < waypoints.Length; i++)
        {
            var wp = new GameObject("WP_" + i);
            wp.transform.SetParent(pathGO.transform);
            wp.transform.position = waypoints[i];
        }

        pathGO.AddComponent<WaypointPath>();
    }
}
