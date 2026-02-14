using UnityEngine;

[CreateAssetMenu(menuName = "KBBQ/Store Tier")]
public class StoreTier : ScriptableObject
{
    public string id = "alley";
    public string displayName = "Alley";
    public int unlockLevel = 1;
    public float incomeMultiplier = 1f;
}
