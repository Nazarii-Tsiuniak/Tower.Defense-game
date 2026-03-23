using UnityEngine;

public class TowerPlacement : MonoBehaviour
{
    public bool isOccupied = false;
    private SpriteRenderer spriteRenderer;

    public static TowerData selectedTowerData;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        if (spriteRenderer.sprite == null)
        {
            spriteRenderer.sprite = SpriteGenerator.CreateCircle(32, new Color(0.9f, 0.9f, 0.3f, 0.5f));
        }
        spriteRenderer.sortingOrder = 0;

        // Add collider for mouse clicks
        var collider = GetComponent<BoxCollider2D>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(1f, 1f);
        }
    }

    public static void SelectTower(TowerData tower)
    {
        selectedTowerData = tower;
    }

    void OnMouseDown()
    {
        if (isOccupied) return;
        if (selectedTowerData == null) return;
        if (GameManager.Instance == null) return;
        if (!GameManager.Instance.SpendGold(selectedTowerData.cost)) return;

        BuildTower(selectedTowerData);
    }

    void OnMouseEnter()
    {
        if (!isOccupied && spriteRenderer != null)
        {
            spriteRenderer.color = new Color(1f, 1f, 0.5f, 0.8f);
        }
    }

    void OnMouseExit()
    {
        if (!isOccupied && spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }
    }

    void BuildTower(TowerData data)
    {
        isOccupied = true;

        var towerObj = new GameObject("Tower_" + data.towerName);
        towerObj.transform.position = transform.position;

        var sr = towerObj.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 2;

        Color towerColor;
        if (data.towerName.Contains("Archer"))
            towerColor = new Color(0.85f, 0.65f, 0.13f);
        else if (data.towerName.Contains("Cannon"))
            towerColor = new Color(0.45f, 0.45f, 0.5f);
        else if (data.towerName.Contains("Mage"))
            towerColor = new Color(0.58f, 0.27f, 0.85f);
        else
            towerColor = Color.gray;

        sr.sprite = SpriteGenerator.CreateDiamond(32, towerColor);

        var tower = towerObj.AddComponent<TowerBehaviour>();
        tower.data = data;

        // Add range indicator
        var rangeObj = new GameObject("RangeIndicator");
        rangeObj.transform.SetParent(towerObj.transform);
        rangeObj.transform.localPosition = Vector3.zero;
        var rangeSr = rangeObj.AddComponent<SpriteRenderer>();
        rangeSr.sprite = SpriteGenerator.CreateCircle(32, new Color(1f, 0f, 0f, 0.08f));
        rangeSr.sortingOrder = -1;
        rangeObj.transform.localScale = Vector3.one * data.range * 2;

        // Dim the placement spot
        spriteRenderer.color = new Color(0.4f, 0.4f, 0.4f, 0.3f);
    }
}
