#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class KBBQDataValidator
{
    public struct Report
    {
        public int errors;
        public int warnings;
        public List<string> messages;

        public bool HasErrors => errors > 0;
    }

    [MenuItem("KBBQ/Validate Data (Portfolio)")]
    public static void ValidateMenu()
    {
        var report = Validate();
        var header = $"KBBQ Validate: errors={report.errors}, warnings={report.warnings}";

        if (report.messages != null)
        {
            foreach (var line in report.messages)
            {
                if (line.StartsWith("ERROR"))
                {
                    Debug.LogError(line);
                }
                else if (line.StartsWith("WARN"))
                {
                    Debug.LogWarning(line);
                }
                else
                {
                    Debug.Log(line);
                }
            }
        }

        if (report.HasErrors)
        {
            Debug.LogError(header);
        }
        else
        {
            Debug.Log(header);
        }
    }

    public static Report Validate()
    {
        var messages = new List<string>();
        var errors = 0;
        var warnings = 0;

        void Error(string msg)
        {
            errors++;
            messages.Add("ERROR: " + msg);
        }

        void Warn(string msg)
        {
            warnings++;
            messages.Add("WARN: " + msg);
        }

        // Menu items
        var menuItems = LoadAllAssets<MenuItem>();
        ValidateUniqueIds(menuItems.Select(x => x != null ? x.id : null), "MenuItem.id", Error);
        foreach (var item in menuItems)
        {
            if (item == null) continue;
            if (item.unlockLevel < 1) Warn($"MenuItem '{item.id}' has unlockLevel < 1");
            if (item.basePrice <= 0) Warn($"MenuItem '{item.id}' has basePrice <= 0");
            if (item.bonusMultiplier <= 0) Warn($"MenuItem '{item.id}' has bonusMultiplier <= 0");
        }

        // Store tiers
        var storeTiers = LoadAllAssets<StoreTier>().OrderBy(t => t != null ? t.unlockLevel : 0).ToList();
        ValidateUniqueIds(storeTiers.Select(x => x != null ? x.id : null), "StoreTier.id", Error);
        var lastUnlock = 0;
        foreach (var tier in storeTiers)
        {
            if (tier == null) continue;
            if (tier.unlockLevel < lastUnlock) Error($"StoreTier unlock levels are not monotonic: '{tier.id}'");
            lastUnlock = tier.unlockLevel;
            if (tier.incomeMultiplier <= 0) Warn($"StoreTier '{tier.id}' has incomeMultiplier <= 0");
        }

        // Upgrades
        var upgrades = LoadAllAssets<UpgradeData>();
        ValidateUniqueIds(upgrades.Select(x => x != null ? x.id : null), "UpgradeData.id", Error);
        var allowedCategories = new HashSet<string>(new[] { "income", "menu", "staff", "service", "sizzle" });
        foreach (var up in upgrades)
        {
            if (up == null) continue;
            var cat = (up.category ?? "").Trim().ToLowerInvariant();
            if (!allowedCategories.Contains(cat)) Warn($"Upgrade '{up.id}' has unknown category '{up.category}'");
            if (up.baseCost <= 0) Warn($"Upgrade '{up.id}' has baseCost <= 0");
            if (up.costMultiplier <= 1f) Warn($"Upgrade '{up.id}' has costMultiplier <= 1 (no scaling)");
            if (up.effectValue <= 0f) Warn($"Upgrade '{up.id}' has effectValue <= 0");
        }

        // Api config (portfolio defaults)
        var apiConfigs = LoadAllAssets<ApiConfig>();
        foreach (var api in apiConfigs)
        {
            if (api == null) continue;

            if (api.enableNetwork)
            {
                if (string.IsNullOrEmpty(api.baseUrl)) Error("ApiConfig.enableNetwork is true but baseUrl is empty");
                if (string.IsNullOrEmpty(api.hmacSecret) || api.hmacSecret == "CHANGE_ME")
                {
                    Error("ApiConfig.enableNetwork is true but hmacSecret is not set");
                }
            }
            else
            {
                // This repo intentionally ships with networking disabled.
                if (!string.IsNullOrEmpty(api.baseUrl))
                {
                    Warn("ApiConfig has baseUrl set but enableNetwork is false (ok for local dev)");
                }
            }
        }

        return new Report { errors = errors, warnings = warnings, messages = messages };
    }

    private static List<T> LoadAllAssets<T>() where T : UnityEngine.Object
    {
        var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
        var list = new List<T>(guids.Length);
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
            {
                list.Add(asset);
            }
        }
        return list;
    }

    private static void ValidateUniqueIds(IEnumerable<string> ids, string label, System.Action<string> onError)
    {
        var seen = new HashSet<string>();
        foreach (var raw in ids)
        {
            var id = (raw ?? "").Trim();
            if (string.IsNullOrEmpty(id))
            {
                onError(label + " contains an empty id");
                continue;
            }

            if (!seen.Add(id))
            {
                onError(label + " contains duplicates: " + id);
            }
        }
    }
}
#endif

