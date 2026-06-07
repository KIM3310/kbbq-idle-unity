using UnityEngine;

public class OptionalEconomyService : MonoBehaviour
{
    [SerializeField] private OptionalEconomyConfig config;

    private GameManager gameManager;

    public OptionalEconomyConfig Config => config;

    public void Bind(GameManager gameManager, OptionalEconomyConfig config)
    {
        this.gameManager = gameManager;
        this.config = config;
    }

    public bool ShowRewardedAd()
    {
        if (config == null || !config.enableAds)
        {
            return false;
        }

        gameManager?.ApplyAdBoost(config.rewardedMultiplier, config.rewardedDuration);
        return true;
    }

    public bool ShowInterstitialAd()
    {
        if (config == null || !config.enableAds)
        {
            return false;
        }

        gameManager?.GrantCurrency(config.interstitialReward, GameManager.RewardSource.Ad);
        return true;
    }

    public bool PurchasePack(string packId)
    {
        if (config == null || !config.enableIap || string.IsNullOrEmpty(packId))
        {
            return false;
        }

        for (int i = 0; i < config.packs.Count; i++)
        {
            if (config.packs[i].id == packId)
            {
                gameManager?.GrantCurrency(config.packs[i].currencyReward, GameManager.RewardSource.Purchase);
                return true;
            }
        }

        return false;
    }
}
