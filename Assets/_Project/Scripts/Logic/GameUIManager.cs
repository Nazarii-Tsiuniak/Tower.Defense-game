using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance;

    public string SelectedTowerType { get; private set; }

    // ── Palette ──────────────────────────────────────────────────────────
    static readonly Color C_PanelBg      = new Color(0.08f, 0.06f, 0.04f, 0.97f);
    static readonly Color C_PanelBorder  = new Color(0.72f, 0.55f, 0.18f, 1.00f); // gold
    static readonly Color C_Gold         = new Color(1.00f, 0.85f, 0.25f, 1.00f);
    static readonly Color C_Red          = new Color(0.95f, 0.28f, 0.22f, 1.00f);
    static readonly Color C_White        = new Color(0.95f, 0.92f, 0.85f, 1.00f);
    static readonly Color C_Muted        = new Color(0.62f, 0.58f, 0.48f, 1.00f);
    static readonly Color C_Green        = new Color(0.30f, 0.90f, 0.35f, 1.00f);
    static readonly Color C_MenuOverlay  = new Color(0.04f, 0.03f, 0.02f, 0.96f);
    static readonly Color C_BtnHover     = new Color(1.15f, 1.10f, 0.90f, 1.00f);
    static readonly Color C_BtnPress     = new Color(0.75f, 0.70f, 0.55f, 1.00f);

    // Tower button accent colours (fill + selected)
    static readonly Color[] TowerColors = {
        new Color(0.28f, 0.62f, 0.22f),  // Archer  – forest green
        new Color(0.52f, 0.22f, 0.78f),  // Mage    – arcane purple
        new Color(0.22f, 0.55f, 0.82f),  // Freezer – ice blue
        new Color(0.80f, 0.45f, 0.12f),  // Cannon  – fire orange
    };
    static readonly Color[] TowerSelectedColors = {
        new Color(0.48f, 1.00f, 0.38f),
        new Color(0.80f, 0.45f, 1.00f),
        new Color(0.48f, 0.85f, 1.00f),
        new Color(1.00f, 0.72f, 0.28f),
    };
    // Rune-style emoji icons per tower
    static readonly string[] TowerIcons = { "🏹", "✨", "❄️", "💣" };

    // Tower configs for UI (Ukrainian names)
    public struct TowerInfo
    {
        public string key;
        public string displayName;
        public int cost;
        public string description;
        public TowerInfo(string k, string dn, int c, string d) { key = k; displayName = dn; cost = c; description = d; }
    }

    public static readonly TowerInfo[] TowerInfos = new TowerInfo[]
    {
        new TowerInfo("Archer",  "Лучник",        75,  "Одна ціль, середня швидкість"),
        new TowerInfo("Mage",    "Маг",          175,  "Область ураження (AoE)"),
        new TowerInfo("Freezer", "Заморожувач",  125,  "Уповільнює ворогів"),
        new TowerInfo("Cannon",  "Гарматник",    275,  "Велика шкода, повільний")
    };

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

    private GameObject menuPanel;

    private GameObject gameOverPanel;
    private Text gameOverText;
    private Button restartBtn;

    private GameObject roundEndPanel;
    private Text roundEndInfoText;
    private Button continueBtn;

    private GameObject attackerPanel;
    private Text budgetText;
    private Text waveListText;
    private List<AIAttacker.EnemyWaveEntry> hotSeatWave = new List<AIAttacker.EnemyWaveEntry>();
    private int hotSeatBudgetUsed;

    private Text waveInfoText;

    private GameObject towerPanelGO;
    private Text selectionText;

    private Color disabledBtnColor = new Color(0.18f, 0.16f, 0.14f);
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

    // ─── Shared: styled bordered panel ───────────────────────────────────────
    // Creates a dark panel with golden outline borders (4 image lines).
    void AddGoldBorder(Transform parent, float w, float h)
    {
        float t = 3f; // border thickness
        // top, bottom, left, right
        float[] bx = { 0, 0, -w / 2f + t / 2f, w / 2f - t / 2f };
        float[] by = { h / 2f - t / 2f, -h / 2f + t / 2f, 0, 0 };
        float[] bw = { w, w, t, t };
        float[] bh = { t, t, h, h };
        string[] names = { "B_Top", "B_Bot", "B_Left", "B_Right" };
        for (int i = 0; i < 4; i++)
        {
            var go = new GameObject(names[i]);
            go.transform.SetParent(parent, false);
            var r = go.AddComponent<RectTransform>();
            r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
            r.pivot = new Vector2(0.5f, 0.5f);
            r.sizeDelta = new Vector2(bw[i], bh[i]);
            r.anchoredPosition = new Vector2(bx[i], by[i]);
            go.AddComponent<Image>().color = C_PanelBorder;
        }
    }

    // ─── Shared: styled action button ────────────────────────────────────────
    // Dark bg + gold outline + hover transition via ColorBlock.
    Button MakeStyledButton(Transform parent, string name, string label,
        Vector2 anchor, Vector2 size, Color accentColor, int fontSize = 18)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var r = go.AddComponent<RectTransform>();
        r.anchorMin = r.anchorMax = anchor;
        r.pivot = new Vector2(0.5f, 0.5f);
        r.sizeDelta = size;

        var bg = go.AddComponent<Image>();
        bg.color = accentColor;

        var btn = go.AddComponent<Button>();
        var cb = btn.colors;
        cb.normalColor      = Color.white;
        cb.highlightedColor = C_BtnHover;
        cb.pressedColor     = C_BtnPress;
        cb.selectedColor    = Color.white;
        btn.colors = cb;

        // Gold border lines
        float t = 2f;
        AddThinBorder(go.transform, size, C_PanelBorder, t);

        CreateText(go.transform, "Label", label,
            Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
            new Vector2(-6f, -6f), Vector2.zero, fontSize, C_White);

        return btn;
    }

    void AddThinBorder(Transform parent, Vector2 size, Color col, float t)
    {
        float w = size.x, h = size.y;
        float[] bx = { 0, 0, -w / 2f + t / 2f, w / 2f - t / 2f };
        float[] by = { h / 2f - t / 2f, -h / 2f + t / 2f, 0, 0 };
        float[] bw = { w, w, t, t };
        float[] bh = { t, t, h, h };
        string[] ns = { "Bt", "Bb", "Bl", "Br" };
        for (int i = 0; i < 4; i++)
        {
            var g = new GameObject(ns[i]);
            g.transform.SetParent(parent, false);
            var rr = g.AddComponent<RectTransform>();
            rr.anchorMin = rr.anchorMax = new Vector2(0.5f, 0.5f);
            rr.pivot = new Vector2(0.5f, 0.5f);
            rr.sizeDelta = new Vector2(bw[i], bh[i]);
            rr.anchoredPosition = new Vector2(bx[i], by[i]);
            g.AddComponent<Image>().color = col;
        }
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
        barRect.sizeDelta = new Vector2(0, 52);

        // Dark stone background
        var barImg = barGO.AddComponent<Image>();
        barImg.color = new Color(0.07f, 0.05f, 0.03f, 0.97f);

        // Bottom golden rule
        var rule = new GameObject("BottomRule");
        rule.transform.SetParent(barGO.transform, false);
        var rr = rule.AddComponent<RectTransform>();
        rr.anchorMin = new Vector2(0, 0); rr.anchorMax = new Vector2(1, 0);
        rr.pivot = new Vector2(0.5f, 1); rr.sizeDelta = new Vector2(0, 2);
        rule.AddComponent<Image>().color = C_PanelBorder;

        // ⚔️ Gold icon + Gold amount (left)
        CreateText(barGO.transform, "GoldIcon", "⚔",
            new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f),
            new Vector2(30, 40), new Vector2(14, 0), 22, C_Gold);

        goldText = CreateText(barGO.transform, "GoldText", "300",
            new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f),
            new Vector2(120, 40), new Vector2(46, 0), 20, C_Gold);

        // ❤️ HP bar section (center-left)
        CreateText(barGO.transform, "HPIcon", "🏰",
            new Vector2(0.3f, 0.5f), new Vector2(0.3f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(30, 40), new Vector2(-110, 0), 22, C_Red);

        hpText = CreateText(barGO.transform, "HPText", "HP: 20/20",
            new Vector2(0.3f, 0.5f), new Vector2(0.3f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(160, 40), new Vector2(0, 0), 18, C_Red);

        // Round info (center)
        roundText = CreateText(barGO.transform, "RoundText", "Раунд: 1/10",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(200, 40), Vector2.zero, 18, C_White);

        // State text (right-center)
        stateText = CreateText(barGO.transform, "StateText", "",
            new Vector2(0.72f, 0.5f), new Vector2(0.72f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(260, 40), Vector2.zero, 15, new Color(0.60f, 0.90f, 0.60f));
    }

    // ========== TOWER PANEL ==========
    void CreateTowerPanel(Transform parent)
    {
        towerPanelGO = new GameObject("TowerPanel");
        towerPanelGO.transform.SetParent(parent, false);
        var panelRect = towerPanelGO.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 0);
        panelRect.anchorMax = new Vector2(1, 0);
        panelRect.pivot = new Vector2(0.5f, 0);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(0, 100);

        var panelImg = towerPanelGO.AddComponent<Image>();
        panelImg.color = new Color(0.07f, 0.05f, 0.03f, 0.97f);

        // Top golden rule
        var rule = new GameObject("TopRule");
        rule.transform.SetParent(towerPanelGO.transform, false);
        var rr = rule.AddComponent<RectTransform>();
        rr.anchorMin = new Vector2(0, 1); rr.anchorMax = new Vector2(1, 1);
        rr.pivot = new Vector2(0.5f, 0); rr.sizeDelta = new Vector2(0, 2);
        rule.AddComponent<Image>().color = C_PanelBorder;

        var layout = towerPanelGO.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 14;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        layout.padding = new RectOffset(16, 16, 8, 8);

        // "ВЕЖІ" title
        var titleGO = new GameObject("TowerTitle");
        titleGO.transform.SetParent(towerPanelGO.transform, false);
        var titleLE = titleGO.AddComponent<LayoutElement>();
        titleLE.preferredWidth = 72; titleLE.preferredHeight = 84;
        var titleText = titleGO.AddComponent<Text>();
        titleText.text = "🏰\nВЕЖІ";
        titleText.font = uiFont;
        titleText.fontSize = 14;
        titleText.fontStyle = FontStyle.Bold;
        titleText.color = C_Gold;
        titleText.alignment = TextAnchor.MiddleCenter;

        towerButtons = new Button[TowerInfos.Length];
        towerButtonImages = new Image[TowerInfos.Length];

        for (int i = 0; i < TowerInfos.Length; i++)
        {
            int idx = i;
            var info = TowerInfos[i];

            var btnGO = new GameObject(info.key + "_Btn");
            btnGO.transform.SetParent(towerPanelGO.transform, false);

            var le = btnGO.AddComponent<LayoutElement>();
            le.preferredWidth = 218; le.preferredHeight = 84;

            var btnImg = btnGO.AddComponent<Image>();
            btnImg.color = TowerColors[i];
            towerButtonImages[i] = btnImg;

            // Gold border (thin)
            var outline = btnGO.AddComponent<Outline>();
            outline.effectColor = new Color(0.55f, 0.42f, 0.10f, 0.80f);
            outline.effectDistance = new Vector2(2, 2);

            var btn = btnGO.AddComponent<Button>();
            var cb = btn.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = C_BtnHover;
            cb.pressedColor = C_BtnPress;
            btn.colors = cb;
            btn.onClick.AddListener(() => SelectTower(TowerInfos[idx].key));
            towerButtons[i] = btn;

            // Icon row (top)
            var iconGO = new GameObject("Icon");
            iconGO.transform.SetParent(btnGO.transform, false);
            var iconRect = iconGO.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0, 0.52f);
            iconRect.anchorMax = new Vector2(0.28f, 1f);
            var iconText = iconGO.AddComponent<Text>();
            iconText.text = TowerIcons[i];
            iconText.font = uiFont;
            iconText.fontSize = 28;
            iconText.alignment = TextAnchor.MiddleCenter;
            iconText.color = C_White;

            // Name
            var nameText = CreateText(btnGO.transform, "Name", info.displayName,
                new Vector2(0.28f, 0.50f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero, 18, C_White);
            nameText.fontStyle = FontStyle.Bold;

            // Cost bar at bottom
            var costBar = new GameObject("CostBar");
            costBar.transform.SetParent(btnGO.transform, false);
            var cbRect = costBar.AddComponent<RectTransform>();
            cbRect.anchorMin = Vector2.zero; cbRect.anchorMax = new Vector2(1, 0.42f);
            cbRect.sizeDelta = Vector2.zero;
            var cbImg = costBar.AddComponent<Image>();
            cbImg.color = new Color(0f, 0f, 0f, 0.35f);

            CreateText(btnGO.transform, "Cost",
                "⚔ " + info.cost + " зол.  |  " + info.description,
                new Vector2(0, 0), new Vector2(1, 0.42f), new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero, 11, C_Gold);
        }

        // Selection hint (above panel)
        selectionText = CreateText(parent, "SelectionHint", "",
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0),
            new Vector2(560, 26), new Vector2(0, 103), 14, new Color(0.75f, 1.0f, 0.65f, 0.95f));
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
        btnRect.anchoredPosition = new Vector2(-12, 106);
        btnRect.sizeDelta = new Vector2(230, 56);

        var btnImg = btnGO.AddComponent<Image>();
        btnImg.color = new Color(0.18f, 0.48f, 0.12f);

        AddThinBorder(btnGO.transform, new Vector2(230, 56), C_PanelBorder, 2f);

        startWaveBtn = btnGO.AddComponent<Button>();
        var cb = startWaveBtn.colors;
        cb.normalColor = Color.white;
        cb.highlightedColor = C_BtnHover;
        cb.pressedColor = C_BtnPress;
        startWaveBtn.colors = cb;
        startWaveBtn.onClick.AddListener(OnStartWaveClicked);

        // Decorative left arrow
        CreateText(btnGO.transform, "Arrow", "▶",
            new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f),
            new Vector2(24, 40), new Vector2(10, 0), 20, C_Gold);

        startWaveBtnText = CreateText(btnGO.transform, "Label", "ПОЧАТИ ХВИЛЮ",
            Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
            new Vector2(-8, 0), new Vector2(8, 0), 17, C_White);
        startWaveBtnText.fontStyle = FontStyle.Bold;
    }

    // ========== WAVE INFO TEXT ==========
    void CreateWaveInfoText(Transform parent)
    {
        waveInfoText = CreateText(parent, "WaveInfo", "",
            new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0),
            new Vector2(300, 30), new Vector2(160, 108), 14, C_Gold);
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

        // Full-screen dark overlay
        var overlay = menuPanel.AddComponent<Image>();
        overlay.color = C_MenuOverlay;

        // ── decorative horizontal golden rules ──
        for (int i = 0; i < 2; i++)
        {
            var rule = new GameObject("HRule" + i);
            rule.transform.SetParent(menuPanel.transform, false);
            var rr = rule.AddComponent<RectTransform>();
            float y = i == 0 ? 0.86f : 0.14f;
            rr.anchorMin = new Vector2(0.12f, y);
            rr.anchorMax = new Vector2(0.88f, y);
            rr.sizeDelta = new Vector2(0, 2);
            rule.AddComponent<Image>().color = C_PanelBorder;
        }

        // Corner diamond ornaments (fake via text chars)
        string[] diamonds = { "◆", "◆", "◆", "◆" };
        float[] dx = { 0.11f, 0.89f, 0.11f, 0.89f };
        float[] dy = { 0.86f, 0.86f, 0.14f, 0.14f };
        for (int i = 0; i < 4; i++)
        {
            var d = CreateText(menuPanel.transform, "Diamond" + i, diamonds[i],
                new Vector2(dx[i], dy[i]), new Vector2(dx[i], dy[i]), new Vector2(0.5f, 0.5f),
                new Vector2(24, 24), Vector2.zero, 18, C_PanelBorder);
        }

        // ── TITLE ──
        // Shadow
        CreateText(menuPanel.transform, "TitleShadow", "⚔  TOWER DEFENSE  ⚔",
            new Vector2(0.5f, 0.78f), new Vector2(0.5f, 0.78f), new Vector2(0.5f, 0.5f),
            new Vector2(560, 64), new Vector2(2, -2), 40, new Color(0, 0, 0, 0.8f));
        // Main
        CreateText(menuPanel.transform, "Title", "⚔  TOWER DEFENSE  ⚔",
            new Vector2(0.5f, 0.78f), new Vector2(0.5f, 0.78f), new Vector2(0.5f, 0.5f),
            new Vector2(560, 64), Vector2.zero, 40, C_Gold);

        CreateText(menuPanel.transform, "Subtitle", "— Захист Фортеці —",
            new Vector2(0.5f, 0.68f), new Vector2(0.5f, 0.68f), new Vector2(0.5f, 0.5f),
            new Vector2(420, 38), Vector2.zero, 22, C_Muted);

        // ── Buttons box ──
        // 1-player
        var spBtn = MakeStyledButton(menuPanel.transform, "SP_Btn",
            "⚔   1 Гравець   (vs Комп'ютер)",
            new Vector2(0.5f, 0.50f), new Vector2(360, 58),
            new Color(0.16f, 0.42f, 0.14f), 18);
        spBtn.onClick.AddListener(() => OnMenuSelect(GameMode.SinglePlayer));

        // 2-player
        var hsBtn = MakeStyledButton(menuPanel.transform, "HS_Btn",
            "🛡   2 Гравці   (Hot-Seat)",
            new Vector2(0.5f, 0.36f), new Vector2(360, 58),
            new Color(0.14f, 0.28f, 0.55f), 18);
        hsBtn.onClick.AddListener(() => OnMenuSelect(GameMode.HotSeat));

        // Instructions
        CreateText(menuPanel.transform, "Info",
            "Захисник будує вежі · Атакуючий формує хвилі ворогів",
            new Vector2(0.5f, 0.22f), new Vector2(0.5f, 0.22f), new Vector2(0.5f, 0.5f),
            new Vector2(640, 34), Vector2.zero, 15, C_Muted);

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
        overlay.color = new Color(0.07f, 0.02f, 0.02f, 0.97f);

        // Gold rule top
        var rule = new GameObject("TopRule");
        rule.transform.SetParent(attackerPanel.transform, false);
        var rr = rule.AddComponent<RectTransform>();
        rr.anchorMin = new Vector2(0.1f, 0.86f); rr.anchorMax = new Vector2(0.9f, 0.86f);
        rr.sizeDelta = new Vector2(0, 2); rule.AddComponent<Image>().color = C_PanelBorder;

        // Shadow + Title
        CreateText(attackerPanel.transform, "TitleShadow", "⚡  ХІД АТАКУЮЧОГО  ⚡",
            new Vector2(0.5f, 0.89f), new Vector2(0.5f, 0.89f), new Vector2(0.5f, 0.5f),
            new Vector2(480, 54), new Vector2(2, -2), 28, new Color(0, 0, 0, 0.8f));
        CreateText(attackerPanel.transform, "Title", "⚡  ХІД АТАКУЮЧОГО  ⚡",
            new Vector2(0.5f, 0.89f), new Vector2(0.5f, 0.89f), new Vector2(0.5f, 0.5f),
            new Vector2(480, 54), Vector2.zero, 28, C_Red);

        budgetText = CreateText(attackerPanel.transform, "Budget", "Бюджет: 200",
            new Vector2(0.5f, 0.80f), new Vector2(0.5f, 0.80f), new Vector2(0.5f, 0.5f),
            new Vector2(320, 36), Vector2.zero, 20, C_Gold);

        // Enemy add/remove buttons
        for (int i = 0; i < EnemyInfos.Length; i++)
        {
            int idx = i;
            var info = EnemyInfos[i];
            float yPos = 0.65f - i * 0.13f;

            var addBtn = MakeStyledButton(attackerPanel.transform, "Add_" + info.key,
                "+ " + info.displayName + "  (" + info.cost + " очок)",
                new Vector2(0.35f, yPos), new Vector2(270, 46),
                new Color(0.18f, 0.44f, 0.18f), 16);
            addBtn.onClick.AddListener(() => AddEnemyToWave(idx));

            var remBtn = MakeStyledButton(attackerPanel.transform, "Rem_" + info.key,
                "✖ Прибрати",
                new Vector2(0.65f, yPos), new Vector2(180, 46),
                new Color(0.50f, 0.18f, 0.18f), 16);
            remBtn.onClick.AddListener(() => RemoveEnemyFromWave(idx));
        }

        waveListText = CreateText(attackerPanel.transform, "WaveList", "Хвиля порожня",
            new Vector2(0.5f, 0.22f), new Vector2(0.5f, 0.22f), new Vector2(0.5f, 0.5f),
            new Vector2(560, 58), Vector2.zero, 16, C_White);

        var submitBtn = MakeStyledButton(attackerPanel.transform, "SubmitWave",
            "▶  ЗАПУСТИТИ ХВИЛЮ",
            new Vector2(0.5f, 0.10f), new Vector2(290, 56),
            new Color(0.62f, 0.14f, 0.14f), 18);
        submitBtn.onClick.AddListener(OnSubmitWave);

        attackerPanel.SetActive(false);
    }

    // ========== ROUND END PANEL ==========
    void CreateRoundEndPanel(Transform parent)
    {
        roundEndPanel = new GameObject("RoundEndPanel");
        roundEndPanel.transform.SetParent(parent, false);
        var panelRect = roundEndPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.25f, 0.22f);
        panelRect.anchorMax = new Vector2(0.75f, 0.82f);
        panelRect.sizeDelta = Vector2.zero;

        var bg = roundEndPanel.AddComponent<Image>();
        bg.color = C_PanelBg;

        // Gold border
        AddGoldBorder(roundEndPanel.transform, 640, 432);

        // Shadow + Title
        CreateText(roundEndPanel.transform, "TitleShadow", "✦  РАУНД ЗАВЕРШЕНО  ✦",
            new Vector2(0.5f, 0.87f), new Vector2(0.5f, 0.87f), new Vector2(0.5f, 0.5f),
            new Vector2(430, 46), new Vector2(2, -2), 26, new Color(0, 0, 0, 0.8f));
        CreateText(roundEndPanel.transform, "Title", "✦  РАУНД ЗАВЕРШЕНО  ✦",
            new Vector2(0.5f, 0.87f), new Vector2(0.5f, 0.87f), new Vector2(0.5f, 0.5f),
            new Vector2(430, 46), Vector2.zero, 26, C_Green);

        // Separator
        var sep = new GameObject("Sep");
        sep.transform.SetParent(roundEndPanel.transform, false);
        var sr = sep.AddComponent<RectTransform>();
        sr.anchorMin = new Vector2(0.1f, 0.74f); sr.anchorMax = new Vector2(0.9f, 0.74f);
        sr.sizeDelta = new Vector2(0, 1); sep.AddComponent<Image>().color = C_PanelBorder;

        roundEndInfoText = CreateText(roundEndPanel.transform, "Info", "",
            new Vector2(0.5f, 0.47f), new Vector2(0.5f, 0.47f), new Vector2(0.5f, 0.5f),
            new Vector2(470, 180), Vector2.zero, 18, C_White);

        var contBtn = MakeStyledButton(roundEndPanel.transform, "ContinueBtn",
            "▶  ДАЛІ",
            new Vector2(0.5f, 0.11f), new Vector2(200, 50),
            new Color(0.18f, 0.46f, 0.16f), 18);
        continueBtn = contBtn;
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
        overlay.color = new Color(0, 0, 0, 0.82f);

        // Central card
        var card = new GameObject("Card");
        card.transform.SetParent(gameOverPanel.transform, false);
        var cardRect = card.AddComponent<RectTransform>();
        cardRect.anchorMin = new Vector2(0.28f, 0.30f);
        cardRect.anchorMax = new Vector2(0.72f, 0.74f);
        cardRect.sizeDelta = Vector2.zero;
        var cardImg = card.AddComponent<Image>();
        cardImg.color = C_PanelBg;
        AddGoldBorder(card.transform, 563, 317);

        // Shadow + main text
        var goShadow = CreateText(card.transform, "GameOverShadow", "ГРА ЗАВЕРШЕНА",
            new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.5f),
            new Vector2(480, 62), new Vector2(3, -3), 36, new Color(0, 0, 0, 0.8f));

        gameOverText = CreateText(card.transform, "GameOverText", "ГРА ЗАВЕРШЕНА",
            new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.5f),
            new Vector2(480, 62), Vector2.zero, 36, C_White);
        gameOverText.fontStyle = FontStyle.Bold;

        // Separator
        var sep = new GameObject("Sep");
        sep.transform.SetParent(card.transform, false);
        var sr = sep.AddComponent<RectTransform>();
        sr.anchorMin = new Vector2(0.1f, 0.55f); sr.anchorMax = new Vector2(0.9f, 0.55f);
        sr.sizeDelta = new Vector2(0, 1); sep.AddComponent<Image>().color = C_PanelBorder;

        var restartBtn2 = MakeStyledButton(card.transform, "RestartBtn",
            "↺  ПЕРЕЗАПУСК",
            new Vector2(0.5f, 0.35f), new Vector2(230, 52),
            new Color(0.22f, 0.40f, 0.70f), 18);
        restartBtn = restartBtn2;
        restartBtn.onClick.AddListener(OnRestartClicked);

        var menuBtn = MakeStyledButton(card.transform, "MenuBtn",
            "⌂  ГОЛОВНЕ МЕНЮ",
            new Vector2(0.5f, 0.12f), new Vector2(230, 52),
            new Color(0.48f, 0.24f, 0.14f), 18);
        menuBtn.onClick.AddListener(OnBackToMenu);

        gameOverPanel.SetActive(false);
    }

    // ========== HELPER: Create Button (legacy, kept for attacker panel) ==========
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
            Vector2.zero, Vector2.zero, 18, C_White);
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

            var outline = towerButtons[i].GetComponent<Outline>();

            if (selected)
            {
                towerButtonImages[i].color = TowerSelectedColors[i];
                if (outline != null)
                {
                    outline.effectColor = C_Gold;
                    outline.effectDistance = new Vector2(4, 4);
                }
            }
            else if (affordable)
            {
                towerButtonImages[i].color = TowerColors[i];
                if (outline != null)
                {
                    outline.effectColor = new Color(0.55f, 0.42f, 0.10f, 0.60f);
                    outline.effectDistance = new Vector2(2, 2);
                }
            }
            else
            {
                towerButtonImages[i].color = disabledBtnColor;
                if (outline != null)
                {
                    outline.effectColor = new Color(0.3f, 0.3f, 0.3f, 0.4f);
                    outline.effectDistance = new Vector2(2, 2);
                }
            }

            towerButtons[i].interactable = canBuy;
        }

        // Selection hint text
        if (selectionText != null)
        {
            if (!canBuy)
            {
                selectionText.text = "";
            }
            else if (string.IsNullOrEmpty(SelectedTowerType))
            {
                selectionText.text = "◆  Оберіть вежу для розміщення  ◆";
                selectionText.color = new Color(0.72f, 0.95f, 0.60f, 0.92f);
            }
            else
            {
                string displayName = SelectedTowerType;
                foreach (var info in TowerInfos)
                    if (info.key == SelectedTowerType) { displayName = info.displayName; break; }
                selectionText.text = "▶▶  Вибрано: " + displayName + "  —  клікніть на поле  ◀◀";
                selectionText.color = C_Gold;
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
                stateText.text = "⚒ Розміщуйте вежі!";
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
                stateText.text = "⚡ Хід Атакуючого";
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
                stateText.text = "⚔ Бій!";
                if (startWaveBtn != null)
                    startWaveBtn.gameObject.SetActive(false);
                if (attackerPanel != null)
                    attackerPanel.SetActive(false);
                if (waveInfoText != null)
                    waveInfoText.gameObject.SetActive(true);
                break;

            case GameState.RoundEnd:
                stateText.text = "✦ Раунд завершено!";
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
                    gameOverText.text = won ? "✦  ЗАХИСНИК ПЕРЕМІГ!  ✦" : "💀  АТАКУЮЧИЙ ПЕРЕМІГ!  💀";
                    gameOverText.color = won ? C_Green : C_Red;
                }
                break;
        }

        UpdateButtonVisuals();
    }

    void ShowRoundEndPanel()
    {
        if (roundEndPanel == null || GameManager.Instance == null) return;

        var gm = GameManager.Instance;
        string info = "🏰  HP Бази: " + gm.BaseHP + "/" + gm.MaxBaseHP + "\n\n";
        info += "⚔  Бонусне золото: +" + gm.LastBonusGold + "\n";
        info += "📜  Бюджет атаки наступного раунду: " + gm.AttackBudget + "\n\n";
        if (gm.Round <= GameManager.MaxRounds)
            info += "◆  Наступний раунд: " + gm.Round + "/" + GameManager.MaxRounds;
        roundEndInfoText.text = info;

        roundEndPanel.SetActive(true);
    }

    void UpdateStatsUI()
    {
        if (GameManager.Instance == null) return;

        if (goldText != null)
            goldText.text = GameManager.Instance.Gold.ToString();
        if (hpText != null)
            hpText.text = "HP: " + GameManager.Instance.BaseHP + "/" + GameManager.Instance.MaxBaseHP;
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
