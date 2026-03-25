using UnityEngine;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance;

    public string SelectedTowerType { get; private set; }

    private static readonly string[] towerTypes = { "Archer", "Mage", "Cannon" };

    private Button[] towerButtons;
    private Color normalColor = new Color(0.35f, 0.35f, 0.35f);
    private Color selectedColor = new Color(0.2f, 0.6f, 0.2f);

    void Awake()
    {
        Instance = this;
        CreateUI();
    }

    void CreateUI()
    {
        var canvasGO = new GameObject("TowerUI_Canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);

        canvasGO.AddComponent<GraphicRaycaster>();

        var panelGO = new GameObject("ButtonPanel");
        panelGO.transform.SetParent(canvasGO.transform, false);

        var panelRect = panelGO.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0f, 0f);
        panelRect.anchorMax = new Vector2(1f, 0f);
        panelRect.pivot = new Vector2(0.5f, 0f);
        panelRect.anchoredPosition = new Vector2(0f, 10f);
        panelRect.sizeDelta = new Vector2(0f, 60f);

        var layout = panelGO.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 10f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        layout.padding = new RectOffset(10, 10, 5, 5);

        towerButtons = new Button[towerTypes.Length];

        for (int i = 0; i < towerTypes.Length; i++)
        {
            towerButtons[i] = CreateTowerButton(panelGO.transform, towerTypes[i]);
        }
    }

    Button CreateTowerButton(Transform parent, string towerType)
    {
        var btnGO = new GameObject(towerType + "_Button");
        btnGO.transform.SetParent(parent, false);

        var rect = btnGO.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(150f, 50f);

        var image = btnGO.AddComponent<Image>();
        image.color = normalColor;

        var button = btnGO.AddComponent<Button>();
        var colors = button.colors;
        colors.normalColor = normalColor;
        colors.highlightedColor = new Color(0.45f, 0.45f, 0.45f);
        colors.pressedColor = new Color(0.25f, 0.25f, 0.25f);
        colors.selectedColor = selectedColor;
        button.colors = colors;

        string type = towerType;
        button.onClick.AddListener(() => SelectTower(type));

        var textGO = new GameObject("Label");
        textGO.transform.SetParent(btnGO.transform, false);

        var textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        var text = textGO.AddComponent<Text>();
        text.text = "[" + towerType[0] + "] " + towerType;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.fontSize = 18;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        return button;
    }

    void SelectTower(string type)
    {
        if (SelectedTowerType == type)
        {
            SelectedTowerType = null;
        }
        else
        {
            SelectedTowerType = type;
        }

        UpdateButtonVisuals();
    }

    void UpdateButtonVisuals()
    {
        for (int i = 0; i < towerButtons.Length; i++)
        {
            var img = towerButtons[i].GetComponent<Image>();
            img.color = (towerTypes[i] == SelectedTowerType) ? selectedColor : normalColor;
        }
    }
}
