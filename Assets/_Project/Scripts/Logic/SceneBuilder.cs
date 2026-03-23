using UnityEngine;

public class SceneBuilder : MonoBehaviour
{
    void Start()
    {
        SetupCamera();
        CreateBackground();
        CreateWaypointPath();
        SetupManagers();
        CreateTowerSpots();
        ApplyEnemyVisuals();
    }

    void SetupCamera()
    {
        var cam = Camera.main;
        if (cam != null)
        {
            cam.orthographicSize = 6;
            cam.backgroundColor = new Color(0.18f, 0.32f, 0.18f);
        }
    }

    void CreateBackground()
    {
        // Grass background
        var bg = new GameObject("Background");
        var sr = bg.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreateRect(128, 80, new Color(0.28f, 0.56f, 0.28f));
        sr.sortingOrder = -10;
        bg.transform.position = Vector3.zero;
        bg.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
    }

    void CreateWaypointPath()
    {
        var pathObj = new GameObject("WaypointPath");
        pathObj.AddComponent<WaypointPath>();

        // Define a winding path across the screen
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

        // Draw path segments
        for (int i = 0; i < positions.Length - 1; i++)
        {
            CreatePathSegment(positions[i], positions[i + 1], i);
        }
    }

    void CreatePathSegment(Vector3 from, Vector3 to, int index)
    {
        Vector3 mid = (from + to) / 2f;
        Vector3 dir = to - from;
        float length = dir.magnitude;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        var segment = new GameObject("PathSegment_" + index);
        var sr = segment.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreateRect(64, 12, new Color(0.55f, 0.38f, 0.22f));
        sr.sortingOrder = -8;

        segment.transform.position = mid;
        segment.transform.rotation = Quaternion.Euler(0, 0, angle);
        segment.transform.localScale = new Vector3(length / 1f, 0.8f, 1f);
    }

    void SetupManagers()
    {
        // GameManager
        var gmObj = new GameObject("GameManager");
        gmObj.AddComponent<GameManager>();

        // ObjectPooler
        var poolObj = new GameObject("ObjectPooler");
        poolObj.AddComponent<ObjectPooler>();

        // WaveSpawner (needs enemy prefab references - these are set up in SceneBuilder)
        var spawnerObj = new GameObject("WaveSpawner");
        var spawner = spawnerObj.AddComponent<WaveSpawner>();

        // We create simple enemy templates since original prefabs may lack sprites
        spawner.enemyPrefabs = CreateEnemyTemplates();

        // Create tower data assets at runtime
        TowerData[] towers = CreateTowerData();

        // UI Manager
        var uiObj = new GameObject("UIManager");
        var ui = uiObj.AddComponent<GameUIManager>();
        ui.Initialize(towers);
    }

    GameObject[] CreateEnemyTemplates()
    {
        // Goblin - green, small, slow
        var goblin = CreateEnemyTemplate("Enemy_Goblin",
            new Color(0.2f, 0.7f, 0.2f), 10, 2f, 5);

        // Ghost - blue, transparent, fast
        var ghost = CreateEnemyTemplate("Enemy_Ghost",
            new Color(0.4f, 0.6f, 0.95f), 12, 5f, 6);

        // Orc - brown, large, medium
        var orc = CreateEnemyTemplate("Enemy_Orc",
            new Color(0.55f, 0.35f, 0.2f), 8, 3f, 6);

        return new GameObject[] { goblin, ghost, orc };
    }

    GameObject CreateEnemyTemplate(string name, Color color, int health, float speed, int cost)
    {
        var enemy = new GameObject(name);
        enemy.SetActive(false);

        // Visual
        var sr = enemy.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreateCircle(16, color);
        sr.sortingOrder = 2;

        // EnemyData (runtime ScriptableObject)
        var data = ScriptableObject.CreateInstance<EnemyData>();
        data.enemyName = name.Replace("Enemy_", "");
        data.health = health;
        data.speed = speed;
        data.cost = cost;

        // Movement script on root
        var movement = enemy.AddComponent<EnemyMovement>();
        movement.data = data;

        // Health component
        var enemyHealth = enemy.AddComponent<EnemyHealth>();
        enemyHealth.Initialize(health, cost);

        // Name label
        CreateNameLabel(enemy.transform, data.enemyName, color);

        return enemy;
    }

    void CreateNameLabel(Transform parent, string labelText, Color color)
    {
        var labelObj = new GameObject("Label");
        labelObj.transform.SetParent(parent);
        labelObj.transform.localPosition = new Vector3(0, -0.6f, 0);

        var canvas = labelObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 10;

        var rect = labelObj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(2f, 0.4f);
        rect.localScale = Vector3.one * 0.02f;

        var textObj = new GameObject("Text");
        textObj.transform.SetParent(labelObj.transform, false);
        var text = textObj.AddComponent<UnityEngine.UI.Text>();
        text.text = labelText;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 14;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;

        var textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
    }

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

    void CreateTowerSpots()
    {
        // Place tower spots alongside the path
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
            spot.transform.localScale = Vector3.one * 0.8f;

            var sr = spot.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteGenerator.CreateRect(32, 32, new Color(0.9f, 0.85f, 0.3f, 0.4f));
            sr.sortingOrder = -5;

            spot.AddComponent<TowerPlacement>();
        }
    }

    void ApplyEnemyVisuals()
    {
        // Apply runtime-generated sprites to any existing enemy prefabs
        // that might have missing sprite references
        var enemies = FindObjectsByType<EnemyMovement>(FindObjectsSortMode.None);
        foreach (var enemy in enemies)
        {
            var sr = enemy.GetComponentInParent<SpriteRenderer>();
            if (sr != null && sr.sprite == null)
            {
                Color color = Color.white;
                if (enemy.data != null)
                {
                    string name = enemy.data.enemyName.ToLower();
                    if (name.Contains("ghost"))
                        color = new Color(0.4f, 0.6f, 0.95f);
                    else if (name.Contains("goblin"))
                        color = new Color(0.2f, 0.7f, 0.2f);
                    else if (name.Contains("orc"))
                        color = new Color(0.55f, 0.35f, 0.2f);
                }
                sr.sprite = SpriteGenerator.CreateCircle(16, color);
            }
        }
    }
}
