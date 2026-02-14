using System.Collections.Generic;
using UnityEngine;

public static class DefaultDataFactory
{
    public static List<MenuItem> CreateMenuItems()
    {
        return new List<MenuItem>
        {
            CreateMenuItem("pork_belly", "Pork Belly", 1, 1.2f, 1.0f),
            CreateMenuItem("pork_shoulder", "Pork Shoulder", 2, 1.5f, 1.05f),
            CreateMenuItem("rib", "Pork Rib", 3, 2.0f, 1.1f),
            CreateMenuItem("spicy_pork", "Spicy Pork", 4, 2.6f, 1.15f),
            CreateMenuItem("kimchi_stew", "Kimchi Stew", 5, 3.0f, 1.18f),
            CreateMenuItem("beef_brisket", "Beef Brisket", 6, 3.8f, 1.2f),
            CreateMenuItem("premium_beef", "Premium Beef", 7, 4.5f, 1.22f),
            CreateMenuItem("signature_sauce", "Signature Sauce", 8, 5.5f, 1.25f),
            CreateMenuItem("cold_noodle", "Cold Noodle", 9, 6.5f, 1.28f),
            CreateMenuItem("seafood_set", "Seafood Set", 10, 8.0f, 1.3f),
            CreateMenuItem("mushroom_platter", "Mushroom Platter", 11, 9.5f, 1.32f),
            CreateMenuItem("rice_set", "Rice Set", 12, 11.0f, 1.35f),
            CreateMenuItem("soju", "Soju", 13, 12.5f, 1.38f),
            CreateMenuItem("makgeolli", "Makgeolli", 14, 14.0f, 1.4f),
            CreateMenuItem("bingsu", "Bingsu", 15, 16.0f, 1.45f)
        };
    }

    public static List<UpgradeData> CreateUpgrades()
    {
        return new List<UpgradeData>
        {
            CreateUpgrade("grill_upgrade", "Grill Upgrade", "income", "", 10f, 1.3f, 0.06f),
            CreateUpgrade("ventilation", "Ventilation", "income", "", 25f, 1.28f, 0.05f),
            CreateUpgrade("sizzle_master", "Sizzle Master", "sizzle", "", 15f, 1.25f, 0.03f),
            CreateUpgrade("staff_training", "Staff Training", "staff", "", 18f, 1.26f, 0.04f),
            CreateUpgrade("service_flow", "Service Flow", "service", "", 22f, 1.27f, 0.05f),
            CreateUpgrade("pork_belly_recipe", "Pork Belly Recipe", "menu", "pork_belly", 12f, 1.32f, 0.08f),
            CreateUpgrade("beef_brisket_recipe", "Beef Brisket Recipe", "menu", "beef_brisket", 18f, 1.33f, 0.08f),
            CreateUpgrade("premium_beef_recipe", "Premium Beef Recipe", "menu", "premium_beef", 30f, 1.35f, 0.09f),
            CreateUpgrade("signature_sauce_recipe", "Signature Sauce Recipe", "menu", "signature_sauce", 35f, 1.36f, 0.1f)
        };
    }

    public static List<StoreTier> CreateStoreTiers()
    {
        return new List<StoreTier>
        {
            CreateStoreTier("alley", "Alley", 1, 1.0f),
            CreateStoreTier("hongdae", "Hongdae", 4, 1.3f),
            CreateStoreTier("gangnam", "Gangnam", 7, 1.6f),
            CreateStoreTier("hanok", "Hanok", 10, 1.95f),
            CreateStoreTier("global", "Global", 14, 2.4f)
        };
    }

    public static List<CustomerType> CreateCustomerTypes()
    {
        return new List<CustomerType>
        {
            CreateCustomerType("local", "Local", 10f, 1.0f),
            CreateCustomerType("tourist", "Tourist", 12f, 1.1f),
            CreateCustomerType("foodie", "Foodie", 8f, 1.2f)
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
        tuning.baseRequirement = 50.0;
        tuning.requirementGrowth = 1.28;
        tuning.baseIncomePerSec = 1.0;
        tuning.incomeGrowth = 1.22;
        tuning.baseUpgradeCost = 10.0;
        tuning.upgradeGrowth = 1.3;
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
