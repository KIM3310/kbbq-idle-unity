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

    [Header("Kitchen Gameplay")]
    [SerializeField] private int grillSlotCount = 2;
    [SerializeField] private float grillCookSeconds = 7f;
    [SerializeField] private float grillBurnSeconds = 12f;
    [SerializeField] private float grillFlipReadySeconds = 3f;
    [SerializeField] private int starterRawStockPerUnlockedMenu = 2;
    [SerializeField] private float meatBuyCostFactor = 0.95f;
    [SerializeField] private float grilledMeatSaleFactor = 1.15f;

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
    private SaveSystem saveSystem;
    private GameStateMachine stateMachine;
    private TutorialSystem tutorialSystem;
    private SaveData saveData;
    private float missionRefreshTimer = 0f;
    private float secondaryUiTimer = 0f;
    private readonly Dictionary<string, MeatInventoryState> meatInventory = new Dictionary<string, MeatInventoryState>();
    private GrillSlotStateRuntime[] grillSlots = Array.Empty<GrillSlotStateRuntime>();

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
    }

    private async void Start()
    {
        stateMachine.TransitionTo(GameState.Boot);
        stateMachine.TransitionTo(saveData.tutorialCompleted ? GameState.MainLoop : GameState.Tutorial);
        ApplyOfflineEarnings();
        TryDailyLogin();
        dailyMissionSystem?.EnsureMissionsForToday(economy.IncomePerSec);
        RefreshUI();
        tutorialSystem?.Start();
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
        uiController?.UpdateEconomy(economy.Currency, economy.IncomePerSec);
        uiController?.UpdateSatisfaction(customerSystem.Satisfaction);

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
        analyticsService?.LogBoost();
        audioManager?.PlayBoost();
        HapticUtil.Light();
        tutorialSystem?.OnBoost();
    }

    public bool PurchaseUpgrade(string upgradeId)
    {
        var success = upgradeSystem.PurchaseUpgrade(upgradeId, economy);
        if (success)
        {
            dailyMissionSystem?.RecordUpgrade();
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
    public IReadOnlyList<CustomerQueueEntry> GetQueueSnapshot() => customerSystem != null ? customerSystem.Queue : null;
    public List<UpgradeUiEntry> GetUpgradeUiEntries() => BuildUpgradeUiEntries();
    public QueueMetrics GetQueueMetrics() => customerSystem != null ? customerSystem.GetMetrics() : default;
    public float GetQueueSpawnMultiplier() => customerSystem != null ? customerSystem.SpawnRateMultiplier : 1f;
    public float GetQueueServiceMultiplier() => customerSystem != null ? customerSystem.ServiceRateMultiplier : 1f;
    public AudioManager GetAudioManager() => audioManager;
    public MonetizationService GetMonetizationService() => monetizationService;
    public NetworkService GetNetworkService() => networkService;
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
        if (saveData.lastOnlineTs <= 0)
        {
            saveData.lastOnlineTs = TimeUtil.UtcNowUnix();
            return;
        }

        var offline = offlineEarnings.Calculate(saveData.lastOnlineTs, economy.IncomePerSec, maxOfflineHours);
        if (offline > 0)
        {
            economy.AddCurrency(offline);
        }

        saveData.lastOnlineTs = TimeUtil.UtcNowUnix();
        RefreshUI();
    }

    private void RefreshUI()
    {
        uiController?.UpdateEconomy(economy.Currency, economy.IncomePerSec);
        uiController?.UpdateSatisfaction(customerSystem.Satisfaction);
        uiController?.UpdateStoreTier(storeTierSystem.CurrentTier);
        uiController?.UpdatePrestige(prestigeSystem.PrestigeLevel, prestigeSystem.PrestigePoints);
        uiController?.UpdateDailyMissions(saveData.dailyMissions);
        uiController?.RefreshGrill();
        RefreshSecondaryUI();
    }

    private void RefreshSecondaryUI()
    {
        uiController?.UpdateQueue(customerSystem.Queue);
        uiController?.UpdateQueueMetrics(customerSystem.GetMetrics());
        uiController?.UpdateUpgrades(BuildUpgradeUiEntries());
        uiController?.UpdateCombo(customerSystem.ComboCount, customerSystem.ComboTimeRemaining, customerSystem.ComboDuration, customerSystem.GetComboMultiplier());
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
        var cookedMatch = TryConsumeCookedMeat(requiredMenuId);
        var fallbackUsed = false;
        if (!cookedMatch)
        {
            fallbackUsed = TryConsumeAnyCookedMeat();
        }

        if (!cookedMatch && !fallbackUsed)
        {
            var requiredName = ResolveMenuDisplayName(requiredMenuId);
            uiController?.ShowGrillStatus("Cook " + requiredName + " before serving.");
            audioManager?.PlayButton();
            return false;
        }

        var result = customerSystem.ForceServe(menuSystem, (float)upgradeSystem.GetCategoryMultiplier("service"));
        if (result.served)
        {
            if (fallbackUsed && !cookedMatch)
            {
                result.tipMultiplier = Mathf.Max(0.5f, result.tipMultiplier * 0.78f);
                result.quality = Mathf.Clamp01(result.quality - 0.22f);
            }
            GrantServeTip(result);
            var happy = customerSystem.Satisfaction >= 0.6f;
            audioManager?.PlayCustomerReaction(happy);
            HapticUtil.Light();
            tutorialSystem?.OnServe();
            uiController?.UpdateSatisfaction(customerSystem.Satisfaction);
            uiController?.ShowGrillStatus(cookedMatch ? "Served fresh grilled meat." : "Served with substitute cut.");
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

    private void GrantServeTip(ServeResult result)
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

        var basePrice = result.basePrice > 0 ? result.basePrice * menuMultiplier : economy.IncomePerSec * 0.5;
        var qualityBonus = Mathf.Lerp(0.6f, 1.25f, result.quality);
        var comboBonus = 1f + Mathf.Clamp(result.comboCount, 0, 8) * 0.05f;
        var tip = basePrice * 0.35f * qualityBonus * result.tipMultiplier * comboBonus;

        if (tip > 0.01f)
        {
            economy.AddCurrency(tip);
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

        if (slot.cookTime < grillFlipReadySeconds)
        {
            uiController?.ShowGrillStatus("Wait a bit more before flipping.");
            return false;
        }

        slot.flipped = true;
        grillSlots[slotIndex] = slot;
        audioManager?.PlayGrillFlip();
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

        var saleReward = item.basePrice * item.bonusMultiplier * Math.Max(0.2f, grilledMeatSaleFactor);
        if (saleReward > 0)
        {
            economy.AddCurrency(saleReward);
        }

        ClearGrillSlot(slotIndex);
        audioManager?.PlayGrillCollect();
        uiController?.ShowGrillStatus(item.displayName + " plated. +" + FormatUtil.FormatCurrency(saleReward));
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

        grillSlotCount = Mathf.Clamp(grillSlotCount, 1, 4);
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
                buyCost = GetRawMeatBuyCost(item)
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
                cookProgress01 = occupied && grillCookSeconds > 0f ? Mathf.Clamp01(slot.cookTime / grillCookSeconds) : 0f,
                secondsToReady = occupied ? Mathf.Max(0f, grillCookSeconds - slot.cookTime) : 0f,
                canFlip = occupied && !slot.flipped && slot.cookTime >= grillFlipReadySeconds && slot.cookTime < grillBurnSeconds,
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
        return slot.flipped && slot.cookTime >= grillCookSeconds && slot.cookTime < grillBurnSeconds;
    }

    private bool IsSlotBurned(GrillSlotStateRuntime slot)
    {
        return slot.cookTime >= grillBurnSeconds;
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

    private bool TryConsumeCookedMeat(string menuId)
    {
        if (string.IsNullOrEmpty(menuId))
        {
            return false;
        }

        var stock = GetMeatStock(menuId);
        if (stock.cooked <= 0)
        {
            return false;
        }

        stock.cooked -= 1;
        SetMeatStock(menuId, stock);
        return true;
    }

    private bool TryConsumeAnyCookedMeat()
    {
        string candidateId = null;
        MeatInventoryState candidateStock = default;

        foreach (var pair in meatInventory)
        {
            if (pair.Value.cooked <= 0)
            {
                continue;
            }

            candidateId = pair.Key;
            candidateStock = pair.Value;
            break;
        }

        if (string.IsNullOrEmpty(candidateId))
        {
            return false;
        }

        candidateStock.cooked -= 1;
        SetMeatStock(candidateId, candidateStock);
        return true;
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

        var newLevel = progressionSystem.GetLevelForIncome(saveData.totalIncome);
        if (newLevel > saveData.playerLevel)
        {
            saveData.playerLevel = newLevel;
            menuSystem.UnlockByLevel(saveData.playerLevel);
            storeTierSystem.TryAdvanceTier(saveData.playerLevel);
            EnsureKitchenStockForUnlockedMenus();
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
            uiController?.ShowLoginReward(reward);
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
        saveData.ResetProgressForPrestige();
        InitializeSystems();
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

    public bool ClaimDailyMission(string missionId)
    {
        if (dailyMissionSystem == null)
        {
            return false;
        }

        var success = dailyMissionSystem.Claim(missionId);
        if (success)
        {
            Save();
            RefreshUI();
        }
        return success;
    }
}
