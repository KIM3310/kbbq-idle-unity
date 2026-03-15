using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager I { get; private set; }

    public enum RewardSource
    {
        Default,
        Ad,
        Purchase
    }

    [Header("Data (optional assets)")]
    [SerializeField] private List<MenuItem> menuItems = new List<MenuItem>();
    [SerializeField] private List<UpgradeData> upgradesData = new List<UpgradeData>();
    [SerializeField] private List<StoreTier> storeTiers = new List<StoreTier>();
    [SerializeField] private List<CustomerType> customerTypes = new List<CustomerType>();
    [SerializeField] private ApiConfig apiConfig;
    [SerializeField] private EconomyTuning economyTuning;
    [SerializeField] private MonetizationConfig monetizationConfig;
    [SerializeField] private GameDataCatalog dataCatalog;

    [Header("Managers")]
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private UIController uiController;
    [SerializeField] private NetworkService networkService;
    [SerializeField] private AnalyticsService analyticsService;
    [SerializeField] private MonetizationService monetizationService;

    [Header("Manual Boost")]
    [SerializeField] private float manualBoostMultiplier = 2f;
    [SerializeField] private float manualBoostDuration = 3f;
    [SerializeField] private int maxOfflineHours = 8;
    [SerializeField] private int dailyMissionsPerDay = 3;

    [Header("Queue Controls")]
    [SerializeField] private float rushServiceMultiplier = 2f;
    [SerializeField] private float rushServiceDuration = 3f;
    [SerializeField] private float shiftEventMinDelay = 42f;
    [SerializeField] private float shiftEventMaxDelay = 78f;
    [SerializeField] private float shiftEventDuration = 28f;

    [Header("Kitchen Gameplay")]
    [SerializeField] private int grillSlotCount = 4;
    [SerializeField] private float grillCookSeconds = 7f;
    [SerializeField] private float grillBurnSeconds = 12f;
    [SerializeField] private float grillFlipReadySeconds = 3f;
    [SerializeField] private int starterRawStockPerUnlockedMenu = 2;
    [SerializeField] private float meatBuyCostFactor = 0.95f;
    [SerializeField] private float grilledMeatSaleFactor = 1.15f;
    [SerializeField] private float chefFeverDuration = 12f;
    [SerializeField] private float chefFeverServeBonus = 1.35f;

    private EconomySystem economy;
    private UpgradeSystem upgradeSystem;
    private MenuSystem menuSystem;
    private StoreTierSystem storeTierSystem;
    private CustomerSystem customerSystem;
    private PrestigeSystem prestigeSystem;
    private ProgressionSystem progressionSystem;
    private OfflineEarnings offlineEarnings;
    private DailyLoginSystem dailyLoginSystem;
    private DailyMissionSystem dailyMissionSystem;
    private StoryQuestSystem storyQuestSystem;
    private DistrictSideQuestSystem districtSideQuestSystem;
    private StoryGuestDirector storyGuestDirector;
    private SaveSystem saveSystem;
    private GameStateMachine stateMachine;
    private TutorialSystem tutorialSystem;
    private SaveData saveData;
    private float missionRefreshTimer = 0f;
    private float secondaryUiTimer = 0f;
    private readonly Dictionary<string, MeatInventoryState> meatInventory = new Dictionary<string, MeatInventoryState>();
    private GrillSlotStateRuntime[] grillSlots = Array.Empty<GrillSlotStateRuntime>();
    private ShiftEventType activeShiftEvent = ShiftEventType.None;
    private float shiftEventTimer;
    private float shiftEventCooldownTimer;
    private float chefFeverTimer;
    private int dailySpecialServeStreak;

    private struct MeatInventoryState
    {
        public int raw;
        public int cooked;
    }

    private struct GrillSlotStateRuntime
    {
        public string menuId;
        public float cookTime;
        public bool flipped;
    }

    private enum ShiftEventType
    {
        None,
        LunchRush,
        HappyHour,
        CriticNight
    }

    private void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }

        I = this;
        var root = transform.root != null ? transform.root.gameObject : gameObject;
        DontDestroyOnLoad(root);

        saveSystem = new SaveSystem();
        saveData = saveSystem.Load();

        EnsureDefaultData();
        InitializeSystems();

        // Attach FloatingTextSystem to GameManager root so it persists
        if (FloatingTextSystem.I == null)
        {
            gameObject.AddComponent<FloatingTextSystem>();
        }
    }

    private async void Start()
    {
        stateMachine.TransitionTo(GameState.Boot);
        stateMachine.TransitionTo(saveData.tutorialCompleted ? GameState.MainLoop : GameState.Tutorial);
        ApplyOfflineEarnings();
        TryDailyLogin();
        dailyMissionSystem?.EnsureMissionsForToday(economy.IncomePerSec);
        ResetShiftEventCooldown();
        RefreshUI();
        tutorialSystem?.Start();
        uiController?.UpdateSessionGoal(GetSessionGoalUiState());
        uiController?.UpdateShowcase(GetRestaurantShowcaseUiState());
        await EnsureNetworkAuth();
    }

    private void Update()
    {
        if (stateMachine.State == GameState.Pause || stateMachine.State == GameState.OfflineCalc)
        {
            return;
        }

        economy.Tick(Time.deltaTime);
        customerSystem.Tick(Time.deltaTime, (float)upgradeSystem.GetCategoryMultiplier("service"), menuSystem);
        TickKitchen(Time.deltaTime);
        TickShiftEvents(Time.deltaTime);
        TickChefFever(Time.deltaTime);
        uiController?.UpdateEconomy(economy.Currency, economy.IncomePerSec);
        uiController?.UpdateSatisfaction(customerSystem.Satisfaction);
        UpdateRestaurantMood();

        missionRefreshTimer -= Time.deltaTime;
        if (missionRefreshTimer <= 0f)
        {
            dailyMissionSystem?.EnsureMissionsForToday(economy.IncomePerSec);
            missionRefreshTimer = 30f;
        }

        secondaryUiTimer -= Time.deltaTime;
        if (secondaryUiTimer <= 0f)
        {
            RefreshSecondaryUI();
            secondaryUiTimer = 0.2f;
        }
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused)
        {
            Save();
        }
        else
        {
            ApplyOfflineEarnings();
            TryDailyLogin();
            dailyMissionSystem?.EnsureMissionsForToday(economy.IncomePerSec);
            RefreshUI();
        }
    }

    private void OnApplicationQuit()
    {
        Save();
    }

    public void TriggerSizzleBoost()
    {
        var sizzleMultiplier = upgradeSystem != null ? (float)upgradeSystem.GetCategoryMultiplier("sizzle") : 1f;
        economy.ApplyBoost(manualBoostMultiplier * sizzleMultiplier, manualBoostDuration);
        dailyMissionSystem?.RecordBoost();
        storyQuestSystem?.RecordBoost();
        districtSideQuestSystem?.RecordBoost();
        ProcessStoryQuestUpdates();
        ProcessSideQuestUpdates();
        analyticsService?.LogBoost();
        audioManager?.PlayBoost();
        HapticUtil.Light();
        tutorialSystem?.OnBoost();
        RefreshSecondaryUI();
    }

    public bool PurchaseUpgrade(string upgradeId)
    {
        var success = upgradeSystem.PurchaseUpgrade(upgradeId, economy);
        if (success)
        {
            dailyMissionSystem?.RecordUpgrade();
            storyQuestSystem?.RecordUpgrade();
            districtSideQuestSystem?.RecordUpgrade();
            ProcessStoryQuestUpdates();
            ProcessSideQuestUpdates();
            analyticsService?.LogUpgrade(upgradeId, upgradeSystem.GetLevel(upgradeId));
            audioManager?.PlayUpgrade();
            HapticUtil.Light();
            tutorialSystem?.OnUpgrade();
            RefreshUI();
        }
        return success;
    }

    public void CompleteTutorial()
    {
        saveData.tutorialCompleted = true;
        stateMachine.TransitionTo(GameState.MainLoop);
    }

    public double GetCurrency() => economy.Currency;
    public double GetIncomePerSec() => economy.IncomePerSec;
    public double GetTotalEarned() => economy.TotalEarned;
    public int GetPlayerLevel() => saveData.playerLevel;
    public StoreTier GetCurrentStoreTier() => storeTierSystem.CurrentTier;
    public float GetPrestigeProgress01()
    {
        var levelProgress = Mathf.Clamp01(saveData.playerLevel / 10f);
        var incomeProgress = Mathf.Clamp01((float)(saveData.totalIncome / 50000d));
        return Mathf.Clamp01(Mathf.Min(levelProgress, incomeProgress));
    }
    public HypeUiState GetHypeUiState()
    {
        var queueMetrics = customerSystem != null ? customerSystem.GetMetrics() : default;
        var queuePressure = Mathf.Clamp01(queueMetrics.queueCount / 6f);
        var satisfaction = customerSystem != null ? customerSystem.Satisfaction : 0.5f;
        var comboHeat = customerSystem != null ? Mathf.Clamp01(customerSystem.ComboCount / 6f) : 0f;
        var fever = IsChefFeverActive() ? 1f : 0f;
        var eventHeat = activeShiftEvent != ShiftEventType.None ? 0.7f : 0f;
        var specialHeat = Mathf.Clamp01(dailySpecialServeStreak / 4f);
        var fill = Mathf.Clamp01(satisfaction * 0.35f + comboHeat * 0.22f + fever * 0.22f + eventHeat * 0.12f + specialHeat * 0.09f);
        var alert = Mathf.Clamp01(queuePressure * 0.55f + (1f - satisfaction) * 0.30f + (activeShiftEvent == ShiftEventType.LunchRush ? 0.15f : 0f));

        string detail;
        if (IsChefFeverActive())
        {
            detail = "The room is roaring. Every clean plate is feeding the legend.";
        }
        else if (activeShiftEvent == ShiftEventType.CriticNight)
        {
            detail = "Critics are watching. One perfect plate can spike tonight's reputation.";
        }
        else if (queueMetrics.queueCount >= 4)
        {
            detail = "Pressure is climbing. Keep the queue moving before patience collapses.";
        }
        else if (dailySpecialServeStreak >= 3)
        {
            detail = "The crowd is locked onto today's special. Keep the streak alive.";
        }
        else
        {
            detail = "Steady service is building a stronger house reputation.";
        }

        return new HypeUiState
        {
            headline = IsChefFeverActive() ? "KITCHEN HYPE MAX" : "HOUSE REPUTATION",
            detail = detail,
            fill01 = fill,
            alert01 = alert,
        };
    }
    public LiveEventBannerUiState GetLiveEventBannerUiState()
    {
        var queueMetrics = customerSystem != null ? customerSystem.GetMetrics() : default;
        if (IsChefFeverActive())
        {
            return new LiveEventBannerUiState
            {
                title = "CHEF FEVER LIVE",
                detail = "Perfect plates and clean combos are in overdrive right now.",
                accent01 = 1f,
                urgency01 = 0.96f,
                visible = true,
            };
        }

        if (activeShiftEvent != ShiftEventType.None)
        {
            return new LiveEventBannerUiState
            {
                title = GetShiftEventLabel(),
                detail = BuildShiftEventGoal().detail,
                accent01 = activeShiftEvent == ShiftEventType.CriticNight ? 0.92f : 0.78f,
                urgency01 = activeShiftEvent == ShiftEventType.LunchRush ? 0.94f : 0.72f,
                visible = true,
            };
        }

        if (IsChefFeverPrimed())
        {
            return new LiveEventBannerUiState
            {
                title = "FEVER READY",
                detail = "One more clean push can send the kitchen into bonus mode.",
                accent01 = 0.82f,
                urgency01 = 0.70f,
                visible = true,
            };
        }

        if (dailySpecialServeStreak >= 3)
        {
            return new LiveEventBannerUiState
            {
                title = "SPECIAL STREAK x" + dailySpecialServeStreak,
                detail = "The room wants " + GetDailySpecialMenuName() + ". Keep feeding the streak for bonus tips.",
                accent01 = 0.74f,
                urgency01 = 0.62f,
                visible = true,
            };
        }

        if (queueMetrics.queueCount >= 4)
        {
            return new LiveEventBannerUiState
            {
                title = "ROOM PRESSURE",
                detail = "The queue is stacking. Keep two grill slots hot and clear tables fast.",
                accent01 = 0.58f,
                urgency01 = 0.84f,
                visible = true,
            };
        }

        if (CanPrestige())
        {
            return new LiveEventBannerUiState
            {
                title = "SEASON FINALE READY",
                detail = "You can prestige now and relaunch the restaurant with a stronger opening.",
                accent01 = 0.86f,
                urgency01 = 0.68f,
                visible = true,
            };
        }

        return default;
    }
    public StoryQuestUiState GetStoryQuestUiState()
    {
        SyncStoryQuestMeta(false);
        return storyQuestSystem != null ? storyQuestSystem.GetUiState() : default;
    }
    public StoryLogUiState GetStoryLogUiState()
    {
        SyncStoryQuestMeta(false);
        return storyQuestSystem != null ? storyQuestSystem.GetStoryLogUiState() : default;
    }
    public DistrictSideQuestUiState GetDistrictSideQuestUiState()
    {
        SyncStoryQuestMeta(false);
        return districtSideQuestSystem != null ? districtSideQuestSystem.GetUiState() : default;
    }
    public string GetMarqueeText()
    {
        var showcase = GetRestaurantShowcaseUiState();
        var hype = GetHypeUiState();
        return showcase.title + "  •  " + showcase.primary + "  •  " + hype.detail + "  •  " + GetPrestigeStatusText();
    }
    public bool IsChefFeverRunning() => chefFeverTimer > 0f;
    public float GetChefFeverRemainingNormalized() => chefFeverDuration > 0f ? Mathf.Clamp01(chefFeverTimer / chefFeverDuration) : 0f;
    public bool IsChefFeverPrimed()
    {
        return !IsChefFeverRunning() && customerSystem != null && customerSystem.ComboCount >= 4;
    }
    public string GetPrestigeStatusText()
    {
        if (prestigeSystem == null)
        {
            return "Prestige service offline";
        }

        var reward = prestigeSystem.CalculateReward(saveData.totalIncome, saveData.playerLevel);
        if (reward.canPrestige)
        {
            return "Ready to prestige for +" + reward.points + " spice stars.";
        }

        var levelGap = Mathf.Max(0, 10 - saveData.playerLevel);
        var incomeGap = System.Math.Max(0d, 50000d - saveData.totalIncome);
        if (levelGap > 0)
        {
            return "Reach Lv " + (saveData.playerLevel + levelGap) + " to unlock prestige.";
        }
        return "Earn " + FormatUtil.FormatCurrency(incomeGap) + " more total sales to prestige.";
    }
    public RestaurantShowcaseUiState GetRestaurantShowcaseUiState()
    {
        var tier = GetCurrentStoreTier();
        var specialName = GetDailySpecialMenuName();
        var canPrestige = prestigeSystem != null && prestigeSystem.CalculateReward(saveData.totalIncome, saveData.playerLevel).canPrestige;
        var nextTier = GetNextStoreTier();
        var eventLabel = GetShiftEventLabel();
        var eventDetail = activeShiftEvent != ShiftEventType.None
            ? eventLabel + " · " + Mathf.CeilToInt(Mathf.Max(0f, shiftEventTimer)) + "s left"
            : IsChefFeverActive()
                ? "CHEF FEVER · " + Mathf.CeilToInt(Mathf.Max(0f, chefFeverTimer)) + "s left"
                : "Open kitchen · steady service";
        var nextMove = canPrestige
            ? "Prestige is ready. Cash out the brand and restart stronger."
            : nextTier != null
                ? "Next district: " + nextTier.displayName + " at Lv " + nextTier.unlockLevel
                : "Final district reached. Chase perfect serves and critic nights.";

        return new RestaurantShowcaseUiState
        {
            title = specialName.ToUpperInvariant(),
            primary = "TODAY'S SPECIAL · +22% payout on " + specialName +
                      (dailySpecialServeStreak >= 2 ? " · STREAK x" + dailySpecialServeStreak : string.Empty),
            secondary = "District " + (tier != null ? tier.displayName : "Alley") + " · " + eventDetail,
            footer = dailySpecialServeStreak >= 3
                ? "Hot streak active. Keep serving " + specialName + " for escalating bonus tips."
                : nextMove,
            heat01 = IsChefFeverActive() ? 1f : (activeShiftEvent != ShiftEventType.None ? 0.92f : (canPrestige ? 0.76f : 0.44f)),
        };
    }
    public SessionGoalUiState GetSessionGoalUiState()
    {
        if (tutorialSystem != null && tutorialSystem.IsActive)
        {
            return new SessionGoalUiState
            {
                headline = "FIRST SHIFT",
                detail = tutorialSystem.CurrentPrompt.Replace("\n", " "),
                accentLabel = "guide",
                urgency01 = 0.28f,
            };
        }

        if (activeShiftEvent != ShiftEventType.None)
        {
            return BuildShiftEventGoal();
        }

        if (IsChefFeverActive())
        {
            return new SessionGoalUiState
            {
                headline = "CHEF FEVER",
                detail = "The grill is on fire. Chain clean serves now while the fever payout is active.",
                accentLabel = "fever",
                urgency01 = 0.98f,
            };
        }

        var queueMetrics = customerSystem != null ? customerSystem.GetMetrics() : default;
        var queueCount = queueMetrics.queueCount;
        var cookedCount = GetTotalCookedStock();
        var rawCount = GetTotalRawStock();
        var nextTier = GetNextStoreTier();
        var dailySpecial = GetDailySpecialMenuName();
        var nextLevel = Mathf.Max(saveData.playerLevel + 1, 2);
        var nextRequirement = progressionSystem != null
            ? progressionSystem.GetNextLevelRequirement(saveData.playerLevel)
            : saveData.totalIncome;
        var remainingIncome = System.Math.Max(0d, nextRequirement - saveData.totalIncome);

        if (HasBurnRiskSlot())
        {
            return new SessionGoalUiState
            {
                headline = "FLIP NOW",
                detail = "A cut is close to burning. Flip or collect it before you lose sale value.",
                accentLabel = "hot",
                urgency01 = 0.96f,
            };
        }

        if (HasReadyGrillSlot())
        {
            return new SessionGoalUiState
            {
                headline = "PLATE THE MEAT",
                detail = "You have cooked meat ready. Collect it now to keep the service chain moving.",
                accentLabel = "serve",
                urgency01 = 0.84f,
            };
        }

        if (queueCount > 0 && cookedCount > 0)
        {
            return new SessionGoalUiState
            {
                headline = "SERVE THE QUEUE",
                detail = "Hot meat is ready and customers are waiting. Fast serving raises combo and tips.",
                accentLabel = "rush",
                urgency01 = Mathf.Clamp01(queueCount / 5f + 0.2f),
            };
        }

        if (rawCount <= 1)
        {
            return new SessionGoalUiState
            {
                headline = "RESTOCK THE FRIDGE",
                detail = "Buy more meat now so the next rush wave does not stall your grill slots.",
                accentLabel = "prep",
                urgency01 = 0.62f,
            };
        }

        if (queueCount >= 4)
        {
            return new SessionGoalUiState
            {
                headline = "RUSH HOUR",
                detail = "Queue pressure is climbing. Keep slots occupied and use boost if the line gets longer.",
                accentLabel = "heat",
                urgency01 = 0.88f,
            };
        }

        var nextTierText = nextTier != null
            ? ("Next district " + nextTier.displayName + " unlocks at Lv " + nextTier.unlockLevel + ".")
            : "You already reached the final district tier.";
        return new SessionGoalUiState
        {
            headline = "BUILD THE HYPE",
            detail = "Today's hot pick is " + dailySpecial + ". Earn " + FormatUtil.FormatCurrency(remainingIncome) + " more to reach Lv " + nextLevel + ". " + nextTierText,
            accentLabel = "goal",
            urgency01 = 0.36f,
        };
    }
    public IReadOnlyList<CustomerQueueEntry> GetQueueSnapshot() => customerSystem != null ? customerSystem.Queue : null;
    public List<UpgradeUiEntry> GetUpgradeUiEntries() => BuildUpgradeUiEntries();
    public QueueMetrics GetQueueMetrics() => customerSystem != null ? customerSystem.GetMetrics() : default;
    public float GetQueueSpawnMultiplier() => customerSystem != null ? customerSystem.SpawnRateMultiplier : 1f;
    public float GetQueueServiceMultiplier() => customerSystem != null ? customerSystem.ServiceRateMultiplier : 1f;
    public AudioManager GetAudioManager() => audioManager;
    public MonetizationService GetMonetizationService() => monetizationService;
    public NetworkService GetNetworkService() => networkService;
    public GameplayReviewPack GetGameplayReviewPack()
    {
        var metrics = GetQueueMetrics();
        var tier = GetCurrentStoreTier();
        var config = monetizationService != null ? monetizationService.Config : monetizationConfig;
        var adsEnabled = config != null && config.enableAds;
        var iapEnabled = config != null && config.enableIap;
        var packCount = config != null && config.packs != null ? config.packs.Count : 0;
        var presetLabel = BuildPresetLabel(GetDebugPresetIndex());

        return new GameplayReviewPack
        {
            contract = "kbbq-idle-review-pack-v1",
            headline = "Gameplay loop and monetization posture are visible from the live overlay.",
            storeTier = tier != null && !string.IsNullOrEmpty(tier.displayName) ? tier.displayName : "Alley",
            playerLevel = GetPlayerLevel(),
            incomePerSecond = GetIncomePerSec(),
            totalEarned = GetTotalEarned(),
            queueCount = metrics.queueCount,
            servedPerMinute = metrics.servedPerMinute,
            averageWaitSeconds = metrics.avgWaitSeconds,
            monetizationMode = BuildMonetizationModeLabel(adsEnabled, iapEnabled, packCount),
            reviewStep = "Check grill flow, queue pressure, then ad/IAP posture.",
            focusedRoute = "Review Pack -> preset 2.0x rush -> grill loop -> perf overlay",
            reviewerSnapshot = $"Tier {(tier != null && !string.IsNullOrEmpty(tier.displayName) ? tier.displayName : "Alley")} / Queue {metrics.queueCount} / Monetize {BuildMonetizationModeLabel(adsEnabled, iapEnabled, packCount)}",
            focusedOpsSnapshot = $"Preset {presetLabel} / Queue {metrics.queueCount} / Wait {metrics.avgWaitSeconds:0.0}s / Served {metrics.servedPerMinute:0}/min",
            twoMinuteReview = "Health/meta -> review-pack -> grill loop -> perf overlay",
            reviewRoutes = "Health, Meta, Review Pack, Rush Preset, Perf Overlay",
            proofAssets = "Health, Meta, Review Pack, Perf Overlay",
        };
    }
    private string BuildPresetLabel(int index)
    {
        switch (index)
        {
            case 0:
                return "0.5x";
            case 1:
                return "1.0x";
            case 2:
                return "2.0x";
            case 3:
                return "Custom";
            default:
                return "1.0x";
        }
    }
    public int GetGrillSlotCount() => grillSlots != null && grillSlots.Length > 0 ? grillSlots.Length : Mathf.Max(1, grillSlotCount);
    public int GetUpgradeVisualTier()
    {
        if (upgradeSystem == null || upgradesData == null || upgradesData.Count == 0)
        {
            return 0;
        }

        var totalLevel = 0;
        for (int i = 0; i < upgradesData.Count; i++)
        {
            var id = upgradesData[i] != null ? upgradesData[i].id : null;
            if (string.IsNullOrEmpty(id))
            {
                continue;
            }
            totalLevel += Mathf.Max(0, upgradeSystem.GetLevel(id));
        }

        if (totalLevel >= 26) return 3;
        if (totalLevel >= 14) return 2;
        if (totalLevel >= 6) return 1;
        return 0;
    }
    public IReadOnlyList<GrillSlotUiState> GetGrillSlotsUi() => BuildGrillSlotUiStates();
    public List<MeatInventoryUiEntry> GetMeatInventoryUiEntries() => BuildMeatInventoryUiEntries();
    public GrillSlotUiState GetGrillSlotUiState(int slotIndex)
    {
        var slots = BuildGrillSlotUiStates();
        if (slotIndex < 0 || slotIndex >= slots.Count)
        {
            return default;
        }
        return slots[slotIndex];
    }

    public void AddPlayerLevels(int amount)
    {
        saveData.playerLevel += Mathf.Max(0, amount);
        menuSystem.UnlockByLevel(saveData.playerLevel);
        storeTierSystem.TryAdvanceTier(saveData.playerLevel);
        EnsureKitchenStockForUnlockedMenus();
        RefreshUI();
    }

    private string BuildMonetizationModeLabel(bool adsEnabled, bool iapEnabled, int packCount)
    {
        var adsLabel = adsEnabled ? "Ads on" : "Ads off";
        var iapLabel = iapEnabled ? "IAP on" : "IAP off";
        return adsLabel + " / " + iapLabel + " / Packs " + packCount;
    }

    public void Save()
    {
        saveData.currency = economy.Currency;
        saveData.totalIncome = economy.TotalEarned;
        saveData.storeTierIndex = storeTierSystem.CurrentTierIndex;
        saveData.unlockedMenuIds = menuSystem.GetUnlockedIds();
        saveData.upgradeLevels = upgradeSystem.ExportLevels();
        saveData.prestigeLevel = prestigeSystem.PrestigeLevel;
        saveData.prestigePoints = prestigeSystem.PrestigePoints;
        saveData.spawnRateMultiplier = customerSystem != null ? customerSystem.SpawnRateMultiplier : 1f;
        saveData.serviceRateMultiplier = customerSystem != null ? customerSystem.ServiceRateMultiplier : 1f;
        saveData.debugPanelVisible = uiController != null && uiController.IsDebugPanelVisible();
        saveData.perfOverlayVisible = uiController != null && uiController.IsPerfOverlayVisible();
        saveData.debugPresetIndex = uiController != null ? uiController.GetDebugPresetIndex() : saveData.debugPresetIndex;
        saveData.debugVisibilityInitialized = true;
        saveData.meatInventory = ExportMeatInventory();
        saveData.grillSlots = ExportGrillSlots();
        saveData.lastOnlineTs = TimeUtil.UtcNowUnix();
        saveData.Sanitize();
        saveSystem.Save(saveData);
    }

    private void ApplyOfflineEarnings()
    {
        var now = TimeUtil.UtcNowUnix();
        if (saveData.lastOnlineTs <= 0)
        {
            saveData.lastOnlineTs = now;
            return;
        }

        var offline = offlineEarnings.Calculate(saveData.lastOnlineTs, economy.IncomePerSec, maxOfflineHours);
        if (offline > 0)
        {
            economy.AddCurrency(offline);
            PresentOfflineReturn(offline);
        }

        saveData.lastOnlineTs = now;
        RefreshUI();
    }

    private void PresentOfflineReturn(double offline)
    {
        if (offline <= 0d)
        {
            return;
        }

        var specialName = GetDailySpecialMenuName();
        var burstColor = offline >= System.Math.Max(40d, economy.IncomePerSec * 75d)
            ? new Color(1f, 0.92f, 0.52f, 1f)
            : new Color(1f, 0.82f, 0.38f, 1f);

        audioManager?.PlayCoin();
        HapticUtil.Light();
        uiController?.ShowGrillStatus(
            "Welcome back. After-hours sales banked " + FormatUtil.FormatCurrency(offline) +
            ". " + specialName + " is today's headliner.");
        uiController?.PlayCelebrationBurst(burstColor);
        uiController?.PlayCameraPunch(0.16f, 0.55f);
        uiController?.ShowMomentSpotlight(
            "AFTER HOURS CASH-IN",
            "Offline sales stacked " + FormatUtil.FormatCurrency(offline) + ". Reopen with " + specialName + ".",
            burstColor);
        if (FloatingTextSystem.I != null)
        {
            FloatingTextSystem.I.Spawn(
                "OFFLINE +" + FormatUtil.FormatCurrency(offline),
                new Vector2(Screen.width * 0.5f, Screen.height * 0.68f),
                burstColor,
                1.28f);
        }

        if (saveData.tutorialCompleted &&
            activeShiftEvent == ShiftEventType.None &&
            saveData.playerLevel >= 2 &&
            offline >= System.Math.Max(15d, economy.IncomePerSec * 45d))
        {
            StartShiftEvent(ShiftEventType.HappyHour);
        }
    }

    private void RefreshUI()
    {
        SyncStoryQuestMeta(false);
        TryQueuePendingStoryGuest();
        uiController?.UpdateEconomy(economy.Currency, economy.IncomePerSec);
        uiController?.UpdateSatisfaction(customerSystem.Satisfaction);
        uiController?.UpdateStoreTier(storeTierSystem.CurrentTier);
        uiController?.UpdatePrestige(prestigeSystem.PrestigeLevel, prestigeSystem.PrestigePoints);
        uiController?.UpdateDailyMissions(saveData.dailyMissions);
        uiController?.UpdateStoryQuest(GetStoryQuestUiState());
        uiController?.UpdateStoryLog(GetStoryLogUiState());
        uiController?.UpdateSideQuest(GetDistrictSideQuestUiState());
        uiController?.UpdateSessionGoal(GetSessionGoalUiState());
        uiController?.UpdateShowcase(GetRestaurantShowcaseUiState());
        uiController?.UpdateLiveEventBanner(GetLiveEventBannerUiState());
        uiController?.RefreshGrill();
        RefreshSecondaryUI();
    }

    private void RefreshSecondaryUI()
    {
        SyncStoryQuestMeta(false);
        TryQueuePendingStoryGuest();
        uiController?.UpdateQueue(customerSystem.Queue);
        uiController?.UpdateQueueMetrics(customerSystem.GetMetrics());
        uiController?.UpdateUpgrades(BuildUpgradeUiEntries());
        uiController?.UpdateStoryQuest(GetStoryQuestUiState());
        uiController?.UpdateStoryLog(GetStoryLogUiState());
        uiController?.UpdateSideQuest(GetDistrictSideQuestUiState());
        uiController?.UpdateCombo(customerSystem.ComboCount, customerSystem.ComboTimeRemaining, customerSystem.ComboDuration, customerSystem.GetComboMultiplier());
        uiController?.UpdateSessionGoal(GetSessionGoalUiState());
        uiController?.UpdateShowcase(GetRestaurantShowcaseUiState());
        uiController?.UpdateLiveEventBanner(GetLiveEventBannerUiState());
        uiController?.RefreshGrill();
    }

    private List<UpgradeUiEntry> BuildUpgradeUiEntries()
    {
        var list = new List<UpgradeUiEntry>();
        if (upgradesData == null || upgradeSystem == null || economy == null)
        {
            return list;
        }

        string bestId = null;
        double bestScore = -1;

        foreach (var upgrade in upgradesData)
        {
            if (upgrade == null)
            {
                continue;
            }

            var level = upgradeSystem.GetLevel(upgrade.id);
            var cost = upgradeSystem.GetUpgradeCost(upgrade.id);
            var name = !string.IsNullOrEmpty(upgrade.displayName) ? upgrade.displayName : upgrade.id;
            var weight = GetUpgradeWeight(upgrade.category);
            var score = cost > 0 ? (upgrade.effectValue * weight) / cost : 0;
            var affordable = economy.Currency >= cost;
            if (affordable && score > bestScore)
            {
                bestScore = score;
                bestId = upgrade.id;
            }
            list.Add(new UpgradeUiEntry
            {
                id = upgrade.id,
                displayName = name,
                badgeText = BuildUpgradeBadge(upgrade),
                impactText = BuildUpgradeImpactText(upgrade),
                level = level,
                cost = cost,
                score = score,
                affordable = affordable,
                category = upgrade.category,
                isBest = false
            });
        }

        if (!string.IsNullOrEmpty(bestId))
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (string.Equals(list[i].id, bestId, StringComparison.OrdinalIgnoreCase))
                {
                    var entry = list[i];
                    entry.isBest = true;
                    list[i] = entry;
                    break;
                }
            }
        }

        list.Sort((a, b) =>
        {
            var categoryCompare = string.Compare(a.category, b.category, StringComparison.OrdinalIgnoreCase);
            if (categoryCompare != 0)
            {
                return categoryCompare;
            }
            return a.cost.CompareTo(b.cost);
        });
        return list;
    }

    private string BuildUpgradeBadge(UpgradeData upgrade)
    {
        if (upgrade == null || string.IsNullOrEmpty(upgrade.category))
        {
            return "GENERAL";
        }

        switch (upgrade.category.ToLowerInvariant())
        {
            case "income":
                return "CASHFLOW";
            case "menu":
                return "SIGNATURE";
            case "staff":
                return "CREW";
            case "service":
                return "RUSH";
            case "sizzle":
                return "HEAT";
            default:
                return upgrade.category.ToUpperInvariant();
        }
    }

    private string BuildUpgradeImpactText(UpgradeData upgrade)
    {
        if (upgrade == null)
        {
            return "Sharper service rhythm.";
        }

        var percent = Mathf.RoundToInt(upgrade.effectValue * 100f);
        switch ((upgrade.category ?? string.Empty).ToLowerInvariant())
        {
            case "income":
                return "+" + percent + "% stronger baseline earnings every second.";
            case "menu":
                return "+" + percent + "% payout spike on that menu line.";
            case "staff":
                return "Staff move cleaner and keep the floor from clogging.";
            case "service":
                return "Tables turn faster so combos survive busy windows.";
            case "sizzle":
                return "Cook rhythm tightens so you flip and plate faster.";
            default:
                return "Tightens the restaurant loop and long-run output.";
        }
    }

    private void ApplyDebugSettingsFromSave()
    {
        if (!Application.isEditor)
        {
            saveData.debugPanelVisible = false;
            saveData.perfOverlayVisible = false;
        }

        if (customerSystem != null)
        {
            customerSystem.SetSpawnRateMultiplier(saveData.spawnRateMultiplier);
            customerSystem.SetServiceRateMultiplier(saveData.serviceRateMultiplier);
        }

        if (uiController != null)
        {
            uiController.SetDebugPanelVisible(saveData.debugPanelVisible);
            uiController.SetPerfOverlayVisible(saveData.perfOverlayVisible);
            uiController.SetDebugPresetIndex(saveData.debugPresetIndex);
            uiController.SetDebugSliderValues(saveData.spawnRateMultiplier, saveData.serviceRateMultiplier, saveData.debugPresetIndex == 3);
        }
    }

    public void ToggleDebugUI()
    {
        if (uiController == null)
        {
            return;
        }

        uiController.ToggleDebugUI();
    }

    public int GetDebugPresetIndex()
    {
        return uiController != null ? uiController.GetDebugPresetIndex() : saveData.debugPresetIndex;
    }

    public bool ServeNextCustomer()
    {
        if (customerSystem == null || upgradeSystem == null)
        {
            return false;
        }

        var next = customerSystem.PeekNext();
        if (next == null)
        {
            return false;
        }

        var requiredMenuId = !string.IsNullOrEmpty(next.menuId) ? next.menuId : GetDefaultUnlockedMenuId();
        var requiredServings = Mathf.Max(1, next.requestedServings);
        var requiresExactCut = next.requiresExactCut;
        var exactAvailable = GetCookedStockCount(requiredMenuId);
        var totalAvailable = GetTotalCookedStock();

        if ((requiresExactCut && exactAvailable < requiredServings) || (!requiresExactCut && totalAvailable < requiredServings))
        {
            var requiredName = ResolveMenuDisplayName(requiredMenuId);
            var exactHint = requiresExactCut ? " exact cut" : string.Empty;
            uiController?.ShowGrillStatus("Need " + requiredServings + exactHint + " plate(s) of " + requiredName + " before serving.");
            audioManager?.PlayButton();
            return false;
        }

        var exactServed = ConsumeCookedMeat(requiredMenuId, requiredServings);
        var remainingServings = Mathf.Max(0, requiredServings - exactServed);
        var fallbackServed = requiresExactCut ? 0 : ConsumeAnyCookedMeat(remainingServings);
        var cookedMatch = exactServed >= requiredServings;
        var fallbackUsed = fallbackServed > 0;

        var result = customerSystem.ForceServe(menuSystem, (float)upgradeSystem.GetCategoryMultiplier("service"));
        if (result.served)
        {
            if (fallbackUsed && !cookedMatch)
            {
                result.tipMultiplier = Mathf.Max(0.5f, result.tipMultiplier * 0.78f);
                result.quality = Mathf.Clamp01(result.quality - 0.22f);
            }
            var perfectServe = cookedMatch && !fallbackUsed && result.quality >= 0.92f && result.waitRatio <= 0.25f;
            var dailySpecialServed = cookedMatch && !fallbackUsed && IsDailySpecial(requiredMenuId);
            var streakBonus = UpdateDailySpecialStreak(dailySpecialServed, perfectServe);
            if (perfectServe || result.comboCount >= 5)
            {
                TriggerChefFever();
            }
            storyQuestSystem?.RecordServe(result.requestedServings, perfectServe, dailySpecialServed, result.isVip || result.isCritic);
            districtSideQuestSystem?.RecordServe(result.requestedServings, perfectServe, dailySpecialServed, result.isVip || result.isCritic);
            ProcessStoryQuestUpdates();
            ProcessSideQuestUpdates();
            ResolveStoryGuestServe(result);
            GrantServeTip(result, perfectServe, streakBonus);
            GrantGuestSpotlightBonus(result, perfectServe);
            var happy = customerSystem.Satisfaction >= 0.6f;
            audioManager?.PlayCustomerReaction(happy);
            audioManager?.PlayCombo(result.comboCount);
            HapticUtil.Light();
            tutorialSystem?.OnServe();
            uiController?.UpdateSatisfaction(customerSystem.Satisfaction);
            if (perfectServe)
            {
                uiController?.PlayCameraPunch(0.14f, 0.24f);
                uiController?.ShowMomentSpotlight(
                    "PERFECT FIRE",
                    "Exact timing. Ride the combo and keep the grills locked in.",
                    new Color(1f, 0.88f, 0.46f, 1f));
            }
            else if (result.isCritic)
            {
                uiController?.PlayCameraPunch(0.18f, 0.34f);
                uiController?.ShowMomentSpotlight(
                    "CRITIC TABLE CLEARED",
                    "High-stakes review served clean. Tonight's reputation just jumped.",
                    new Color(1f, 0.78f, 0.52f, 1f));
            }
            else if (result.isVip || result.isPartyTable)
            {
                uiController?.PlayCameraPunch(0.12f, 0.28f);
                uiController?.ShowMomentSpotlight(
                    result.isVip ? "VIP IMPRESSED" : "GROUP ORDER LANDED",
                    result.isVip
                        ? "Premium guests are happy. Keep the premium flow hot."
                        : "Big table secured. Stack the next rush before the room cools.",
                    result.isVip ? new Color(1f, 0.86f, 0.50f, 1f) : new Color(0.98f, 0.80f, 0.44f, 1f));
            }
            if (perfectServe)
            {
                uiController?.ShowGrillStatus("Perfect serve! Rush the next table while the combo window is hot.");
            }
            else if (dailySpecialServeStreak >= 3)
            {
                uiController?.ShowGrillStatus("Hot streak x" + dailySpecialServeStreak + "! The house special is catching fire.");
            }
            else if (result.isCritic)
            {
                uiController?.ShowGrillStatus("Critic served. A strong exact-cut plate can swing the whole night.");
            }
            else if (result.isVip)
            {
                uiController?.ShowGrillStatus("VIP table served. Keep the premium flow alive.");
            }
            else if (result.isPartyTable)
            {
                uiController?.ShowGrillStatus("Party table cleared. Big groups love a steady grill rhythm.");
            }
            else if (IsChefFeverActive())
            {
                uiController?.ShowGrillStatus("Chef Fever! Every clean plate is paying out big.");
            }
            else
            {
                uiController?.ShowGrillStatus(cookedMatch ? "Served fresh grilled meat." : "Served with substitute cut.");
            }
            uiController?.PlayCustomerEating(result.customerName, result.menuName, happy);
            RefreshSecondaryUI();
            Save();
        }
        return result.served;
    }

    public void TriggerRushService()
    {
        if (customerSystem == null)
        {
            return;
        }

        customerSystem.ApplyRush(rushServiceMultiplier, rushServiceDuration);
        audioManager?.PlayBoost();
    }

    public void SetQueueSpawnMultiplier(float value)
    {
        customerSystem?.SetSpawnRateMultiplier(value);
    }

    public void SetQueueServiceMultiplier(float value)
    {
        customerSystem?.SetServiceRateMultiplier(value);
    }

    public void SkipTutorial()
    {
        tutorialSystem?.Skip();
    }

    private void GrantServeTip(ServeResult result, bool perfectServe, double streakBonus)
    {
        if (economy == null || !result.served)
        {
            return;
        }

        var menuMultiplier = 1.0;
        if (upgradeSystem != null && !string.IsNullOrEmpty(result.menuId))
        {
            menuMultiplier = upgradeSystem.GetMenuMultiplier(result.menuId);
        }

        var orderWeight = Mathf.Max(1, result.requestedServings);
        var exactBonus = result.requiresExactCut ? 1.18f : 1f;
        var basePrice = result.basePrice > 0 ? result.basePrice * menuMultiplier * orderWeight * exactBonus : economy.IncomePerSec * 0.5;
        var qualityBonus = Mathf.Lerp(0.6f, 1.25f, result.quality);
        var comboBonus = 1f + Mathf.Clamp(result.comboCount, 0, 8) * 0.05f;
        var perfectBonus = perfectServe ? 1.22f : 1f;
        var feverBonus = IsChefFeverActive() ? chefFeverServeBonus : 1f;
        var tip = basePrice * 0.35f * qualityBonus * result.tipMultiplier * comboBonus * perfectBonus * feverBonus * streakBonus;

        if (tip > 0.01f)
        {
            economy.AddCurrency(tip);
            if (FloatingTextSystem.I != null)
            {
                var label = perfectServe ? "PERFECT +" : "TIP +";
                var color = perfectServe
                    ? new Color(1f, 0.92f, 0.54f, 1f)
                    : new Color(0.96f, 0.80f, 0.40f, 1f);
                FloatingTextSystem.I.Spawn(
                    (IsChefFeverActive() ? "FEVER " : string.Empty) + label + FormatUtil.FormatCurrency(tip),
                    new Vector2(Screen.width * 0.5f, Screen.height * 0.46f),
                    color,
                    perfectServe ? 1.25f : 1f);
            }
        }
    }

    private double UpdateDailySpecialStreak(bool dailySpecialServed, bool perfectServe)
    {
        if (!dailySpecialServed)
        {
            dailySpecialServeStreak = 0;
            return 1d;
        }

        dailySpecialServeStreak += 1;
        var streakMultiplier = 1d + System.Math.Min(0.32d, System.Math.Max(0, dailySpecialServeStreak - 1) * 0.06d);
        if (perfectServe && dailySpecialServeStreak >= 2 && FloatingTextSystem.I != null)
        {
            FloatingTextSystem.I.Spawn(
                "HOT STREAK x" + dailySpecialServeStreak,
                new Vector2(Screen.width * 0.5f, Screen.height * 0.60f),
                new Color(1f, 0.86f, 0.42f, 1f),
                1.18f);
        }
        return streakMultiplier;
    }

    private void GrantGuestSpotlightBonus(ServeResult result, bool perfectServe)
    {
        if (economy == null || !result.served)
        {
            return;
        }

        var specialGuestMultiplier = 1d;
        var spotlightLabel = string.Empty;
        if (result.isCritic)
        {
            specialGuestMultiplier = perfectServe ? 1.85d : 1.45d;
            spotlightLabel = "CRITIC BONUS";
        }
        else if (result.isVip)
        {
            specialGuestMultiplier = perfectServe ? 1.65d : 1.25d;
            spotlightLabel = "VIP BONUS";
        }
        else if (result.isPartyTable)
        {
            specialGuestMultiplier = 1.18d;
            spotlightLabel = "TABLE BONUS";
        }

        if (specialGuestMultiplier <= 1d || string.IsNullOrEmpty(spotlightLabel))
        {
            return;
        }

        var orderWeight = Mathf.Max(1, result.requestedServings);
        var bonus = result.basePrice * orderWeight * 0.25d * (specialGuestMultiplier - 1d);
        if (bonus <= 0.01d)
        {
            return;
        }

        economy.AddCurrency(bonus);
        audioManager?.PlayCoin();
        if (FloatingTextSystem.I != null)
        {
            var color = result.isCritic
                ? new Color(1f, 0.78f, 0.52f, 1f)
                : result.isVip
                    ? new Color(1f, 0.88f, 0.56f, 1f)
                    : new Color(0.94f, 0.86f, 0.52f, 1f);
            FloatingTextSystem.I.Spawn(
                spotlightLabel + " +" + FormatUtil.FormatCurrency(bonus),
                new Vector2(Screen.width * 0.5f, Screen.height * 0.54f),
                color,
                result.isCritic ? 1.3f : 1.12f);
        }
    }

    public void GrantCurrency(double amount, bool fromAd)
    {
        GrantCurrency(amount, fromAd ? RewardSource.Ad : RewardSource.Default);
    }

    public void GrantCurrency(double amount, RewardSource source)
    {
        if (amount <= 0 || economy == null)
        {
            return;
        }

        economy.AddCurrency(amount);
        switch (source)
        {
            case RewardSource.Ad:
                audioManager?.PlayAdReward();
                break;
            case RewardSource.Purchase:
                audioManager?.PlayPurchase();
                break;
            default:
                audioManager?.PlayCoin();
                break;
        }
        HapticUtil.Light();
        RefreshUI();
    }

    public void ApplyAdBoost(float multiplier, float duration)
    {
        if (economy == null)
        {
            return;
        }

        economy.ApplyBoost(multiplier, duration);
        audioManager?.PlayAdReward();
        HapticUtil.Light();
    }

    public bool BuyBestUpgrade()
    {
        if (upgradesData == null || upgradeSystem == null || economy == null)
        {
            return false;
        }

        string bestId = null;
        double bestScore = 0;
        double bestCost = 0;

        foreach (var upgrade in upgradesData)
        {
            if (upgrade == null || string.IsNullOrEmpty(upgrade.id))
            {
                continue;
            }

            var cost = upgradeSystem.GetUpgradeCost(upgrade.id);
            if (cost <= 0 || cost > economy.Currency)
            {
                continue;
            }

            var weight = GetUpgradeWeight(upgrade.category);
            var score = (upgrade.effectValue * weight) / cost;

            if (bestId == null || score > bestScore || (Math.Abs(score - bestScore) < 0.000001 && cost < bestCost))
            {
                bestId = upgrade.id;
                bestScore = score;
                bestCost = cost;
            }
        }

        if (string.IsNullOrEmpty(bestId))
        {
            return false;
        }

        return PurchaseUpgrade(bestId);
    }

    public bool BuyRawMeat(string menuId, int amount)
    {
        if (economy == null || amount <= 0)
        {
            return false;
        }

        var item = FindMenuItem(menuId);
        if (item == null)
        {
            return false;
        }

        var totalCost = GetRawMeatBuyCost(item) * amount;
        if (!economy.Spend(totalCost))
        {
            uiController?.ShowGrillStatus("Not enough cash to buy " + item.displayName + ".");
            return false;
        }

        var stock = GetMeatStock(menuId);
        stock.raw += amount;
        SetMeatStock(menuId, stock);
        audioManager?.PlayPurchase();
        HapticUtil.Medium();
        tutorialSystem?.OnBuyMeat();
        uiController?.ShowGrillStatus(item.displayName + " purchased +" + amount + ".");
        RefreshSecondaryUI();
        Save();
        return true;
    }

    public bool PlaceRawMeatOnGrill(int slotIndex, string menuId)
    {
        if (!IsValidGrillSlot(slotIndex))
        {
            return false;
        }

        if (!string.IsNullOrEmpty(grillSlots[slotIndex].menuId))
        {
            uiController?.ShowGrillStatus("Slot " + (slotIndex + 1) + " is already occupied.");
            return false;
        }

        var item = FindMenuItem(menuId);
        if (item == null)
        {
            return false;
        }

        var stock = GetMeatStock(menuId);
        if (stock.raw <= 0)
        {
            uiController?.ShowGrillStatus("No raw " + item.displayName + " left.");
            return false;
        }

        stock.raw -= 1;
        SetMeatStock(menuId, stock);

        var slot = grillSlots[slotIndex];
        slot.menuId = menuId;
        slot.cookTime = 0f;
        slot.flipped = false;
        grillSlots[slotIndex] = slot;

        audioManager?.PlayGrillLoad();
        tutorialSystem?.OnLoadMeat();
        uiController?.ShowGrillStatus("Loaded " + item.displayName + " on grill " + (slotIndex + 1) + ".");
        RefreshSecondaryUI();
        Save();
        return true;
    }

    public bool FlipMeat(int slotIndex)
    {
        if (!IsValidGrillSlot(slotIndex))
        {
            return false;
        }

        var slot = grillSlots[slotIndex];
        if (string.IsNullOrEmpty(slot.menuId))
        {
            return false;
        }

        if (slot.flipped)
        {
            uiController?.ShowGrillStatus("Meat on slot " + (slotIndex + 1) + " is already flipped.");
            return false;
        }

        if (slot.cookTime < GetFlipReadySeconds())
        {
            uiController?.ShowGrillStatus("Wait a bit more before flipping.");
            return false;
        }

        slot.flipped = true;
        grillSlots[slotIndex] = slot;
        audioManager?.PlayGrillFlip();
        tutorialSystem?.OnFlip();
        uiController?.ShowGrillStatus("Flip complete on slot " + (slotIndex + 1) + ".");
        RefreshSecondaryUI();
        Save();
        return true;
    }

    public bool CollectFromGrill(int slotIndex)
    {
        if (!IsValidGrillSlot(slotIndex))
        {
            return false;
        }

        var slot = grillSlots[slotIndex];
        if (string.IsNullOrEmpty(slot.menuId))
        {
            return false;
        }

        var item = FindMenuItem(slot.menuId);
        if (item == null)
        {
            ClearGrillSlot(slotIndex);
            RefreshSecondaryUI();
            Save();
            return false;
        }

        if (!IsSlotBurned(slot) && !IsSlotReady(slot))
        {
            uiController?.ShowGrillStatus("Still cooking. Flip and wait.");
            return false;
        }

        if (IsSlotBurned(slot))
        {
            ClearGrillSlot(slotIndex);
            audioManager?.PlayGrillBurn();
            uiController?.ShowGrillStatus(item.displayName + " burned. Discarded.");
            RefreshSecondaryUI();
            Save();
            return true;
        }

        var stock = GetMeatStock(slot.menuId);
        stock.cooked += 1;
        SetMeatStock(slot.menuId, stock);

        var specialMenu = IsDailySpecial(item.id);
        var specialBonus = specialMenu ? 1.22f : 1f;
        var saleReward = item.basePrice * item.bonusMultiplier * Math.Max(0.2f, grilledMeatSaleFactor) * specialBonus;
        if (saleReward > 0)
        {
            economy.AddCurrency(saleReward);
            if (FloatingTextSystem.I != null && uiController != null)
            {
                // Spawn floating text near center
                var pos = new Vector2(Screen.width * 0.5f, Screen.height * 0.4f);
                var rewardColor = specialMenu
                    ? new Color(1f, 0.92f, 0.56f, 1f)
                    : new Color(0.98f, 0.82f, 0.42f, 1f);
                var rewardLabel = specialMenu
                    ? "SPECIAL +" + FormatUtil.FormatCurrency(saleReward)
                    : "+$" + FormatUtil.FormatCurrency(saleReward);
                FloatingTextSystem.I.Spawn(rewardLabel, pos, rewardColor, specialMenu ? 1.15f : 1f);
            }
        }

        ClearGrillSlot(slotIndex);
        audioManager?.PlayGrillCollect();
        audioManager?.PlayCoin();
        HapticUtil.Heavy();
        tutorialSystem?.OnCollect();
        uiController?.ShowGrillStatus(
            specialMenu
                ? item.displayName + " is today's hot menu! +" + FormatUtil.FormatCurrency(saleReward)
                : item.displayName + " plated. +" + FormatUtil.FormatCurrency(saleReward));
        RefreshSecondaryUI();
        Save();
        return true;
    }

    private double GetUpgradeWeight(string category)
    {
        if (string.IsNullOrEmpty(category))
        {
            return 1.0;
        }

        switch (category.ToLowerInvariant())
        {
            case "income":
                return 1.0;
            case "menu":
                return 0.9;
            case "staff":
                return 0.8;
            case "service":
                return 0.8;
            case "sizzle":
                return 0.6;
            default:
                return 0.75;
        }
    }

    private void InitializeKitchenFromSave()
    {
        meatInventory.Clear();
        if (saveData != null && saveData.meatInventory != null)
        {
            for (int i = 0; i < saveData.meatInventory.Count; i++)
            {
                var entry = saveData.meatInventory[i];
                if (string.IsNullOrEmpty(entry.menuId))
                {
                    continue;
                }

                var state = new MeatInventoryState
                {
                    raw = Mathf.Max(0, entry.rawCount),
                    cooked = Mathf.Max(0, entry.cookedCount)
                };
                meatInventory[entry.menuId] = state;
            }
        }

        EnsureKitchenStockForUnlockedMenus();

        grillSlotCount = 4;
        grillSlots = new GrillSlotStateRuntime[grillSlotCount];
        if (saveData != null && saveData.grillSlots != null)
        {
            for (int i = 0; i < saveData.grillSlots.Count; i++)
            {
                var slot = saveData.grillSlots[i];
                if (slot.slotIndex < 0 || slot.slotIndex >= grillSlots.Length)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(slot.menuId))
                {
                    continue;
                }

                grillSlots[slot.slotIndex].menuId = slot.menuId;
                grillSlots[slot.slotIndex].cookTime = Mathf.Max(0f, slot.cookTime);
                grillSlots[slot.slotIndex].flipped = slot.flipped;
            }
        }

        EnsureEmergencyStock();
    }

    private void TickKitchen(float deltaTime)
    {
        if (grillSlots == null || grillSlots.Length == 0 || deltaTime <= 0f)
        {
            return;
        }

        var speedMultiplier = upgradeSystem != null ? Mathf.Clamp((float)upgradeSystem.GetCategoryMultiplier("sizzle"), 0.8f, 3.5f) : 1f;
        var step = deltaTime * speedMultiplier;

        for (int i = 0; i < grillSlots.Length; i++)
        {
            var slot = grillSlots[i];
            if (string.IsNullOrEmpty(slot.menuId))
            {
                continue;
            }

            slot.cookTime += step;
            grillSlots[i] = slot;
        }

        var occupiedCount = 0;
        for (int i = 0; i < grillSlots.Length; i++)
        {
            if (!string.IsNullOrEmpty(grillSlots[i].menuId))
            {
                occupiedCount++;
            }
        }

        var sizzleIntensity = grillSlots.Length > 0 ? occupiedCount / (float)grillSlots.Length : 0f;
        audioManager?.SetSizzleIntensity(sizzleIntensity);
    }

    private List<MeatInventoryUiEntry> BuildMeatInventoryUiEntries()
    {
        var entries = new List<MeatInventoryUiEntry>();
        if (menuSystem == null)
        {
            return entries;
        }

        var unlocked = menuSystem.GetUnlockedItems();
        unlocked.Sort((a, b) =>
        {
            if (a == null && b == null) return 0;
            if (a == null) return 1;
            if (b == null) return -1;
            var lv = a.unlockLevel.CompareTo(b.unlockLevel);
            if (lv != 0) return lv;
            return string.Compare(a.displayName, b.displayName, StringComparison.OrdinalIgnoreCase);
        });

        for (int i = 0; i < unlocked.Count; i++)
        {
            var item = unlocked[i];
            if (item == null || string.IsNullOrEmpty(item.id))
            {
                continue;
            }

            var stock = GetMeatStock(item.id);
            entries.Add(new MeatInventoryUiEntry
            {
                menuId = item.id,
                displayName = item.displayName,
                rawCount = stock.raw,
                cookedCount = stock.cooked,
                buyCost = GetRawMeatBuyCost(item),
                isFeatured = IsDailySpecial(item.id)
            });
        }

        return entries;
    }

    private IReadOnlyList<GrillSlotUiState> BuildGrillSlotUiStates()
    {
        var list = new List<GrillSlotUiState>();
        if (grillSlots == null || grillSlots.Length == 0)
        {
            return list;
        }

        for (int i = 0; i < grillSlots.Length; i++)
        {
            var slot = grillSlots[i];
            var occupied = !string.IsNullOrEmpty(slot.menuId);
            var state = new GrillSlotUiState
            {
                slotIndex = i,
                occupied = occupied,
                menuId = slot.menuId,
                displayName = ResolveMenuDisplayName(slot.menuId),
                cookProgress01 = occupied && GetCookSeconds() > 0f ? Mathf.Clamp01(slot.cookTime / GetCookSeconds()) : 0f,
                secondsToReady = occupied ? Mathf.Max(0f, GetCookSeconds() - slot.cookTime) : 0f,
                canFlip = occupied && !slot.flipped && slot.cookTime >= GetFlipReadySeconds() && slot.cookTime < GetBurnSeconds(),
                perfectWindow = occupied && !slot.flipped && slot.cookTime >= GetFlipReadySeconds() && slot.cookTime <= (GetFlipReadySeconds() + GetPerfectWindowSeconds()),
                flipped = slot.flipped,
                readyToCollect = occupied && IsSlotReady(slot),
                burned = occupied && IsSlotBurned(slot)
            };
            list.Add(state);
        }

        return list;
    }

    private bool IsValidGrillSlot(int slotIndex)
    {
        return grillSlots != null && slotIndex >= 0 && slotIndex < grillSlots.Length;
    }

    private bool IsSlotReady(GrillSlotStateRuntime slot)
    {
        return slot.flipped && slot.cookTime >= GetCookSeconds() && slot.cookTime < GetBurnSeconds();
    }

    private bool IsSlotBurned(GrillSlotStateRuntime slot)
    {
        return slot.cookTime >= GetBurnSeconds();
    }

    private void ClearGrillSlot(int slotIndex)
    {
        if (!IsValidGrillSlot(slotIndex))
        {
            return;
        }

        grillSlots[slotIndex].menuId = null;
        grillSlots[slotIndex].cookTime = 0f;
        grillSlots[slotIndex].flipped = false;
    }

    private MeatInventoryState GetMeatStock(string menuId)
    {
        if (string.IsNullOrEmpty(menuId))
        {
            return default;
        }

        MeatInventoryState stock;
        if (meatInventory.TryGetValue(menuId, out stock))
        {
            return stock;
        }

        return default;
    }

    private void SetMeatStock(string menuId, MeatInventoryState stock)
    {
        if (string.IsNullOrEmpty(menuId))
        {
            return;
        }

        stock.raw = Mathf.Max(0, stock.raw);
        stock.cooked = Mathf.Max(0, stock.cooked);
        meatInventory[menuId] = stock;
    }

    private int GetCookedStockCount(string menuId)
    {
        if (string.IsNullOrEmpty(menuId))
        {
            return 0;
        }

        return Mathf.Max(0, GetMeatStock(menuId).cooked);
    }

    private int ConsumeCookedMeat(string menuId, int amount)
    {
        if (string.IsNullOrEmpty(menuId) || amount <= 0)
        {
            return 0;
        }

        var stock = GetMeatStock(menuId);
        if (stock.cooked <= 0)
        {
            return 0;
        }

        var consumed = Mathf.Min(stock.cooked, amount);
        stock.cooked -= consumed;
        SetMeatStock(menuId, stock);
        return consumed;
    }

    private int ConsumeAnyCookedMeat(int amount)
    {
        if (amount <= 0)
        {
            return 0;
        }

        var remaining = amount;
        var consumed = 0;
        foreach (var pair in meatInventory)
        {
            if (remaining <= 0)
            {
                break;
            }

            if (pair.Value.cooked <= 0)
            {
                continue;
            }

            var stock = pair.Value;
            var take = Mathf.Min(stock.cooked, remaining);
            stock.cooked -= take;
            consumed += take;
            remaining -= take;
            SetMeatStock(pair.Key, stock);
        }

        return consumed;
    }

    private List<MeatInventoryEntry> ExportMeatInventory()
    {
        var list = new List<MeatInventoryEntry>();
        foreach (var pair in meatInventory)
        {
            if (string.IsNullOrEmpty(pair.Key))
            {
                continue;
            }

            var stock = pair.Value;
            if (stock.raw <= 0 && stock.cooked <= 0)
            {
                continue;
            }

            list.Add(new MeatInventoryEntry
            {
                menuId = pair.Key,
                rawCount = stock.raw,
                cookedCount = stock.cooked
            });
        }

        return list;
    }

    private List<GrillSlotSaveState> ExportGrillSlots()
    {
        var list = new List<GrillSlotSaveState>();
        if (grillSlots == null)
        {
            return list;
        }

        for (int i = 0; i < grillSlots.Length; i++)
        {
            var slot = grillSlots[i];
            if (string.IsNullOrEmpty(slot.menuId))
            {
                continue;
            }

            list.Add(new GrillSlotSaveState
            {
                slotIndex = i,
                menuId = slot.menuId,
                cookTime = slot.cookTime,
                flipped = slot.flipped
            });
        }

        return list;
    }

    private void EnsureKitchenStockForUnlockedMenus()
    {
        if (menuSystem == null)
        {
            return;
        }

        var unlocked = menuSystem.GetUnlockedItems();
        for (int i = 0; i < unlocked.Count; i++)
        {
            var item = unlocked[i];
            if (item == null || string.IsNullOrEmpty(item.id))
            {
                continue;
            }

            if (!meatInventory.ContainsKey(item.id))
            {
                meatInventory[item.id] = new MeatInventoryState
                {
                    raw = Mathf.Max(0, starterRawStockPerUnlockedMenu),
                    cooked = 0
                };
            }
        }
    }

    private void EnsureEmergencyStock()
    {
        var total = 0;
        foreach (var pair in meatInventory)
        {
            total += Mathf.Max(0, pair.Value.raw);
            total += Mathf.Max(0, pair.Value.cooked);
        }

        if (total > 0)
        {
            return;
        }

        var fallback = menuSystem != null ? menuSystem.GetRandomUnlockedItem() : null;
        if (fallback == null || string.IsNullOrEmpty(fallback.id))
        {
            return;
        }

        var stock = GetMeatStock(fallback.id);
        stock.raw = Mathf.Max(stock.raw, 2);
        SetMeatStock(fallback.id, stock);
    }

    private int GetTotalRawStock()
    {
        var total = 0;
        foreach (var pair in meatInventory)
        {
            total += Mathf.Max(0, pair.Value.raw);
        }
        return total;
    }

    private int GetTotalCookedStock()
    {
        var total = 0;
        foreach (var pair in meatInventory)
        {
            total += Mathf.Max(0, pair.Value.cooked);
        }
        return total;
    }

    private bool HasReadyGrillSlot()
    {
        if (grillSlots == null)
        {
            return false;
        }

        for (int i = 0; i < grillSlots.Length; i++)
        {
            var slot = grillSlots[i];
            if (string.IsNullOrEmpty(slot.menuId))
            {
                continue;
            }

            if (!IsSlotBurned(slot) && IsSlotReady(slot))
            {
                return true;
            }
        }

        return false;
    }

    private bool HasBurnRiskSlot()
    {
        if (grillSlots == null)
        {
            return false;
        }

        var warningPoint = Mathf.Lerp(GetFlipReadySeconds(), GetBurnSeconds(), 0.82f);
        for (int i = 0; i < grillSlots.Length; i++)
        {
            var slot = grillSlots[i];
            if (string.IsNullOrEmpty(slot.menuId) || slot.cookTime < warningPoint)
            {
                continue;
            }
            if (!IsSlotBurned(slot))
            {
                return true;
            }
        }

        return false;
    }

    private float GetCookSeconds()
    {
        var levelBoost = Mathf.Clamp01((saveData.playerLevel - 1) / 12f);
        var feverBoost = IsChefFeverActive() ? 0.18f : 0f;
        var tierBoost = Mathf.Clamp01(GetUpgradeVisualTier() / 4f) * 0.08f;
        return Mathf.Max(4.4f, grillCookSeconds * (1f - levelBoost * 0.12f - feverBoost - tierBoost));
    }

    private float GetFlipReadySeconds()
    {
        return Mathf.Max(1.55f, GetCookSeconds() * 0.46f);
    }

    private float GetBurnSeconds()
    {
        var tierGrace = Mathf.Clamp01(GetUpgradeVisualTier() / 3f) * 0.45f;
        var feverGrace = IsChefFeverActive() ? 0.35f : 0f;
        var baseGap = Mathf.Max(2.6f, grillBurnSeconds - grillCookSeconds);
        return GetCookSeconds() + baseGap + tierGrace + feverGrace;
    }

    private float GetPerfectWindowSeconds()
    {
        var tierWindow = Mathf.Clamp01(GetUpgradeVisualTier() / 3f) * 0.35f;
        var feverWindow = IsChefFeverActive() ? 0.25f : 0f;
        return 1.15f + tierWindow + feverWindow;
    }

    private StoreTier GetNextStoreTier()
    {
        if (storeTiers == null || storeTierSystem == null)
        {
            return null;
        }

        var nextIndex = storeTierSystem.CurrentTierIndex + 1;
        if (nextIndex < 0 || nextIndex >= storeTiers.Count)
        {
            return null;
        }
        return storeTiers[nextIndex];
    }

    private void UpdateRestaurantMood()
    {
        if (audioManager == null || customerSystem == null)
        {
            return;
        }

        var queuePressure = Mathf.Clamp01(customerSystem.Queue.Count / 6f);
        var comboPressure = Mathf.Clamp01(customerSystem.ComboCount / 6f);
        var tierPressure = Mathf.Clamp01(GetUpgradeVisualTier() / 3f);
        if (IsChefFeverActive())
        {
            comboPressure = 1f;
        }
        audioManager.SetRestaurantMood(queuePressure, comboPressure, tierPressure, customerSystem.IsRushActive || IsChefFeverActive());
    }

    private bool IsDailySpecial(string menuId)
    {
        var specialId = GetDailySpecialMenuId();
        return !string.IsNullOrEmpty(menuId) &&
               !string.IsNullOrEmpty(specialId) &&
               string.Equals(menuId, specialId, StringComparison.OrdinalIgnoreCase);
    }

    private string GetDailySpecialMenuId()
    {
        if (menuSystem == null)
        {
            return null;
        }

        var unlocked = menuSystem.GetUnlockedItems();
        if (unlocked == null || unlocked.Count == 0)
        {
            return null;
        }

        unlocked.Sort((a, b) =>
        {
            if (a == null && b == null) return 0;
            if (a == null) return 1;
            if (b == null) return -1;
            return string.Compare(a.id, b.id, StringComparison.OrdinalIgnoreCase);
        });

        var dayStamp = TimeUtil.UtcDayStamp();
        var index = Mathf.Abs(dayStamp.GetHashCode()) % unlocked.Count;
        var item = unlocked[index];
        return item != null ? item.id : null;
    }

    private string GetDailySpecialMenuName()
    {
        var item = FindMenuItem(GetDailySpecialMenuId());
        return item != null && !string.IsNullOrEmpty(item.displayName) ? item.displayName : "house special";
    }

    private void TickShiftEvents(float dt)
    {
        if (customerSystem == null || dt <= 0f)
        {
            return;
        }

        if (tutorialSystem != null && tutorialSystem.IsActive)
        {
            customerSystem.SetLiveEventModifiers(1f, 1f, 1f, 0f);
            return;
        }

        if (activeShiftEvent != ShiftEventType.None)
        {
            shiftEventTimer -= dt;
            if (shiftEventTimer <= 0f)
            {
                EndShiftEvent();
            }
            return;
        }

        shiftEventCooldownTimer -= dt;
        if (shiftEventCooldownTimer > 0f || saveData.playerLevel < 2)
        {
            return;
        }

        StartShiftEvent(PickNextShiftEvent());
    }

    private void TickChefFever(float dt)
    {
        if (chefFeverTimer <= 0f)
        {
            chefFeverTimer = 0f;
            return;
        }

        chefFeverTimer -= dt;
        if (chefFeverTimer <= 0f)
        {
            chefFeverTimer = 0f;
            uiController?.ShowGrillStatus("Chef Fever cools off. Hold the line and build it again.");
            uiController?.UpdateSessionGoal(GetSessionGoalUiState());
            uiController?.UpdateShowcase(GetRestaurantShowcaseUiState());
        }
    }

    private void TriggerChefFever()
    {
        var wasInactive = chefFeverTimer <= 0f;
        chefFeverTimer = Mathf.Max(chefFeverTimer, chefFeverDuration);
        if (!wasInactive)
        {
            return;
        }

        storyQuestSystem?.RecordChefFever();
        districtSideQuestSystem?.RecordChefFever();
        ProcessStoryQuestUpdates();
        ProcessSideQuestUpdates();
        audioManager?.PlayLevelUp();
        uiController?.ShowGrillStatus("Chef Fever activated! Every clean plate hits harder for a short time.");
        uiController?.UpdateSessionGoal(GetSessionGoalUiState());
        uiController?.UpdateShowcase(GetRestaurantShowcaseUiState());
        uiController?.PlayCelebrationBurst(new Color(1f, 0.78f, 0.32f, 1f));
        uiController?.PlayCameraPunch(0.24f, 0.48f);
        uiController?.ShowMomentSpotlight(
            "CHEF FEVER",
            "The kitchen is peaking. Clean plates and combos are paying out huge right now.",
            new Color(1f, 0.78f, 0.32f, 1f));
        if (FloatingTextSystem.I != null)
        {
            FloatingTextSystem.I.Spawn(
                "CHEF FEVER",
                new Vector2(Screen.width * 0.5f, Screen.height * 0.72f),
                new Color(1f, 0.84f, 0.46f, 1f),
                1.45f);
        }
    }

    private bool IsChefFeverActive()
    {
        return chefFeverTimer > 0f;
    }

    private void StartShiftEvent(ShiftEventType shiftEvent)
    {
        activeShiftEvent = shiftEvent;
        shiftEventTimer = shiftEventDuration + UnityEngine.Random.Range(-4f, 5f);
        ApplyShiftEventModifiers();
        var goal = BuildShiftEventGoal();
        uiController?.ShowGrillStatus(goal.headline + "!");
        uiController?.UpdateSessionGoal(goal);
        audioManager?.PlayTierUp();
        uiController?.PlayCameraPunch(0.22f, 0.44f);
        uiController?.ShowMomentSpotlight(
            goal.headline,
            goal.detail,
            new Color(1f, 0.84f, 0.46f, 1f));
        if (FloatingTextSystem.I != null)
        {
            FloatingTextSystem.I.Spawn(
                goal.headline,
                new Vector2(Screen.width * 0.5f, Screen.height * 0.76f),
                new Color(1f, 0.84f, 0.46f, 1f),
                1.32f);
        }
    }

    private void EndShiftEvent()
    {
        activeShiftEvent = ShiftEventType.None;
        customerSystem.SetLiveEventModifiers(1f, 1f, 1f, 0f);
        ResetShiftEventCooldown();
        uiController?.ShowGrillStatus("The crowd settles. Prep the next wave.");
        uiController?.UpdateSessionGoal(GetSessionGoalUiState());
    }

    private void ResetShiftEventCooldown()
    {
        shiftEventCooldownTimer = UnityEngine.Random.Range(shiftEventMinDelay, shiftEventMaxDelay);
    }

    private ShiftEventType PickNextShiftEvent()
    {
        var queueCount = customerSystem != null ? customerSystem.Queue.Count : 0;
        if (queueCount >= 4)
        {
            return ShiftEventType.LunchRush;
        }

        var roll = UnityEngine.Random.value;
        if (saveData.playerLevel >= 5 && roll < 0.34f)
        {
            return ShiftEventType.CriticNight;
        }
        if (roll < 0.58f)
        {
            return ShiftEventType.LunchRush;
        }
        return ShiftEventType.HappyHour;
    }

    private void ApplyShiftEventModifiers()
    {
        switch (activeShiftEvent)
        {
            case ShiftEventType.LunchRush:
                customerSystem.SetLiveEventModifiers(1.45f, 0.86f, 1.12f, 0.04f);
                break;
            case ShiftEventType.HappyHour:
                customerSystem.SetLiveEventModifiers(1.18f, 1.08f, 1.34f, 0.06f);
                break;
            case ShiftEventType.CriticNight:
                customerSystem.SetLiveEventModifiers(0.96f, 0.82f, 1.56f, 0.16f);
                break;
            default:
                customerSystem.SetLiveEventModifiers(1f, 1f, 1f, 0f);
                break;
        }
    }

    private SessionGoalUiState BuildShiftEventGoal()
    {
        switch (activeShiftEvent)
        {
            case ShiftEventType.LunchRush:
                return new SessionGoalUiState
                {
                    headline = "LUNCH RUSH",
                    detail = "Spawn rate is surging. Keep two grill slots hot and serve before patience collapses.",
                    accentLabel = "event",
                    urgency01 = 0.92f,
                };
            case ShiftEventType.HappyHour:
                return new SessionGoalUiState
                {
                    headline = "HAPPY HOUR",
                    detail = "Tips are boosted. Chain fast serves now to cash in on the crowd mood.",
                    accentLabel = "bonus",
                    urgency01 = 0.78f,
                };
            case ShiftEventType.CriticNight:
                return new SessionGoalUiState
                {
                    headline = "CRITIC NIGHT",
                    detail = "Premium guests are appearing. Perfect timing and clean serves pay out much bigger.",
                    accentLabel = "vip",
                    urgency01 = 0.88f,
                };
            default:
                return default;
        }
    }

    private string GetShiftEventLabel()
    {
        switch (activeShiftEvent)
        {
            case ShiftEventType.LunchRush:
                return "LUNCH RUSH";
            case ShiftEventType.HappyHour:
                return "HAPPY HOUR";
            case ShiftEventType.CriticNight:
                return "CRITIC NIGHT";
            default:
                return "OPEN KITCHEN";
        }
    }

    private string ResolveMenuDisplayName(string menuId)
    {
        if (string.IsNullOrEmpty(menuId))
        {
            return "Unknown Cut";
        }

        var item = FindMenuItem(menuId);
        if (item != null && !string.IsNullOrEmpty(item.displayName))
        {
            return item.displayName;
        }

        return menuId;
    }

    private MenuItem FindMenuItem(string menuId)
    {
        if (string.IsNullOrEmpty(menuId) || menuItems == null)
        {
            return null;
        }

        for (int i = 0; i < menuItems.Count; i++)
        {
            var item = menuItems[i];
            if (item == null || string.IsNullOrEmpty(item.id))
            {
                continue;
            }

            if (string.Equals(item.id, menuId, StringComparison.OrdinalIgnoreCase))
            {
                return item;
            }
        }

        return null;
    }

    private string GetDefaultUnlockedMenuId()
    {
        var unlocked = menuSystem != null ? menuSystem.GetUnlockedItems() : null;
        if (unlocked == null || unlocked.Count == 0)
        {
            return null;
        }

        var item = unlocked[0];
        return item != null ? item.id : null;
    }

    private double GetRawMeatBuyCost(MenuItem item)
    {
        if (item == null)
        {
            return 1d;
        }

        var baseCost = Math.Max(1d, item.basePrice * Math.Max(0.2f, meatBuyCostFactor));
        var marketPressure = 1d + Math.Min(0.6d, saveData.playerLevel * 0.03d);
        return baseCost * marketPressure;
    }

    private void EnsureDefaultData()
    {
        if (dataCatalog != null)
        {
            if ((menuItems == null || menuItems.Count == 0) && dataCatalog.menuItems != null)
            {
                menuItems = new List<MenuItem>(dataCatalog.menuItems);
            }

            if ((upgradesData == null || upgradesData.Count == 0) && dataCatalog.upgrades != null)
            {
                upgradesData = new List<UpgradeData>(dataCatalog.upgrades);
            }

            if ((storeTiers == null || storeTiers.Count == 0) && dataCatalog.storeTiers != null)
            {
                storeTiers = new List<StoreTier>(dataCatalog.storeTiers);
            }

            if ((customerTypes == null || customerTypes.Count == 0) && dataCatalog.customerTypes != null)
            {
                customerTypes = new List<CustomerType>(dataCatalog.customerTypes);
            }

            if (apiConfig == null)
            {
                apiConfig = dataCatalog.apiConfig;
            }

            if (economyTuning == null)
            {
                economyTuning = dataCatalog.economyTuning;
            }

            if (monetizationConfig == null)
            {
                monetizationConfig = dataCatalog.monetizationConfig;
            }
        }

        if (menuItems == null || menuItems.Count == 0)
        {
            menuItems = DefaultDataFactory.CreateMenuItems();
        }

        if (upgradesData == null || upgradesData.Count == 0)
        {
            upgradesData = DefaultDataFactory.CreateUpgrades();
        }

        if (storeTiers == null || storeTiers.Count == 0)
        {
            storeTiers = DefaultDataFactory.CreateStoreTiers();
        }

        if (customerTypes == null || customerTypes.Count == 0)
        {
            customerTypes = DefaultDataFactory.CreateCustomerTypes();
        }

        if (apiConfig == null)
        {
            apiConfig = DefaultDataFactory.CreateApiConfig();
        }

        if (economyTuning == null)
        {
            economyTuning = DefaultDataFactory.CreateEconomyTuning();
        }

        if (monetizationConfig == null)
        {
            monetizationConfig = DefaultDataFactory.CreateMonetizationConfig();
        }
    }

    private void InitializeSystems()
    {
        upgradeSystem = new UpgradeSystem(upgradesData, saveData.upgradeLevels);
        storeTierSystem = new StoreTierSystem(storeTiers, saveData.storeTierIndex);
        menuSystem = new MenuSystem(menuItems, upgradeSystem, saveData.unlockedMenuIds, saveData.playerLevel);
        menuSystem.SetSpotlightMenu(GetDailySpecialMenuId());
        customerSystem = new CustomerSystem(customerTypes);
        customerSystem.SetAutoServeEnabled(false);
        prestigeSystem = new PrestigeSystem(saveData.prestigeLevel, saveData.prestigePoints);
        economy = new EconomySystem(menuSystem, upgradeSystem, storeTierSystem, customerSystem, prestigeSystem, saveData.currency, saveData.totalIncome);
        economy.OnIncomeGained += HandleIncomeGained;
        progressionSystem = new ProgressionSystem(economyTuning);
        offlineEarnings = new OfflineEarnings();
        dailyLoginSystem = new DailyLoginSystem(saveData, economy);
        dailyMissionSystem = new DailyMissionSystem(saveData, economy, dailyMissionsPerDay);
        dailyMissionSystem.OnMissionsUpdated += missions => uiController?.UpdateDailyMissions(missions);
        storyQuestSystem = new StoryQuestSystem(saveData);
        districtSideQuestSystem = new DistrictSideQuestSystem(saveData);
        storyGuestDirector = new StoryGuestDirector(saveData);
        stateMachine = new GameStateMachine();
        InitializeKitchenFromSave();
        uiController?.Bind(this);
        tutorialSystem = new TutorialSystem(this, uiController, saveData.tutorialCompleted);
        if (monetizationService != null)
        {
            monetizationService.Bind(this, monetizationConfig);
        }
        analyticsService?.BindNetwork(networkService);
        ApplyDebugSettingsFromSave();
        SyncStoryQuestMeta(false);
        TryQueuePendingStoryGuest();
        UpdateProgressionFromIncome();
    }

    private void HandleIncomeGained(double amount)
    {
        saveData.lifetimeIncome += amount;
        saveData.totalIncome = economy.TotalEarned;
        dailyMissionSystem?.RecordEarnings(amount);
        UpdateProgressionFromIncome();
    }

    private void UpdateProgressionFromIncome()
    {
        if (progressionSystem == null)
        {
            return;
        }

        var previousLevel = saveData.playerLevel;
        var previousTierIndex = storeTierSystem != null ? storeTierSystem.CurrentTierIndex : 0;
        var newLevel = progressionSystem.GetLevelForIncome(saveData.totalIncome);
        if (newLevel > saveData.playerLevel)
        {
            saveData.playerLevel = newLevel;
            menuSystem.UnlockByLevel(saveData.playerLevel);
            var tierAdvanced = storeTierSystem.TryAdvanceTier(saveData.playerLevel);
            menuSystem.SetSpotlightMenu(GetDailySpecialMenuId());
            EnsureKitchenStockForUnlockedMenus();
            audioManager?.PlayLevelUp();
            uiController?.ShowGrillStatus("Level " + saveData.playerLevel + " unlocked. New cuts are opening up.");
            uiController?.PlayCelebrationBurst(new Color(1f, 0.84f, 0.38f, 1f));
            uiController?.PlayCameraPunch(0.18f, 0.42f);
            uiController?.ShowMomentSpotlight(
                "LEVEL " + saveData.playerLevel,
                "More cuts, more pressure, more hype. Keep pushing the district upward.",
                new Color(1f, 0.84f, 0.38f, 1f));
            if (FloatingTextSystem.I != null)
            {
                FloatingTextSystem.I.Spawn(
                    "LEVEL " + saveData.playerLevel,
                    new Vector2(Screen.width * 0.5f, Screen.height * 0.68f),
                    new Color(1f, 0.83f, 0.38f, 1f),
                    1.4f);
            }
            if (tierAdvanced && storeTierSystem.CurrentTierIndex > previousTierIndex)
            {
                var tier = storeTierSystem.CurrentTier;
                audioManager?.PlayTierUp();
                uiController?.PlayCelebrationBurst(new Color(1f, 0.95f, 0.66f, 1f));
                uiController?.PlayCameraPunch(0.26f, 0.62f);
                if (tier != null)
                {
                    uiController?.ShowGrillStatus("District upgraded to " + tier.displayName + ".");
                    uiController?.ShowMomentSpotlight(
                        tier.displayName.ToUpperInvariant(),
                        "New district unlocked. The restaurant brand just leveled up visually and economically.",
                        new Color(1f, 0.95f, 0.66f, 1f));
                    if (FloatingTextSystem.I != null)
                    {
                        FloatingTextSystem.I.Spawn(
                            tier.displayName.ToUpperInvariant(),
                            new Vector2(Screen.width * 0.5f, Screen.height * 0.58f),
                            new Color(1f, 0.95f, 0.66f, 1f),
                            1.15f);
                    }
                }
            }
            SyncStoryQuestMeta(true);
            ProcessStoryQuestUpdates();
            ProcessSideQuestUpdates();
            RefreshUI();
        }
    }

    private void TryDailyLogin()
    {
        if (dailyLoginSystem == null)
        {
            return;
        }

        var reward = dailyLoginSystem.TryClaim();
        if (reward.granted)
        {
            var starterPack = GrantDailySpecialStarterPack(reward.streakDay);
            uiController?.ShowLoginReward(reward);
            uiController?.ShowGrillStatus(
                "Login Day " + reward.streakDay + ". Fresh crate dropped: +" + starterPack + " " + GetDailySpecialMenuName() + ".");
            uiController?.PlayCelebrationBurst(new Color(1f, 0.88f, 0.50f, 1f));
            uiController?.PlayCameraPunch(0.18f, 0.46f);
            uiController?.ShowMomentSpotlight(
                "DAY " + reward.streakDay + " DROP",
                "Festival stock is in. +" + starterPack + " " + GetDailySpecialMenuName() + " to kick off today's run.",
                new Color(1f, 0.88f, 0.50f, 1f));
            audioManager?.PlayLevelUp();
            if (FloatingTextSystem.I != null)
            {
                FloatingTextSystem.I.Spawn(
                    "DAY " + reward.streakDay + " DROP",
                    new Vector2(Screen.width * 0.5f, Screen.height * 0.64f),
                    new Color(1f, 0.90f, 0.54f, 1f),
                    1.18f);
            }
            if (saveData.tutorialCompleted &&
                activeShiftEvent == ShiftEventType.None &&
                saveData.playerLevel >= 2 &&
                reward.streakDay >= 4)
            {
                StartShiftEvent(ShiftEventType.HappyHour);
            }
            SyncStoryQuestMeta(true);
            ProcessStoryQuestUpdates();
            ProcessSideQuestUpdates();
            Save();
        }
    }

    private async System.Threading.Tasks.Task EnsureNetworkAuth()
    {
        if (networkService == null || !networkService.IsNetworkEnabled())
        {
            return;
        }

        try
        {
            await networkService.EnsureGuestAuth();
        }
        catch (Exception ex)
        {
            Debug.LogWarning("Network auth failed: " + ex.Message);
        }
    }

    public bool TryPrestige()
    {
        var reward = prestigeSystem.CalculateReward(saveData.totalIncome, saveData.playerLevel);
        if (!reward.canPrestige)
        {
            return false;
        }

        prestigeSystem.ApplyPrestige(reward);
        saveData.prestigeLevel = prestigeSystem.PrestigeLevel;
        saveData.prestigePoints = prestigeSystem.PrestigePoints;
        saveData.ResetProgressForPrestige();
        InitializeSystems();
        storyQuestSystem?.RecordPrestige();
        districtSideQuestSystem?.RecordPrestige();
        ProcessStoryQuestUpdates();
        ProcessSideQuestUpdates();
        GrantPrestigeLaunchBonus(reward.points);
        Save();
        RefreshUI();
        analyticsService?.LogPrestige(reward.points);
        return true;
    }

    public bool CanPrestige()
    {
        var reward = prestigeSystem.CalculateReward(saveData.totalIncome, saveData.playerLevel);
        return reward.canPrestige;
    }

    private void SyncStoryQuestMeta(bool emitUpdates)
    {
        if (storyQuestSystem == null)
        {
            return;
        }

        storyQuestSystem.SyncMetaState(
            storeTierSystem != null ? storeTierSystem.CurrentTierIndex : 0,
            saveData.playerLevel,
            CanPrestige(),
            prestigeSystem != null ? prestigeSystem.PrestigeLevel : saveData.prestigeLevel,
            economy != null ? economy.IncomePerSec : 1d,
            emitUpdates);

        districtSideQuestSystem?.SyncMetaState(
            storeTierSystem != null ? storeTierSystem.CurrentTierIndex : 0,
            CanPrestige(),
            prestigeSystem != null ? prestigeSystem.PrestigeLevel : saveData.prestigeLevel,
            economy != null ? economy.IncomePerSec : 1d,
            emitUpdates);

        storyGuestDirector?.SyncMetaState(
            storeTierSystem != null ? storeTierSystem.CurrentTierIndex : 0,
            CanPrestige());
    }

    private void ProcessStoryQuestUpdates()
    {
        if (storyQuestSystem == null || economy == null)
        {
            return;
        }

        StoryQuestUpdate update;
        while (storyQuestSystem.TryDequeueUpdate(out update))
        {
            if (update.rewardCurrency > 0d)
            {
                economy.AddCurrency(update.rewardCurrency);
                audioManager?.PlayCoin();
            }

            uiController?.ShowGrillStatus(update.detail);
            uiController?.ShowMomentSpotlight(
                string.IsNullOrEmpty(update.speakerName) ? update.title : update.speakerName + " · " + update.title,
                update.rewardCurrency > 0d
                    ? update.detail + " +" + FormatUtil.FormatCurrency(update.rewardCurrency)
                    : update.detail,
                Color.Lerp(new Color(0.96f, 0.74f, 0.34f, 1f), new Color(1f, 0.90f, 0.56f, 1f), update.accent01));

            if (FloatingTextSystem.I != null)
            {
                FloatingTextSystem.I.Spawn(
                    update.rewardCurrency > 0d ? "STORY +" + FormatUtil.FormatCurrency(update.rewardCurrency) : "NEW CHAPTER",
                    new Vector2(Screen.width * 0.5f, Screen.height * 0.70f),
                    new Color(1f, 0.88f, 0.54f, 1f),
                    1.1f);
            }
        }
    }

    private void ProcessSideQuestUpdates()
    {
        if (districtSideQuestSystem == null || economy == null)
        {
            return;
        }

        DistrictSideQuestUpdate update;
        while (districtSideQuestSystem.TryDequeueUpdate(out update))
        {
            if (update.rewardCurrency > 0d)
            {
                economy.AddCurrency(update.rewardCurrency);
                audioManager?.PlayCoin();
            }

            uiController?.ShowGrillStatus(update.detail);
            uiController?.ShowMomentSpotlight(
                string.IsNullOrEmpty(update.speakerName) ? update.title : update.speakerName + " · " + update.title,
                update.rewardCurrency > 0d
                    ? update.detail + " +" + FormatUtil.FormatCurrency(update.rewardCurrency)
                    : update.detail,
                Color.Lerp(new Color(0.84f, 0.62f, 0.28f, 1f), new Color(0.98f, 0.86f, 0.48f, 1f), update.accent01));
        }
    }

    private void TryQueuePendingStoryGuest()
    {
        if (storyGuestDirector == null || customerSystem == null || menuSystem == null)
        {
            return;
        }

        if (customerSystem.Queue.Count >= 4)
        {
            return;
        }

        if (!storyGuestDirector.TryDequeueEncounter(out var encounter))
        {
            return;
        }

        if (customerSystem.HasStoryGuest(encounter.id))
        {
            return;
        }

        var menuItem = FindMenuItem(GetDailySpecialMenuId()) ?? menuSystem.GetRandomUnlockedItem();
        if (menuItem == null || string.IsNullOrEmpty(menuItem.id))
        {
            return;
        }

        var entry = new CustomerQueueEntry
        {
            customerTypeId = "story",
            customerName = encounter.displayName,
            menuId = menuItem.id,
            menuName = menuItem.displayName,
            storyGuestId = encounter.id,
            storyGuestLabel = encounter.label,
            menuBasePrice = menuItem.basePrice * menuItem.bonusMultiplier * (encounter.isFinaleGuest ? 1.35d : 1.18d),
            patience = Mathf.Max(5f, encounter.patienceSeconds),
            waitTime = 0f,
            tipMultiplier = Mathf.Max(1f, encounter.tipMultiplier),
            isVip = encounter.isVip,
            isCritic = encounter.isCritic,
            isPartyTable = !encounter.isVip && !encounter.isCritic && encounter.requestedServings > 1,
            isStoryGuest = true,
            isFinaleGuest = encounter.isFinaleGuest,
            isBossGuest = encounter.isBossGuest,
            requestedServings = Mathf.Max(1, encounter.requestedServings),
            requiresExactCut = encounter.requiresExactCut
        };

        if (customerSystem.EnqueuePriorityGuest(entry))
        {
            uiController?.ShowGrillStatus(encounter.arrivalLine);
            uiController?.ShowMomentSpotlight(
                encounter.label,
                encounter.arrivalLine,
                encounter.isFinaleGuest ? new Color(1f, 0.88f, 0.48f, 1f) : new Color(0.96f, 0.78f, 0.46f, 1f));
        }
    }

    private void ResolveStoryGuestServe(ServeResult result)
    {
        if (!result.isStoryGuest || storyGuestDirector == null || economy == null)
        {
            return;
        }

        if (!storyGuestDirector.TryResolveEncounter(result.storyGuestId, out var encounter))
        {
            return;
        }

        var bonus = Math.Max(
            result.basePrice * Math.Max(1, result.requestedServings) * (encounter.isBossGuest ? 0.75d : 0.40d),
            encounter.isBossGuest ? 24d : 12d);
        economy.AddCurrency(bonus);
        audioManager?.PlayTierUp();
        uiController?.ShowGrillStatus(encounter.resolvedLine);
        uiController?.ShowMomentSpotlight(
            encounter.label,
            encounter.resolvedLine + " +" + FormatUtil.FormatCurrency(bonus),
            encounter.isFinaleGuest ? new Color(1f, 0.90f, 0.54f, 1f) : new Color(0.98f, 0.84f, 0.52f, 1f));
    }

    public bool ClaimDailyMission(string missionId)
    {
        if (dailyMissionSystem == null)
        {
            return false;
        }

        var success = dailyMissionSystem.Claim(missionId);
        if (success)
        {
            audioManager?.PlayCoin();
            uiController?.ShowGrillStatus("Mission payout secured. Keep the momentum rolling.");
            uiController?.PlayCelebrationBurst(new Color(1f, 0.88f, 0.42f, 1f));
            uiController?.PlayCameraPunch(0.16f, 0.38f);
            uiController?.ShowMomentSpotlight(
                "MISSION CLEAR",
                "Payout collected. Keep chaining goals while the room is hot.",
                new Color(1f, 0.88f, 0.42f, 1f));
            if (FloatingTextSystem.I != null)
            {
                FloatingTextSystem.I.Spawn(
                    "MISSION CLEAR",
                    new Vector2(Screen.width * 0.5f, Screen.height * 0.62f),
                    new Color(1f, 0.88f, 0.42f, 1f),
                    1.15f);
            }
            Save();
            RefreshUI();
        }
        return success;
    }

    private int GrantDailySpecialStarterPack(int streakDay)
    {
        var specialId = GetDailySpecialMenuId();
        if (string.IsNullOrEmpty(specialId))
        {
            return 0;
        }

        var rawBonus = Mathf.Clamp(1 + Mathf.FloorToInt(streakDay * 0.5f), 1, 5);
        var stock = GetMeatStock(specialId);
        stock.raw += rawBonus;
        if (streakDay >= 5)
        {
            stock.cooked += 1;
        }
        SetMeatStock(specialId, stock);
        dailySpecialServeStreak = Mathf.Max(dailySpecialServeStreak, Mathf.Min(2, Mathf.Max(0, streakDay - 2)));
        return rawBonus;
    }

    private void GrantPrestigeLaunchBonus(int spiceStars)
    {
        var launchCash = System.Math.Max(0d, spiceStars * 18d);
        if (launchCash > 0d)
        {
            economy.AddCurrency(launchCash);
        }

        var starterPack = GrantDailySpecialStarterPack(Mathf.Clamp(2 + spiceStars, 2, 7));
        var specialName = GetDailySpecialMenuName();
        if (spiceStars > 0)
        {
            economy.ApplyBoost(1f + Mathf.Clamp(spiceStars * 0.04f, 0.04f, 0.22f), 18f);
        }

        shiftEventCooldownTimer = Mathf.Min(shiftEventCooldownTimer <= 0f ? 8f : shiftEventCooldownTimer, 8f);
        audioManager?.PlayTierUp();
        audioManager?.PlayLevelUp();
        uiController?.ShowGrillStatus(
            "Prestige complete. +" + spiceStars + " spice stars, " +
            FormatUtil.FormatCurrency(launchCash) + " launch cash, and +" + starterPack + " " + specialName + " to open the next season.");
        uiController?.PlayCelebrationBurst(new Color(1f, 0.90f, 0.54f, 1f));
        uiController?.PlayCameraPunch(0.32f, 0.90f);
        uiController?.ShowMomentSpotlight(
            "NEW SEASON",
            "Fresh district run online. Launch cash, special stock, and hype are all primed.",
            new Color(1f, 0.90f, 0.54f, 1f),
            useThumb: true);
        if (FloatingTextSystem.I != null)
        {
            FloatingTextSystem.I.Spawn(
                "NEW SEASON +" + spiceStars,
                new Vector2(Screen.width * 0.5f, Screen.height * 0.66f),
                new Color(1f, 0.90f, 0.54f, 1f),
                1.42f);
        }
    }
}
