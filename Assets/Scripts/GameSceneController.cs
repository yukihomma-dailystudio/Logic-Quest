using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class GameSceneController : MonoBehaviour
{
    [SerializeField] private string homeSceneName = "HomeScene";

    private readonly string[] responses =
    {
        "巻物の主張をもっと明確にする。",
        "より強い根拠で答える。",
        "弱点を認めて、主張を鍛え直す。"
    };

    private string selectedTheme = "毎日少しずつ考えを鍛えるクエスト";
    private string selectedEnemy = "見習いスライム";
    private string enemyObjection = "この主張を一文でわかりやすく説明できますか？";
    private int selectedResponse = -1;

    private void Start()
    {
        selectedTheme = PlayerPrefs.GetString(ThemeInputSceneController.LastThemeKey, selectedTheme);
        selectedEnemy = PlayerPrefs.GetString(EnemySelectSceneController.LastEnemyKey, selectedEnemy);
        enemyObjection = GetEnemyObjection(selectedEnemy);
    }

    private void OnGUI()
    {
        DrawBackground();
        DrawGamePanel();
    }

    private void DrawBackground()
    {
        var previousColor = GUI.color;
        GUI.color = new Color(0.08f, 0.07f, 0.08f, 1f);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = previousColor;
    }

    private void DrawGamePanel()
    {
        const float panelWidth = 700f;
        const float panelHeight = 480f;

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
            fontSize = 30,
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
            normal = { textColor = new Color(0.22f, 0.15f, 0.08f) }
        };

        var resultStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 15,
            wordWrap = true,
            normal = { textColor = new Color(0.18f, 0.29f, 0.14f) }
        };

        GUI.Label(new Rect(panelRect.x, panelRect.y + 26f, panelRect.width, 42f), "第一の試練", titleStyle);

        var contentX = panelRect.x + 48f;
        var contentWidth = panelRect.width - 96f;

        GUI.Label(new Rect(contentX, panelRect.y + 88f, contentWidth, 24f), "クエストの巻物", sectionStyle);
        GUI.Label(
            new Rect(contentX, panelRect.y + 116f, contentWidth, 52f),
            selectedTheme,
            bodyStyle);

        GUI.Label(new Rect(contentX, panelRect.y + 178f, contentWidth, 24f), selectedEnemy, sectionStyle);
        GUI.Label(
            new Rect(contentX, panelRect.y + 206f, contentWidth, 52f),
            enemyObjection,
            bodyStyle);

        GUI.Label(new Rect(contentX, panelRect.y + 268f, contentWidth, 24f), "返答を選ぶ", sectionStyle);

        for (var i = 0; i < responses.Length; i++)
        {
            if (GUI.Button(new Rect(contentX, panelRect.y + 302f + i * 42f, contentWidth, 34f), responses[i]))
            {
                selectedResponse = i;
            }
        }

        if (selectedResponse >= 0)
        {
            GUI.Label(
                new Rect(contentX, panelRect.y + 424f, contentWidth, 28f),
                "返答を誓いました。次の段階でEXPとランク進行を追加します。",
                resultStyle);
        }

        if (GUI.Button(new Rect(panelRect.x + 24f, panelRect.y + 24f, 110f, 32f), "ギルド"))
        {
            SceneManager.LoadScene(homeSceneName);
        }
    }

    private string GetEnemyObjection(string enemyName)
    {
        switch (enemyName)
        {
            case "論理の騎士":
                return "この主張が成り立つと言える、いちばん強い根拠は何ですか？";
            case "現実商人":
                return "誰がこの市場の屋台に戻ってきますか？また、なぜ再び関心を持つのですか？";
            case "辛口審査官":
                return "旅人はどこで面倒になり、習慣になる前に足を止めそうですか？";
            case "倫理の守護者":
                return "この術は誤解されたり、危ない使われ方をしたりしませんか？";
            case "冷徹な投資卿":
                return "この旗印は、競合する家門に勝てるほど強い違いを持っていますか？";
            default:
                return "この主張を一文でわかりやすく説明できますか？";
        }
    }
}
