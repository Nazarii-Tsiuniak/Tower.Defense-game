using UnityEngine;

public class SceneBuilder : MonoBehaviour
{
    void Start()
    {
        SetupCamera();
        CreateBackground();
        CreateWaypointPath();
        CreateDecorations();
        SetupManagers();
        CreateTowerSpots();
        ApplyEnemyVisuals();
    }

    // ──────── Camera ────────

    void SetupCamera()
    {
        var cam = Camera.main;
        if (cam != null)
        {
            cam.orthographicSize = 6;
            cam.backgroundColor = new Color(0.30f, 0.60f, 0.25f);
        }
    }

    // ──────── Background ────────

    void CreateBackground()
    {
        var bg = new GameObject("Background");
        var sr = bg.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreateGrassBackground(256, 160);
        sr.sortingOrder = -10;
        bg.transform.position = Vector3.zero;
        bg.transform.localScale = new Vector3(1.0f, 1.0f, 1f);
    }

    // ──────── Path ────────

    void CreateWaypointPath()
    {
        var pathObj = new GameObject("WaypointPath");
        pathObj.AddComponent<WaypointPath>();

        Vector3[] positions = new Vector3[]
        {
            new Vector3(-8.5f, 0f, 0f),
            new Vector3(-5f, 3f, 0f),
            new Vector3(-1.5f, -2f, 0f),
            new Vector3(2f, 3f, 0f),
            new Vector3(5.5f, -1f, 0f),
            new Vector3(8.5f, 1.5f, 0f)
        };

        for (int i = 0; i < positions.Length; i++)
        {
            var wp = new GameObject("Waypoint_" + i);
            wp.transform.SetParent(pathObj.transform);
            wp.transform.position = positions[i];
        }

        for (int i = 0; i < positions.Length - 1; i++)
            CreatePathSegment(positions[i], positions[i + 1], i);
    }

    void CreatePathSegment(Vector3 from, Vector3 to, int index)
    {
        Vector3 mid = (from + to) / 2f;
        Vector3 dir = to - from;
        float length = dir.magnitude;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        var segment = new GameObject("PathSegment_" + index);
        var sr = segment.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreatePathTile(64, 18);
        sr.sortingOrder = -8;

        segment.transform.position = mid;
        segment.transform.rotation = Quaternion.Euler(0, 0, angle);
        segment.transform.localScale = new Vector3(length / 1f, 1.2f, 1f);
    }

    // ──────── Decorations (trees, bushes, flowers) ────────

    void CreateDecorations()
    {
        var parent = new GameObject("Decorations");

        // Trees along path edges
        Vector3[] treePositions = new Vector3[]
        {
            new Vector3(-7f, -3f, 0), new Vector3(-4f, -4f, 0),
            new Vector3(0f, 4f, 0),   new Vector3(3f, -4f, 0),
            new Vector3(7f, -3.5f, 0), new Vector3(-6f, 4.5f, 0),
            new Vector3(5f, 4.5f, 0),  new Vector3(8f, 4f, 0),
        };
        foreach (var pos in treePositions)
        {
            var tree = new GameObject("Tree");
            tree.transform.SetParent(parent.transform);
            tree.transform.position = pos;
            var sr = tree.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteGenerator.CreateTree(48);
            sr.sortingOrder = -6;
            float s = Random.Range(0.8f, 1.2f);
            tree.transform.localScale = new Vector3(s, s, 1f);
        }

        // Bushes
        Vector3[] bushPositions = new Vector3[]
        {
            new Vector3(-3.5f, -3f, 0), new Vector3(1f, 4.5f, 0),
            new Vector3(4f, 3.5f, 0),   new Vector3(-7.5f, 2f, 0),
            new Vector3(6f, -2f, 0),     new Vector3(-2f, -4.5f, 0),
        };
        foreach (var pos in bushPositions)
        {
            var bush = new GameObject("Bush");
            bush.transform.SetParent(parent.transform);
            bush.transform.position = pos;
            var sr = bush.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteGenerator.CreateBush(32);
            sr.sortingOrder = -5;
            float s = Random.Range(0.6f, 1.1f);
            bush.transform.localScale = new Vector3(s, s, 1f);
        }

        // Flowers
        Color[] flowerColors = new Color[]
        {
            new Color(1f, 0.4f, 0.4f), new Color(1f, 0.8f, 0.2f),
            new Color(0.8f, 0.4f, 1f), new Color(1f, 0.55f, 0.7f),
        };
        Vector3[] flowerPositions = new Vector3[]
        {
            new Vector3(-6.5f, -1.5f, 0), new Vector3(-1f, -3.8f, 0),
            new Vector3(2.5f, 4.2f, 0),   new Vector3(6.5f, 3f, 0),
            new Vector3(-5f, 4f, 0),       new Vector3(3.5f, -3.5f, 0),
        };
        for (int i = 0; i < flowerPositions.Length; i++)
        {
            var flower = new GameObject("Flower");
            flower.transform.SetParent(parent.transform);
            flower.transform.position = flowerPositions[i];
            var sr = flower.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteGenerator.CreateFlower(20, flowerColors[i % flowerColors.Length]);
            sr.sortingOrder = -4;
            flower.transform.localScale = Vector3.one * 0.5f;
        }
    }

    // ──────── Managers ────────

    void SetupManagers()
    {
        var gmObj = new GameObject("GameManager");
        gmObj.AddComponent<GameManager>();

        var poolObj = new GameObject("ObjectPooler");
        poolObj.AddComponent<ObjectPooler>();

        var spawnerObj = new GameObject("WaveSpawner");
        var spawner = spawnerObj.AddComponent<WaveSpawner>();
        spawner.enemyPrefabs = CreateEnemyTemplates();

        TowerData[] towers = CreateTowerData();

        var uiObj = new GameObject("UIManager");
        var ui = uiObj.AddComponent<GameUIManager>();
        ui.Initialize(towers);
    }

    // ──────── Enemy Templates ────────

    GameObject[] CreateEnemyTemplates()
    {
        var goblin = CreateEnemyTemplate("Enemy_Goblin",
            new Color(0.30f, 0.75f, 0.25f), "goblin", 10, 2f, 5);

        var ghost = CreateEnemyTemplate("Enemy_Ghost",
            new Color(0.50f, 0.70f, 0.95f), "ghost", 12, 5f, 6);

        var orc = CreateEnemyTemplate("Enemy_Orc",
            new Color(0.60f, 0.38f, 0.22f), "orc", 8, 3f, 6);

        return new GameObject[] { goblin, ghost, orc };
    }

    GameObject CreateEnemyTemplate(string name, Color color, string type,
        int health, float speed, int cost)
    {
        var enemy = new GameObject(name);
        enemy.SetActive(false);

        var sr = enemy.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreateCartoonEnemy(48, color, type);
        sr.sortingOrder = 2;

        var data = ScriptableObject.CreateInstance<EnemyData>();
        data.enemyName = name.Replace("Enemy_", "");
        data.health = health;
        data.speed = speed;
        data.cost = cost;

        var movement = enemy.AddComponent<EnemyMovement>();
        movement.data = data;

        var enemyHealth = enemy.AddComponent<EnemyHealth>();
        enemyHealth.Initialize(health, cost);

        return enemy;
    }

    // ──────── Tower Data ────────

    TowerData[] CreateTowerData()
    {
        var archer = ScriptableObject.CreateInstance<TowerData>();
        archer.towerName = "Archer";
        archer.cost = 50;
        archer.damage = 2;
        archer.range = 3f;
        archer.fireRate = 1.5f;

        var cannon = ScriptableObject.CreateInstance<TowerData>();
        cannon.towerName = "Cannon";
        cannon.cost = 80;
        cannon.damage = 5;
        cannon.range = 2.5f;
        cannon.fireRate = 0.5f;

        var mage = ScriptableObject.CreateInstance<TowerData>();
        mage.towerName = "Mage";
        mage.cost = 100;
        mage.damage = 3;
        mage.range = 4f;
        mage.fireRate = 1f;

        return new TowerData[] { archer, cannon, mage };
    }

    // ──────── Tower Spots (stone platforms) ────────

    void CreateTowerSpots()
    {
        Vector3[] spotPositions = new Vector3[]
        {
            new Vector3(-3f, 1.5f, 0f),
            new Vector3(-3f, -1f, 0f),
            new Vector3(0.5f, 1.5f, 0f),
            new Vector3(0.5f, -3f, 0f),
            new Vector3(3.5f, 2f, 0f),
            new Vector3(3.5f, -2.5f, 0f),
            new Vector3(6.5f, 0f, 0f),
            new Vector3(6.5f, 3f, 0f),
        };

        var spotsParent = new GameObject("TowerSpots");

        for (int i = 0; i < spotPositions.Length; i++)
        {
            var spot = new GameObject("TowerSpot_" + i);
            spot.transform.SetParent(spotsParent.transform);
            spot.transform.position = spotPositions[i];
            spot.transform.localScale = Vector3.one * 0.9f;

            var sr = spot.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteGenerator.CreateStonePlatform(48);
            sr.sortingOrder = -3;

            spot.AddComponent<TowerPlacement>();
        }
    }

    // ──────── Enemy Visuals Patch ────────

    void ApplyEnemyVisuals()
    {
        var enemies = FindObjectsByType<EnemyMovement>(FindObjectsSortMode.None);
        foreach (var enemy in enemies)
        {
            var sr = enemy.GetComponentInParent<SpriteRenderer>();
            if (sr != null && sr.sprite == null)
            {
                string type = "goblin";
                Color color = Color.white;
                if (enemy.data != null)
                {
                    string n = enemy.data.enemyName.ToLower();
                    if (n.Contains("ghost")) { color = new Color(0.50f, 0.70f, 0.95f); type = "ghost"; }
                    else if (n.Contains("goblin")) { color = new Color(0.30f, 0.75f, 0.25f); type = "goblin"; }
                    else if (n.Contains("orc")) { color = new Color(0.60f, 0.38f, 0.22f); type = "orc"; }
                }
                sr.sprite = SpriteGenerator.CreateCartoonEnemy(48, color, type);
            }
        }
    }
}
