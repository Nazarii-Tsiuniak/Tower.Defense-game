using UnityEngine;

public class Projectile : MonoBehaviour
{
    public EnemyMovement target;
    public Vector3 targetPosition;
    public int damage;
    public float speed = 8f;
    public AttackType attackType;
    public float slowAmount;
    public float slowDuration;
    public float aoeRadius;

    private bool hasHit;

    void Update()
    {
        if (hasHit)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 destination;
        if (target != null && target.gameObject.activeInHierarchy)
            destination = target.transform.position;
        else
            destination = targetPosition;

        Vector3 direction = destination - transform.position;
        float dist = direction.magnitude;
        float step = speed * Time.deltaTime;

        if (dist <= step)
        {
            transform.position = destination;
            Hit();
        }
        else
        {
            transform.position += direction.normalized * step;
        }
    }

    void Hit()
    {
        hasHit = true;

        switch (attackType)
        {
            case AttackType.Single:
                if (target != null && target.gameObject.activeInHierarchy)
                    target.TakeDamage(damage);
                break;

            case AttackType.AoE:
                foreach (var enemy in EnemyMovement.ActiveEnemies.ToArray())
                {
                    if (enemy == null || !enemy.gameObject.activeInHierarchy) continue;
                    float dist = Vector3.Distance(transform.position, enemy.transform.position);
                    if (dist <= aoeRadius)
                        enemy.TakeDamage(damage);
                }
                break;

            case AttackType.Slow:
                if (target != null && target.gameObject.activeInHierarchy)
                {
                    target.TakeDamage(damage);
                    target.ApplySlow(slowAmount, slowDuration);
                }
                break;
        }

        Destroy(gameObject);
    }
}
