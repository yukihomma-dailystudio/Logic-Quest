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
    private const string ClarisseTalkRootName = "ClarisseTalkRoot";
    private const string ClarisseBubbleName = "ClarisseBubble";
    private const string CharacterName = "ClarisseCharacter";
    private static readonly string[] CharacterResourcePaths =
    {
        "Characters/ClarisseThinking",
        "Characters/ClarisseNormal"
    };
    private const string StartButtonName = "StartButton";
    private const string DailyHuntButtonName = "DailyHuntButton";
    private const string BackButtonName = "BackButton";
    private const string SendButtonName = "SendButton";
    private const string EventSystemName = "EventSystem";
    private const float ReferenceWidth = 1920f;
    private const float ReferenceHeight = 1080f;
    private const int ClarisseInputCharacterLimit = 200;
    private static Sprite roundedPanelSprite;

    private static readonly string[] ClarisseTapLines =
    {
        "今日も少しだけ、考える練習をしていきましょうですわ。",
        "焦らなくて大丈夫ですわ。言葉は少しずつ整えていけばよいのです。",
        "強い考えは、最初から強いわけではありませんわ。磨くものです。",
        "本日の敵は、なかなか手ごわそうですわね。",
        "一問だけでも十分ですわ。継続こそ、いちばん上品な勝利です。",
        "今日はどんな考えを鍛えますの？",
        "小さな違和感も、立派なテーマになりますわ。",
        "迷ったら、最近もやっとしたことから始めてみましょう。"
    };

    private static readonly string[] TiredReplies =
    {
        "お疲れさまですわ。今日は一問だけでも十分です。言葉にする練習を、小さく整えるところから始めましょう。",
        "無理に戦わなくても大丈夫ですわ。小さく整える日も大切です。短い一言だけ、そっと置いてみましょう。"
    };

    private static readonly string[] ThemeReplies =
    {
        "最近もやっとしたことを一つ選ぶと、良いテーマになりますわ。大きく考えず、まず一文にしてみましょう。",
        "誰かに説明したいことや、少し引っかかった出来事がおすすめですわ。そこから問いを一つ作れますの。"
    };

    private static readonly string[] DifficultReplies =
    {
        "勝つことより、考えを少し整えることが大切ですわ。難しかった点を一つだけ言葉にしてみましょう。",
        "難しいと感じたところこそ、伸びる場所ですわ。答えを急がず、引っかかった理由を探してみましょう。"
    };

    private static readonly string[] MotivationReplies =
    {
        "今日は一言だけ書く、を勝利条件にしてしまいましょう。軽い一歩でも、考える習慣には十分ですわ。",
        "やる気が少ない日ほど、軽い一歩で十分ですわ。まずは気になる言葉を一つだけ選んでみましょう。"
    };

    private static readonly string[] DefaultClarisseReplies =
    {
        "その言葉、少しテーマにできそうですわね。何が気になったのか、一つだけ説明の練習をしてみましょう。",
        "なるほどですわ。では、それを一つの問いにしてみましょう。急がず、短い言葉から整えれば十分ですわ。",
        "よろしければ、その考えをもう少しだけ言葉にしてみましょう。小さな違和感も、立派な入口ですわ。"
    };

    [SerializeField] private string titleSceneName = "TitleScene";
    [SerializeField] private string themeInputSceneName = "ThemeInputScene";
    [SerializeField] private Texture2D guildHallBackground;
    [SerializeField] private Texture2D clarisseCharacter;

    private bool showMissingThemeInputSceneMessage;
    private UserProfile profile;
    private RawImage backgroundImage;
    private RawImage characterImage;
    private Text statusHeaderText;
    private Text[] abilityTexts;
    private Text messageText;
    private Text clarisseLineText;
    private InputField clarisseInputField;
    private Button startButton;
    private Button dailyHuntButton;
    private Button backButton;
    private Button characterButton;
    private Button clarisseSendButton;

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

        if (clarisseCharacter == null)
        {
            clarisseCharacter = LoadRandomCharacter();
            if (clarisseCharacter == null)
            {
                Debug.LogWarning("No Clarisse character textures could be loaded from Resources/Characters.");
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

        var clarisseTalkRoot = (RectTransform)GetOrCreateChild(canvasTransform, ClarisseTalkRootName);
        ConfigureReferenceRectFromBottom(clarisseTalkRoot, 0.38f, 0.06f, 0.34f, 0.30f);
        BuildClarisseTalkPanel(clarisseTalkRoot);

        characterImage = CreateRawImage(canvasTransform, CharacterName);
        ConfigureCharacterRect(characterImage.rectTransform);
        characterImage.texture = clarisseCharacter;
        characterImage.color = Color.white;
        characterImage.raycastTarget = true;
        characterButton = GetOrAddComponent<Button>(characterImage.gameObject);
        characterButton.targetGraphic = characterImage;
        characterButton.onClick.RemoveListener(ShowRandomClarisseLine);
        characterButton.onClick.AddListener(ShowRandomClarisseLine);
        characterImage.transform.SetAsLastSibling();
        clarisseTalkRoot.SetAsLastSibling();
    }

    private static Texture2D LoadRandomCharacter()
    {
        return LoadRandomCharacter(null);
    }

    private static Texture2D LoadRandomCharacter(Texture2D excludedCharacter)
    {
        var startIndex = Random.Range(0, CharacterResourcePaths.Length);
        Texture2D fallbackCharacter = null;

        for (var i = 0; i < CharacterResourcePaths.Length; i++)
        {
            var resourcePath = CharacterResourcePaths[(startIndex + i) % CharacterResourcePaths.Length];
            var character = Resources.Load<Texture2D>(resourcePath);
            if (character != null)
            {
                if (fallbackCharacter == null)
                {
                    fallbackCharacter = character;
                }

                if (character != excludedCharacter)
                {
                    return character;
                }

                continue;
            }

            Debug.LogWarning($"Character texture 'Resources/{resourcePath}' could not be loaded.");
        }

        return fallbackCharacter;
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
        const float buttonY = 0.38f;
        const float buttonWidth = 0.26f;
        const float buttonHeight = 0.22f;
        const float buttonGap = 0.04f;
        const float firstButtonX = 0.07f;

        startButton = CreateTransparentButton(
            navigationRoot,
            StartButtonName,
            new Rect(firstButtonX, buttonY, buttonWidth, buttonHeight),
            "クエスト開始");
        startButton.onClick.RemoveListener(TryLoadThemeInputScene);
        startButton.onClick.AddListener(TryLoadThemeInputScene);

        dailyHuntButton = CreateTransparentButton(
            navigationRoot,
            DailyHuntButtonName,
            new Rect(firstButtonX + buttonWidth + buttonGap, buttonY, buttonWidth, buttonHeight),
            "本日の討伐依頼");
        dailyHuntButton.onClick.RemoveListener(TryLoadThemeInputScene);
        dailyHuntButton.onClick.AddListener(TryLoadThemeInputScene);

        backButton = CreateTransparentButton(
            navigationRoot,
            BackButtonName,
            new Rect(firstButtonX + (buttonWidth + buttonGap) * 2f, buttonY, buttonWidth, buttonHeight),
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

    private void BuildClarisseTalkPanel(RectTransform talkRoot)
    {
        var rootImage = GetOrAddComponent<Image>(talkRoot.gameObject);
        rootImage.color = Color.clear;
        rootImage.raycastTarget = false;

        var bubbleTransform = (RectTransform)GetOrCreateChild(talkRoot, ClarisseBubbleName);
        ConfigureReferenceRect(bubbleTransform, 0f, 0f, 1f, 0.54f);

        var bubbleImage = GetOrAddComponent<Image>(bubbleTransform.gameObject);
        bubbleImage.sprite = GetRoundedPanelSprite();
        bubbleImage.type = Image.Type.Sliced;
        bubbleImage.color = new Color(0f, 0f, 0f, 0.58f);
        bubbleImage.raycastTarget = false;

        var speakerNameText = CreateText(
            bubbleTransform,
            "SpeakerName",
            new Rect(0.06f, 0.08f, 0.88f, 0.18f),
            18,
            FontStyle.Bold,
            TextAnchor.MiddleLeft);
        speakerNameText.color = new Color(1f, 0.90f, 0.62f);
        speakerNameText.text = "クラリス";

        clarisseLineText = CreateText(
            bubbleTransform,
            "ClarisseLine",
            new Rect(0.06f, 0.28f, 0.88f, 0.60f),
            22,
            FontStyle.Normal,
            TextAnchor.MiddleLeft);
        clarisseLineText.color = new Color(1f, 0.96f, 0.84f);
        clarisseLineText.horizontalOverflow = HorizontalWrapMode.Wrap;
        clarisseLineText.verticalOverflow = VerticalWrapMode.Truncate;
        clarisseLineText.text = "何かありましたら、気軽に話しかけてくださいませ。";

        clarisseInputField = CreateInputField(
            talkRoot,
            "ClarisseInput",
            new Rect(0f, 0.68f, 0.70f, 0.22f),
            "クラリスに話しかける");
        clarisseInputField.characterLimit = ClarisseInputCharacterLimit;

        clarisseSendButton = CreateTransparentButton(
            talkRoot,
            SendButtonName,
            new Rect(0.74f, 0.68f, 0.22f, 0.22f),
            "送信");
        ConfigureRoundedImage(GetOrAddComponent<Image>(clarisseSendButton.gameObject), new Color(0f, 0f, 0f, 0.48f));
        var sendLabel = clarisseSendButton.transform.Find("Label");
        if (sendLabel != null && sendLabel.TryGetComponent<Text>(out var sendLabelText))
        {
            sendLabelText.color = new Color(1f, 0.96f, 0.84f);
        }

        clarisseSendButton.onClick.RemoveListener(SubmitClarisseInput);
        clarisseSendButton.onClick.AddListener(SubmitClarisseInput);
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
            26,
            FontStyle.Bold,
            TextAnchor.MiddleCenter);
        labelText.text = label;
        labelText.color = new Color(0.24f, 0.13f, 0.06f);
        labelText.resizeTextForBestFit = true;
        labelText.resizeTextMinSize = 16;
        labelText.resizeTextMaxSize = 26;

        return button;
    }

    private static InputField CreateInputField(
        Transform parent,
        string objectName,
        Rect topLeftRect,
        string placeholder)
    {
        var inputTransform = (RectTransform)GetOrCreateChild(parent, objectName);
        ConfigureReferenceRect(inputTransform, topLeftRect.x, topLeftRect.y, topLeftRect.width, topLeftRect.height);

        var image = GetOrAddComponent<Image>(inputTransform.gameObject);
        ConfigureRoundedImage(image, new Color(1f, 0.96f, 0.86f, 0.92f));

        var inputField = GetOrAddComponent<InputField>(inputTransform.gameObject);
        inputField.targetGraphic = image;
        inputField.lineType = InputField.LineType.SingleLine;

        var text = CreateText(
            inputTransform,
            "Text",
            new Rect(0.04f, 0f, 0.92f, 1f),
            20,
            FontStyle.Normal,
            TextAnchor.MiddleLeft);
        text.color = new Color(0.22f, 0.13f, 0.07f);
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;

        var placeholderText = CreateText(
            inputTransform,
            "Placeholder",
            new Rect(0.04f, 0f, 0.92f, 1f),
            20,
            FontStyle.Italic,
            TextAnchor.MiddleLeft);
        placeholderText.text = placeholder;
        placeholderText.color = new Color(0.42f, 0.32f, 0.22f, 0.72f);

        inputField.textComponent = text;
        inputField.placeholder = placeholderText;

        return inputField;
    }

    private static void ConfigureRoundedImage(Image image, Color color)
    {
        image.sprite = GetRoundedPanelSprite();
        image.type = Image.Type.Sliced;
        image.color = color;
        image.raycastTarget = true;
    }

    private static Sprite GetRoundedPanelSprite()
    {
        if (roundedPanelSprite != null)
        {
            return roundedPanelSprite;
        }

        const int size = 48;
        const int radius = 12;
        var texture = new Texture2D(size, size, TextureFormat.ARGB32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };

        var pixels = new Color32[size * size];
        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                var nearestX = Mathf.Clamp(x, radius, size - radius - 1);
                var nearestY = Mathf.Clamp(y, radius, size - radius - 1);
                var distance = Vector2.Distance(new Vector2(x, y), new Vector2(nearestX, nearestY));
                var alpha = distance <= radius ? byte.MaxValue : byte.MinValue;
                pixels[y * size + x] = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, alpha);
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply();

        roundedPanelSprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, size, size),
            new Vector2(0.5f, 0.5f),
            100f,
            0,
            SpriteMeshType.FullRect,
            new Vector4(radius, radius, radius, radius));

        return roundedPanelSprite;
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

    private static void ConfigureReferenceRectFromBottom(RectTransform rect, float x, float yFromBottom, float width, float height)
    {
        rect.anchorMin = new Vector2(x, yFromBottom);
        rect.anchorMax = new Vector2(x + width, yFromBottom + height);
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

    private void ShowRandomClarisseLine()
    {
        SetClarisseLine(ChooseRandom(ClarisseTapLines));
    }

    private void SubmitClarisseInput()
    {
        if (clarisseInputField == null)
        {
            return;
        }

        var input = clarisseInputField.text.Trim();
        if (string.IsNullOrEmpty(input))
        {
            return;
        }

        SetClarisseLine(CreateClarisseReply(input));
        clarisseInputField.text = string.Empty;
    }

    private static string CreateClarisseReply(string input)
    {
        if (ContainsAny(input, "疲れた", "つかれた", "しんどい"))
        {
            return ChooseRandom(TiredReplies);
        }

        if (ContainsAny(input, "テーマ", "何を", "なにを"))
        {
            return ChooseRandom(ThemeReplies);
        }

        if (ContainsAny(input, "勝てない", "むずかしい", "難しい"))
        {
            return ChooseRandom(DifficultReplies);
        }

        if (ContainsAny(input, "やる気", "めんどい", "面倒"))
        {
            return ChooseRandom(MotivationReplies);
        }

        return ChooseRandom(DefaultClarisseReplies);
    }

    private void SetClarisseLine(string line)
    {
        if (clarisseLineText != null)
        {
            clarisseLineText.text = line;
        }

        RefreshClarisseCharacter();
    }

    private void RefreshClarisseCharacter()
    {
        var nextCharacter = LoadRandomCharacter(clarisseCharacter);
        if (nextCharacter == null)
        {
            return;
        }

        clarisseCharacter = nextCharacter;
        if (characterImage != null)
        {
            characterImage.texture = clarisseCharacter;
        }
    }

    private static bool ContainsAny(string input, params string[] keywords)
    {
        foreach (var keyword in keywords)
        {
            if (input.Contains(keyword))
            {
                return true;
            }
        }

        return false;
    }

    private static string ChooseRandom(string[] lines)
    {
        return lines[Random.Range(0, lines.Length)];
    }
}
