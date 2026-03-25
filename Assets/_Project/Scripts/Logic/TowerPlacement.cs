using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

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
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            if (canPlace && canAfford)
            {
                PlaceTower(snappedPos, cell, selectedType, cost);
            }
        }
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
        ApplyTowerStats(ctrl, towerType);
    }

    void ApplyTowerStats(TowerController ctrl, string towerType)
    {
        ctrl.towerName = towerType;
        switch (towerType)
        {
            case "Archer":
                ctrl.damage = 20;
                ctrl.range = 3.0f;
                ctrl.fireRate = 0.8f;
                ctrl.attackType = AttackType.Single;
                break;
            case "Mage":
                ctrl.damage = 15;
                ctrl.range = 2.5f;
                ctrl.fireRate = 0.4f;
                ctrl.attackType = AttackType.AoE;
                ctrl.aoeRadius = 1.5f;
                break;
            case "Freezer":
                ctrl.damage = 3;
                ctrl.range = 3.0f;
                ctrl.fireRate = 0.7f;
                ctrl.attackType = AttackType.Slow;
                ctrl.slowAmount = 0.5f;
                ctrl.slowDuration = 2.0f;
                break;
            case "Cannon":
                ctrl.damage = 80;
                ctrl.range = 4.0f;
                ctrl.fireRate = 0.2f;
                ctrl.attackType = AttackType.Single;
                break;
        }
    }

    float GetTowerRange(string towerType)
    {
        switch (towerType)
        {
            case "Archer":  return 3.0f;
            case "Mage":    return 2.5f;
            case "Freezer": return 3.0f;
            case "Cannon":  return 4.0f;
            default:        return 3.0f;
        }
    }
}
