using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class HomeSceneController : MonoBehaviour
{
    [SerializeField] private string titleSceneName = "TitleScene";
    [SerializeField] private string themeInputSceneName = "ThemeInputScene";

    private bool showMissingThemeInputSceneMessage;
    private UserProfile profile;

    private void Start()
    {
        profile = UserDataManager.LoadProfile();
    }

    private void OnGUI()
    {
        profile = UserDataManager.LoadProfile();
        DrawBackground();
        DrawHomePanel();
    }

    private void DrawBackground()
    {
        var previousColor = GUI.color;
        GUI.color = new Color(0.09f, 0.11f, 0.09f, 1f);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = previousColor;
    }

    private void DrawHomePanel()
    {
        const float panelWidth = 760f;
        const float panelHeight = 540f;

        var panelRect = new Rect(
            (Screen.width - panelWidth) * 0.5f,
            (Screen.height - panelHeight) * 0.5f,
            panelWidth,
            panelHeight);

        var previousColor = GUI.color;
        GUI.color = new Color(0.82f, 0.74f, 0.56f, 1f);
        GUI.Box(panelRect, GUIContent.none);
        GUI.color = previousColor;

        var titleStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 34,
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(0.2f, 0.11f, 0.05f) }
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
            fontSize = 16,
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(0.24f, 0.12f, 0.05f) }
        };

        var messageStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 14,
            wordWrap = true,
            normal = { textColor = new Color(0.55f, 0.22f, 0.16f) }
        };

        GUI.Label(new Rect(panelRect.x, panelRect.y + 32f, panelRect.width, 48f), "ギルドホール", titleStyle);
        GUI.Label(
            new Rect(panelRect.x + 52f, panelRect.y + 88f, panelRect.width - 104f, 44f),
            "冒険者の記録を確認し、次の思考クエストへ向かいましょう。",
            bodyStyle);

        DrawProfile(panelRect, sectionStyle, bodyStyle);

        if (GUI.Button(new Rect(panelRect.x + 210f, panelRect.y + 458f, 160f, 40f), "クエスト開始"))
        {
            TryLoadThemeInputScene();
        }

        if (GUI.Button(new Rect(panelRect.x + 390f, panelRect.y + 458f, 160f, 40f), "門へ戻る"))
        {
            SceneManager.LoadScene(titleSceneName);
        }

        if (showMissingThemeInputSceneMessage)
        {
            GUI.Label(
                new Rect(panelRect.x + 70f, panelRect.y + 506f, panelRect.width - 140f, 24f),
                "ThemeInputScene がまだ登録されていません。次の試作段階でつながります。",
                messageStyle);
        }
    }

    private void DrawProfile(Rect panelRect, GUIStyle sectionStyle, GUIStyle bodyStyle)
    {
        var leftX = panelRect.x + 52f;
        var rightX = panelRect.x + 410f;
        var topY = panelRect.y + 150f;

        GUI.Label(new Rect(leftX, topY, 300f, 24f), "冒険者記録", sectionStyle);
        GUI.Label(
            new Rect(leftX, topY + 30f, 300f, 92f),
            $"ランク: {profile.Level}\n累計EXP: {profile.TotalExp}\n完了した試練: {profile.BattlesCompleted}",
            bodyStyle);

        var lastTheme = PlayerPrefs.GetString(ThemeInputSceneController.LastThemeKey, "まだ巻物はありません");
        var lastEnemy = PlayerPrefs.GetString(EnemySelectSceneController.LastEnemyKey, "まだ挑戦者はいません");
        GUI.Label(new Rect(rightX, topY, 300f, 24f), "直近のクエスト", sectionStyle);
        GUI.Label(
            new Rect(rightX, topY + 30f, 300f, 108f),
            $"巻物: {lastTheme}\n挑戦者: {lastEnemy}",
            bodyStyle);

        GUI.Label(new Rect(leftX, topY + 154f, 300f, 24f), "能力値", sectionStyle);
        for (var i = 0; i < profile.Abilities.Length; i++)
        {
            var ability = profile.Abilities[i];
            var column = i % 2;
            var row = i / 2;
            var x = leftX + column * 358f;
            var y = topY + 186f + row * 42f;

            GUI.Label(
                new Rect(x, y, 320f, 36f),
                $"{ability.Name}  Lv.{ability.Level}  EXP {ability.Exp}",
                bodyStyle);
        }
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
