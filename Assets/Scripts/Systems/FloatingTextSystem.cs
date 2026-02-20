using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FloatingTextSystem : MonoBehaviour
{
    public static FloatingTextSystem I { get; private set; }

    private RectTransform textContainer;
    private List<RectTransform> pool = new List<RectTransform>();

    private void Awake()
    {
        if (I == null)
        {
            I = this;
            SetupContainer();
        }
    }

    private void SetupContainer()
    {
        var canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;

        var go = new GameObject("FloatingTextContainer", typeof(RectTransform));
        textContainer = go.GetComponent<RectTransform>();
        textContainer.SetParent(canvas.transform, false);
        textContainer.anchorMin = Vector2.zero;
        textContainer.anchorMax = Vector2.one;
        textContainer.offsetMin = Vector2.zero;
        textContainer.offsetMax = Vector2.zero;
    }

    public void Spawn(string text, Vector2 screenPosition, Color color, float scale = 1f)
    {
        if (textContainer == null) return;
        StartCoroutine(AnimateText(text, screenPosition, color, scale));
    }

    private IEnumerator AnimateText(string msg, Vector2 startPos, Color color, float scale)
    {
        RectTransform rt = GetFromPool();
        rt.gameObject.SetActive(true);
        rt.position = startPos;
        rt.localScale = Vector3.one * scale;

        Text textComp = rt.GetComponent<Text>();
        textComp.text = msg;
        textComp.color = color;

        float t = 0;
        float duration = 1.2f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float n = t / duration;

            // Float up and fade out
            rt.position = startPos + new Vector2(0, 150f * Mathf.Pow(n, 0.5f));
            Color c = textComp.color;
            c.a = 1f - Mathf.Pow(n, 3f);
            textComp.color = c;

            yield return null;
        }

        rt.gameObject.SetActive(false);
        pool.Add(rt);
    }

    private RectTransform GetFromPool()
    {
        if (pool.Count > 0)
        {
            var rt = pool[pool.Count - 1];
            pool.RemoveAt(pool.Count - 1);
            return rt;
        }

        return CreateNewText();
    }

    private RectTransform CreateNewText()
    {
        var go = new GameObject("FloatingText", typeof(RectTransform), typeof(Text), typeof(Shadow));
        var rt = go.GetComponent<RectTransform>();
        rt.SetParent(textContainer, false);
        rt.sizeDelta = new Vector2(300, 100);

        var text = go.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontStyle = FontStyle.Bold;
        text.fontSize = 42;
        text.alignment = TextAnchor.MiddleCenter;

        var shadow = go.GetComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.5f);
        shadow.effectDistance = new Vector2(2, -2);

        return rt;
    }
}
