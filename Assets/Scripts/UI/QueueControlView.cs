using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QueueControlView : MonoBehaviour
{
    private const int MaxVisibleCards = 4;

    [SerializeField] private Button serveButton;
    [SerializeField] private Button rushButton;

    private readonly QueueCard[] queueCards = new QueueCard[MaxVisibleCards];
    private readonly Dictionary<string, Sprite> avatarCache = new Dictionary<string, Sprite>();
    private readonly Dictionary<string, Sprite> cutSpriteCache = new Dictionary<string, Sprite>();

    private GameManager gameManager;
    private RectTransform root;
    private RectTransform runtimeRoot;
    private RectTransform cardsRoot;
    private Text summaryText;
    private Text helperText;
    private Text eatText;
    private Image eatAvatar;
    private Image eatSpark;
    private Image eaterPanel;
    private float eatTimer;
    private bool eatHappy;
    private QueueMetrics latestMetrics;
    private double latestCurrency;
    private Vector2 lastCardsSize = new Vector2(-1f, -1f);
    private float layoutPollTimer;

    private Sprite bubbleSprite;
    private Sprite coinSprite;
    private Sprite customerBaseSprite;
    private Sprite eaterSparkSprite;
    private Sprite meatBellySprite;
    private Sprite meatBrisketSprite;
    private Sprite meatRibSprite;
    private Sprite meatSeafoodSprite;
    private Sprite meatGenericSprite;

    private struct QueueCard
    {
        public RectTransform root;
        public Image panel;
        public Image avatar;
        public Image bubble;
        public Image requestIcon;
        public Text bubbleText;
        public Text nameText;
        public Text timerText;
        public Image patienceBack;
        public Image patienceFill;
    }

    private void Awake()
    {
        root = transform as RectTransform;
        BuildSprites();
        BuildRuntimeVisual();
    }

    private void Update()
    {
        LayoutCardsIfNeeded(false);

        if (eatTimer <= 0f || eatAvatar == null)
        {
            return;
        }

        eatTimer -= Time.unscaledDeltaTime;
        var t = Mathf.Clamp01(1f - (eatTimer / 2.8f));
        var chew = Mathf.Sin(Time.unscaledTime * 12f) * 5f;

        var color = eatAvatar.color;
        color.a = Mathf.Lerp(0.95f, 0f, t);
        eatAvatar.color = color;

        if (eatText != null)
        {
            var textColor = eatText.color;
            textColor.a = Mathf.Lerp(1f, 0f, t);
            eatText.color = textColor;
            eatText.rectTransform.anchoredPosition = new Vector2(74f + chew, 10f + (t * 22f));
        }

        eatAvatar.rectTransform.localScale = Vector3.one * (1f + Mathf.Sin(Time.unscaledTime * 10f) * 0.08f);
        if (eaterPanel != null)
        {
            eaterPanel.color = eatHappy
                ? new Color(0.28f, 0.18f, 0.12f, Mathf.Lerp(0.60f, 0.28f, t))
                : new Color(0.30f, 0.15f, 0.12f, Mathf.Lerp(0.60f, 0.28f, t));
        }
        if (eatSpark != null)
        {
            var sparkScale = 0.92f + Mathf.Sin(Time.unscaledTime * 14f) * 0.10f;
            eatSpark.rectTransform.localScale = new Vector3(sparkScale, sparkScale, 1f);
            eatSpark.color = eatHappy
                ? new Color(1f, 0.78f, 0.34f, Mathf.Lerp(0.62f, 0f, t))
                : new Color(0.88f, 0.48f, 0.34f, Mathf.Lerp(0.62f, 0f, t));
        }

        if (eatTimer <= 0f)
        {
            eatAvatar.gameObject.SetActive(false);
            if (eatText != null)
            {
                eatText.gameObject.SetActive(false);
            }
        }
    }

    public void Bind(GameManager manager)
    {
        gameManager = manager;

        if (serveButton == null)
        {
            serveButton = FindButton("ServeButton");
        }

        if (rushButton == null)
        {
            rushButton = FindButton("RushButton");
        }

        if (serveButton != null)
        {
            serveButton.onClick.RemoveAllListeners();
            serveButton.onClick.AddListener(HandleServe);
        }

        if (rushButton != null)
        {
            rushButton.onClick.RemoveAllListeners();
            rushButton.onClick.AddListener(HandleRush);
        }
    }

    public void RenderQueue(IReadOnlyList<CustomerQueueEntry> queue)
    {
        var queueCount = queue != null ? queue.Count : 0;

        for (int i = 0; i < queueCards.Length; i++)
        {
            var has = queue != null && i < queue.Count && queue[i] != null;
            var card = queueCards[i];
            if (card.root == null)
            {
                continue;
            }

            card.root.gameObject.SetActive(has);
            if (!has)
            {
                continue;
            }

            var entry = queue[i];
            var remaining = Mathf.Max(0f, entry.patience - entry.waitTime);
            var patience = entry.patience > 0f ? Mathf.Clamp01(remaining / entry.patience) : 0f;

            if (card.nameText != null)
            {
                card.nameText.text = ClipText(entry.customerName, 11);
            }

            if (card.timerText != null)
            {
                card.timerText.text = remaining.ToString("0") + "s";
                card.timerText.color = Color.Lerp(new Color(1f, 0.32f, 0.26f, 1f), new Color(0.94f, 0.90f, 0.78f, 1f), patience);
            }

            if (card.bubbleText != null)
            {
                card.bubbleText.text = ClipText(entry.menuName, 11);
            }

            if (card.avatar != null)
            {
                card.avatar.sprite = ResolveAvatar(entry.customerName, i);
                card.avatar.color = Color.white;
            }

            if (card.requestIcon != null)
            {
                card.requestIcon.sprite = ResolveCutSprite(entry.menuId, entry.menuName);
                card.requestIcon.color = Color.white;
            }

            if (card.patienceFill != null)
            {
                card.patienceFill.fillAmount = patience;
                card.patienceFill.color = Color.Lerp(new Color(0.90f, 0.22f, 0.18f, 1f), new Color(0.30f, 0.82f, 0.42f, 1f), patience);
            }
        }

        if (helperText != null)
        {
            helperText.text = queueCount > 0
                ? "Tap customer cards to see requested cut. Serve quickly for better combo/tips."
                : "No customers waiting. Keep grilling to prepare for next wave.";
        }

        RefreshSummary();
    }

    public void RenderMetrics(QueueMetrics metrics, double currency)
    {
        latestMetrics = metrics;
        latestCurrency = currency;
        RefreshSummary();
    }

    public void PlayEating(string customerName, string menuName, bool happy)
    {
        if (eatAvatar == null)
        {
            return;
        }

        eatHappy = happy;
        eatTimer = 2.8f;
        eatAvatar.gameObject.SetActive(true);
        eatAvatar.sprite = ResolveAvatar(customerName, 0);
        eatAvatar.color = Color.white;

        if (eatText != null)
        {
            eatText.gameObject.SetActive(true);
            eatText.text = ClipText(customerName, 9) + (happy ? " yum " : " hmm ") + ClipText(menuName, 10);
            eatText.color = happy
                ? new Color(0.96f, 0.93f, 0.74f, 1f)
                : new Color(0.93f, 0.75f, 0.68f, 1f);
        }
    }

    private void HandleServe()
    {
        gameManager?.ServeNextCustomer();
    }

    private void HandleRush()
    {
        gameManager?.TriggerRushService();
    }

    private void RefreshSummary()
    {
        if (summaryText == null)
        {
            return;
        }

        summaryText.text = "$ " + FormatUtil.FormatCurrency(latestCurrency)
            + "   |   SERVED " + latestMetrics.totalServed
            + "   |   CUSTOMERS " + latestMetrics.totalArrived
            + "   |   QUEUE " + latestMetrics.queueCount;
    }

    private void BuildRuntimeVisual()
    {
        if (root == null || runtimeRoot != null)
        {
            return;
        }

        HideLegacy("QueueTitle");
        HideLegacy("QueueHint");
        HideLegacy("QueueList");
        HideLegacy("QueueMetrics");

        var runtime = new GameObject("RuntimeQueueVisual", typeof(RectTransform), typeof(Image));
        runtimeRoot = runtime.GetComponent<RectTransform>();
        runtimeRoot.SetParent(root, false);
        SetRect(runtimeRoot, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(12f, 72f), new Vector2(-12f, -10f));
        var runtimeBg = runtime.GetComponent<Image>();
        runtimeBg.color = new Color(0.20f, 0.14f, 0.10f, 0.32f);

        var coinIcon = CreateImage("CoinIcon", runtimeRoot, coinSprite, new Color(1f, 0.91f, 0.62f, 1f), false);
        SetRect(coinIcon.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(8f, -38f), new Vector2(36f, -10f));

        summaryText = CreateText("SummaryText", runtimeRoot, 12, TextAnchor.MiddleLeft, new Color(0.97f, 0.93f, 0.82f, 1f));
        SetRect(summaryText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(44f, -42f), new Vector2(-8f, -8f));

        cardsRoot = new GameObject("CardsRoot", typeof(RectTransform)).GetComponent<RectTransform>();
        cardsRoot.SetParent(runtimeRoot, false);
        SetRect(cardsRoot, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(4f, 48f), new Vector2(-4f, -44f));

        for (int i = 0; i < MaxVisibleCards; i++)
        {
            queueCards[i] = BuildCard(i);
        }

        helperText = CreateText("HelperText", runtimeRoot, 11, TextAnchor.LowerLeft, new Color(0.89f, 0.83f, 0.73f, 0.95f));
        SetRect(helperText.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(6f, 6f), new Vector2(-8f, 36f));

        var eaterRoot = new GameObject("EaterFx", typeof(RectTransform), typeof(Image));
        var eaterRect = eaterRoot.GetComponent<RectTransform>();
        eaterRect.SetParent(runtimeRoot, false);
        SetRect(eaterRect, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-234f, 8f), new Vector2(-8f, 86f));
        var eaterBg = eaterRoot.GetComponent<Image>();
        eaterBg.color = new Color(0.28f, 0.18f, 0.12f, 0.28f);
        eaterPanel = eaterBg;

        eatAvatar = CreateImage("EaterAvatar", eaterRect, customerBaseSprite, Color.white, false);
        SetRect(eatAvatar.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(10f, -32f), new Vector2(70f, 32f));
        eatAvatar.gameObject.SetActive(false);

        var spark = CreateImage("EaterSpark", eaterRect, eaterSparkSprite, new Color(1f, 0.78f, 0.34f, 0.68f), false);
        SetRect(spark.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(52f, -24f), new Vector2(100f, 24f));
        eatSpark = spark;

        eatText = CreateText("EaterText", eaterRect, 12, TextAnchor.MiddleLeft, new Color(0.96f, 0.92f, 0.76f, 1f));
        SetRect(eatText.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(74f, 8f), new Vector2(-8f, -8f));
        eatText.gameObject.SetActive(false);

        LayoutCardsIfNeeded(true);
    }

    private QueueCard BuildCard(int index)
    {
        var card = new QueueCard();
        var cardGo = new GameObject("QueueCard" + index, typeof(RectTransform), typeof(Image));
        card.root = cardGo.GetComponent<RectTransform>();
        card.root.SetParent(cardsRoot, false);

        SetRect(card.root, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(2f, -54f), new Vector2(-2f, -4f));

        card.panel = cardGo.GetComponent<Image>();
        card.panel.color = new Color(0.14f, 0.08f, 0.06f, 0.76f);

        card.avatar = CreateImage("Avatar", card.root, customerBaseSprite, Color.white, false);
        SetRect(card.avatar.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(6f, 6f), new Vector2(50f, -6f));

        card.nameText = CreateText("Name", card.root, 11, TextAnchor.UpperLeft, new Color(0.95f, 0.91f, 0.82f, 1f));
        SetRect(card.nameText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(56f, -20f), new Vector2(132f, -2f));

        card.timerText = CreateText("Timer", card.root, 11, TextAnchor.UpperRight, new Color(1f, 0.76f, 0.62f, 1f));
        SetRect(card.timerText.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-58f, -20f), new Vector2(-8f, -2f));

        card.bubble = CreateImage("Bubble", card.root, bubbleSprite, new Color(0.95f, 0.90f, 0.78f, 0.96f), false);
        SetRect(card.bubble.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(94f, 10f), new Vector2(-8f, -24f));

        card.requestIcon = CreateImage("RequestIcon", card.root, meatGenericSprite, Color.white, false);
        SetRect(card.requestIcon.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(58f, -16f), new Vector2(90f, 16f));

        card.bubbleText = CreateText("BubbleText", card.root, 11, TextAnchor.MiddleCenter, new Color(0.21f, 0.12f, 0.08f, 1f));
        SetRect(card.bubbleText.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(96f, 12f), new Vector2(-14f, -26f));

        card.patienceBack = CreateImage("PatienceBack", card.root, null, new Color(0.22f, 0.12f, 0.10f, 0.92f), false);
        SetRect(card.patienceBack.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(58f, 4f), new Vector2(-8f, 10f));

        card.patienceFill = CreateImage("PatienceFill", card.root, null, new Color(0.58f, 0.84f, 0.42f, 0.96f), false);
        SetRect(card.patienceFill.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(58f, 4f), new Vector2(-8f, 10f));
        card.patienceFill.type = Image.Type.Filled;
        card.patienceFill.fillMethod = Image.FillMethod.Horizontal;
        card.patienceFill.fillOrigin = (int)Image.OriginHorizontal.Left;
        card.patienceFill.fillAmount = 1f;

        card.root.gameObject.SetActive(false);
        return card;
    }

    private void LayoutCardsIfNeeded(bool force)
    {
        if (cardsRoot == null)
        {
            return;
        }

        if (!force)
        {
            layoutPollTimer -= Time.unscaledDeltaTime;
            if (layoutPollTimer > 0f)
            {
                return;
            }
            layoutPollTimer = 0.20f;
        }

        var size = cardsRoot.rect.size;
        if (!force &&
            Mathf.Abs(size.x - lastCardsSize.x) < 0.5f &&
            Mathf.Abs(size.y - lastCardsSize.y) < 0.5f)
        {
            return;
        }

        lastCardsSize = size;
        var height = Mathf.Max(160f, size.y);
        var gap = Mathf.Clamp(height * 0.018f, 2f, 8f);
        var rowHeight = (height - gap * (MaxVisibleCards - 1)) / MaxVisibleCards;
        rowHeight = Mathf.Clamp(rowHeight, 34f, 110f);
        var compact = rowHeight < 72f;

        for (int i = 0; i < queueCards.Length; i++)
        {
            var card = queueCards[i];
            if (card.root == null)
            {
                continue;
            }

            var top = i * (rowHeight + gap);
            SetRect(card.root, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(2f, -top - rowHeight), new Vector2(-2f, -top));

            if (card.nameText != null)
            {
                card.nameText.resizeTextMinSize = compact ? 8 : 10;
                card.nameText.resizeTextMaxSize = compact ? 11 : 13;
            }

            if (card.timerText != null)
            {
                card.timerText.resizeTextMinSize = compact ? 8 : 10;
                card.timerText.resizeTextMaxSize = compact ? 11 : 13;
            }

            if (card.bubbleText != null)
            {
                card.bubbleText.resizeTextMinSize = compact ? 8 : 10;
                card.bubbleText.resizeTextMaxSize = compact ? 10 : 12;
            }
        }
    }

    private void HideLegacy(string name)
    {
        var legacy = FindRect(name);
        if (legacy != null)
        {
            legacy.gameObject.SetActive(false);
        }
    }

    private RectTransform FindRect(string name)
    {
        if (root == null || string.IsNullOrEmpty(name))
        {
            return null;
        }

        var stack = new Stack<Transform>();
        stack.Push(root);
        while (stack.Count > 0)
        {
            var current = stack.Pop();
            if (current.name == name)
            {
                return current as RectTransform;
            }

            for (int i = 0; i < current.childCount; i++)
            {
                stack.Push(current.GetChild(i));
            }
        }

        return null;
    }

    private Button FindButton(string name)
    {
        var rect = FindRect(name);
        return rect != null ? rect.GetComponent<Button>() : null;
    }

    private Sprite ResolveAvatar(string customerName, int fallbackIndex)
    {
        var key = string.IsNullOrEmpty(customerName) ? "guest_" + fallbackIndex : customerName;
        Sprite sprite;
        if (avatarCache.TryGetValue(key, out sprite) && sprite != null)
        {
            return sprite;
        }

        var hash = Mathf.Abs(key.GetHashCode());
        var palette = new Dictionary<char, Color32>
        {
            ['.'] = new Color32(0, 0, 0, 0),
            ['s'] = new Color32((byte)(180 + (hash % 50)), (byte)(135 + (hash % 40)), (byte)(95 + (hash % 35)), 255),
            ['h'] = new Color32((byte)(40 + (hash % 35)), (byte)(30 + (hash % 30)), (byte)(25 + (hash % 25)), 255),
            ['c'] = new Color32((byte)(130 + (hash % 80)), (byte)(60 + (hash % 100)), (byte)(45 + (hash % 80)), 255),
            ['e'] = new Color32(248, 236, 210, 255),
            ['u'] = new Color32(70, 46, 36, 255),
        };

        sprite = BuildPixelSprite("cust_" + Mathf.Abs(hash), new[]
        {
            "................",
            "......hhhh......",
            "....hhhhhhhh....",
            "...hhsssssshh...",
            "..hhsssssssshh..",
            "..hhssesseesshh..",
            "..hhsssssssshh..",
            "...hhsssssshh...",
            "....hhsssshh....",
            "....cccccccc....",
            "...cccuuuuccc...",
            "...cccuuuuccc...",
            "....cccccccc....",
            "................",
            "................",
            "................"
        }, palette);

        avatarCache[key] = sprite;
        return sprite;
    }

    private Sprite ResolveCutSprite(string menuId, string menuName)
    {
        var key = (menuId ?? string.Empty) + "|" + (menuName ?? string.Empty);
        Sprite cached;
        if (cutSpriteCache.TryGetValue(key, out cached) && cached != null)
        {
            return cached;
        }

        var normalized = (menuId ?? string.Empty) + " " + (menuName ?? string.Empty);
        normalized = normalized.ToLowerInvariant();

        Sprite selected;
        if (normalized.Contains("pork") || normalized.Contains("belly") || normalized.Contains("samgyeop"))
        {
            selected = meatBellySprite;
        }
        else if (normalized.Contains("brisket") || normalized.Contains("beef"))
        {
            selected = meatBrisketSprite;
        }
        else if (normalized.Contains("rib") || normalized.Contains("galbi"))
        {
            selected = meatRibSprite;
        }
        else if (normalized.Contains("shrimp") || normalized.Contains("seafood") || normalized.Contains("squid"))
        {
            selected = meatSeafoodSprite;
        }
        else
        {
            selected = meatGenericSprite;
        }

        if (selected == null)
        {
            selected = meatGenericSprite;
        }

        cutSpriteCache[key] = selected;
        return selected;
    }

    private void BuildSprites()
    {
        bubbleSprite = BuildPixelSprite("queue_bubble", new[]
        {
            "................",
            ".##############.",
            "#..............#",
            "#..............#",
            "#..............#",
            "#..............#",
            "#..............#",
            "#..............#",
            "#..............#",
            "#..............#",
            "#.......##.....#",
            "#......####....#",
            ".##############.",
            ".....####.......",
            "......##........",
            "................"
        }, new Dictionary<char, Color32>
        {
            ['.'] = new Color32(0, 0, 0, 0),
            ['#'] = new Color32(242, 228, 195, 255)
        });

        coinSprite = BuildPixelSprite("queue_coin", new[]
        {
            "................",
            ".....yyyyyy.....",
            "....yyyyyyyy....",
            "...yyyggggyyy...",
            "..yyyggggggyyy..",
            "..yyygwwgggyyy..",
            "..yyygwwgggyyy..",
            "..yyygwwgggyyy..",
            "..yyyggggggyyy..",
            "...yyyggggyyy...",
            "....yyyyyyyy....",
            ".....yyyyyy.....",
            "................",
            "................",
            "................",
            "................"
        }, new Dictionary<char, Color32>
        {
            ['.'] = new Color32(0, 0, 0, 0),
            ['y'] = new Color32(246, 202, 84, 255),
            ['g'] = new Color32(228, 162, 45, 255),
            ['w'] = new Color32(255, 236, 162, 255)
        });

        customerBaseSprite = BuildPixelSprite("queue_customer_base", new[]
        {
            "................",
            "......hhhh......",
            "....hhsssshh....",
            "...hhssessshh...",
            "...hhsssssshh...",
            "....hhsssshh....",
            "....cccccccc....",
            "...cccuuuuccc...",
            "...cccuuuuccc...",
            "....cccccccc....",
            "................",
            "................",
            "................",
            "................",
            "................",
            "................"
        }, new Dictionary<char, Color32>
        {
            ['.'] = new Color32(0, 0, 0, 0),
            ['s'] = new Color32(214, 157, 122, 255),
            ['h'] = new Color32(54, 38, 30, 255),
            ['c'] = new Color32(146, 72, 48, 255),
            ['u'] = new Color32(72, 45, 35, 255),
            ['e'] = new Color32(246, 233, 211, 255)
        });

        eaterSparkSprite = BuildPixelSprite("queue_spark", new[]
        {
            "................",
            ".......w........",
            "......www.......",
            ".....wwwww......",
            "....wwwwwww.....",
            ".....wwwww......",
            "......www.......",
            ".......w........",
            "................",
            "................",
            "................",
            "................",
            "................",
            "................",
            "................",
            "................"
        }, new Dictionary<char, Color32>
        {
            ['.'] = new Color32(0, 0, 0, 0),
            ['w'] = new Color32(255, 206, 113, 255)
        });

        meatBellySprite = BuildPixelSprite("queue_cut_belly", new[]
        {
            "................",
            "....rrrrrrrr....",
            "...rrpppppprr...",
            "..rrpppppppprr..",
            "..rrpppppppprr..",
            "..rrppwwpppprr..",
            "..rrppwwpppprr..",
            "..rrpppppppprr..",
            "...rrpppppprr...",
            "....rrrrrrrr....",
            "................",
            "................",
            "................",
            "................",
            "................",
            "................"
        }, new Dictionary<char, Color32>
        {
            ['.'] = new Color32(0, 0, 0, 0),
            ['r'] = new Color32(190, 54, 45, 255),
            ['p'] = new Color32(235, 154, 144, 255),
            ['w'] = new Color32(250, 233, 196, 255)
        });

        meatBrisketSprite = BuildPixelSprite("queue_cut_brisket", new[]
        {
            "................",
            "....bbbbbbbb....",
            "...bbccccccbb...",
            "..bbccccccccbb..",
            "..bbccddddccbb..",
            "..bbccddddccbb..",
            "..bbccddddccbb..",
            "..bbccccccccbb..",
            "...bbccccccbb...",
            "....bbbbbbbb....",
            "................",
            "................",
            "................",
            "................",
            "................",
            "................"
        }, new Dictionary<char, Color32>
        {
            ['.'] = new Color32(0, 0, 0, 0),
            ['b'] = new Color32(122, 66, 45, 255),
            ['c'] = new Color32(168, 98, 64, 255),
            ['d'] = new Color32(100, 58, 40, 255)
        });

        meatRibSprite = BuildPixelSprite("queue_cut_rib", new[]
        {
            "................",
            "......rrrr......",
            "....rrrrrrrr....",
            "...rrrwwwwrrr...",
            "..rrrwwwwwwrrr..",
            "..rrrwwwwwwrrr..",
            "..rrrwwwwwwrrr..",
            "...rrrwwwwrrr...",
            "....rrrrrrrr....",
            "......rrrr......",
            "................",
            "................",
            "................",
            "................",
            "................",
            "................"
        }, new Dictionary<char, Color32>
        {
            ['.'] = new Color32(0, 0, 0, 0),
            ['r'] = new Color32(168, 54, 44, 255),
            ['w'] = new Color32(242, 230, 205, 255)
        });

        meatSeafoodSprite = BuildPixelSprite("queue_cut_seafood", new[]
        {
            "................",
            "......cccc......",
            "....cccyyycc....",
            "...ccyyyyyycc...",
            "..ccyyyyyyyycc..",
            "..ccyyyyyyyycc..",
            "..ccyyyyyyyycc..",
            "...ccyyyyyycc...",
            "....cccyyycc....",
            "......cccc......",
            "................",
            "................",
            "................",
            "................",
            "................",
            "................"
        }, new Dictionary<char, Color32>
        {
            ['.'] = new Color32(0, 0, 0, 0),
            ['c'] = new Color32(106, 72, 48, 255),
            ['y'] = new Color32(244, 188, 92, 255)
        });

        meatGenericSprite = BuildPixelSprite("queue_cut_generic", new[]
        {
            "................",
            "......gggg......",
            "....ggyyyygg....",
            "...ggyyyyyygg...",
            "..ggyyyyyyyygg..",
            "..ggyyyyyyyygg..",
            "..ggyyyyyyyygg..",
            "...ggyyyyyygg...",
            "....ggyyyygg....",
            "......gggg......",
            "................",
            "................",
            "................",
            "................",
            "................",
            "................"
        }, new Dictionary<char, Color32>
        {
            ['.'] = new Color32(0, 0, 0, 0),
            ['g'] = new Color32(156, 96, 58, 255),
            ['y'] = new Color32(230, 156, 84, 255)
        });
    }

    private static string ClipText(string value, int max)
    {
        var raw = string.IsNullOrEmpty(value) ? "Guest" : value.Trim();
        if (raw.Length <= max)
        {
            return raw;
        }
        return raw.Substring(0, Mathf.Max(1, max - 3)) + "...";
    }

    private static Text CreateText(string name, RectTransform parent, int fontSize, TextAnchor anchor, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text), typeof(Shadow));
        var rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);

        var text = go.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = fontSize;
        text.fontStyle = FontStyle.Bold;
        text.color = color;
        text.alignment = anchor;
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = 10;
        text.resizeTextMaxSize = fontSize + 2;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;

        var shadow = go.GetComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.34f);
        shadow.effectDistance = new Vector2(1f, -1f);

        return text;
    }

    private static Image CreateImage(string name, RectTransform parent, Sprite sprite, Color color, bool raycastTarget)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        var rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);

        var image = go.GetComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.raycastTarget = raycastTarget;
        image.preserveAspect = sprite != null;
        return image;
    }

    private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        if (rect == null)
        {
            return;
        }

        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
    }

    private static Sprite BuildPixelSprite(string name, string[] pattern, Dictionary<char, Color32> palette)
    {
        var height = pattern != null ? pattern.Length : 1;
        var width = 1;
        if (pattern != null)
        {
            for (int i = 0; i < pattern.Length; i++)
            {
                width = Mathf.Max(width, pattern[i] != null ? pattern[i].Length : 0);
            }
        }

        var tex = new Texture2D(width, Mathf.Max(1, height), TextureFormat.RGBA32, false);
        tex.name = name;
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;

        for (int y = 0; y < height; y++)
        {
            var row = pattern[height - 1 - y] ?? string.Empty;
            for (int x = 0; x < width; x++)
            {
                var key = x < row.Length ? row[x] : '.';
                Color32 color;
                if (!palette.TryGetValue(key, out color))
                {
                    color = new Color32(0, 0, 0, 0);
                }
                tex.SetPixel(x, y, color);
            }
        }

        tex.Apply(false, false);
        return Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 16f);
    }
}
