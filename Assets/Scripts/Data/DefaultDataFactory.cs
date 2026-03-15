using System.Collections.Generic;
using UnityEngine;

public static class DefaultDataFactory
{
    public static List<MenuItem> CreateMenuItems()
    {
        return new List<MenuItem>
        {
            CreateMenuItem("pork_belly", "Pork Belly", 1, 1.2f, 1.0f),
            CreateMenuItem("pork_shoulder", "Pork Shoulder", 2, 1.6f, 1.06f),
            CreateMenuItem("rib", "Pork Rib", 3, 2.2f, 1.12f),
            CreateMenuItem("spicy_pork", "Spicy Pork", 3, 2.9f, 1.16f),
            CreateMenuItem("kimchi_stew", "Kimchi Stew", 4, 3.4f, 1.20f),
            CreateMenuItem("beef_brisket", "Beef Brisket", 5, 4.1f, 1.23f),
            CreateMenuItem("premium_beef", "Premium Beef", 6, 4.9f, 1.26f),
            CreateMenuItem("signature_sauce", "Signature Sauce", 7, 6.0f, 1.29f),
            CreateMenuItem("cold_noodle", "Cold Noodle", 8, 7.0f, 1.31f),
            CreateMenuItem("seafood_set", "Seafood Set", 9, 8.5f, 1.34f),
            CreateMenuItem("mushroom_platter", "Mushroom Platter", 10, 10.2f, 1.36f),
            CreateMenuItem("rice_set", "Rice Set", 11, 11.8f, 1.39f),
            CreateMenuItem("soju", "Soju", 12, 13.8f, 1.42f),
            CreateMenuItem("makgeolli", "Makgeolli", 13, 15.2f, 1.45f),
            CreateMenuItem("bingsu", "Bingsu", 14, 17.8f, 1.50f)
        };
    }

    public static List<UpgradeData> CreateUpgrades()
    {
        return new List<UpgradeData>
        {
            CreateUpgrade("grill_upgrade", "Grill Upgrade", "income", "", 8f, 1.27f, 0.07f),
            CreateUpgrade("ventilation", "Ventilation", "income", "", 20f, 1.25f, 0.05f),
            CreateUpgrade("sizzle_master", "Sizzle Master", "sizzle", "", 12f, 1.22f, 0.04f),
            CreateUpgrade("staff_training", "Staff Training", "staff", "", 15f, 1.24f, 0.05f),
            CreateUpgrade("service_flow", "Service Flow", "service", "", 18f, 1.25f, 0.06f),
            CreateUpgrade("pork_belly_recipe", "Pork Belly Recipe", "menu", "pork_belly", 10f, 1.28f, 0.09f),
            CreateUpgrade("beef_brisket_recipe", "Beef Brisket Recipe", "menu", "beef_brisket", 16f, 1.30f, 0.09f),
            CreateUpgrade("premium_beef_recipe", "Premium Beef Recipe", "menu", "premium_beef", 26f, 1.32f, 0.10f),
            CreateUpgrade("signature_sauce_recipe", "Signature Sauce Recipe", "menu", "signature_sauce", 30f, 1.34f, 0.11f)
        };
    }

    public static List<StoreTier> CreateStoreTiers()
    {
        return new List<StoreTier>
        {
            CreateStoreTier("alley", "Alley", 1, 1.0f),
            CreateStoreTier("hongdae", "Hongdae", 3, 1.28f),
            CreateStoreTier("gangnam", "Gangnam", 6, 1.60f),
            CreateStoreTier("hanok", "Hanok", 9, 1.96f),
            CreateStoreTier("global", "Global", 13, 2.45f)
        };
    }

    public static List<CustomerType> CreateCustomerTypes()
    {
        return new List<CustomerType>
        {
            CreateCustomerType("local", "Local", 10.5f, 1.0f),
            CreateCustomerType("tourist", "Tourist", 12.5f, 1.14f),
            CreateCustomerType("foodie", "Foodie", 8.4f, 1.24f)
        };
    }

    public static ApiConfig CreateApiConfig()
    {
        var config = ScriptableObject.CreateInstance<ApiConfig>();
        // Keep networking disabled by default in the portfolio build.
        // Reviewers can opt-in by setting a real base URL + secret.
        config.baseUrl = "";
        config.region = "KR";
        config.hmacSecret = "CHANGE_ME";
        config.timeoutSeconds = 10;
        config.enableNetwork = false;
        config.allowInEditor = false;
        return config;
    }

    public static EconomyTuning CreateEconomyTuning()
    {
        var tuning = ScriptableObject.CreateInstance<EconomyTuning>();
        tuning.maxLevel = 100;
        tuning.baseRequirement = 42.0;
        tuning.requirementGrowth = 1.24;
        tuning.baseIncomePerSec = 1.18;
        tuning.incomeGrowth = 1.20;
        tuning.baseUpgradeCost = 8.0;
        tuning.upgradeGrowth = 1.26;
        tuning.RebuildTable();
        return tuning;
    }

    public static MonetizationConfig CreateMonetizationConfig()
    {
        var config = ScriptableObject.CreateInstance<MonetizationConfig>();
        config.enableAds = true;
        config.enableIap = true;
        config.rewardedMultiplier = 2f;
        config.rewardedDuration = 120f;
        config.interstitialReward = 100;
        config.packs = new List<IapPack>
        {
            new IapPack { id = "starter", displayName = "Starter Pack", priceLabel = "$0.99", currencyReward = 500 },
            new IapPack { id = "premium", displayName = "Premium Pack", priceLabel = "$4.99", currencyReward = 4000 }
        };
        return config;
    }

    private static MenuItem CreateMenuItem(string id, string name, int unlockLevel, float basePrice, float bonusMultiplier)
    {
        var item = ScriptableObject.CreateInstance<MenuItem>();
        item.id = id;
        item.displayName = name;
        item.unlockLevel = unlockLevel;
        item.basePrice = basePrice;
        item.bonusMultiplier = bonusMultiplier;
        return item;
    }

    private static UpgradeData CreateUpgrade(string id, string displayName, string category, string targetId, float baseCost, float costMultiplier, float effectValue)
    {
        var upgrade = ScriptableObject.CreateInstance<UpgradeData>();
        upgrade.id = id;
        upgrade.displayName = displayName;
        upgrade.category = category;
        upgrade.targetId = targetId;
        upgrade.baseCost = baseCost;
        upgrade.costMultiplier = costMultiplier;
        upgrade.effectValue = effectValue;
        return upgrade;
    }

    private static StoreTier CreateStoreTier(string id, string name, int unlockLevel, float incomeMultiplier)
    {
        var tier = ScriptableObject.CreateInstance<StoreTier>();
        tier.id = id;
        tier.displayName = name;
        tier.unlockLevel = unlockLevel;
        tier.incomeMultiplier = incomeMultiplier;
        return tier;
    }

    private static CustomerType CreateCustomerType(string id, string name, float patience, float tipMultiplier)
    {
        var customer = ScriptableObject.CreateInstance<CustomerType>();
        customer.id = id;
        customer.displayName = name;
        customer.patience = patience;
        customer.tipMultiplier = tipMultiplier;
        return customer;
    }
}
