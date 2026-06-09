using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[ExecuteAlways]
public sealed class TitleSceneController : MonoBehaviour
{
    [SerializeField] private string nextSceneName = "HomeScene";

    private const string CanvasName = "TitleCanvas";
    private const string BackgroundName = "Background";
    private const string PanelName = "Panel";
    private const string TitleName = "Title";
    private const string SubtitleName = "Subtitle";
    private const string StartButtonName = "StartButton";
    private const string StartButtonLabelName = "Label";
    private const string MessageName = "Message";

    private bool showMissingSceneMessage;

    private Text titleText;
    private Text subtitleText;
    private Text messageText;
    private Button startButton;

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

    private void EnsureUi()
    {
        var canvasTransform = GetOrCreateCanvas();
        CreateOrUpdateBackground(canvasTransform);

        var panelTransform = GetOrCreateChild(canvasTransform, PanelName);
        var panelRect = GetOrAddComponent<RectTransform>(panelTransform.gameObject);
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(520f, 320f);
        panelRect.anchoredPosition = Vector2.zero;

        var panelImage = GetOrAddComponent<Image>(panelTransform.gameObject);
        panelImage.color = new Color(0.92f, 0.94f, 0.98f, 1f);

        titleText = CreateText(
            panelTransform,
            TitleName,
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0f, -62f),
            new Vector2(400f, 56f),
            42,
            FontStyle.Bold,
            TextAnchor.MiddleCenter,
            new Color(0.12f, 0.16f, 0.24f),
            "ThinQuest");

        subtitleText = CreateText(
            panelTransform,
            SubtitleName,
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0f, -130f),
            new Vector2(448f, 72f),
            18,
            FontStyle.Normal,
            TextAnchor.MiddleCenter,
            new Color(0.28f, 0.32f, 0.42f),
            "Think through enemy objections and strengthen your own ideas.");

        startButton = CreateButton(
            panelTransform,
            StartButtonName,
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0f, -28f),
            new Vector2(200f, 42f),
            "Start");

        startButton.onClick.RemoveListener(HandleStartPressed);
        startButton.onClick.AddListener(HandleStartPressed);

        messageText = CreateText(
            panelTransform,
            MessageName,
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(0f, 28f),
            new Vector2(448f, 44f),
            14,
            FontStyle.Normal,
            TextAnchor.MiddleCenter,
            new Color(0.55f, 0.2f, 0.16f),
            "HomeScene is not added yet. Create the next scene and the Start button will connect automatically.");

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
            subtitleText.text = "Think through enemy objections and strengthen your own ideas.";
        }

        if (messageText != null)
        {
            messageText.text = "HomeScene is not added yet. Create the next scene and the Start button will connect automatically.";
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
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        GetOrAddComponent<GraphicRaycaster>(canvasObject);
    }

    private void CreateOrUpdateBackground(Transform canvasTransform)
    {
        var background = GetOrCreateChild(canvasTransform, BackgroundName);
        var rect = GetOrAddComponent<RectTransform>(background.gameObject);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        var image = GetOrAddComponent<Image>(background.gameObject);
        image.color = new Color(0.11f, 0.12f, 0.18f, 1f);
    }

    private Button CreateButton(
        Transform parent,
        string objectName,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 anchoredPosition,
        Vector2 sizeDelta,
        string label)
    {
        var buttonTransform = GetOrCreateChild(parent, objectName);
        var rect = GetOrAddComponent<RectTransform>(buttonTransform.gameObject);
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;

        var image = GetOrAddComponent<Image>(buttonTransform.gameObject);
        image.color = new Color(0.2f, 0.37f, 0.76f, 1f);

        var button = GetOrAddComponent<Button>(buttonTransform.gameObject);
        var colors = button.colors;
        colors.normalColor = image.color;
        colors.highlightedColor = new Color(0.24f, 0.42f, 0.84f, 1f);
        colors.pressedColor = new Color(0.16f, 0.31f, 0.65f, 1f);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(0.35f, 0.35f, 0.4f, 0.7f);
        button.colors = colors;

        CreateText(
            buttonTransform,
            StartButtonLabelName,
            Vector2.zero,
            Vector2.one,
            new Vector2(0.5f, 0.5f),
            Vector2.zero,
            Vector2.zero,
            20,
            FontStyle.Bold,
            TextAnchor.MiddleCenter,
            Color.white,
            label);

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
