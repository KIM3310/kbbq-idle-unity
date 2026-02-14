using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class KBBQAutoSetup
{
    private static readonly Color BgColor = new Color(0.98f, 0.93f, 0.85f);
    private static readonly Color TopBarColor = new Color(0.95f, 0.85f, 0.70f);
    private static readonly Color BottomBarColor = new Color(0.20f, 0.13f, 0.10f);
    private static readonly Color GrillColor = new Color(0.45f, 0.23f, 0.18f);
    private static readonly Color PanelColor = new Color(0.96f, 0.88f, 0.74f);
    private static readonly Color AccentColor = new Color(0.80f, 0.33f, 0.20f);
    private static readonly Color TextDark = new Color(0.23f, 0.16f, 0.12f);
    private static readonly Color TextLight = new Color(0.98f, 0.95f, 0.90f);

    [UnityEditor.MenuItem("KBBQ/Run Auto Setup")]
    public static void Run()
    {
        CreateFolders();
        var catalog = CreateDataAssets();
        CreatePrefabsAndScene(catalog);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void CreateFolders()
    {
        EnsureFolder("Assets", "Scenes");
        EnsureFolder("Assets", "Prefabs");
        EnsureFolder("Assets", "Data");
        EnsureFolder("Assets/Data", "MenuItems");
        EnsureFolder("Assets/Data", "Upgrades");
        EnsureFolder("Assets/Data", "StoreTiers");
        EnsureFolder("Assets/Data", "Customers");
        EnsureFolder("Assets/Data", "Config");
    }

    private static GameDataCatalog CreateDataAssets()
    {
        var menuItems = new List<MenuItem>
        {
            CreateMenuItemAsset("Assets/Data/MenuItems/PorkBelly.asset", "pork_belly", "Pork Belly", 1, 1.2f, 1.0f),
            CreateMenuItemAsset("Assets/Data/MenuItems/PorkShoulder.asset", "pork_shoulder", "Pork Shoulder", 2, 1.5f, 1.05f),
            CreateMenuItemAsset("Assets/Data/MenuItems/PorkRib.asset", "rib", "Pork Rib", 3, 2.0f, 1.1f),
            CreateMenuItemAsset("Assets/Data/MenuItems/SpicyPork.asset", "spicy_pork", "Spicy Pork", 4, 2.6f, 1.15f),
            CreateMenuItemAsset("Assets/Data/MenuItems/KimchiStew.asset", "kimchi_stew", "Kimchi Stew", 5, 3.0f, 1.18f),
            CreateMenuItemAsset("Assets/Data/MenuItems/BeefBrisket.asset", "beef_brisket", "Beef Brisket", 6, 3.8f, 1.2f),
            CreateMenuItemAsset("Assets/Data/MenuItems/PremiumBeef.asset", "premium_beef", "Premium Beef", 7, 4.5f, 1.22f),
            CreateMenuItemAsset("Assets/Data/MenuItems/SignatureSauce.asset", "signature_sauce", "Signature Sauce", 8, 5.5f, 1.25f),
            CreateMenuItemAsset("Assets/Data/MenuItems/ColdNoodle.asset", "cold_noodle", "Cold Noodle", 9, 6.5f, 1.28f),
            CreateMenuItemAsset("Assets/Data/MenuItems/SeafoodSet.asset", "seafood_set", "Seafood Set", 10, 8.0f, 1.3f),
            CreateMenuItemAsset("Assets/Data/MenuItems/MushroomPlatter.asset", "mushroom_platter", "Mushroom Platter", 11, 9.5f, 1.32f),
            CreateMenuItemAsset("Assets/Data/MenuItems/RiceSet.asset", "rice_set", "Rice Set", 12, 11.0f, 1.35f),
            CreateMenuItemAsset("Assets/Data/MenuItems/Soju.asset", "soju", "Soju", 13, 12.5f, 1.38f),
            CreateMenuItemAsset("Assets/Data/MenuItems/Makgeolli.asset", "makgeolli", "Makgeolli", 14, 14.0f, 1.4f),
            CreateMenuItemAsset("Assets/Data/MenuItems/Bingsu.asset", "bingsu", "Bingsu", 15, 16.0f, 1.45f)
        };

        var upgrades = new List<UpgradeData>
        {
            CreateUpgradeAsset("Assets/Data/Upgrades/GrillUpgrade.asset", "grill_upgrade", "Grill Upgrade", "income", "", 10f, 1.3f, 0.06f),
            CreateUpgradeAsset("Assets/Data/Upgrades/Ventilation.asset", "ventilation", "Ventilation", "income", "", 25f, 1.28f, 0.05f),
            CreateUpgradeAsset("Assets/Data/Upgrades/SizzleMaster.asset", "sizzle_master", "Sizzle Master", "sizzle", "", 15f, 1.25f, 0.03f),
            CreateUpgradeAsset("Assets/Data/Upgrades/StaffTraining.asset", "staff_training", "Staff Training", "staff", "", 18f, 1.26f, 0.04f),
            CreateUpgradeAsset("Assets/Data/Upgrades/ServiceFlow.asset", "service_flow", "Service Flow", "service", "", 22f, 1.27f, 0.05f),
            CreateUpgradeAsset("Assets/Data/Upgrades/PorkBellyRecipe.asset", "pork_belly_recipe", "Pork Belly Recipe", "menu", "pork_belly", 12f, 1.32f, 0.08f),
            CreateUpgradeAsset("Assets/Data/Upgrades/BeefBrisketRecipe.asset", "beef_brisket_recipe", "Beef Brisket Recipe", "menu", "beef_brisket", 18f, 1.33f, 0.08f),
            CreateUpgradeAsset("Assets/Data/Upgrades/PremiumBeefRecipe.asset", "premium_beef_recipe", "Premium Beef Recipe", "menu", "premium_beef", 30f, 1.35f, 0.09f),
            CreateUpgradeAsset("Assets/Data/Upgrades/SignatureSauceRecipe.asset", "signature_sauce_recipe", "Signature Sauce Recipe", "menu", "signature_sauce", 35f, 1.36f, 0.1f)
        };

        var storeTiers = new List<StoreTier>
        {
            CreateStoreTierAsset("Assets/Data/StoreTiers/Alley.asset", "alley", "Alley", 1, 1.0f),
            CreateStoreTierAsset("Assets/Data/StoreTiers/Hongdae.asset", "hongdae", "Hongdae", 4, 1.3f),
            CreateStoreTierAsset("Assets/Data/StoreTiers/Gangnam.asset", "gangnam", "Gangnam", 7, 1.6f),
            CreateStoreTierAsset("Assets/Data/StoreTiers/Hanok.asset", "hanok", "Hanok", 10, 1.95f),
            CreateStoreTierAsset("Assets/Data/StoreTiers/Global.asset", "global", "Global", 14, 2.4f)
        };

        var customers = new List<CustomerType>
        {
            CreateCustomerAsset("Assets/Data/Customers/Local.asset", "local", "Local", 10f, 1.0f),
            CreateCustomerAsset("Assets/Data/Customers/Tourist.asset", "tourist", "Tourist", 12f, 1.1f),
            CreateCustomerAsset("Assets/Data/Customers/Foodie.asset", "foodie", "Foodie", 8f, 1.2f)
        };

        var apiConfig = CreateApiConfig("Assets/Data/Config/ApiConfig.asset");
        var economyTuning = CreateEconomyTuning("Assets/Data/Config/EconomyTuning.asset");
        var monetizationConfig = CreateMonetizationConfig("Assets/Data/Config/MonetizationConfig.asset");

        var catalogPath = "Assets/Data/Config/GameDataCatalog.asset";
        var catalog = AssetDatabase.LoadAssetAtPath<GameDataCatalog>(catalogPath);
        if (catalog == null)
        {
            catalog = ScriptableObject.CreateInstance<GameDataCatalog>();
            AssetDatabase.CreateAsset(catalog, catalogPath);
        }

        catalog.menuItems = menuItems;
        catalog.upgrades = upgrades;
        catalog.storeTiers = storeTiers;
        catalog.customerTypes = customers;
        catalog.apiConfig = apiConfig;
        catalog.economyTuning = economyTuning;
        catalog.monetizationConfig = monetizationConfig;
        EditorUtility.SetDirty(catalog);
        return catalog;
    }

    private static void CreatePrefabsAndScene(GameDataCatalog catalog)
    {
        var dailyMissionPrefab = CreateDailyMissionPrefab();
        var prestigePrefab = CreatePrestigePrefab();

        var uiRoot = CreateUIRoot(dailyMissionPrefab, prestigePrefab);
        var uiPrefabPath = "Assets/Prefabs/UIRoot.prefab";
        var uiPrefab = PrefabUtility.SaveAsPrefabAsset(uiRoot, uiPrefabPath);
        Object.DestroyImmediate(uiRoot);

        var gameRoot = CreateGameRoot(catalog);
        var gamePrefabPath = "Assets/Prefabs/GameRoot.prefab";
        var gamePrefab = PrefabUtility.SaveAsPrefabAsset(gameRoot, gamePrefabPath);
        Object.DestroyImmediate(gameRoot);

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var gameInstance = (GameObject)PrefabUtility.InstantiatePrefab(gamePrefab);
        var uiInstance = (GameObject)PrefabUtility.InstantiatePrefab(uiPrefab);
        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

        var sceneManager = gameInstance != null ? gameInstance.GetComponentInChildren<GameManager>() : null;
        var sceneUi = uiInstance != null ? uiInstance.GetComponentInChildren<UIController>() : null;
        if (sceneManager != null && sceneUi != null)
        {
            var managerSerialized = new SerializedObject(sceneManager);
            managerSerialized.FindProperty("uiController").objectReferenceValue = sceneUi;
            managerSerialized.ApplyModifiedPropertiesWithoutUndo();
        }
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Main.unity");
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene("Assets/Scenes/Main.unity", true) };
    }

    private static GameObject CreateUIRoot(GameObject dailyMissionPrefab, GameObject prestigePrefab)
    {
        var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);

        var uiController = canvasGo.AddComponent<UIController>();
        var resources = new DefaultControls.Resources();

        var background = CreatePanel("Background", canvasGo.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, BgColor);
        var topBar = CreatePanel("TopBar", canvasGo.transform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0, 200), new Vector2(0, -10), TopBarColor);
        var bottomBar = CreatePanel("BottomBar", canvasGo.transform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0, 220), new Vector2(0, 10), BottomBarColor);
        var grillPanel = CreatePanel("GrillPanel", canvasGo.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(680, 680), new Vector2(0, 60), GrillColor);
        var queuePanel = CreatePanel("QueuePanel", canvasGo.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(260, 640), new Vector2(150, 40), PanelColor);
        var upgradesPanel = CreatePanel("UpgradesPanel", canvasGo.transform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(260, 640), new Vector2(-150, 40), PanelColor);

        var titleText = CreateText(resources, "TitleText", topBar.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -28), new Vector2(600, 50), 30, TextDark, TextAnchor.MiddleCenter);
        titleText.GetComponent<Text>().text = "K-BBQ Idle Tycoon";

        var currencyText = CreateText(resources, "CurrencyText", topBar.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(140, -20), new Vector2(280, 40), 24, TextDark, TextAnchor.MiddleLeft);
        var incomeText = CreateText(resources, "IncomeText", topBar.transform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-140, -20), new Vector2(280, 40), 24, TextDark, TextAnchor.MiddleRight);
        var storeTierText = CreateText(resources, "StoreTierText", topBar.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -60), new Vector2(260, 32), 22, TextDark, TextAnchor.MiddleCenter);
        var prestigeText = CreateText(resources, "PrestigeText", topBar.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 26), new Vector2(260, 28), 20, TextDark, TextAnchor.MiddleCenter);
        var loginRewardText = CreateText(resources, "LoginRewardText", topBar.transform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-140, 26), new Vector2(260, 28), 18, TextDark, TextAnchor.MiddleRight);
        var dailyText = CreateText(resources, "DailyMissionsText", topBar.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(140, 26), new Vector2(260, 28), 18, TextDark, TextAnchor.MiddleLeft);

        var debugToggleGo = DefaultControls.CreateButton(resources);
        debugToggleGo.name = "DebugToggleButton";
        debugToggleGo.transform.SetParent(topBar.transform, false);
        var debugToggleRect = debugToggleGo.GetComponent<RectTransform>();
        debugToggleRect.anchorMin = new Vector2(1f, 1f);
        debugToggleRect.anchorMax = new Vector2(1f, 1f);
        debugToggleRect.anchoredPosition = new Vector2(-50, -28);
        debugToggleRect.sizeDelta = new Vector2(60, 28);
        var debugToggleText = debugToggleGo.GetComponentInChildren<Text>();
        if (debugToggleText != null)
        {
            debugToggleText.text = "DBG";
            debugToggleText.color = TextLight;
            debugToggleText.fontSize = 14;
        }
        var debugToggleImage = debugToggleGo.GetComponent<Image>();
        if (debugToggleImage != null)
        {
            debugToggleImage.color = new Color(0.25f, 0.16f, 0.12f);
        }
        var debugToggleScript = debugToggleGo.AddComponent<DebugToggleButton>();
        var debugToggleButton = debugToggleGo.GetComponent<Button>();
        UnityEventTools.AddPersistentListener(debugToggleButton.onClick, debugToggleScript.ToggleDebug);

        var debugIndicator = CreateText(resources, "DebugIndicator", topBar.transform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-130, -28), new Vector2(70, 24), 12, TextLight, TextAnchor.MiddleRight);
        debugIndicator.GetComponent<Text>().text = "DBG OFF";
        debugIndicator.SetActive(false);

        var leaderboardButtonGo = DefaultControls.CreateButton(resources);
        leaderboardButtonGo.name = "LeaderboardButton";
        leaderboardButtonGo.transform.SetParent(topBar.transform, false);
        var leaderboardRect = leaderboardButtonGo.GetComponent<RectTransform>();
        leaderboardRect.anchorMin = new Vector2(1f, 1f);
        leaderboardRect.anchorMax = new Vector2(1f, 1f);
        leaderboardRect.anchoredPosition = new Vector2(-190, -28);
        leaderboardRect.sizeDelta = new Vector2(60, 28);
        var leaderboardText = leaderboardButtonGo.GetComponentInChildren<Text>();
        if (leaderboardText != null)
        {
            leaderboardText.text = "LB";
            leaderboardText.color = TextLight;
            leaderboardText.fontSize = 14;
        }
        var leaderboardImage = leaderboardButtonGo.GetComponent<Image>();
        if (leaderboardImage != null)
        {
            leaderboardImage.color = new Color(0.30f, 0.18f, 0.14f);
        }
        var leaderboardButton = leaderboardButtonGo.GetComponent<Button>();

        var shopButtonGo = DefaultControls.CreateButton(resources);
        shopButtonGo.name = "ShopButton";
        shopButtonGo.transform.SetParent(topBar.transform, false);
        var shopRect = shopButtonGo.GetComponent<RectTransform>();
        shopRect.anchorMin = new Vector2(1f, 1f);
        shopRect.anchorMax = new Vector2(1f, 1f);
        shopRect.anchoredPosition = new Vector2(-250, -28);
        shopRect.sizeDelta = new Vector2(60, 28);
        var shopText = shopButtonGo.GetComponentInChildren<Text>();
        if (shopText != null)
        {
            shopText.text = "SHOP";
            shopText.color = TextLight;
            shopText.fontSize = 12;
        }
        var shopImage = shopButtonGo.GetComponent<Image>();
        if (shopImage != null)
        {
            shopImage.color = new Color(0.30f, 0.18f, 0.14f);
        }
        var shopButton = shopButtonGo.GetComponent<Button>();

        var grillLabel = CreateText(resources, "GrillLabel", grillPanel.transform, new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.65f), Vector2.zero, new Vector2(300, 60), 26, TextLight, TextAnchor.MiddleCenter);
        grillLabel.GetComponent<Text>().text = "GRILL AREA";
        var grillHint = CreateText(resources, "GrillHint", grillPanel.transform, new Vector2(0.5f, 0.4f), new Vector2(0.5f, 0.4f), Vector2.zero, new Vector2(320, 40), 18, TextLight, TextAnchor.MiddleCenter);
        grillHint.GetComponent<Text>().text = "Tap to sizzle";
        var comboText = CreateText(resources, "ComboText", grillPanel.transform, new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.2f), new Vector2(0, 10), new Vector2(300, 30), 16, TextLight, TextAnchor.MiddleCenter);
        comboText.GetComponent<Text>().text = "Serve fast to build combo";
        var comboSliderGo = DefaultControls.CreateSlider(resources);
        comboSliderGo.name = "ComboSlider";
        comboSliderGo.transform.SetParent(grillPanel.transform, false);
        var comboSliderRect = comboSliderGo.GetComponent<RectTransform>();
        comboSliderRect.anchorMin = new Vector2(0.5f, 0.2f);
        comboSliderRect.anchorMax = new Vector2(0.5f, 0.2f);
        comboSliderRect.anchoredPosition = new Vector2(0, -16);
        comboSliderRect.sizeDelta = new Vector2(260, 16);
        var comboSlider = comboSliderGo.GetComponent<Slider>();
        comboSlider.minValue = 0f;
        comboSlider.maxValue = 1f;
        comboSlider.value = 0f;
        comboSlider.gameObject.SetActive(false);
        var comboFill = comboSliderGo.transform.Find("Fill Area/Fill")?.GetComponent<Image>();
        if (comboFill != null)
        {
            comboFill.color = AccentColor;
        }
        var comboBg = comboSliderGo.transform.Find("Background")?.GetComponent<Image>();
        if (comboBg != null)
        {
            comboBg.color = new Color(0.12f, 0.08f, 0.06f, 0.6f);
        }

        var queueTitle = CreateText(resources, "QueueTitle", queuePanel.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -24), new Vector2(200, 32), 20, TextDark, TextAnchor.MiddleCenter);
        queueTitle.GetComponent<Text>().text = "QUEUE";
        var queueHint = CreateText(resources, "QueueHint", queuePanel.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -60), new Vector2(220, 28), 16, TextDark, TextAnchor.MiddleCenter);
        queueHint.GetComponent<Text>().text = "Customers waiting...";
        var queueList = CreateText(resources, "QueueList", queuePanel.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -30), new Vector2(220, 440), 16, TextDark, TextAnchor.UpperLeft);
        var queueText = queueList.GetComponent<Text>();
        if (queueText != null)
        {
            queueText.text = "No customers yet.";
            queueText.horizontalOverflow = HorizontalWrapMode.Wrap;
            queueText.verticalOverflow = VerticalWrapMode.Overflow;
        }
        var queueMetrics = CreateText(resources, "QueueMetrics", queuePanel.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 140), new Vector2(220, 50), 14, TextDark, TextAnchor.MiddleCenter);
        queueMetrics.GetComponent<Text>().text = "Avg wait 0s\nServed/min 0";
        var queueControlView = queuePanel.AddComponent<QueueControlView>();
        var serveButtonGo = DefaultControls.CreateButton(resources);
        serveButtonGo.name = "ServeButton";
        serveButtonGo.transform.SetParent(queuePanel.transform, false);
        var serveRect = serveButtonGo.GetComponent<RectTransform>();
        serveRect.anchorMin = new Vector2(0.5f, 0f);
        serveRect.anchorMax = new Vector2(0.5f, 0f);
        serveRect.anchoredPosition = new Vector2(0, 90);
        serveRect.sizeDelta = new Vector2(200, 44);
        var serveText = serveButtonGo.GetComponentInChildren<Text>();
        if (serveText != null)
        {
            serveText.text = "Serve Now";
            serveText.color = TextLight;
        }
        var serveImage = serveButtonGo.GetComponent<Image>();
        if (serveImage != null)
        {
            serveImage.color = AccentColor;
        }
        var rushButtonGo = DefaultControls.CreateButton(resources);
        rushButtonGo.name = "RushButton";
        rushButtonGo.transform.SetParent(queuePanel.transform, false);
        var rushRect = rushButtonGo.GetComponent<RectTransform>();
        rushRect.anchorMin = new Vector2(0.5f, 0f);
        rushRect.anchorMax = new Vector2(0.5f, 0f);
        rushRect.anchoredPosition = new Vector2(0, 35);
        rushRect.sizeDelta = new Vector2(200, 44);
        var rushText = rushButtonGo.GetComponentInChildren<Text>();
        if (rushText != null)
        {
            rushText.text = "Rush Service";
            rushText.color = TextLight;
        }
        var rushImage = rushButtonGo.GetComponent<Image>();
        if (rushImage != null)
        {
            rushImage.color = new Color(0.65f, 0.22f, 0.16f);
        }

        var upgradesTitle = CreateText(resources, "UpgradesTitle", upgradesPanel.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -24), new Vector2(200, 32), 20, TextDark, TextAnchor.MiddleCenter);
        upgradesTitle.GetComponent<Text>().text = "UPGRADES";
        var upgradesHint = CreateText(resources, "UpgradesHint", upgradesPanel.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -60), new Vector2(220, 28), 16, TextDark, TextAnchor.MiddleCenter);
        upgradesHint.GetComponent<Text>().text = "Upgrade grill & staff";
        var upgradesScroll = DefaultControls.CreateScrollView(resources);
        upgradesScroll.name = "UpgradesScroll";
        upgradesScroll.transform.SetParent(upgradesPanel.transform, false);
        var upgradesScrollRect = upgradesScroll.GetComponent<ScrollRect>();
        upgradesScrollRect.horizontal = false;
        upgradesScrollRect.vertical = true;
        var scrollRectTransform = upgradesScroll.GetComponent<RectTransform>();
        scrollRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        scrollRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        scrollRectTransform.anchoredPosition = new Vector2(0, -30);
        scrollRectTransform.sizeDelta = new Vector2(230, 460);

        var viewport = upgradesScroll.transform.Find("Viewport");
        if (viewport != null)
        {
            var viewportImage = viewport.GetComponent<Image>();
            if (viewportImage != null)
            {
                viewportImage.color = new Color(0.97f, 0.92f, 0.84f, 0.4f);
            }
        }

        var content = upgradesScrollRect.content;
        if (content != null)
        {
            var layout = content.gameObject.GetComponent<VerticalLayoutGroup>();
            if (layout == null)
            {
                layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            }
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;
            layout.spacing = 8f;
            layout.padding = new RectOffset(6, 6, 8, 8);

            var fitter = content.gameObject.GetComponent<ContentSizeFitter>();
            if (fitter == null)
            {
                fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            }
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        }

        var rowTemplate = DefaultControls.CreateButton(resources);
        rowTemplate.name = "UpgradeRowTemplate";
        rowTemplate.transform.SetParent(content, false);
        var rowRect = rowTemplate.GetComponent<RectTransform>();
        rowRect.sizeDelta = new Vector2(210, 70);
        var rowImage = rowTemplate.GetComponent<Image>();
        if (rowImage != null)
        {
            rowImage.color = AccentColor;
        }
        var rowText = rowTemplate.GetComponentInChildren<Text>();
        if (rowText != null)
        {
            rowText.text = "Upgrade";
            rowText.fontSize = 14;
            rowText.alignment = TextAnchor.MiddleCenter;
            rowText.color = TextLight;
        }
        var rowView = rowTemplate.AddComponent<UpgradeRowView>();
        rowTemplate.SetActive(false);

        var upgradeListView = upgradesPanel.AddComponent<UpgradeListView>();

        var sliderGo = DefaultControls.CreateSlider(resources);
        sliderGo.name = "SatisfactionSlider";
        sliderGo.transform.SetParent(topBar.transform, false);
        var sliderRect = sliderGo.GetComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0.5f, 0f);
        sliderRect.anchorMax = new Vector2(0.5f, 0f);
        sliderRect.anchoredPosition = new Vector2(0, 60);
        sliderRect.sizeDelta = new Vector2(360, 20);
        var slider = sliderGo.GetComponent<Slider>();
        slider.minValue = 0;
        slider.maxValue = 1;
        slider.value = 0.75f;
        var sliderBg = sliderGo.transform.Find("Background")?.GetComponent<Image>();
        if (sliderBg != null)
        {
            sliderBg.color = new Color(0.85f, 0.73f, 0.60f);
        }
        var sliderFill = sliderGo.transform.Find("Fill Area/Fill")?.GetComponent<Image>();
        if (sliderFill != null)
        {
            sliderFill.color = AccentColor;
        }

        var bestButtonGo = DefaultControls.CreateButton(resources);
        bestButtonGo.name = "BestUpgradeButton";
        bestButtonGo.transform.SetParent(bottomBar.transform, false);
        var bestButton = bestButtonGo.GetComponent<Button>();
        var bestText = bestButtonGo.GetComponentInChildren<Text>();
        if (bestText != null)
        {
            bestText.text = "Buy Best";
            bestText.color = TextLight;
        }
        var bestImage = bestButtonGo.GetComponent<Image>();
        if (bestImage != null)
        {
            bestImage.color = new Color(0.62f, 0.26f, 0.18f);
        }
        var bestRect = bestButtonGo.GetComponent<RectTransform>();
        bestRect.anchorMin = new Vector2(0.5f, 0.5f);
        bestRect.anchorMax = new Vector2(0.5f, 0.5f);
        bestRect.anchoredPosition = new Vector2(-240, 18);
        bestRect.sizeDelta = new Vector2(220, 70);
        var bestScript = bestButtonGo.AddComponent<BuyBestUpgradeButton>();
        UnityEventTools.AddPersistentListener(bestButton.onClick, bestScript.TriggerBuyBest);

        var boostButtonGo = DefaultControls.CreateButton(resources);
        boostButtonGo.name = "BoostButton";
        boostButtonGo.transform.SetParent(bottomBar.transform, false);
        var boostButton = boostButtonGo.GetComponent<Button>();
        var boostText = boostButtonGo.GetComponentInChildren<Text>();
        if (boostText != null)
        {
            boostText.text = "Sizzle Boost";
            boostText.color = TextLight;
        }
        var boostImage = boostButtonGo.GetComponent<Image>();
        if (boostImage != null)
        {
            boostImage.color = AccentColor;
        }
        var boostRect = boostButtonGo.GetComponent<RectTransform>();
        boostRect.anchorMin = new Vector2(0.5f, 0.5f);
        boostRect.anchorMax = new Vector2(0.5f, 0.5f);
        boostRect.anchoredPosition = new Vector2(0, 18);
        boostRect.sizeDelta = new Vector2(260, 70);
        var boostScript = boostButtonGo.AddComponent<BoostButton>();
        UnityEventTools.AddPersistentListener(boostButton.onClick, boostScript.TriggerBoost);

        var debugPanel = CreatePanel("DebugPanel", canvasGo.transform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(260, 230), new Vector2(-160, 260), PanelColor);
        var debugTitle = CreateText(resources, "DebugTitle", debugPanel.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -20), new Vector2(200, 24), 16, TextDark, TextAnchor.MiddleCenter);
        debugTitle.GetComponent<Text>().text = "DEBUG";
        var debugView = debugPanel.AddComponent<DebugPanelView>();

        var presetLabel = CreateText(resources, "PresetLabel", debugPanel.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(70, -45), new Vector2(120, 20), 14, TextDark, TextAnchor.MiddleLeft);
        presetLabel.GetComponent<Text>().text = "Preset";
        var presetDropdownGo = DefaultControls.CreateDropdown(resources);
        presetDropdownGo.name = "PresetDropdown";
        presetDropdownGo.transform.SetParent(debugPanel.transform, false);
        var presetRect = presetDropdownGo.GetComponent<RectTransform>();
        presetRect.anchorMin = new Vector2(0.5f, 1f);
        presetRect.anchorMax = new Vector2(0.5f, 1f);
        presetRect.anchoredPosition = new Vector2(0, -70);
        presetRect.sizeDelta = new Vector2(210, 26);
        var presetDropdown = presetDropdownGo.GetComponent<Dropdown>();

        var spawnLabel = CreateText(resources, "SpawnLabel", debugPanel.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(70, -95), new Vector2(120, 20), 14, TextDark, TextAnchor.MiddleLeft);
        spawnLabel.GetComponent<Text>().text = "Spawn Rate";
        var spawnValue = CreateText(resources, "SpawnValue", debugPanel.transform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-40, -95), new Vector2(60, 20), 14, TextDark, TextAnchor.MiddleRight);
        spawnValue.GetComponent<Text>().text = "x1.0";
        var spawnSliderGo = DefaultControls.CreateSlider(resources);
        spawnSliderGo.name = "SpawnRateSlider";
        spawnSliderGo.transform.SetParent(debugPanel.transform, false);
        var spawnSliderRect = spawnSliderGo.GetComponent<RectTransform>();
        spawnSliderRect.anchorMin = new Vector2(0.5f, 1f);
        spawnSliderRect.anchorMax = new Vector2(0.5f, 1f);
        spawnSliderRect.anchoredPosition = new Vector2(0, -125);
        spawnSliderRect.sizeDelta = new Vector2(220, 18);
        var spawnSlider = spawnSliderGo.GetComponent<Slider>();
        spawnSlider.minValue = 0.5f;
        spawnSlider.maxValue = 2.5f;
        spawnSlider.value = 1f;
        var spawnFill = spawnSliderGo.transform.Find("Fill Area/Fill")?.GetComponent<Image>();
        if (spawnFill != null)
        {
            spawnFill.color = AccentColor;
        }

        var serviceLabel = CreateText(resources, "ServiceLabel", debugPanel.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(70, -155), new Vector2(120, 20), 14, TextDark, TextAnchor.MiddleLeft);
        serviceLabel.GetComponent<Text>().text = "Service Rate";
        var serviceValue = CreateText(resources, "ServiceValue", debugPanel.transform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-40, -155), new Vector2(60, 20), 14, TextDark, TextAnchor.MiddleRight);
        serviceValue.GetComponent<Text>().text = "x1.0";
        var serviceSliderGo = DefaultControls.CreateSlider(resources);
        serviceSliderGo.name = "ServiceRateSlider";
        serviceSliderGo.transform.SetParent(debugPanel.transform, false);
        var serviceSliderRect = serviceSliderGo.GetComponent<RectTransform>();
        serviceSliderRect.anchorMin = new Vector2(0.5f, 1f);
        serviceSliderRect.anchorMax = new Vector2(0.5f, 1f);
        serviceSliderRect.anchoredPosition = new Vector2(0, -185);
        serviceSliderRect.sizeDelta = new Vector2(220, 18);
        var serviceSlider = serviceSliderGo.GetComponent<Slider>();
        serviceSlider.minValue = 0.5f;
        serviceSlider.maxValue = 2.5f;
        serviceSlider.value = 1f;
        var serviceFill = serviceSliderGo.transform.Find("Fill Area/Fill")?.GetComponent<Image>();
        if (serviceFill != null)
        {
            serviceFill.color = AccentColor;
        }

        var resetButtonGo = DefaultControls.CreateButton(resources);
        resetButtonGo.name = "ResetRatesButton";
        resetButtonGo.transform.SetParent(debugPanel.transform, false);
        var resetRect = resetButtonGo.GetComponent<RectTransform>();
        resetRect.anchorMin = new Vector2(0.5f, 0f);
        resetRect.anchorMax = new Vector2(0.5f, 0f);
        resetRect.anchoredPosition = new Vector2(0, 20);
        resetRect.sizeDelta = new Vector2(200, 28);
        var resetText = resetButtonGo.GetComponentInChildren<Text>();
        if (resetText != null)
        {
            resetText.text = "Reset Rates";
            resetText.color = TextLight;
            resetText.fontSize = 14;
        }
        var resetImage = resetButtonGo.GetComponent<Image>();
        if (resetImage != null)
        {
            resetImage.color = new Color(0.55f, 0.22f, 0.16f);
        }

        var perfPanel = CreatePanel("PerfOverlay", canvasGo.transform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(200, 110), new Vector2(-120, -220), new Color(0.10f, 0.08f, 0.07f, 0.65f));
        var perfText = CreateText(resources, "PerfText", perfPanel.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(180, 90), 14, TextLight, TextAnchor.UpperLeft);
        perfText.GetComponent<Text>().text = "FPS 60\nQueue 0\nServed/min 0\nAvg wait 0.0s";
        var perfView = perfPanel.AddComponent<PerfOverlayView>();

        var leaderboardPanel = CreatePanel("LeaderboardPanel", canvasGo.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(700, 860), Vector2.zero, new Color(0.97f, 0.92f, 0.84f, 0.98f));
        var leaderboardView = leaderboardPanel.AddComponent<LeaderboardView>();
        var leaderboardTitle = CreateText(resources, "LeaderboardTitle", leaderboardPanel.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -30), new Vector2(400, 40), 24, TextDark, TextAnchor.MiddleCenter);
        leaderboardTitle.GetComponent<Text>().text = "LEADERBOARD";
        var leaderboardList = CreateText(resources, "LeaderboardList", leaderboardPanel.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -40), new Vector2(520, 520), 18, TextDark, TextAnchor.UpperLeft);
        var playerRankText = CreateText(resources, "PlayerRank", leaderboardPanel.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 90), new Vector2(520, 30), 16, TextDark, TextAnchor.MiddleCenter);
        var friendsText = CreateText(resources, "FriendsList", leaderboardPanel.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 40), new Vector2(520, 40), 14, TextDark, TextAnchor.MiddleCenter);

        var leaderboardRefreshGo = DefaultControls.CreateButton(resources);
        leaderboardRefreshGo.name = "LeaderboardRefresh";
        leaderboardRefreshGo.transform.SetParent(leaderboardPanel.transform, false);
        var leaderboardRefreshRect = leaderboardRefreshGo.GetComponent<RectTransform>();
        leaderboardRefreshRect.anchorMin = new Vector2(1f, 1f);
        leaderboardRefreshRect.anchorMax = new Vector2(1f, 1f);
        leaderboardRefreshRect.anchoredPosition = new Vector2(-70, -30);
        leaderboardRefreshRect.sizeDelta = new Vector2(90, 32);
        var leaderboardRefreshText = leaderboardRefreshGo.GetComponentInChildren<Text>();
        if (leaderboardRefreshText != null)
        {
            leaderboardRefreshText.text = "Refresh";
            leaderboardRefreshText.color = TextLight;
        }
        var leaderboardRefreshImage = leaderboardRefreshGo.GetComponent<Image>();
        if (leaderboardRefreshImage != null)
        {
            leaderboardRefreshImage.color = AccentColor;
        }

        var leaderboardCloseGo = DefaultControls.CreateButton(resources);
        leaderboardCloseGo.name = "LeaderboardClose";
        leaderboardCloseGo.transform.SetParent(leaderboardPanel.transform, false);
        var leaderboardCloseRect = leaderboardCloseGo.GetComponent<RectTransform>();
        leaderboardCloseRect.anchorMin = new Vector2(0f, 1f);
        leaderboardCloseRect.anchorMax = new Vector2(0f, 1f);
        leaderboardCloseRect.anchoredPosition = new Vector2(70, -30);
        leaderboardCloseRect.sizeDelta = new Vector2(90, 32);
        var leaderboardCloseText = leaderboardCloseGo.GetComponentInChildren<Text>();
        if (leaderboardCloseText != null)
        {
            leaderboardCloseText.text = "Close";
            leaderboardCloseText.color = TextLight;
        }
        var leaderboardCloseImage = leaderboardCloseGo.GetComponent<Image>();
        if (leaderboardCloseImage != null)
        {
            leaderboardCloseImage.color = new Color(0.55f, 0.22f, 0.16f);
        }
        leaderboardPanel.SetActive(false);

        var monetizationPanel = CreatePanel("MonetizationPanel", canvasGo.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(700, 720), Vector2.zero, new Color(0.97f, 0.92f, 0.84f, 0.98f));
        var monetizationView = monetizationPanel.AddComponent<MonetizationView>();
        var shopTitle = CreateText(resources, "ShopTitle", monetizationPanel.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -30), new Vector2(400, 40), 24, TextDark, TextAnchor.MiddleCenter);
        shopTitle.GetComponent<Text>().text = "SHOP";
        var shopStatus = CreateText(resources, "ShopStatus", monetizationPanel.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -80), new Vector2(500, 30), 16, TextDark, TextAnchor.MiddleCenter);
        shopStatus.GetComponent<Text>().text = "광고/패키지 시뮬레이션";

        var rewardedGo = DefaultControls.CreateButton(resources);
        rewardedGo.name = "RewardedButton";
        rewardedGo.transform.SetParent(monetizationPanel.transform, false);
        var rewardedRect = rewardedGo.GetComponent<RectTransform>();
        rewardedRect.anchorMin = new Vector2(0.5f, 0.75f);
        rewardedRect.anchorMax = new Vector2(0.5f, 0.75f);
        rewardedRect.anchoredPosition = new Vector2(0, 20);
        rewardedRect.sizeDelta = new Vector2(260, 50);
        var rewardedText = rewardedGo.GetComponentInChildren<Text>();
        if (rewardedText != null)
        {
            rewardedText.text = "보상형 광고 (x2)";
            rewardedText.color = TextLight;
        }
        var rewardedImage = rewardedGo.GetComponent<Image>();
        if (rewardedImage != null)
        {
            rewardedImage.color = AccentColor;
        }

        var interstitialGo = DefaultControls.CreateButton(resources);
        interstitialGo.name = "InterstitialButton";
        interstitialGo.transform.SetParent(monetizationPanel.transform, false);
        var interstitialRect = interstitialGo.GetComponent<RectTransform>();
        interstitialRect.anchorMin = new Vector2(0.5f, 0.65f);
        interstitialRect.anchorMax = new Vector2(0.5f, 0.65f);
        interstitialRect.anchoredPosition = new Vector2(0, 20);
        interstitialRect.sizeDelta = new Vector2(260, 50);
        var interstitialText = interstitialGo.GetComponentInChildren<Text>();
        if (interstitialText != null)
        {
            interstitialText.text = "전면 광고 (+보상)";
            interstitialText.color = TextLight;
        }
        var interstitialImage = interstitialGo.GetComponent<Image>();
        if (interstitialImage != null)
        {
            interstitialImage.color = new Color(0.65f, 0.26f, 0.18f);
        }

        var packButton1 = DefaultControls.CreateButton(resources);
        packButton1.name = "PackButton1";
        packButton1.transform.SetParent(monetizationPanel.transform, false);
        var packRect1 = packButton1.GetComponent<RectTransform>();
        packRect1.anchorMin = new Vector2(0.5f, 0.45f);
        packRect1.anchorMax = new Vector2(0.5f, 0.45f);
        packRect1.anchoredPosition = new Vector2(0, 40);
        packRect1.sizeDelta = new Vector2(360, 70);
        var packLabel1 = packButton1.GetComponentInChildren<Text>();
        if (packLabel1 != null)
        {
            packLabel1.text = "Starter Pack";
            packLabel1.color = TextLight;
        }
        var packImage1 = packButton1.GetComponent<Image>();
        if (packImage1 != null)
        {
            packImage1.color = new Color(0.68f, 0.30f, 0.20f);
        }

        var packButton2 = DefaultControls.CreateButton(resources);
        packButton2.name = "PackButton2";
        packButton2.transform.SetParent(monetizationPanel.transform, false);
        var packRect2 = packButton2.GetComponent<RectTransform>();
        packRect2.anchorMin = new Vector2(0.5f, 0.32f);
        packRect2.anchorMax = new Vector2(0.5f, 0.32f);
        packRect2.anchoredPosition = new Vector2(0, 20);
        packRect2.sizeDelta = new Vector2(360, 70);
        var packLabel2 = packButton2.GetComponentInChildren<Text>();
        if (packLabel2 != null)
        {
            packLabel2.text = "Premium Pack";
            packLabel2.color = TextLight;
        }
        var packImage2 = packButton2.GetComponent<Image>();
        if (packImage2 != null)
        {
            packImage2.color = new Color(0.72f, 0.34f, 0.22f);
        }

        var shopCloseGo = DefaultControls.CreateButton(resources);
        shopCloseGo.name = "ShopClose";
        shopCloseGo.transform.SetParent(monetizationPanel.transform, false);
        var shopCloseRect = shopCloseGo.GetComponent<RectTransform>();
        shopCloseRect.anchorMin = new Vector2(0.5f, 0.1f);
        shopCloseRect.anchorMax = new Vector2(0.5f, 0.1f);
        shopCloseRect.anchoredPosition = new Vector2(0, 10);
        shopCloseRect.sizeDelta = new Vector2(200, 40);
        var shopCloseText = shopCloseGo.GetComponentInChildren<Text>();
        if (shopCloseText != null)
        {
            shopCloseText.text = "Close";
            shopCloseText.color = TextLight;
        }
        var shopCloseImage = shopCloseGo.GetComponent<Image>();
        if (shopCloseImage != null)
        {
            shopCloseImage.color = new Color(0.55f, 0.22f, 0.16f);
        }
        monetizationPanel.SetActive(false);

        var tutorialOverlay = CreatePanel("TutorialOverlay", canvasGo.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0f, 0f, 0f, 0.6f));
        var tutorialView = tutorialOverlay.AddComponent<TutorialView>();
        var tutorialOverlayImage = tutorialOverlay.GetComponent<Image>();
        if (tutorialOverlayImage != null)
        {
            tutorialOverlayImage.raycastTarget = false;
        }
        var tutorialMessage = CreateText(resources, "TutorialMessage", tutorialOverlay.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 40), new Vector2(700, 120), 26, TextLight, TextAnchor.MiddleCenter);
        var tutorialMessageText = tutorialMessage.GetComponent<Text>();
        if (tutorialMessageText != null)
        {
            tutorialMessageText.text = "튜토리얼";
            tutorialMessageText.raycastTarget = false;
        }
        var tutorialSkipGo = DefaultControls.CreateButton(resources);
        tutorialSkipGo.name = "TutorialSkip";
        tutorialSkipGo.transform.SetParent(tutorialOverlay.transform, false);
        var tutorialSkipRect = tutorialSkipGo.GetComponent<RectTransform>();
        tutorialSkipRect.anchorMin = new Vector2(0.5f, 0.5f);
        tutorialSkipRect.anchorMax = new Vector2(0.5f, 0.5f);
        tutorialSkipRect.anchoredPosition = new Vector2(0, -60);
        tutorialSkipRect.sizeDelta = new Vector2(200, 40);
        var tutorialSkipText = tutorialSkipGo.GetComponentInChildren<Text>();
        if (tutorialSkipText != null)
        {
            tutorialSkipText.text = "Skip";
            tutorialSkipText.color = TextLight;
        }
        var tutorialSkipImage = tutorialSkipGo.GetComponent<Image>();
        if (tutorialSkipImage != null)
        {
            tutorialSkipImage.color = new Color(0.55f, 0.22f, 0.16f);
        }
        tutorialOverlay.SetActive(false);

        var leaderboardSerialized = new SerializedObject(leaderboardView);
        leaderboardSerialized.FindProperty("listText").objectReferenceValue = leaderboardList.GetComponent<Text>();
        leaderboardSerialized.FindProperty("playerText").objectReferenceValue = playerRankText.GetComponent<Text>();
        leaderboardSerialized.FindProperty("friendsText").objectReferenceValue = friendsText.GetComponent<Text>();
        leaderboardSerialized.FindProperty("refreshButton").objectReferenceValue = leaderboardRefreshGo.GetComponent<Button>();
        leaderboardSerialized.FindProperty("closeButton").objectReferenceValue = leaderboardCloseGo.GetComponent<Button>();
        leaderboardSerialized.ApplyModifiedPropertiesWithoutUndo();

        var monetizationSerialized = new SerializedObject(monetizationView);
        monetizationSerialized.FindProperty("statusText").objectReferenceValue = shopStatus.GetComponent<Text>();
        monetizationSerialized.FindProperty("rewardedButton").objectReferenceValue = rewardedGo.GetComponent<Button>();
        monetizationSerialized.FindProperty("interstitialButton").objectReferenceValue = interstitialGo.GetComponent<Button>();
        monetizationSerialized.FindProperty("closeButton").objectReferenceValue = shopCloseGo.GetComponent<Button>();
        monetizationSerialized.FindProperty("packButtons").arraySize = 2;
        monetizationSerialized.FindProperty("packLabels").arraySize = 2;
        monetizationSerialized.FindProperty("packButtons").GetArrayElementAtIndex(0).objectReferenceValue = packButton1.GetComponent<Button>();
        monetizationSerialized.FindProperty("packButtons").GetArrayElementAtIndex(1).objectReferenceValue = packButton2.GetComponent<Button>();
        monetizationSerialized.FindProperty("packLabels").GetArrayElementAtIndex(0).objectReferenceValue = packLabel1;
        monetizationSerialized.FindProperty("packLabels").GetArrayElementAtIndex(1).objectReferenceValue = packLabel2;
        monetizationSerialized.ApplyModifiedPropertiesWithoutUndo();

        var tutorialSerialized = new SerializedObject(tutorialView);
        tutorialSerialized.FindProperty("messageText").objectReferenceValue = tutorialMessage.GetComponent<Text>();
        tutorialSerialized.FindProperty("skipButton").objectReferenceValue = tutorialSkipGo.GetComponent<Button>();
        tutorialSerialized.ApplyModifiedPropertiesWithoutUndo();

        if (leaderboardButton != null)
        {
            UnityEventTools.AddPersistentListener(leaderboardButton.onClick, leaderboardView.Open);
        }

        if (shopButton != null)
        {
            UnityEventTools.AddPersistentListener(shopButton.onClick, monetizationView.Open);
        }

        var dailyInstance = (GameObject)PrefabUtility.InstantiatePrefab(dailyMissionPrefab);
        dailyInstance.transform.SetParent(canvasGo.transform, false);
        var dailyRect = dailyInstance.GetComponent<RectTransform>();
        dailyRect.anchorMin = new Vector2(0f, 0f);
        dailyRect.anchorMax = new Vector2(0f, 0f);
        dailyRect.anchoredPosition = new Vector2(40, 260);

        var prestigeInstance = (GameObject)PrefabUtility.InstantiatePrefab(prestigePrefab);
        prestigeInstance.transform.SetParent(canvasGo.transform, false);
        var prestigeRect = prestigeInstance.GetComponent<RectTransform>();
        prestigeRect.anchorMin = new Vector2(1f, 0f);
        prestigeRect.anchorMax = new Vector2(1f, 0f);
        prestigeRect.anchoredPosition = new Vector2(-40, 260);

        var dailyView = dailyInstance.GetComponent<DailyMissionView>();
        var prestigeView = prestigeInstance.GetComponent<PrestigeView>();

        var uiSerialized = new SerializedObject(uiController);
        uiSerialized.FindProperty("currencyText").objectReferenceValue = currencyText.GetComponent<Text>();
        uiSerialized.FindProperty("incomeText").objectReferenceValue = incomeText.GetComponent<Text>();
        uiSerialized.FindProperty("storeTierText").objectReferenceValue = storeTierText.GetComponent<Text>();
        uiSerialized.FindProperty("prestigeText").objectReferenceValue = prestigeText.GetComponent<Text>();
        uiSerialized.FindProperty("loginRewardText").objectReferenceValue = loginRewardText.GetComponent<Text>();
        uiSerialized.FindProperty("dailyMissionsText").objectReferenceValue = dailyText.GetComponent<Text>();
        uiSerialized.FindProperty("queueText").objectReferenceValue = queueList.GetComponent<Text>();
        uiSerialized.FindProperty("queueMetricsText").objectReferenceValue = queueMetrics.GetComponent<Text>();
        uiSerialized.FindProperty("upgradesText").objectReferenceValue = null;
        uiSerialized.FindProperty("debugIndicatorText").objectReferenceValue = debugIndicator.GetComponent<Text>();
        uiSerialized.FindProperty("comboText").objectReferenceValue = comboText.GetComponent<Text>();
        uiSerialized.FindProperty("debugToggleButton").objectReferenceValue = debugToggleGo.GetComponent<Button>();
        uiSerialized.FindProperty("satisfactionSlider").objectReferenceValue = slider;
        uiSerialized.FindProperty("comboSlider").objectReferenceValue = comboSlider;
        uiSerialized.FindProperty("dailyMissionView").objectReferenceValue = dailyView;
        uiSerialized.FindProperty("prestigeView").objectReferenceValue = prestigeView;
        uiSerialized.FindProperty("queueControlView").objectReferenceValue = queueControlView;
        uiSerialized.FindProperty("upgradeListView").objectReferenceValue = upgradeListView;
        uiSerialized.FindProperty("debugPanelView").objectReferenceValue = debugView;
        uiSerialized.FindProperty("perfOverlayView").objectReferenceValue = perfView;
        uiSerialized.FindProperty("tutorialView").objectReferenceValue = tutorialView;
        uiSerialized.FindProperty("leaderboardView").objectReferenceValue = leaderboardView;
        uiSerialized.FindProperty("monetizationView").objectReferenceValue = monetizationView;
        uiSerialized.ApplyModifiedPropertiesWithoutUndo();

        var queueControlSerialized = new SerializedObject(queueControlView);
        queueControlSerialized.FindProperty("serveButton").objectReferenceValue = serveButtonGo.GetComponent<Button>();
        queueControlSerialized.FindProperty("rushButton").objectReferenceValue = rushButtonGo.GetComponent<Button>();
        queueControlSerialized.ApplyModifiedPropertiesWithoutUndo();

        var upgradeListSerialized = new SerializedObject(upgradeListView);
        upgradeListSerialized.FindProperty("scrollRect").objectReferenceValue = upgradesScrollRect;
        upgradeListSerialized.FindProperty("content").objectReferenceValue = content;
        upgradeListSerialized.FindProperty("rowTemplate").objectReferenceValue = rowView;
        upgradeListSerialized.FindProperty("initialPoolSize").intValue = 8;
        upgradeListSerialized.ApplyModifiedPropertiesWithoutUndo();

        var debugSerialized = new SerializedObject(debugView);
        debugSerialized.FindProperty("spawnRateSlider").objectReferenceValue = spawnSlider;
        debugSerialized.FindProperty("serviceRateSlider").objectReferenceValue = serviceSlider;
        debugSerialized.FindProperty("spawnValueText").objectReferenceValue = spawnValue.GetComponent<Text>();
        debugSerialized.FindProperty("serviceValueText").objectReferenceValue = serviceValue.GetComponent<Text>();
        debugSerialized.FindProperty("presetDropdown").objectReferenceValue = presetDropdown;
        debugSerialized.FindProperty("resetButton").objectReferenceValue = resetButtonGo.GetComponent<Button>();
        debugSerialized.ApplyModifiedPropertiesWithoutUndo();

        var perfSerialized = new SerializedObject(perfView);
        perfSerialized.FindProperty("overlayText").objectReferenceValue = perfText.GetComponent<Text>();
        perfSerialized.FindProperty("updateInterval").floatValue = 0.5f;
        perfSerialized.ApplyModifiedPropertiesWithoutUndo();

        return canvasGo;
    }

    private static GameObject CreateGameRoot(GameDataCatalog catalog)
    {
        var root = new GameObject("GameRoot");
        var audioRoot = new GameObject("AudioManager");
        audioRoot.transform.SetParent(root.transform, false);
        var audioManager = audioRoot.AddComponent<AudioManager>();

        var sizzleA = audioRoot.AddComponent<AudioSource>();
        var sizzleB = audioRoot.AddComponent<AudioSource>();
        var sizzleC = audioRoot.AddComponent<AudioSource>();
        var uiSource = audioRoot.AddComponent<AudioSource>();

        var audioSerialized = new SerializedObject(audioManager);
        audioSerialized.FindProperty("sizzleLayerA").objectReferenceValue = sizzleA;
        audioSerialized.FindProperty("sizzleLayerB").objectReferenceValue = sizzleB;
        audioSerialized.FindProperty("sizzleLayerC").objectReferenceValue = sizzleC;
        audioSerialized.FindProperty("uiSource").objectReferenceValue = uiSource;
        audioSerialized.ApplyModifiedPropertiesWithoutUndo();

        var networkRoot = new GameObject("NetworkService");
        networkRoot.transform.SetParent(root.transform, false);
        var networkService = networkRoot.AddComponent<NetworkService>();
        var networkSerialized = new SerializedObject(networkService);
        networkSerialized.FindProperty("apiConfig").objectReferenceValue = catalog != null ? catalog.apiConfig : null;
        networkSerialized.ApplyModifiedPropertiesWithoutUndo();

        var analyticsRoot = new GameObject("AnalyticsService");
        analyticsRoot.transform.SetParent(root.transform, false);
        var analyticsService = analyticsRoot.AddComponent<AnalyticsService>();

        var monetizationRoot = new GameObject("MonetizationService");
        monetizationRoot.transform.SetParent(root.transform, false);
        var monetizationService = monetizationRoot.AddComponent<MonetizationService>();
        var monetizationSerialized = new SerializedObject(monetizationService);
        monetizationSerialized.FindProperty("config").objectReferenceValue = catalog != null ? catalog.monetizationConfig : null;
        monetizationSerialized.ApplyModifiedPropertiesWithoutUndo();

        var managerRoot = new GameObject("GameManager");
        managerRoot.transform.SetParent(root.transform, false);
        var gameManager = managerRoot.AddComponent<GameManager>();

        var managerSerialized = new SerializedObject(gameManager);
        managerSerialized.FindProperty("audioManager").objectReferenceValue = audioManager;
        managerSerialized.FindProperty("networkService").objectReferenceValue = networkService;
        managerSerialized.FindProperty("analyticsService").objectReferenceValue = analyticsService;
        managerSerialized.FindProperty("monetizationService").objectReferenceValue = monetizationService;
        managerSerialized.FindProperty("dataCatalog").objectReferenceValue = catalog;
        managerSerialized.ApplyModifiedPropertiesWithoutUndo();

        var cameraRoot = new GameObject("Main Camera");
        cameraRoot.transform.SetParent(root.transform, false);
        cameraRoot.AddComponent<Camera>();
        cameraRoot.AddComponent<AudioListener>();

        return root;
    }

    private static GameObject CreateDailyMissionPrefab()
    {
        var resources = new DefaultControls.Resources();
        var panel = new GameObject("DailyMissionPanel", typeof(RectTransform), typeof(Image), typeof(DailyMissionView));
        var rect = panel.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(420, 240);
        var panelImage = panel.GetComponent<Image>();
        if (panelImage != null)
        {
            panelImage.color = PanelColor;
        }

        var view = panel.GetComponent<DailyMissionView>();
        var texts = new Text[3];
        var buttons = new Button[3];

        for (int i = 0; i < 3; i++)
        {
            var textGo = CreateText(resources, "MissionText" + (i + 1), panel.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20, -30 - (i * 50)), new Vector2(260, 30), 18, TextDark, TextAnchor.MiddleLeft);
            texts[i] = textGo.GetComponent<Text>();

            var buttonGo = DefaultControls.CreateButton(resources);
            buttonGo.name = "ClaimButton" + (i + 1);
            buttonGo.transform.SetParent(panel.transform, false);
            var buttonRect = buttonGo.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(1f, 1f);
            buttonRect.anchorMax = new Vector2(1f, 1f);
            buttonRect.anchoredPosition = new Vector2(-60, -30 - (i * 50));
            buttonRect.sizeDelta = new Vector2(100, 30);
            var btnText = buttonGo.GetComponentInChildren<Text>();
            if (btnText != null)
            {
                btnText.text = "Claim";
                btnText.color = TextLight;
            }
            var buttonImage = buttonGo.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = AccentColor;
            }
            buttons[i] = buttonGo.GetComponent<Button>();
        }

        var viewSerialized = new SerializedObject(view);
        viewSerialized.FindProperty("missionTexts").arraySize = texts.Length;
        viewSerialized.FindProperty("claimButtons").arraySize = buttons.Length;
        for (int i = 0; i < texts.Length; i++)
        {
            viewSerialized.FindProperty("missionTexts").GetArrayElementAtIndex(i).objectReferenceValue = texts[i];
            viewSerialized.FindProperty("claimButtons").GetArrayElementAtIndex(i).objectReferenceValue = buttons[i];
            UnityEventTools.AddIntPersistentListener(buttons[i].onClick, view.ClaimMission, i);
        }
        viewSerialized.ApplyModifiedPropertiesWithoutUndo();

        var prefabPath = "Assets/Prefabs/DailyMissionPanel.prefab";
        var prefab = PrefabUtility.SaveAsPrefabAsset(panel, prefabPath);
        Object.DestroyImmediate(panel);
        return prefab;
    }

    private static GameObject CreatePrestigePrefab()
    {
        var resources = new DefaultControls.Resources();
        var panel = new GameObject("PrestigePanel", typeof(RectTransform), typeof(Image), typeof(PrestigeView));
        var rect = panel.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(320, 160);
        var panelImage = panel.GetComponent<Image>();
        if (panelImage != null)
        {
            panelImage.color = PanelColor;
        }

        var prestigeTextGo = CreateText(resources, "PrestigeInfo", panel.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -30), new Vector2(280, 36), 20, TextDark, TextAnchor.MiddleCenter);
        var buttonGo = DefaultControls.CreateButton(resources);
        buttonGo.name = "PrestigeButton";
        buttonGo.transform.SetParent(panel.transform, false);
        var buttonRect = buttonGo.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0f);
        buttonRect.anchorMax = new Vector2(0.5f, 0f);
        buttonRect.anchoredPosition = new Vector2(0, 20);
        buttonRect.sizeDelta = new Vector2(180, 40);
        var btnText = buttonGo.GetComponentInChildren<Text>();
        if (btnText != null)
        {
            btnText.text = "Prestige";
            btnText.color = TextLight;
        }
        var buttonImage = buttonGo.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.color = AccentColor;
        }

        var view = panel.GetComponent<PrestigeView>();
        var viewSerialized = new SerializedObject(view);
        viewSerialized.FindProperty("prestigeInfoText").objectReferenceValue = prestigeTextGo.GetComponent<Text>();
        viewSerialized.FindProperty("prestigeButton").objectReferenceValue = buttonGo.GetComponent<Button>();
        viewSerialized.ApplyModifiedPropertiesWithoutUndo();

        var button = buttonGo.GetComponent<Button>();
        UnityEventTools.AddPersistentListener(button.onClick, view.OnPrestigeClicked);

        var prefabPath = "Assets/Prefabs/PrestigePanel.prefab";
        var prefab = PrefabUtility.SaveAsPrefabAsset(panel, prefabPath);
        Object.DestroyImmediate(panel);
        return prefab;
    }

    private static GameObject CreateText(DefaultControls.Resources resources, string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 size, int fontSize, Color color, TextAnchor alignment)
    {
        var go = DefaultControls.CreateText(resources);
        go.name = name;
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = size;

        var text = go.GetComponent<Text>();
        if (text != null)
        {
            text.text = name;
            text.alignment = alignment;
            text.fontSize = fontSize;
            text.color = color;
        }
        return go;
    }

    private static GameObject CreatePanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 size, Vector2 anchoredPos, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = size;
        var image = go.GetComponent<Image>();
        if (image != null)
        {
            image.color = color;
        }
        return go;
    }

    private static MenuItem CreateMenuItemAsset(string path, string id, string name, int unlockLevel, float basePrice, float bonusMultiplier)
    {
        var asset = AssetDatabase.LoadAssetAtPath<MenuItem>(path);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<MenuItem>();
            AssetDatabase.CreateAsset(asset, path);
        }

        asset.id = id;
        asset.displayName = name;
        asset.unlockLevel = unlockLevel;
        asset.basePrice = basePrice;
        asset.bonusMultiplier = bonusMultiplier;
        EditorUtility.SetDirty(asset);
        return asset;
    }

    private static UpgradeData CreateUpgradeAsset(string path, string id, string displayName, string category, string targetId, float baseCost, float costMultiplier, float effectValue)
    {
        var asset = AssetDatabase.LoadAssetAtPath<UpgradeData>(path);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<UpgradeData>();
            AssetDatabase.CreateAsset(asset, path);
        }

        asset.id = id;
        asset.displayName = displayName;
        asset.category = category;
        asset.targetId = targetId;
        asset.baseCost = baseCost;
        asset.costMultiplier = costMultiplier;
        asset.effectValue = effectValue;
        EditorUtility.SetDirty(asset);
        return asset;
    }

    private static StoreTier CreateStoreTierAsset(string path, string id, string name, int unlockLevel, float multiplier)
    {
        var asset = AssetDatabase.LoadAssetAtPath<StoreTier>(path);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<StoreTier>();
            AssetDatabase.CreateAsset(asset, path);
        }

        asset.id = id;
        asset.displayName = name;
        asset.unlockLevel = unlockLevel;
        asset.incomeMultiplier = multiplier;
        EditorUtility.SetDirty(asset);
        return asset;
    }

    private static CustomerType CreateCustomerAsset(string path, string id, string name, float patience, float tipMultiplier)
    {
        var asset = AssetDatabase.LoadAssetAtPath<CustomerType>(path);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<CustomerType>();
            AssetDatabase.CreateAsset(asset, path);
        }

        asset.id = id;
        asset.displayName = name;
        asset.patience = patience;
        asset.tipMultiplier = tipMultiplier;
        EditorUtility.SetDirty(asset);
        return asset;
    }

    private static ApiConfig CreateApiConfig(string path)
    {
        var asset = AssetDatabase.LoadAssetAtPath<ApiConfig>(path);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<ApiConfig>();
            AssetDatabase.CreateAsset(asset, path);
        }

        asset.baseUrl = "https://api.example.com";
        asset.region = "KR";
        asset.hmacSecret = "CHANGE_ME";
        asset.timeoutSeconds = 10;
        asset.enableNetwork = true;
        asset.allowInEditor = false;
        EditorUtility.SetDirty(asset);
        return asset;
    }

    private static EconomyTuning CreateEconomyTuning(string path)
    {
        var asset = AssetDatabase.LoadAssetAtPath<EconomyTuning>(path);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<EconomyTuning>();
            AssetDatabase.CreateAsset(asset, path);
        }

        asset.maxLevel = 100;
        asset.baseRequirement = 50.0;
        asset.requirementGrowth = 1.28;
        asset.baseIncomePerSec = 1.0;
        asset.incomeGrowth = 1.22;
        asset.baseUpgradeCost = 10.0;
        asset.upgradeGrowth = 1.3;
        asset.RebuildTable();
        EditorUtility.SetDirty(asset);
        return asset;
    }

    private static MonetizationConfig CreateMonetizationConfig(string path)
    {
        var asset = AssetDatabase.LoadAssetAtPath<MonetizationConfig>(path);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<MonetizationConfig>();
            AssetDatabase.CreateAsset(asset, path);
        }

        asset.enableAds = true;
        asset.enableIap = true;
        asset.rewardedMultiplier = 2f;
        asset.rewardedDuration = 120f;
        asset.interstitialReward = 100;
        asset.packs = new List<IapPack>
        {
            new IapPack { id = "starter", displayName = "Starter Pack", priceLabel = "$0.99", currencyReward = 500 },
            new IapPack { id = "premium", displayName = "Premium Pack", priceLabel = "$4.99", currencyReward = 4000 }
        };

        EditorUtility.SetDirty(asset);
        return asset;
    }

    private static void EnsureFolder(string parent, string name)
    {
        var path = parent + "/" + name;
        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
