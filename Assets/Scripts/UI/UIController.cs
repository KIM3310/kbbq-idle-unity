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
        TintPanel(topBar, new Color(0.12f, 0.09f, 0.08f, 0.88f));
        TintPanel(bottomBar, new Color(0.10f, 0.07f, 0.06f, 0.90f));
        TintPanel(queuePanel, new Color(0.95f, 0.88f, 0.76f, 0.97f));
        TintPanel(upgradesPanel, new Color(0.95f, 0.88f, 0.76f, 0.97f));
        TintPanel(grillPanel, new Color(0.33f, 0.18f, 0.13f, 0.97f));

        SetTextStyle(currencyText, 12, 28);
        SetTextStyle(incomeText, 12, 28);
        SetTextStyle(storeTierText, 11, 24);
        SetTextStyle(prestigeText, 11, 24);
        SetTextStyle(loginRewardText, 10, 22);
        SetTextStyle(dailyMissionsText, 10, 22);
        SetTextStyle(queueText, 11, 20);
        SetTextStyle(queueMetricsText, 10, 20);
        SetTextStyle(comboText, 10, 24);
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

        if (canvasScaler != null)
        {
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            canvasScaler.referenceResolution = landscape ? new Vector2(1920f, 1080f) : new Vector2(1080f, 1920f);
            canvasScaler.matchWidthOrHeight = landscape ? 0.55f : 0.70f;
        }

        var canvas = GetComponent<Canvas>();
        var scaleFactor = canvas != null ? Mathf.Max(0.01f, canvas.scaleFactor) : 1f;
        var safeArea = Screen.safeArea;
        var safeLeft = safeArea.xMin / scaleFactor;
        var safeRight = Mathf.Max(0f, Screen.width - safeArea.xMax) / scaleFactor;
        var safeBottom = safeArea.yMin / scaleFactor;
        var safeTop = Mathf.Max(0f, Screen.height - safeArea.yMax) / scaleFactor;

        var margin = panelMargin + (compact ? -4f : 2f);
        var topHeight = landscape ? 138f : 212f;
        var bottomHeight = landscape ? 130f : 186f;

        var availableWidth = Mathf.Max(640f, uiWidth - safeLeft - safeRight - margin * 4f);
        var leftWidth = landscape
            ? Mathf.Clamp(availableWidth * 0.22f, 280f, 390f)
            : Mathf.Clamp(availableWidth * 0.25f, 220f, 320f);
        var rightWidth = landscape
            ? Mathf.Clamp(availableWidth * 0.26f, 320f, 500f)
            : Mathf.Clamp(availableWidth * 0.25f, 220f, 320f);

        var maxSideTotal = availableWidth - (compact ? 220f : 280f);
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
        LayoutQueuePanelChildren();
        LayoutUpgradesPanelChildren();
    }

    private void LayoutTopBarFields(bool landscape)
    {
        var row1 = landscape ? 42f : 56f;
        var row2 = landscape ? 92f : 136f;
        var slotWidth = landscape ? 320f : 260f;
        var slotHeight = landscape ? 38f : 54f;

        PlaceTopText(currencyText, 0.16f, row1, slotWidth, slotHeight, TextAnchor.MiddleLeft);
        PlaceTopText(incomeText, 0.16f, row2, slotWidth, slotHeight, TextAnchor.MiddleLeft);
        PlaceTopText(storeTierText, 0.50f, row1, 280f, slotHeight, TextAnchor.MiddleCenter);
        PlaceTopText(prestigeText, 0.50f, row2, 280f, slotHeight, TextAnchor.MiddleCenter);
        PlaceTopText(dailyMissionsText, 0.84f, row1, slotWidth, slotHeight, TextAnchor.MiddleRight);
        PlaceTopText(loginRewardText, 0.84f, row2, slotWidth, slotHeight, TextAnchor.MiddleRight);
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
        var queueList = FindRectTransformByName("QueueList");
        var queueMetrics = FindRectTransformByName("QueueMetrics");
        var serveButtonRect = FindRectTransformByName("ServeButton");
        var rushButtonRect = FindRectTransformByName("RushButton");

        SetLocalTop(queueTitle, 14f, 14f, 12f, 36f);
        SetLocalStretch(queueList, 14f, 132f, 14f, 58f);
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
        var upgradesScroll = FindRectTransformByName("UpgradesScroll");
        SetLocalTop(upgradesTitle, 14f, 14f, 12f, 36f);
        SetLocalStretch(upgradesScroll, 12f, 12f, 12f, 58f);
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
