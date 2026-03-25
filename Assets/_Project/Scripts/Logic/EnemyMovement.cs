using UnityEngine;
using System.Collections.Generic;

public class EnemyMovement : MonoBehaviour
{
    public static List<EnemyMovement> ActiveEnemies = new List<EnemyMovement>();

    public int maxHP;
    public int currentHP;
    public float moveSpeed;
    public float baseSpeed;
    public bool immuneToSlow;
    public int rewardGold;
    public string enemyType;

    public float PathProgress { get; private set; }

    private int waypointIndex;
    private bool isDead;

    // Slow effect
    private float slowTimer;
    private float slowMultiplier = 1f;

    // Health bar
    private Transform healthBarBG;
    private Transform healthBarFill;
    private float healthBarWidth;

    void OnEnable()
    {
        ActiveEnemies.Add(this);
    }

    void OnDisable()
    {
        ActiveEnemies.Remove(this);
    }

    public void ResetEnemy()
    {
        waypointIndex = 0;
        isDead = false;
        currentHP = maxHP;
        PathProgress = 0f;
        slowTimer = 0f;
        slowMultiplier = 1f;
        moveSpeed = baseSpeed;
        UpdateHealthBar();
    }

    public void SetupHealthBar(Sprite bgSprite, Sprite fillSprite)
    {
        // Background
        var bgGO = new GameObject("HealthBG");
        bgGO.transform.SetParent(transform);
        bgGO.transform.localPosition = new Vector3(0, 0.55f, 0);
        var bgSR = bgGO.AddComponent<SpriteRenderer>();
        bgSR.sprite = bgSprite;
        bgSR.sortingOrder = 12;
        healthBarBG = bgGO.transform;

        // Fill
        var fillGO = new GameObject("HealthFill");
        fillGO.transform.SetParent(transform);
        fillGO.transform.localPosition = new Vector3(-0.46f, 0.55f, 0);
        var fillSR = fillGO.AddComponent<SpriteRenderer>();
        fillSR.sprite = fillSprite;
        fillSR.sortingOrder = 13;
        healthBarFill = fillGO.transform;
        healthBarWidth = 0.92f;
    }

    void Update()
    {
        if (isDead) return;
        if (WaypointPath.Points == null || waypointIndex >= WaypointPath.Points.Length) return;

        // Process slow
        if (slowTimer > 0)
        {
            slowTimer -= Time.deltaTime;
            moveSpeed = baseSpeed * slowMultiplier;
            if (slowTimer <= 0)
            {
                moveSpeed = baseSpeed;
                slowMultiplier = 1f;
            }
        }

        Transform target = WaypointPath.Points[waypointIndex];
        Vector3 direction = target.position - transform.position;
        float dist = direction.magnitude;
        float step = moveSpeed * Time.deltaTime;

        if (dist <= step)
        {
            transform.position = target.position;
            PathProgress += dist;
            waypointIndex++;

            if (waypointIndex >= WaypointPath.Points.Length)
            {
                ReachBase();
                return;
            }
        }
        else
        {
            transform.position += direction.normalized * step;
            PathProgress += step;
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;
        currentHP -= damage;
        UpdateHealthBar();

        if (currentHP <= 0)
        {
            Die();
        }
    }

    public void ApplySlow(float multiplier, float duration)
    {
        if (immuneToSlow) return;
        slowMultiplier = multiplier;
        slowTimer = duration;
        moveSpeed = baseSpeed * slowMultiplier;
    }

    void UpdateHealthBar()
    {
        if (healthBarFill == null) return;
        float ratio = Mathf.Clamp01((float)currentHP / maxHP);
        healthBarFill.localScale = new Vector3(ratio, 1f, 1f);

        // Color: green → yellow → red
        var fillSR = healthBarFill.GetComponent<SpriteRenderer>();
        if (fillSR != null)
        {
            if (ratio > 0.5f)
                fillSR.color = Color.Lerp(Color.yellow, Color.green, (ratio - 0.5f) * 2f);
            else
                fillSR.color = Color.Lerp(Color.red, Color.yellow, ratio * 2f);
        }
    }

    void Die()
    {
        isDead = true;
        if (GameManager.Instance != null)
            GameManager.Instance.EnemyKilled(rewardGold);
        if (WaveSpawner.Instance != null)
            WaveSpawner.Instance.EnemyEliminated();
        if (ObjectPooler.Instance != null)
            ObjectPooler.Instance.ReturnToPool(gameObject);
    }

    void ReachBase()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.EnemyReachedBase();
        if (WaveSpawner.Instance != null)
            WaveSpawner.Instance.EnemyEliminated();
        if (ObjectPooler.Instance != null)
            ObjectPooler.Instance.ReturnToPool(gameObject);
    }
}