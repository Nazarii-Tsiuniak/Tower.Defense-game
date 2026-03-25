using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class TowerPlacement : MonoBehaviour
{
    private GameObject previewGO;
    private SpriteRenderer previewSR;
    private SpriteRenderer rangeIndicatorSR;

    void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.State != GameState.Preparation)
        {
            HidePreview();
            return;
        }

        if (GameUIManager.Instance == null) return;
        string selectedType = GameUIManager.Instance.SelectedTowerType;

        if (string.IsNullOrEmpty(selectedType))
        {
            HidePreview();
            return;
        }

        if (Mouse.current == null) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        Vector2 screenPos = Mouse.current.position.ReadValue();
        Vector3 worldPos = cam.ScreenToWorldPoint(screenPos);
        worldPos.z = 0f;

        Vector2Int cell = GridManager.WorldToCell(worldPos);
        Vector3 snappedPos = GridManager.CellToWorld(cell);

        bool canPlace = GridManager.Instance != null && GridManager.Instance.CanPlaceTower(cell);
        int cost = GameUIManager.Instance.GetTowerCost(selectedType);
        bool canAfford = GameManager.Instance.Gold >= cost;

        ShowPreview(snappedPos, selectedType, canPlace && canAfford);

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            // Robust UI-over check compatible with new Input System
            if (IsPointerOverUI(screenPos))
                return;

            if (canPlace && canAfford)
            {
                PlaceTower(snappedPos, cell, selectedType, cost);
            }
        }
    }

    // Manual UI raycast — works reliably with InputSystemUIInputModule
    static bool IsPointerOverUI(Vector2 screenPos)
    {
        if (EventSystem.current == null) return false;
        var eventData = new PointerEventData(EventSystem.current) { position = screenPos };
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        return results.Count > 0;
    }

    void ShowPreview(Vector3 pos, string towerType, bool valid)
    {
        if (previewGO == null)
        {
            previewGO = new GameObject("TowerPreview");
            previewSR = previewGO.AddComponent<SpriteRenderer>();
            previewSR.sortingOrder = 20;

            var rangeGO = new GameObject("RangeIndicator");
            rangeGO.transform.SetParent(previewGO.transform);
            rangeGO.transform.localPosition = Vector3.zero;
            rangeIndicatorSR = rangeGO.AddComponent<SpriteRenderer>();
            rangeIndicatorSR.sprite = SpriteGenerator.CreateRangeIndicator();
            rangeIndicatorSR.sortingOrder = 19;
        }

        previewGO.SetActive(true);
        previewGO.transform.position = pos;
        previewSR.sprite = SpriteGenerator.CreateTowerSprite(towerType);
        previewSR.color = valid ? new Color(1, 1, 1, 0.6f) : new Color(1, 0.3f, 0.3f, 0.6f);

        float range = GetTowerRange(towerType);
        float scale = range * 2f;
        rangeIndicatorSR.transform.localScale = new Vector3(scale, scale, 1);
    }

    void HidePreview()
    {
        if (previewGO != null)
            previewGO.SetActive(false);
    }

    // Centralized tower configuration
    public struct TowerStats
    {
        public float damage;
        public float range;
        public float fireRate;
        public AttackType attackType;
        public float slowAmount;
        public float slowDuration;
        public float aoeRadius;
    }

    static readonly System.Collections.Generic.Dictionary<string, TowerStats> TowerConfigs =
        new System.Collections.Generic.Dictionary<string, TowerStats>
    {
        { "Archer",  new TowerStats { damage = 20, range = 3.0f, fireRate = 0.8f, attackType = AttackType.Single } },
        { "Mage",    new TowerStats { damage = 15, range = 2.5f, fireRate = 0.4f, attackType = AttackType.AoE, aoeRadius = 1.5f } },
        { "Freezer", new TowerStats { damage = 3,  range = 3.0f, fireRate = 0.7f, attackType = AttackType.Slow, slowAmount = 0.5f, slowDuration = 2.0f } },
        { "Cannon",  new TowerStats { damage = 80, range = 4.0f, fireRate = 0.2f, attackType = AttackType.Single } }
    };

    void PlaceTower(Vector3 position, Vector2Int cell, string towerType, int cost)
    {
        if (!GameManager.Instance.SpendGold(cost)) return;

        GridManager.Instance.SetTower(cell.x, cell.y);

        var tower = new GameObject("Tower_" + towerType);
        tower.transform.position = position;

        var sr = tower.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreateTowerSprite(towerType);
        sr.sortingOrder = 5;

        var ctrl = tower.AddComponent<TowerController>();
        if (TowerConfigs.TryGetValue(towerType, out TowerStats stats))
        {
            ctrl.towerName = towerType;
            ctrl.damage = stats.damage;
            ctrl.range = stats.range;
            ctrl.fireRate = stats.fireRate;
            ctrl.attackType = stats.attackType;
            ctrl.slowAmount = stats.slowAmount;
            ctrl.slowDuration = stats.slowDuration;
            ctrl.aoeRadius = stats.aoeRadius;
        }
    }

    float GetTowerRange(string towerType)
    {
        if (TowerConfigs.TryGetValue(towerType, out TowerStats stats))
            return stats.range;
        return 3.0f;
    }
}
