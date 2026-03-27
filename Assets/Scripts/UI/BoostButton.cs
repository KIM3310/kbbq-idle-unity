using UnityEngine;

public class BoostButton : MonoBehaviour
{
    public void TriggerBoost()
    {
        GameManager.I?.TriggerSizzleBoost();
    }
}
