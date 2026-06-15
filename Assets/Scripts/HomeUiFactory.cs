using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

internal static class HomeUiFactory
{
    private static Sprite roundedPanelSprite;

    public static void ConfigureCanvas(GameObject canvasObject, float referenceWidth, float referenceHeight)
    {
        var canvas = GetOrAddComponent<Canvas>(canvasObject);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = GetOrAddComponent<CanvasScaler>(canvasObject);
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(referenceWidth, referenceHeight);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        GetOrAddComponent<GraphicRaycaster>(canvasObject);
    }

    public static void EnsureEventSystem(string eventSystemName)
    {
        if (Object.FindObjectOfType<EventSystem>() != null)
        {
            return;
        }

        var eventSystemObject = new GameObject(eventSystemName);
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<StandaloneInputModule>();
    }

    public static RawImage CreateRawImage(Transform parent, string objectName)
    {
        var child = GetOrCreateChild(parent, objectName);
        return GetOrAddComponent<RawImage>(child.gameObject);
    }

    public static Text CreateText(
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

    public static Button CreateTransparentButton(
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

    public static InputField CreateInputField(
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

    public static void ConfigureRoundedImage(Image image, Color color)
    {
        image.sprite = GetRoundedPanelSprite();
        image.type = Image.Type.Sliced;
        image.color = color;
        image.raycastTarget = true;
    }

    public static Sprite GetRoundedPanelSprite()
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

    public static Transform GetOrCreateChild(Transform parent, string objectName)
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

    public static void ConfigureFullScreen(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.localScale = Vector3.one;
    }

    public static void ConfigureCharacterRect(RectTransform rect)
    {
        rect.anchorMin = new Vector2(0.82f, 0f);
        rect.anchorMax = new Vector2(0.82f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(430f, 573f);
        rect.localScale = Vector3.one;
    }

    public static void ConfigureReferenceRect(RectTransform rect, float x, float yFromTop, float width, float height)
    {
        rect.anchorMin = new Vector2(x, 1f - yFromTop - height);
        rect.anchorMax = new Vector2(x + width, 1f - yFromTop);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.localScale = Vector3.one;
    }

    public static void ConfigureReferenceRectFromBottom(RectTransform rect, float x, float yFromBottom, float width, float height)
    {
        rect.anchorMin = new Vector2(x, yFromBottom);
        rect.anchorMax = new Vector2(x + width, yFromBottom + height);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.localScale = Vector3.one;
    }

    public static T GetOrAddComponent<T>(GameObject target) where T : Component
    {
        var component = target.GetComponent<T>();
        return component != null ? component : target.AddComponent<T>();
    }
}
