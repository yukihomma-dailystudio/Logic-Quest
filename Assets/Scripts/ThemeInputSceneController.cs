using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class ThemeInputSceneController : MonoBehaviour
{
    public const string LastThemeKey = "ThinQuest.LastTheme";

    [SerializeField] private string homeSceneName = "HomeScene";
    [SerializeField] private string enemySelectSceneName = "EnemySelectScene";
    [SerializeField] private Texture2D noticeBoardBackground;

    private string themeText = "";
    private bool showMissingThemeMessage;
    private bool showMissingEnemySelectSceneMessage;

    private void Start()
    {
        themeText = PlayerPrefs.GetString(LastThemeKey, themeText);
        if (noticeBoardBackground == null)
        {
            noticeBoardBackground = Resources.Load<Texture2D>("Backgrounds/QuestNoticeBoardBackground");
        }
    }

    private void OnGUI()
    {
        DrawBackground();
        DrawThemePanel();
    }

    private void DrawBackground()
    {
        if (noticeBoardBackground != null)
        {
            GUI.DrawTexture(
                new Rect(0f, 0f, Screen.width, Screen.height),
                noticeBoardBackground,
                ScaleMode.ScaleAndCrop,
                false);
            return;
        }

        var previousColor = GUI.color;
        GUI.color = new Color(0.08f, 0.09f, 0.08f, 1f);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = previousColor;
    }

    private void DrawThemePanel()
    {
        var paperRect = GetScaledBackgroundRect(630f, 255f, 420f, 468f);
        var titleRect = GetScaledBackgroundRect(747f, 314f, 175f, 35f);
        var inputRect = GetScaledBackgroundRect(694f, 381f, 280f, 205f);
        var challengeButtonRect = GetScaledBackgroundRect(668f, 628f, 230f, 52f);
        var messageRect = GetScaledBackgroundRect(680f, 690f, 300f, 30f);

        var titleStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = Mathf.Clamp(Mathf.RoundToInt(titleRect.height * 0.6f), 18, 28),
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(0.2f, 0.11f, 0.05f) }
        };

        var inputStyle = new GUIStyle(GUI.skin.textArea)
        {
            fontSize = Mathf.Clamp(Mathf.RoundToInt(inputRect.height * 0.08f), 14, 20),
            wordWrap = true,
            padding = new RectOffset(14, 14, 10, 10),
            normal =
            {
                background = null,
                textColor = new Color(0.22f, 0.13f, 0.07f)
            },
            focused =
            {
                background = null,
                textColor = new Color(0.22f, 0.13f, 0.07f)
            }
        };

        var messageStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = Mathf.Clamp(Mathf.RoundToInt(paperRect.height * 0.035f), 12, 16),
            wordWrap = true,
            normal = { textColor = new Color(0.55f, 0.2f, 0.16f) }
        };

        var previousColor = GUI.color;
        GUI.color = new Color(1f, 0.96f, 0.84f, 0.18f);
        GUI.DrawTexture(inputRect, Texture2D.whiteTexture);
        GUI.color = previousColor;

        GUI.Label(titleRect, "今日の出来事", titleStyle);

        GUI.SetNextControlName("ThemeInput");
        themeText = GUI.TextArea(
            inputRect,
            themeText,
            180,
            inputStyle);

        if (GUI.Button(GetSafeBackButtonRect(), "戻る"))
        {
            SceneManager.LoadScene(homeSceneName);
        }

        if (GUI.Button(challengeButtonRect, "今日の試練を選ぶ"))
        {
            TryChooseEnemy();
        }

        if (showMissingThemeMessage)
        {
            GUI.Label(
                messageRect,
                "今日の出来事を一言だけ書いてください。",
                messageStyle);
        }

        if (showMissingEnemySelectSceneMessage)
        {
            GUI.Label(
                messageRect,
                "EnemySelectScene がまだ登録されていません。戦闘導線は自動でつながります。",
                messageStyle);
        }
    }

    private static Rect GetScaledBackgroundRect(float x, float y, float width, float height)
    {
        const float sourceWidth = 1680f;
        const float sourceHeight = 920f;

        var scale = Mathf.Max(Screen.width / sourceWidth, Screen.height / sourceHeight);
        var drawnWidth = sourceWidth * scale;
        var drawnHeight = sourceHeight * scale;
        var offsetX = (Screen.width - drawnWidth) * 0.5f;
        var offsetY = (Screen.height - drawnHeight) * 0.5f;

        return new Rect(
            offsetX + x * scale,
            offsetY + y * scale,
            width * scale,
            height * scale);
    }

    private static Rect GetSafeBackButtonRect()
    {
        var width = Mathf.Clamp(Screen.width * 0.11f, 86f, 128f);
        var height = Mathf.Clamp(Screen.height * 0.05f, 32f, 44f);
        return new Rect(24f, 24f, width, height);
    }

    private void TryChooseEnemy()
    {
        themeText = themeText.Trim();
        showMissingThemeMessage = string.IsNullOrEmpty(themeText);
        showMissingEnemySelectSceneMessage = false;

        if (showMissingThemeMessage)
        {
            return;
        }

        PlayerPrefs.SetString(LastThemeKey, themeText);
        PlayerPrefs.Save();

        if (Application.CanStreamedLevelBeLoaded(enemySelectSceneName))
        {
            SceneManager.LoadScene(enemySelectSceneName);
            return;
        }

        showMissingEnemySelectSceneMessage = true;
        Debug.LogWarning($"Scene '{enemySelectSceneName}' is not available yet.");
    }
}
