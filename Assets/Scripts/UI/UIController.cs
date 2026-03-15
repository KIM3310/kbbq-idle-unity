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
    private double latestCurrency;
    private QueueMetrics latestQueueMetrics;
    private RectTransform sessionGoalHudRect;
    private Image sessionGoalHudImage;
    private Text sessionGoalTagText;
    private Text sessionGoalHeadlineText;
    private Text sessionGoalDetailText;
    private float sessionGoalUrgency;
    private RectTransform storyQuestHudRect;
    private Image storyQuestHudImage;
    private Text storyQuestActText;
    private Text storyQuestChapterText;
    private Text storyQuestNarrativeText;
    private Text storyQuestObjectiveText;
    private Text storyQuestRewardText;
    private float storyQuestHeat;
    private RectTransform storyLogHudRect;
    private Image storyLogHudImage;
    private Text storyLogHeadlineText;
    private Text storyLogSpeakerText;
    private Text storyLogLineText;
    private float storyLogHeat;
    private RectTransform sideQuestHudRect;
    private Image sideQuestHudImage;
    private Text sideQuestDistrictText;
    private Text sideQuestSpeakerText;
    private Text sideQuestTitleText;
    private Text sideQuestObjectiveText;
    private Text sideQuestRewardText;
    private float sideQuestHeat;
    private RectTransform showcaseHudRect;
    private Image showcaseHudImage;
    private Text showcaseTitleText;
    private Text showcasePrimaryText;
    private Text showcaseSecondaryText;
    private Text showcaseFooterText;
    private float showcaseHeat;
    private RectTransform feverMeterRect;
    private Image feverMeterBack;
    private Image feverMeterFill;
    private Text feverMeterText;
    private float feverVisualIntensity;
    private RectTransform hypeMeterRect;
    private Image hypeMeterBack;
    private Image hypeMeterFill;
    private Text hypeMeterText;
    private Text hypeDetailText;
    private RectTransform marqueeRect;
    private Text marqueeText;
    private float marqueeOffset;
    private RectTransform feverOverlayRect;
    private Image feverOverlayImage;
    private RectTransform celebrationBurstRect;
    private readonly List<Image> celebrationBurstPieces = new List<Image>();
    private float celebrationBurstTimer;
    private Color celebrationBurstColor = Color.white;
    private RectTransform chefHypeRect;
    private Image chefHypePanel;
    private Image chefHypeSpriteImage;
    private Text chefHypeText;
    private RectTransform momentSpotlightRect;
    private Image momentSpotlightPanel;
    private Image momentSpotlightGlow;
    private Image momentSpotlightIcon;
    private Text momentSpotlightTitleText;
    private Text momentSpotlightDetailText;
    private float momentSpotlightTimer;
    private float momentSpotlightDuration;
    private Color momentSpotlightColor = Color.white;
    private bool momentSpotlightUseThumb;
    private RectTransform liveEventBannerRect;
    private Image liveEventBannerPanel;
    private Image liveEventBannerStripe;
    private Text liveEventBannerTitleText;
    private Text liveEventBannerDetailText;
    private float liveEventBannerAccent;
    private float liveEventBannerUrgency;
    private RectTransform brandBoardRect;
    private Image brandBoardPanel;
    private Text brandBoardTitleText;
    private Text brandBoardDetailText;
    private RectTransform topBrandRibbonRect;
    private Image topBrandRibbonPanel;
    private Image topBrandRibbonShine;
    private Text topBrandRibbonTitleText;
    private Text topBrandRibbonSubtitleText;
    private RectTransform heroHeaderRect;
    private Image heroHeaderPanel;
    private Image heroHeaderShine;
    private Text heroHeaderTitleText;
    private Text heroHeaderSubtitleText;
    private RectTransform stageSpotlightsRect;
    private readonly List<Image> stageSpotlights = new List<Image>();
    private RectTransform heatEmbersRect;
    private readonly List<Image> heatEmbers = new List<Image>();
    private RectTransform feverAuraRect;
    private readonly List<Image> feverAuraPieces = new List<Image>();
    private RectTransform grillFrameRect;
    private readonly List<Image> grillFramePieces = new List<Image>();
    private Image queueNeonRail;
    private Image upgradesNeonRail;
    private Image queueGlossOverlay;
    private Image upgradesGlossOverlay;
    private Image grillGlossOverlay;
    private Button serveActionButton;
    private Button rushActionButton;
    private Button bestUpgradeActionButton;
    private Button boostActionButton;
    private Button prestigeActionButton;
    private Button shopActionButton;
    private Button leaderboardActionButton;
    private Color currentThemeAccent = new Color(0.72f, 0.30f, 0.18f, 1f);
    private Color currentThemeAccentStrong = new Color(1f, 0.82f, 0.36f, 1f);
    private Sprite chefIdleSprite;
    private Sprite chefHypeSprite;
    private Sprite chefThumbSprite;
    private Sprite confettiSprite;
    private RectTransform districtBackdropRect;
    private Image districtBackdropImage;
    private Image districtSignImage;
    private Text districtSignText;
    private RectTransform districtBadgeRect;
    private Image districtBadgeImage;
    private Text districtBadgeTitleText;
    private Text districtBadgeSubtitleText;
    private RectTransform festivalLightsRect;
    private readonly List<Image> festivalLights = new List<Image>();
    private RectTransform crowdRowRect;
    private readonly List<Image> crowdSilhouettes = new List<Image>();
    private Sprite districtAlleySprite;
    private Sprite districtHongdaeSprite;
    private Sprite districtGangnamSprite;
    private Sprite districtHanokSprite;
    private Sprite districtGlobalSprite;
    private Sprite districtSignSprite;
    private Sprite lightBulbSprite;
    private Sprite crowdSprite;
    private bool wasFeverRunning;
    private bool wasPrestigeReady;
    private float chefThumbTimer;
    private RectTransform prestigeFinaleRect;
    private Image prestigeFinalePanel;
    private Image prestigeFinaleGlow;
    private Text prestigeFinaleTitleText;
    private Text prestigeFinaleDetailText;
    private float prestigeFinaleTimer;
    private Camera cachedCamera;
    private bool cameraStateReady;
    private Vector3 cameraBasePosition;
    private float cameraBaseOrthoSize;
    private float cameraBaseFieldOfView;
    private float cameraPunchTimer;
    private float cameraPunchDuration;
    private float cameraPunchStrength;

    private struct ThemePalette
    {
        public Color topBar;
        public Color bottomBar;
        public Color sidePanel;
        public Color grillPanel;
        public Color missionPanel;
        public Color accent;
        public Color accentStrong;
        public Color textPrimary;
        public Color textMuted;
    }

    private void Awake()
    {
        BuildChefSprites();
        BuildBackdropSprites();
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

        if (cameraPunchTimer > 0f)
        {
            cameraPunchTimer = Mathf.Max(0f, cameraPunchTimer - Time.unscaledDeltaTime);
        }

        if (gameManager != null)
        {
            var prestigeReady = gameManager.CanPrestige();
            if (prestigeReady && !wasPrestigeReady)
            {
                TriggerPrestigeFinale();
            }
            wasPrestigeReady = prestigeReady;
        }

        if (sessionGoalHudRect != null)
        {
            var pulse = 1f + Mathf.Sin(Time.unscaledTime * (2.2f + sessionGoalUrgency * 4f)) * (0.01f + sessionGoalUrgency * 0.01f);
            sessionGoalHudRect.localScale = new Vector3(pulse, pulse, 1f);
        }
        if (storyQuestHudRect != null)
        {
            var pulse = 1f + Mathf.Sin(Time.unscaledTime * (1.8f + storyQuestHeat * 3.2f)) * (0.008f + storyQuestHeat * 0.01f);
            storyQuestHudRect.localScale = new Vector3(pulse, pulse, 1f);
        }
        if (storyLogHudRect != null)
        {
            var pulse = 1f + Mathf.Sin(Time.unscaledTime * (1.4f + storyLogHeat * 2.8f)) * (0.006f + storyLogHeat * 0.008f);
            storyLogHudRect.localScale = new Vector3(pulse, pulse, 1f);
        }
        if (sideQuestHudRect != null)
        {
            var pulse = 1f + Mathf.Sin(Time.unscaledTime * (1.5f + sideQuestHeat * 3f)) * (0.006f + sideQuestHeat * 0.008f);
            sideQuestHudRect.localScale = new Vector3(pulse, pulse, 1f);
        }
        if (showcaseHudRect != null)
        {
            var shimmer = 1f + Mathf.Sin(Time.unscaledTime * (1.6f + showcaseHeat * 3f)) * (0.006f + showcaseHeat * 0.008f);
            showcaseHudRect.localScale = new Vector3(shimmer, shimmer, 1f);
        }
        if (feverMeterRect != null)
        {
            var pulse = 1f + Mathf.Sin(Time.unscaledTime * (2.4f + feverVisualIntensity * 5f)) * (0.006f + feverVisualIntensity * 0.012f);
            feverMeterRect.localScale = new Vector3(pulse, pulse, 1f);
        }
        if (liveEventBannerRect != null && liveEventBannerRect.gameObject.activeSelf)
        {
            var pulse = 1f + Mathf.Sin(Time.unscaledTime * (3.2f + liveEventBannerUrgency * 5f)) * (0.008f + liveEventBannerAccent * 0.012f);
            liveEventBannerRect.localScale = new Vector3(pulse, pulse, 1f);
        }
        UpdateCelebrationBurst();
        AnimateDistrictBackdropScene();
        AnimateCrowdRow();
        AnimateFestivalLights();
        AnimateStageSpotlights();
        AnimateFeverAura();
        AnimateHeatEmbers();
        AnimatePremiumPresentation();
        AnimateActionButtons();
        UpdateMarquee();
        UpdateMomentSpotlight();
        UpdatePrestigeFinale();
        ApplyFeverWarpVisuals();
        UpdateChefHypeHud();
        ApplyCameraFeel();
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
        UpdateSessionGoal(gameManager != null ? gameManager.GetSessionGoalUiState() : default);
        UpdateStoryQuest(gameManager != null ? gameManager.GetStoryQuestUiState() : default);
        UpdateStoryLog(gameManager != null ? gameManager.GetStoryLogUiState() : default);
        UpdateSideQuest(gameManager != null ? gameManager.GetDistrictSideQuestUiState() : default);
        UpdateShowcase(gameManager != null ? gameManager.GetRestaurantShowcaseUiState() : default);
        UpdateHypeDisplay(gameManager != null ? gameManager.GetHypeUiState() : default);
        UpdateLiveEventBanner(gameManager != null ? gameManager.GetLiveEventBannerUiState() : default);
        UpdateFeverDisplay();
    }

    private double displayCurrency;
    private Coroutine currencyCoroutine;

    public void UpdateEconomy(double currency, double incomePerSec)
    {
        latestCurrency = currency;
        
        if (currencyText != null)
        {
            if (currencyCoroutine != null) StopCoroutine(currencyCoroutine);
            currencyCoroutine = StartCoroutine(AnimateCurrencyText(currency));
        }

        if (incomeText != null)
        {
            incomeText.text = "Income " + FormatUtil.FormatCurrency(incomePerSec) + "/s";
        }

        queueControlView?.RenderMetrics(latestQueueMetrics, latestCurrency);
    }

    private System.Collections.IEnumerator AnimateCurrencyText(double targetCurrency)
    {
        double startValue = displayCurrency;
        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            // Ease out quad
            t = t * (2 - t);
            
            displayCurrency = startValue + (targetCurrency - startValue) * t;
            currencyText.text = "$ " + FormatUtil.FormatCurrency(displayCurrency);
            yield return null;
        }

        displayCurrency = targetCurrency;
        currencyText.text = "$ " + FormatUtil.FormatCurrency(displayCurrency);
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
        prestigeView?.Refresh(level, points, gameManager != null ? gameManager.GetPrestigeStatusText() : string.Empty);
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
        queueControlView?.RenderQueue(queue);

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

        queueText.text = "Queue " + queue.Count + "\n" + string.Join("\n", lines);
    }

    public void UpdateQueueMetrics(QueueMetrics metrics)
    {
        latestQueueMetrics = metrics;
        queueControlView?.RenderMetrics(metrics, latestCurrency);

        if (queueMetricsText == null)
        {
            return;
        }

        queueMetricsText.text = "Served " + metrics.totalServed +
                                "\nAvg " + metrics.avgWaitSeconds.ToString("0.0") + "s";
    }

    public void UpdateCombo(int comboCount, float comboTimeRemaining, float comboDuration, float comboMultiplier)
    {
        var feverLive = gameManager != null && gameManager.IsChefFeverRunning();
        var feverReady = gameManager != null && gameManager.IsChefFeverPrimed();
        if (comboText != null)
        {
            if (feverLive)
            {
                var feverPct = Mathf.RoundToInt(gameManager.GetChefFeverRemainingNormalized() * 100f);
                comboText.text = "CHEF FEVER LIVE  +" + Mathf.RoundToInt((1.35f - 1f) * 100f) + "%  [" + feverPct + "%]";
                comboText.color = new Color(1f, 0.90f, 0.56f, 1f);
            }
            else if (comboCount > 0)
            {
                var bonusPct = Mathf.RoundToInt((comboMultiplier - 1f) * 100f);
                comboText.text = "Sizzle Combo x" + comboCount + " (+" + bonusPct + "%)";
                comboText.color = feverReady
                    ? new Color(1f, 0.84f, 0.52f, 1f)
                    : new Color(0.98f, 0.94f, 0.84f, 1f);
            }
            else
            {
                comboText.text = feverReady ? "Chef Fever Ready" : "Serve fast to build combo";
                comboText.color = feverReady
                    ? new Color(1f, 0.84f, 0.52f, 1f)
                    : new Color(0.98f, 0.94f, 0.84f, 1f);
            }
        }

        if (comboSlider != null)
        {
            if (feverLive)
            {
                comboSlider.gameObject.SetActive(true);
                comboSlider.value = gameManager.GetChefFeverRemainingNormalized();
                TintSlider(comboSlider, new Color(1f, 0.80f, 0.34f, 1f), new Color(0.30f, 0.14f, 0.08f, 0.92f));
            }
            else if (comboCount > 0 && comboDuration > 0f)
            {
                comboSlider.gameObject.SetActive(true);
                comboSlider.value = Mathf.Clamp01(comboTimeRemaining / comboDuration);
                TintSlider(
                    comboSlider,
                    feverReady ? new Color(1f, 0.78f, 0.36f, 1f) : new Color(0.78f, 0.22f, 0.16f, 0.96f),
                    new Color(0.18f, 0.10f, 0.08f, 0.88f));
            }
            else
            {
                comboSlider.value = 0f;
                comboSlider.gameObject.SetActive(false);
            }
        }

        UpdateFeverDisplay();
    }

    public void UpdateSessionGoal(SessionGoalUiState goal)
    {
        EnsureSessionGoalHud();
        if (sessionGoalHudRect == null)
        {
            return;
        }

        sessionGoalUrgency = Mathf.Clamp01(goal.urgency01);
        if (sessionGoalTagText != null)
        {
            sessionGoalTagText.text = string.IsNullOrEmpty(goal.accentLabel) ? "SHIFT" : goal.accentLabel.ToUpperInvariant();
        }
        if (sessionGoalHeadlineText != null)
        {
            sessionGoalHeadlineText.text = string.IsNullOrEmpty(goal.headline) ? "KEEP THE GRILL HOT" : goal.headline;
        }
        if (sessionGoalDetailText != null)
        {
            sessionGoalDetailText.text = string.IsNullOrEmpty(goal.detail)
                ? "Buy cuts, grill on time, and serve fast to build tips and combo."
                : goal.detail;
        }
        if (sessionGoalHudImage != null)
        {
            sessionGoalHudImage.color = Color.Lerp(
                new Color(0.18f, 0.10f, 0.08f, 0.82f),
                new Color(0.46f, 0.17f, 0.10f, 0.92f),
                sessionGoalUrgency);
        }
    }

    public void UpdateShowcase(RestaurantShowcaseUiState showcase)
    {
        EnsureDistrictBackdrop();
        EnsureShowcaseHud();
        EnsureBrandBoard();
        EnsureHeroHeader();
        if (showcaseHudRect == null)
        {
            return;
        }

        showcaseHeat = Mathf.Clamp01(showcase.heat01);
        if (showcaseTitleText != null)
        {
            showcaseTitleText.text = string.IsNullOrEmpty(showcase.title) ? "HOUSE SPECIAL" : showcase.title;
        }
        if (showcasePrimaryText != null)
        {
            showcasePrimaryText.text = string.IsNullOrEmpty(showcase.primary) ? "Today's crowd favorite is on the grill." : showcase.primary;
        }
        if (showcaseSecondaryText != null)
        {
            showcaseSecondaryText.text = showcase.secondary;
        }
        if (showcaseFooterText != null)
        {
            showcaseFooterText.text = showcase.footer;
        }
        if (showcaseHudImage != null)
        {
            showcaseHudImage.color = Color.Lerp(
                new Color(0.14f, 0.08f, 0.06f, 0.80f),
                new Color(0.52f, 0.24f, 0.10f, 0.92f),
                showcaseHeat);
        }

        UpdateDistrictBackdrop(showcase);
        UpdateDistrictBadge();
        UpdateBrandBoard(showcase);
        UpdateTopBrandRibbon(showcase);
        UpdateHeroHeader(showcase);

        ApplyDynamicTheme();
    }

    public void UpdateStoryQuest(StoryQuestUiState story)
    {
        EnsureStoryQuestHud();
        if (storyQuestHudRect == null)
        {
            return;
        }

        storyQuestHudRect.gameObject.SetActive(story.visible);
        if (!story.visible)
        {
            return;
        }

        storyQuestHeat = Mathf.Clamp01(story.accent01);
        if (storyQuestActText != null)
        {
            storyQuestActText.text = string.IsNullOrEmpty(story.actTitle) ? "STORY ARC" : story.actTitle;
        }
        if (storyQuestChapterText != null)
        {
            var chapter = string.IsNullOrEmpty(story.chapterTitle) ? "Current Chapter" : story.chapterTitle;
            storyQuestChapterText.text = string.IsNullOrEmpty(story.speakerName)
                ? chapter
                : story.speakerName + " · " + chapter;
        }
        if (storyQuestNarrativeText != null)
        {
            storyQuestNarrativeText.text = story.narrative;
        }
        if (storyQuestObjectiveText != null)
        {
            storyQuestObjectiveText.text = story.objectiveLine + "\n" + story.statusLine;
        }
        if (storyQuestRewardText != null)
        {
            storyQuestRewardText.text = story.rewardLine;
        }
        if (storyQuestHudImage != null)
        {
            storyQuestHudImage.color = Color.Lerp(
                new Color(0.18f, 0.10f, 0.08f, 0.84f),
                new Color(0.40f, 0.18f, 0.08f, 0.94f),
                storyQuestHeat);
        }
    }

    public void UpdateStoryLog(StoryLogUiState storyLog)
    {
        EnsureStoryLogHud();
        if (storyLogHudRect == null)
        {
            return;
        }

        storyLogHudRect.gameObject.SetActive(storyLog.visible);
        if (!storyLog.visible)
        {
            return;
        }

        storyLogHeat = Mathf.Clamp01(storyLog.accent01);
        if (storyLogHeadlineText != null)
        {
            storyLogHeadlineText.text = string.IsNullOrEmpty(storyLog.headline) ? "STORY LOG" : storyLog.headline;
        }
        if (storyLogSpeakerText != null)
        {
            storyLogSpeakerText.text = string.IsNullOrEmpty(storyLog.speaker) ? "The House" : storyLog.speaker;
        }
        if (storyLogLineText != null)
        {
            storyLogLineText.text = storyLog.line;
        }
        if (storyLogHudImage != null)
        {
            storyLogHudImage.color = Color.Lerp(
                new Color(0.16f, 0.10f, 0.08f, 0.80f),
                new Color(0.34f, 0.16f, 0.08f, 0.92f),
                storyLogHeat);
        }
    }

    public void UpdateSideQuest(DistrictSideQuestUiState sideQuest)
    {
        EnsureSideQuestHud();
        if (sideQuestHudRect == null)
        {
            return;
        }

        sideQuestHudRect.gameObject.SetActive(sideQuest.visible);
        if (!sideQuest.visible)
        {
            return;
        }

        sideQuestHeat = Mathf.Clamp01(sideQuest.accent01);
        if (sideQuestDistrictText != null)
        {
            sideQuestDistrictText.text = string.IsNullOrEmpty(sideQuest.districtTitle) ? "SIDE STORY" : sideQuest.districtTitle;
        }
        if (sideQuestSpeakerText != null)
        {
            sideQuestSpeakerText.text = string.IsNullOrEmpty(sideQuest.speakerName) ? "Local Voice" : sideQuest.speakerName;
        }
        if (sideQuestTitleText != null)
        {
            sideQuestTitleText.text = string.IsNullOrEmpty(sideQuest.chapterTitle) ? "Neighborhood Episode" : sideQuest.chapterTitle;
        }
        if (sideQuestObjectiveText != null)
        {
            sideQuestObjectiveText.text = sideQuest.objectiveLine + "\n" + sideQuest.statusLine;
        }
        if (sideQuestRewardText != null)
        {
            sideQuestRewardText.text = sideQuest.rewardLine;
        }
        if (sideQuestHudImage != null)
        {
            sideQuestHudImage.color = Color.Lerp(
                new Color(0.16f, 0.10f, 0.08f, 0.82f),
                new Color(0.34f, 0.18f, 0.08f, 0.92f),
                sideQuestHeat);
        }
    }

    public void UpdateHypeDisplay(HypeUiState hype)
    {
        EnsureHypeMeter();
        EnsureMarquee();
        if (hypeMeterRect == null)
        {
            return;
        }

        if (hypeMeterFill != null)
        {
            hypeMeterFill.fillAmount = Mathf.Clamp01(hype.fill01);
            hypeMeterFill.color = Color.Lerp(new Color(0.78f, 0.24f, 0.18f, 0.94f), new Color(1f, 0.80f, 0.34f, 1f), hype.fill01);
        }
        if (hypeMeterBack != null)
        {
            hypeMeterBack.color = Color.Lerp(new Color(0.18f, 0.10f, 0.08f, 0.86f), new Color(0.30f, 0.12f, 0.08f, 0.90f), hype.alert01);
        }
        if (hypeMeterText != null)
        {
            hypeMeterText.text = string.IsNullOrEmpty(hype.headline) ? "HOUSE REPUTATION" : hype.headline;
        }
        if (hypeDetailText != null)
        {
            hypeDetailText.text = hype.detail;
            hypeDetailText.color = Color.Lerp(new Color(0.92f, 0.86f, 0.78f, 0.94f), new Color(1f, 0.84f, 0.62f, 1f), hype.alert01);
        }
    }

    public void UpdateLiveEventBanner(LiveEventBannerUiState banner)
    {
        EnsureLiveEventBanner();
        if (liveEventBannerRect == null)
        {
            return;
        }

        liveEventBannerRect.gameObject.SetActive(banner.visible);
        if (!banner.visible)
        {
            liveEventBannerAccent = 0f;
            liveEventBannerUrgency = 0f;
            liveEventBannerRect.localScale = Vector3.one;
            return;
        }

        liveEventBannerAccent = Mathf.Clamp01(banner.accent01);
        liveEventBannerUrgency = Mathf.Clamp01(banner.urgency01);
        var districtAccent = ResolveDistrictAccentColor(gameManager != null && gameManager.GetCurrentStoreTier() != null ? gameManager.GetCurrentStoreTier().id : "alley");

        if (liveEventBannerTitleText != null)
        {
            liveEventBannerTitleText.text = string.IsNullOrEmpty(banner.title) ? "LIVE SERVICE" : banner.title;
        }
        if (liveEventBannerDetailText != null)
        {
            liveEventBannerDetailText.text = string.IsNullOrEmpty(banner.detail) ? "Keep the room moving." : banner.detail;
        }
        if (liveEventBannerPanel != null)
        {
            liveEventBannerPanel.color = Color.Lerp(
                new Color(0.22f, 0.10f, 0.08f, 0.86f),
                new Color(districtAccent.r * 0.55f, districtAccent.g * 0.25f, districtAccent.b * 0.18f, 0.94f),
                liveEventBannerAccent);
        }
        if (liveEventBannerStripe != null)
        {
            liveEventBannerStripe.color = Color.Lerp(
                new Color(districtAccent.r * 0.8f, districtAccent.g * 0.42f, districtAccent.b * 0.28f, 0.92f),
                districtAccent,
                liveEventBannerAccent);
        }
    }

    private void UpdateBrandBoard(RestaurantShowcaseUiState showcase)
    {
        if (brandBoardRect == null || gameManager == null)
        {
            return;
        }

        var tier = gameManager.GetCurrentStoreTier();
        var tierName = tier != null ? tier.displayName : "Alley";
        var tierId = tier != null ? tier.id : "alley";
        var districtAccent = ResolveDistrictAccentColor(tierId);
        if (brandBoardTitleText != null)
        {
            brandBoardTitleText.text = gameManager.CanPrestige()
                ? "SEASON FINALE"
                : tierName.ToUpperInvariant() + " BBQ STAGE";
        }
        if (brandBoardDetailText != null)
        {
            brandBoardDetailText.text = gameManager.CanPrestige()
                ? gameManager.GetPrestigeStatusText()
                : ResolveDistrictFlavor(tierId) + " · " + (string.IsNullOrEmpty(showcase.title) ? "HOUSE SPECIAL" : showcase.title);
        }
        if (brandBoardPanel != null)
        {
            brandBoardPanel.color = Color.Lerp(
                new Color(0.18f, 0.10f, 0.08f, 0.88f),
                gameManager.CanPrestige()
                    ? new Color(0.48f, 0.20f, 0.08f, 0.96f)
                    : new Color(districtAccent.r * 0.42f, districtAccent.g * 0.18f, districtAccent.b * 0.14f, 0.94f),
                showcase.heat01);
        }
    }

    private void UpdateTopBrandRibbon(RestaurantShowcaseUiState showcase)
    {
        if (topBrandRibbonRect == null || gameManager == null)
        {
            return;
        }

        var tier = gameManager.GetCurrentStoreTier();
        var tierName = tier != null ? tier.displayName : "Alley";
        var tierId = tier != null ? tier.id : "alley";
        var districtAccent = ResolveDistrictAccentColor(tierId);
        if (topBrandRibbonTitleText != null)
        {
            topBrandRibbonTitleText.text = gameManager.CanPrestige()
                ? "SPICE STAR FINALE"
                : ResolveDistrictHeadline(tierId);
        }
        if (topBrandRibbonSubtitleText != null)
        {
            topBrandRibbonSubtitleText.text = gameManager.CanPrestige()
                ? gameManager.GetPrestigeStatusText()
                : tierName.ToUpperInvariant() + " · " + ResolveDistrictFlavor(tierId);
        }
        if (topBrandRibbonPanel != null)
        {
            topBrandRibbonPanel.color = Color.Lerp(
                new Color(0.20f, 0.10f, 0.08f, 0.88f),
                gameManager.CanPrestige()
                    ? new Color(0.46f, 0.18f, 0.08f, 0.95f)
                    : new Color(districtAccent.r * 0.44f, districtAccent.g * 0.18f, districtAccent.b * 0.14f, 0.94f),
                showcase.heat01);
        }
    }

    private void UpdateHeroHeader(RestaurantShowcaseUiState showcase)
    {
        if (heroHeaderRect == null || gameManager == null)
        {
            return;
        }

        var live = gameManager.GetLiveEventBannerUiState();
        var tier = gameManager.GetCurrentStoreTier();
        var tierName = tier != null ? tier.displayName : "Alley";
        var tierId = tier != null ? tier.id : "alley";
        var prestigeReady = gameManager.CanPrestige();
        heroHeaderRect.gameObject.SetActive(true);
        if (heroHeaderTitleText != null)
        {
            heroHeaderTitleText.text = prestigeReady
                ? "NEW SEASON WITHIN REACH"
                : live.visible
                ? live.title
                : ResolveDistrictHeroTitle(tierId, tierName);
        }
        if (heroHeaderSubtitleText != null)
        {
            heroHeaderSubtitleText.text = prestigeReady
                ? "Cash out the current run and relaunch with more spice stars, stronger opening cash, and more swagger."
                : live.visible
                ? live.detail
                : "HEADLINER · " + (string.IsNullOrEmpty(showcase.title) ? "HOUSE SPECIAL" : showcase.title) + " · " + ResolveDistrictFlavor(tierId);
        }
        if (heroHeaderPanel != null)
        {
            heroHeaderPanel.color = Color.Lerp(
                new Color(0.18f, 0.10f, 0.08f, 0.74f),
                prestigeReady
                    ? new Color(0.52f, 0.20f, 0.08f, 0.92f)
                    : new Color(ResolveDistrictAccentColor(tierId).r * 0.50f, ResolveDistrictAccentColor(tierId).g * 0.20f, ResolveDistrictAccentColor(tierId).b * 0.16f, 0.90f),
                Mathf.Max(showcase.heat01, live.accent01));
        }
    }

    private Color ResolveDistrictAccentColor(string tierId)
    {
        switch ((tierId ?? "alley").ToLowerInvariant())
        {
            case "hongdae":
                return new Color(1f, 0.48f, 0.76f, 1f);
            case "gangnam":
                return new Color(1f, 0.86f, 0.44f, 1f);
            case "hanok":
                return new Color(0.92f, 0.64f, 0.34f, 1f);
            case "global":
                return new Color(0.58f, 0.82f, 1f, 1f);
            default:
                return new Color(1f, 0.72f, 0.34f, 1f);
        }
    }

    private string ResolveDistrictHeadline(string tierId)
    {
        switch ((tierId ?? "alley").ToLowerInvariant())
        {
            case "hongdae":
                return "NEON GRILL NIGHTS";
            case "gangnam":
                return "PREMIUM NIGHT SERVICE";
            case "hanok":
                return "HERITAGE FIRE HOUSE";
            case "global":
                return "WORLD TOUR BARBECUE";
            default:
                return "K-BBQ FESTIVAL IDLE";
        }
    }

    private string ResolveDistrictFlavor(string tierId)
    {
        switch ((tierId ?? "alley").ToLowerInvariant())
        {
            case "hongdae":
                return "street buzz and neon smoke";
            case "gangnam":
                return "late-night premium rush";
            case "hanok":
                return "woodfire elegance and slow heat";
            case "global":
                return "tourist crush and world-table hype";
            default:
                return "laneway smoke and first regulars";
        }
    }

    private string ResolveDistrictHeroTitle(string tierId, string tierName)
    {
        switch ((tierId ?? "alley").ToLowerInvariant())
        {
            case "hongdae":
                return tierName.ToUpperInvariant() + " AFTER DARK";
            case "gangnam":
                return tierName.ToUpperInvariant() + " VIP SERVICE";
            case "hanok":
                return tierName.ToUpperInvariant() + " FIRE TABLE";
            case "global":
                return tierName.ToUpperInvariant() + " WORLD STAGE";
            default:
                return tierName.ToUpperInvariant() + " NIGHT SERVICE";
        }
    }

    public void UpdateFeverDisplay()
    {
        EnsureFeverMeter();
        EnsureFeverOverlay();
        if (feverMeterRect == null || gameManager == null)
        {
            return;
        }

        var feverLive = gameManager.IsChefFeverRunning();
        var feverReady = gameManager.IsChefFeverPrimed();
        var fillAmount = feverLive
            ? gameManager.GetChefFeverRemainingNormalized()
            : Mathf.Clamp01(gameManager.GetQueueMetrics().servedPerMinute / 10f + (feverReady ? 0.2f : 0f));

        feverVisualIntensity = feverLive ? 1f : (feverReady ? 0.78f : fillAmount * 0.6f);
        feverMeterRect.gameObject.SetActive(feverLive || feverReady || fillAmount > 0.08f);

        if (feverMeterFill != null)
        {
            feverMeterFill.fillAmount = fillAmount;
            feverMeterFill.color = feverLive
                ? new Color(1f, 0.78f, 0.30f, 1f)
                : feverReady
                    ? new Color(0.98f, 0.64f, 0.24f, 0.98f)
                    : new Color(0.76f, 0.22f, 0.16f, 0.94f);
        }

        if (feverMeterBack != null)
        {
            feverMeterBack.color = feverLive
                ? new Color(0.28f, 0.10f, 0.06f, 0.92f)
                : new Color(0.18f, 0.10f, 0.08f, 0.86f);
        }

        if (feverMeterText != null)
        {
            if (feverLive)
            {
                feverMeterText.text = "CHEF FEVER LIVE";
                feverMeterText.color = new Color(1f, 0.90f, 0.58f, 1f);
            }
            else if (feverReady)
            {
                feverMeterText.text = "CHEF FEVER READY";
                feverMeterText.color = new Color(1f, 0.82f, 0.46f, 1f);
            }
            else
            {
                feverMeterText.text = "HEAT BUILDING";
                feverMeterText.color = new Color(0.96f, 0.88f, 0.74f, 0.96f);
            }
        }

        if (feverOverlayImage != null)
        {
            var districtAccent = ResolveDistrictAccentColor(gameManager.GetCurrentStoreTier() != null ? gameManager.GetCurrentStoreTier().id : "alley");
            var overlayPulse = feverLive
                ? (0.72f + Mathf.Sin(Time.unscaledTime * 7.5f) * 0.28f)
                : 1f;
            var overlayAlpha = feverLive
                ? Mathf.Lerp(0.04f, 0.11f, fillAmount)
                : feverReady
                    ? 0.025f
                    : 0f;
            overlayAlpha *= overlayPulse;
            feverOverlayImage.color = feverLive
                ? new Color(districtAccent.r * 0.58f, districtAccent.g * 0.24f, districtAccent.b * 0.18f, overlayAlpha)
                : new Color(0.40f, 0.12f, 0.08f, overlayAlpha);
            feverOverlayRect.gameObject.SetActive(overlayAlpha > 0.001f);
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

    public void PlayCustomerEating(string customerName, string menuName, bool happy)
    {
        queueControlView?.PlayEating(customerName, menuName, happy);
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
        if (serveActionButton == null) serveActionButton = FindButtonByName("ServeButton");
        if (rushActionButton == null) rushActionButton = FindButtonByName("RushButton");
        if (bestUpgradeActionButton == null) bestUpgradeActionButton = FindButtonByName("BestUpgradeButton");
        if (boostActionButton == null) boostActionButton = FindButtonByName("BoostButton");
        if (prestigeActionButton == null) prestigeActionButton = FindButtonByName("PrestigeButton");
        if (shopActionButton == null) shopActionButton = FindButtonByName("ShopButton");
        if (leaderboardActionButton == null) leaderboardActionButton = FindButtonByName("LeaderboardButton");
        EnsureDistrictBackdrop();
        EnsureDistrictBadge();
        EnsureSessionGoalHud();
        EnsureStoryQuestHud();
        EnsureStoryLogHud();
        EnsureSideQuestHud();
        EnsureShowcaseHud();
        EnsureHypeMeter();
        EnsureMarquee();
        EnsureFeverMeter();
        EnsureFeverOverlay();
        EnsureCelebrationBurst();
        EnsureChefHypeHud();
        EnsureMomentSpotlight();
        EnsureLiveEventBanner();
        EnsureBrandBoard();
        EnsureTopBrandRibbon();
        EnsureHeroHeader();
        EnsureStageSpotlights();
        EnsureHeatEmbers();
        EnsureFeverAura();
        EnsurePrestigeFinale();
        EnsureGrillFrameAndRails();
        EnsurePanelGlossOverlays();
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

    private void EnsureSessionGoalHud()
    {
        if (sessionGoalHudRect != null || grillPanel == null)
        {
            return;
        }

        var hud = new GameObject("SessionGoalHud", typeof(RectTransform), typeof(Image));
        sessionGoalHudRect = hud.GetComponent<RectTransform>();
        sessionGoalHudRect.SetParent(grillPanel, false);
        sessionGoalHudImage = hud.GetComponent<Image>();
        sessionGoalHudImage.raycastTarget = false;

        sessionGoalTagText = CreateRuntimeText("GoalTag", sessionGoalHudRect, 12, FontStyle.Bold, TextAnchor.UpperLeft, new Color(1f, 0.86f, 0.58f, 0.98f));
        sessionGoalHeadlineText = CreateRuntimeText("GoalHeadline", sessionGoalHudRect, 20, FontStyle.Bold, TextAnchor.UpperLeft, new Color(0.99f, 0.95f, 0.86f, 1f));
        sessionGoalDetailText = CreateRuntimeText("GoalDetail", sessionGoalHudRect, 13, FontStyle.Bold, TextAnchor.UpperLeft, new Color(0.94f, 0.88f, 0.78f, 0.95f));

        if (sessionGoalTagText != null)
        {
            SetLocalTop(sessionGoalTagText.rectTransform, 16f, 16f, 10f, 18f);
        }
        if (sessionGoalHeadlineText != null)
        {
            SetLocalTop(sessionGoalHeadlineText.rectTransform, 16f, 16f, 28f, 28f);
        }
        if (sessionGoalDetailText != null)
        {
            SetLocalStretch(sessionGoalDetailText.rectTransform, 16f, 12f, 16f, 58f);
        }
    }

    private void EnsureStoryQuestHud()
    {
        if (storyQuestHudRect != null || grillPanel == null)
        {
            return;
        }

        var hud = new GameObject("StoryQuestHud", typeof(RectTransform), typeof(Image));
        storyQuestHudRect = hud.GetComponent<RectTransform>();
        storyQuestHudRect.SetParent(grillPanel, false);
        storyQuestHudImage = hud.GetComponent<Image>();
        storyQuestHudImage.raycastTarget = false;

        storyQuestActText = CreateRuntimeText("StoryQuestAct", storyQuestHudRect, 11, FontStyle.Bold, TextAnchor.UpperLeft, new Color(1f, 0.86f, 0.58f, 0.98f));
        storyQuestChapterText = CreateRuntimeText("StoryQuestChapter", storyQuestHudRect, 16, FontStyle.Bold, TextAnchor.UpperLeft, new Color(0.99f, 0.95f, 0.86f, 1f));
        storyQuestNarrativeText = CreateRuntimeText("StoryQuestNarrative", storyQuestHudRect, 11, FontStyle.Bold, TextAnchor.UpperLeft, new Color(0.96f, 0.90f, 0.80f, 0.96f));
        storyQuestObjectiveText = CreateRuntimeText("StoryQuestObjective", storyQuestHudRect, 11, FontStyle.Bold, TextAnchor.LowerLeft, new Color(0.98f, 0.88f, 0.72f, 0.96f));
        storyQuestRewardText = CreateRuntimeText("StoryQuestReward", storyQuestHudRect, 10, FontStyle.Bold, TextAnchor.LowerRight, new Color(1f, 0.86f, 0.52f, 0.96f));

        SetLocalTop(storyQuestActText.rectTransform, 14f, 12f, 10f, 16f);
        SetLocalTop(storyQuestChapterText.rectTransform, 14f, 12f, 24f, 22f);
        SetLocalStretch(storyQuestNarrativeText.rectTransform, 14f, 40f, 14f, 50f);
        SetLocalBottom(storyQuestObjectiveText.rectTransform, 14f, 12f, 96f, 30f);
        SetLocalBottom(storyQuestRewardText.rectTransform, 120f, 12f, 14f, 18f);
    }

    private void EnsureStoryLogHud()
    {
        if (storyLogHudRect != null || grillPanel == null)
        {
            return;
        }

        var hud = new GameObject("StoryLogHud", typeof(RectTransform), typeof(Image));
        storyLogHudRect = hud.GetComponent<RectTransform>();
        storyLogHudRect.SetParent(grillPanel, false);
        storyLogHudImage = hud.GetComponent<Image>();
        storyLogHudImage.raycastTarget = false;

        storyLogHeadlineText = CreateRuntimeText("StoryLogHeadline", storyLogHudRect, 10, FontStyle.Bold, TextAnchor.UpperLeft, new Color(1f, 0.86f, 0.58f, 0.98f));
        storyLogSpeakerText = CreateRuntimeText("StoryLogSpeaker", storyLogHudRect, 11, FontStyle.Bold, TextAnchor.UpperLeft, new Color(1f, 0.94f, 0.84f, 1f));
        storyLogLineText = CreateRuntimeText("StoryLogLine", storyLogHudRect, 10, FontStyle.Bold, TextAnchor.LowerLeft, new Color(0.96f, 0.90f, 0.80f, 0.96f));

        SetLocalTop(storyLogHeadlineText.rectTransform, 12f, 10f, 10f, 14f);
        SetLocalTop(storyLogSpeakerText.rectTransform, 12f, 10f, 24f, 16f);
        SetLocalStretch(storyLogLineText.rectTransform, 12f, 10f, 12f, 40f);
    }

    private void EnsureSideQuestHud()
    {
        if (sideQuestHudRect != null || grillPanel == null)
        {
            return;
        }

        var hud = new GameObject("SideQuestHud", typeof(RectTransform), typeof(Image));
        sideQuestHudRect = hud.GetComponent<RectTransform>();
        sideQuestHudRect.SetParent(grillPanel, false);
        sideQuestHudImage = hud.GetComponent<Image>();
        sideQuestHudImage.raycastTarget = false;

        sideQuestDistrictText = CreateRuntimeText("SideQuestDistrict", sideQuestHudRect, 10, FontStyle.Bold, TextAnchor.UpperLeft, new Color(1f, 0.86f, 0.58f, 0.98f));
        sideQuestSpeakerText = CreateRuntimeText("SideQuestSpeaker", sideQuestHudRect, 11, FontStyle.Bold, TextAnchor.UpperLeft, new Color(1f, 0.94f, 0.84f, 1f));
        sideQuestTitleText = CreateRuntimeText("SideQuestTitle", sideQuestHudRect, 11, FontStyle.Bold, TextAnchor.UpperLeft, new Color(0.98f, 0.92f, 0.84f, 0.98f));
        sideQuestObjectiveText = CreateRuntimeText("SideQuestObjective", sideQuestHudRect, 10, FontStyle.Bold, TextAnchor.LowerLeft, new Color(0.96f, 0.90f, 0.80f, 0.96f));
        sideQuestRewardText = CreateRuntimeText("SideQuestReward", sideQuestHudRect, 10, FontStyle.Bold, TextAnchor.LowerRight, new Color(1f, 0.86f, 0.52f, 0.96f));

        SetLocalTop(sideQuestDistrictText.rectTransform, 12f, 10f, 10f, 14f);
        SetLocalTop(sideQuestSpeakerText.rectTransform, 12f, 10f, 22f, 16f);
        SetLocalTop(sideQuestTitleText.rectTransform, 12f, 10f, 38f, 18f);
        SetLocalBottom(sideQuestObjectiveText.rectTransform, 12f, 10f, 86f, 28f);
        SetLocalBottom(sideQuestRewardText.rectTransform, 110f, 10f, 12f, 18f);
    }

    private void EnsureDistrictBackdrop()
    {
        if (districtBackdropRect != null || grillPanel == null)
        {
            return;
        }

        districtBackdropRect = new GameObject("DistrictBackdrop", typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
        districtBackdropRect.SetParent(grillPanel, false);
        districtBackdropRect.SetAsFirstSibling();
        districtBackdropImage = districtBackdropRect.GetComponent<Image>();
        districtBackdropImage.raycastTarget = false;
        districtBackdropImage.preserveAspect = false;

        districtSignImage = CreateRuntimeImage("DistrictSign", districtBackdropRect, Color.white);
        districtSignText = CreateRuntimeText("DistrictSignText", districtBackdropRect, 12, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(1f, 0.94f, 0.82f, 1f));
        festivalLightsRect = new GameObject("FestivalLights", typeof(RectTransform)).GetComponent<RectTransform>();
        festivalLightsRect.SetParent(districtBackdropRect, false);
        for (int i = 0; i < 7; i++)
        {
            var light = CreateRuntimeImage("Light" + i, festivalLightsRect, Color.white);
            light.sprite = lightBulbSprite;
            festivalLights.Add(light);
        }
        crowdRowRect = new GameObject("CrowdRow", typeof(RectTransform)).GetComponent<RectTransform>();
        crowdRowRect.SetParent(districtBackdropRect, false);
        for (int i = 0; i < 5; i++)
        {
            var crowd = CreateRuntimeImage("Crowd" + i, crowdRowRect, new Color(0.18f, 0.10f, 0.08f, 0.36f));
            crowd.sprite = crowdSprite;
            crowdSilhouettes.Add(crowd);
        }

        SetRect(districtSignImage.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-108f, 12f), new Vector2(108f, 62f));
        SetRect(districtSignText.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-100f, 18f), new Vector2(100f, 56f));
    }

    private void EnsureDistrictBadge()
    {
        if (districtBadgeRect != null || topBar == null)
        {
            return;
        }

        districtBadgeRect = new GameObject("DistrictBadge", typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
        districtBadgeRect.SetParent(topBar, false);
        districtBadgeImage = districtBadgeRect.GetComponent<Image>();
        districtBadgeImage.raycastTarget = false;

        districtBadgeTitleText = CreateRuntimeText("DistrictBadgeTitle", districtBadgeRect, 13, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(1f, 0.94f, 0.84f, 1f));
        districtBadgeSubtitleText = CreateRuntimeText("DistrictBadgeSubtitle", districtBadgeRect, 10, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.92f, 0.86f, 0.78f, 0.94f));
        SetRect(districtBadgeTitleText.rectTransform, new Vector2(0f, 0.5f), new Vector2(1f, 1f), new Vector2(8f, -22f), new Vector2(-8f, -2f));
        SetRect(districtBadgeSubtitleText.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0.5f), new Vector2(8f, 2f), new Vector2(-8f, 18f));
    }

    private void EnsureShowcaseHud()
    {
        if (showcaseHudRect != null || grillPanel == null)
        {
            return;
        }

        var hud = new GameObject("RestaurantShowcaseHud", typeof(RectTransform), typeof(Image));
        showcaseHudRect = hud.GetComponent<RectTransform>();
        showcaseHudRect.SetParent(grillPanel, false);
        showcaseHudImage = hud.GetComponent<Image>();
        showcaseHudImage.raycastTarget = false;

        showcaseTitleText = CreateRuntimeText("ShowcaseTitle", showcaseHudRect, 18, FontStyle.Bold, TextAnchor.UpperLeft, new Color(1f, 0.90f, 0.62f, 1f));
        showcasePrimaryText = CreateRuntimeText("ShowcasePrimary", showcaseHudRect, 13, FontStyle.Bold, TextAnchor.UpperLeft, new Color(0.98f, 0.94f, 0.84f, 1f));
        showcaseSecondaryText = CreateRuntimeText("ShowcaseSecondary", showcaseHudRect, 12, FontStyle.Bold, TextAnchor.UpperLeft, new Color(0.96f, 0.86f, 0.74f, 0.96f));
        showcaseFooterText = CreateRuntimeText("ShowcaseFooter", showcaseHudRect, 11, FontStyle.Bold, TextAnchor.LowerLeft, new Color(0.90f, 0.82f, 0.70f, 0.92f));

        if (showcaseTitleText != null)
        {
            SetLocalTop(showcaseTitleText.rectTransform, 16f, 16f, 10f, 24f);
        }
        if (showcasePrimaryText != null)
        {
            SetLocalTop(showcasePrimaryText.rectTransform, 16f, 16f, 34f, 26f);
        }
        if (showcaseSecondaryText != null)
        {
            SetLocalTop(showcaseSecondaryText.rectTransform, 16f, 16f, 62f, 22f);
        }
        if (showcaseFooterText != null)
        {
            SetLocalBottom(showcaseFooterText.rectTransform, 16f, 10f, 16f, 20f);
        }
    }

    private void EnsureFeverMeter()
    {
        if (feverMeterRect != null || topBar == null)
        {
            return;
        }

        feverMeterRect = new GameObject("ChefFeverMeter", typeof(RectTransform)).GetComponent<RectTransform>();
        feverMeterRect.SetParent(topBar, false);

        feverMeterBack = CreateRuntimeImage("FeverBack", feverMeterRect, new Color(0.18f, 0.10f, 0.08f, 0.86f));
        feverMeterFill = CreateRuntimeImage("FeverFill", feverMeterRect, new Color(0.76f, 0.22f, 0.16f, 0.94f));
        feverMeterFill.type = Image.Type.Filled;
        feverMeterFill.fillMethod = Image.FillMethod.Horizontal;
        feverMeterFill.fillOrigin = (int)Image.OriginHorizontal.Left;
        feverMeterFill.fillAmount = 0f;
        feverMeterText = CreateRuntimeText("FeverText", feverMeterRect, 11, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.96f, 0.88f, 0.74f, 0.96f));

        SetFullStretch(feverMeterBack.rectTransform, 0f, 0f, 0f, 0f);
        SetFullStretch(feverMeterFill.rectTransform, 0f, 0f, 0f, 0f);
        SetFullStretch(feverMeterText.rectTransform, 6f, 0f, 6f, 0f);
    }

    private void EnsureFeverOverlay()
    {
        if (feverOverlayRect != null)
        {
            return;
        }

        feverOverlayRect = new GameObject("ChefFeverOverlay", typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
        feverOverlayRect.SetParent(transform, false);
        feverOverlayRect.SetAsFirstSibling();
        feverOverlayImage = feverOverlayRect.GetComponent<Image>();
        feverOverlayImage.raycastTarget = false;
        feverOverlayImage.color = new Color(0.40f, 0.12f, 0.08f, 0f);
        SetFullStretch(feverOverlayRect, 0f, 0f, 0f, 0f);
        feverOverlayRect.gameObject.SetActive(false);
    }

    private void EnsureCelebrationBurst()
    {
        if (celebrationBurstRect != null)
        {
            return;
        }

        celebrationBurstRect = new GameObject("CelebrationBurst", typeof(RectTransform)).GetComponent<RectTransform>();
        celebrationBurstRect.SetParent(transform, false);
        celebrationBurstRect.SetAsLastSibling();
        celebrationBurstRect.anchorMin = new Vector2(0.5f, 0.5f);
        celebrationBurstRect.anchorMax = new Vector2(0.5f, 0.5f);
        celebrationBurstRect.pivot = new Vector2(0.5f, 0.5f);
        celebrationBurstRect.sizeDelta = new Vector2(0f, 0f);
        celebrationBurstRect.gameObject.SetActive(false);

        for (int i = 0; i < 12; i++)
        {
            var piece = CreateRuntimeImage("BurstPiece" + i, celebrationBurstRect, Color.white);
            piece.sprite = confettiSprite;
            celebrationBurstPieces.Add(piece);
        }
    }

    public void PlayCelebrationBurst(Color color)
    {
        EnsureCelebrationBurst();
        if (celebrationBurstRect == null)
        {
            return;
        }

        celebrationBurstColor = color;
        celebrationBurstTimer = 0.9f;
        celebrationBurstRect.gameObject.SetActive(true);
    }

    public void PlayCameraPunch(float strength, float duration = 0.35f)
    {
        cameraPunchStrength = Mathf.Max(cameraPunchStrength, Mathf.Clamp01(strength));
        cameraPunchDuration = Mathf.Max(0.08f, duration);
        cameraPunchTimer = Mathf.Max(cameraPunchTimer, cameraPunchDuration);
    }

    private void UpdateCelebrationBurst()
    {
        if (celebrationBurstRect == null || celebrationBurstTimer <= 0f)
        {
            if (celebrationBurstRect != null)
            {
                celebrationBurstRect.gameObject.SetActive(false);
            }
            return;
        }

        celebrationBurstTimer -= Time.unscaledDeltaTime;
        var n = 1f - Mathf.Clamp01(celebrationBurstTimer / 0.9f);
        for (int i = 0; i < celebrationBurstPieces.Count; i++)
        {
            var piece = celebrationBurstPieces[i];
            if (piece == null)
            {
                continue;
            }

            var angle = (Mathf.PI * 2f / celebrationBurstPieces.Count) * i + n * 0.6f;
            var radius = Mathf.Lerp(16f, 180f, n);
            var wobble = Mathf.Sin(Time.unscaledTime * 10f + i) * 8f;
            piece.rectTransform.anchoredPosition = new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius + wobble);
            piece.rectTransform.localScale = Vector3.one * Mathf.Lerp(0.6f, 1.2f, 1f - n);
            piece.color = new Color(
                Mathf.Clamp01(celebrationBurstColor.r + Mathf.Sin(i) * 0.08f),
                Mathf.Clamp01(celebrationBurstColor.g + Mathf.Cos(i * 0.7f) * 0.08f),
                Mathf.Clamp01(celebrationBurstColor.b + Mathf.Sin(i * 1.3f) * 0.08f),
                Mathf.Lerp(0.95f, 0f, n));
        }
    }

    private void AnimateDistrictBackdropScene()
    {
        if (districtBackdropRect == null)
        {
            return;
        }

        var queuePressure = gameManager != null ? Mathf.Clamp01(gameManager.GetQueueMetrics().queueCount / 6f) : 0f;
        var fever = gameManager != null && gameManager.IsChefFeverRunning() ? gameManager.GetChefFeverRemainingNormalized() : 0f;
        var punch = GetCameraPunch01() * cameraPunchStrength;
        var swayX = Mathf.Sin(Time.unscaledTime * (0.48f + showcaseHeat * 0.8f)) * (2f + queuePressure * 4f);
        var swayY = Mathf.Cos(Time.unscaledTime * (0.68f + fever * 1.2f)) * (1.5f + fever * 3f);

        districtBackdropRect.anchoredPosition = new Vector2(swayX + punch * 10f, swayY + punch * 4f);
        districtBackdropRect.localScale = Vector3.one * (1.01f + showcaseHeat * 0.03f + fever * 0.04f + punch * 0.05f);

        if (districtSignImage != null)
        {
            var signRect = districtSignImage.rectTransform;
            signRect.localScale = Vector3.one * (1f + showcaseHeat * 0.02f + fever * 0.03f + punch * 0.04f);
            signRect.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(Time.unscaledTime * 0.9f) * (1.2f + punch * 2.4f));
        }

        if (districtBadgeRect != null)
        {
            var badgePulse = 1f + Mathf.Sin(Time.unscaledTime * (1.6f + showcaseHeat * 2.2f)) * (0.01f + fever * 0.02f);
            districtBadgeRect.localScale = Vector3.one * (badgePulse + punch * 0.06f);
        }

    }

    private void EnsureHypeMeter()
    {
        if (hypeMeterRect != null || topBar == null)
        {
            return;
        }

        hypeMeterRect = new GameObject("HypeMeter", typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
        hypeMeterRect.SetParent(topBar, false);
        hypeMeterBack = hypeMeterRect.GetComponent<Image>();
        hypeMeterBack.raycastTarget = false;
        hypeMeterBack.color = new Color(0.18f, 0.10f, 0.08f, 0.86f);

        hypeMeterFill = CreateRuntimeImage("HypeFill", hypeMeterRect, new Color(0.78f, 0.24f, 0.18f, 0.94f));
        hypeMeterFill.type = Image.Type.Filled;
        hypeMeterFill.fillMethod = Image.FillMethod.Horizontal;
        hypeMeterFill.fillOrigin = (int)Image.OriginHorizontal.Left;
        hypeMeterFill.fillAmount = 0f;

        hypeMeterText = CreateRuntimeText("HypeText", hypeMeterRect, 11, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(1f, 0.92f, 0.78f, 0.96f));
        hypeDetailText = CreateRuntimeText("HypeDetail", topBar, 10, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.92f, 0.86f, 0.78f, 0.94f));

        SetFullStretch(hypeMeterFill.rectTransform, 0f, 0f, 0f, 0f);
        SetFullStretch(hypeMeterText.rectTransform, 6f, 0f, 6f, 0f);
    }

    private void EnsureMarquee()
    {
        if (marqueeRect != null || bottomBar == null)
        {
            return;
        }

        marqueeRect = new GameObject("MarqueeStrip", typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
        marqueeRect.SetParent(bottomBar, false);
        var bg = marqueeRect.GetComponent<Image>();
        bg.color = new Color(0.18f, 0.10f, 0.08f, 0.82f);
        bg.raycastTarget = false;
        marqueeText = CreateRuntimeText("MarqueeText", marqueeRect, 12, FontStyle.Bold, TextAnchor.MiddleLeft, new Color(1f, 0.90f, 0.68f, 0.96f));
        marqueeText.horizontalOverflow = HorizontalWrapMode.Overflow;
        marqueeText.verticalOverflow = VerticalWrapMode.Truncate;
        marqueeOffset = 0f;
    }

    private void UpdateMarquee()
    {
        if (marqueeRect == null || marqueeText == null || gameManager == null || bottomBar == null)
        {
            return;
        }

        var text = gameManager.GetMarqueeText();
        marqueeText.text = "  " + text + "  •  " + text + "  •  " + text + "  ";
        marqueeOffset -= Time.unscaledDeltaTime * 48f;
        if (marqueeOffset < -800f)
        {
            marqueeOffset = 0f;
        }
        marqueeText.rectTransform.anchoredPosition = new Vector2(marqueeOffset, 0f);
    }

    private void EnsureChefHypeHud()
    {
        if (chefHypeRect != null || grillPanel == null)
        {
            return;
        }

        var root = new GameObject("ChefHypeHud", typeof(RectTransform), typeof(Image));
        chefHypeRect = root.GetComponent<RectTransform>();
        chefHypeRect.SetParent(grillPanel, false);
        chefHypePanel = root.GetComponent<Image>();
        chefHypePanel.raycastTarget = false;
        chefHypePanel.color = new Color(0.16f, 0.10f, 0.08f, 0.78f);

        chefHypeSpriteImage = CreateRuntimeImage("ChefSprite", chefHypeRect, Color.white);
        chefHypeText = CreateRuntimeText("ChefText", chefHypeRect, 12, FontStyle.Bold, TextAnchor.MiddleLeft, new Color(1f, 0.92f, 0.76f, 0.98f));

        SetRect(chefHypeSpriteImage.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(10f, 10f), new Vector2(70f, -10f));
        SetRect(chefHypeText.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(76f, 10f), new Vector2(-10f, -10f));
        chefHypeRect.gameObject.SetActive(false);
    }

    private void EnsureMomentSpotlight()
    {
        if (momentSpotlightRect != null || grillPanel == null)
        {
            return;
        }

        var root = new GameObject("MomentSpotlight", typeof(RectTransform), typeof(Image));
        momentSpotlightRect = root.GetComponent<RectTransform>();
        momentSpotlightRect.SetParent(grillPanel, false);
        momentSpotlightRect.SetAsLastSibling();
        momentSpotlightPanel = root.GetComponent<Image>();
        momentSpotlightPanel.raycastTarget = false;
        momentSpotlightPanel.color = new Color(0.30f, 0.14f, 0.09f, 0f);

        momentSpotlightGlow = CreateRuntimeImage("SpotlightGlow", momentSpotlightRect, new Color(1f, 0.82f, 0.42f, 0f));
        momentSpotlightIcon = CreateRuntimeImage("SpotlightIcon", momentSpotlightRect, Color.white);
        momentSpotlightTitleText = CreateRuntimeText("SpotlightTitle", momentSpotlightRect, 18, FontStyle.Bold, TextAnchor.UpperLeft, new Color(1f, 0.94f, 0.82f, 0f));
        momentSpotlightDetailText = CreateRuntimeText("SpotlightDetail", momentSpotlightRect, 12, FontStyle.Bold, TextAnchor.MiddleLeft, new Color(0.98f, 0.88f, 0.74f, 0f));

        if (momentSpotlightGlow != null)
        {
            momentSpotlightGlow.raycastTarget = false;
            SetFullStretch(momentSpotlightGlow.rectTransform, -18f, -10f, -18f, -10f);
        }
        if (momentSpotlightIcon != null)
        {
            momentSpotlightIcon.raycastTarget = false;
            SetRect(momentSpotlightIcon.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(14f, 12f), new Vector2(70f, -12f));
        }
        if (momentSpotlightTitleText != null)
        {
            SetLocalTop(momentSpotlightTitleText.rectTransform, 82f, 14f, 12f, 24f);
        }
        if (momentSpotlightDetailText != null)
        {
            SetLocalStretch(momentSpotlightDetailText.rectTransform, 82f, 12f, 14f, 40f);
        }

        momentSpotlightRect.gameObject.SetActive(false);
    }

    private void EnsureLiveEventBanner()
    {
        if (liveEventBannerRect != null || grillPanel == null)
        {
            return;
        }

        var root = new GameObject("LiveEventBanner", typeof(RectTransform), typeof(Image));
        liveEventBannerRect = root.GetComponent<RectTransform>();
        liveEventBannerRect.SetParent(grillPanel, false);
        liveEventBannerPanel = root.GetComponent<Image>();
        liveEventBannerPanel.raycastTarget = false;
        liveEventBannerPanel.color = new Color(0.22f, 0.10f, 0.08f, 0f);

        liveEventBannerStripe = CreateRuntimeImage("LiveEventStripe", liveEventBannerRect, new Color(1f, 0.82f, 0.36f, 0.96f));
        liveEventBannerTitleText = CreateRuntimeText("LiveEventTitle", liveEventBannerRect, 14, FontStyle.Bold, TextAnchor.UpperLeft, new Color(1f, 0.94f, 0.82f, 1f));
        liveEventBannerDetailText = CreateRuntimeText("LiveEventDetail", liveEventBannerRect, 11, FontStyle.Bold, TextAnchor.UpperLeft, new Color(0.98f, 0.88f, 0.74f, 0.96f));

        if (liveEventBannerStripe != null)
        {
            SetRect(liveEventBannerStripe.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0f), new Vector2(8f, 0f));
        }
        if (liveEventBannerTitleText != null)
        {
            SetLocalTop(liveEventBannerTitleText.rectTransform, 18f, 12f, 10f, 20f);
        }
        if (liveEventBannerDetailText != null)
        {
            SetLocalStretch(liveEventBannerDetailText.rectTransform, 18f, 10f, 12f, 32f);
        }

        liveEventBannerRect.gameObject.SetActive(false);
    }

    private void EnsureBrandBoard()
    {
        if (brandBoardRect != null || grillPanel == null)
        {
            return;
        }

        var root = new GameObject("BrandBoard", typeof(RectTransform), typeof(Image));
        brandBoardRect = root.GetComponent<RectTransform>();
        brandBoardRect.SetParent(grillPanel, false);
        brandBoardPanel = root.GetComponent<Image>();
        brandBoardPanel.raycastTarget = false;
        brandBoardPanel.color = new Color(0.18f, 0.10f, 0.08f, 0.88f);

        brandBoardTitleText = CreateRuntimeText("BrandBoardTitle", brandBoardRect, 13, FontStyle.Bold, TextAnchor.UpperLeft, new Color(1f, 0.94f, 0.84f, 1f));
        brandBoardDetailText = CreateRuntimeText("BrandBoardDetail", brandBoardRect, 10, FontStyle.Bold, TextAnchor.LowerLeft, new Color(0.98f, 0.88f, 0.72f, 0.94f));
        SetLocalTop(brandBoardTitleText.rectTransform, 12f, 10f, 10f, 18f);
        SetLocalBottom(brandBoardDetailText.rectTransform, 12f, 8f, 10f, 18f);
    }

    private void EnsureTopBrandRibbon()
    {
        if (topBrandRibbonRect != null || topBar == null)
        {
            return;
        }

        var root = new GameObject("TopBrandRibbon", typeof(RectTransform), typeof(Image));
        topBrandRibbonRect = root.GetComponent<RectTransform>();
        topBrandRibbonRect.SetParent(topBar, false);
        topBrandRibbonPanel = root.GetComponent<Image>();
        topBrandRibbonPanel.raycastTarget = false;
        topBrandRibbonPanel.color = new Color(0.20f, 0.10f, 0.08f, 0.88f);

        topBrandRibbonShine = CreateRuntimeImage("TopBrandRibbonShine", topBrandRibbonRect, new Color(1f, 1f, 1f, 0.06f));
        topBrandRibbonTitleText = CreateRuntimeText("TopBrandRibbonTitle", topBrandRibbonRect, 12, FontStyle.Bold, TextAnchor.UpperCenter, new Color(1f, 0.95f, 0.84f, 1f));
        topBrandRibbonSubtitleText = CreateRuntimeText("TopBrandRibbonSubtitle", topBrandRibbonRect, 9, FontStyle.Bold, TextAnchor.LowerCenter, new Color(0.98f, 0.88f, 0.72f, 0.96f));
        SetRect(topBrandRibbonShine.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(10f, -4f), new Vector2(-10f, -16f));
        SetRect(topBrandRibbonTitleText.rectTransform, new Vector2(0f, 0.5f), new Vector2(1f, 1f), new Vector2(8f, -18f), new Vector2(-8f, -2f));
        SetRect(topBrandRibbonSubtitleText.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0.5f), new Vector2(8f, 2f), new Vector2(-8f, 12f));
    }

    private void EnsureHeroHeader()
    {
        if (heroHeaderRect != null || grillPanel == null)
        {
            return;
        }

        var root = new GameObject("HeroHeader", typeof(RectTransform), typeof(Image));
        heroHeaderRect = root.GetComponent<RectTransform>();
        heroHeaderRect.SetParent(grillPanel, false);
        heroHeaderPanel = root.GetComponent<Image>();
        heroHeaderPanel.raycastTarget = false;
        heroHeaderPanel.color = new Color(0.18f, 0.10f, 0.08f, 0.74f);

        heroHeaderShine = CreateRuntimeImage("HeroHeaderShine", heroHeaderRect, new Color(1f, 1f, 1f, 0.05f));
        heroHeaderTitleText = CreateRuntimeText("HeroHeaderTitle", heroHeaderRect, 15, FontStyle.Bold, TextAnchor.UpperCenter, new Color(1f, 0.95f, 0.86f, 1f));
        heroHeaderSubtitleText = CreateRuntimeText("HeroHeaderSubtitle", heroHeaderRect, 10, FontStyle.Bold, TextAnchor.LowerCenter, new Color(0.98f, 0.88f, 0.72f, 0.96f));
        SetRect(heroHeaderShine.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(14f, -6f), new Vector2(-14f, -20f));
        SetRect(heroHeaderTitleText.rectTransform, new Vector2(0f, 0.5f), new Vector2(1f, 1f), new Vector2(10f, -18f), new Vector2(-10f, -2f));
        SetRect(heroHeaderSubtitleText.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0.5f), new Vector2(10f, 2f), new Vector2(-10f, 12f));
    }

    private void EnsureGrillFrameAndRails()
    {
        if (grillFrameRect == null && grillPanel != null)
        {
            grillFrameRect = new GameObject("GrillFrame", typeof(RectTransform)).GetComponent<RectTransform>();
            grillFrameRect.SetParent(grillPanel, false);
            for (int i = 0; i < 4; i++)
            {
                grillFramePieces.Add(CreateRuntimeImage("FramePiece" + i, grillFrameRect, new Color(1f, 0.82f, 0.36f, 0.28f)));
            }
        }

        if (queueNeonRail == null && queuePanel != null)
        {
            queueNeonRail = CreateRuntimeImage("QueueNeonRail", queuePanel, new Color(1f, 0.82f, 0.36f, 0.22f));
        }

        if (upgradesNeonRail == null && upgradesPanel != null)
        {
            upgradesNeonRail = CreateRuntimeImage("UpgradesNeonRail", upgradesPanel, new Color(1f, 0.82f, 0.36f, 0.22f));
        }
    }

    private void EnsurePanelGlossOverlays()
    {
        if (queueGlossOverlay == null && queuePanel != null)
        {
            queueGlossOverlay = CreateRuntimeImage("QueueGlossOverlay", queuePanel, new Color(1f, 1f, 1f, 0.06f));
        }

        if (upgradesGlossOverlay == null && upgradesPanel != null)
        {
            upgradesGlossOverlay = CreateRuntimeImage("UpgradesGlossOverlay", upgradesPanel, new Color(1f, 1f, 1f, 0.06f));
        }

        if (grillGlossOverlay == null && grillPanel != null)
        {
            grillGlossOverlay = CreateRuntimeImage("GrillGlossOverlay", grillPanel, new Color(1f, 1f, 1f, 0.05f));
        }
    }

    private void EnsureStageSpotlights()
    {
        if (stageSpotlightsRect != null || grillPanel == null)
        {
            return;
        }

        stageSpotlightsRect = new GameObject("StageSpotlights", typeof(RectTransform)).GetComponent<RectTransform>();
        stageSpotlightsRect.SetParent(grillPanel, false);
        stageSpotlightsRect.SetAsFirstSibling();

        for (int i = 0; i < 3; i++)
        {
            var beam = CreateRuntimeImage("SpotlightBeam" + i, stageSpotlightsRect, new Color(1f, 0.82f, 0.38f, 0.08f));
            stageSpotlights.Add(beam);
        }
    }

    private void EnsureHeatEmbers()
    {
        if (heatEmbersRect != null || grillPanel == null)
        {
            return;
        }

        heatEmbersRect = new GameObject("HeatEmbers", typeof(RectTransform)).GetComponent<RectTransform>();
        heatEmbersRect.SetParent(grillPanel, false);
        heatEmbersRect.SetAsFirstSibling();
        for (int i = 0; i < 10; i++)
        {
            var ember = CreateRuntimeImage("Ember" + i, heatEmbersRect, new Color(1f, 0.70f, 0.26f, 0.18f));
            ember.sprite = confettiSprite;
            heatEmbers.Add(ember);
        }
    }

    private void EnsureFeverAura()
    {
        if (feverAuraRect != null || grillPanel == null)
        {
            return;
        }

        feverAuraRect = new GameObject("FeverAura", typeof(RectTransform)).GetComponent<RectTransform>();
        feverAuraRect.SetParent(grillPanel, false);
        feverAuraRect.SetAsFirstSibling();
        for (int i = 0; i < 3; i++)
        {
            feverAuraPieces.Add(CreateRuntimeImage("FeverAuraPiece" + i, feverAuraRect, new Color(1f, 0.58f, 0.18f, 0.04f)));
        }
    }

    private void EnsurePrestigeFinale()
    {
        if (prestigeFinaleRect != null)
        {
            return;
        }

        var root = new GameObject("PrestigeFinale", typeof(RectTransform), typeof(Image));
        prestigeFinaleRect = root.GetComponent<RectTransform>();
        prestigeFinaleRect.SetParent(transform, false);
        prestigeFinaleRect.SetAsLastSibling();
        prestigeFinalePanel = root.GetComponent<Image>();
        prestigeFinalePanel.raycastTarget = false;
        prestigeFinalePanel.color = new Color(0.48f, 0.18f, 0.08f, 0f);

        prestigeFinaleGlow = CreateRuntimeImage("PrestigeFinaleGlow", prestigeFinaleRect, new Color(1f, 0.86f, 0.42f, 0f));
        prestigeFinaleTitleText = CreateRuntimeText("PrestigeFinaleTitle", prestigeFinaleRect, 22, FontStyle.Bold, TextAnchor.UpperCenter, new Color(1f, 0.96f, 0.86f, 0f));
        prestigeFinaleDetailText = CreateRuntimeText("PrestigeFinaleDetail", prestigeFinaleRect, 12, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.98f, 0.90f, 0.74f, 0f));
        SetFullStretch(prestigeFinaleGlow.rectTransform, -18f, -18f, -18f, -18f);
        SetRect(prestigeFinaleTitleText.rectTransform, new Vector2(0f, 0.5f), new Vector2(1f, 1f), new Vector2(12f, -38f), new Vector2(-12f, -8f));
        SetRect(prestigeFinaleDetailText.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0.5f), new Vector2(16f, 10f), new Vector2(-16f, 28f));
        prestigeFinaleRect.gameObject.SetActive(false);
    }

    public void ShowMomentSpotlight(string title, string detail, Color color, bool useThumb = false)
    {
        EnsureMomentSpotlight();
        if (momentSpotlightRect == null)
        {
            return;
        }

        momentSpotlightColor = color;
        momentSpotlightUseThumb = useThumb;
        momentSpotlightDuration = 1.75f;
        momentSpotlightTimer = momentSpotlightDuration;
        momentSpotlightRect.gameObject.SetActive(true);

        if (momentSpotlightTitleText != null)
        {
            momentSpotlightTitleText.text = string.IsNullOrEmpty(title) ? "HOUSE MOMENT" : title;
        }
        if (momentSpotlightDetailText != null)
        {
            momentSpotlightDetailText.text = string.IsNullOrEmpty(detail) ? "Keep the grill hot." : detail;
        }
        if (momentSpotlightIcon != null)
        {
            momentSpotlightIcon.sprite = useThumb ? chefThumbSprite : chefHypeSprite;
        }
    }

    private void UpdateMomentSpotlight()
    {
        if (momentSpotlightRect == null)
        {
            return;
        }

        if (momentSpotlightTimer <= 0f)
        {
            momentSpotlightRect.gameObject.SetActive(false);
            return;
        }

        momentSpotlightTimer -= Time.unscaledDeltaTime;
        var progress = 1f - Mathf.Clamp01(momentSpotlightTimer / Mathf.Max(0.01f, momentSpotlightDuration));
        var fadeIn = Mathf.Clamp01(progress / 0.18f);
        var fadeOut = Mathf.Clamp01(momentSpotlightTimer / 0.42f);
        var alpha = Mathf.Min(fadeIn, fadeOut);
        var pulse = 1f + Mathf.Sin(Time.unscaledTime * 8f) * 0.03f;

        momentSpotlightRect.gameObject.SetActive(alpha > 0.001f);
        if (!momentSpotlightRect.gameObject.activeSelf)
        {
            return;
        }

        momentSpotlightRect.localScale = Vector3.one * pulse;
        if (momentSpotlightPanel != null)
        {
            momentSpotlightPanel.color = new Color(
                Mathf.Lerp(0.16f, momentSpotlightColor.r * 0.55f, 0.6f),
                Mathf.Lerp(0.08f, momentSpotlightColor.g * 0.35f, 0.55f),
                Mathf.Lerp(0.06f, momentSpotlightColor.b * 0.22f, 0.45f),
                0.92f * alpha);
        }
        if (momentSpotlightGlow != null)
        {
            momentSpotlightGlow.color = new Color(momentSpotlightColor.r, momentSpotlightColor.g, momentSpotlightColor.b, 0.20f * alpha);
        }
        if (momentSpotlightIcon != null)
        {
            momentSpotlightIcon.color = new Color(1f, 1f, 1f, alpha);
            momentSpotlightIcon.rectTransform.localScale = Vector3.one * (1f + (momentSpotlightUseThumb ? 0.04f : 0.08f) * fadeIn);
        }
        if (momentSpotlightTitleText != null)
        {
            momentSpotlightTitleText.color = new Color(1f, 0.95f, 0.86f, alpha);
        }
        if (momentSpotlightDetailText != null)
        {
            momentSpotlightDetailText.color = new Color(0.98f, 0.88f, 0.74f, alpha);
        }
    }

    private void AnimateHeatEmbers()
    {
        if (heatEmbersRect == null || heatEmbers.Count == 0)
        {
            return;
        }

        var fever = gameManager != null && gameManager.IsChefFeverRunning() ? gameManager.GetChefFeverRemainingNormalized() : 0f;
        var intensity = Mathf.Clamp01(showcaseHeat * 0.75f + fever * 0.45f + liveEventBannerAccent * 0.12f);
        heatEmbersRect.gameObject.SetActive(intensity > 0.08f);
        if (!heatEmbersRect.gameObject.activeSelf)
        {
            return;
        }

        for (int i = 0; i < heatEmbers.Count; i++)
        {
            var ember = heatEmbers[i];
            if (ember == null)
            {
                continue;
            }

            var seed = Time.unscaledTime * (0.9f + intensity * 1.8f) + i * 0.42f;
            var rise = Mathf.Repeat(seed * 42f, 132f);
            var x = Mathf.Sin(seed * 1.4f) * (100f + intensity * 70f);
            var scale = 0.55f + intensity * 0.8f + Mathf.Sin(seed * 2.6f) * 0.08f;
            var alpha = Mathf.Lerp(0.10f, 0.34f, intensity) * (1f - rise / 132f);
            ember.rectTransform.anchoredPosition = new Vector2(x, 8f + rise);
            ember.rectTransform.localScale = Vector3.one * scale;
            ember.color = Color.Lerp(
                new Color(1f, 0.62f, 0.20f, alpha),
                new Color(1f, 0.86f, 0.40f, alpha),
                Mathf.PingPong(seed * 0.4f, 1f));
        }
    }

    private void AnimateFeverAura()
    {
        if (feverAuraRect == null || feverAuraPieces.Count == 0)
        {
            return;
        }

        var fever = gameManager != null && gameManager.IsChefFeverRunning() ? gameManager.GetChefFeverRemainingNormalized() : 0f;
        var primed = gameManager != null && gameManager.IsChefFeverPrimed() ? 0.34f : 0f;
        var intensity = Mathf.Clamp01(fever + primed);
        feverAuraRect.gameObject.SetActive(intensity > 0.04f);
        if (!feverAuraRect.gameObject.activeSelf)
        {
            return;
        }

        for (int i = 0; i < feverAuraPieces.Count; i++)
        {
            var piece = feverAuraPieces[i];
            if (piece == null)
            {
                continue;
            }

            var seed = Time.unscaledTime * (1.2f + intensity * 3.6f) + i * 0.7f;
            var alpha = Mathf.Lerp(0.03f, 0.16f, intensity) * (0.72f + Mathf.Sin(seed) * 0.22f);
            piece.color = Color.Lerp(
                new Color(1f, 0.42f, 0.16f, alpha),
                new Color(1f, 0.80f, 0.34f, alpha),
                Mathf.PingPong(seed * 0.35f, 1f));
            piece.rectTransform.localScale = Vector3.one * (1f + intensity * 0.05f);
        }
    }

    private void AnimateStageSpotlights()
    {
        if (stageSpotlightsRect == null || stageSpotlights.Count == 0)
        {
            return;
        }

        var fever = gameManager != null && gameManager.IsChefFeverRunning() ? gameManager.GetChefFeverRemainingNormalized() : 0f;
        var beamHeat = Mathf.Clamp01(showcaseHeat * 0.55f + liveEventBannerAccent * 0.30f + liveEventBannerUrgency * 0.10f + fever * 0.28f);
        stageSpotlightsRect.gameObject.SetActive(beamHeat > 0.08f);
        if (!stageSpotlightsRect.gameObject.activeSelf)
        {
            return;
        }

        for (int i = 0; i < stageSpotlights.Count; i++)
        {
            var beam = stageSpotlights[i];
            if (beam == null)
            {
                continue;
            }

            var seed = Time.unscaledTime * (0.8f + beamHeat * 1.3f) + i * 0.9f;
            var swing = Mathf.Sin(seed) * (6f + beamHeat * 10f);
            var scaleY = 1f + beamHeat * 0.18f + Mathf.Sin(seed * 1.7f) * 0.03f;
            beam.rectTransform.localRotation = Quaternion.Euler(0f, 0f, swing);
            beam.rectTransform.localScale = new Vector3(1f, scaleY, 1f);
            beam.color = Color.Lerp(
                new Color(1f, 0.72f, 0.30f, 0.04f + beamHeat * 0.04f),
                new Color(1f, 0.92f, 0.56f, 0.08f + beamHeat * 0.10f),
                Mathf.PingPong(seed * 0.25f, 1f));
        }
    }

    private void AnimatePremiumPresentation()
    {
        var prestigeReady = gameManager != null && gameManager.CanPrestige();
        var pulse = 0.5f + Mathf.Sin(Time.unscaledTime * (3.2f + showcaseHeat * 4f)) * 0.5f;
        var edgeAlpha = Mathf.Lerp(0.14f, 0.34f, showcaseHeat * 0.6f + liveEventBannerAccent * 0.4f);
        if (prestigeReady)
        {
            pulse = 0.5f + Mathf.Sin(Time.unscaledTime * 5.2f) * 0.5f;
            edgeAlpha = Mathf.Max(edgeAlpha, 0.28f);
        }

        for (int i = 0; i < grillFramePieces.Count; i++)
        {
            var piece = grillFramePieces[i];
            if (piece == null)
            {
                continue;
            }

            piece.color = Color.Lerp(
                new Color(currentThemeAccent.r, currentThemeAccent.g, currentThemeAccent.b, edgeAlpha * 0.65f),
                new Color(currentThemeAccentStrong.r, currentThemeAccentStrong.g, currentThemeAccentStrong.b, edgeAlpha),
                pulse);
        }

        if (queueNeonRail != null)
        {
            queueNeonRail.color = Color.Lerp(
                new Color(currentThemeAccent.r, currentThemeAccent.g, currentThemeAccent.b, 0.16f),
                new Color(currentThemeAccentStrong.r, currentThemeAccentStrong.g, currentThemeAccentStrong.b, 0.28f),
                pulse);
        }

        if (upgradesNeonRail != null)
        {
            upgradesNeonRail.color = Color.Lerp(
                new Color(currentThemeAccent.r, currentThemeAccent.g, currentThemeAccent.b, 0.16f),
                new Color(currentThemeAccentStrong.r, currentThemeAccentStrong.g, currentThemeAccentStrong.b, 0.28f),
                1f - pulse * 0.35f);
        }

        if (brandBoardRect != null)
        {
            brandBoardRect.localScale = Vector3.one * (1f + Mathf.Sin(Time.unscaledTime * (1.6f + showcaseHeat * 2.2f + (prestigeReady ? 2.2f : 0f))) * (prestigeReady ? 0.018f : 0.01f));
        }

        if (topBrandRibbonRect != null)
        {
            topBrandRibbonRect.localScale = Vector3.one * (1f + Mathf.Sin(Time.unscaledTime * (1.4f + showcaseHeat * 1.8f + (prestigeReady ? 2f : 0f))) * (prestigeReady ? 0.015f : 0.008f));
        }
        if (heroHeaderRect != null)
        {
            heroHeaderRect.localScale = Vector3.one * (1f + Mathf.Sin(Time.unscaledTime * (1.9f + liveEventBannerAccent * 2.4f + (prestigeReady ? 2.4f : 0f))) * (prestigeReady ? 0.018f : 0.010f));
        }

        if (topBrandRibbonShine != null)
        {
            topBrandRibbonShine.color = new Color(1f, 1f, 1f, Mathf.Lerp(0.04f, 0.10f, pulse));
        }
        if (heroHeaderShine != null)
        {
            heroHeaderShine.color = new Color(1f, 1f, 1f, Mathf.Lerp(0.03f, 0.08f, 1f - pulse * 0.2f));
        }

        if (queueGlossOverlay != null)
        {
            queueGlossOverlay.color = new Color(1f, 1f, 1f, Mathf.Lerp(0.03f, 0.08f, pulse));
        }

        if (upgradesGlossOverlay != null)
        {
            upgradesGlossOverlay.color = new Color(1f, 1f, 1f, Mathf.Lerp(0.03f, 0.08f, 1f - pulse * 0.25f));
        }

        if (grillGlossOverlay != null)
        {
            grillGlossOverlay.color = new Color(1f, 1f, 1f, Mathf.Lerp(0.02f, 0.07f, showcaseHeat));
        }
    }

    private void TriggerPrestigeFinale()
    {
        EnsurePrestigeFinale();
        if (prestigeFinaleRect == null || gameManager == null)
        {
            return;
        }

        prestigeFinaleTimer = 2.4f;
        prestigeFinaleRect.gameObject.SetActive(true);
        PlayCelebrationBurst(new Color(1f, 0.88f, 0.42f, 1f));
        PlayCameraPunch(0.24f, 0.65f);
        if (prestigeFinaleTitleText != null)
        {
            prestigeFinaleTitleText.text = "SEASON FINALE READY";
        }
        if (prestigeFinaleDetailText != null)
        {
            prestigeFinaleDetailText.text = gameManager.GetPrestigeStatusText();
        }
    }

    private void UpdatePrestigeFinale()
    {
        if (prestigeFinaleRect == null)
        {
            return;
        }

        if (prestigeFinaleTimer <= 0f)
        {
            prestigeFinaleRect.gameObject.SetActive(false);
            return;
        }

        prestigeFinaleTimer -= Time.unscaledDeltaTime;
        var n = 1f - Mathf.Clamp01(prestigeFinaleTimer / 2.4f);
        var alpha = Mathf.Min(Mathf.Clamp01(n / 0.18f), Mathf.Clamp01(prestigeFinaleTimer / 0.45f));
        var pulse = 1f + Mathf.Sin(Time.unscaledTime * 7.5f) * 0.04f;

        prestigeFinaleRect.gameObject.SetActive(alpha > 0.001f);
        if (!prestigeFinaleRect.gameObject.activeSelf)
        {
            return;
        }

        var accent = ResolveDistrictAccentColor(gameManager != null && gameManager.GetCurrentStoreTier() != null ? gameManager.GetCurrentStoreTier().id : "alley");
        prestigeFinaleRect.localScale = Vector3.one * pulse;
        prestigeFinalePanel.color = new Color(accent.r * 0.46f, accent.g * 0.20f, accent.b * 0.16f, 0.76f * alpha);
        if (prestigeFinaleGlow != null)
        {
            prestigeFinaleGlow.color = new Color(accent.r, Mathf.Max(accent.g, 0.72f), accent.b, 0.22f * alpha);
        }
        if (prestigeFinaleTitleText != null)
        {
            prestigeFinaleTitleText.color = new Color(1f, 0.96f, 0.86f, alpha);
        }
        if (prestigeFinaleDetailText != null)
        {
            prestigeFinaleDetailText.color = new Color(0.98f, 0.90f, 0.74f, alpha);
        }
    }

    private void AnimateActionButtons()
    {
        if (gameManager == null)
        {
            return;
        }

        var queueMetrics = gameManager.GetQueueMetrics();
        var queuePressure = Mathf.Clamp01(queueMetrics.queueCount / 4f);
        var fever = gameManager.IsChefFeverRunning() ? 1f : gameManager.IsChefFeverPrimed() ? 0.45f : 0f;
        var prestige = gameManager.CanPrestige() ? 1f : 0f;
        var upgradeHeat = Mathf.Clamp01(showcaseHeat * 0.6f + liveEventBannerAccent * 0.3f);

        PulseActionButton(serveActionButton, queuePressure, currentThemeAccentStrong);
        PulseActionButton(rushActionButton, Mathf.Clamp01(queuePressure * 0.7f + liveEventBannerUrgency * 0.5f), currentThemeAccent);
        PulseActionButton(bestUpgradeActionButton, upgradeHeat, currentThemeAccent);
        PulseActionButton(boostActionButton, Mathf.Clamp01(fever + showcaseHeat * 0.2f), currentThemeAccentStrong);
        PulseActionButton(prestigeActionButton, prestige, currentThemeAccentStrong);
        PulseActionButton(shopActionButton, Mathf.Clamp01(liveEventBannerAccent * 0.35f), currentThemeAccent);
        PulseActionButton(leaderboardActionButton, Mathf.Clamp01(showcaseHeat * 0.18f), currentThemeAccent);
    }

    private void PulseActionButton(Button button, float intensity, Color hotColor)
    {
        if (button == null || !button.gameObject.activeInHierarchy)
        {
            return;
        }

        var clamped = Mathf.Clamp01(intensity);
        var image = button.targetGraphic as Image;
        if (image != null)
        {
            var baseColor = button.interactable ? button.colors.normalColor : button.colors.disabledColor;
            image.color = Color.Lerp(baseColor, hotColor, clamped * 0.28f);
        }

        var pulse = 1f + Mathf.Sin(Time.unscaledTime * (4f + clamped * 8f)) * 0.04f * clamped;
        button.transform.localScale = new Vector3(pulse, pulse, 1f);

        var label = button.GetComponentInChildren<Text>(true);
        if (label != null)
        {
            label.color = Color.Lerp(new Color(0.98f, 0.95f, 0.88f, 1f), new Color(1f, 0.96f, 0.82f, 1f), clamped * 0.4f);
        }
    }

    private void UpdateChefHypeHud()
    {
        EnsureChefHypeHud();
        if (chefHypeRect == null || gameManager == null)
        {
            return;
        }

        var feverRunning = gameManager.IsChefFeverRunning();
        if (feverRunning && !wasFeverRunning)
        {
            chefThumbTimer = 0f;
        }
        else if (!feverRunning && wasFeverRunning)
        {
            chefThumbTimer = 1.8f;
        }
        wasFeverRunning = feverRunning;

        if (chefThumbTimer > 0f)
        {
            chefThumbTimer -= Time.unscaledDeltaTime;
        }

        var showThumb = !feverRunning && chefThumbTimer > 0f;
        chefHypeRect.gameObject.SetActive(feverRunning || showThumb);
        if (!chefHypeRect.gameObject.activeSelf)
        {
            return;
        }

        chefHypeRect.anchorMin = new Vector2(0f, 0f);
        chefHypeRect.anchorMax = new Vector2(0f, 0f);
        chefHypeRect.pivot = new Vector2(0f, 0f);
        chefHypeRect.anchoredPosition = new Vector2(14f, 14f);
        chefHypeRect.sizeDelta = new Vector2(188f, 92f);

        if (feverRunning)
        {
            var bounce = 1f + Mathf.Sin(Time.unscaledTime * 8f) * 0.05f;
            chefHypeRect.localScale = new Vector3(bounce, bounce, 1f);
            chefHypePanel.color = Color.Lerp(new Color(0.24f, 0.12f, 0.08f, 0.82f), new Color(0.46f, 0.18f, 0.08f, 0.92f), gameManager.GetChefFeverRemainingNormalized());
            if (chefHypeSpriteImage != null)
            {
                chefHypeSpriteImage.sprite = chefHypeSprite;
                chefHypeSpriteImage.color = Color.white;
            }
            if (chefHypeText != null)
            {
                chefHypeText.text = "CHEF IS FEELING IT\nKeep the grill hot!";
            }
            return;
        }

        var thumbAlpha = Mathf.Clamp01(chefThumbTimer / 1.8f);
        chefHypeRect.localScale = Vector3.one * (1f + thumbAlpha * 0.04f);
        chefHypePanel.color = new Color(0.30f, 0.16f, 0.10f, Mathf.Lerp(0.36f, 0.82f, thumbAlpha));
        if (chefHypeSpriteImage != null)
        {
            chefHypeSpriteImage.sprite = chefThumbSprite;
            chefHypeSpriteImage.color = new Color(1f, 1f, 1f, thumbAlpha);
        }
        if (chefHypeText != null)
        {
            chefHypeText.text = "NICE SERVICE\nChef approves!";
            chefHypeText.color = new Color(1f, 0.92f, 0.76f, thumbAlpha);
        }
    }

    private void UpdateDistrictBackdrop(RestaurantShowcaseUiState showcase)
    {
        if (districtBackdropRect == null || gameManager == null)
        {
            return;
        }

        var tier = gameManager.GetCurrentStoreTier();
        var tierId = tier != null ? tier.id : "alley";
        var accent = ResolveDistrictAccentColor(tierId);
        if (districtBackdropImage != null)
        {
            districtBackdropImage.sprite = ResolveBackdropSprite(tierId);
            districtBackdropImage.color = new Color(1f, 1f, 1f, Mathf.Lerp(0.22f, 0.40f, showcase.heat01));
        }
        if (districtSignImage != null)
        {
            districtSignImage.sprite = districtSignSprite;
            districtSignImage.color = Color.Lerp(new Color(accent.r * 0.50f, accent.g * 0.22f, accent.b * 0.14f, 0.84f), accent, showcase.heat01);
        }
        if (districtSignText != null)
        {
            districtSignText.text = (tier != null ? tier.displayName : "Alley").ToUpperInvariant();
            districtSignText.color = Color.Lerp(new Color(0.98f, 0.92f, 0.80f, 0.94f), new Color(1f, 0.98f, 0.86f, 1f), showcase.heat01);
        }

        if (festivalLights.Count > 0)
        {
            var colors = ResolveFestivalLightColors(tierId, showcase.heat01);
            for (int i = 0; i < festivalLights.Count; i++)
            {
                if (festivalLights[i] != null)
                {
                    festivalLights[i].color = colors[i % colors.Length];
                }
            }
        }
    }

    private Sprite ResolveBackdropSprite(string tierId)
    {
        var normalized = string.IsNullOrEmpty(tierId) ? "alley" : tierId.ToLowerInvariant();
        switch (normalized)
        {
            case "hongdae":
                return districtHongdaeSprite;
            case "gangnam":
                return districtGangnamSprite;
            case "hanok":
                return districtHanokSprite;
            case "global":
                return districtGlobalSprite;
            default:
                return districtAlleySprite;
        }
    }

    private void UpdateDistrictBadge()
    {
        if (districtBadgeRect == null || gameManager == null)
        {
            return;
        }

        var tier = gameManager.GetCurrentStoreTier();
        var tierName = tier != null ? tier.displayName : "Alley";
        var tierId = tier != null ? tier.id : "alley";
        var level = gameManager.GetPlayerLevel();
        var accent = ResolveDistrictAccentColor(tierId);
        districtBadgeRect.gameObject.SetActive(true);
        districtBadgeImage.color = Color.Lerp(
            new Color(accent.r * 0.34f, accent.g * 0.16f, accent.b * 0.12f, 0.82f),
            gameManager.CanPrestige()
                ? new Color(0.58f, 0.22f, 0.08f, 0.94f)
                : accent,
            showcaseHeat);
        if (districtBadgeTitleText != null)
        {
            districtBadgeTitleText.text = gameManager.CanPrestige() ? "FINALE" : tierName.ToUpperInvariant();
        }
        if (districtBadgeSubtitleText != null)
        {
            districtBadgeSubtitleText.text = gameManager.CanPrestige()
                ? "PRESTIGE READY"
                : "LV " + level + " BBQ HOUSE";
        }
    }

    private Color[] ResolveFestivalLightColors(string tierId, float heat)
    {
        var normalized = string.IsNullOrEmpty(tierId) ? "alley" : tierId.ToLowerInvariant();
        switch (normalized)
        {
            case "hongdae":
                return new[]
                {
                    Color.Lerp(new Color(0.98f, 0.42f, 0.62f, 0.96f), new Color(1f, 0.72f, 0.84f, 1f), heat),
                    Color.Lerp(new Color(0.52f, 0.72f, 1f, 0.96f), new Color(0.82f, 0.88f, 1f, 1f), heat),
                    Color.Lerp(new Color(1f, 0.82f, 0.46f, 0.96f), new Color(1f, 0.94f, 0.68f, 1f), heat),
                };
            case "gangnam":
                return new[]
                {
                    Color.Lerp(new Color(1f, 0.88f, 0.58f, 0.96f), new Color(1f, 0.96f, 0.76f, 1f), heat),
                    Color.Lerp(new Color(0.82f, 0.88f, 1f, 0.96f), new Color(0.96f, 0.98f, 1f, 1f), heat),
                };
            case "hanok":
                return new[]
                {
                    Color.Lerp(new Color(1f, 0.78f, 0.44f, 0.96f), new Color(1f, 0.90f, 0.62f, 1f), heat),
                    Color.Lerp(new Color(0.94f, 0.68f, 0.34f, 0.96f), new Color(1f, 0.82f, 0.48f, 1f), heat),
                };
            case "global":
                return new[]
                {
                    Color.Lerp(new Color(0.62f, 0.82f, 1f, 0.96f), new Color(0.84f, 0.94f, 1f, 1f), heat),
                    Color.Lerp(new Color(1f, 0.74f, 0.52f, 0.96f), new Color(1f, 0.88f, 0.66f, 1f), heat),
                    Color.Lerp(new Color(0.98f, 0.52f, 0.70f, 0.96f), new Color(1f, 0.72f, 0.84f, 1f), heat),
                };
            default:
                return new[]
                {
                    Color.Lerp(new Color(1f, 0.76f, 0.40f, 0.96f), new Color(1f, 0.88f, 0.58f, 1f), heat),
                    Color.Lerp(new Color(1f, 0.88f, 0.54f, 0.96f), new Color(1f, 0.96f, 0.72f, 1f), heat),
                };
        }
    }

    private void AnimateFestivalLights()
    {
        if (festivalLights.Count == 0)
        {
            return;
        }

        for (int i = 0; i < festivalLights.Count; i++)
        {
            var light = festivalLights[i];
            if (light == null)
            {
                continue;
            }

            var pulse = 0.82f + Mathf.Sin(Time.unscaledTime * (3.4f + showcaseHeat * 7f) + i * 0.85f) * (0.10f + showcaseHeat * 0.10f);
            var c = light.color;
            c.a = Mathf.Clamp01(pulse);
            light.color = c;
            light.rectTransform.localScale = Vector3.one * (0.90f + pulse * 0.16f);
        }
    }

    private void AnimateCrowdRow()
    {
        if (crowdSilhouettes.Count == 0)
        {
            return;
        }

        for (int i = 0; i < crowdSilhouettes.Count; i++)
        {
            var crowd = crowdSilhouettes[i];
            if (crowd == null)
            {
                continue;
            }

            var sway = Mathf.Sin(Time.unscaledTime * (1.3f + showcaseHeat * 2.2f) + i * 0.8f) * 4f;
            var bob = Mathf.Sin(Time.unscaledTime * (2.2f + showcaseHeat * 3.2f) + i * 0.6f) * 2f;
            var pos = crowd.rectTransform.anchoredPosition;
            crowd.rectTransform.anchoredPosition = new Vector2(pos.x, bob);
            crowd.rectTransform.localScale = new Vector3(1f + sway * 0.002f, 1f, 1f);
            crowd.color = Color.Lerp(new Color(0.10f, 0.06f, 0.05f, 0.24f), new Color(0.18f, 0.08f, 0.06f, 0.42f), showcaseHeat);
        }
    }

    private void ApplyFeverWarpVisuals()
    {
        var feverIntensity = gameManager != null && gameManager.IsChefFeverRunning()
            ? gameManager.GetChefFeverRemainingNormalized()
            : gameManager != null && gameManager.IsChefFeverPrimed()
                ? 0.35f
                : 0f;
        var pulse = 1f + Mathf.Sin(Time.unscaledTime * (5f + feverIntensity * 7f)) * feverIntensity * 0.018f;
        var centerScale = 1f + feverIntensity * 0.022f;
        var sideScale = 1f - feverIntensity * 0.028f;

        ApplyPanelScale(grillPanel, centerScale * pulse);
        ApplyPanelScale(queuePanel, sideScale);
        ApplyPanelScale(upgradesPanel, sideScale);
        ApplyPanelScale(topBar, 1f - feverIntensity * 0.010f);
        ApplyPanelScale(bottomBar, 1f - feverIntensity * 0.010f);
        ApplyPanelScale(dailyMissionPanelRect, 1f - feverIntensity * 0.012f);
        ApplyPanelScale(prestigePanelRect, 1f - feverIntensity * 0.012f);
    }

    private void ApplyPanelScale(RectTransform rect, float scale)
    {
        if (rect != null)
        {
            rect.localScale = new Vector3(scale, scale, 1f);
        }
    }

    private void ApplyCameraFeel()
    {
        if (cachedCamera == null)
        {
            cachedCamera = Camera.main;
        }

        if (cachedCamera == null)
        {
            return;
        }

        if (!cameraStateReady)
        {
            cameraBasePosition = cachedCamera.transform.position;
            cameraBaseOrthoSize = cachedCamera.orthographicSize;
            cameraBaseFieldOfView = cachedCamera.fieldOfView;
            cameraStateReady = true;
        }

        var fever = gameManager != null && gameManager.IsChefFeverRunning() ? gameManager.GetChefFeverRemainingNormalized() : 0f;
        var queuePressure = gameManager != null ? Mathf.Clamp01(gameManager.GetQueueMetrics().queueCount / 6f) : 0f;
        var punch = GetCameraPunch01();
        var feverShake = Mathf.Sin(Time.unscaledTime * (12f + fever * 10f)) * 0.03f * fever;
        var pressureShake = Mathf.Sin(Time.unscaledTime * (5f + queuePressure * 6f)) * 0.01f * queuePressure;
        var punchWave = Mathf.Sin((1f - punch) * Mathf.PI * 4f) * punch * 0.07f * cameraPunchStrength;
        var shake = feverShake + pressureShake + punchWave;

        cachedCamera.transform.position = new Vector3(
            cameraBasePosition.x + shake,
            cameraBasePosition.y + shake * 0.35f + punchWave * 0.45f,
            cameraBasePosition.z);

        if (cachedCamera.orthographic)
        {
            var targetSize = cameraBaseOrthoSize * (1f - fever * 0.035f + queuePressure * 0.01f - punch * 0.02f * cameraPunchStrength);
            cachedCamera.orthographicSize = Mathf.Lerp(cachedCamera.orthographicSize, targetSize, Time.unscaledDeltaTime * 3f);
        }
        else
        {
            var targetFov = cameraBaseFieldOfView * (1f - fever * 0.03f + queuePressure * 0.008f - punch * 0.025f * cameraPunchStrength);
            cachedCamera.fieldOfView = Mathf.Lerp(cachedCamera.fieldOfView, targetFov, Time.unscaledDeltaTime * 3f);
        }

        if (cameraPunchTimer <= 0f)
        {
            cameraPunchStrength = Mathf.Lerp(cameraPunchStrength, 0f, Time.unscaledDeltaTime * 8f);
        }
    }

    private float GetCameraPunch01()
    {
        if (cameraPunchTimer <= 0f || cameraPunchDuration <= 0f)
        {
            return 0f;
        }

        return Mathf.Clamp01(cameraPunchTimer / cameraPunchDuration);
    }

    private Text CreateRuntimeText(string name, RectTransform parent, int fontSize, FontStyle fontStyle, TextAnchor alignment, Color color)
    {
        if (parent == null)
        {
            return null;
        }

        var go = new GameObject(name, typeof(RectTransform), typeof(Text), typeof(Outline));
        var rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        var text = go.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.alignment = alignment;
        text.color = color;
        text.supportRichText = true;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        text.lineSpacing = 1.05f;

        var outline = go.GetComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.28f);
        outline.effectDistance = new Vector2(1.2f, -1.2f);
        return text;
    }

    private void BuildChefSprites()
    {
        var palette = new Dictionary<char, Color32>
        {
            ['.'] = new Color32(0, 0, 0, 0),
            ['w'] = new Color32(248, 241, 222, 255),
            ['s'] = new Color32(217, 171, 136, 255),
            ['h'] = new Color32(52, 36, 28, 255),
            ['c'] = new Color32(238, 238, 238, 255),
            ['r'] = new Color32(173, 60, 42, 255),
            ['g'] = new Color32(255, 197, 75, 255),
            ['k'] = new Color32(31, 22, 18, 255),
            ['b'] = new Color32(112, 76, 48, 255),
        };

        chefIdleSprite = BuildPixelSprite("chef_idle", new[]
        {
            "................",
            "......cccc......",
            ".....cccccc.....",
            "....ccwwwccc....",
            "...ccsssssscc...",
            "...chsshhsshc...",
            "...chsssssshc...",
            "...ccsssssscc...",
            "....crrrrrc.....",
            "...crrrrrrrc....",
            "...crrbbrrrc....",
            "....rrbbbbrr....",
            ".....r....r.....",
            "....r......r....",
            "................",
            "................"
        }, palette);

        chefHypeSprite = BuildPixelSprite("chef_hype", new[]
        {
            "................",
            "......cccc......",
            ".....cccccc.....",
            "....ccwwwccc....",
            "...ccsssssscc...",
            "...chsshhsshc...",
            "...chsssssshc...",
            "...ccsssssscc...",
            "..gcrrrrrrrcg...",
            ".gcrrrrrrrrrcg..",
            "..crrbbrrrccg...",
            ".g.rrbbbbrr.g...",
            "...r..gg..r.....",
            "..r........r....",
            ".g..........g...",
            "................"
        }, palette);

        chefThumbSprite = BuildPixelSprite("chef_thumb", new[]
        {
            "................",
            "......cccc......",
            ".....cccccc.....",
            "....ccwwwccc....",
            "...ccsssssscc...",
            "...chsshhsshc...",
            "...chsssssshc...",
            "...ccsssssscc...",
            "....crrrrrc.....",
            "...crrrrrrrg....",
            "...crrbbrrrgg...",
            "....rrbbbbrrg...",
            ".....r....r.....",
            "....r......r....",
            "................",
            "................"
        }, palette);
    }

    private void BuildBackdropSprites()
    {
        var palette = new Dictionary<char, Color32>
        {
            ['.'] = new Color32(0, 0, 0, 0),
            ['n'] = new Color32(28, 24, 34, 255),
            ['b'] = new Color32(46, 40, 58, 255),
            ['c'] = new Color32(86, 68, 72, 255),
            ['w'] = new Color32(246, 230, 180, 255),
            ['y'] = new Color32(246, 194, 80, 255),
            ['r'] = new Color32(182, 70, 50, 255),
            ['p'] = new Color32(212, 102, 152, 255),
            ['g'] = new Color32(78, 138, 188, 255),
            ['h'] = new Color32(118, 92, 64, 255),
            ['m'] = new Color32(214, 204, 190, 255),
        };

        districtAlleySprite = BuildPixelSprite("district_alley", new[]
        {
            "................................",
            "....bbbb....bbbb.....bbbb......",
            "...bbccbb..bbccbb...bbccbb.....",
            "...bbccbb..bbccbb...bbccbb.....",
            "..bbbccbbbbbcccbbbbbbccbb......",
            "..bbccccccccccccccccccccbb.....",
            "..bbcwccwccccwcccwccccwcbb.....",
            "..bbccccccccccccccccccccbb.....",
            "..bbcwccwccccwcccwccccwcbb.....",
            "..bbbbbbbbbbbbbbbbbbbbbbbb.....",
            "................................",
            "................................",
            "................................",
            "................................",
            "................................",
            "................................"
        }, palette);

        districtHongdaeSprite = BuildPixelSprite("district_hongdae", new[]
        {
            "................................",
            "...pppp....pppp....gggg........",
            "..ppyypp..ppyypp..ggyggg.......",
            "..ppyypp..ppyypp..ggyggg.......",
            "..ppyypp..ppyypp..gggggg.......",
            ".pppyypppppyypppppgyygyg.......",
            ".pppywpppppywpppppgywygg.......",
            ".pppyypppppyypppppgyygyg.......",
            ".pppywpppppywpppppgywygg.......",
            ".pppppppppppppppppgggggg.......",
            "................................",
            "................................",
            "................................",
            "................................",
            "................................",
            "................................"
        }, palette);

        districtGangnamSprite = BuildPixelSprite("district_gangnam", new[]
        {
            "................................",
            "......gggg....gggg....gggg.....",
            ".....gbbbg...gbbbg...gbbbg.....",
            ".....gbwwg...gbwwg...gbwwg.....",
            "....ggbwwgg.ggbwwgg.ggbwwgg....",
            "....ggbwwgg.ggbwwgg.ggbwwgg....",
            "...gggbwwgggggbwwgggggbwwgg....",
            "...gggbwwgggggbwwgggggbwwgg....",
            "...gggbwwgggggbwwgggggbwwgg....",
            "...gggggggggggggggggggggggg....",
            "................................",
            "................................",
            "................................",
            "................................",
            "................................",
            "................................"
        }, palette);

        districtHanokSprite = BuildPixelSprite("district_hanok", new[]
        {
            "................................",
            "......hhhhhhhhhhhhhhhh.........",
            ".....hhmmmmmmmmmmmmmmhh........",
            "....hhmmmmmmmmmmmmmmmmhh.......",
            "...hhmmmrrrrrrrrrrrrmmmhh......",
            "..hhmmrrrrrrrrrrrrrrrrmmhh.....",
            "..hhmmrrwwrrwwrrwwrrwwmmhh.....",
            "..hhmmrrrrrrrrrrrrrrrrmmhh.....",
            "..hhmmrrwwrrwwrrwwrrwwmmhh.....",
            "..hhhhhhhhhhhhhhhhhhhhhhhh.....",
            "................................",
            "................................",
            "................................",
            "................................",
            "................................",
            "................................"
        }, palette);

        districtGlobalSprite = BuildPixelSprite("district_global", new[]
        {
            "................................",
            "...gggg....pppp....yyyy........",
            "..ggbbgg..ppyypp..yywwyy.......",
            "..ggbbgg..ppyypp..yywwyy.......",
            ".gggbbggg.ppyypp.yyywwyyy......",
            ".gggbwggg.ppywpp.yyywwyyy......",
            ".gggbwggggppywppgyyywwyyy......",
            ".gggbwggggppywppgyyywwyyy......",
            ".gggbwggggppywppgyyywwyyy......",
            ".gggggggggppppppggyyyyyyy......",
            "................................",
            "................................",
            "................................",
            "................................",
            "................................",
            "................................"
        }, palette);

        districtSignSprite = BuildPixelSprite("district_sign", new[]
        {
            "................................",
            ".rrrrrrrrrrrrrrrrrrrrrrrrrrrr..",
            ".rwwwwwwwwwwwwwwwwwwwwwwwwwwr..",
            ".rwwwwwwwwwwwwwwwwwwwwwwwwwwr..",
            ".rwwwwwwwwwwwwwwwwwwwwwwwwwwr..",
            ".rwwwwwwwwwwwwwwwwwwwwwwwwwwr..",
            ".rwwwwwwwwwwwwwwwwwwwwwwwwwwr..",
            ".rwwwwwwwwwwwwwwwwwwwwwwwwwwr..",
            ".rwwwwwwwwwwwwwwwwwwwwwwwwwwr..",
            ".rrrrrrrrrrrrrrrrrrrrrrrrrrrr..",
            "................................",
            "................................",
            "................................",
            "................................",
            "................................",
            "................................"
        }, palette);

        lightBulbSprite = BuildPixelSprite("festival_light", new[]
        {
            "................",
            "......yy........",
            ".....yyyy.......",
            ".....yyyy.......",
            "......yy........",
            "......bb........",
            ".......b........",
            "................",
            "................",
            "................",
            "................",
            "................",
            "................",
            "................",
            "................",
            "................"
        }, palette);

        crowdSprite = BuildPixelSprite("crowd_silhouette", new[]
        {
            "................................",
            "................................",
            ".........bbbb...................",
            "........bbbbbb....bbbb..........",
            ".......bbbbbbbb..bbbbbb.........",
            "......bbbbbbbbbbbbbbbbbb........",
            "......bbbbbbbbbbbbbbbbbb........",
            ".....bbbbbbbbbbbbbbbbbbbb.......",
            ".....bbbbbbbbbbbbbbbbbbbb.......",
            ".....bbbbbbbbbbbbbbbbbbbb.......",
            "......bbbb..bbbb..bbbb..........",
            "......bbbb..bbbb..bbbb..........",
            ".....bbbb....bb....bbbb.........",
            "................................",
            "................................",
            "................................"
        }, palette);

        confettiSprite = BuildPixelSprite("confetti_piece", new[]
        {
            "................",
            "................",
            "......yy........",
            ".....yyyy.......",
            ".....yyyy.......",
            "......yy........",
            "................",
            "................",
            "................",
            "................",
            "................",
            "................",
            "................",
            "................",
            "................",
            "................"
        }, palette);
    }

    private Image CreateRuntimeImage(string name, RectTransform parent, Color color)
    {
        if (parent == null)
        {
            return null;
        }

        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        var rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        var image = go.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private Sprite BuildPixelSprite(string name, string[] pattern, Dictionary<char, Color32> palette)
    {
        var height = pattern != null ? pattern.Length : 1;
        var width = 1;
        if (pattern != null)
        {
            for (int i = 0; i < pattern.Length; i++)
            {
                width = Mathf.Max(width, pattern[i] != null ? pattern[i].Length : 0);
            }
        }

        var tex = new Texture2D(width, Mathf.Max(1, height), TextureFormat.RGBA32, false);
        tex.name = name;
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;

        for (int y = 0; y < height; y++)
        {
            var row = pattern[height - 1 - y] ?? string.Empty;
            for (int x = 0; x < width; x++)
            {
                var key = x < row.Length ? row[x] : '.';
                Color32 color;
                if (!palette.TryGetValue(key, out color))
                {
                    color = new Color32(0, 0, 0, 0);
                }
                tex.SetPixel(x, y, color);
            }
        }

        tex.Apply(false, false);
        return Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 16f);
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

    private void ApplyDynamicTheme()
    {
        if (gameManager == null)
        {
            return;
        }

        var tierId = gameManager.GetCurrentStoreTier() != null ? gameManager.GetCurrentStoreTier().id : "alley";
        var queueCount = gameManager.GetQueueMetrics().queueCount;
        var fever = gameManager.IsChefFeverRunning();
        var palette = ResolveThemePalette(tierId, showcaseHeat, fever, queueCount);
        currentThemeAccent = palette.accent;
        currentThemeAccentStrong = palette.accentStrong;

        TintPanel(topBar, palette.topBar);
        TintPanel(bottomBar, palette.bottomBar);
        TintPanel(queuePanel, palette.sidePanel);
        TintPanel(upgradesPanel, palette.sidePanel);
        TintPanel(grillPanel, palette.grillPanel);
        TintPanel(dailyMissionPanelRect, palette.missionPanel);
        TintPanel(prestigePanelRect, palette.missionPanel);
        TintPanel(storyQuestHudRect, Color.Lerp(palette.grillPanel, palette.missionPanel, 0.24f));
        TintPanel(storyLogHudRect, Color.Lerp(palette.bottomBar, palette.grillPanel, 0.30f));
        TintPanel(sideQuestHudRect, Color.Lerp(palette.bottomBar, palette.missionPanel, 0.32f));
        TintPanel(monetizationPanelRect, Color.Lerp(palette.bottomBar, palette.missionPanel, 0.42f));
        TintPanel(topBrandRibbonRect, Color.Lerp(palette.bottomBar, palette.accent, 0.18f));
        TintPanel(heroHeaderRect, Color.Lerp(palette.bottomBar, palette.accentStrong, 0.20f));

        SetTextColor(currencyText, palette.textPrimary);
        SetTextColor(incomeText, palette.textMuted);
        SetTextColor(storeTierText, palette.accentStrong);
        SetTextColor(prestigeText, palette.textPrimary);
        SetTextColor(loginRewardText, palette.textMuted);
        SetTextColor(dailyMissionsText, palette.textPrimary);
        SetTextColor(queueText, palette.textPrimary);
        SetTextColor(queueMetricsText, palette.textMuted);
        SetTextColor(comboText, fever ? palette.accentStrong : palette.textPrimary);
        SetTextColor(storyQuestActText, palette.accentStrong);
        SetTextColor(storyQuestChapterText, palette.textPrimary);
        SetTextColor(storyQuestNarrativeText, palette.textMuted);
        SetTextColor(storyQuestObjectiveText, palette.textPrimary);
        SetTextColor(storyQuestRewardText, palette.accentStrong);
        SetTextColor(storyLogHeadlineText, palette.accentStrong);
        SetTextColor(storyLogSpeakerText, palette.textPrimary);
        SetTextColor(storyLogLineText, palette.textMuted);
        SetTextColor(sideQuestDistrictText, palette.accentStrong);
        SetTextColor(sideQuestSpeakerText, palette.textPrimary);
        SetTextColor(sideQuestTitleText, palette.textPrimary);
        SetTextColor(sideQuestObjectiveText, palette.textMuted);
        SetTextColor(sideQuestRewardText, palette.accentStrong);
        SetTextColor(hypeMeterText, palette.textPrimary);
        SetTextColor(hypeDetailText, palette.textMuted);
        SetTextColor(marqueeText, palette.accentStrong);
        SetTextColor(topBrandRibbonTitleText, palette.textPrimary);
        SetTextColor(topBrandRibbonSubtitleText, palette.textMuted);
        SetTextColor(heroHeaderTitleText, palette.textPrimary);
        SetTextColor(heroHeaderSubtitleText, palette.textMuted);

        if (marqueeRect != null && marqueeRect.GetComponent<Image>() != null)
        {
            marqueeRect.GetComponent<Image>().color = Color.Lerp(palette.bottomBar, palette.accent, 0.18f);
        }

        TryStyleButton("ServeButton", palette.accentStrong);
        TryStyleButton("RushButton", palette.accent);
        TryStyleButton("BestUpgradeButton", palette.accent);
        TryStyleButton("BoostButton", palette.accentStrong);
        TryStyleButton("PrestigeButton", palette.accent);
        TryStyleButton("LeaderboardButton", palette.bottomBar);
        TryStyleButton("ShopButton", palette.bottomBar);
        TryStyleButton("DebugToggleButton", palette.bottomBar);
    }

    private ThemePalette ResolveThemePalette(string tierId, float heat, bool fever, int queueCount)
    {
        var normalizedTier = string.IsNullOrEmpty(tierId) ? "alley" : tierId.ToLowerInvariant();
        var pressure = Mathf.Clamp01(Mathf.Max(heat, queueCount / 6f));
        var districtAccent = ResolveDistrictAccentColor(normalizedTier);
        ThemePalette palette;

        switch (normalizedTier)
        {
            case "hongdae":
                palette = new ThemePalette
                {
                    topBar = new Color(0.16f, 0.08f, 0.14f, 0.95f),
                    bottomBar = new Color(0.14f, 0.07f, 0.12f, 0.95f),
                    sidePanel = new Color(0.92f, 0.80f, 0.84f, 0.98f),
                    grillPanel = new Color(0.28f, 0.10f, 0.16f, 0.98f),
                    missionPanel = new Color(0.97f, 0.88f, 0.92f, 0.98f),
                    accent = new Color(0.84f, 0.28f, 0.46f, 1f),
                    accentStrong = new Color(0.96f, 0.42f, 0.58f, 1f),
                    textPrimary = new Color(0.98f, 0.95f, 0.92f, 1f),
                    textMuted = new Color(0.92f, 0.82f, 0.86f, 1f),
                };
                break;
            case "gangnam":
                palette = new ThemePalette
                {
                    topBar = new Color(0.08f, 0.10f, 0.13f, 0.96f),
                    bottomBar = new Color(0.07f, 0.09f, 0.11f, 0.95f),
                    sidePanel = new Color(0.87f, 0.84f, 0.76f, 0.98f),
                    grillPanel = new Color(0.18f, 0.13f, 0.10f, 0.99f),
                    missionPanel = new Color(0.94f, 0.91f, 0.82f, 0.98f),
                    accent = new Color(0.76f, 0.54f, 0.22f, 1f),
                    accentStrong = new Color(0.90f, 0.68f, 0.30f, 1f),
                    textPrimary = new Color(0.99f, 0.97f, 0.92f, 1f),
                    textMuted = new Color(0.92f, 0.88f, 0.80f, 1f),
                };
                break;
            case "hanok":
                palette = new ThemePalette
                {
                    topBar = new Color(0.12f, 0.09f, 0.07f, 0.96f),
                    bottomBar = new Color(0.10f, 0.08f, 0.06f, 0.95f),
                    sidePanel = new Color(0.90f, 0.86f, 0.78f, 0.98f),
                    grillPanel = new Color(0.23f, 0.14f, 0.09f, 0.98f),
                    missionPanel = new Color(0.95f, 0.92f, 0.84f, 0.98f),
                    accent = new Color(0.55f, 0.38f, 0.18f, 1f),
                    accentStrong = new Color(0.72f, 0.48f, 0.22f, 1f),
                    textPrimary = new Color(0.99f, 0.96f, 0.88f, 1f),
                    textMuted = new Color(0.90f, 0.84f, 0.74f, 1f),
                };
                break;
            case "global":
                palette = new ThemePalette
                {
                    topBar = new Color(0.07f, 0.09f, 0.15f, 0.96f),
                    bottomBar = new Color(0.06f, 0.08f, 0.13f, 0.95f),
                    sidePanel = new Color(0.84f, 0.88f, 0.95f, 0.98f),
                    grillPanel = new Color(0.14f, 0.12f, 0.18f, 0.99f),
                    missionPanel = new Color(0.90f, 0.93f, 0.98f, 0.98f),
                    accent = new Color(0.38f, 0.54f, 0.84f, 1f),
                    accentStrong = new Color(0.52f, 0.70f, 0.98f, 1f),
                    textPrimary = new Color(0.98f, 0.99f, 1f, 1f),
                    textMuted = new Color(0.84f, 0.90f, 0.98f, 1f),
                };
                break;
            default:
                palette = new ThemePalette
                {
                    topBar = new Color(0.13f, 0.10f, 0.09f, 0.94f),
                    bottomBar = new Color(0.11f, 0.08f, 0.07f, 0.94f),
                    sidePanel = new Color(0.93f, 0.86f, 0.73f, 0.98f),
                    grillPanel = new Color(0.27f, 0.14f, 0.10f, 0.98f),
                    missionPanel = new Color(0.96f, 0.90f, 0.79f, 0.98f),
                    accent = new Color(0.58f, 0.25f, 0.16f, 1f),
                    accentStrong = new Color(0.72f, 0.30f, 0.18f, 1f),
                    textPrimary = new Color(0.98f, 0.95f, 0.88f, 1f),
                    textMuted = new Color(0.91f, 0.86f, 0.78f, 1f),
                };
                break;
        }

        if (fever)
        {
            palette.accent = Color.Lerp(palette.accent, new Color(1f, 0.62f, 0.18f, 1f), 0.65f);
            palette.accentStrong = Color.Lerp(palette.accentStrong, new Color(1f, 0.80f, 0.32f, 1f), 0.8f);
            palette.grillPanel = Color.Lerp(palette.grillPanel, new Color(0.46f, 0.14f, 0.08f, 0.99f), 0.7f);
        }
        else
        {
            palette.accent = Color.Lerp(palette.accent, palette.accentStrong, pressure * 0.22f);
            palette.grillPanel = Color.Lerp(palette.grillPanel, palette.accent, pressure * 0.12f);
        }

        palette.topBar = Color.Lerp(palette.topBar, new Color(districtAccent.r * 0.18f, districtAccent.g * 0.16f, districtAccent.b * 0.18f, palette.topBar.a), 0.28f);
        palette.bottomBar = Color.Lerp(palette.bottomBar, new Color(districtAccent.r * 0.16f, districtAccent.g * 0.14f, districtAccent.b * 0.16f, palette.bottomBar.a), 0.24f);
        palette.sidePanel = Color.Lerp(palette.sidePanel, new Color(
            Mathf.Lerp(palette.sidePanel.r, districtAccent.r, 0.18f),
            Mathf.Lerp(palette.sidePanel.g, districtAccent.g, 0.10f),
            Mathf.Lerp(palette.sidePanel.b, districtAccent.b, 0.10f),
            palette.sidePanel.a), 0.32f);
        palette.missionPanel = Color.Lerp(palette.missionPanel, new Color(
            Mathf.Lerp(palette.missionPanel.r, districtAccent.r, 0.12f),
            Mathf.Lerp(palette.missionPanel.g, districtAccent.g, 0.08f),
            Mathf.Lerp(palette.missionPanel.b, districtAccent.b, 0.08f),
            palette.missionPanel.a), 0.28f);

        return palette;
    }

    private void SetTextColor(Text text, Color color)
    {
        if (text != null)
        {
            text.color = color;
        }
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

    private void TintSlider(Slider slider, Color fillColor, Color backgroundColor)
    {
        if (slider == null)
        {
            return;
        }

        if (slider.fillRect != null)
        {
            var fill = slider.fillRect.GetComponent<Image>();
            if (fill != null)
            {
                fill.color = fillColor;
            }
        }

        if (slider.targetGraphic is Image background)
        {
            background.color = backgroundColor;
        }
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
            canvasScaler.referenceResolution = landscape
                ? (compact ? new Vector2(1366f, 768f) : new Vector2(1920f, 1080f))
                : (compact ? new Vector2(720f, 1280f) : new Vector2(1080f, 1920f));
            canvasScaler.matchWidthOrHeight = landscape
                ? (compact ? 0.44f : (ultraWide ? 0.62f : 0.52f))
                : (compact ? 0.50f : 0.68f);
        }

        var canvas = GetComponent<Canvas>();
        var scaleFactor = canvas != null ? Mathf.Max(0.01f, canvas.scaleFactor) : 1f;
        var safeArea = Screen.safeArea;
        var safeLeft = safeArea.xMin / scaleFactor;
        var safeRight = Mathf.Max(0f, Screen.width - safeArea.xMax) / scaleFactor;
        var safeBottom = safeArea.yMin / scaleFactor;
        var safeTop = Mathf.Max(0f, Screen.height - safeArea.yMax) / scaleFactor;

        var margin = panelMargin + (compact ? -8f : 2f);
        var topHeight = landscape ? (compact ? 92f : 128f) : (compact ? 98f : 188f);
        var bottomHeight = landscape ? (compact ? 102f : 176f) : (compact ? 124f : 256f);

        var availableWidthRaw = uiWidth - safeLeft - safeRight - margin * 2f;
        var minAvailableWidth = landscape ? (compact ? 420f : 760f) : (compact ? 260f : 480f);
        var availableWidth = Mathf.Max(minAvailableWidth, availableWidthRaw);
        var leftWidth = landscape
            ? Mathf.Clamp(availableWidth * 0.21f, compact ? 120f : 280f, compact ? 230f : 420f)
            : Mathf.Clamp(availableWidth * 0.24f, compact ? 70f : 220f, compact ? 138f : 340f);
        var rightWidth = landscape
            ? Mathf.Clamp(availableWidth * 0.27f, compact ? 140f : 340f, compact ? 280f : 520f)
            : Mathf.Clamp(availableWidth * 0.24f, compact ? 70f : 220f, compact ? 138f : 340f);

        var centerMinimum = landscape ? (compact ? 180f : 340f) : (compact ? 140f : 220f);
        var maxSideTotal = availableWidth - centerMinimum;
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

        var missionWidth = landscape ? (compact ? 190f : 320f) : (compact ? 170f : 360f);
        var missionHeight = landscape ? (compact ? 88f : 144f) : (compact ? 110f : 190f);
        var prestigeWidth = landscape ? (compact ? 170f : 280f) : (compact ? 148f : 320f);
        var prestigeHeight = landscape ? (compact ? 82f : 132f) : (compact ? 96f : 168f);
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
        LayoutDistrictBadge(landscape, compact);
        LayoutBottomBarChildren(landscape);
        LayoutQueuePanelChildren();
        LayoutUpgradesPanelChildren();
        LayoutDistrictBackdrop(landscape, compact);
        LayoutSessionGoalHud(landscape, compact);
        LayoutStoryQuestHud(landscape, compact);
        LayoutStoryLogHud(landscape, compact);
        LayoutSideQuestHud(landscape, compact);
        LayoutShowcaseHud(landscape, compact);
        LayoutLiveEventBanner(landscape, compact);
        LayoutMomentSpotlight(landscape, compact);
        LayoutHypeMeter(landscape, compact);
        LayoutMarquee(landscape, compact);
    }

    private void LayoutDistrictBackdrop(bool landscape, bool compact)
    {
        if (districtBackdropRect == null || grillPanel == null)
        {
            return;
        }

        districtBackdropRect.SetParent(grillPanel, worldPositionStays: false);
        districtBackdropRect.anchorMin = new Vector2(0f, 0f);
        districtBackdropRect.anchorMax = new Vector2(1f, 1f);
        districtBackdropRect.pivot = new Vector2(0.5f, 0.5f);
        districtBackdropRect.offsetMin = new Vector2(10f, 10f);
        districtBackdropRect.offsetMax = new Vector2(-10f, -10f);

        if (districtBackdropImage != null)
        {
            SetFullStretch(districtBackdropImage.rectTransform, 0f, 0f, 0f, 0f);
        }
        if (grillGlossOverlay != null)
        {
            SetRect(grillGlossOverlay.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(18f, -8f), new Vector2(-18f, -(compact ? 70f : 96f)));
        }
        if (feverAuraRect != null)
        {
            feverAuraRect.SetParent(grillPanel, worldPositionStays: false);
            feverAuraRect.anchorMin = new Vector2(0f, 0f);
            feverAuraRect.anchorMax = new Vector2(1f, 1f);
            feverAuraRect.pivot = new Vector2(0.5f, 0.5f);
            feverAuraRect.offsetMin = new Vector2(0f, 0f);
            feverAuraRect.offsetMax = new Vector2(0f, 0f);
            if (feverAuraPieces.Count >= 3)
            {
                SetRect(feverAuraPieces[0].rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, -(compact ? 90f : 130f)), new Vector2(compact ? 46f : 62f, compact ? 90f : 130f));
                SetRect(feverAuraPieces[1].rectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-(compact ? 46f : 62f), -(compact ? 90f : 130f)), new Vector2(0f, compact ? 90f : 130f));
                SetRect(feverAuraPieces[2].rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-(compact ? 120f : 180f), -(compact ? 52f : 66f)), new Vector2(compact ? 120f : 180f, 0f));
            }
        }
        if (grillFrameRect != null)
        {
            grillFrameRect.SetParent(grillPanel, worldPositionStays: false);
            grillFrameRect.anchorMin = new Vector2(0f, 0f);
            grillFrameRect.anchorMax = new Vector2(1f, 1f);
            grillFrameRect.pivot = new Vector2(0.5f, 0.5f);
            grillFrameRect.offsetMin = new Vector2(4f, 4f);
            grillFrameRect.offsetMax = new Vector2(-4f, -4f);

            if (grillFramePieces.Count >= 4)
            {
                SetRect(grillFramePieces[0].rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(12f, -6f), new Vector2(-12f, 0f));
                SetRect(grillFramePieces[1].rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(12f, 0f), new Vector2(-12f, 6f));
                SetRect(grillFramePieces[2].rectTransform, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 12f), new Vector2(6f, -12f));
                SetRect(grillFramePieces[3].rectTransform, new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(-6f, 12f), new Vector2(0f, -12f));
            }
        }
        if (stageSpotlightsRect != null)
        {
            stageSpotlightsRect.SetParent(grillPanel, worldPositionStays: false);
            stageSpotlightsRect.anchorMin = new Vector2(0.5f, 1f);
            stageSpotlightsRect.anchorMax = new Vector2(0.5f, 1f);
            stageSpotlightsRect.pivot = new Vector2(0.5f, 1f);
            stageSpotlightsRect.anchoredPosition = new Vector2(0f, -6f);
            stageSpotlightsRect.sizeDelta = new Vector2(compact ? 260f : 420f, compact ? 180f : 260f);

            var width = stageSpotlightsRect.rect.width > 0f ? stageSpotlightsRect.rect.width : (compact ? 260f : 420f);
            var offsets = new[] { -width * 0.28f, 0f, width * 0.28f };
            for (int i = 0; i < stageSpotlights.Count; i++)
            {
                if (stageSpotlights[i] == null)
                {
                    continue;
                }

                var half = compact ? 34f : 46f;
                SetRect(
                    stageSpotlights[i].rectTransform,
                    new Vector2(0.5f, 1f),
                    new Vector2(0.5f, 1f),
                    new Vector2(offsets[Mathf.Min(i, offsets.Length - 1)] - half, -(compact ? 10f : 12f)),
                    new Vector2(offsets[Mathf.Min(i, offsets.Length - 1)] + half, compact ? -140f : -210f));
            }
        }
        if (festivalLightsRect != null)
        {
            festivalLightsRect.SetParent(districtBackdropRect, worldPositionStays: false);
            festivalLightsRect.anchorMin = new Vector2(0f, 1f);
            festivalLightsRect.anchorMax = new Vector2(1f, 1f);
            festivalLightsRect.pivot = new Vector2(0.5f, 1f);
            festivalLightsRect.offsetMin = new Vector2(18f, -42f);
            festivalLightsRect.offsetMax = new Vector2(-18f, -8f);

            var width = districtBackdropRect.rect.width > 0f ? districtBackdropRect.rect.width - 36f : (compact ? 220f : 320f);
            var spacing = width / Mathf.Max(1, festivalLights.Count);
            for (int i = 0; i < festivalLights.Count; i++)
            {
                if (festivalLights[i] == null)
                {
                    continue;
                }

                var x = spacing * i + spacing * 0.5f;
                var wobble = Mathf.Sin(Time.unscaledTime * 1.4f + i) * 2.5f;
                SetRect(
                    festivalLights[i].rectTransform,
                    new Vector2(0f, 1f),
                    new Vector2(0f, 1f),
                    new Vector2(x - 12f, -28f + wobble),
                    new Vector2(x + 12f, -2f + wobble));
            }
        }
        if (crowdRowRect != null)
        {
            crowdRowRect.SetParent(districtBackdropRect, worldPositionStays: false);
            crowdRowRect.anchorMin = new Vector2(0f, 0f);
            crowdRowRect.anchorMax = new Vector2(1f, 0f);
            crowdRowRect.pivot = new Vector2(0.5f, 0f);
            crowdRowRect.offsetMin = new Vector2(18f, 58f);
            crowdRowRect.offsetMax = new Vector2(-18f, compact ? 110f : 126f);

            var width = districtBackdropRect.rect.width > 0f ? districtBackdropRect.rect.width - 36f : (compact ? 220f : 320f);
            var spacing = width / Mathf.Max(1, crowdSilhouettes.Count);
            for (int i = 0; i < crowdSilhouettes.Count; i++)
            {
                if (crowdSilhouettes[i] == null)
                {
                    continue;
                }

                var x = spacing * i + spacing * 0.5f;
                SetRect(
                    crowdSilhouettes[i].rectTransform,
                    new Vector2(0f, 0f),
                    new Vector2(0f, 0f),
                    new Vector2(x - 28f, 0f),
                    new Vector2(x + 28f, compact ? 54f : 66f));
            }
        }
        if (heatEmbersRect != null)
        {
            heatEmbersRect.SetParent(grillPanel, worldPositionStays: false);
            heatEmbersRect.anchorMin = new Vector2(0.5f, 0f);
            heatEmbersRect.anchorMax = new Vector2(0.5f, 0f);
            heatEmbersRect.pivot = new Vector2(0.5f, 0f);
            heatEmbersRect.anchoredPosition = new Vector2(0f, compact ? 56f : 72f);
            heatEmbersRect.sizeDelta = new Vector2(compact ? 220f : 360f, compact ? 132f : 180f);
        }
        if (districtBackdropRect != null)
        {
            var baseIndex = districtBackdropRect.GetSiblingIndex();
            if (stageSpotlightsRect != null)
            {
                stageSpotlightsRect.SetSiblingIndex(baseIndex + 1);
            }
            if (heatEmbersRect != null)
            {
                heatEmbersRect.SetSiblingIndex(baseIndex + 2);
            }
            if (grillFrameRect != null)
            {
                grillFrameRect.SetSiblingIndex(baseIndex + 3);
            }
            if (heroHeaderRect != null)
            {
                heroHeaderRect.SetSiblingIndex(baseIndex + 5);
            }
        }
        if (districtSignImage != null)
        {
            SetRect(
                districtSignImage.rectTransform,
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(-(compact ? 86f : 120f), 10f),
                new Vector2(compact ? 86f : 120f, compact ? 48f : 60f));
        }
        if (districtSignText != null)
        {
            SetRect(
                districtSignText.rectTransform,
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(-(compact ? 80f : 112f), 16f),
                new Vector2(compact ? 80f : 112f, compact ? 42f : 54f));
        }
        if (brandBoardRect != null)
        {
            brandBoardRect.SetParent(grillPanel, worldPositionStays: false);
            brandBoardRect.anchorMin = new Vector2(0f, 1f);
            brandBoardRect.anchorMax = new Vector2(0f, 1f);
            brandBoardRect.pivot = new Vector2(0f, 1f);
            brandBoardRect.anchoredPosition = new Vector2(compact ? 12f : 16f, compact ? -10f : -14f);
            brandBoardRect.sizeDelta = new Vector2(compact ? 150f : 200f, compact ? 42f : 52f);
        }
        if (heroHeaderRect != null)
        {
            heroHeaderRect.SetParent(grillPanel, worldPositionStays: false);
            heroHeaderRect.anchorMin = new Vector2(0.5f, 1f);
            heroHeaderRect.anchorMax = new Vector2(0.5f, 1f);
            heroHeaderRect.pivot = new Vector2(0.5f, 1f);
            heroHeaderRect.anchoredPosition = new Vector2(0f, compact ? -70f : -82f);
            heroHeaderRect.sizeDelta = new Vector2(compact ? 220f : (landscape ? 320f : 280f), compact ? 38f : 48f);
        }
    }

    private void LayoutTopBarFields(bool landscape)
    {
        var compactTop = topBar != null && topBar.rect.height < 140f;
        var row1 = landscape ? (compactTop ? 30f : 42f) : (compactTop ? 24f : 56f);
        var row2 = landscape ? (compactTop ? 64f : 88f) : (compactTop ? 58f : 132f);
        var slotWidth = landscape ? (compactTop ? 220f : 320f) : (compactTop ? 130f : 260f);
        var slotHeight = landscape ? (compactTop ? 30f : 38f) : (compactTop ? 24f : 54f);

        PlaceTopText(currencyText, 0.16f, row1, slotWidth, slotHeight, TextAnchor.MiddleLeft);
        PlaceTopText(incomeText, 0.16f, row2, slotWidth, slotHeight, TextAnchor.MiddleLeft);
        PlaceTopText(storeTierText, 0.50f, row1, compactTop ? 150f : 280f, slotHeight, TextAnchor.MiddleCenter);
        PlaceTopText(prestigeText, 0.50f, row2, compactTop ? 150f : 280f, slotHeight, TextAnchor.MiddleCenter);
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
                rect.anchoredPosition = new Vector2(0f, compactTop ? 6f : (landscape ? 10f : 14f));
                rect.sizeDelta = new Vector2(
                    compactTop ? 220f : (landscape ? 400f : 460f),
                    compactTop ? 12f : (landscape ? 16f : 22f));
            }
        }

        if (feverMeterRect != null)
        {
            feverMeterRect.SetParent(topBar, worldPositionStays: false);
            feverMeterRect.anchorMin = new Vector2(0.5f, 0f);
            feverMeterRect.anchorMax = new Vector2(0.5f, 0f);
            feverMeterRect.pivot = new Vector2(0.5f, 0f);
            feverMeterRect.anchoredPosition = new Vector2(0f, compactTop ? 26f : (landscape ? 28f : 34f));
            feverMeterRect.sizeDelta = new Vector2(
                compactTop ? 200f : (landscape ? 280f : 340f),
                compactTop ? 16f : (landscape ? 20f : 24f));
        }
    }

    private void LayoutDistrictBadge(bool landscape, bool compact)
    {
        if (districtBadgeRect == null || topBar == null)
        {
            return;
        }

        districtBadgeRect.SetParent(topBar, worldPositionStays: false);
        districtBadgeRect.anchorMin = new Vector2(0.5f, 1f);
        districtBadgeRect.anchorMax = new Vector2(0.5f, 1f);
        districtBadgeRect.pivot = new Vector2(0.5f, 1f);
        districtBadgeRect.anchoredPosition = new Vector2(0f, compact ? -8f : -10f);
        districtBadgeRect.sizeDelta = new Vector2(compact ? 150f : (landscape ? 220f : 200f), compact ? 40f : 50f);

        if (topBrandRibbonRect != null)
        {
            topBrandRibbonRect.SetParent(topBar, worldPositionStays: false);
            topBrandRibbonRect.anchorMin = new Vector2(0.5f, 1f);
            topBrandRibbonRect.anchorMax = new Vector2(0.5f, 1f);
            topBrandRibbonRect.pivot = new Vector2(0.5f, 1f);
            topBrandRibbonRect.anchoredPosition = new Vector2(0f, compact ? -52f : -60f);
            topBrandRibbonRect.sizeDelta = new Vector2(compact ? 210f : (landscape ? 280f : 240f), compact ? 28f : 34f);
        }
    }

    private void LayoutSessionGoalHud(bool landscape, bool compact)
    {
        if (sessionGoalHudRect == null || grillPanel == null)
        {
            return;
        }

        sessionGoalHudRect.anchorMin = new Vector2(0.5f, 1f);
        sessionGoalHudRect.anchorMax = new Vector2(0.5f, 1f);
        sessionGoalHudRect.pivot = new Vector2(0.5f, 1f);
        sessionGoalHudRect.anchoredPosition = new Vector2(0f, compact ? -8f : -12f);
        sessionGoalHudRect.sizeDelta = landscape
            ? (compact ? new Vector2(360f, 72f) : new Vector2(460f, 82f))
            : new Vector2(compact ? 260f : 320f, compact ? 92f : 104f);
    }

    private void LayoutStoryQuestHud(bool landscape, bool compact)
    {
        if (storyQuestHudRect == null || grillPanel == null)
        {
            return;
        }

        storyQuestHudRect.anchorMin = new Vector2(0f, 0f);
        storyQuestHudRect.anchorMax = new Vector2(0f, 0f);
        storyQuestHudRect.pivot = new Vector2(0f, 0f);
        storyQuestHudRect.anchoredPosition = new Vector2(compact ? 12f : 16f, compact ? 112f : 124f);
        storyQuestHudRect.sizeDelta = landscape
            ? (compact ? new Vector2(230f, 96f) : new Vector2(290f, 116f))
            : new Vector2(compact ? 210f : 250f, compact ? 108f : 124f);
    }

    private void LayoutStoryLogHud(bool landscape, bool compact)
    {
        if (storyLogHudRect == null || grillPanel == null)
        {
            return;
        }

        storyLogHudRect.anchorMin = new Vector2(0f, 0f);
        storyLogHudRect.anchorMax = new Vector2(0f, 0f);
        storyLogHudRect.pivot = new Vector2(0f, 0f);
        storyLogHudRect.anchoredPosition = new Vector2(compact ? 12f : 16f, compact ? 214f : 248f);
        storyLogHudRect.sizeDelta = landscape
            ? (compact ? new Vector2(230f, 78f) : new Vector2(290f, 90f))
            : new Vector2(compact ? 210f : 250f, compact ? 82f : 94f);
    }

    private void LayoutSideQuestHud(bool landscape, bool compact)
    {
        if (sideQuestHudRect == null || grillPanel == null)
        {
            return;
        }

        sideQuestHudRect.anchorMin = new Vector2(0f, 0f);
        sideQuestHudRect.anchorMax = new Vector2(0f, 0f);
        sideQuestHudRect.pivot = new Vector2(0f, 0f);
        sideQuestHudRect.anchoredPosition = new Vector2(compact ? 12f : 16f, compact ? 300f : 350f);
        sideQuestHudRect.sizeDelta = landscape
            ? (compact ? new Vector2(230f, 88f) : new Vector2(290f, 100f))
            : new Vector2(compact ? 210f : 250f, compact ? 92f : 108f);
    }

    private void LayoutShowcaseHud(bool landscape, bool compact)
    {
        if (showcaseHudRect == null || grillPanel == null)
        {
            return;
        }

        showcaseHudRect.anchorMin = new Vector2(1f, 0f);
        showcaseHudRect.anchorMax = new Vector2(1f, 0f);
        showcaseHudRect.pivot = new Vector2(1f, 0f);
        showcaseHudRect.anchoredPosition = new Vector2(compact ? -10f : -14f, compact ? 10f : 14f);
        showcaseHudRect.sizeDelta = landscape
            ? (compact ? new Vector2(280f, 118f) : new Vector2(340f, 136f))
            : new Vector2(compact ? 220f : 280f, compact ? 104f : 122f);
    }

    private void LayoutLiveEventBanner(bool landscape, bool compact)
    {
        if (liveEventBannerRect == null || grillPanel == null)
        {
            return;
        }

        liveEventBannerRect.SetParent(grillPanel, worldPositionStays: false);
        liveEventBannerRect.anchorMin = new Vector2(0f, 1f);
        liveEventBannerRect.anchorMax = new Vector2(0f, 1f);
        liveEventBannerRect.pivot = new Vector2(0f, 1f);
        liveEventBannerRect.anchoredPosition = new Vector2(compact ? 10f : 14f, compact ? -92f : -104f);
        liveEventBannerRect.sizeDelta = landscape
            ? (compact ? new Vector2(250f, 56f) : new Vector2(320f, 64f))
            : new Vector2(compact ? 210f : 260f, compact ? 52f : 60f);
    }

    private void LayoutMomentSpotlight(bool landscape, bool compact)
    {
        if (momentSpotlightRect == null || grillPanel == null)
        {
            return;
        }

        momentSpotlightRect.SetParent(grillPanel, worldPositionStays: false);
        momentSpotlightRect.anchorMin = new Vector2(0.5f, 0.5f);
        momentSpotlightRect.anchorMax = new Vector2(0.5f, 0.5f);
        momentSpotlightRect.pivot = new Vector2(0.5f, 0.5f);
        momentSpotlightRect.anchoredPosition = new Vector2(0f, landscape ? 18f : 42f);
        momentSpotlightRect.sizeDelta = landscape
            ? (compact ? new Vector2(280f, 84f) : new Vector2(360f, 96f))
            : new Vector2(compact ? 240f : 300f, compact ? 82f : 92f);
    }

    private void LayoutHypeMeter(bool landscape, bool compact)
    {
        if (hypeMeterRect == null || topBar == null)
        {
            return;
        }

        hypeMeterRect.SetParent(topBar, worldPositionStays: false);
        hypeMeterRect.anchorMin = new Vector2(0.16f, 0f);
        hypeMeterRect.anchorMax = new Vector2(0.16f, 0f);
        hypeMeterRect.pivot = new Vector2(0.5f, 0f);
        hypeMeterRect.anchoredPosition = new Vector2(0f, compact ? 8f : 10f);
        hypeMeterRect.sizeDelta = new Vector2(
            compact ? 160f : (landscape ? 220f : 240f),
            compact ? 18f : 22f);

        if (hypeDetailText != null)
        {
            hypeDetailText.rectTransform.SetParent(topBar, worldPositionStays: false);
            hypeDetailText.rectTransform.anchorMin = new Vector2(0.16f, 0f);
            hypeDetailText.rectTransform.anchorMax = new Vector2(0.16f, 0f);
            hypeDetailText.rectTransform.pivot = new Vector2(0.5f, 0f);
            hypeDetailText.rectTransform.anchoredPosition = new Vector2(0f, compact ? 28f : 34f);
            hypeDetailText.rectTransform.sizeDelta = new Vector2(
                compact ? 220f : (landscape ? 300f : 320f),
                compact ? 16f : 18f);
        }
    }

    private void LayoutMarquee(bool landscape, bool compact)
    {
        if (marqueeRect == null || bottomBar == null || marqueeText == null)
        {
            return;
        }

        marqueeRect.SetParent(bottomBar, worldPositionStays: false);
        marqueeRect.anchorMin = new Vector2(0.5f, 0.5f);
        marqueeRect.anchorMax = new Vector2(0.5f, 0.5f);
        marqueeRect.pivot = new Vector2(0.5f, 0.5f);
        marqueeRect.anchoredPosition = new Vector2(0f, compact ? -18f : -26f);
        marqueeRect.sizeDelta = new Vector2(
            compact ? 260f : (landscape ? 620f : 480f),
            compact ? 20f : 24f);

        marqueeText.rectTransform.SetParent(marqueeRect, worldPositionStays: false);
        marqueeText.rectTransform.anchorMin = new Vector2(0f, 0.5f);
        marqueeText.rectTransform.anchorMax = new Vector2(0f, 0.5f);
        marqueeText.rectTransform.pivot = new Vector2(0f, 0.5f);
        marqueeText.rectTransform.anchoredPosition = new Vector2(marqueeOffset, 0f);
        marqueeText.rectTransform.sizeDelta = new Vector2(1400f, compact ? 18f : 22f);
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

        if (queueNeonRail != null)
        {
            SetRect(queueNeonRail.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 10f), new Vector2(8f, -10f));
        }
        if (queueGlossOverlay != null)
        {
            SetRect(queueGlossOverlay.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(10f, -8f), new Vector2(-10f, -64f));
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

        if (upgradesNeonRail != null)
        {
            SetRect(upgradesNeonRail.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(-8f, 10f), new Vector2(0f, -10f));
        }
        if (upgradesGlossOverlay != null)
        {
            SetRect(upgradesGlossOverlay.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(10f, -8f), new Vector2(-10f, -64f));
        }
    }

    private void LayoutBottomBarChildren(bool landscape)
    {
        var bestButtonRect = FindRectTransformByName("BestUpgradeButton");
        var boostButtonRect = FindRectTransformByName("BoostButton");
        var compactBottom = bottomBar != null && bottomBar.rect.width < 620f;

        if (landscape && !compactBottom)
        {
            PlaceBottomButton(bestButtonRect, -170f, 0f, 210f, 62f);
            PlaceBottomButton(boostButtonRect, 90f, 0f, 250f, 62f);
        }
        else
        {
            if (compactBottom)
            {
                PlaceBottomButton(bestButtonRect, -62f, 0f, 120f, 44f);
                PlaceBottomButton(boostButtonRect, 62f, 0f, 128f, 44f);
            }
            else
            {
                PlaceBottomButton(bestButtonRect, 0f, 34f, 280f, 66f);
                PlaceBottomButton(boostButtonRect, 0f, -38f, 320f, 66f);
            }
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
