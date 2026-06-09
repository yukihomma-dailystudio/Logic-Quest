using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class ResultSceneController : MonoBehaviour
{
    public const string LastAwardedAbilityKey = "ThinQuest.LastAwardedAbility";
    public const string LastAwardedExpKey = "ThinQuest.LastAwardedExp";

    [SerializeField] private string homeSceneName = "HomeScene";
    [SerializeField] private string themeInputSceneName = "ThemeInputScene";

    private UserProfile profile;
    private string selectedTheme = "まだ巻物はありません";
    private string selectedEnemy = "まだ挑戦者はいません";
    private string awardedAbility = "説明力";
    private int awardedExp = 20;

    private void Start()
    {
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
        awardedExp = PlayerPrefs.GetInt(LastAwardedExpKey, awardedExp);
    }

    private void DrawBackground()
    {
        var previousColor = GUI.color;
        GUI.color = new Color(0.08f, 0.08f, 0.07f, 1f);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = previousColor;
    }

    private void DrawResultPanel()
    {
        const float panelWidth = 720f;
        const float panelHeight = 500f;

        var panelRect = new Rect(
            (Screen.width - panelWidth) * 0.5f,
            (Screen.height - panelHeight) * 0.5f,
            panelWidth,
            panelHeight);

        var previousColor = GUI.color;
        GUI.color = new Color(0.84f, 0.76f, 0.58f, 1f);
        GUI.Box(panelRect, GUIContent.none);
        GUI.color = previousColor;

        var titleStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 32,
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(0.2f, 0.11f, 0.05f) }
        };

        var sectionStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.UpperLeft,
            fontSize = 16,
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(0.24f, 0.12f, 0.05f) }
        };

        var bodyStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.UpperLeft,
            fontSize = 15,
            wordWrap = true,
            normal = { textColor = new Color(0.23f, 0.15f, 0.08f) }
        };

        var rewardStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 18,
            fontStyle = FontStyle.Bold,
            wordWrap = true,
            normal = { textColor = new Color(0.2f, 0.3f, 0.13f) }
        };

        GUI.Label(new Rect(panelRect.x, panelRect.y + 26f, panelRect.width, 42f), "試練の戦果", titleStyle);

        var contentX = panelRect.x + 54f;
        var contentWidth = panelRect.width - 108f;

        GUI.Label(new Rect(contentX, panelRect.y + 88f, contentWidth, 24f), "挑戦した巻物", sectionStyle);
        GUI.Label(new Rect(contentX, panelRect.y + 116f, contentWidth, 48f), selectedTheme, bodyStyle);

        GUI.Label(new Rect(contentX, panelRect.y + 174f, contentWidth, 24f), "対峙した相手", sectionStyle);
        GUI.Label(new Rect(contentX, panelRect.y + 202f, contentWidth, 30f), selectedEnemy, bodyStyle);

        GUI.Label(
            new Rect(contentX, panelRect.y + 252f, contentWidth, 36f),
            $"{awardedAbility} EXP +{awardedExp}",
            rewardStyle);

        GUI.Label(new Rect(contentX, panelRect.y + 312f, contentWidth, 24f), "冒険者記録", sectionStyle);
        GUI.Label(
            new Rect(contentX, panelRect.y + 340f, contentWidth, 54f),
            $"ランク: {profile.Level}\n累計EXP: {profile.TotalExp}\n完了した試練: {profile.BattlesCompleted}",
            bodyStyle);

        if (GUI.Button(new Rect(panelRect.x + 190f, panelRect.y + 424f, 150f, 38f), "ギルドへ戻る"))
        {
            SceneManager.LoadScene(homeSceneName);
        }

        if (GUI.Button(new Rect(panelRect.x + 380f, panelRect.y + 424f, 150f, 38f), "次の巻物"))
        {
            SceneManager.LoadScene(themeInputSceneName);
        }
    }
}
