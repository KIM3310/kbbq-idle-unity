using UnityEngine;

[CreateAssetMenu(menuName = "KBBQ/Upgrade Data")]
public class UpgradeData : ScriptableObject
{
    public string id = "grill_upgrade";
    public string displayName = "Grill Upgrade";
    public string category = "income"; // income, menu, staff, sizzle
    public string targetId = "";
    public int level = 0;
    public float baseCost = 10f;
    public float costMultiplier = 1.3f;
    public float effectValue = 0.05f;
}
