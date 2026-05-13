using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

public class SceneBuilder : MonoBehaviour
{
    void Awake()
    {
        SetupCamera();
        EnsureEventSystem();
        EnsureObjectPooler();
        CreateManagers();
        CreateMapBackground();
        CreateWaypointPath();
        CreateEnemyTemplates();
    }

    void SetupCamera()
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            // Image is 1407x768; at PPU=96 it becomes ~14.65x8 world units
            // Set orthographic size to 5 so the full map height (8 units) fits with padding
            cam.orthographicSize = 5.0f;
            cam.transform.position = new Vector3(0f, 0f, -10f);
            cam.backgroundColor = new Color(0.06f, 0.08f, 0.06f);
        }
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

    void CreateManagers()
    {
        var gridGO = new GameObject("GridManager");
        gridGO.AddComponent<GridManager>();

        var gmGO = new GameObject("GameManager");
        gmGO.AddComponent<GameManager>();

        var wsGO = new GameObject("WaveSpawner");
        wsGO.AddComponent<WaveSpawner>();
    }

    void CreateMapBackground()
    {
        // Load the user-provided map image from Resources
        Texture2D tex = Resources.Load<Texture2D>("Sprites/map_background");
        if (tex == null)
        {
            Debug.LogWarning("SceneBuilder: map_background.png not found in Resources/Sprites/. Falling back to procedural tiles.");
            CreateProceduralFallback();
            return;
        }

        // Image is 1407x768. Use PPU=96 so height = 768/96 = 8 world units (matches grid height).
        // Width = 1407/96 ≈ 14.65 world units (slightly wider than 12-wide grid — fine, covers border).
        float ppu = 96f;
        var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f), ppu);

        var bgGO = new GameObject("MapBackground");
        var sr = bgGO.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = -10;

        // Center of grid: CellToWorld spans col 0..11 → x from -5.5 to 5.5, center = 0
        //                               row 0..7  → y from -3.5 to 3.5, center = 0
        bgGO.transform.position = new Vector3(0f, 0f, 0f);
    }

    // Minimal fallback in case image is missing
    void CreateProceduralFallback()
    {
        Sprite grassSprite = SpriteGenerator.CreateGrassTile();
        Sprite pathSprite  = SpriteGenerator.CreatePathTile();
        Sprite waterSprite = SpriteGenerator.CreateWaterTile();
        var gridParent = new GameObject("Grid");
        for (int col = 0; col < GridManager.Cols; col++)
        {
            for (int row = 0; row < GridManager.Rows; row++)
            {
                var tile = new GameObject("Tile_" + col + "_" + row);
                tile.transform.SetParent(gridParent.transform);
                tile.transform.position = GridManager.CellToWorld(col, row);
                var tsr = tile.AddComponent<SpriteRenderer>();
                tsr.sortingOrder = 0;
                if (GridManager.Instance.IsWater(col, row))       tsr.sprite = waterSprite;
                else if (GridManager.Instance.IsPath(col, row))   tsr.sprite = pathSprite;
                else                                               tsr.sprite = grassSprite;
            }
        }
    }

    void CreateWaypointPath()
    {
        var pathGO = new GameObject("WaypointPath");

        foreach (var wp in GridManager.WaypointCells)
        {
            Vector3 pos = GridManager.CellToWorld(wp);
            var waypointGO = new GameObject("WP_" + wp.x + "_" + wp.y);
            waypointGO.transform.SetParent(pathGO.transform);
            waypointGO.transform.position = pos;
        }

        pathGO.AddComponent<WaypointPath>();
    }

    void CreateEnemyTemplates()
    {
        if (WaveSpawner.Instance == null) return;

        Sprite hpBgSprite = SpriteGenerator.CreateHealthBarBG();
        Sprite hpFillSprite = SpriteGenerator.CreateHealthBarFill();

        WaveSpawner.Instance.EnemyTemplates["Goblin"] = CreateEnemyTemplate(
            "Goblin", 50, 0.8f, 10, 5, false, hpBgSprite, hpFillSprite);
        WaveSpawner.Instance.EnemyTemplates["Orc"] = CreateEnemyTemplate(
            "Orc", 200, 0.6f, 25, 15, false, hpBgSprite, hpFillSprite);
        WaveSpawner.Instance.EnemyTemplates["Ghost"] = CreateEnemyTemplate(
            "Ghost", 100, 0.7f, 20, 10, true, hpBgSprite, hpFillSprite);
    }

    GameObject CreateEnemyTemplate(string enemyName, int hp, float speed, int cost,
        int goldReward, bool immuneToSlow, Sprite hpBg, Sprite hpFill)
    {
        var go = new GameObject(enemyName);
        go.SetActive(false);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreateEnemySprite(enemyName);
        sr.sortingOrder = 10;

        var em = go.AddComponent<EnemyMovement>();
        em.maxHP = hp;
        em.currentHP = hp;
        em.baseSpeed = speed;
        em.moveSpeed = speed;
        em.immuneToSlow = immuneToSlow;
        em.rewardGold = goldReward;
        em.enemyType = enemyName;
        em.SetupHealthBar(hpBg, hpFill);

        return go;
    }
}
