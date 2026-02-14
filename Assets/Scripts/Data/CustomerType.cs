using UnityEngine;

[CreateAssetMenu(menuName = "KBBQ/Customer Type")]
public class CustomerType : ScriptableObject
{
    public string id = "local";
    public string displayName = "Local";
    public float patience = 10f;
    public float tipMultiplier = 1f;
}
