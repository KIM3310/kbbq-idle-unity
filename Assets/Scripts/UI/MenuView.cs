using UnityEngine;

public class MenuView : MonoBehaviour
{
    public void Show(bool visible)
    {
        gameObject.SetActive(visible);
    }
}
