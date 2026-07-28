using System;
using System.Collections.Generic;
using UnityEngine;

public interface IOptionalEconomyGateway
{
    bool ShowRewarded(Action onRewarded);
    bool ShowInterstitial();
    bool PurchaseAndVerify(string packId, Action<string> onVerified);
}

public class OptionalEconomyService : MonoBehaviour
{
    [SerializeField] private OptionalEconomyConfig config;

    private GameManager gameManager;
    private IOptionalEconomyGateway gateway;
    private readonly HashSet<string> verifiedTransactionIds = new HashSet<string>();

    public OptionalEconomyConfig Config => config;

    public void Bind(
        GameManager gameManager,
        OptionalEconomyConfig config,
        IOptionalEconomyGateway gateway = null)
    {
        this.gameManager = gameManager;
        this.config = config;
        this.gateway = gateway;
    }

    public bool ShowRewardedAd()
    {
        if (config == null || !config.enableAds || gateway == null)
        {
            return false;
        }

        return gateway.ShowRewarded(
            () => gameManager?.ApplyAdBoost(config.rewardedMultiplier, config.rewardedDuration));
    }

    public bool ShowInterstitialAd()
    {
        if (config == null || !config.enableAds || gateway == null)
        {
            return false;
        }

        return gateway.ShowInterstitial();
    }

    public bool PurchasePack(string packId)
    {
        if (
            config == null ||
            !config.enableIap ||
            gateway == null ||
            string.IsNullOrEmpty(packId) ||
            FindPack(packId) == null)
        {
            return false;
        }

        return gateway.PurchaseAndVerify(
            packId,
            transactionId => GrantVerifiedPurchase(packId, transactionId));
    }

    private IapPack? FindPack(string packId)
    {
        for (int i = 0; i < config.packs.Count; i++)
        {
            if (config.packs[i].id == packId)
            {
                return config.packs[i];
            }
        }

        return null;
    }

    private void GrantVerifiedPurchase(string packId, string transactionId)
    {
        if (string.IsNullOrWhiteSpace(transactionId))
        {
            return;
        }

        var pack = FindPack(packId);
        if (!pack.HasValue || !verifiedTransactionIds.Add(transactionId))
        {
            return;
        }

        gameManager?.GrantCurrency(
            pack.Value.currencyReward,
            GameManager.RewardSource.Purchase);
    }
}
