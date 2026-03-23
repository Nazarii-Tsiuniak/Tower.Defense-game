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
        var canvasObj = new GameObject("GameCanvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);

        canvasObj.AddComponent<GraphicRaycaster>();

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
        var topBar = CreatePanel(parent, "TopBar", new Color(0.12f, 0.12f, 0.18f, 0.92f));
        var topRect = topBar.GetComponent<RectTransform>();
        topRect.anchorMin = new Vector2(0, 1);
        topRect.anchorMax = new Vector2(1, 1);
        topRect.pivot = new Vector2(0.5f, 1);
        topRect.sizeDelta = new Vector2(0, 56);
        topRect.anchoredPosition = Vector2.zero;

        var layout = topBar.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(24, 24, 6, 6);
        layout.spacing = 50;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = true;

        livesText = CreateLabel(topBar.transform, "LivesText", "\u2764 Lives: 20", new Color(1f, 0.35f, 0.35f), 24);
        goldText = CreateLabel(topBar.transform, "GoldText", "\uD83D\uDCB0 Gold: 100", new Color(1f, 0.85f, 0.15f), 24);
        waveText = CreateLabel(topBar.transform, "WaveText", "\u2694 Wave: 0", new Color(0.55f, 0.85f, 1f), 24);
    }

    void CreateTowerPanel(Transform parent)
    {
        towerPanel = CreatePanel(parent, "TowerPanel", new Color(0.12f, 0.12f, 0.18f, 0.92f));
        var panelRect = towerPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 0);
        panelRect.anchorMax = new Vector2(1, 0);
        panelRect.pivot = new Vector2(0.5f, 0);
        panelRect.sizeDelta = new Vector2(0, 80);
        panelRect.anchoredPosition = Vector2.zero;

        var layout = towerPanel.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(24, 24, 10, 10);
        layout.spacing = 18;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = true;

        CreateLabel(towerPanel.transform, "TowerTitle", "\uD83C\uDFF0 Towers:", Color.white, 22);

        if (availableTowers != null)
        {
            foreach (var tower in availableTowers)
                CreateTowerButton(towerPanel.transform, tower);
        }
    }

    void CreateTowerButton(Transform parent, TowerData data)
    {
        var btnObj = new GameObject("Btn_" + data.towerName);
        btnObj.transform.SetParent(parent, false);

        var btnImage = btnObj.AddComponent<Image>();
        btnImage.color = GetTowerButtonColor(data.towerName);

        var btn = btnObj.AddComponent<Button>();
        var colors = btn.colors;
        colors.highlightedColor = new Color(
            btnImage.color.r + 0.12f,
            btnImage.color.g + 0.12f,
            btnImage.color.b + 0.12f, 1f);
        colors.pressedColor = new Color(
            btnImage.color.r - 0.08f,
            btnImage.color.g - 0.08f,
            btnImage.color.b - 0.08f, 1f);
        btn.colors = colors;

        var btnRect = btnObj.GetComponent<RectTransform>();
        btnRect.sizeDelta = new Vector2(170, 58);

        // Outline frame
        var outline = btnObj.AddComponent<Outline>();
        outline.effectColor = new Color(1f, 1f, 1f, 0.3f);
        outline.effectDistance = new Vector2(2, -2);

        var labelObj = new GameObject("Label");
        labelObj.transform.SetParent(btnObj.transform, false);
        var text = labelObj.AddComponent<Text>();
        text.text = GetTowerEmoji(data.towerName) + " " + data.towerName + "\n$" + data.cost;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 17;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
        var labelRect = labelObj.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.sizeDelta = Vector2.zero;

        // Shadow on text
        var textShadow = labelObj.AddComponent<Shadow>();
        textShadow.effectColor = new Color(0, 0, 0, 0.5f);
        textShadow.effectDistance = new Vector2(1, -1);

        TowerData capturedData = data;
        btn.onClick.AddListener(() =>
        {
            TowerPlacement.SelectTower(capturedData);
            HighlightButton(btnObj.transform.parent, btnObj);
        });
    }

    Color GetTowerButtonColor(string name)
    {
        if (name.Contains("Archer")) return new Color(0.55f, 0.40f, 0.15f);
        if (name.Contains("Cannon")) return new Color(0.35f, 0.35f, 0.40f);
        if (name.Contains("Mage")) return new Color(0.40f, 0.20f, 0.55f);
        return new Color(0.3f, 0.3f, 0.4f);
    }

    string GetTowerEmoji(string name)
    {
        if (name.Contains("Archer")) return "\uD83C\uDFF9";
        if (name.Contains("Cannon")) return "\uD83D\uDCA3";
        if (name.Contains("Mage")) return "\u2728";
        return "\uD83C\uDFF0";
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
                    ? new Color(0.25f, 0.60f, 0.25f, 1f)
                    : GetTowerButtonColor(child.name.Replace("Btn_", ""));
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

    Text CreateLabel(Transform parent, string name, string content, Color color, int fontSize = 22)
    {
        var textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);
        var text = textObj.AddComponent<Text>();
        text.text = content;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = TextAnchor.MiddleLeft;

        // Add shadow for cartoon look
        var shadow = textObj.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.5f);
        shadow.effectDistance = new Vector2(1, -1);

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
        if (goldText != null) goldText.text = "\uD83D\uDCB0 Gold: " + GameManager.Instance.gold;
        if (waveText != null) waveText.text = "\u2694 Wave: " + GameManager.Instance.currentWave;
    }
}
