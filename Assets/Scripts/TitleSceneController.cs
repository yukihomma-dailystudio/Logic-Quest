using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[ExecuteAlways]
public sealed class TitleSceneController : MonoBehaviour
{
    [SerializeField] private string nextSceneName = "HomeScene";
    [SerializeField] private string backgroundResourcePath = "Backgrounds/GuildEntranceBackground";

    private const string CanvasName = "TitleCanvas";
    private const string BackgroundName = "Background";
    private const string CloudLayerName = "CloudLayer";
    private const string CloudNamePrefix = "Cloud";
    private const string PanelName = "Panel";
    private const string TitleName = "Title";
    private const string SubtitleName = "Subtitle";
    private const string StartButtonName = "StartButton";
    private const string ExitButtonName = "ExitButton";
    private const string SettingsButtonName = "SettingsButton";
    private const string StartButtonLabelName = "Label";
    private const string MessageName = "Message";
    private const float ReferenceWidth = 1920f;
    private const float ReferenceHeight = 1080f;
    private const int CloudCount = 4;

    private static Sprite cloudSprite;

    private bool showMissingSceneMessage;

    private Text titleText;
    private Text subtitleText;
    private Text messageText;
    private Button startButton;
    private Button exitButton;
    private Button settingsButton;
    private RectTransform[] cloudRects;

    private void OnEnable()
    {
        EnsureUi();
        RefreshUi();
    }

    private void OnValidate()
    {
        EnsureUi();
        RefreshUi();
    }

    private void Start()
    {
        EnsureUi();
        RefreshUi();
    }

    private void Update()
    {
        AnimateClouds();
    }

    private void EnsureUi()
    {
        var canvasTransform = GetOrCreateCanvas();
        CreateOrUpdateBackground(canvasTransform);
        CreateOrUpdateCloudLayer(canvasTransform);

        var panelTransform = GetOrCreateChild(canvasTransform, PanelName);
        var panelRect = GetOrAddComponent<RectTransform>(panelTransform.gameObject);
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        panelRect.sizeDelta = Vector2.zero;
        panelRect.anchoredPosition = Vector2.zero;

        var panelImage = GetOrAddComponent<Image>(panelTransform.gameObject);
        panelImage.color = Color.clear;
        panelImage.raycastTarget = false;

        titleText = CreateText(
            panelTransform,
            TitleName,
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0f, -62f),
            new Vector2(520f, 64f),
            42,
            FontStyle.Bold,
            TextAnchor.MiddleCenter,
            Color.white,
            "ThinQuest");
        AddTextEffects(titleText, true);

        subtitleText = CreateText(
            panelTransform,
            SubtitleName,
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0f, -130f),
            new Vector2(680f, 72f),
            18,
            FontStyle.Normal,
            TextAnchor.MiddleCenter,
            Color.white,
            "ギルドに入り、思考の試練で主張を鍛えよう。");
        AddTextEffects(subtitleText, false);

        startButton = CreateButton(
            panelTransform,
            StartButtonName,
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0f, 260f),
            new Vector2(320f, 72f),
            "ギルドにはいる",
            30);

        startButton.onClick.RemoveListener(HandleStartPressed);
        startButton.onClick.AddListener(HandleStartPressed);

        exitButton = CreateButton(
            panelTransform,
            ExitButtonName,
            new Vector2(0f, 0f),
            new Vector2(0f, 0f),
            new Vector2(0f, 0f),
            new Vector2(56f, 42f),
            new Vector2(180f, 56f),
            "終了",
            24);

        exitButton.onClick.RemoveListener(HandleExitPressed);
        exitButton.onClick.AddListener(HandleExitPressed);

        settingsButton = CreateButton(
            panelTransform,
            SettingsButtonName,
            new Vector2(1f, 0f),
            new Vector2(1f, 0f),
            new Vector2(1f, 0f),
            new Vector2(-56f, 42f),
            new Vector2(180f, 56f),
            "設定",
            24);

        settingsButton.onClick.RemoveListener(HandleSettingsPressed);
        settingsButton.onClick.AddListener(HandleSettingsPressed);

        messageText = CreateText(
            panelTransform,
            MessageName,
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(0f, 116f),
            new Vector2(680f, 44f),
            18,
            FontStyle.Normal,
            TextAnchor.MiddleCenter,
            Color.white,
            "HomeScene がまだ登録されていません。次のシーンを作ると自動でつながります。");
        AddTextEffects(messageText, false);

        EnsureEventSystem();
    }

    private void RefreshUi()
    {
        if (titleText != null)
        {
            titleText.text = "ThinQuest";
        }

        if (subtitleText != null)
        {
            subtitleText.text = "ギルドに入り、思考の試練で主張を鍛えよう。";
        }

        if (messageText != null)
        {
            messageText.text = "HomeScene がまだ登録されていません。次のシーンを作ると自動でつながります。";
            messageText.enabled = showMissingSceneMessage;
        }
    }

    private void HandleStartPressed()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        TryLoadNextScene();
    }

    private void TryLoadNextScene()
    {
        if (Application.CanStreamedLevelBeLoaded(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
            return;
        }

        showMissingSceneMessage = true;
        RefreshUi();
        Debug.LogWarning($"Scene '{nextSceneName}' is not available yet.");
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

    private void ConfigureCanvas(GameObject canvasObject)
    {
        var canvas = GetOrAddComponent<Canvas>(canvasObject);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.pixelPerfect = true;

        var scaler = GetOrAddComponent<CanvasScaler>(canvasObject);
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(ReferenceWidth, ReferenceHeight);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        GetOrAddComponent<GraphicRaycaster>(canvasObject);
    }

    private void CreateOrUpdateBackground(Transform canvasTransform)
    {
        var background = GetOrCreateChild(canvasTransform, BackgroundName);
        var rect = GetOrAddComponent<RectTransform>(background.gameObject);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        var image = GetOrAddComponent<Image>(background.gameObject);
        image.color = Color.white;
        image.sprite = Resources.Load<Sprite>(backgroundResourcePath);
        image.type = Image.Type.Simple;
        image.preserveAspect = false;
        image.raycastTarget = false;

        var aspectFitter = GetOrAddComponent<AspectRatioFitter>(background.gameObject);
        aspectFitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;

        if (image.sprite == null)
        {
            image.color = new Color(0.08f, 0.09f, 0.08f, 1f);
            aspectFitter.enabled = false;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return;
        }

        aspectFitter.enabled = true;
        aspectFitter.aspectRatio = image.sprite.rect.width / image.sprite.rect.height;
    }

    private void CreateOrUpdateCloudLayer(Transform canvasTransform)
    {
        var cloudLayer = GetOrCreateChild(canvasTransform, CloudLayerName);
        var layerRect = GetOrAddComponent<RectTransform>(cloudLayer.gameObject);
        layerRect.anchorMin = Vector2.zero;
        layerRect.anchorMax = Vector2.one;
        layerRect.offsetMin = Vector2.zero;
        layerRect.offsetMax = Vector2.zero;
        layerRect.pivot = new Vector2(0.5f, 0.5f);

        cloudRects = new RectTransform[CloudCount];
        for (var i = 0; i < CloudCount; i++)
        {
            var cloud = GetOrCreateChild(cloudLayer, $"{CloudNamePrefix}{i + 1}");
            var cloudRect = GetOrAddComponent<RectTransform>(cloud.gameObject);
            cloudRect.anchorMin = new Vector2(0f, 1f);
            cloudRect.anchorMax = new Vector2(0f, 1f);
            cloudRect.pivot = new Vector2(0.5f, 0.5f);
            cloudRect.sizeDelta = GetCloudSize(i);
            cloudRect.localScale = Vector3.one;

            var image = GetOrAddComponent<Image>(cloud.gameObject);
            image.sprite = GetCloudSprite();
            image.type = Image.Type.Simple;
            image.preserveAspect = true;
            image.color = new Color(1f, 1f, 1f, GetCloudAlpha(i));
            image.raycastTarget = false;

            cloudRects[i] = cloudRect;
        }

        cloudLayer.SetSiblingIndex(1);
        AnimateClouds();
    }

    private Button CreateButton(
        Transform parent,
        string objectName,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 anchoredPosition,
        Vector2 sizeDelta,
        string label,
        int fontSize)
    {
        var buttonTransform = GetOrCreateChild(parent, objectName);
        var rect = GetOrAddComponent<RectTransform>(buttonTransform.gameObject);
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;

        var image = GetOrAddComponent<Image>(buttonTransform.gameObject);
        image.color = Color.clear;
        image.raycastTarget = true;

        var button = GetOrAddComponent<Button>(buttonTransform.gameObject);
        var colors = button.colors;
        colors.normalColor = image.color;
        colors.highlightedColor = Color.clear;
        colors.pressedColor = Color.clear;
        colors.selectedColor = Color.clear;
        colors.disabledColor = Color.clear;
        button.colors = colors;

        var text = CreateText(
            buttonTransform,
            StartButtonLabelName,
            Vector2.zero,
            Vector2.one,
            new Vector2(0.5f, 0.5f),
            Vector2.zero,
            Vector2.zero,
            fontSize,
            FontStyle.Bold,
            TextAnchor.MiddleCenter,
            Color.white,
            label);
        AddTextEffects(text, true);

        return button;
    }

    private Text CreateText(
        Transform parent,
        string objectName,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 anchoredPosition,
        Vector2 sizeDelta,
        int fontSize,
        FontStyle fontStyle,
        TextAnchor alignment,
        Color color,
        string content)
    {
        var textTransform = GetOrCreateChild(parent, objectName);
        var rect = GetOrAddComponent<RectTransform>(textTransform.gameObject);
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;
        rect.offsetMin = sizeDelta == Vector2.zero ? Vector2.zero : rect.offsetMin;
        rect.offsetMax = sizeDelta == Vector2.zero ? Vector2.zero : rect.offsetMax;

        var text = GetOrAddComponent<Text>(textTransform.gameObject);
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.alignment = alignment;
        text.color = color;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.text = content;

        return text;
    }

    private static void AddTextEffects(Text text, bool strong)
    {
        if (text == null)
        {
            return;
        }

        var shadow = GetOrAddExactComponent<Shadow, Outline>(text.gameObject);
        shadow.effectColor = new Color(0f, 0f, 0f, strong ? 0.85f : 0.7f);
        shadow.effectDistance = strong ? new Vector2(4f, -4f) : new Vector2(3f, -3f);

        var outline = GetOrAddComponent<Outline>(text.gameObject);
        outline.effectColor = new Color(0.05f, 0.035f, 0.02f, strong ? 0.95f : 0.85f);
        outline.effectDistance = strong ? new Vector2(3f, -3f) : new Vector2(2f, -2f);
    }

    private void AnimateClouds()
    {
        if (cloudRects == null)
        {
            return;
        }

        var time = Application.isPlaying ? Time.time : 0f;
        for (var i = 0; i < cloudRects.Length; i++)
        {
            var cloudRect = cloudRects[i];
            if (cloudRect == null)
            {
                continue;
            }

            var width = cloudRect.sizeDelta.x;
            var travelWidth = ReferenceWidth + width + 360f;
            var x = Mathf.Repeat(GetCloudStartOffset(i) + time * GetCloudSpeed(i), travelWidth) - width - 180f;
            cloudRect.anchoredPosition = new Vector2(x, -GetCloudTopOffset(i));
        }
    }

    private static Vector2 GetCloudSize(int index)
    {
        return index switch
        {
            0 => new Vector2(360f, 110f),
            1 => new Vector2(280f, 86f),
            2 => new Vector2(430f, 128f),
            _ => new Vector2(320f, 96f),
        };
    }

    private static float GetCloudAlpha(int index)
    {
        return index switch
        {
            0 => 0.18f,
            1 => 0.14f,
            2 => 0.11f,
            _ => 0.16f,
        };
    }

    private static float GetCloudSpeed(int index)
    {
        return index switch
        {
            0 => 7f,
            1 => 11f,
            2 => 4.5f,
            _ => 8.5f,
        };
    }

    private static float GetCloudStartOffset(int index)
    {
        return index switch
        {
            0 => 80f,
            1 => 520f,
            2 => 980f,
            _ => 1420f,
        };
    }

    private static float GetCloudTopOffset(int index)
    {
        return index switch
        {
            0 => 110f,
            1 => 185f,
            2 => 72f,
            _ => 245f,
        };
    }

    private static Sprite GetCloudSprite()
    {
        if (cloudSprite != null)
        {
            return cloudSprite;
        }

        const int width = 256;
        const int height = 96;
        var texture = new Texture2D(width, height, TextureFormat.ARGB32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };

        var pixels = new Color32[width * height];
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var alpha = Mathf.Max(
                    CloudBlobAlpha(x, y, 58f, 56f, 48f, 26f),
                    CloudBlobAlpha(x, y, 102f, 44f, 60f, 34f),
                    CloudBlobAlpha(x, y, 154f, 50f, 64f, 30f),
                    CloudBlobAlpha(x, y, 202f, 58f, 46f, 22f));
                pixels[y * width + x] = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, (byte)(alpha * 255f));
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply();

        cloudSprite = Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), 100f);
        return cloudSprite;
    }

    private static float CloudBlobAlpha(float x, float y, float centerX, float centerY, float radiusX, float radiusY)
    {
        var normalizedX = (x - centerX) / radiusX;
        var normalizedY = (y - centerY) / radiusY;
        var distance = normalizedX * normalizedX + normalizedY * normalizedY;
        return Mathf.Clamp01((1f - distance) * 1.4f);
    }

    private void HandleExitPressed()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        Application.Quit();
    }

    private void HandleSettingsPressed()
    {
        showMissingSceneMessage = true;
        if (messageText != null)
        {
            messageText.text = "設定画面はまだ準備中です。";
            messageText.enabled = true;
        }
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

    private static T GetOrAddComponent<T>(GameObject target) where T : Component
    {
        if (!target.TryGetComponent<T>(out var component))
        {
            component = target.AddComponent<T>();
        }

        return component;
    }

    private static TBase GetOrAddExactComponent<TBase, TExclude>(GameObject target)
        where TBase : Component
        where TExclude : TBase
    {
        foreach (var component in target.GetComponents<TBase>())
        {
            if (!(component is TExclude))
            {
                return component;
            }
        }

        return target.AddComponent<TBase>();
    }

    private static void EnsureEventSystem()
    {
        if (FindObjectOfType<EventSystem>() != null)
        {
            return;
        }

        var eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        eventSystemObject.hideFlags = HideFlags.None;
    }
}
