using UnityEngine;

public class TowerController : MonoBehaviour
{
    public string towerName;
    public float damage;
    public float range;
    public float fireRate;
    public AttackType attackType;
    public float slowAmount;
    public float slowDuration;
    public float aoeRadius;

    private float fireCooldown;
    private EnemyMovement currentTarget;

    void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.State != GameState.Battle)
            return;

        fireCooldown -= Time.deltaTime;

        currentTarget = FindTarget();
        if (currentTarget != null && fireCooldown <= 0f)
        {
            Shoot(currentTarget);
            fireCooldown = 1f / fireRate;
        }
    }

    EnemyMovement FindTarget()
    {
        EnemyMovement best = null;
        float bestProgress = -1f;

        foreach (var enemy in EnemyMovement.ActiveEnemies)
        {
            if (enemy == null || !enemy.gameObject.activeInHierarchy) continue;
            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist <= range && enemy.PathProgress > bestProgress)
            {
                bestProgress = enemy.PathProgress;
                best = enemy;
            }
        }

        return best;
    }

    void Shoot(EnemyMovement target)
    {
        Color projColor;
        switch (towerName)
        {
            case "Archer":  projColor = new Color(0.8f, 0.7f, 0.2f); break;
            case "Mage":    projColor = new Color(0.8f, 0.2f, 0.9f); break;
            case "Freezer": projColor = new Color(0.3f, 0.7f, 1.0f); break;
            case "Cannon":  projColor = new Color(0.4f, 0.4f, 0.4f); break;
            default:        projColor = Color.white; break;
        }

        var projGO = new GameObject("Projectile_" + towerName);
        projGO.transform.position = transform.position;

        var sr = projGO.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreateProjectileSprite(projColor);
        sr.sortingOrder = 15;

        var proj = projGO.AddComponent<Projectile>();
        proj.target = target;
        proj.targetPosition = target.transform.position;
        proj.damage = Mathf.RoundToInt(damage);
        proj.speed = 8f;
        proj.attackType = attackType;
        proj.slowAmount = slowAmount;
        proj.slowDuration = slowDuration;
        proj.aoeRadius = aoeRadius;
    }
}
