using UnityEngine;

public class MenuView : MonoBehaviour
{
    public void Show(bool visible)
    {
        if (visible == gameObject.activeSelf) return;

        if (visible)
        {
            gameObject.SetActive(true);
            transform.localScale = Vector3.one * 0.8f;
            StartCoroutine(AnimatePopIn());
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private System.Collections.IEnumerator AnimatePopIn()
    {
        float t = 0;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * 6f; 
            float val = 1f - Mathf.Exp(-5f * t) * Mathf.Cos(30f * t);
            transform.localScale = Vector3.one * val;
            yield return null;
        }
        transform.localScale = Vector3.one;
    }
}
