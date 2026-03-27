using UnityEngine;

public class SettingsView : MonoBehaviour
{
    public void Show(bool visible)
    {
        gameObject.SetActive(visible);
    }
}
