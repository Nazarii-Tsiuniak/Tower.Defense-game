using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance;

    public string SelectedTowerType { get; private set; }

    // Tower configs for UI (Ukrainian names)
    public struct TowerInfo
    {
        public string key;        // internal key
        public string displayName; // Ukrainian display name
        public int cost;
        public string description;
        public TowerInfo(string k, string dn, int c, string d) { key = k; displayName = dn; cost = c; description = d; }
    }

    public static readonly TowerInfo[] TowerInfos = new TowerInfo[]
    {
        new TowerInfo("Archer",  "Лучник",       100, "Одна ціль, середня швидкість"),
        new TowerInfo("Mage",    "Маг",          150, "Область ураження (AoE)"),
        new TowerInfo("Freezer", "Заморожувач",  120, "Уповільнює ворогів"),
        new TowerInfo("Cannon",  "Гарматник",    200, "Велика шкода, повільний")
    };

    // Enemy info for attacker panel (Ukrainian)
    public struct EnemyInfo
    {
        public string key;
        public string displayName;
        public int cost;
        public string description;
        public EnemyInfo(string k, string dn, int c, string d) { key = k; displayName = dn; cost = c; description = d; }
    }

    public static readonly EnemyInfo[] EnemyInfos = new EnemyInfo[]
    {
        new EnemyInfo("Goblin", "Гоблін",  10, "Швидкий, слабкий"),
        new EnemyInfo("Orc",    "Орк",     25, "Повільний танк"),
        new EnemyInfo("Ghost",  "Привид",  20, "Ігнорує заморожувач")
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

    // Menu panel
    private GameObject menuPanel;

    // Game Over panel
    private GameObject gameOverPanel;
    private Text gameOverText;
    private Button restartBtn;

    // Round End panel
    private GameObject roundEndPanel;
    private Text roundEndInfoText;
    private Button continueBtn;

    // Attacker panel (hot-seat)
    private GameObject attackerPanel;
    private Text budgetText;
    private Text waveListText;
    private List<AIAttacker.EnemyWaveEntry> hotSeatWave = new List<AIAttacker.EnemyWaveEntry>();
    private int hotSeatBudgetUsed;

    // Wave info during battle
    private Text waveInfoText;

    // Tower panel + selection
    private GameObject towerPanelGO;
    private Text selectionText;

    private Color normalBtnColor = new Color(0.30f, 0.30f, 0.35f);
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
        CreateWaveInfoText(canvasGO.transform);
        CreateMenuPanel(canvasGO.transform);
        CreateAttackerPanel(canvasGO.transform);
        CreateRoundEndPanel(canvasGO.transform);
        CreateGameOverPanel(canvasGO.transform);
    }

    // ========== TOP BAR ==========
    void CreateTopBar(Transform parent)
    {
        var barGO = new GameObject("TopBar");
        barGO.transform.SetParent(parent, false);
        var barRect = barGO.AddComponent<RectTransform>();
        barRect.anchorMin = new Vector2(0, 1);
        barRect.anchorMax = new Vector2(1, 1);
        barRect.pivot = new Vector2(0.5f, 1);
        barRect.sizeDelta = new Vector2(0, 45);
        var barImg = barGO.AddComponent<Image>();
        barImg.color = new Color(0, 0, 0, 0.7f);

        goldText = CreateText(barGO.transform, "GoldText", "Золото: 300",
            new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f),
            new Vector2(170, 35), new Vector2(20, 0), 20, new Color(1f, 0.85f, 0.2f));

        hpText = CreateText(barGO.transform, "HPText", "HP Бази: 20/20",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(200, 35), Vector2.zero, 20, new Color(0.9f, 0.3f, 0.3f));

        roundText = CreateText(barGO.transform, "RoundText", "Раунд: 1/10",
            new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f),
            new Vector2(180, 35), new Vector2(-20, 0), 20, Color.white);

        stateText = CreateText(barGO.transform, "StateText", "",
            new Vector2(0.3f, 0.5f), new Vector2(0.3f, 0.5f), new Vector2(0.3f, 0.5f),
            new Vector2(250, 35), Vector2.zero, 16, new Color(0.7f, 0.9f, 0.7f));
    }

    // Tower-specific colors (bright for visibility)
    static readonly Color[] TowerColors = new Color[]
    {
        new Color(0.30f, 0.65f, 0.25f), // Archer — bright green
        new Color(0.55f, 0.25f, 0.80f), // Mage — bright purple
        new Color(0.30f, 0.60f, 0.85f), // Freezer — bright blue
        new Color(0.80f, 0.50f, 0.15f), // Cannon — bright orange
    };

    // Selection highlight colors (very bright)
    static readonly Color[] TowerSelectedColors = new Color[]
    {
        new Color(0.50f, 1.0f, 0.40f),  // Archer selected
        new Color(0.80f, 0.45f, 1.0f),  // Mage selected
        new Color(0.50f, 0.85f, 1.0f),  // Freezer selected
        new Color(1.0f, 0.75f, 0.30f),  // Cannon selected
    };

    // ========== TOWER PANEL ==========
    void CreateTowerPanel(Transform parent)
    {
        towerPanelGO = new GameObject("TowerPanel");
        towerPanelGO.transform.SetParent(parent, false);
        var panelRect = towerPanelGO.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 0);
        panelRect.anchorMax = new Vector2(1, 0);
        panelRect.pivot = new Vector2(0.5f, 0);
        panelRect.anchoredPosition = new Vector2(0, 0);
        panelRect.sizeDelta = new Vector2(0, 95);

        var panelImg = towerPanelGO.AddComponent<Image>();
        panelImg.color = new Color(0.08f, 0.08f, 0.12f, 0.92f);

        var layout = towerPanelGO.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 12;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        layout.padding = new RectOffset(15, 15, 5, 5);

        // "ВЕЖІ:" title label (as first child in the layout)
        var titleGO = new GameObject("TowerTitle");
        titleGO.transform.SetParent(towerPanelGO.transform, false);
        var titleRect = titleGO.AddComponent<RectTransform>();
        titleRect.sizeDelta = new Vector2(70, 80);
        var titleLE = titleGO.AddComponent<LayoutElement>();
        titleLE.preferredWidth = 70;
        titleLE.preferredHeight = 80;
        var titleText = titleGO.AddComponent<Text>();
        titleText.text = "ВЕЖІ:";
        titleText.font = uiFont;
        titleText.fontSize = 16;
        titleText.color = new Color(0.9f, 0.9f, 0.6f);
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.horizontalOverflow = HorizontalWrapMode.Overflow;

        towerButtons = new Button[TowerInfos.Length];
        towerButtonImages = new Image[TowerInfos.Length];

        for (int i = 0; i < TowerInfos.Length; i++)
        {
            int idx = i;
            var info = TowerInfos[i];

            var btnGO = new GameObject(info.key + "_Btn");
            btnGO.transform.SetParent(towerPanelGO.transform, false);
            var btnRect = btnGO.AddComponent<RectTransform>();
            btnRect.sizeDelta = new Vector2(210, 80);

            // Layout element to control size
            var le = btnGO.AddComponent<LayoutElement>();
            le.preferredWidth = 210;
            le.preferredHeight = 80;

            var btnImg = btnGO.AddComponent<Image>();
            btnImg.color = TowerColors[i];
            towerButtonImages[i] = btnImg;

            // Add Outline component for selected state visibility
            var outline = btnGO.AddComponent<Outline>();
            outline.effectColor = Color.clear;
            outline.effectDistance = new Vector2(3, 3);

            var btn = btnGO.AddComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.2f, 1.2f, 1.2f);
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f);
            colors.selectedColor = new Color(1.1f, 1.1f, 1.0f);
            btn.colors = colors;
            btn.onClick.AddListener(() => SelectTower(TowerInfos[idx].key));
            towerButtons[i] = btn;

            // Tower name (top half — larger font)
            var nameText = CreateText(btnGO.transform, "Name", info.displayName,
                new Vector2(0, 0.45f), new Vector2(1, 1), new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero, 19, Color.white);
            nameText.fontStyle = FontStyle.Bold;

            // Cost + description (bottom half)
            CreateText(btnGO.transform, "Cost", info.cost + " зол. | " + info.description,
                new Vector2(0, 0), new Vector2(1, 0.45f), new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero, 11, new Color(1f, 0.90f, 0.45f));
        }

        // Selection indicator text (above the panel)
        selectionText = CreateText(parent, "SelectionHint", "Виберіть вежу для розміщення",
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0),
            new Vector2(500, 28), new Vector2(0, 97), 15, new Color(0.7f, 1.0f, 0.7f, 0.9f));
    }

    // ========== START WAVE BUTTON ==========
    void CreateStartWaveButton(Transform parent)
    {
        var btnGO = new GameObject("StartWaveBtn");
        btnGO.transform.SetParent(parent, false);
        var btnRect = btnGO.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(1, 0);
        btnRect.anchorMax = new Vector2(1, 0);
        btnRect.pivot = new Vector2(1, 0);
        btnRect.anchoredPosition = new Vector2(-10, 100);
        btnRect.sizeDelta = new Vector2(220, 55);

        var btnImg = btnGO.AddComponent<Image>();
        btnImg.color = new Color(0.15f, 0.55f, 0.15f);

        startWaveBtn = btnGO.AddComponent<Button>();
        startWaveBtn.onClick.AddListener(OnStartWaveClicked);

        startWaveBtnText = CreateText(btnGO.transform, "Label", "ПОЧАТИ ХВИЛЮ",
            Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero, 18, Color.white);
    }

    // ========== WAVE INFO TEXT ==========
    void CreateWaveInfoText(Transform parent)
    {
        waveInfoText = CreateText(parent, "WaveInfo", "",
            new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0),
            new Vector2(300, 30), new Vector2(160, 100), 14, new Color(1f, 0.9f, 0.6f));
        waveInfoText.alignment = TextAnchor.MiddleLeft;
        waveInfoText.gameObject.SetActive(false);
    }

    // ========== MENU PANEL ==========
    void CreateMenuPanel(Transform parent)
    {
        menuPanel = new GameObject("MenuPanel");
        menuPanel.transform.SetParent(parent, false);
        var panelRect = menuPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;

        var overlay = menuPanel.AddComponent<Image>();
        overlay.color = new Color(0.05f, 0.1f, 0.05f, 0.92f);

        // Title
        CreateText(menuPanel.transform, "Title", "TOWER DEFENSE",
            new Vector2(0.5f, 0.78f), new Vector2(0.5f, 0.78f), new Vector2(0.5f, 0.5f),
            new Vector2(500, 60), Vector2.zero, 40, new Color(1f, 0.85f, 0.2f));

        CreateText(menuPanel.transform, "Subtitle", "Захист Вежами",
            new Vector2(0.5f, 0.68f), new Vector2(0.5f, 0.68f), new Vector2(0.5f, 0.5f),
            new Vector2(400, 40), Vector2.zero, 22, new Color(0.7f, 0.9f, 0.7f));

        // Single player button
        var spBtnGO = CreateButton(menuPanel.transform, "SP_Btn",
            "1 Гравець (vs Комп'ютер)",
            new Vector2(0.5f, 0.48f), new Vector2(320, 55),
            new Color(0.2f, 0.55f, 0.2f));
        spBtnGO.GetComponent<Button>().onClick.AddListener(() => OnMenuSelect(GameMode.SinglePlayer));

        // Hot-seat button
        var hsBtnGO = CreateButton(menuPanel.transform, "HS_Btn",
            "2 Гравці (Hot-Seat)",
            new Vector2(0.5f, 0.36f), new Vector2(320, 55),
            new Color(0.2f, 0.35f, 0.65f));
        hsBtnGO.GetComponent<Button>().onClick.AddListener(() => OnMenuSelect(GameMode.HotSeat));

        // Instructions
        CreateText(menuPanel.transform, "Info", "Захисник будує вежі, Атакуючий формує хвилі ворогів",
            new Vector2(0.5f, 0.22f), new Vector2(0.5f, 0.22f), new Vector2(0.5f, 0.5f),
            new Vector2(600, 35), Vector2.zero, 15, new Color(0.6f, 0.6f, 0.6f));

        menuPanel.SetActive(true);
    }

    // ========== ATTACKER PANEL (Hot-Seat) ==========
    void CreateAttackerPanel(Transform parent)
    {
        attackerPanel = new GameObject("AttackerPanel");
        attackerPanel.transform.SetParent(parent, false);
        var panelRect = attackerPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;

        var overlay = attackerPanel.AddComponent<Image>();
        overlay.color = new Color(0.12f, 0.05f, 0.05f, 0.9f);

        CreateText(attackerPanel.transform, "Title", "ХІД АТАКУЮЧОГО",
            new Vector2(0.5f, 0.88f), new Vector2(0.5f, 0.88f), new Vector2(0.5f, 0.5f),
            new Vector2(400, 50), Vector2.zero, 28, new Color(1f, 0.4f, 0.4f));

        budgetText = CreateText(attackerPanel.transform, "Budget", "Бюджет: 200",
            new Vector2(0.5f, 0.80f), new Vector2(0.5f, 0.80f), new Vector2(0.5f, 0.5f),
            new Vector2(300, 35), Vector2.zero, 20, new Color(1f, 0.85f, 0.2f));

        // Enemy add buttons
        for (int i = 0; i < EnemyInfos.Length; i++)
        {
            int idx = i;
            var info = EnemyInfos[i];
            float yPos = 0.65f - i * 0.12f;

            // Add button
            var addBtnGO = CreateButton(attackerPanel.transform, "Add_" + info.key,
                "+ " + info.displayName + " (" + info.cost + " очок)",
                new Vector2(0.35f, yPos), new Vector2(260, 45),
                new Color(0.25f, 0.50f, 0.25f));
            addBtnGO.GetComponent<Button>().onClick.AddListener(() => AddEnemyToWave(idx));

            // Remove button
            var remBtnGO = CreateButton(attackerPanel.transform, "Rem_" + info.key,
                "- Прибрати",
                new Vector2(0.65f, yPos), new Vector2(180, 45),
                new Color(0.55f, 0.25f, 0.25f));
            remBtnGO.GetComponent<Button>().onClick.AddListener(() => RemoveEnemyFromWave(idx));
        }

        // Wave list text
        waveListText = CreateText(attackerPanel.transform, "WaveList", "Хвиля порожня",
            new Vector2(0.5f, 0.22f), new Vector2(0.5f, 0.22f), new Vector2(0.5f, 0.5f),
            new Vector2(550, 55), Vector2.zero, 16, Color.white);

        // Submit button
        var submitBtnGO = CreateButton(attackerPanel.transform, "SubmitWave",
            "ЗАПУСТИТИ ХВИЛЮ",
            new Vector2(0.5f, 0.10f), new Vector2(280, 55),
            new Color(0.7f, 0.2f, 0.2f));
        submitBtnGO.GetComponent<Button>().onClick.AddListener(OnSubmitWave);

        attackerPanel.SetActive(false);
    }

    // ========== ROUND END PANEL ==========
    void CreateRoundEndPanel(Transform parent)
    {
        roundEndPanel = new GameObject("RoundEndPanel");
        roundEndPanel.transform.SetParent(parent, false);
        var panelRect = roundEndPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.2f, 0.2f);
        panelRect.anchorMax = new Vector2(0.8f, 0.8f);
        panelRect.sizeDelta = Vector2.zero;

        var bg = roundEndPanel.AddComponent<Image>();
        bg.color = new Color(0.08f, 0.12f, 0.08f, 0.95f);

        CreateText(roundEndPanel.transform, "Title", "РАУНД ЗАВЕРШЕНО!",
            new Vector2(0.5f, 0.85f), new Vector2(0.5f, 0.85f), new Vector2(0.5f, 0.5f),
            new Vector2(400, 45), Vector2.zero, 26, new Color(0.3f, 1f, 0.3f));

        roundEndInfoText = CreateText(roundEndPanel.transform, "Info", "",
            new Vector2(0.5f, 0.50f), new Vector2(0.5f, 0.50f), new Vector2(0.5f, 0.5f),
            new Vector2(450, 140), Vector2.zero, 18, Color.white);

        var contBtnGO = CreateButton(roundEndPanel.transform, "ContinueBtn",
            "ДАЛІ",
            new Vector2(0.5f, 0.12f), new Vector2(200, 50),
            new Color(0.2f, 0.55f, 0.2f));
        continueBtn = contBtnGO.GetComponent<Button>();
        continueBtn.onClick.AddListener(OnContinueClicked);

        roundEndPanel.SetActive(false);
    }

    // ========== GAME OVER PANEL ==========
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

        gameOverText = CreateText(gameOverPanel.transform, "GameOverText", "ГРА ЗАВЕРШЕНА",
            new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.5f),
            new Vector2(500, 60), Vector2.zero, 36, Color.white);

        // Restart button
        var restartGO = CreateButton(gameOverPanel.transform, "RestartBtn",
            "ПЕРЕЗАПУСК",
            new Vector2(0.5f, 0.4f), new Vector2(220, 55),
            new Color(0.3f, 0.5f, 0.8f));
        restartBtn = restartGO.GetComponent<Button>();
        restartBtn.onClick.AddListener(OnRestartClicked);

        // Back to menu button
        var menuBtnGO = CreateButton(gameOverPanel.transform, "MenuBtn",
            "ГОЛОВНЕ МЕНЮ",
            new Vector2(0.5f, 0.28f), new Vector2(220, 55),
            new Color(0.5f, 0.3f, 0.3f));
        menuBtnGO.GetComponent<Button>().onClick.AddListener(OnBackToMenu);

        gameOverPanel.SetActive(false);
    }

    // ========== HELPER: Create Button ==========
    GameObject CreateButton(Transform parent, string name, string label,
        Vector2 anchor, Vector2 size, Color bgColor)
    {
        var btnGO = new GameObject(name);
        btnGO.transform.SetParent(parent, false);
        var btnRect = btnGO.AddComponent<RectTransform>();
        btnRect.anchorMin = anchor;
        btnRect.anchorMax = anchor;
        btnRect.pivot = new Vector2(0.5f, 0.5f);
        btnRect.sizeDelta = size;
        var btnImg = btnGO.AddComponent<Image>();
        btnImg.color = bgColor;
        btnGO.AddComponent<Button>();
        CreateText(btnGO.transform, "Label", label,
            Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero, 18, Color.white);
        return btnGO;
    }

    // ========== HELPER: Create Text ==========
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

    // ========== TOWER SELECTION ==========
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
            bool selected = TowerInfos[i].key == SelectedTowerType;

            // Update button outline for selection
            var outline = towerButtons[i].GetComponent<Outline>();

            if (selected)
            {
                towerButtonImages[i].color = TowerSelectedColors[i];
                if (outline != null)
                    outline.effectColor = Color.white;
            }
            else if (affordable)
            {
                towerButtonImages[i].color = TowerColors[i];
                if (outline != null)
                    outline.effectColor = Color.clear;
            }
            else
            {
                towerButtonImages[i].color = disabledBtnColor;
                if (outline != null)
                    outline.effectColor = Color.clear;
            }

            towerButtons[i].interactable = canBuy;
        }

        // Update selection hint text
        if (selectionText != null)
        {
            if (!canBuy)
            {
                selectionText.text = "";
            }
            else if (string.IsNullOrEmpty(SelectedTowerType))
            {
                selectionText.text = "<<< Виберіть вежу для розміщення >>>";
                selectionText.color = new Color(0.7f, 1.0f, 0.7f, 0.9f);
            }
            else
            {
                string displayName = SelectedTowerType;
                foreach (var info in TowerInfos)
                    if (info.key == SelectedTowerType) { displayName = info.displayName; break; }
                selectionText.text = ">>> Вибрано: " + displayName + " — клікніть на зелене поле <<<";
                selectionText.color = new Color(1f, 1f, 0.3f, 1.0f);
            }
        }
    }

    // ========== ACTIONS ==========
    void OnMenuSelect(GameMode mode)
    {
        menuPanel.SetActive(false);
        if (GameManager.Instance != null)
            GameManager.Instance.StartGame(mode);
    }

    void OnStartWaveClicked()
    {
        if (GameManager.Instance != null)
        {
            SelectedTowerType = null;
            GameManager.Instance.StartBattle();
        }
    }

    void OnContinueClicked()
    {
        roundEndPanel.SetActive(false);
        if (GameManager.Instance != null)
            GameManager.Instance.ContinueAfterRoundEnd();
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

    void OnBackToMenu()
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

        if (GridManager.Instance != null)
            GridManager.Instance.ResetTowers();

        SelectedTowerType = null;
        gameOverPanel.SetActive(false);
        roundEndPanel.SetActive(false);
        attackerPanel.SetActive(false);
        menuPanel.SetActive(true);
    }

    // ========== ATTACKER HOT-SEAT ==========
    void AddEnemyToWave(int enemyIdx)
    {
        if (GameManager.Instance == null) return;
        var info = EnemyInfos[enemyIdx];
        int budget = GameManager.Instance.AttackBudget;

        if (hotSeatBudgetUsed + info.cost > budget) return;
        if (hotSeatWave.Count >= 50) return;

        hotSeatWave.Add(new AIAttacker.EnemyWaveEntry(info.key));
        hotSeatBudgetUsed += info.cost;
        UpdateAttackerPanel();
    }

    void RemoveEnemyFromWave(int enemyIdx)
    {
        var info = EnemyInfos[enemyIdx];
        // Remove last occurrence of this enemy type
        for (int i = hotSeatWave.Count - 1; i >= 0; i--)
        {
            if (hotSeatWave[i].enemyType == info.key)
            {
                hotSeatWave.RemoveAt(i);
                hotSeatBudgetUsed -= info.cost;
                break;
            }
        }
        UpdateAttackerPanel();
    }

    void UpdateAttackerPanel()
    {
        if (GameManager.Instance == null) return;
        int budget = GameManager.Instance.AttackBudget;
        int remaining = budget - hotSeatBudgetUsed;
        budgetText.text = "Бюджет: " + remaining + "/" + budget + " очок";

        if (hotSeatWave.Count == 0)
        {
            waveListText.text = "Хвиля порожня — додайте ворогів!";
        }
        else
        {
            int goblins = 0, orcs = 0, ghosts = 0;
            foreach (var e in hotSeatWave)
            {
                switch (e.enemyType)
                {
                    case "Goblin": goblins++; break;
                    case "Orc": orcs++; break;
                    case "Ghost": ghosts++; break;
                }
            }
            string info = "Хвиля (" + hotSeatWave.Count + " ворогів): ";
            if (goblins > 0) info += "Гоблінів:" + goblins + " ";
            if (orcs > 0) info += "Орків:" + orcs + " ";
            if (ghosts > 0) info += "Привидів:" + ghosts;
            waveListText.text = info;
        }
    }

    void OnSubmitWave()
    {
        if (hotSeatWave.Count == 0) return;
        if (GameManager.Instance == null) return;

        attackerPanel.SetActive(false);

        // Shuffle for variety
        for (int i = hotSeatWave.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            var tmp = hotSeatWave[i];
            hotSeatWave[i] = hotSeatWave[j];
            hotSeatWave[j] = tmp;
        }

        var waveCopy = new List<AIAttacker.EnemyWaveEntry>(hotSeatWave);
        GameManager.Instance.StartBattle(); // transitions AttackerTurn → Battle
        GameManager.Instance.LaunchWave(waveCopy);
    }

    // ========== STATE CHANGES ==========
    void OnGameStateChanged(GameState state)
    {
        if (stateText == null) return;

        // Always manage tower panel and selection text visibility
        bool showTowerPanel = (state == GameState.Preparation);
        if (towerPanelGO != null)
            towerPanelGO.SetActive(showTowerPanel);
        if (selectionText != null)
            selectionText.gameObject.SetActive(showTowerPanel);

        switch (state)
        {
            case GameState.Menu:
                stateText.text = "";
                if (startWaveBtn != null)
                    startWaveBtn.gameObject.SetActive(false);
                if (gameOverPanel != null)
                    gameOverPanel.SetActive(false);
                if (roundEndPanel != null)
                    roundEndPanel.SetActive(false);
                if (attackerPanel != null)
                    attackerPanel.SetActive(false);
                if (waveInfoText != null)
                    waveInfoText.gameObject.SetActive(false);
                if (menuPanel != null)
                    menuPanel.SetActive(true);
                break;

            case GameState.Preparation:
                stateText.text = "Розміщуйте вежі!";
                if (startWaveBtn != null)
                {
                    startWaveBtn.gameObject.SetActive(true);
                    startWaveBtnText.text = "ПОЧАТИ ХВИЛЮ";
                }
                if (gameOverPanel != null)
                    gameOverPanel.SetActive(false);
                if (roundEndPanel != null)
                    roundEndPanel.SetActive(false);
                if (attackerPanel != null)
                    attackerPanel.SetActive(false);
                if (waveInfoText != null)
                    waveInfoText.gameObject.SetActive(false);
                if (menuPanel != null)
                    menuPanel.SetActive(false);
                break;

            case GameState.AttackerTurn:
                stateText.text = "Хід Атакуючого";
                if (startWaveBtn != null)
                    startWaveBtn.gameObject.SetActive(false);
                // Reset attacker wave
                hotSeatWave.Clear();
                hotSeatBudgetUsed = 0;
                if (attackerPanel != null)
                {
                    attackerPanel.SetActive(true);
                    UpdateAttackerPanel();
                }
                break;

            case GameState.Battle:
                stateText.text = "Бій!";
                if (startWaveBtn != null)
                    startWaveBtn.gameObject.SetActive(false);
                if (attackerPanel != null)
                    attackerPanel.SetActive(false);
                if (waveInfoText != null)
                    waveInfoText.gameObject.SetActive(true);
                break;

            case GameState.RoundEnd:
                stateText.text = "Раунд завершено!";
                if (startWaveBtn != null)
                    startWaveBtn.gameObject.SetActive(false);
                if (waveInfoText != null)
                    waveInfoText.gameObject.SetActive(false);
                ShowRoundEndPanel();
                break;

            case GameState.GameOver:
                stateText.text = "";
                if (startWaveBtn != null)
                    startWaveBtn.gameObject.SetActive(false);
                if (waveInfoText != null)
                    waveInfoText.gameObject.SetActive(false);
                if (roundEndPanel != null)
                    roundEndPanel.SetActive(false);
                if (gameOverPanel != null)
                {
                    gameOverPanel.SetActive(true);
                    bool won = GameManager.Instance != null && GameManager.Instance.DefenderWon;
                    gameOverText.text = won ? "ЗАХИСНИК ПЕРЕМІГ!" : "АТАКУЮЧИЙ ПЕРЕМІГ!";
                    gameOverText.color = won ? new Color(0.3f, 1f, 0.3f) : new Color(1f, 0.3f, 0.3f);
                }
                break;
        }

        UpdateButtonVisuals();
    }

    void ShowRoundEndPanel()
    {
        if (roundEndPanel == null || GameManager.Instance == null) return;

        var gm = GameManager.Instance;
        string info = "HP Бази: " + gm.BaseHP + "/" + gm.MaxBaseHP + "\n\n";
        info += "Бонусне золото: +" + gm.LastBonusGold + "\n";
        info += "Бюджет атаки наступного раунду: " + gm.AttackBudget + "\n\n";
        if (gm.Round <= GameManager.MaxRounds)
            info += "Наступний раунд: " + gm.Round + "/" + GameManager.MaxRounds;
        roundEndInfoText.text = info;

        roundEndPanel.SetActive(true);
    }

    void UpdateStatsUI()
    {
        if (GameManager.Instance == null) return;

        if (goldText != null)
            goldText.text = "Золото: " + GameManager.Instance.Gold;
        if (hpText != null)
            hpText.text = "HP Бази: " + GameManager.Instance.BaseHP + "/" + GameManager.Instance.MaxBaseHP;
        if (roundText != null)
            roundText.text = "Раунд: " + GameManager.Instance.Round + "/" + GameManager.MaxRounds;

        UpdateButtonVisuals();
    }

    void Update()
    {
        // Update wave info during battle
        if (GameManager.Instance != null && GameManager.Instance.State == GameState.Battle
            && waveInfoText != null && waveInfoText.gameObject.activeSelf)
        {
            int alive = EnemyMovement.ActiveEnemies.Count;
            waveInfoText.text = "Ворогів на полі: " + alive;
        }
    }

    public int GetTowerCost(string towerName)
    {
        foreach (var info in TowerInfos)
            if (info.key == towerName)
                return info.cost;
        return 0;
    }
}
