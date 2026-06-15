using UnityEngine;
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
            clarisseCharacter = ClarisseDialogue.LoadRandomCharacter();
            if (clarisseCharacter == null)
            {
                Debug.LogWarning("No Clarisse character textures could be loaded from Resources/Characters.");
            }
        }
    }

    private void CreateOrUpdateHomeCanvas()
    {
        var canvasTransform = GetOrCreateCanvas();

        backgroundImage = HomeUiFactory.CreateRawImage(canvasTransform, BackgroundName);
        HomeUiFactory.ConfigureFullScreen(backgroundImage.rectTransform);
        backgroundImage.texture = guildHallBackground;
        backgroundImage.color = Color.white;

        var overlay = HomeUiFactory.GetOrCreateChild(canvasTransform, OverlayName);
        var overlayImage = HomeUiFactory.GetOrAddComponent<Image>(overlay.gameObject);
        HomeUiFactory.ConfigureFullScreen(overlayImage.rectTransform);
        overlayImage.color = new Color(0.02f, 0.02f, 0.02f, 0.32f);
        overlayImage.raycastTarget = false;

        var statusRoot = (RectTransform)HomeUiFactory.GetOrCreateChild(canvasTransform, StatusRootName);
        HomeUiFactory.ConfigureReferenceRect(statusRoot, 0.16f, 0.09f, 0.68f, 0.17f);
        BuildStatusPanel(statusRoot);

        var navigationRoot = (RectTransform)HomeUiFactory.GetOrCreateChild(canvasTransform, NavigationRootName);
        HomeUiFactory.ConfigureReferenceRect(navigationRoot, 0.05f, 0.73f, 0.40f, 0.20f);
        BuildNavigationPanel(navigationRoot);

        var clarisseTalkRoot = (RectTransform)HomeUiFactory.GetOrCreateChild(canvasTransform, ClarisseTalkRootName);
        HomeUiFactory.ConfigureReferenceRectFromBottom(clarisseTalkRoot, 0.38f, 0.06f, 0.34f, 0.30f);
        BuildClarisseTalkPanel(clarisseTalkRoot);

        characterImage = HomeUiFactory.CreateRawImage(canvasTransform, CharacterName);
        HomeUiFactory.ConfigureCharacterRect(characterImage.rectTransform);
        characterImage.texture = clarisseCharacter;
        characterImage.color = Color.white;
        characterImage.raycastTarget = true;
        characterButton = HomeUiFactory.GetOrAddComponent<Button>(characterImage.gameObject);
        characterButton.targetGraphic = characterImage;
        characterButton.onClick.RemoveListener(ShowRandomClarisseLine);
        characterButton.onClick.AddListener(ShowRandomClarisseLine);
        characterImage.transform.SetAsLastSibling();
        clarisseTalkRoot.SetAsLastSibling();
    }

    private void BuildStatusPanel(RectTransform statusRoot)
    {
        statusHeaderText = HomeUiFactory.CreateText(
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
            abilityTexts[i] = HomeUiFactory.CreateText(
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

        startButton = HomeUiFactory.CreateTransparentButton(
            navigationRoot,
            StartButtonName,
            new Rect(firstButtonX, buttonY, buttonWidth, buttonHeight),
            "クエスト開始");
        startButton.onClick.RemoveListener(TryLoadThemeInputScene);
        startButton.onClick.AddListener(TryLoadThemeInputScene);

        dailyHuntButton = HomeUiFactory.CreateTransparentButton(
            navigationRoot,
            DailyHuntButtonName,
            new Rect(firstButtonX + buttonWidth + buttonGap, buttonY, buttonWidth, buttonHeight),
            "本日の討伐依頼");
        dailyHuntButton.onClick.RemoveListener(TryLoadThemeInputScene);
        dailyHuntButton.onClick.AddListener(TryLoadThemeInputScene);

        backButton = HomeUiFactory.CreateTransparentButton(
            navigationRoot,
            BackButtonName,
            new Rect(firstButtonX + (buttonWidth + buttonGap) * 2f, buttonY, buttonWidth, buttonHeight),
            "門へ戻る");
        backButton.onClick.RemoveListener(LoadTitleScene);
        backButton.onClick.AddListener(LoadTitleScene);

        messageText = HomeUiFactory.CreateText(
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
        var rootImage = HomeUiFactory.GetOrAddComponent<Image>(talkRoot.gameObject);
        rootImage.color = Color.clear;
        rootImage.raycastTarget = false;

        var bubbleTransform = (RectTransform)HomeUiFactory.GetOrCreateChild(talkRoot, ClarisseBubbleName);
        HomeUiFactory.ConfigureReferenceRect(bubbleTransform, 0f, 0f, 1f, 0.54f);

        var bubbleImage = HomeUiFactory.GetOrAddComponent<Image>(bubbleTransform.gameObject);
        bubbleImage.sprite = HomeUiFactory.GetRoundedPanelSprite();
        bubbleImage.type = Image.Type.Sliced;
        bubbleImage.color = new Color(0f, 0f, 0f, 0.58f);
        bubbleImage.raycastTarget = false;

        var speakerNameText = HomeUiFactory.CreateText(
            bubbleTransform,
            "SpeakerName",
            new Rect(0.06f, 0.08f, 0.88f, 0.18f),
            18,
            FontStyle.Bold,
            TextAnchor.MiddleLeft);
        speakerNameText.color = new Color(1f, 0.90f, 0.62f);
        speakerNameText.text = "クラリス";

        clarisseLineText = HomeUiFactory.CreateText(
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

        clarisseInputField = HomeUiFactory.CreateInputField(
            talkRoot,
            "ClarisseInput",
            new Rect(0f, 0.68f, 0.70f, 0.22f),
            "クラリスに話しかける");
        clarisseInputField.characterLimit = ClarisseDialogue.InputCharacterLimit;

        clarisseSendButton = HomeUiFactory.CreateTransparentButton(
            talkRoot,
            SendButtonName,
            new Rect(0.74f, 0.68f, 0.22f, 0.22f),
            "送信");
        HomeUiFactory.ConfigureRoundedImage(HomeUiFactory.GetOrAddComponent<Image>(clarisseSendButton.gameObject), new Color(0f, 0f, 0f, 0.48f));
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
        HomeUiFactory.ConfigureCanvas(canvasObject, ReferenceWidth, ReferenceHeight);
    }

    private static void EnsureEventSystem()
    {
        HomeUiFactory.EnsureEventSystem(EventSystemName);
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
        SetClarisseLine(ClarisseDialogue.ChooseTapLine());
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

        SetClarisseLine(ClarisseDialogue.CreateReply(input));
        clarisseInputField.text = string.Empty;
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
        var nextCharacter = ClarisseDialogue.LoadRandomCharacter(clarisseCharacter);
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
}
