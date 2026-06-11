using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class EnemySelectSceneController : MonoBehaviour
{
    public const string LastEnemyKey = "ThinQuest.LastEnemy";

    [SerializeField] private string themeInputSceneName = "ThemeInputScene";
    [SerializeField] private string battleSceneName = "BattleScene";

    private readonly EnemyOption[] enemies =
    {
        new EnemyOption("見習いスライム", "主張がわかりやすく説明できているかを試す低地の魔物。", "しずく", new Color(0.24f, 0.48f, 0.38f, 1f)),
        new EnemyOption("論理の騎士", "理由の弱さ、矛盾、根拠不足を見抜く鎧の決闘者。", "盾", new Color(0.35f, 0.38f, 0.48f, 1f)),
        new EnemyOption("現実商人", "需要、手間、継続して使われる理由を測る市場の策士。", "天秤", new Color(0.55f, 0.34f, 0.13f, 1f)),
        new EnemyOption("辛口審査官", "使いにくさ、離脱しそうな点、不満の種を指摘する宮廷批評家。", "羽ペン", new Color(0.43f, 0.25f, 0.38f, 1f)),
        new EnemyOption("倫理の守護者", "誤解、悪用、安全でない表現を見張る神殿の番人。", "聖印", new Color(0.22f, 0.42f, 0.52f, 1f)),
        new EnemyOption("冷徹な投資卿", "成長性、差別化、勝ち筋を問いただす宝物庫の領主。", "王冠", new Color(0.48f, 0.32f, 0.12f, 1f))
    };

    private int selectedEnemyIndex;
    private bool showMissingBattleSceneMessage;

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
        const float panelWidth = 860f;
        const float panelHeight = 620f;

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

        var enemyNameStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 16,
            fontStyle = FontStyle.Bold,
            wordWrap = true,
            normal = { textColor = new Color(0.22f, 0.12f, 0.06f) }
        };

        var portraitStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 24,
            fontStyle = FontStyle.Bold,
            wordWrap = true,
            normal = { textColor = new Color(0.92f, 0.84f, 0.64f) }
        };

        GUI.Label(new Rect(panelRect.x, panelRect.y + 26f, panelRect.width, 42f), "挑戦者を選ぶ", titleStyle);
        GUI.Label(
            new Rect(panelRect.x + 58f, panelRect.y + 78f, panelRect.width - 116f, 42f),
            "闘技場であなたの巻物を試す相手を選びましょう。",
            bodyStyle);

        for (var i = 0; i < enemies.Length; i++)
        {
            var column = i % 3;
            var row = i / 3;
            var cardRect = new Rect(
                panelRect.x + 58f + column * 250f,
                panelRect.y + 140f + row * 172f,
                218f,
                146f);

            if (GUI.Button(cardRect, GUIContent.none))
            {
                selectedEnemyIndex = i;
            }

            DrawEnemyCard(cardRect, enemies[i], i == selectedEnemyIndex, enemyNameStyle, portraitStyle);
        }

        var selectedEnemy = enemies[selectedEnemyIndex];
        GUI.Label(
            new Rect(panelRect.x + 88f, panelRect.y + 492f, panelRect.width - 176f, 44f),
            $"{selectedEnemy.Name}: {selectedEnemy.Description}",
            selectedStyle);

        if (GUI.Button(new Rect(panelRect.x + 264f, panelRect.y + 548f, 140f, 38f), "戻る"))
        {
            SceneManager.LoadScene(themeInputSceneName);
        }

        if (GUI.Button(new Rect(panelRect.x + 456f, panelRect.y + 548f, 140f, 38f), "闘技場へ"))
        {
            TryStartBattle();
        }

        if (showMissingBattleSceneMessage)
        {
            GUI.Label(
                new Rect(panelRect.x + 88f, panelRect.y + 592f, panelRect.width - 176f, 24f),
                "BattleScene がまだ登録されていません。戦闘ボタンは自動でつながります。",
                messageStyle);
        }
    }

    private static void DrawEnemyCard(
        Rect cardRect,
        EnemyOption enemy,
        bool isSelected,
        GUIStyle enemyNameStyle,
        GUIStyle portraitStyle)
    {
        var previousColor = GUI.color;

        GUI.color = isSelected
            ? new Color(0.93f, 0.82f, 0.54f, 1f)
            : new Color(0.68f, 0.53f, 0.34f, 1f);
        GUI.DrawTexture(cardRect, Texture2D.whiteTexture);

        var innerRect = new Rect(cardRect.x + 4f, cardRect.y + 4f, cardRect.width - 8f, cardRect.height - 8f);
        GUI.color = new Color(0.79f, 0.68f, 0.48f, 1f);
        GUI.DrawTexture(innerRect, Texture2D.whiteTexture);

        var portraitRect = new Rect(cardRect.x + 18f, cardRect.y + 14f, cardRect.width - 36f, 86f);
        GUI.color = enemy.PortraitColor;
        GUI.DrawTexture(portraitRect, Texture2D.whiteTexture);

        GUI.color = new Color(0.12f, 0.08f, 0.05f, 0.22f);
        GUI.DrawTexture(new Rect(portraitRect.x + 12f, portraitRect.y + 60f, portraitRect.width - 24f, 12f), Texture2D.whiteTexture);

        GUI.color = previousColor;
        GUI.Label(portraitRect, enemy.PortraitLabel, portraitStyle);
        GUI.Label(new Rect(cardRect.x + 8f, cardRect.y + 106f, cardRect.width - 16f, 32f), enemy.Name, enemyNameStyle);
    }

    private void TryStartBattle()
    {
        PlayerPrefs.SetString(LastEnemyKey, enemies[selectedEnemyIndex].Name);
        PlayerPrefs.Save();

        if (Application.CanStreamedLevelBeLoaded(battleSceneName))
        {
            SceneManager.LoadScene(battleSceneName);
            return;
        }

        showMissingBattleSceneMessage = true;
        Debug.LogWarning($"Scene '{battleSceneName}' is not available yet.");
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
        public EnemyOption(string name, string description, string portraitLabel, Color portraitColor)
        {
            Name = name;
            Description = description;
            PortraitLabel = portraitLabel;
            PortraitColor = portraitColor;
        }

        public string Name { get; }
        public string Description { get; }
        public string PortraitLabel { get; }
        public Color PortraitColor { get; }
    }
}
