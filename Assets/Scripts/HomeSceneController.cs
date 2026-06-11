using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class HomeSceneController : MonoBehaviour
{
    private const string CanvasName = "HomeCanvas";
    private const string BackgroundName = "Background";
    private const string OverlayName = "Overlay";
    private const string StatusRootName = "StatusRoot";
    private const string NavigationRootName = "NavigationRoot";
    private const string CharacterName = "ClarisseThinking";
    private const string StartButtonName = "StartButton";
    private const string DailyHuntButtonName = "DailyHuntButton";
    private const string BackButtonName = "BackButton";
    private const string EventSystemName = "EventSystem";
    private const float ReferenceWidth = 1920f;
    private const float ReferenceHeight = 1080f;

    [SerializeField] private string titleSceneName = "TitleScene";
    [SerializeField] private string themeInputSceneName = "ThemeInputScene";
    [SerializeField] private Texture2D guildHallBackground;
    [SerializeField] private Texture2D clarisseThinkingCharacter;

    private bool showMissingThemeInputSceneMessage;
    private UserProfile profile;
    private RawImage backgroundImage;
    private RawImage characterImage;
    private Text statusHeaderText;
    private Text[] abilityTexts;
    private Text messageText;
    private Button startButton;
    private Button dailyHuntButton;
    private Button backButton;

    private void Start()
    {
        LoadAssetsIfNeeded();
        CreateOrUpdateHomeCanvas();
        EnsureEventSystem();
        RefreshView();
    }

    private void Update()
    {
        RefreshView();
    }

    private void LoadAssetsIfNeeded()
    {
        if (guildHallBackground == null)
        {
            guildHallBackground = Resources.Load<Texture2D>("Backgrounds/GuildHallHomeBackground");
        }

        if (clarisseThinkingCharacter == null)
        {
            clarisseThinkingCharacter = Resources.Load<Texture2D>("Characters/ClarisseThinking");
            if (clarisseThinkingCharacter == null)
            {
                Debug.LogWarning("Character texture 'Resources/Characters/ClarisseThinking' could not be loaded.");
            }
        }
    }

    private void CreateOrUpdateHomeCanvas()
    {
        var canvasTransform = GetOrCreateCanvas();

        backgroundImage = CreateRawImage(canvasTransform, BackgroundName);
        ConfigureFullScreen(backgroundImage.rectTransform);
        backgroundImage.texture = guildHallBackground;
        backgroundImage.color = Color.white;

        var overlay = GetOrCreateChild(canvasTransform, OverlayName);
        var overlayImage = GetOrAddComponent<Image>(overlay.gameObject);
        ConfigureFullScreen(overlayImage.rectTransform);
        overlayImage.color = new Color(0.02f, 0.02f, 0.02f, 0.32f);
        overlayImage.raycastTarget = false;

        var statusRoot = (RectTransform)GetOrCreateChild(canvasTransform, StatusRootName);
        ConfigureReferenceRect(statusRoot, 0.16f, 0.09f, 0.68f, 0.17f);
        BuildStatusPanel(statusRoot);

        var navigationRoot = (RectTransform)GetOrCreateChild(canvasTransform, NavigationRootName);
        ConfigureReferenceRect(navigationRoot, 0.05f, 0.73f, 0.40f, 0.20f);
        BuildNavigationPanel(navigationRoot);

        characterImage = CreateRawImage(canvasTransform, CharacterName);
        ConfigureCharacterRect(characterImage.rectTransform);
        characterImage.texture = clarisseThinkingCharacter;
        characterImage.color = Color.white;
        characterImage.raycastTarget = false;
        characterImage.transform.SetAsLastSibling();
    }

    private void BuildStatusPanel(RectTransform statusRoot)
    {
        statusHeaderText = CreateText(
            statusRoot,
            "StatusHeader",
            new Rect(0.08f, 0.08f, 0.84f, 0.26f),
            24,
            FontStyle.Bold,
            TextAnchor.MiddleLeft);

        abilityTexts = new Text[UserDataManager.LoadProfile().Abilities.Length];

        for (var i = 0; i < abilityTexts.Length; i++)
        {
            var column = i % 3;
            var row = i / 3;
            abilityTexts[i] = CreateText(
                statusRoot,
                $"Ability{i + 1}",
                new Rect(0.08f + column * 0.30f, 0.42f + row * 0.22f, 0.27f, 0.18f),
                22,
                FontStyle.Normal,
                TextAnchor.MiddleLeft);
        }
    }

    private void BuildNavigationPanel(RectTransform navigationRoot)
    {
        startButton = CreateTransparentButton(
            navigationRoot,
            StartButtonName,
            new Rect(0.04f, 0.36f, 0.28f, 0.28f),
            "クエスト開始");
        startButton.onClick.RemoveListener(TryLoadThemeInputScene);
        startButton.onClick.AddListener(TryLoadThemeInputScene);

        dailyHuntButton = CreateTransparentButton(
            navigationRoot,
            DailyHuntButtonName,
            new Rect(0.35f, 0.36f, 0.32f, 0.28f),
            "本日の討伐依頼");
        dailyHuntButton.onClick.RemoveListener(TryLoadThemeInputScene);
        dailyHuntButton.onClick.AddListener(TryLoadThemeInputScene);

        backButton = CreateTransparentButton(
            navigationRoot,
            BackButtonName,
            new Rect(0.70f, 0.36f, 0.26f, 0.28f),
            "門へ戻る");
        backButton.onClick.RemoveListener(LoadTitleScene);
        backButton.onClick.AddListener(LoadTitleScene);

        messageText = CreateText(
            navigationRoot,
            "Message",
            new Rect(0f, -0.22f, 1f, 0.18f),
            20,
            FontStyle.Normal,
            TextAnchor.MiddleCenter);
        messageText.color = new Color(0.55f, 0.22f, 0.16f);
    }

    private void RefreshView()
    {
        profile = UserDataManager.LoadProfile();

        if (statusHeaderText != null)
        {
            statusHeaderText.text = $"冒険者記録   ランク {profile.Level}   累計EXP {profile.TotalExp}   完了した試練 {profile.BattlesCompleted}";
        }

        if (abilityTexts != null)
        {
            for (var i = 0; i < abilityTexts.Length && i < profile.Abilities.Length; i++)
            {
                var ability = profile.Abilities[i];
                abilityTexts[i].text = $"{ability.Name}  Lv.{ability.Level}  EXP {ability.Exp}";
            }
        }

        if (messageText != null)
        {
            messageText.text = "ThemeInputScene がまだ登録されていません。次の試作段階でつながります。";
            messageText.enabled = showMissingThemeInputSceneMessage;
        }
    }

    private Transform GetOrCreateCanvas()
    {
        var existing = transform.Find(CanvasName);
        if (existing != null)
        {
            ConfigureCanvas(existing.gameObject);
            return existing;
        }

        var canvasObject = new GameObject(CanvasName, typeof(RectTransform));
        canvasObject.transform.SetParent(transform, false);
        ConfigureCanvas(canvasObject);
        return canvasObject.transform;
    }

    private static void ConfigureCanvas(GameObject canvasObject)
    {
        var canvas = GetOrAddComponent<Canvas>(canvasObject);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = GetOrAddComponent<CanvasScaler>(canvasObject);
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(ReferenceWidth, ReferenceHeight);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        GetOrAddComponent<GraphicRaycaster>(canvasObject);
    }

    private static void EnsureEventSystem()
    {
        if (FindObjectOfType<EventSystem>() != null)
        {
            return;
        }

        var eventSystemObject = new GameObject(EventSystemName);
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<StandaloneInputModule>();
    }

    private static RawImage CreateRawImage(Transform parent, string objectName)
    {
        var child = GetOrCreateChild(parent, objectName);
        var image = GetOrAddComponent<RawImage>(child.gameObject);
        return image;
    }

    private static Text CreateText(
        Transform parent,
        string objectName,
        Rect topLeftRect,
        int fontSize,
        FontStyle fontStyle,
        TextAnchor alignment)
    {
        var child = GetOrCreateChild(parent, objectName);
        var text = GetOrAddComponent<Text>(child.gameObject);
        ConfigureReferenceRect(text.rectTransform, topLeftRect.x, topLeftRect.y, topLeftRect.width, topLeftRect.height);
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.alignment = alignment;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.color = new Color(0.25f, 0.17f, 0.09f);
        text.raycastTarget = false;
        return text;
    }

    private static Button CreateTransparentButton(
        Transform parent,
        string objectName,
        Rect topLeftRect,
        string label)
    {
        var buttonTransform = (RectTransform)GetOrCreateChild(parent, objectName);
        ConfigureReferenceRect(buttonTransform, topLeftRect.x, topLeftRect.y, topLeftRect.width, topLeftRect.height);

        var image = GetOrAddComponent<Image>(buttonTransform.gameObject);
        image.color = new Color(1f, 1f, 1f, 0f);
        image.raycastTarget = true;

        var button = GetOrAddComponent<Button>(buttonTransform.gameObject);
        button.targetGraphic = image;

        var labelText = CreateText(
            buttonTransform,
            "Label",
            new Rect(0f, 0f, 1f, 1f),
            30,
            FontStyle.Bold,
            TextAnchor.MiddleCenter);
        labelText.text = label;
        labelText.color = new Color(0.24f, 0.13f, 0.06f);
        labelText.resizeTextForBestFit = true;
        labelText.resizeTextMinSize = 18;
        labelText.resizeTextMaxSize = 30;

        return button;
    }

    private static Transform GetOrCreateChild(Transform parent, string objectName)
    {
        var existing = parent.Find(objectName);
        if (existing != null)
        {
            return existing;
        }

        var child = new GameObject(objectName, typeof(RectTransform));
        child.transform.SetParent(parent, false);
        return child.transform;
    }

    private static void ConfigureFullScreen(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.localScale = Vector3.one;
    }

    private static void ConfigureCharacterRect(RectTransform rect)
    {
        rect.anchorMin = new Vector2(0.82f, 0f);
        rect.anchorMax = new Vector2(0.82f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(430f, 573f);
        rect.localScale = Vector3.one;
    }

    private static void ConfigureReferenceRect(RectTransform rect, float x, float yFromTop, float width, float height)
    {
        rect.anchorMin = new Vector2(x, 1f - yFromTop - height);
        rect.anchorMax = new Vector2(x + width, 1f - yFromTop);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.localScale = Vector3.one;
    }

    private static T GetOrAddComponent<T>(GameObject target) where T : Component
    {
        var component = target.GetComponent<T>();
        return component != null ? component : target.AddComponent<T>();
    }

    private void TryLoadThemeInputScene()
    {
        if (Application.CanStreamedLevelBeLoaded(themeInputSceneName))
        {
            SceneManager.LoadScene(themeInputSceneName);
            return;
        }

        showMissingThemeInputSceneMessage = true;
        Debug.LogWarning($"Scene '{themeInputSceneName}' is not available yet.");
        RefreshView();
    }

    private void LoadTitleScene()
    {
        SceneManager.LoadScene(titleSceneName);
    }
}
