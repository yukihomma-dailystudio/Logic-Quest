using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class EnemySelectSceneController : MonoBehaviour
{
    public const string LastEnemyKey = "ThinQuest.LastEnemy";

    [SerializeField] private string themeInputSceneName = "ThemeInputScene";
    [SerializeField] private string battleSceneName = "BattleScene";
    [SerializeField] private Texture2D baseBackground;

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
    private bool isSwipeTracking;
    private Vector2 swipeStartPosition;

    private const float MinSwipeDistance = 64f;
    private const float MaxVerticalSwipeOffset = 96f;

    private void Start()
    {
        LoadAssetsIfNeeded();
        var savedEnemy = PlayerPrefs.GetString(LastEnemyKey, enemies[0].Name);
        selectedEnemyIndex = FindEnemyIndex(savedEnemy);
    }

    private void OnGUI()
    {
        DrawBackground();
        DrawEnemyPanel();
    }

    private void Update()
    {
        HandleSwipeInput();
    }

    private void DrawBackground()
    {
        if (baseBackground != null)
        {
            GUI.DrawTexture(
                new Rect(0f, 0f, Screen.width, Screen.height),
                baseBackground,
                ScaleMode.ScaleAndCrop,
                false);
            return;
        }

        var previousColor = GUI.color;
        GUI.color = new Color(0.09f, 0.08f, 0.07f, 1f);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = previousColor;
    }

    private void LoadAssetsIfNeeded()
    {
        if (baseBackground == null)
        {
            baseBackground = Resources.Load<Texture2D>("Backgrounds/EnemySelectBaseBackground");
        }
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
            fontSize = 24,
            fontStyle = FontStyle.Bold,
            wordWrap = true,
            normal = { textColor = new Color(0.22f, 0.12f, 0.06f) }
        };

        var portraitStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 36,
            fontStyle = FontStyle.Bold,
            wordWrap = true,
            normal = { textColor = new Color(0.92f, 0.84f, 0.64f) }
        };

        var descriptionStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 17,
            fontStyle = FontStyle.Bold,
            wordWrap = true,
            normal = { textColor = new Color(0.24f, 0.12f, 0.05f) }
        };

        var hintStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 13,
            wordWrap = true,
            normal = { textColor = new Color(0.34f, 0.22f, 0.12f) }
        };

        var counterStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 15,
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(0.31f, 0.18f, 0.08f) }
        };

        GUI.Label(new Rect(panelRect.x, panelRect.y + 26f, panelRect.width, 42f), "挑戦者を選ぶ", titleStyle);
        GUI.Label(
            new Rect(panelRect.x + 58f, panelRect.y + 78f, panelRect.width - 116f, 42f),
            "左右にスワイプして、闘技場であなたの巻物を試す相手を選びましょう。",
            bodyStyle);

        var selectedEnemy = enemies[selectedEnemyIndex];
        var cardRect = new Rect(panelRect.x + 242f, panelRect.y + 134f, 376f, 318f);
        DrawEnemyCard(cardRect, selectedEnemy, enemyNameStyle, portraitStyle, descriptionStyle);

        GUI.Label(
            new Rect(panelRect.x + 320f, panelRect.y + 462f, panelRect.width - 640f, 24f),
            $"{selectedEnemyIndex + 1} / {enemies.Length}",
            counterStyle);

        GUI.Label(
            new Rect(panelRect.x + 216f, panelRect.y + 490f, panelRect.width - 432f, 28f),
            "左右スワイプで相手を切り替え",
            hintStyle);

        if (GUI.Button(new Rect(panelRect.x + 128f, panelRect.y + 274f, 72f, 54f), "←"))
        {
            ChangeEnemy(-1);
        }

        if (GUI.Button(new Rect(panelRect.x + 660f, panelRect.y + 274f, 72f, 54f), "→"))
        {
            ChangeEnemy(1);
        }

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
        GUIStyle enemyNameStyle,
        GUIStyle portraitStyle,
        GUIStyle descriptionStyle)
    {
        var previousColor = GUI.color;

        GUI.color = new Color(0.93f, 0.82f, 0.54f, 1f);
        GUI.DrawTexture(cardRect, Texture2D.whiteTexture);

        var innerRect = new Rect(cardRect.x + 6f, cardRect.y + 6f, cardRect.width - 12f, cardRect.height - 12f);
        GUI.color = new Color(0.79f, 0.68f, 0.48f, 1f);
        GUI.DrawTexture(innerRect, Texture2D.whiteTexture);

        var portraitRect = new Rect(cardRect.x + 34f, cardRect.y + 28f, cardRect.width - 68f, 156f);
        GUI.color = enemy.PortraitColor;
        GUI.DrawTexture(portraitRect, Texture2D.whiteTexture);

        GUI.color = new Color(0.12f, 0.08f, 0.05f, 0.22f);
        GUI.DrawTexture(new Rect(portraitRect.x + 24f, portraitRect.y + 116f, portraitRect.width - 48f, 18f), Texture2D.whiteTexture);

        GUI.color = previousColor;
        GUI.Label(portraitRect, enemy.PortraitLabel, portraitStyle);
        GUI.Label(new Rect(cardRect.x + 18f, cardRect.y + 198f, cardRect.width - 36f, 36f), enemy.Name, enemyNameStyle);
        GUI.Label(new Rect(cardRect.x + 42f, cardRect.y + 244f, cardRect.width - 84f, 52f), enemy.Description, descriptionStyle);
    }

    private void HandleSwipeInput()
    {
        if (Input.touchCount > 0)
        {
            var touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                StartSwipe(touch.position);
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                EndSwipe(touch.position);
            }

            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            StartSwipe(Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            EndSwipe(Input.mousePosition);
        }
    }

    private void StartSwipe(Vector2 position)
    {
        isSwipeTracking = true;
        swipeStartPosition = position;
    }

    private void EndSwipe(Vector2 position)
    {
        if (!isSwipeTracking)
        {
            return;
        }

        isSwipeTracking = false;
        var delta = position - swipeStartPosition;

        if (Mathf.Abs(delta.x) < MinSwipeDistance || Mathf.Abs(delta.y) > MaxVerticalSwipeOffset)
        {
            return;
        }

        ChangeEnemy(delta.x < 0f ? 1 : -1);
    }

    private void ChangeEnemy(int direction)
    {
        selectedEnemyIndex = (selectedEnemyIndex + direction + enemies.Length) % enemies.Length;
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
