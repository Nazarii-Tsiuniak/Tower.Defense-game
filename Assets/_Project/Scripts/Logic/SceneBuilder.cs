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
        CreateGridOverlay();
        CreateWaypointPath();
        CreateEnemyTemplates();
        CreateBackgroundMusic();
    }

    void SetupCamera()
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            // Grid is 22x12 cells, PPU=64 => world size 22x12
            // orthographicSize = half-height = 12/2 = 6
            cam.orthographicSize = 6.0f;
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

        // Image is 1407x768, PPU=64 => world size = 1407/64 x 768/64 = 21.98 x 12
        // Centered at (0,0): fits the 22x12 cell grid perfectly
        float ppu = 64f;
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

    // Semi-transparent grid overlay so player can see cells for tower placement
    void CreateGridOverlay()
    {
        // Create a simple 1x1 white pixel sprite for cell outlines
        var tex = new Texture2D(1, 1);
        tex.filterMode = FilterMode.Point;
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        var cellSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);

        // Border-only cell: use LineRenderer per cell is expensive; instead use a 32x32 border texture
        var borderTex = new Texture2D(32, 32);
        borderTex.filterMode = FilterMode.Point;
        Color clear = new Color(0, 0, 0, 0);
        Color line  = new Color(1f, 1f, 1f, 0.18f);
        for (int y = 0; y < 32; y++)
            for (int x = 0; x < 32; x++)
                borderTex.SetPixel(x, y, (x == 0 || x == 31 || y == 0 || y == 31) ? line : clear);
        borderTex.Apply();
        var borderSprite = Sprite.Create(borderTex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32f);

        var overlayParent = new GameObject("GridOverlay");
        overlayParent.SetActive(false);  // hidden by default; shown only during tower placement
        for (int col = 0; col < GridManager.Cols; col++)
        {
            for (int row = 0; row < GridManager.Rows; row++)
            {
                var cell = new GameObject("Cell_" + col + "_" + row);
                cell.transform.SetParent(overlayParent.transform);
                cell.transform.position = GridManager.CellToWorld(col, row);
                var sr = cell.AddComponent<SpriteRenderer>();
                sr.sprite = borderSprite;
                sr.sortingOrder = 5;  // above background, below enemies/towers
            }
        }
    }

    void CreateWaypointPath()
    {
        var pathGO = new GameObject("WaypointPath");
        foreach (var wp in GridManager.WaypointCells)
        {
            var waypointGO = new GameObject("WP_" + wp.x + "_" + wp.y);
            waypointGO.transform.SetParent(pathGO.transform);
            waypointGO.transform.position = GridManager.CellToWorld(wp);
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

    void CreateBackgroundMusic()
    {
        var musicGO = new GameObject("BackgroundMusic");
        musicGO.AddComponent<BackgroundMusic>();
    }
}
