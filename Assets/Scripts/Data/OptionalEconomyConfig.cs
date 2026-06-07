using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct IapPack
{
    public string id;
    public string displayName;
    public string priceLabel;
    public double currencyReward;
}

[CreateAssetMenu(menuName = "KBBQ/OptionalEconomy Config")]
public class OptionalEconomyConfig : ScriptableObject
{
    public bool enableAds = true;
    public bool enableIap = true;
    public float rewardedMultiplier = 2f;
    public float rewardedDuration = 120f;
    public double interstitialReward = 100;
    public List<IapPack> packs = new List<IapPack>();
}
