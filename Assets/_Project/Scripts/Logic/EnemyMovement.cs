using System.Collections.Generic;
using UnityEngine;

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
        // Private non-serialized fields are lost after Instantiate — re-resolve from children
        var fillT = transform.Find("HealthFill");
        var bgT   = transform.Find("HealthBG");
        if (fillT != null) healthBarFill = fillT;
        if (bgT   != null) healthBarBG   = bgT;
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
        healthBarFill.localPosition = new Vector3(-0.46f + 0.46f * ratio, 0.55f, 0f);

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
        SpawnDeathEffect();
        if (GameManager.Instance != null)
            GameManager.Instance.EnemyKilled(rewardGold);
        if (WaveSpawner.Instance != null)
            WaveSpawner.Instance.EnemyEliminated();
        if (ObjectPooler.Instance != null)
            ObjectPooler.Instance.ReturnToPool(gameObject);
    }

    void SpawnDeathEffect()
    {
        switch (enemyType)
        {
            case "Goblin": SpawnGoblinDeath(); break;
            case "Orc":    SpawnOrcDeath();    break;
            case "Ghost":  SpawnGhostDeath();  break;
            default:       SpawnGenericDeath(Color.red, 6); break;
        }
    }

    // ── Goblin: green + gold coin burst ───────────────────────────────────
    void SpawnGoblinDeath()
    {
        SpawnParticleBurst(transform.position, new Color(0.30f, 0.72f, 0.22f), 8, 3.5f, 0.55f);
        SpawnParticleBurst(transform.position + Vector3.up * 0.1f,
            new Color(1.0f, 0.82f, 0.18f), 5, 2.5f, 0.45f); // gold coins
        SpawnFloatingText(transform.position, "💀", 0.5f);
    }

    // ── Orc: brown + metal sparks + screen shake feel ─────────────────────
    void SpawnOrcDeath()
    {
        SpawnParticleBurst(transform.position, new Color(0.42f, 0.55f, 0.28f), 12, 4.5f, 0.70f);
        SpawnParticleBurst(transform.position, new Color(0.65f, 0.65f, 0.70f), 8, 3.0f, 0.40f); // metal
        SpawnParticleBurst(transform.position, new Color(0.80f, 0.18f, 0.10f), 5, 2.5f, 0.35f); // blood
        SpawnFloatingText(transform.position, "☠", 0.6f);
    }

    // ── Ghost: dissolve into blue sparkle wisps ───────────────────────────
    void SpawnGhostDeath()
    {
        SpawnParticleBurst(transform.position, new Color(0.78f, 0.82f, 0.95f), 6, 2.5f, 0.80f);
        SpawnParticleBurst(transform.position, new Color(0.25f, 0.45f, 1.00f), 10, 4.0f, 0.90f); // blue wisps
        // Extra upward wisps to simulate dissolving
        for (int i = 0; i < 4; i++)
            SpawnSingleParticle(transform.position + Vector3.right * (i - 2) * 0.3f,
                new Color(0.55f, 0.70f, 1.00f, 0.8f),
                new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(1.5f, 3.0f)),
                0.7f + Random.value * 0.5f);
        SpawnFloatingText(transform.position, "👻", 0.6f);
    }

    void SpawnGenericDeath(Color col, int count)
    {
        SpawnParticleBurst(transform.position, col, count, 3f, 0.5f);
    }

    // ── Particle helpers ─────────────────────────────────────────────────
    static void SpawnParticleBurst(Vector3 pos, Color color, int count, float speed, float life)
    {
        for (int i = 0; i < count; i++)
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float mag   = Random.Range(0.3f, 1f) * speed;
            var vel = new Vector2(Mathf.Cos(angle) * mag, Mathf.Sin(angle) * mag);
            SpawnSingleParticle(pos, color, vel, life * Random.Range(0.7f, 1.3f));
        }
    }

    static void SpawnSingleParticle(Vector3 pos, Color color, Vector2 velocity, float life)
    {
        var go = new GameObject("DeathParticle");
        go.transform.position = pos;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreateParticleSprite(color, 6);
        sr.sortingOrder = 15;

        go.AddComponent<DeathParticle>().Init(velocity, life, color);
    }

    static void SpawnFloatingText(Vector3 pos, string text, float life)
    {
        var go = new GameObject("DeathText");
        go.transform.position = pos + Vector3.up * 0.3f;

        // We use a world-space TextMesh for a quick floating label
        var tm = go.AddComponent<TextMesh>();
        tm.text = text;
        tm.fontSize = 28;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.characterSize = 0.06f;
        go.GetComponent<MeshRenderer>().sortingOrder = 16;

        go.AddComponent<FloatingText>().Init(life);
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
