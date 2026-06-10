using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class GameSceneController : MonoBehaviour
{
    [SerializeField] private string homeSceneName = "HomeScene";
    [SerializeField] private string resultSceneName = "ResultScene";

    private string selectedTheme = "毎日少しずつ考えを鍛えるクエスト";
    private string selectedEnemy = "見習いスライム";
    private string enemyObjection = "この主張を一文でわかりやすく説明できますか？";
    private string answerText = "";
    private string evaluationText = "";
    private bool showMissingAnswerMessage;
    private bool rewardClaimed;
    private string awardedAbility = "説明力";

    private void Start()
    {
        selectedTheme = PlayerPrefs.GetString(ThemeInputSceneController.LastThemeKey, selectedTheme);
        selectedEnemy = PlayerPrefs.GetString(EnemySelectSceneController.LastEnemyKey, selectedEnemy);
        enemyObjection = GetEnemyObjection(selectedEnemy);
        awardedAbility = GetAwardedAbility(selectedEnemy);
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
        const float panelWidth = 760f;
        const float panelHeight = 560f;

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

        var inputStyle = new GUIStyle(GUI.skin.textArea)
        {
            fontSize = 15,
            wordWrap = true
        };

        var messageStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 14,
            wordWrap = true,
            normal = { textColor = new Color(0.55f, 0.2f, 0.16f) }
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

        GUI.Label(new Rect(contentX, panelRect.y + 268f, contentWidth, 24f), "あなたの返答", sectionStyle);

        if (!rewardClaimed)
        {
            answerText = GUI.TextArea(
                new Rect(contentX, panelRect.y + 300f, contentWidth, 96f),
                answerText,
                220,
                inputStyle);

            if (GUI.Button(new Rect(panelRect.x + 300f, panelRect.y + 414f, 160f, 38f), "返答する"))
            {
                TrySubmitAnswer();
            }

            if (showMissingAnswerMessage)
            {
                GUI.Label(
                    new Rect(contentX, panelRect.y + 462f, contentWidth, 24f),
                    "敵に返す言葉を巻物へ書いてください。",
                    messageStyle);
            }
        }
        else
        {
            GUI.Label(new Rect(contentX, panelRect.y + 300f, contentWidth, 70f), evaluationText, bodyStyle);
            GUI.Label(
                new Rect(contentX, panelRect.y + 402f, contentWidth, 28f),
                $"{awardedAbility} EXP +20 を獲得しました。ギルドで記録を確認できます。",
                resultStyle);

            if (GUI.Button(new Rect(panelRect.x + 285f, panelRect.y + 450f, 190f, 38f), "戦果を確認"))
            {
                TryLoadResultScene();
            }
        }

        if (GUI.Button(new Rect(panelRect.x + 24f, panelRect.y + 24f, 110f, 32f), "ギルド"))
        {
            SceneManager.LoadScene(homeSceneName);
        }
    }

    private void TrySubmitAnswer()
    {
        answerText = answerText.Trim();
        showMissingAnswerMessage = string.IsNullOrEmpty(answerText);

        if (showMissingAnswerMessage)
        {
            return;
        }

        evaluationText = BuildEvaluation(answerText);
        ClaimReward();
    }

    private string BuildEvaluation(string answer)
    {
        var hasReason = ContainsAny(answer, "理由", "なぜ", "だから", "ため", "根拠");
        var hasExample = ContainsAny(answer, "例えば", "たとえば", "具体", "例");
        var isDetailed = answer.Length >= 45;

        if (hasReason && hasExample && isDetailed)
        {
            return "根拠と具体例が入っているため、巻物の主張がかなり伝わりやすくなっています。次は、相手が疑いそうな点を先に補うとさらに強くなります。";
        }

        if (hasReason && isDetailed)
        {
            return "理由を添えて答えられています。説得力は出ていますが、具体例を一つ足すと、聞き手が場面を想像しやすくなります。";
        }

        if (hasExample)
        {
            return "具体例があるので、主張の姿は見えやすくなっています。次は、その例からなぜ主張が成り立つのかを一文で補いましょう。";
        }

        return "返答の方向は見えています。次は、理由や具体例を加えて、敵が突っ込みにくい形へ鍛え直しましょう。";
    }

    private static bool ContainsAny(string text, params string[] keywords)
    {
        for (var i = 0; i < keywords.Length; i++)
        {
            if (text.Contains(keywords[i]))
            {
                return true;
            }
        }

        return false;
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

    private string GetAwardedAbility(string enemyName)
    {
        switch (enemyName)
        {
            case "論理の騎士":
                return "論理力";
            case "現実商人":
                return "具体化力";
            case "辛口審査官":
                return "反論耐性";
            case "倫理の守護者":
                return "視点切替力";
            case "冷徹な投資卿":
                return "要約力";
            default:
                return "説明力";
        }
    }

    private void ClaimReward()
    {
        if (rewardClaimed)
        {
            return;
        }

        UserDataManager.AddExp(20, awardedAbility);
        PlayerPrefs.SetString(ResultSceneController.LastAwardedAbilityKey, awardedAbility);
        PlayerPrefs.SetInt(ResultSceneController.LastAwardedExpKey, 20);
        PlayerPrefs.SetString(ResultSceneController.LastAnswerKey, answerText);
        PlayerPrefs.SetString(ResultSceneController.LastEvaluationKey, evaluationText);
        PlayerPrefs.Save();
        rewardClaimed = true;
    }

    private void TryLoadResultScene()
    {
        if (Application.CanStreamedLevelBeLoaded(resultSceneName))
        {
            SceneManager.LoadScene(resultSceneName);
            return;
        }

        SceneManager.LoadScene(homeSceneName);
        Debug.LogWarning($"Scene '{resultSceneName}' is not available yet.");
    }
}
