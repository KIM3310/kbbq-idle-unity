using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "KBBQ/Game Data Catalog")]
public class GameDataCatalog : ScriptableObject
{
    public List<MenuItem> menuItems = new List<MenuItem>();
    public List<UpgradeData> upgrades = new List<UpgradeData>();
    public List<StoreTier> storeTiers = new List<StoreTier>();
    public List<CustomerType> customerTypes = new List<CustomerType>();
    public EconomyTuning economyTuning;
    public MonetizationConfig monetizationConfig;
    public ApiConfig apiConfig;
}
