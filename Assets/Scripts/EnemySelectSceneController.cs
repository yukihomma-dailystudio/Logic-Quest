using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class EnemySelectSceneController : MonoBehaviour
{
    public const string LastEnemyKey = "ThinQuest.LastEnemy";

    [SerializeField] private string themeInputSceneName = "ThemeInputScene";
    [SerializeField] private string gameSceneName = "GameScene";

    private readonly EnemyOption[] enemies =
    {
        new EnemyOption("見習いスライム", "主張がわかりやすく説明できているかを試す低地の魔物。"),
        new EnemyOption("論理の騎士", "理由の弱さ、矛盾、根拠不足を見抜く鎧の決闘者。"),
        new EnemyOption("現実商人", "需要、手間、継続して使われる理由を測る市場の策士。"),
        new EnemyOption("辛口審査官", "使いにくさ、離脱しそうな点、不満の種を指摘する宮廷批評家。"),
        new EnemyOption("倫理の守護者", "誤解、悪用、安全でない表現を見張る神殿の番人。"),
        new EnemyOption("冷徹な投資卿", "成長性、差別化、勝ち筋を問いただす宝物庫の領主。")
    };

    private int selectedEnemyIndex;
    private bool showMissingGameSceneMessage;

    private void Start()
    {
        var savedEnemy = PlayerPrefs.GetString(LastEnemyKey, enemies[0].Name);
        selectedEnemyIndex = FindEnemyIndex(savedEnemy);
    }

    private void OnGUI()
    {
        DrawBackground();
        DrawEnemyPanel();
    }

    private void DrawBackground()
    {
        var previousColor = GUI.color;
        GUI.color = new Color(0.09f, 0.08f, 0.07f, 1f);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = previousColor;
    }

    private void DrawEnemyPanel()
    {
        const float panelWidth = 760f;
        const float panelHeight = 520f;

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
            fontSize = 32,
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(0.2f, 0.11f, 0.05f) }
        };

        var bodyStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 15,
            wordWrap = true,
            normal = { textColor = new Color(0.25f, 0.17f, 0.09f) }
        };

        var selectedStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 16,
            fontStyle = FontStyle.Bold,
            wordWrap = true,
            normal = { textColor = new Color(0.24f, 0.12f, 0.05f) }
        };

        var messageStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 14,
            wordWrap = true,
            normal = { textColor = new Color(0.55f, 0.2f, 0.16f) }
        };

        GUI.Label(new Rect(panelRect.x, panelRect.y + 26f, panelRect.width, 42f), "挑戦者を選ぶ", titleStyle);
        GUI.Label(
            new Rect(panelRect.x + 58f, panelRect.y + 78f, panelRect.width - 116f, 42f),
            "闘技場であなたの巻物を試す相手を選びましょう。",
            bodyStyle);

        for (var i = 0; i < enemies.Length; i++)
        {
            var column = i % 2;
            var row = i / 2;
            var buttonRect = new Rect(
                panelRect.x + 58f + column * 330f,
                panelRect.y + 140f + row * 74f,
                302f,
                56f);

            if (GUI.Button(buttonRect, enemies[i].Name))
            {
                selectedEnemyIndex = i;
            }
        }

        var selectedEnemy = enemies[selectedEnemyIndex];
        GUI.Label(
            new Rect(panelRect.x + 78f, panelRect.y + 372f, panelRect.width - 156f, 48f),
            $"{selectedEnemy.Name}: {selectedEnemy.Description}",
            selectedStyle);

        if (GUI.Button(new Rect(panelRect.x + 214f, panelRect.y + 440f, 140f, 38f), "戻る"))
        {
            SceneManager.LoadScene(themeInputSceneName);
        }

        if (GUI.Button(new Rect(panelRect.x + 406f, panelRect.y + 440f, 140f, 38f), "闘技場へ"))
        {
            TryStartBattle();
        }

        if (showMissingGameSceneMessage)
        {
            GUI.Label(
                new Rect(panelRect.x + 78f, panelRect.y + 486f, panelRect.width - 156f, 24f),
                "GameScene がまだ登録されていません。戦闘ボタンは自動でつながります。",
                messageStyle);
        }
    }

    private void TryStartBattle()
    {
        PlayerPrefs.SetString(LastEnemyKey, enemies[selectedEnemyIndex].Name);
        PlayerPrefs.Save();

        if (Application.CanStreamedLevelBeLoaded(gameSceneName))
        {
            SceneManager.LoadScene(gameSceneName);
            return;
        }

        showMissingGameSceneMessage = true;
        Debug.LogWarning($"Scene '{gameSceneName}' is not available yet.");
    }

    private int FindEnemyIndex(string enemyName)
    {
        for (var i = 0; i < enemies.Length; i++)
        {
            if (enemies[i].Name == enemyName)
            {
                return i;
            }
        }

        return 0;
    }

    private readonly struct EnemyOption
    {
        public EnemyOption(string name, string description)
        {
            Name = name;
            Description = description;
        }

        public string Name { get; }
        public string Description { get; }
    }
}
