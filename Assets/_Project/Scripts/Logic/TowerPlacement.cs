using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class TowerPlacement : MonoBehaviour
{
    void Update()
    {
        if (Mouse.current == null) return;
        if (!Mouse.current.leftButton.wasPressedThisFrame) return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (GameUIManager.Instance == null) return;
        if (string.IsNullOrEmpty(GameUIManager.Instance.SelectedTowerType)) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        Vector2 screenPos = Mouse.current.position.ReadValue();
        Vector3 worldPos = cam.ScreenToWorldPoint(screenPos);
        worldPos.z = 0f;

        worldPos.x = Mathf.Round(worldPos.x);
        worldPos.y = Mathf.Round(worldPos.y);

        PlaceTower(worldPos, GameUIManager.Instance.SelectedTowerType);
    }

    void PlaceTower(Vector3 position, string towerType)
    {
        var tower = new GameObject("Tower_" + towerType);
        tower.transform.position = position;

        var sr = tower.AddComponent<SpriteRenderer>();
        sr.sprite = CreatePlaceholderSprite(towerType);
        sr.sortingOrder = 5;
    }

    Sprite CreatePlaceholderSprite(string towerType)
    {
        int size = 32;
        var tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Point;

        Color color;
        switch (towerType)
        {
            case "Archer": color = new Color(0.2f, 0.7f, 0.2f); break;
            case "Mage":   color = new Color(0.4f, 0.2f, 0.8f); break;
            case "Cannon": color = new Color(0.8f, 0.4f, 0.1f); break;
            default:       color = Color.gray; break;
        }

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool border = x == 0 || x == size - 1 || y == 0 || y == size - 1;
                tex.SetPixel(x, y, border ? Color.black : color);
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
