using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public EnemyData data; // ѕризначаЇтьс€ в ≥нспектор≥ префаба
    private int wavePointIndex = 0;

    void Update()
    {
        // ѕерев≥рка, чи ≥н≥ц≥ал≥зовано шл€х
        if (WaypointPath.Points == null || wavePointIndex >= WaypointPath.Points.Length) return;

        Transform target = WaypointPath.Points[wavePointIndex];

        // –ух зг≥дно з “« (Vector3.MoveTowards)
        transform.position = Vector3.MoveTowards(transform.position, target.position, data.speed * Time.deltaTime);

        // якщо п≥д≥йшли близько до точки - перемикаЇмо на наступну
        if (Vector3.Distance(transform.position, target.position) <= 0.1f)
        {
            wavePointIndex++;
        }

        // якщо шл€х зак≥нчено - повертаЇмо в пул (база отримала шкоду)
        if (wavePointIndex >= WaypointPath.Points.Length)
        {
            ObjectPooler.Instance.ReturnToPool(gameObject);
        }
    }
}