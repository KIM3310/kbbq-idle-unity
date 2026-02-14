using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeListView : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform content;
    [SerializeField] private UpgradeRowView rowTemplate;
    [SerializeField] private int initialPoolSize = 8;

    private readonly List<UpgradeRowView> pool = new List<UpgradeRowView>();
    private GameManager gameManager;

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
            rowTemplate.gameObject.SetActive(false);
        }

        BuildPool(initialPoolSize);
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
}
