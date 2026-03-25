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
        CreateGrid();
        CreateWaypointPath();
        CreateEnemyTemplates();
    }

    void SetupCamera()
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.orthographicSize = 5.5f;
            cam.transform.position = new Vector3(0, 0.5f, -10f);
            cam.backgroundColor = new Color(0.18f, 0.22f, 0.18f);
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

    void CreateGrid()
    {
        Sprite grassSprite = SpriteGenerator.CreateGrassTile();
        Sprite pathSprite = SpriteGenerator.CreatePathTile();
        Sprite entrySprite = SpriteGenerator.CreateEntryMarker();
        Sprite baseSprite = SpriteGenerator.CreateBaseMarker();

        var gridParent = new GameObject("Grid");

        for (int col = 0; col < GridManager.Cols; col++)
        {
            for (int row = 0; row < GridManager.Rows; row++)
            {
                Vector3 pos = GridManager.CellToWorld(col, row);
                bool isPath = GridManager.Instance != null && GridManager.Instance.IsPath(col, row);

                var tile = new GameObject("Tile_" + col + "_" + row);
                tile.transform.SetParent(gridParent.transform);
                tile.transform.position = pos;

                var sr = tile.AddComponent<SpriteRenderer>();
                sr.sortingOrder = 0;

                Vector2Int cell = new Vector2Int(col, row);
                if (cell == GridManager.Instance.EntryCell)
                    sr.sprite = entrySprite;
                else if (cell == GridManager.Instance.BaseCell)
                    sr.sprite = baseSprite;
                else if (isPath)
                    sr.sprite = pathSprite;
                else
                    sr.sprite = grassSprite;
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
            "Goblin", 50, 3.0f, 10, 5, false, hpBgSprite, hpFillSprite);
        WaveSpawner.Instance.EnemyTemplates["Orc"] = CreateEnemyTemplate(
            "Orc", 200, 1.2f, 25, 15, false, hpBgSprite, hpFillSprite);
        WaveSpawner.Instance.EnemyTemplates["Ghost"] = CreateEnemyTemplate(
            "Ghost", 100, 2.0f, 20, 10, true, hpBgSprite, hpFillSprite);
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
