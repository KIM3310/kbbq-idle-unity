using UnityEngine;

[CreateAssetMenu(menuName = "KBBQ/Menu Item")]
public class MenuItem : ScriptableObject
{
    public string id = "pork_belly";
    public string displayName = "Pork Belly";
    public int unlockLevel = 1;
    public float basePrice = 1f;
    public float bonusMultiplier = 1f;
}
