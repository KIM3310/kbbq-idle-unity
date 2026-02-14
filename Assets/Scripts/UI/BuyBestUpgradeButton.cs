using UnityEngine;

public class BuyBestUpgradeButton : MonoBehaviour
{
    public void TriggerBuyBest()
    {
        GameManager.I?.BuyBestUpgrade();
    }
}
