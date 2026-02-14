using UnityEngine;

public class DebugToggleButton : MonoBehaviour
{
    public void ToggleDebug()
    {
        GameManager.I?.ToggleDebugUI();
    }
}
