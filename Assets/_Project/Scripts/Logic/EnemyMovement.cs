using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public EnemyData data;
    private int wavePointIndex = 0;

    void OnEnable()
    {
        wavePointIndex = 0;
    }

    void Update()
    {
        if (data == null) return;
        if (WaypointPath.Points == null || wavePointIndex >= WaypointPath.Points.Length) return;

        Transform target = WaypointPath.Points[wavePointIndex];
        Transform root = transform.root;

        root.position = Vector3.MoveTowards(
            root.position, target.position, data.speed * Time.deltaTime);

        if (Vector3.Distance(root.position, target.position) <= 0.1f)
        {
            wavePointIndex++;
        }

        if (wavePointIndex >= WaypointPath.Points.Length)
        {
            if (GameManager.Instance != null)
                GameManager.Instance.LoseLife();

            ObjectPooler.Instance.ReturnToPool(root.gameObject);
        }
    }
}