using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIController : MonoBehaviour
{
    [Header("Texts")]
    [SerializeField] private Text currencyText;
    [SerializeField] private Text incomeText;
    [SerializeField] private Text storeTierText;
    [SerializeField] private Text prestigeText;
    [SerializeField] private Text loginRewardText;
    [SerializeField] private Text dailyMissionsText;
    [SerializeField] private Text queueText;
    [SerializeField] private Text queueMetricsText;
    [SerializeField] private Text upgradesText;
    [SerializeField] private Text debugIndicatorText;
    [SerializeField] private Text comboText;
    [SerializeField] private Button debugToggleButton;

    [Header("Meters")]
    [SerializeField] private Slider satisfactionSlider;
    [SerializeField] private Slider comboSlider;

    [Header("Views")]
    [SerializeField] private DailyMissionView dailyMissionView;
    [SerializeField] private PrestigeView prestigeView;
    [SerializeField] private QueueControlView queueControlView;
    [SerializeField] private UpgradeListView upgradeListView;
    [SerializeField] private DebugPanelView debugPanelView;
    [SerializeField] private PerfOverlayView perfOverlayView;
    [SerializeField] private TutorialView tutorialView;
    [SerializeField] private LeaderboardView leaderboardView;
    [SerializeField] private MonetizationView monetizationView;
    [SerializeField] private GrillStationView grillStationView;

    [Header("Responsive Layout")]
    [SerializeField] private CanvasScaler canvasScaler;
    [SerializeField] private RectTransform topBar;
    [SerializeField] private RectTransform queuePanel;
    [SerializeField] private RectTransform upgradesPanel;
    [SerializeField] private RectTransform grillPanel;
    [SerializeField] private RectTransform bottomBar;
    [SerializeField] private RectTransform debugPanelRect;
    [SerializeField] private RectTransform perfOverlayRect;
    [SerializeField] private RectTransform tutorialOverlayRect;
    [SerializeField] private RectTransform leaderboardPanelRect;
    [SerializeField] private RectTransform monetizationPanelRect;
    [SerializeField] private RectTransform dailyMissionPanelRect;
    [SerializeField] private RectTransform prestigePanelRect;
    [SerializeField] private float panelMargin = 20f;

    private GameManager gameManager;
    private Vector2Int lastScreenSize = new Vector2Int(-1, -1);

    private void Awake()
    {
        ResolveLayoutReferences();
        ApplyVisualPolish();
        ApplyResponsiveLayout(force: true);
    }

    private void OnEnable()
    {
        ApplyResponsiveLayout(force: true);
    }

    private void LateUpdate()
    {
        if (Screen.width != lastScreenSize.x || Screen.height != lastScreenSize.y)
        {
            ApplyResponsiveLayout(force: true);
        }
    }

    public void Bind(GameManager manager)
    {
        gameManager = manager;
        ResolveLayoutReferences();
        dailyMissionView?.Bind(manager);
        prestigeView?.Bind(manager);
        queueControlView?.Bind(manager);
        upgradeListView?.Bind(manager);
        debugPanelView?.Bind(manager);
        perfOverlayView?.Bind(manager);
        tutorialView?.Bind(manager);
        leaderboardView?.Bind(manager);
        monetizationView?.Bind(manager);
        grillStationView?.Bind(manager);
        if (debugToggleButton != null && !Application.isEditor)
        {
            debugToggleButton.gameObject.SetActive(false);
        }
        ApplyResponsiveLayout(force: true);
        UpdateDebugIndicator();
    }

    public void UpdateEconomy(double currency, double incomePerSec)
    {
        if (currencyText != null)
        {
            currencyText.text = FormatUtil.FormatCurrency(currency);
        }

        if (incomeText != null)
        {
            incomeText.text = FormatUtil.FormatCurrency(incomePerSec) + "/s";
        }
    }

    public void UpdateStoreTier(StoreTier tier)
    {
        if (storeTierText == null)
        {
            return;
        }

        storeTierText.text = tier != null ? tier.displayName : "";
    }

    public void UpdateSatisfaction(float value)
    {
        if (satisfactionSlider != null)
        {
            satisfactionSlider.value = value;
        }
    }

    public void UpdatePrestige(int level, int points)
    {
        if (prestigeText == null)
        {
            return;
        }

        prestigeText.text = "Prestige " + level + " (+" + points + ")";
        prestigeView?.Refresh(level, points);
    }

    public void UpdateDailyMissions(IReadOnlyList<DailyMissionState> missions)
    {
        if (dailyMissionsText == null || missions == null)
        {
            dailyMissionView?.Render(missions);
            return;
        }

        var completed = 0;
        var total = 0;
        foreach (var mission in missions)
        {
            if (mission == null)
            {
                continue;
            }

            total++;
            if (mission.claimed || mission.completed)
            {
                completed++;
            }
        }

        if (total <= 0)
        {
            dailyMissionsText.text = "Missions: None";
        }
        else
        {
            dailyMissionsText.text = "Missions " + completed + "/" + total + " complete";
        }
        dailyMissionView?.Render(missions);
    }

    public void UpdateQueue(IReadOnlyList<CustomerQueueEntry> queue)
    {
        if (queueText == null)
        {
            return;
        }

        if (queue == null || queue.Count == 0)
        {
            queueText.text = "No customers waiting.";
            return;
        }

        var lines = new List<string>();
        var maxLines = Mathf.Min(6, queue.Count);
        for (int i = 0; i < maxLines; i++)
        {
            var entry = queue[i];
            if (entry == null)
            {
                continue;
            }

            var remaining = Mathf.Max(0f, entry.patience - entry.waitTime);
            lines.Add(entry.customerName + " · " + remaining.ToString("0") + "s");
        }

        queueText.text = string.Join("\n", lines);
    }

    public void UpdateQueueMetrics(QueueMetrics metrics)
    {
        if (queueMetricsText == null)
        {
            return;
        }

        queueMetricsText.text = "Avg wait " + metrics.avgWaitSeconds.ToString("0.0") +
                                "s\nServed/min " + metrics.servedPerMinute.ToString("0");
    }

    public void UpdateCombo(int comboCount, float comboTimeRemaining, float comboDuration, float comboMultiplier)
    {
        if (comboText != null)
        {
            if (comboCount > 0)
            {
                var bonusPct = Mathf.RoundToInt((comboMultiplier - 1f) * 100f);
                comboText.text = "Sizzle Combo x" + comboCount + " (+" + bonusPct + "%)";
            }
            else
            {
                comboText.text = "Serve fast to build combo";
            }
        }

        if (comboSlider != null)
        {
            if (comboCount > 0 && comboDuration > 0f)
            {
                comboSlider.gameObject.SetActive(true);
                comboSlider.value = Mathf.Clamp01(comboTimeRemaining / comboDuration);
            }
            else
            {
                comboSlider.value = 0f;
                comboSlider.gameObject.SetActive(false);
            }
        }
    }

    public void RefreshGrill()
    {
        grillStationView?.Refresh();
    }

    public void ShowGrillStatus(string message)
    {
        if (grillStationView != null)
        {
            grillStationView.ShowMessage(message);
        }
    }

    public void SetDebugPanelVisible(bool visible)
    {
        if (debugPanelView != null)
        {
            debugPanelView.gameObject.SetActive(visible);
        }
        if (!visible)
        {
            SetPerfOverlayVisible(false);
        }
        UpdateDebugIndicator();
    }

    public void SetPerfOverlayVisible(bool visible)
    {
        if (perfOverlayView != null)
        {
            perfOverlayView.gameObject.SetActive(visible);
        }
        UpdateDebugIndicator();
    }

    public bool IsDebugPanelVisible()
    {
        return debugPanelView != null && debugPanelView.gameObject.activeSelf;
    }

    public bool IsPerfOverlayVisible()
    {
        return perfOverlayView != null && perfOverlayView.gameObject.activeSelf;
    }

    public void ToggleDebugUI()
    {
        var newState = !IsDebugPanelVisible();
        SetDebugPanelVisible(newState);
        SetPerfOverlayVisible(newState);
    }

    public void SetDebugPresetIndex(int index)
    {
        debugPanelView?.SetPresetIndex(index);
    }

    public int GetDebugPresetIndex()
    {
        return debugPanelView != null ? debugPanelView.GetPresetIndex() : 1;
    }

    public void SetDebugSliderValues(float spawnValue, float serviceValue, bool markCustom)
    {
        debugPanelView?.SetSliderValues(spawnValue, serviceValue, markCustom);
    }

    public void ShowTutorial(string message)
    {
        tutorialView?.Show(message);
    }

    public void HideTutorial()
    {
        tutorialView?.Hide();
    }

    private void UpdateDebugIndicator()
    {
        if (debugIndicatorText == null)
        {
            return;
        }

        if (!Application.isEditor)
        {
            debugIndicatorText.gameObject.SetActive(false);
            return;
        }

        var show = !IsDebugPanelVisible();
        debugIndicatorText.gameObject.SetActive(show);
        if (show)
        {
            debugIndicatorText.text = "DBG OFF";
        }
    }

    public void UpdateUpgrades(IReadOnlyList<UpgradeUiEntry> upgrades)
    {
        upgradeListView?.Render(upgrades);

        if (upgradeListView != null)
        {
            return;
        }

        if (upgradesText == null)
        {
            return;
        }

        if (upgrades == null || upgrades.Count == 0)
        {
            upgradesText.text = "No upgrades available.";
            return;
        }

        var lines = new List<string>();
        var maxLines = Mathf.Min(6, upgrades.Count);
        for (int i = 0; i < maxLines; i++)
        {
            var entry = upgrades[i];
            var costText = FormatUtil.FormatCurrency(entry.cost);
            var status = entry.affordable ? "Ready" : "Locked";
            lines.Add(entry.displayName + " Lv." + entry.level + " - " + costText + " [" + status + "]");
        }

        upgradesText.text = string.Join("\n", lines);
    }

    private void ResolveLayoutReferences()
    {
        if (canvasScaler == null)
        {
            canvasScaler = GetComponent<CanvasScaler>();
        }

        if (topBar == null) topBar = FindRectTransformByName("TopBar");
        if (queuePanel == null) queuePanel = FindRectTransformByName("QueuePanel");
        if (upgradesPanel == null) upgradesPanel = FindRectTransformByName("UpgradesPanel");
        if (grillPanel == null) grillPanel = FindRectTransformByName("GrillPanel");
        if (bottomBar == null) bottomBar = FindRectTransformByName("BottomBar");

        if (debugPanelRect == null && debugPanelView != null)
        {
            debugPanelRect = debugPanelView.transform as RectTransform;
        }
        if (perfOverlayRect == null && perfOverlayView != null)
        {
            perfOverlayRect = perfOverlayView.transform as RectTransform;
        }
        if (tutorialOverlayRect == null && tutorialView != null)
        {
            tutorialOverlayRect = tutorialView.transform as RectTransform;
        }
        if (leaderboardPanelRect == null && leaderboardView != null)
        {
            leaderboardPanelRect = leaderboardView.transform as RectTransform;
        }
        if (monetizationPanelRect == null && monetizationView != null)
        {
            monetizationPanelRect = monetizationView.transform as RectTransform;
        }
        if (grillStationView == null && grillPanel != null)
        {
            grillStationView = grillPanel.GetComponent<GrillStationView>();
            if (grillStationView == null)
            {
                grillStationView = grillPanel.gameObject.AddComponent<GrillStationView>();
            }
        }
        if (dailyMissionPanelRect == null && dailyMissionView != null)
        {
            dailyMissionPanelRect = dailyMissionView.transform as RectTransform;
        }
        if (prestigePanelRect == null && prestigeView != null)
        {
            prestigePanelRect = prestigeView.transform as RectTransform;
        }
    }

    private RectTransform FindRectTransformByName(string targetName)
    {
        if (string.IsNullOrEmpty(targetName))
        {
            return null;
        }

        var stack = new Stack<Transform>();
        stack.Push(transform);
        while (stack.Count > 0)
        {
            var current = stack.Pop();
            if (current.name == targetName)
            {
                return current as RectTransform;
            }

            for (int i = 0; i < current.childCount; i++)
            {
                stack.Push(current.GetChild(i));
            }
        }

        return null;
    }

    private void ApplyVisualPolish()
    {
        TintPanel(topBar, new Color(0.13f, 0.10f, 0.09f, 0.94f));
        TintPanel(bottomBar, new Color(0.11f, 0.08f, 0.07f, 0.94f));
        TintPanel(queuePanel, new Color(0.93f, 0.86f, 0.73f, 0.98f));
        TintPanel(upgradesPanel, new Color(0.93f, 0.86f, 0.73f, 0.98f));
        TintPanel(grillPanel, new Color(0.27f, 0.14f, 0.10f, 0.98f));
        TintPanel(dailyMissionPanelRect, new Color(0.96f, 0.90f, 0.79f, 0.98f));
        TintPanel(prestigePanelRect, new Color(0.96f, 0.90f, 0.79f, 0.98f));

        SetTextStyle(currencyText, 16, 36);
        SetTextStyle(incomeText, 15, 34);
        SetTextStyle(storeTierText, 14, 28);
        SetTextStyle(prestigeText, 13, 26);
        SetTextStyle(loginRewardText, 12, 24);
        SetTextStyle(dailyMissionsText, 12, 24);
        SetTextStyle(queueText, 13, 24);
        SetTextStyle(queueMetricsText, 12, 22);
        SetTextStyle(comboText, 13, 28);

        if (currencyText != null) currencyText.fontStyle = FontStyle.Bold;
        if (incomeText != null) incomeText.fontStyle = FontStyle.Bold;
        if (storeTierText != null) storeTierText.fontStyle = FontStyle.Bold;
        if (prestigeText != null) prestigeText.fontStyle = FontStyle.Bold;
        if (comboText != null) comboText.fontStyle = FontStyle.Bold;

        ApplyArcadeTextPolish();
        ApplyButtonPolish();
    }

    private void SetTextStyle(Text text, int minSize, int maxSize)
    {
        if (text == null)
        {
            return;
        }

        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = minSize;
        text.resizeTextMaxSize = maxSize;

        var outline = text.GetComponent<Outline>();
        if (outline == null)
        {
            outline = text.gameObject.AddComponent<Outline>();
        }
        outline.effectColor = new Color(0f, 0f, 0f, 0.25f);
        outline.effectDistance = new Vector2(1.2f, -1.2f);
    }

    private void TintPanel(RectTransform panel, Color color)
    {
        if (panel == null)
        {
            return;
        }

        var image = panel.GetComponent<Image>();
        if (image != null)
        {
            image.color = color;
        }
    }

    private void ApplyArcadeTextPolish()
    {
        var texts = GetComponentsInChildren<Text>(includeInactive: true);
        if (texts == null)
        {
            return;
        }

        for (int i = 0; i < texts.Length; i++)
        {
            var text = texts[i];
            if (text == null)
            {
                continue;
            }

            if (text.fontSize <= 20 && text.fontStyle == FontStyle.Normal)
            {
                text.fontStyle = FontStyle.Bold;
            }
            text.lineSpacing = 1.06f;

            var shadow = text.GetComponent<Shadow>();
            if (shadow == null)
            {
                shadow = text.gameObject.AddComponent<Shadow>();
            }
            shadow.effectColor = new Color(0f, 0f, 0f, 0.22f);
            shadow.effectDistance = new Vector2(0.9f, -0.9f);
        }
    }

    private void ApplyButtonPolish()
    {
        TryStyleButton("ServeButton", new Color(0.68f, 0.24f, 0.16f, 1f));
        TryStyleButton("RushButton", new Color(0.52f, 0.20f, 0.14f, 1f));
        TryStyleButton("BestUpgradeButton", new Color(0.56f, 0.23f, 0.16f, 1f));
        TryStyleButton("BoostButton", new Color(0.72f, 0.30f, 0.18f, 1f));
        TryStyleButton("PrestigeButton", new Color(0.49f, 0.18f, 0.14f, 1f));
        TryStyleButton("LeaderboardButton", new Color(0.32f, 0.18f, 0.14f, 1f));
        TryStyleButton("ShopButton", new Color(0.32f, 0.18f, 0.14f, 1f));
        TryStyleButton("DebugToggleButton", new Color(0.24f, 0.15f, 0.12f, 1f));
    }

    private void TryStyleButton(string objectName, Color normalColor)
    {
        var button = FindButtonByName(objectName);
        if (button == null)
        {
            return;
        }

        var image = button.targetGraphic as Image;
        if (image != null)
        {
            image.color = normalColor;
        }

        var colors = button.colors;
        colors.normalColor = normalColor;
        colors.highlightedColor = Color.Lerp(normalColor, Color.white, 0.16f);
        colors.pressedColor = Color.Lerp(normalColor, Color.black, 0.16f);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(0.35f, 0.35f, 0.35f, 0.8f);
        button.colors = colors;

        var text = button.GetComponentInChildren<Text>(true);
        if (text != null)
        {
            text.fontStyle = FontStyle.Bold;
            text.color = new Color(0.98f, 0.95f, 0.88f, 1f);
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 11;
            text.resizeTextMaxSize = 20;
        }
    }

    private Button FindButtonByName(string targetName)
    {
        if (string.IsNullOrEmpty(targetName))
        {
            return null;
        }

        var stack = new Stack<Transform>();
        stack.Push(transform);
        while (stack.Count > 0)
        {
            var current = stack.Pop();
            if (current.name == targetName)
            {
                return current.GetComponent<Button>();
            }

            for (int i = 0; i < current.childCount; i++)
            {
                stack.Push(current.GetChild(i));
            }
        }

        return null;
    }

    private void ApplyResponsiveLayout(bool force)
    {
        if (!force && Screen.width == lastScreenSize.x && Screen.height == lastScreenSize.y)
        {
            return;
        }

        ResolveLayoutReferences();

        lastScreenSize = new Vector2Int(Mathf.Max(1, Screen.width), Mathf.Max(1, Screen.height));

        var rootRect = transform as RectTransform;
        var uiWidth = rootRect != null && rootRect.rect.width > 1f ? rootRect.rect.width : 1080f;
        var uiHeight = rootRect != null && rootRect.rect.height > 1f ? rootRect.rect.height : 1920f;
        var landscape = uiWidth >= uiHeight;
        var compact = Mathf.Min(uiWidth, uiHeight) < 700f;
        var ultraWide = landscape && (uiWidth / Mathf.Max(1f, uiHeight)) > 1.95f;

        if (canvasScaler != null)
        {
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            canvasScaler.referenceResolution = landscape ? new Vector2(1920f, 1080f) : new Vector2(1080f, 1920f);
            canvasScaler.matchWidthOrHeight = landscape ? (ultraWide ? 0.62f : 0.52f) : 0.68f;
        }

        var canvas = GetComponent<Canvas>();
        var scaleFactor = canvas != null ? Mathf.Max(0.01f, canvas.scaleFactor) : 1f;
        var safeArea = Screen.safeArea;
        var safeLeft = safeArea.xMin / scaleFactor;
        var safeRight = Mathf.Max(0f, Screen.width - safeArea.xMax) / scaleFactor;
        var safeBottom = safeArea.yMin / scaleFactor;
        var safeTop = Mathf.Max(0f, Screen.height - safeArea.yMax) / scaleFactor;

        var margin = panelMargin + (compact ? -4f : 2f);
        var topHeight = landscape ? 128f : 188f;
        var bottomHeight = landscape ? 176f : 256f;

        var availableWidth = Mathf.Max(760f, uiWidth - safeLeft - safeRight - margin * 4f);
        var leftWidth = landscape
            ? Mathf.Clamp(availableWidth * 0.21f, 280f, 420f)
            : Mathf.Clamp(availableWidth * 0.25f, 220f, 340f);
        var rightWidth = landscape
            ? Mathf.Clamp(availableWidth * 0.27f, 340f, 520f)
            : Mathf.Clamp(availableWidth * 0.25f, 220f, 340f);

        var maxSideTotal = availableWidth - (compact ? 260f : 340f);
        if (maxSideTotal > 0f && leftWidth + rightWidth > maxSideTotal)
        {
            var scale = maxSideTotal / Mathf.Max(1f, leftWidth + rightWidth);
            leftWidth *= scale;
            rightWidth *= scale;
        }

        SetTopStrip(topBar, margin + safeLeft, margin + safeRight, safeTop, topHeight);
        SetBottomStrip(bottomBar, margin + safeLeft, margin + safeRight, safeBottom, bottomHeight);
        SetLeftColumn(queuePanel, margin + safeLeft, safeBottom + bottomHeight + margin, leftWidth, safeTop + topHeight + margin);
        SetRightColumn(upgradesPanel, margin + safeRight, safeBottom + bottomHeight + margin, rightWidth, safeTop + topHeight + margin);
        SetCenterPanel(grillPanel, margin + safeLeft + leftWidth + margin, safeBottom + bottomHeight + margin, margin + safeRight + rightWidth + margin, safeTop + topHeight + margin);

        var missionWidth = landscape ? 320f : 360f;
        var missionHeight = landscape ? 144f : 190f;
        var prestigeWidth = landscape ? 280f : 320f;
        var prestigeHeight = landscape ? 132f : 168f;
        SetBottomLeftPanel(dailyMissionPanelRect, margin + safeLeft + 2f, safeBottom + margin + 2f, missionWidth, missionHeight);
        SetBottomRightPanel(prestigePanelRect, margin + safeRight + 2f, safeBottom + margin + 2f, prestigeWidth, prestigeHeight);
        LayoutAuxiliaryPanels(landscape);

        SetCenteredPanel(leaderboardPanelRect, landscape ? 920f : 760f, landscape ? 700f : 1020f);
        SetCenteredPanel(monetizationPanelRect, landscape ? 900f : 760f, landscape ? 620f : 980f);

        if (debugPanelRect != null)
        {
            debugPanelRect.anchorMin = landscape ? new Vector2(1f, 0f) : new Vector2(0.5f, 0f);
            debugPanelRect.anchorMax = debugPanelRect.anchorMin;
            debugPanelRect.pivot = debugPanelRect.anchorMin;
            debugPanelRect.anchoredPosition = landscape ? new Vector2(-(safeRight + margin), safeBottom + bottomHeight + margin) : new Vector2(0f, safeBottom + bottomHeight + margin);
        }

        if (perfOverlayRect != null)
        {
            perfOverlayRect.anchorMin = new Vector2(1f, 1f);
            perfOverlayRect.anchorMax = new Vector2(1f, 1f);
            perfOverlayRect.pivot = new Vector2(1f, 1f);
            perfOverlayRect.anchoredPosition = new Vector2(-(safeRight + margin), -(safeTop + topHeight + margin));
        }

        if (tutorialOverlayRect != null)
        {
            SetFullStretch(tutorialOverlayRect, safeLeft + margin, safeBottom + margin, safeRight + margin, safeTop + margin);
        }

        LayoutTopBarFields(landscape);
        LayoutBottomBarChildren(landscape);
        LayoutQueuePanelChildren();
        LayoutUpgradesPanelChildren();
    }

    private void LayoutTopBarFields(bool landscape)
    {
        var row1 = landscape ? 42f : 56f;
        var row2 = landscape ? 88f : 132f;
        var slotWidth = landscape ? 320f : 260f;
        var slotHeight = landscape ? 38f : 54f;

        PlaceTopText(currencyText, 0.16f, row1, slotWidth, slotHeight, TextAnchor.MiddleLeft);
        PlaceTopText(incomeText, 0.16f, row2, slotWidth, slotHeight, TextAnchor.MiddleLeft);
        PlaceTopText(storeTierText, 0.50f, row1, 280f, slotHeight, TextAnchor.MiddleCenter);
        PlaceTopText(prestigeText, 0.50f, row2, 280f, slotHeight, TextAnchor.MiddleCenter);
        PlaceTopText(dailyMissionsText, 0.84f, row1, slotWidth, slotHeight, TextAnchor.MiddleRight);
        PlaceTopText(loginRewardText, 0.84f, row2, slotWidth, slotHeight, TextAnchor.MiddleRight);

        if (satisfactionSlider != null && topBar != null)
        {
            var rect = satisfactionSlider.transform as RectTransform;
            if (rect != null)
            {
                rect.SetParent(topBar, worldPositionStays: false);
                rect.anchorMin = new Vector2(0.5f, 0f);
                rect.anchorMax = new Vector2(0.5f, 0f);
                rect.pivot = new Vector2(0.5f, 0f);
                rect.anchoredPosition = new Vector2(0f, landscape ? 10f : 14f);
                rect.sizeDelta = new Vector2(landscape ? 400f : 460f, landscape ? 16f : 22f);
            }
        }
    }

    private void PlaceTopText(Text text, float anchorX, float topOffset, float width, float height, TextAnchor alignment)
    {
        if (text == null || topBar == null)
        {
            return;
        }

        var rect = text.rectTransform;
        rect.SetParent(topBar, worldPositionStays: false);
        rect.anchorMin = new Vector2(anchorX, 1f);
        rect.anchorMax = new Vector2(anchorX, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, -topOffset);
        rect.sizeDelta = new Vector2(width, height);
        text.alignment = alignment;
    }

    private void LayoutQueuePanelChildren()
    {
        var queueTitle = FindRectTransformByName("QueueTitle");
        var queueHint = FindRectTransformByName("QueueHint");
        var queueList = FindRectTransformByName("QueueList");
        var queueMetrics = FindRectTransformByName("QueueMetrics");
        var serveButtonRect = FindRectTransformByName("ServeButton");
        var rushButtonRect = FindRectTransformByName("RushButton");

        SetLocalTop(queueTitle, 14f, 14f, 12f, 36f);
        SetLocalTop(queueHint, 14f, 14f, 50f, 26f);
        SetLocalStretch(queueList, 14f, 136f, 14f, 84f);
        SetLocalBottom(queueMetrics, 14f, 72f, 14f, 50f);

        if (serveButtonRect != null)
        {
            serveButtonRect.anchorMin = new Vector2(0f, 0f);
            serveButtonRect.anchorMax = new Vector2(0.5f, 0f);
            serveButtonRect.pivot = new Vector2(0.5f, 0f);
            serveButtonRect.offsetMin = new Vector2(14f, 14f);
            serveButtonRect.offsetMax = new Vector2(-7f, 62f);
        }

        if (rushButtonRect != null)
        {
            rushButtonRect.anchorMin = new Vector2(0.5f, 0f);
            rushButtonRect.anchorMax = new Vector2(1f, 0f);
            rushButtonRect.pivot = new Vector2(0.5f, 0f);
            rushButtonRect.offsetMin = new Vector2(7f, 14f);
            rushButtonRect.offsetMax = new Vector2(-14f, 62f);
        }
    }

    private void LayoutUpgradesPanelChildren()
    {
        var upgradesTitle = FindRectTransformByName("UpgradesTitle");
        var upgradesHint = FindRectTransformByName("UpgradesHint");
        var upgradesScroll = FindRectTransformByName("UpgradesScroll");
        SetLocalTop(upgradesTitle, 14f, 14f, 12f, 36f);
        SetLocalTop(upgradesHint, 14f, 14f, 50f, 26f);
        SetLocalStretch(upgradesScroll, 12f, 12f, 12f, 84f);
    }

    private void LayoutBottomBarChildren(bool landscape)
    {
        var bestButtonRect = FindRectTransformByName("BestUpgradeButton");
        var boostButtonRect = FindRectTransformByName("BoostButton");

        if (landscape)
        {
            PlaceBottomButton(bestButtonRect, -170f, 0f, 210f, 62f);
            PlaceBottomButton(boostButtonRect, 90f, 0f, 250f, 62f);
        }
        else
        {
            PlaceBottomButton(bestButtonRect, 0f, 34f, 280f, 66f);
            PlaceBottomButton(boostButtonRect, 0f, -38f, 320f, 66f);
        }
    }

    private void LayoutAuxiliaryPanels(bool landscape)
    {
        if (dailyMissionPanelRect != null)
        {
            var missionText1 = FindRectTransformByNameFromRoot(dailyMissionPanelRect, "MissionText1");
            var missionText2 = FindRectTransformByNameFromRoot(dailyMissionPanelRect, "MissionText2");
            var missionText3 = FindRectTransformByNameFromRoot(dailyMissionPanelRect, "MissionText3");
            var claimButton1 = FindRectTransformByNameFromRoot(dailyMissionPanelRect, "ClaimButton1");
            var claimButton2 = FindRectTransformByNameFromRoot(dailyMissionPanelRect, "ClaimButton2");
            var claimButton3 = FindRectTransformByNameFromRoot(dailyMissionPanelRect, "ClaimButton3");

            SetLocalStretch(missionText1, 14f, 14f, 14f, 14f);
            SetPanelChildVisible(missionText2, false);
            SetPanelChildVisible(missionText3, false);
            SetPanelChildVisible(claimButton1, false);
            SetPanelChildVisible(claimButton2, false);
            SetPanelChildVisible(claimButton3, false);

            var missionText = missionText1 != null ? missionText1.GetComponent<Text>() : null;
            if (missionText != null)
            {
                missionText.alignment = TextAnchor.MiddleLeft;
                missionText.resizeTextForBestFit = true;
                missionText.resizeTextMinSize = 12;
                missionText.resizeTextMaxSize = landscape ? 19 : 22;
                missionText.fontStyle = FontStyle.Bold;
                missionText.horizontalOverflow = HorizontalWrapMode.Wrap;
            }
        }

        if (prestigePanelRect != null)
        {
            var prestigeInfoRect = FindRectTransformByNameFromRoot(prestigePanelRect, "PrestigeInfo");
            var prestigeButtonRect = FindRectTransformByNameFromRoot(prestigePanelRect, "PrestigeButton");
            SetLocalTop(prestigeInfoRect, 12f, 12f, 10f, 50f);
            SetLocalBottom(prestigeButtonRect, 30f, 12f, 30f, 44f);
        }
    }

    private void SetPanelChildVisible(RectTransform rect, bool visible)
    {
        if (rect == null)
        {
            return;
        }

        if (rect.gameObject.activeSelf != visible)
        {
            rect.gameObject.SetActive(visible);
        }
    }

    private void PlaceBottomButton(RectTransform rect, float x, float y, float width, float height)
    {
        if (rect == null)
        {
            return;
        }

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(x, y);
        rect.sizeDelta = new Vector2(width, height);
    }

    private void SetBottomLeftPanel(RectTransform rect, float left, float bottom, float width, float height)
    {
        if (rect == null)
        {
            return;
        }

        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(0f, 0f);
        rect.pivot = new Vector2(0f, 0f);
        rect.anchoredPosition = new Vector2(left, bottom);
        rect.sizeDelta = new Vector2(width, height);
    }

    private void SetBottomRightPanel(RectTransform rect, float right, float bottom, float width, float height)
    {
        if (rect == null)
        {
            return;
        }

        rect.anchorMin = new Vector2(1f, 0f);
        rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(1f, 0f);
        rect.anchoredPosition = new Vector2(-right, bottom);
        rect.sizeDelta = new Vector2(width, height);
    }

    private RectTransform FindRectTransformByNameFromRoot(RectTransform root, string targetName)
    {
        if (root == null || string.IsNullOrEmpty(targetName))
        {
            return null;
        }

        var stack = new Stack<Transform>();
        stack.Push(root);
        while (stack.Count > 0)
        {
            var current = stack.Pop();
            if (current.name == targetName)
            {
                return current as RectTransform;
            }

            for (int i = 0; i < current.childCount; i++)
            {
                stack.Push(current.GetChild(i));
            }
        }

        return null;
    }

    private void SetTopStrip(RectTransform rect, float left, float right, float top, float height)
    {
        if (rect == null)
        {
            return;
        }

        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.offsetMin = new Vector2(left, -(top + height));
        rect.offsetMax = new Vector2(-right, -top);
    }

    private void SetBottomStrip(RectTransform rect, float left, float right, float bottom, float height)
    {
        if (rect == null)
        {
            return;
        }

        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.offsetMin = new Vector2(left, bottom);
        rect.offsetMax = new Vector2(-right, bottom + height);
    }

    private void SetLeftColumn(RectTransform rect, float left, float bottom, float width, float top)
    {
        if (rect == null)
        {
            return;
        }

        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 0.5f);
        rect.offsetMin = new Vector2(left, bottom);
        rect.offsetMax = new Vector2(left + width, -top);
    }

    private void SetRightColumn(RectTransform rect, float right, float bottom, float width, float top)
    {
        if (rect == null)
        {
            return;
        }

        rect.anchorMin = new Vector2(1f, 0f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 0.5f);
        rect.offsetMin = new Vector2(-(right + width), bottom);
        rect.offsetMax = new Vector2(-right, -top);
    }

    private void SetCenterPanel(RectTransform rect, float left, float bottom, float right, float top)
    {
        if (rect == null)
        {
            return;
        }

        SetFullStretch(rect, left, bottom, right, top);
    }

    private void SetCenteredPanel(RectTransform rect, float width, float height)
    {
        if (rect == null)
        {
            return;
        }

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(width, height);
        rect.anchoredPosition = Vector2.zero;
    }

    private void SetFullStretch(RectTransform rect, float left, float bottom, float right, float top)
    {
        if (rect == null)
        {
            return;
        }

        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = new Vector2(left, bottom);
        rect.offsetMax = new Vector2(-right, -top);
    }

    private void SetLocalTop(RectTransform rect, float left, float right, float top, float height)
    {
        if (rect == null)
        {
            return;
        }

        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.offsetMin = new Vector2(left, -(top + height));
        rect.offsetMax = new Vector2(-right, -top);
    }

    private void SetLocalBottom(RectTransform rect, float left, float bottom, float right, float height)
    {
        if (rect == null)
        {
            return;
        }

        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.offsetMin = new Vector2(left, bottom);
        rect.offsetMax = new Vector2(-right, bottom + height);
    }

    private void SetLocalStretch(RectTransform rect, float left, float bottom, float right, float top)
    {
        if (rect == null)
        {
            return;
        }

        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = new Vector2(left, bottom);
        rect.offsetMax = new Vector2(-right, -top);
    }

    public void ShowLoginReward(DailyLoginReward reward)
    {
        if (loginRewardText == null || !reward.granted)
        {
            return;
        }

        loginRewardText.text = "Login Day " + reward.streakDay + " +" + FormatUtil.FormatCurrency(reward.currency);
    }

    public void OnSizzleBoostClicked()
    {
        gameManager?.TriggerSizzleBoost();
    }
}
