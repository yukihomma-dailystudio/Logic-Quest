using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class ResultSceneController : MonoBehaviour
{
    private const string BackgroundResourcePath = "Backgrounds/ResultLootExchangeShop";

    public const string LastAwardedAbilityKey = "ThinQuest.LastAwardedAbility";
    public const string LastAwardedExpKey = "ThinQuest.LastAwardedExp";
    public const string LastAnswerKey = "ThinQuest.LastAnswer";
    public const string LastEvaluationKey = "ThinQuest.LastEvaluation";

    [SerializeField] private string homeSceneName = "HomeScene";
    [SerializeField] private string themeInputSceneName = "ThemeInputScene";

    private UserProfile profile;
    private string selectedTheme = "まだ巻物はありません";
    private string selectedEnemy = "まだ挑戦者はいません";
    private string awardedAbility = "説明力";
    private string answerText = "まだ返答はありません";
    private string evaluationText = "評価はまだありません";
    private int awardedExp = 20;
    private Texture2D backgroundTexture;
    private Vector2 scrollPosition;

    private void Start()
    {
        backgroundTexture = Resources.Load<Texture2D>(BackgroundResourcePath);
        LoadResultData();
    }

    private void OnGUI()
    {
        LoadResultData();
        DrawBackground();
        DrawResultPanel();
    }

    private void LoadResultData()
    {
        profile = UserDataManager.LoadProfile();
        selectedTheme = PlayerPrefs.GetString(ThemeInputSceneController.LastThemeKey, selectedTheme);
        selectedEnemy = PlayerPrefs.GetString(EnemySelectSceneController.LastEnemyKey, selectedEnemy);
        awardedAbility = PlayerPrefs.GetString(LastAwardedAbilityKey, awardedAbility);
        answerText = PlayerPrefs.GetString(LastAnswerKey, answerText);
        evaluationText = PlayerPrefs.GetString(LastEvaluationKey, evaluationText);
        awardedExp = PlayerPrefs.GetInt(LastAwardedExpKey, awardedExp);
    }

    private void DrawBackground()
    {
        var previousColor = GUI.color;
        if (backgroundTexture != null)
        {
            DrawCoverTexture(new Rect(0f, 0f, Screen.width, Screen.height), backgroundTexture);
        }
        else
        {
            DrawSolidRect(new Rect(0f, 0f, Screen.width, Screen.height), new Color(0.07f, 0.08f, 0.08f, 1f));
        }

        DrawSolidRect(new Rect(Screen.width * 0.42f, 0f, Screen.width * 0.58f, Screen.height), new Color(0.02f, 0.03f, 0.03f, 0.22f));
        DrawSolidRect(new Rect(0f, 0f, Screen.width, 18f), new Color(0.7f, 0.52f, 0.25f, 0.48f));
        DrawSolidRect(new Rect(0f, Screen.height - 18f, Screen.width, 18f), new Color(0.7f, 0.52f, 0.25f, 0.48f));
        GUI.color = previousColor;
    }

    private void DrawResultPanel()
    {
        var wideLayout = Screen.width >= 760f;
        var panelX = wideLayout ? Mathf.Max(Screen.width * 0.45f, 390f) : 16f;
        var panelWidth = wideLayout ? Screen.width - panelX - 28f : Screen.width - 32f;
        var panelHeight = Mathf.Min(650f, Screen.height - 52f);
        panelWidth = Mathf.Clamp(panelWidth, 320f, 860f);
        panelHeight = Mathf.Max(420f, panelHeight);

        var panelRect = new Rect(
            wideLayout ? panelX : (Screen.width - panelWidth) * 0.5f,
            (Screen.height - panelHeight) * 0.5f,
            panelWidth,
            panelHeight);

        DrawParchmentFrame(panelRect);

        var titleStyle = CreateLabelStyle(30, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.98f, 0.88f, 0.56f, 1f));
        var subtitleStyle = CreateLabelStyle(14, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.9f, 0.78f, 0.5f, 1f));
        var sectionStyle = CreateLabelStyle(16, FontStyle.Bold, TextAnchor.UpperLeft, new Color(0.27f, 0.13f, 0.05f, 1f));
        var bodyStyle = CreateLabelStyle(15, FontStyle.Normal, TextAnchor.UpperLeft, new Color(0.22f, 0.14f, 0.07f, 1f));
        var smallStyle = CreateLabelStyle(13, FontStyle.Normal, TextAnchor.UpperLeft, new Color(0.32f, 0.21f, 0.12f, 1f));
        var metricLabelStyle = CreateLabelStyle(13, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.42f, 0.27f, 0.12f, 1f));
        var metricValueStyle = CreateLabelStyle(24, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.16f, 0.33f, 0.15f, 1f));
        var buttonStyle = CreateButtonStyle();

        GUI.Label(new Rect(panelRect.x + 14f, panelRect.y + 18f, panelRect.width - 28f, 34f), "今回のリザルト", titleStyle);
        GUI.Label(new Rect(panelRect.x + 14f, panelRect.y + 50f, panelRect.width - 28f, 22f), "今回の試練で得た報酬と記録", subtitleStyle);

        var padding = panelRect.width < 560f ? 18f : 34f;
        var contentX = panelRect.x + padding;
        var contentWidth = panelRect.width - padding * 2f;
        var buttonHeight = 46f;
        var buttonY = panelRect.y + panelRect.height - buttonHeight - 22f;
        var viewportRect = new Rect(contentX, panelRect.y + 96f, contentWidth, buttonY - panelRect.y - 112f);
        var compactPanel = panelRect.width < 620f;
        var contentHeight = compactPanel ? 760f : 560f;

        scrollPosition = GUI.BeginScrollView(
            viewportRect,
            scrollPosition,
            new Rect(0f, 0f, contentWidth - 18f, contentHeight),
            false,
            true);

        var innerWidth = contentWidth - 28f;
        var y = 0f;
        var cardGap = 12f;
        var cardWidth = (innerWidth - cardGap * 2f) / 3f;
        var abilityProgress = FindAbilityProgress(awardedAbility);

        DrawExpMetricCard(new Rect(0f, y, cardWidth, 92f), $"+{awardedExp}", metricLabelStyle, metricValueStyle);
        DrawMetricCard(new Rect(cardWidth + cardGap, y, cardWidth, 92f), "伸びた能力", awardedAbility, metricLabelStyle, metricValueStyle);
        DrawMetricCard(new Rect((cardWidth + cardGap) * 2f, y, cardWidth, 92f), "現在ランク", $"{profile.Level}", metricLabelStyle, metricValueStyle);
        y += 108f;

        if (compactPanel)
        {
            DrawSectionBox(new Rect(0f, y, innerWidth, 94f), "挑戦した巻物", selectedTheme, sectionStyle, bodyStyle);
            y += 106f;
            DrawSectionBox(new Rect(0f, y, innerWidth, 72f), "対峙した相手", selectedEnemy, sectionStyle, bodyStyle);
            y += 84f;
        }
        else
        {
            var halfWidth = (innerWidth - cardGap) * 0.5f;
            DrawSectionBox(new Rect(0f, y, halfWidth, 92f), "挑戦した巻物", selectedTheme, sectionStyle, bodyStyle);
            DrawSectionBox(new Rect(halfWidth + cardGap, y, halfWidth, 92f), "対峙した相手", selectedEnemy, sectionStyle, bodyStyle);
            y += 108f;
        }

        if (compactPanel)
        {
            DrawSectionBox(new Rect(0f, y, innerWidth, 128f), "あなたの返答", answerText, sectionStyle, bodyStyle);
            y += 144f;
            DrawSectionBox(new Rect(0f, y, innerWidth, 128f), "試練の講評", evaluationText, sectionStyle, bodyStyle);
            y += 144f;
            DrawRecordBox(new Rect(0f, y, innerWidth, 106f), awardedAbility, abilityProgress, sectionStyle, smallStyle);
        }
        else
        {
            var leftColumnWidth = (innerWidth - cardGap) * 0.56f;
            var rightColumnWidth = innerWidth - leftColumnWidth - cardGap;
            DrawSectionBox(new Rect(0f, y, leftColumnWidth, 138f), "あなたの返答", answerText, sectionStyle, bodyStyle);
            DrawSectionBox(new Rect(leftColumnWidth + cardGap, y, rightColumnWidth, 138f), "試練の講評", evaluationText, sectionStyle, bodyStyle);
            y += 154f;
            DrawRecordBox(new Rect(0f, y, innerWidth, 86f), awardedAbility, abilityProgress, sectionStyle, smallStyle);
        }

        GUI.EndScrollView();

        var buttonWidth = Mathf.Min(210f, (contentWidth - 16f) * 0.5f);
        var firstButtonX = panelRect.x + (panelRect.width - buttonWidth * 2f - 16f) * 0.5f;

        if (GUI.Button(new Rect(firstButtonX, buttonY, buttonWidth, buttonHeight), "ギルドへ戻る", buttonStyle))
        {
            SceneManager.LoadScene(homeSceneName);
        }

        if (GUI.Button(new Rect(firstButtonX + buttonWidth + 16f, buttonY, buttonWidth, buttonHeight), "次の巻物", buttonStyle))
        {
            SceneManager.LoadScene(themeInputSceneName);
        }
    }

    private AbilityProgress FindAbilityProgress(string abilityName)
    {
        if (profile.Abilities == null)
        {
            return new AbilityProgress(abilityName, string.Empty);
        }

        for (var i = 0; i < profile.Abilities.Length; i++)
        {
            var ability = profile.Abilities[i];
            if (ability.Name == abilityName)
            {
                return ability;
            }
        }

        return new AbilityProgress(abilityName, string.Empty);
    }

    private static void DrawSectionBox(Rect rect, string title, string body, GUIStyle titleStyle, GUIStyle bodyStyle)
    {
        DrawSolidRect(rect, new Color(0.98f, 0.92f, 0.78f, 0.88f));
        DrawSolidRect(new Rect(rect.x, rect.y, rect.width, 4f), new Color(0.58f, 0.38f, 0.14f, 0.82f));
        GUI.Label(new Rect(rect.x + 16f, rect.y + 14f, rect.width - 32f, 24f), title, titleStyle);
        GUI.Label(new Rect(rect.x + 16f, rect.y + 44f, rect.width - 32f, rect.height - 54f), body, bodyStyle);
    }

    private void DrawRecordBox(Rect rect, string abilityName, AbilityProgress abilityProgress, GUIStyle titleStyle, GUIStyle bodyStyle)
    {
        DrawSolidRect(rect, new Color(0.72f, 0.62f, 0.43f, 0.34f));
        DrawSolidRect(new Rect(rect.x, rect.y, 4f, rect.height), new Color(0.58f, 0.38f, 0.14f, 0.76f));
        GUI.Label(new Rect(rect.x + 16f, rect.y + 10f, rect.width - 32f, 24f), "冒険者記録", titleStyle);
        GUI.Label(
            new Rect(rect.x + 16f, rect.y + 38f, rect.width - 32f, rect.height - 44f),
            $"累計EXP: {profile.TotalExp}    完了した試練: {profile.BattlesCompleted}    {abilityName}: Lv {abilityProgress.Level} / EXP {abilityProgress.Exp}",
            bodyStyle);
    }

    private static void DrawMetricCard(Rect rect, string label, string value, GUIStyle labelStyle, GUIStyle valueStyle)
    {
        DrawSolidRect(rect, new Color(0.99f, 0.94f, 0.78f, 0.96f));
        DrawSolidRect(new Rect(rect.x, rect.y + rect.height - 5f, rect.width, 5f), new Color(0.56f, 0.38f, 0.16f, 0.8f));
        GUI.Label(new Rect(rect.x + 8f, rect.y + 14f, rect.width - 16f, 22f), label, labelStyle);
        GUI.Label(new Rect(rect.x + 8f, rect.y + 42f, rect.width - 16f, 34f), value, valueStyle);
    }

    private static void DrawExpMetricCard(Rect rect, string value, GUIStyle labelStyle, GUIStyle valueStyle)
    {
        DrawMetricCard(rect, "獲得EXP", value, labelStyle, valueStyle);

        var textRect = new Rect(rect.x + 8f, rect.y + 42f, rect.width - 16f, 34f);
        var highlightWidth = Mathf.Max(28f, textRect.width * 0.28f);
        var progress = Mathf.Repeat(Time.time * 0.78f, 1f);
        var highlightX = Mathf.Lerp(-highlightWidth, textRect.width, progress);

        DrawSolidRect(
            new Rect(textRect.x + highlightX, rect.y + 10f, highlightWidth, rect.height - 20f),
            new Color(1f, 0.96f, 0.58f, 0.18f));
        DrawTextHighlight(textRect, value, valueStyle, highlightX, highlightWidth);
    }

    private static void DrawTextHighlight(Rect textRect, string value, GUIStyle baseStyle, float highlightX, float highlightWidth)
    {
        var highlightStyle = new GUIStyle(baseStyle)
        {
            normal = { textColor = new Color(1f, 0.98f, 0.62f, 1f) }
        };

        GUI.BeginGroup(new Rect(textRect.x + highlightX, textRect.y, highlightWidth, textRect.height));
        GUI.Label(new Rect(-highlightX, 0f, textRect.width, textRect.height), value, highlightStyle);
        GUI.EndGroup();
    }

    private static GUIStyle CreateLabelStyle(int fontSize, FontStyle fontStyle, TextAnchor alignment, Color textColor)
    {
        return new GUIStyle(GUI.skin.label)
        {
            alignment = alignment,
            fontSize = fontSize,
            fontStyle = fontStyle,
            wordWrap = true,
            normal = { textColor = textColor }
        };
    }

    private static GUIStyle CreateButtonStyle()
    {
        var style = new GUIStyle(GUI.skin.button)
        {
            fontSize = 18,
            fontStyle = FontStyle.Bold,
            wordWrap = true,
            normal = { textColor = new Color(0.24f, 0.13f, 0.06f, 1f) },
            hover = { textColor = new Color(0.17f, 0.09f, 0.04f, 1f) },
            active = { textColor = new Color(0.17f, 0.09f, 0.04f, 1f) }
        };

        return style;
    }

    private static void DrawParchmentFrame(Rect rect)
    {
        DrawSolidRect(Offset(rect, 8f, 10f), new Color(0f, 0f, 0f, 0.42f));
        DrawSolidRect(rect, new Color(0.3f, 0.17f, 0.07f, 0.98f));
        DrawSolidRect(new Rect(rect.x + 8f, rect.y + 8f, rect.width - 16f, rect.height - 16f), new Color(0.74f, 0.55f, 0.28f, 1f));
        DrawSolidRect(new Rect(rect.x + 16f, rect.y + 16f, rect.width - 32f, rect.height - 32f), new Color(0.96f, 0.88f, 0.67f, 0.94f));
        DrawSolidRect(new Rect(rect.x + 18f, rect.y + 18f, rect.width - 36f, 64f), new Color(0.35f, 0.2f, 0.08f, 0.96f));
        DrawSolidRect(new Rect(rect.x + 18f, rect.y + rect.height - 24f, rect.width - 36f, 6f), new Color(0.38f, 0.22f, 0.08f, 0.72f));
        DrawSolidRect(new Rect(rect.x + 26f, rect.y + 90f, rect.width - 52f, 2f), new Color(0.53f, 0.35f, 0.12f, 0.55f));
    }

    private static void DrawCoverTexture(Rect targetRect, Texture texture)
    {
        var sourceAspect = (float)texture.width / texture.height;
        var targetAspect = targetRect.width / targetRect.height;
        var scale = targetAspect > sourceAspect ? targetRect.width / texture.width : targetRect.height / texture.height;
        var drawWidth = texture.width * scale;
        var drawHeight = texture.height * scale;
        var drawRect = new Rect(
            targetRect.x + (targetRect.width - drawWidth) * 0.5f,
            targetRect.y + (targetRect.height - drawHeight) * 0.5f,
            drawWidth,
            drawHeight);

        GUI.DrawTexture(drawRect, texture);
    }

    private static Rect Offset(Rect rect, float x, float y)
    {
        return new Rect(rect.x + x, rect.y + y, rect.width, rect.height);
    }

    private static void DrawSolidRect(Rect rect, Color color)
    {
        var previousColor = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = previousColor;
    }
}
