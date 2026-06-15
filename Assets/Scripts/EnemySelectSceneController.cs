using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class EnemySelectSceneController : MonoBehaviour
{
    public const string LastEnemyKey = "ThinQuest.LastEnemy";

    [SerializeField] private string homeSceneName = "HomeScene";
    [SerializeField] private string battleSceneName = "BattleScene";
    [SerializeField] private Texture2D baseBackground;
    [SerializeField] private Texture2D fieldFrame;

    private readonly EnemyOption[] enemies =
    {
        new EnemyOption("見習いスライム", "主張がわかりやすく説明できているかを試す低地の魔物。", "しずく", new Color(0.24f, 0.48f, 0.38f, 1f), "Backgrounds/EnemyFieldBeginnerSlime", "Characters/Enemies/BeginnerSlime"),
        new EnemyOption("論理の騎士", "理由の弱さ、矛盾、根拠不足を見抜く鎧の決闘者。", "盾", new Color(0.35f, 0.38f, 0.48f, 1f), "Backgrounds/EnemyFieldLogicKnight", "Characters/Enemies/LogicKnight"),
        new EnemyOption("現実商人", "需要、手間、継続して使われる理由を測る市場の策士。", "天秤", new Color(0.55f, 0.34f, 0.13f, 1f), "Backgrounds/EnemyFieldRealistMerchant", "Characters/Enemies/RealistMerchant"),
        new EnemyOption("辛口審査官", "使いにくさ、離脱しそうな点、不満の種を指摘する宮廷批評家。", "羽ペン", new Color(0.43f, 0.25f, 0.38f, 1f), "Backgrounds/EnemyFieldHarshReviewer", "Characters/Enemies/HarshReviewer"),
        new EnemyOption("倫理の守護者", "誤解、悪用、安全でない表現を見張る神殿の番人。", "聖印", new Color(0.22f, 0.42f, 0.52f, 1f), "Backgrounds/EnemyFieldEthicsGuardian", "Characters/Enemies/EthicsGuardian"),
        new EnemyOption("冷徹な投資卿", "成長性、差別化、勝ち筋を問いただす宝物庫の領主。", "王冠", new Color(0.48f, 0.32f, 0.12f, 1f), "Backgrounds/EnemyFieldInvestmentLord", "Characters/Enemies/InvestmentLord")
    };

    private int selectedEnemyIndex;
    private bool showMissingBattleSceneMessage;
    private bool isSwipeTracking;
    private Vector2 swipeStartPosition;
    private Texture2D selectedFieldBackground;
    private Texture2D selectedEnemyPortrait;

    private const float MinSwipeDistance = 64f;
    private const float MaxVerticalSwipeOffset = 96f;

    private void Start()
    {
        var savedEnemy = PlayerPrefs.GetString(LastEnemyKey, enemies[0].Name);
        selectedEnemyIndex = FindEnemyIndex(savedEnemy);
        LoadAssetsIfNeeded();
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
        }
        else
        {
            var previousColor = GUI.color;
            GUI.color = new Color(0.09f, 0.08f, 0.07f, 1f);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = previousColor;
        }

        DrawFieldBackground();
    }

    private void LoadAssetsIfNeeded()
    {
        if (baseBackground == null)
        {
            baseBackground = Resources.Load<Texture2D>("Backgrounds/EnemySelectBaseBackground");
        }

        if (fieldFrame == null)
        {
            fieldFrame = Resources.Load<Texture2D>("Backgrounds/EnemyFieldFrame");
        }

        LoadSelectedEnemyAssets();
    }

    private void DrawFieldBackground()
    {
        if (selectedFieldBackground == null)
        {
            return;
        }

        var previousColor = GUI.color;
        var fieldRect = GetFieldBackgroundRect();
        GUI.color = new Color(1f, 1f, 1f, 0.92f);
        GUI.DrawTexture(fieldRect, selectedFieldBackground, ScaleMode.ScaleAndCrop, false);
        GUI.color = new Color(0f, 0f, 0f, 0.22f);
        GUI.DrawTexture(fieldRect, Texture2D.whiteTexture);
        DrawFieldFrame(fieldRect);
        GUI.color = previousColor;
    }

    private void DrawFieldFrame(Rect fieldRect)
    {
        if (fieldFrame == null)
        {
            return;
        }

        var horizontalPadding = fieldRect.width * 96f / 1856f;
        var verticalPadding = fieldRect.height * 54f / 1044f;
        var frameRect = new Rect(
            fieldRect.x - horizontalPadding,
            fieldRect.y - verticalPadding,
            fieldRect.width + horizontalPadding * 2f,
            fieldRect.height + verticalPadding * 2f);

        GUI.DrawTexture(frameRect, fieldFrame, ScaleMode.StretchToFill, true);
    }

    private Rect GetFieldBackgroundRect()
    {
        var width = Mathf.Min(Screen.width * 0.88f, 1440f);
        var height = width * 9f / 16f;
        var maxHeight = Screen.height * 0.78f;
        if (height > maxHeight)
        {
            height = maxHeight;
            width = height * 16f / 9f;
        }

        return new Rect(
            (Screen.width - width) * 0.5f,
            Mathf.Max(24f, Screen.height * 0.105f),
            width,
            height);
    }

    private void LoadSelectedEnemyAssets()
    {
        var enemy = enemies[selectedEnemyIndex];
        selectedFieldBackground = string.IsNullOrEmpty(enemy.FieldBackgroundResourcePath)
            ? null
            : Resources.Load<Texture2D>(enemy.FieldBackgroundResourcePath);
        selectedEnemyPortrait = string.IsNullOrEmpty(enemy.PortraitResourcePath)
            ? null
            : Resources.Load<Texture2D>(enemy.PortraitResourcePath);
    }

    private void DrawEnemyPanel()
    {
        var titleStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 30,
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(0.95f, 0.86f, 0.62f) }
        };

        var bodyStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 15,
            wordWrap = true,
            normal = { textColor = new Color(0.92f, 0.84f, 0.68f) }
        };

        var messageStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 14,
            wordWrap = true,
            normal = { textColor = new Color(1f, 0.7f, 0.56f) }
        };

        var enemyNameStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 30,
            fontStyle = FontStyle.Bold,
            wordWrap = true,
            normal = { textColor = new Color(0.95f, 0.84f, 0.58f) }
        };

        var portraitStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 36,
            fontStyle = FontStyle.Bold,
            wordWrap = true,
            normal = { textColor = new Color(0.94f, 0.9f, 0.72f) }
        };

        var descriptionStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 19,
            fontStyle = FontStyle.Bold,
            wordWrap = true,
            normal = { textColor = new Color(0.96f, 0.9f, 0.76f) }
        };

        var hintStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 13,
            wordWrap = true,
            normal = { textColor = new Color(0.88f, 0.78f, 0.62f) }
        };

        var counterStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 15,
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(0.95f, 0.84f, 0.58f) }
        };

        var arrowButtonStyle = new GUIStyle(GUI.skin.button)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 34,
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(1f, 0.92f, 0.68f) },
            hover = { textColor = Color.white },
            active = { textColor = Color.white }
        };

        var actionButtonStyle = new GUIStyle(GUI.skin.button)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 22,
            fontStyle = FontStyle.Bold,
            wordWrap = true,
            normal = { textColor = new Color(1f, 0.92f, 0.68f) },
            hover = { textColor = Color.white },
            active = { textColor = Color.white }
        };

        GUI.Label(new Rect(0f, 26f, Screen.width, 42f), "挑戦者を選ぶ", titleStyle);
        GUI.Label(
            new Rect(Screen.width * 0.2f, 74f, Screen.width * 0.6f, 42f),
            "左右にスワイプして、闘技場であなたの巻物を試す相手を選びましょう。",
            bodyStyle);

        var selectedEnemy = enemies[selectedEnemyIndex];
        var portraitRect = GetEnemyPortraitRect();
        DrawEnemyPortrait(portraitRect, selectedEnemy, selectedEnemyPortrait, portraitStyle);
        DrawNamePlate(portraitRect, selectedEnemy, enemyNameStyle, descriptionStyle);

        GUI.Label(
            new Rect(Screen.width * 0.44f, Screen.height * 0.78f, Screen.width * 0.12f, 24f),
            $"{selectedEnemyIndex + 1} / {enemies.Length}",
            counterStyle);

        GUI.Label(
            new Rect(Screen.width * 0.30f, Screen.height * 0.815f, Screen.width * 0.40f, 28f),
            "左右スワイプで相手を切り替え",
            hintStyle);

        var arrowWidth = Mathf.Clamp(Screen.width * 0.075f, 88f, 116f);
        var arrowHeight = Mathf.Clamp(Screen.height * 0.095f, 72f, 96f);
        var arrowY = Screen.height * 0.44f;
        if (DrawProminentButton(
                new Rect(Screen.width * 0.065f, arrowY, arrowWidth, arrowHeight),
                "←",
                arrowButtonStyle))
        {
            ChangeEnemy(-1);
        }

        if (DrawProminentButton(
                new Rect(Screen.width * 0.935f - arrowWidth, arrowY, arrowWidth, arrowHeight),
                "→",
                arrowButtonStyle))
        {
            ChangeEnemy(1);
        }

        var bottomMargin = Mathf.Max(24f, Screen.height * 0.04f);
        var buttonWidth = Mathf.Clamp(Screen.width * 0.25f, 220f, 320f);
        var buttonHeight = Mathf.Clamp(Screen.height * 0.082f, 64f, 82f);
        if (DrawProminentButton(
                new Rect(28f, Screen.height - bottomMargin - buttonHeight, buttonWidth, buttonHeight),
                "ギルドに戻る",
                actionButtonStyle))
        {
            SceneManager.LoadScene(homeSceneName);
        }

        if (DrawProminentButton(
                new Rect(Screen.width - 28f - buttonWidth, Screen.height - bottomMargin - buttonHeight, buttonWidth, buttonHeight),
                "敵と戦う",
                actionButtonStyle))
        {
            TryStartBattle();
        }

        if (showMissingBattleSceneMessage)
        {
            GUI.Label(
                new Rect(Screen.width * 0.20f, Screen.height - bottomMargin - buttonHeight - 34f, Screen.width * 0.60f, 24f),
                "BattleScene がまだ登録されていません。戦闘ボタンは自動でつながります。",
                messageStyle);
        }
    }

    private static bool DrawProminentButton(Rect rect, string text, GUIStyle style)
    {
        var previousColor = GUI.color;
        var previousBackgroundColor = GUI.backgroundColor;

        GUI.color = new Color(0f, 0f, 0f, 0.48f);
        GUI.DrawTexture(new Rect(rect.x + 5f, rect.y + 6f, rect.width, rect.height), Texture2D.whiteTexture);
        GUI.color = new Color(0.82f, 0.61f, 0.28f, 0.95f);
        GUI.DrawTexture(new Rect(rect.x - 3f, rect.y - 3f, rect.width + 6f, rect.height + 6f), Texture2D.whiteTexture);
        GUI.color = new Color(0.16f, 0.09f, 0.04f, 0.98f);
        GUI.DrawTexture(new Rect(rect.x + 2f, rect.y + 2f, rect.width - 4f, rect.height - 4f), Texture2D.whiteTexture);

        GUI.color = Color.white;
        GUI.backgroundColor = new Color(0.52f, 0.31f, 0.12f, 1f);
        var clicked = GUI.Button(rect, text, style);

        GUI.backgroundColor = previousBackgroundColor;
        GUI.color = previousColor;
        return clicked;
    }

    private static Rect GetEnemyPortraitRect()
    {
        var width = Mathf.Min(540f, Screen.width * 0.42f);
        var height = Mathf.Min(520f, Screen.height * 0.52f);
        return new Rect(
            (Screen.width - width) * 0.5f,
            Screen.height * 0.205f,
            width,
            height);
    }

    private static void DrawEnemyPortrait(
        Rect portraitRect,
        EnemyOption enemy,
        Texture2D portraitTexture,
        GUIStyle portraitStyle)
    {
        var previousColor = GUI.color;
        GUI.color = new Color(0f, 0f, 0f, 0.26f);
        GUI.DrawTexture(
            new Rect(portraitRect.x + 20f, portraitRect.y + portraitRect.height - 18f, portraitRect.width - 40f, 22f),
            Texture2D.whiteTexture);

        if (portraitTexture != null)
        {
            GUI.color = Color.white;
            GUI.DrawTexture(portraitRect, portraitTexture, ScaleMode.ScaleToFit, true);
            GUI.color = previousColor;
            return;
        }

        GUI.color = enemy.PortraitColor;
        GUI.DrawTexture(portraitRect, Texture2D.whiteTexture);
        GUI.color = new Color(0.12f, 0.08f, 0.05f, 0.22f);
        GUI.DrawTexture(
            new Rect(portraitRect.x + 24f, portraitRect.y + portraitRect.height * 0.74f, portraitRect.width - 48f, 18f),
            Texture2D.whiteTexture);

        GUI.color = previousColor;
        GUI.Label(portraitRect, enemy.PortraitLabel, portraitStyle);
    }

    private static void DrawNamePlate(
        Rect portraitRect,
        EnemyOption enemy,
        GUIStyle enemyNameStyle,
        GUIStyle descriptionStyle)
    {
        var plateRect = new Rect(
            portraitRect.x - 120f,
            portraitRect.y + portraitRect.height + 10f,
            portraitRect.width + 240f,
            112f);

        var previousColor = GUI.color;
        GUI.color = new Color(0.14f, 0.08f, 0.04f, 0.78f);
        GUI.DrawTexture(plateRect, Texture2D.whiteTexture);
        GUI.color = new Color(0.82f, 0.64f, 0.33f, 0.92f);
        GUI.DrawTexture(new Rect(plateRect.x, plateRect.y, plateRect.width, 4f), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(plateRect.x, plateRect.yMax - 4f, plateRect.width, 4f), Texture2D.whiteTexture);
        GUI.color = previousColor;

        GUI.Label(new Rect(plateRect.x + 18f, plateRect.y + 10f, plateRect.width - 36f, 38f), enemy.Name, enemyNameStyle);
        GUI.Label(new Rect(plateRect.x + 30f, plateRect.y + 54f, plateRect.width - 60f, 48f), enemy.Description, descriptionStyle);
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
        LoadSelectedEnemyAssets();
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
        public EnemyOption(
            string name,
            string description,
            string portraitLabel,
            Color portraitColor,
            string fieldBackgroundResourcePath,
            string portraitResourcePath)
        {
            Name = name;
            Description = description;
            PortraitLabel = portraitLabel;
            PortraitColor = portraitColor;
            FieldBackgroundResourcePath = fieldBackgroundResourcePath;
            PortraitResourcePath = portraitResourcePath;
        }

        public string Name { get; }
        public string Description { get; }
        public string PortraitLabel { get; }
        public Color PortraitColor { get; }
        public string FieldBackgroundResourcePath { get; }
        public string PortraitResourcePath { get; }
    }
}
