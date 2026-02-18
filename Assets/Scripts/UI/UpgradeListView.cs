using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeListView : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform content;
    [SerializeField] private UpgradeRowView rowTemplate;
    [SerializeField] private int initialPoolSize = 8;
    [SerializeField] private float rowHeight = 84f;
    [SerializeField] private float rowSpacing = 8f;
    [SerializeField] private float rowHorizontalPadding = 6f;

    private readonly List<UpgradeRowView> pool = new List<UpgradeRowView>();
    private GameManager gameManager;
    private float lastContentWidth = -1f;

    private void Awake()
    {
        if (scrollRect == null)
        {
            scrollRect = GetComponentInChildren<ScrollRect>();
        }

        if (content == null && scrollRect != null)
        {
            content = scrollRect.content;
        }

        if (rowTemplate == null)
        {
            rowTemplate = GetComponentInChildren<UpgradeRowView>(true);
        }

        if (rowTemplate != null)
        {
            ConfigureRowRect(rowTemplate.transform as RectTransform);
            rowTemplate.gameObject.SetActive(false);
        }

        EnsureContentLayout();
        BuildPool(initialPoolSize);
        UpdateRowWidths();
    }

    private void LateUpdate()
    {
        if (content == null)
        {
            return;
        }

        var width = content.rect.width;
        if (Mathf.Abs(width - lastContentWidth) > 0.5f)
        {
            UpdateRowWidths();
        }
    }

    public void Bind(GameManager manager)
    {
        gameManager = manager;
        for (int i = 0; i < pool.Count; i++)
        {
            pool[i].Bind(manager);
        }
    }

    public void Render(IReadOnlyList<UpgradeUiEntry> upgrades)
    {
        var count = upgrades != null ? upgrades.Count : 0;
        EnsurePoolSize(count);
        UpdateRowWidths();

        for (int i = 0; i < pool.Count; i++)
        {
            if (upgrades != null && i < upgrades.Count)
            {
                var entry = upgrades[i];
                var row = pool[i];
                row.gameObject.SetActive(true);
                row.Bind(gameManager);
                row.SetData(entry);
            }
            else
            {
                pool[i].Clear();
                pool[i].gameObject.SetActive(false);
            }
        }

        if (content != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        }
    }

    private void BuildPool(int count)
    {
        if (rowTemplate == null || content == null)
        {
            return;
        }

        for (int i = pool.Count; i < count; i++)
        {
            var instance = Instantiate(rowTemplate, content);
            ConfigureRowRect(instance.transform as RectTransform);
            instance.gameObject.SetActive(false);
            instance.Bind(gameManager);
            pool.Add(instance);
        }
    }

    private void EnsurePoolSize(int count)
    {
        if (pool.Count < count)
        {
            BuildPool(count);
        }
    }

    private void EnsureContentLayout()
    {
        if (content == null)
        {
            return;
        }

        var layout = content.GetComponent<VerticalLayoutGroup>();
        if (layout == null)
        {
            layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
        }

        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        layout.spacing = rowSpacing;
        layout.padding = new RectOffset(0, 0, 0, 0);

        var fitter = content.GetComponent<ContentSizeFitter>();
        if (fitter == null)
        {
            fitter = content.gameObject.AddComponent<ContentSizeFitter>();
        }

        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    private void ConfigureRowRect(RectTransform rect)
    {
        if (rect == null)
        {
            return;
        }

        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.sizeDelta = new Vector2(0f, rowHeight);

        var element = rect.GetComponent<LayoutElement>();
        if (element == null)
        {
            element = rect.gameObject.AddComponent<LayoutElement>();
        }
        element.minHeight = rowHeight;
        element.preferredHeight = rowHeight;
        element.flexibleHeight = 0f;
    }

    private void UpdateRowWidths()
    {
        if (content == null)
        {
            return;
        }

        lastContentWidth = content.rect.width;
        var width = Mathf.Max(80f, content.rect.width - (rowHorizontalPadding * 2f));

        for (int i = 0; i < pool.Count; i++)
        {
            var rect = pool[i] != null ? pool[i].transform as RectTransform : null;
            if (rect == null)
            {
                continue;
            }

            var size = rect.sizeDelta;
            size.x = width;
            size.y = rowHeight;
            rect.sizeDelta = size;
        }
    }
}
