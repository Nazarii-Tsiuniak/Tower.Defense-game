using UnityEngine;

public class TowerBehaviour : MonoBehaviour
{
    public TowerData data;
    private float fireCountdown = 0f;
    private Transform target;
    private float searchTimer = 0f;

    void Update()
    {
        if (data == null) return;

        searchTimer -= Time.deltaTime;
        if (searchTimer <= 0f)
        {
            FindTarget();
            searchTimer = 0.25f;
        }

        if (target == null) return;

        if (fireCountdown <= 0f)
        {
            Shoot();
            fireCountdown = 1f / data.fireRate;
        }

        fireCountdown -= Time.deltaTime;
    }

    void FindTarget()
    {
        float shortestDist = Mathf.Infinity;
        Transform nearest = null;

        EnemyMovement[] enemies = FindObjectsByType<EnemyMovement>(FindObjectsSortMode.None);
        foreach (var enemy in enemies)
        {
            if (!enemy.gameObject.activeInHierarchy) continue;

            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist < shortestDist && dist <= data.range)
            {
                shortestDist = dist;
                nearest = enemy.transform;
            }
        }

        target = nearest;
    }

    void Shoot()
    {
        if (target == null) return;

        var projObj = new GameObject("Projectile");
        projObj.transform.position = transform.position;

        var proj = projObj.AddComponent<Projectile>();
        proj.Initialize(target, data.damage);

        var sr = projObj.AddComponent<SpriteRenderer>();
        Color projColor;
        if (data.towerName != null && data.towerName.Contains("Mage"))
            projColor = new Color(0.7f, 0.3f, 1f);
        else if (data.towerName != null && data.towerName.Contains("Cannon"))
            projColor = new Color(0.9f, 0.4f, 0.1f);
        else
            projColor = new Color(1f, 0.9f, 0.3f);

        sr.sprite = SpriteGenerator.CreateCartoonProjectile(12, projColor);
        sr.sortingOrder = 3;
        projObj.transform.localScale = Vector3.one * 0.35f;
    }

    void OnDrawGizmosSelected()
    {
        if (data != null)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, data.range);
        }
    }
}
