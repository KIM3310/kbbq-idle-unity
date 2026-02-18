using System.Collections.Generic;
using UnityEngine;

public class CustomerQueueEntry
{
    public string customerName;
    public string menuId;
    public string menuName;
    public double menuBasePrice;
    public float patience;
    public float waitTime;
    public float tipMultiplier;
}

public struct ServeResult
{
    public bool served;
    public float quality;
    public float waitRatio;
    public float tipMultiplier;
    public string menuId;
    public string menuName;
    public double basePrice;
    public int comboCount;
}

public class CustomerSystem
{
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
    private float runtime = 0f;
    private float serveWaitSum = 0f;
    private int comboCount = 0;
    private float comboTimer = 0f;
    private float comboDuration = 6f;
    private int comboMax = 8;
    private float comboStepBonus = 0.05f;
    private bool autoServeEnabled = true;

    public float Satisfaction => satisfaction;
    public IReadOnlyList<CustomerQueueEntry> Queue => queue;
    public float SpawnRateMultiplier => spawnRateMultiplier;
    public float ServiceRateMultiplier => serviceRateMultiplier;
    public int ComboCount => comboCount;
    public float ComboTimeRemaining => comboTimer;
    public float ComboDuration => comboDuration;

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
        result.menuId = served.menuId;
        result.menuName = served.menuName;
        result.basePrice = served.menuBasePrice;
        result.comboCount = comboCount;
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

    public void SetAutoServeEnabled(bool enabled)
    {
        autoServeEnabled = enabled;
    }

    public CustomerQueueEntry PeekNext()
    {
        return queue.Count > 0 ? queue[0] : null;
    }

    public QueueMetrics GetMetrics()
    {
        var count = serveSamples.Count;
        return new QueueMetrics
        {
            queueCount = queue.Count,
            avgWaitSeconds = count > 0 ? serveWaitSum / count : 0f,
            servedPerMinute = count
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
            }

            var satisfactionFactor = Mathf.Lerp(1.25f, 0.65f, satisfaction);
            var spawnMultiplier = Mathf.Max(0.25f, spawnRateMultiplier);
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
        var menuItem = menuSystem != null ? menuSystem.GetRandomUnlockedItem() : null;

        var entry = new CustomerQueueEntry();
        entry.customerName = customer != null && !string.IsNullOrEmpty(customer.displayName) ? customer.displayName : "Guest";
        entry.menuId = menuItem != null ? menuItem.id : "";
        entry.menuName = menuItem != null && !string.IsNullOrEmpty(menuItem.displayName) ? menuItem.displayName : "BBQ Set";
        entry.menuBasePrice = menuItem != null ? (menuItem.basePrice * menuItem.bonusMultiplier) : 1.0;
        entry.patience = customer != null ? Mathf.Max(3f, customer.patience) : 10f;
        entry.waitTime = 0f;
        entry.tipMultiplier = customer != null ? Mathf.Max(0.8f, customer.tipMultiplier) : 1f;
        return entry;
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
