using System.Collections.Generic;
using UnityEngine;

public class CustomerQueueEntry
{
    public string customerTypeId;
    public string customerName;
    public string menuId;
    public string menuName;
    public string storyGuestId;
    public string storyGuestLabel;
    public string storyBadgeName;
    public string storyRuleText;
    public double menuBasePrice;
    public float patience;
    public float waitTime;
    public float tipMultiplier;
    public bool isVip;
    public bool isCritic;
    public bool isPartyTable;
    public bool isStoryGuest;
    public bool isFinaleGuest;
    public bool isBossGuest;
    public int bossPhase;
    public int bossPhaseCount;
    public int requestedServings;
    public bool requiresExactCut;
}

public struct ServeResult
{
    public bool served;
    public float quality;
    public float waitRatio;
    public float tipMultiplier;
    public string customerName;
    public string menuId;
    public string menuName;
    public string storyGuestId;
    public string storyGuestLabel;
    public string storyBadgeName;
    public string storyRuleText;
    public double basePrice;
    public int comboCount;
    public bool isVip;
    public bool isCritic;
    public bool isPartyTable;
    public bool isStoryGuest;
    public bool isFinaleGuest;
    public bool isBossGuest;
    public int bossPhase;
    public int bossPhaseCount;
    public int requestedServings;
    public bool requiresExactCut;
}

public class CustomerSystem
{
    private const float VipChance = 0.08f;
    private const float CriticChance = 0.06f;
    private const float RegularGroupChance = 0.10f;

    private readonly List<CustomerType> customers = new List<CustomerType>();
    private readonly List<CustomerQueueEntry> queue = new List<CustomerQueueEntry>();
    private readonly Queue<ServeSample> serveSamples = new Queue<ServeSample>();
    private float satisfaction = 0.75f;
    private float satisfactionDecay = 0.01f;
    private float spawnTimer;
    private float serviceTimer;
    private float baseSpawnInterval = 6f;
    private float baseServiceInterval = 4f;
    private int maxQueue = 6;
    private float rushTimer = 0f;
    private float rushMultiplier = 1f;
    private float spawnRateMultiplier = 1f;
    private float serviceRateMultiplier = 1f;
    private float eventSpawnMultiplier = 1f;
    private float eventPatienceMultiplier = 1f;
    private float eventTipMultiplier = 1f;
    private float eventVipChanceBonus = 0f;
    private float runtime = 0f;
    private float serveWaitSum = 0f;
    private int comboCount = 0;
    private float comboTimer = 0f;
    private float comboDuration = 6f;
    private int comboMax = 8;
    private float comboStepBonus = 0.05f;
    private bool autoServeEnabled = true;
    private int totalServed = 0;
    private int totalArrived = 0;

    public float Satisfaction => satisfaction;
    public IReadOnlyList<CustomerQueueEntry> Queue => queue;
    public float SpawnRateMultiplier => spawnRateMultiplier;
    public float ServiceRateMultiplier => serviceRateMultiplier;
    public int ComboCount => comboCount;
    public float ComboTimeRemaining => comboTimer;
    public float ComboDuration => comboDuration;
    public bool IsRushActive => rushTimer > 0f;

    public float GetComboMultiplier()
    {
        return 1f + comboCount * comboStepBonus;
    }

    public CustomerSystem(IEnumerable<CustomerType> customers)
    {
        if (customers != null)
        {
            foreach (var customer in customers)
            {
                if (customer == null || string.IsNullOrEmpty(customer.id))
                {
                    continue;
                }
                this.customers.Add(customer);
            }
        }

        spawnTimer = baseSpawnInterval;
        serviceTimer = baseServiceInterval;
    }

    public void RegisterService(float quality)
    {
        satisfaction = Mathf.Clamp01((satisfaction + Mathf.Clamp01(quality)) * 0.5f);
    }

    public void Tick(float dt, float serviceQualityMultiplier, MenuSystem menuSystem)
    {
        runtime += Mathf.Max(0f, dt);
        UpdateRush(dt);
        var serviceBoost = Mathf.Clamp01(serviceQualityMultiplier - 1f);
        var delta = (serviceBoost * 0.015f) - (satisfactionDecay * dt);
        satisfaction = Mathf.Clamp01(satisfaction + delta);
        UpdateComboTimer(dt);
        CullServeSamples();
        UpdateQueue(dt, serviceQualityMultiplier, menuSystem);
    }

    public float GetTipMultiplier()
    {
        var baseTip = Mathf.Lerp(0.9f, 1.25f, satisfaction);
        if (queue.Count == 0)
        {
            return baseTip;
        }

        var total = 0f;
        for (int i = 0; i < queue.Count; i++)
        {
            total += queue[i].tipMultiplier;
        }
        var avg = total / Mathf.Max(1, queue.Count);
        avg = Mathf.Clamp(avg, 0.9f, 1.15f);
        return baseTip * avg;
    }

    public void ApplyRush(float multiplier, float duration)
    {
        rushMultiplier = Mathf.Max(1f, multiplier);
        rushTimer = Mathf.Max(0f, duration);
    }

    public ServeResult ForceServe(MenuSystem menuSystem, float serviceQualityMultiplier)
    {
        var result = new ServeResult();
        if (queue.Count == 0)
        {
            return result;
        }

        var served = queue[0];
        queue.RemoveAt(0);
        var waitRatio = served.patience > 0f ? Mathf.Clamp01(served.waitTime / served.patience) : 0f;
        var quality = Mathf.Clamp01(0.8f + (serviceQualityMultiplier - 1f) * 0.2f - waitRatio * 0.4f);
        RegisterService(quality);
        RecordServe(served.waitTime);
        UpdateCombo(true, quality, waitRatio);

        result.served = true;
        result.quality = quality;
        result.waitRatio = waitRatio;
        result.tipMultiplier = served.tipMultiplier;
        result.customerName = served.customerName;
        result.menuId = served.menuId;
        result.menuName = served.menuName;
        result.storyGuestId = served.storyGuestId;
        result.storyGuestLabel = served.storyGuestLabel;
        result.storyBadgeName = served.storyBadgeName;
        result.storyRuleText = served.storyRuleText;
        result.basePrice = served.menuBasePrice;
        result.comboCount = comboCount;
        result.isVip = served.isVip;
        result.isCritic = served.isCritic;
        result.isPartyTable = served.isPartyTable;
        result.isStoryGuest = served.isStoryGuest;
        result.isFinaleGuest = served.isFinaleGuest;
        result.isBossGuest = served.isBossGuest;
        result.bossPhase = served.bossPhase;
        result.bossPhaseCount = served.bossPhaseCount;
        result.requestedServings = Mathf.Max(1, served.requestedServings);
        result.requiresExactCut = served.requiresExactCut;
        totalServed++;
        return result;
    }

    public void SetSpawnRateMultiplier(float value)
    {
        spawnRateMultiplier = Mathf.Clamp(value, 0.25f, 3f);
    }

    public void SetServiceRateMultiplier(float value)
    {
        serviceRateMultiplier = Mathf.Clamp(value, 0.25f, 3f);
    }

    public void SetLiveEventModifiers(float spawnMultiplier, float patienceMultiplier, float tipMultiplier, float vipChanceBonus)
    {
        eventSpawnMultiplier = Mathf.Clamp(spawnMultiplier, 0.5f, 2.5f);
        eventPatienceMultiplier = Mathf.Clamp(patienceMultiplier, 0.6f, 1.5f);
        eventTipMultiplier = Mathf.Clamp(tipMultiplier, 0.8f, 2.2f);
        eventVipChanceBonus = Mathf.Clamp01(vipChanceBonus);
    }

    public void SetAutoServeEnabled(bool enabled)
    {
        autoServeEnabled = enabled;
    }

    public CustomerQueueEntry PeekNext()
    {
        return queue.Count > 0 ? queue[0] : null;
    }

    public bool EnqueuePriorityGuest(CustomerQueueEntry entry)
    {
        if (entry == null || queue.Count >= maxQueue)
        {
            return false;
        }

        queue.Insert(0, entry);
        totalArrived++;
        return true;
    }

    public bool HasStoryGuest(string storyGuestId)
    {
        if (string.IsNullOrEmpty(storyGuestId))
        {
            return false;
        }

        for (int i = 0; i < queue.Count; i++)
        {
            var entry = queue[i];
            if (entry != null && string.Equals(entry.storyGuestId, storyGuestId, System.StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    public QueueMetrics GetMetrics()
    {
        var count = serveSamples.Count;
        return new QueueMetrics
        {
            queueCount = queue.Count,
            avgWaitSeconds = count > 0 ? serveWaitSum / count : 0f,
            servedPerMinute = count,
            totalServed = totalServed,
            totalArrived = totalArrived
        };
    }

    private void UpdateRush(float dt)
    {
        if (rushTimer <= 0f)
        {
            rushMultiplier = 1f;
            return;
        }

        rushTimer -= dt;
        if (rushTimer <= 0f)
        {
            rushMultiplier = 1f;
        }
    }

    private void UpdateQueue(float dt, float serviceQualityMultiplier, MenuSystem menuSystem)
    {
        if (dt <= 0f)
        {
            return;
        }

        for (int i = queue.Count - 1; i >= 0; i--)
        {
            var entry = queue[i];
            entry.waitTime += dt;
            if (entry.waitTime >= entry.patience)
            {
                queue.RemoveAt(i);
                satisfaction = Mathf.Clamp01(satisfaction - 0.06f);
            }
        }

        spawnTimer -= dt;
        if (spawnTimer <= 0f)
        {
            if (queue.Count < maxQueue)
            {
                queue.Add(GenerateEntry(menuSystem));
                totalArrived++;
            }

            var satisfactionFactor = Mathf.Lerp(1.25f, 0.65f, satisfaction);
            var spawnMultiplier = Mathf.Max(0.25f, spawnRateMultiplier * eventSpawnMultiplier);
            spawnTimer = (baseSpawnInterval * satisfactionFactor * Random.Range(0.85f, 1.2f)) / spawnMultiplier;
        }

        serviceTimer -= dt * Mathf.Max(1f, serviceQualityMultiplier) * rushMultiplier * serviceRateMultiplier;
        if (autoServeEnabled && serviceTimer <= 0f && queue.Count > 0)
        {
            var served = queue[0];
            queue.RemoveAt(0);
            var waitRatio = served.patience > 0f ? Mathf.Clamp01(served.waitTime / served.patience) : 0f;
            var quality = Mathf.Clamp01(0.8f + (serviceQualityMultiplier - 1f) * 0.2f - waitRatio * 0.4f);
            RegisterService(quality);
            RecordServe(served.waitTime);
            totalServed++;
            var serviceMultiplier = Mathf.Max(0.25f, serviceRateMultiplier);
            serviceTimer = (baseServiceInterval * Random.Range(0.85f, 1.2f)) / serviceMultiplier;
        }
    }

    private void RecordServe(float waitTime)
    {
        var sample = new ServeSample { time = runtime, wait = Mathf.Max(0f, waitTime) };
        serveSamples.Enqueue(sample);
        serveWaitSum += sample.wait;
    }

    private void CullServeSamples()
    {
        while (serveSamples.Count > 0 && runtime - serveSamples.Peek().time > 60f)
        {
            var old = serveSamples.Dequeue();
            serveWaitSum -= old.wait;
        }
    }

    private CustomerQueueEntry GenerateEntry(MenuSystem menuSystem)
    {
        var customer = customers.Count > 0 ? customers[Random.Range(0, customers.Count)] : null;
        var customerTypeId = customer != null ? customer.id : string.Empty;
        var menuItem = menuSystem != null ? menuSystem.GetPreferredUnlockedItem(customerTypeId) : null;

        var entry = new CustomerQueueEntry();
        entry.customerTypeId = customerTypeId;
        entry.customerName = customer != null && !string.IsNullOrEmpty(customer.displayName) ? customer.displayName : "Guest";
        entry.menuId = menuItem != null ? menuItem.id : "";
        entry.menuName = menuItem != null && !string.IsNullOrEmpty(menuItem.displayName) ? menuItem.displayName : "BBQ Set";
        entry.menuBasePrice = menuItem != null ? (menuItem.basePrice * menuItem.bonusMultiplier) : 1.0;
        entry.patience = customer != null ? Mathf.Max(3f, customer.patience) : 10f;
        entry.waitTime = 0f;
        entry.tipMultiplier = customer != null ? Mathf.Max(0.8f, customer.tipMultiplier) : 1f;
        entry.requestedServings = 1;
        entry.requiresExactCut = false;
        entry.patience *= eventPatienceMultiplier;
        entry.tipMultiplier *= eventTipMultiplier;
        ApplyGuestFlavor(entry);
        return entry;
    }

    private void ApplyGuestFlavor(CustomerQueueEntry entry)
    {
        var vipChance = Mathf.Clamp01(VipChance + eventVipChanceBonus);
        var criticChance = Mathf.Clamp01(CriticChance + eventVipChanceBonus * 0.8f);
        var roll = Random.value;
        if (roll < vipChance)
        {
            entry.isVip = true;
            entry.customerName = "VIP " + entry.customerName;
            entry.tipMultiplier *= 1.75f;
            entry.patience *= 0.88f;
            return;
        }

        if (roll < vipChance + criticChance)
        {
            entry.isCritic = true;
            entry.customerName = "Critic " + entry.customerName;
            entry.tipMultiplier *= 1.45f;
            entry.patience *= 0.82f;
            entry.requiresExactCut = true;
            return;
        }

        if (roll < VipChance + CriticChance + RegularGroupChance)
        {
            entry.isPartyTable = true;
            entry.customerName = "Table 2 " + entry.customerName;
            entry.tipMultiplier *= 1.20f;
            entry.patience *= 1.12f;
            entry.requestedServings = 2;
        }
    }

    private void UpdateComboTimer(float dt)
    {
        if (comboCount <= 0)
        {
            comboTimer = 0f;
            return;
        }

        comboTimer -= dt;
        if (comboTimer <= 0f)
        {
            comboTimer = 0f;
            comboCount = 0;
        }
    }

    private void UpdateCombo(bool isManualServe, float quality, float waitRatio)
    {
        if (!isManualServe)
        {
            return;
        }

        var fastServe = waitRatio <= 0.4f;
        var strongQuality = quality >= 0.82f;
        var weakServe = quality < 0.6f || waitRatio >= 0.75f;

        if (strongQuality && fastServe)
        {
            comboCount = Mathf.Clamp(comboCount + 1, 0, comboMax);
            comboTimer = comboDuration;
            return;
        }

        if (weakServe)
        {
            comboCount = 0;
            comboTimer = 0f;
        }
    }

    private struct ServeSample
    {
        public float time;
        public float wait;
    }
}
