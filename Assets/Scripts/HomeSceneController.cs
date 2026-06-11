using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class HomeSceneController : MonoBehaviour
{
    [SerializeField] private string titleSceneName = "TitleScene";
    [SerializeField] private string themeInputSceneName = "ThemeInputScene";
    [SerializeField] private Texture2D guildHallBackground;

    private bool showMissingThemeInputSceneMessage;
    private UserProfile profile;

    private void Start()
    {
        profile = UserDataManager.LoadProfile();
        LoadBackgroundIfNeeded();
    }

    private void OnGUI()
    {
        profile = UserDataManager.LoadProfile();
        LoadBackgroundIfNeeded();
        DrawBackground();
        DrawHomePanel();
    }

    private void DrawBackground()
    {
        var previousColor = GUI.color;

        if (guildHallBackground != null)
        {
            GUI.color = Color.white;
            GUI.DrawTexture(GetCoverRect(guildHallBackground), guildHallBackground);

            GUI.color = new Color(0.02f, 0.02f, 0.02f, 0.32f);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = previousColor;
            return;
        }

        GUI.color = new Color(0.09f, 0.11f, 0.09f, 1f);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = previousColor;
    }

    private void LoadBackgroundIfNeeded()
    {
        if (guildHallBackground != null)
        {
            return;
        }

        guildHallBackground = Resources.Load<Texture2D>("Backgrounds/GuildHallBackground");
    }

    private static Rect GetCoverRect(Texture2D texture)
    {
        var screenRatio = Screen.width / (float)Screen.height;
        var textureRatio = texture.width / (float)texture.height;

        if (textureRatio > screenRatio)
        {
            var width = Screen.height * textureRatio;
            return new Rect((Screen.width - width) * 0.5f, 0f, width, Screen.height);
        }

        var height = Screen.width / textureRatio;
        return new Rect(0f, (Screen.height - height) * 0.5f, Screen.width, height);
    }

    private void DrawHomePanel()
    {
        var titleStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 36,
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(0.94f, 0.88f, 0.74f) }
        };

        var bodyStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.UpperLeft,
            fontSize = 15,
            wordWrap = true,
            normal = { textColor = new Color(0.25f, 0.17f, 0.09f) }
        };

        var sectionStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.UpperLeft,
            fontSize = 15,
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(0.22f, 0.12f, 0.06f) }
        };

        var messageStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 14,
            wordWrap = true,
            normal = { textColor = new Color(0.55f, 0.22f, 0.16f) }
        };

        var margin = Mathf.Clamp(Screen.width * 0.035f, 24f, 48f);
        var top = Mathf.Clamp(Screen.height * 0.13f, 86f, 120f);
        var sideWidth = Mathf.Clamp(Screen.width * 0.27f, 260f, 340f);
        var sideHeight = Mathf.Max(360f, Screen.height - top - 122f);
        var leftPanel = new Rect(margin, top, sideWidth, sideHeight);
        var rightPanel = new Rect(Screen.width - margin - sideWidth, top, sideWidth, sideHeight);
        var actionPanelWidth = Mathf.Clamp(Screen.width * 0.46f, 360f, 520f);
        var actionPanel = new Rect(
            (Screen.width - actionPanelWidth) * 0.5f,
            Screen.height - 82f,
            actionPanelWidth,
            58f);

        GUI.Label(new Rect(0f, 28f, Screen.width, 52f), "ギルドホール", titleStyle);
        DrawProfile(leftPanel, sectionStyle, bodyStyle);
        DrawQuestSummary(rightPanel, sectionStyle, bodyStyle);
        DrawActionBar(actionPanel, messageStyle);
    }

    private void DrawProfile(Rect panelRect, GUIStyle sectionStyle, GUIStyle bodyStyle)
    {
        DrawSurface(panelRect, new Color(0.86f, 0.76f, 0.56f, 0.9f));

        var contentX = panelRect.x + 18f;
        var contentWidth = panelRect.width - 36f;
        var y = panelRect.y + 18f;

        GUI.Label(new Rect(contentX, y, contentWidth, 22f), "冒険者記録", sectionStyle);
        GUI.Label(
            new Rect(contentX, y + 30f, contentWidth, 74f),
            $"ランク: {profile.Level}\n累計EXP: {profile.TotalExp}\n完了した試練: {profile.BattlesCompleted}",
            bodyStyle);

        y += 124f;

        GUI.Label(new Rect(contentX, y, contentWidth, 22f), "能力値", sectionStyle);
        DrawAbilityRows(new Rect(contentX, y + 30f, contentWidth, panelRect.yMax - y - 48f), bodyStyle);
    }

    private void DrawQuestSummary(Rect panelRect, GUIStyle sectionStyle, GUIStyle bodyStyle)
    {
        DrawSurface(panelRect, new Color(0.86f, 0.76f, 0.56f, 0.9f));

        var contentX = panelRect.x + 18f;
        var contentWidth = panelRect.width - 36f;
        var y = panelRect.y + 18f;

        var lastTheme = PlayerPrefs.GetString(ThemeInputSceneController.LastThemeKey, "まだ巻物はありません");
        var lastEnemy = PlayerPrefs.GetString(EnemySelectSceneController.LastEnemyKey, "まだ挑戦者はいません");

        GUI.Label(new Rect(contentX, y, contentWidth, 22f), "直近のクエスト", sectionStyle);
        GUI.Label(
            new Rect(contentX, y + 30f, contentWidth, 118f),
            $"巻物:\n{lastTheme}\n\n挑戦者:\n{lastEnemy}",
            bodyStyle);

        y += 178f;

        GUI.Label(new Rect(contentX, y, contentWidth, 22f), "次の思考クエスト", sectionStyle);
        GUI.Label(
            new Rect(contentX, y + 30f, contentWidth, 116f),
            "中央の案内役から依頼を受け、巻物に主張を書いて試練へ進みます。",
            bodyStyle);
    }

    private void DrawAbilityRows(Rect rect, GUIStyle bodyStyle)
    {
        for (var i = 0; i < profile.Abilities.Length; i++)
        {
            var ability = profile.Abilities[i];
            var rowRect = new Rect(rect.x, rect.y + i * 42f, rect.width, 32f);

            var previousColor = GUI.color;
            GUI.color = new Color(0.56f, 0.38f, 0.18f, 0.2f);
            GUI.DrawTexture(rowRect, Texture2D.whiteTexture);
            GUI.color = previousColor;
            GUI.Label(
                new Rect(rowRect.x + 10f, rowRect.y + 6f, rowRect.width - 20f, 22f),
                $"{ability.Name}  Lv.{ability.Level}  EXP {ability.Exp}",
                bodyStyle);
        }
    }

    private void DrawActionBar(Rect panelRect, GUIStyle messageStyle)
    {
        DrawSurface(panelRect, new Color(0.16f, 0.12f, 0.08f, 0.78f));

        var buttonWidth = (panelRect.width - 48f) * 0.5f;
        var buttonY = panelRect.y + 11f;
        var previousBackgroundColor = GUI.backgroundColor;

        GUI.backgroundColor = new Color(0.63f, 0.39f, 0.14f, 1f);
        if (GUI.Button(new Rect(panelRect.x + 16f, buttonY, buttonWidth, 36f), "クエスト開始"))
        {
            TryLoadThemeInputScene();
        }

        GUI.backgroundColor = new Color(0.28f, 0.23f, 0.18f, 1f);
        if (GUI.Button(new Rect(panelRect.x + 32f + buttonWidth, buttonY, buttonWidth, 36f), "門へ戻る"))
        {
            SceneManager.LoadScene(titleSceneName);
        }

        GUI.backgroundColor = previousBackgroundColor;

        if (showMissingThemeInputSceneMessage)
        {
            GUI.Label(
                new Rect(panelRect.x, panelRect.y - 34f, panelRect.width, 24f),
                "ThemeInputScene がまだ登録されていません。次の試作段階でつながります。",
                messageStyle);
        }
    }

    private static void DrawSurface(Rect rect, Color color)
    {
        var previousColor = GUI.color;
        GUI.color = new Color(0f, 0f, 0f, 0.24f);
        GUI.DrawTexture(new Rect(rect.x + 3f, rect.y + 4f, rect.width, rect.height), Texture2D.whiteTexture);
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = new Color(0.98f, 0.86f, 0.58f, 0.55f);
        GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, 2f), Texture2D.whiteTexture);
        GUI.color = previousColor;
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
    }
}
