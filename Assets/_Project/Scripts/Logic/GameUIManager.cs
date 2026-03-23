using UnityEngine;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    private Text livesText;
    private Text goldText;
    private Text waveText;
    private GameObject towerPanel;

    public TowerData[] availableTowers;

    public void Initialize(TowerData[] towers)
    {
        availableTowers = towers;
        CreateCanvas();
    }

    void CreateCanvas()
    {
        // Main Canvas
        var canvasObj = new GameObject("GameCanvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);

        canvasObj.AddComponent<GraphicRaycaster>();

        // EventSystem
        if (FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var eventObj = new GameObject("EventSystem");
            eventObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        CreateTopBar(canvasObj.transform);
        CreateTowerPanel(canvasObj.transform);
    }

    void CreateTopBar(Transform parent)
    {
        // Top bar panel
        var topBar = CreatePanel(parent, "TopBar", new Color(0.15f, 0.15f, 0.2f, 0.85f));
        var topRect = topBar.GetComponent<RectTransform>();
        topRect.anchorMin = new Vector2(0, 1);
        topRect.anchorMax = new Vector2(1, 1);
        topRect.pivot = new Vector2(0.5f, 1);
        topRect.sizeDelta = new Vector2(0, 50);
        topRect.anchoredPosition = Vector2.zero;

        var layout = topBar.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(20, 20, 5, 5);
        layout.spacing = 40;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = true;

        livesText = CreateLabel(topBar.transform, "LivesText", "\u2764 Lives: 20", new Color(1f, 0.4f, 0.4f));
        goldText = CreateLabel(topBar.transform, "GoldText", "\u2B50 Gold: 100", new Color(1f, 0.85f, 0.2f));
        waveText = CreateLabel(topBar.transform, "WaveText", "\u2694 Wave: 0", new Color(0.6f, 0.8f, 1f));
    }

    void CreateTowerPanel(Transform parent)
    {
        // Bottom tower selection panel
        towerPanel = CreatePanel(parent, "TowerPanel", new Color(0.15f, 0.15f, 0.2f, 0.85f));
        var panelRect = towerPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 0);
        panelRect.anchorMax = new Vector2(1, 0);
        panelRect.pivot = new Vector2(0.5f, 0);
        panelRect.sizeDelta = new Vector2(0, 70);
        panelRect.anchoredPosition = Vector2.zero;

        var layout = towerPanel.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(20, 20, 8, 8);
        layout.spacing = 15;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = true;

        // Title
        CreateLabel(towerPanel.transform, "TowerTitle", "Towers:", Color.white);

        if (availableTowers != null)
        {
            foreach (var tower in availableTowers)
            {
                CreateTowerButton(towerPanel.transform, tower);
            }
        }
    }

    void CreateTowerButton(Transform parent, TowerData data)
    {
        var btnObj = new GameObject("Btn_" + data.towerName);
        btnObj.transform.SetParent(parent, false);

        var btnImage = btnObj.AddComponent<Image>();
        btnImage.color = new Color(0.3f, 0.3f, 0.4f, 1f);

        var btn = btnObj.AddComponent<Button>();
        var colors = btn.colors;
        colors.highlightedColor = new Color(0.4f, 0.5f, 0.6f, 1f);
        colors.pressedColor = new Color(0.2f, 0.3f, 0.4f, 1f);
        btn.colors = colors;

        var btnRect = btnObj.GetComponent<RectTransform>();
        btnRect.sizeDelta = new Vector2(160, 54);

        // Button label
        var labelObj = new GameObject("Label");
        labelObj.transform.SetParent(btnObj.transform, false);
        var text = labelObj.AddComponent<Text>();
        text.text = data.towerName + "\n$" + data.cost;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 16;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
        var labelRect = labelObj.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.sizeDelta = Vector2.zero;

        TowerData capturedData = data;
        btn.onClick.AddListener(() =>
        {
            TowerPlacement.SelectTower(capturedData);
            HighlightButton(btnObj.transform.parent, btnObj);
        });
    }

    void HighlightButton(Transform parent, GameObject selected)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            var child = parent.GetChild(i);
            var img = child.GetComponent<Image>();
            if (img != null)
            {
                img.color = (child.gameObject == selected)
                    ? new Color(0.3f, 0.6f, 0.3f, 1f)
                    : new Color(0.3f, 0.3f, 0.4f, 1f);
            }
        }
    }

    GameObject CreatePanel(Transform parent, string name, Color color)
    {
        var panelObj = new GameObject(name);
        panelObj.transform.SetParent(parent, false);
        var img = panelObj.AddComponent<Image>();
        img.color = color;
        return panelObj;
    }

    Text CreateLabel(Transform parent, string name, string content, Color color)
    {
        var textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);
        var text = textObj.AddComponent<Text>();
        text.text = content;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 22;
        text.color = color;
        text.alignment = TextAnchor.MiddleLeft;

        var fitter = textObj.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

        return text;
    }

    void OnEnable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStatsChanged += UpdateUI;
    }

    void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStatsChanged -= UpdateUI;
    }

    void UpdateUI()
    {
        if (GameManager.Instance == null) return;
        if (livesText != null) livesText.text = "\u2764 Lives: " + GameManager.Instance.lives;
        if (goldText != null) goldText.text = "\u2B50 Gold: " + GameManager.Instance.gold;
        if (waveText != null) waveText.text = "\u2694 Wave: " + GameManager.Instance.currentWave;
    }
}
