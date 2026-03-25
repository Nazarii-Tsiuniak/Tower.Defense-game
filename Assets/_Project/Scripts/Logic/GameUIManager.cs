using UnityEngine;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance;

    public string SelectedTowerType { get; private set; }

    // Tower configs for UI
    public struct TowerInfo
    {
        public string name;
        public int cost;
        public string description;
        public TowerInfo(string n, int c, string d) { name = n; cost = c; description = d; }
    }

    public static readonly TowerInfo[] TowerInfos = new TowerInfo[]
    {
        new TowerInfo("Archer",  100, "Single target, medium speed"),
        new TowerInfo("Mage",    150, "AoE damage, slow speed"),
        new TowerInfo("Freezer", 120, "Slows enemies"),
        new TowerInfo("Cannon",  200, "High damage, very slow")
    };

    private Canvas canvas;
    private Text goldText;
    private Text hpText;
    private Text roundText;
    private Text stateText;
    private Button startWaveBtn;
    private Text startWaveBtnText;
    private Button[] towerButtons;
    private Image[] towerButtonImages;
    private GameObject gameOverPanel;
    private Text gameOverText;
    private Button restartBtn;

    private Color normalBtnColor = new Color(0.30f, 0.30f, 0.35f);
    private Color selectedBtnColor = new Color(0.15f, 0.55f, 0.20f);
    private Color disabledBtnColor = new Color(0.20f, 0.20f, 0.20f);
    private Font uiFont;

    void Awake()
    {
        Instance = this;
        uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }

    void Start()
    {
        CreateUI();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged += OnGameStateChanged;
            GameManager.Instance.OnStatsChanged += UpdateStatsUI;
            OnGameStateChanged(GameManager.Instance.State);
            UpdateStatsUI();
        }
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged -= OnGameStateChanged;
            GameManager.Instance.OnStatsChanged -= UpdateStatsUI;
        }
    }

    void CreateUI()
    {
        // Canvas
        var canvasGO = new GameObject("GameUI_Canvas");
        canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        canvasGO.AddComponent<GraphicRaycaster>();

        CreateTopBar(canvasGO.transform);
        CreateTowerPanel(canvasGO.transform);
        CreateStartWaveButton(canvasGO.transform);
        CreateGameOverPanel(canvasGO.transform);
    }

    void CreateTopBar(Transform parent)
    {
        // Top bar background
        var barGO = new GameObject("TopBar");
        barGO.transform.SetParent(parent, false);
        var barRect = barGO.AddComponent<RectTransform>();
        barRect.anchorMin = new Vector2(0, 1);
        barRect.anchorMax = new Vector2(1, 1);
        barRect.pivot = new Vector2(0.5f, 1);
        barRect.sizeDelta = new Vector2(0, 45);
        var barImg = barGO.AddComponent<Image>();
        barImg.color = new Color(0, 0, 0, 0.7f);

        // Gold
        goldText = CreateText(barGO.transform, "GoldText", "Gold: 300",
            new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f),
            new Vector2(150, 35), new Vector2(20, 0), 20, new Color(1f, 0.85f, 0.2f));

        // Base HP
        hpText = CreateText(barGO.transform, "HPText", "Base HP: 20/20",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(200, 35), Vector2.zero, 20, new Color(0.9f, 0.3f, 0.3f));

        // Round
        roundText = CreateText(barGO.transform, "RoundText", "Round: 1/10",
            new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f),
            new Vector2(160, 35), new Vector2(-20, 0), 20, Color.white);

        // State text
        stateText = CreateText(barGO.transform, "StateText", "",
            new Vector2(0.3f, 0.5f), new Vector2(0.3f, 0.5f), new Vector2(0.3f, 0.5f),
            new Vector2(200, 35), Vector2.zero, 16, new Color(0.7f, 0.9f, 0.7f));
    }

    void CreateTowerPanel(Transform parent)
    {
        var panelGO = new GameObject("TowerPanel");
        panelGO.transform.SetParent(parent, false);
        var panelRect = panelGO.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 0);
        panelRect.anchorMax = new Vector2(1, 0);
        panelRect.pivot = new Vector2(0.5f, 0);
        panelRect.anchoredPosition = new Vector2(0, 5);
        panelRect.sizeDelta = new Vector2(0, 65);

        var panelImg = panelGO.AddComponent<Image>();
        panelImg.color = new Color(0, 0, 0, 0.7f);

        var layout = panelGO.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 8;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        layout.padding = new RectOffset(10, 10, 5, 5);

        towerButtons = new Button[TowerInfos.Length];
        towerButtonImages = new Image[TowerInfos.Length];

        for (int i = 0; i < TowerInfos.Length; i++)
        {
            int idx = i;
            var info = TowerInfos[i];

            var btnGO = new GameObject(info.name + "_Btn");
            btnGO.transform.SetParent(panelGO.transform, false);
            var btnRect = btnGO.AddComponent<RectTransform>();
            btnRect.sizeDelta = new Vector2(160, 55);

            var btnImg = btnGO.AddComponent<Image>();
            btnImg.color = normalBtnColor;
            towerButtonImages[i] = btnImg;

            var btn = btnGO.AddComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.2f, 1.2f, 1.2f);
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f);
            btn.colors = colors;
            btn.onClick.AddListener(() => SelectTower(TowerInfos[idx].name));
            towerButtons[i] = btn;

            string label = "[" + info.name[0] + "] " + info.name + " " + info.cost + "g";
            CreateText(btnGO.transform, "Label", label,
                Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero, 15, Color.white);
        }
    }

    void CreateStartWaveButton(Transform parent)
    {
        var btnGO = new GameObject("StartWaveBtn");
        btnGO.transform.SetParent(parent, false);
        var btnRect = btnGO.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(1, 0);
        btnRect.anchorMax = new Vector2(1, 0);
        btnRect.pivot = new Vector2(1, 0);
        btnRect.anchoredPosition = new Vector2(-10, 75);
        btnRect.sizeDelta = new Vector2(180, 50);

        var btnImg = btnGO.AddComponent<Image>();
        btnImg.color = new Color(0.2f, 0.6f, 0.2f);

        startWaveBtn = btnGO.AddComponent<Button>();
        startWaveBtn.onClick.AddListener(OnStartWaveClicked);

        startWaveBtnText = CreateText(btnGO.transform, "Label", "START WAVE",
            Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero, 20, Color.white);
    }

    void CreateGameOverPanel(Transform parent)
    {
        gameOverPanel = new GameObject("GameOverPanel");
        gameOverPanel.transform.SetParent(parent, false);
        var panelRect = gameOverPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;

        var overlay = gameOverPanel.AddComponent<Image>();
        overlay.color = new Color(0, 0, 0, 0.75f);

        gameOverText = CreateText(gameOverPanel.transform, "GameOverText", "GAME OVER",
            new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.5f),
            new Vector2(500, 60), Vector2.zero, 36, Color.white);

        // Restart button
        var restartGO = new GameObject("RestartBtn");
        restartGO.transform.SetParent(gameOverPanel.transform, false);
        var restartRect = restartGO.AddComponent<RectTransform>();
        restartRect.anchorMin = new Vector2(0.5f, 0.4f);
        restartRect.anchorMax = new Vector2(0.5f, 0.4f);
        restartRect.pivot = new Vector2(0.5f, 0.5f);
        restartRect.sizeDelta = new Vector2(200, 50);
        var restartImg = restartGO.AddComponent<Image>();
        restartImg.color = new Color(0.3f, 0.5f, 0.8f);

        restartBtn = restartGO.AddComponent<Button>();
        restartBtn.onClick.AddListener(OnRestartClicked);

        CreateText(restartGO.transform, "Label", "RESTART",
            Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero, 22, Color.white);

        gameOverPanel.SetActive(false);
    }

    Text CreateText(Transform parent, string name, string content,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 sizeDelta, Vector2 anchoredPos, int fontSize, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.sizeDelta = sizeDelta;
        rect.anchoredPosition = anchoredPos;

        var text = go.AddComponent<Text>();
        text.text = content;
        text.font = uiFont;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = TextAnchor.MiddleCenter;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        return text;
    }

    void SelectTower(string type)
    {
        if (GameManager.Instance != null && GameManager.Instance.State != GameState.Preparation)
            return;

        if (SelectedTowerType == type)
            SelectedTowerType = null;
        else
            SelectedTowerType = type;

        UpdateButtonVisuals();
    }

    void UpdateButtonVisuals()
    {
        int gold = GameManager.Instance != null ? GameManager.Instance.Gold : 0;
        bool canBuy = GameManager.Instance != null && GameManager.Instance.State == GameState.Preparation;

        for (int i = 0; i < towerButtons.Length; i++)
        {
            bool affordable = canBuy && gold >= TowerInfos[i].cost;
            bool selected = TowerInfos[i].name == SelectedTowerType;

            if (selected)
                towerButtonImages[i].color = selectedBtnColor;
            else if (affordable)
                towerButtonImages[i].color = normalBtnColor;
            else
                towerButtonImages[i].color = disabledBtnColor;

            towerButtons[i].interactable = canBuy;
        }
    }

    void OnStartWaveClicked()
    {
        if (GameManager.Instance != null)
        {
            SelectedTowerType = null;
            GameManager.Instance.StartBattle();
        }
    }

    void OnRestartClicked()
    {
        // Clean up existing enemies
        foreach (var enemy in EnemyMovement.ActiveEnemies.ToArray())
        {
            if (enemy != null && enemy.gameObject.activeInHierarchy)
                ObjectPooler.Instance.ReturnToPool(enemy.gameObject);
        }

        // Clean up existing towers
        foreach (var tower in Object.FindObjectsByType<TowerController>(FindObjectsSortMode.None))
        {
            if (tower != null)
                Destroy(tower.gameObject);
        }

        // Reset grid
        if (GridManager.Instance != null)
            GridManager.Instance.ResetTowers();

        SelectedTowerType = null;

        if (GameManager.Instance != null)
            GameManager.Instance.StartNewGame();
    }

    void OnGameStateChanged(GameState state)
    {
        if (stateText == null) return;

        switch (state)
        {
            case GameState.Preparation:
                stateText.text = "Place towers!";
                if (startWaveBtn != null)
                {
                    startWaveBtn.gameObject.SetActive(true);
                    startWaveBtnText.text = "START WAVE";
                }
                if (gameOverPanel != null)
                    gameOverPanel.SetActive(false);
                break;

            case GameState.Battle:
                stateText.text = "Battle!";
                if (startWaveBtn != null)
                    startWaveBtn.gameObject.SetActive(false);
                break;

            case GameState.RoundEnd:
                stateText.text = "Round complete!";
                break;

            case GameState.GameOver:
                stateText.text = "";
                if (startWaveBtn != null)
                    startWaveBtn.gameObject.SetActive(false);
                if (gameOverPanel != null)
                {
                    gameOverPanel.SetActive(true);
                    bool won = GameManager.Instance != null && GameManager.Instance.DefenderWon;
                    gameOverText.text = won ? "DEFENDER WINS!" : "ATTACKER WINS!";
                    gameOverText.color = won ? new Color(0.3f, 1f, 0.3f) : new Color(1f, 0.3f, 0.3f);
                }
                break;
        }

        UpdateButtonVisuals();
    }

    void UpdateStatsUI()
    {
        if (GameManager.Instance == null) return;

        if (goldText != null)
            goldText.text = "Gold: " + GameManager.Instance.Gold;
        if (hpText != null)
            hpText.text = "Base HP: " + GameManager.Instance.BaseHP + "/" + GameManager.Instance.MaxBaseHP;
        if (roundText != null)
            roundText.text = "Round: " + GameManager.Instance.Round + "/" + GameManager.MaxRounds;

        UpdateButtonVisuals();
    }

    public int GetTowerCost(string towerName)
    {
        foreach (var info in TowerInfos)
            if (info.name == towerName)
                return info.cost;
        return 0;
    }
}
