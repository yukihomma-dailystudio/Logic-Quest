using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class EnemySelectSceneController : MonoBehaviour
{
    public const string LastEnemyKey = "ThinQuest.LastEnemy";

    [SerializeField] private string themeInputSceneName = "ThemeInputScene";
    [SerializeField] private string gameSceneName = "GameScene";

    private readonly EnemyOption[] enemies =
    {
        new EnemyOption("Beginner Slime", "A lowland creature that tests whether the claim is plain enough."),
        new EnemyOption("Logic Knight", "An armored duelist who searches for weak reasons and contradictions."),
        new EnemyOption("Realist Merchant", "A market tactician who weighs demand, cost, and repeat use."),
        new EnemyOption("Harsh Reviewer", "A stern court critic who points out friction and drop-off risks."),
        new EnemyOption("Ethics Guardian", "A temple sentinel who checks safety, misuse, and harmful wording."),
        new EnemyOption("Cold Investor", "A treasury lord who challenges growth, advantage, and winning odds.")
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

        GUI.Label(new Rect(panelRect.x, panelRect.y + 26f, panelRect.width, 42f), "Choose Foe", titleStyle);
        GUI.Label(
            new Rect(panelRect.x + 58f, panelRect.y + 78f, panelRect.width - 116f, 42f),
            "Select the rival who will test your scroll in the arena.",
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

        if (GUI.Button(new Rect(panelRect.x + 214f, panelRect.y + 440f, 140f, 38f), "Back"))
        {
            SceneManager.LoadScene(themeInputSceneName);
        }

        if (GUI.Button(new Rect(panelRect.x + 406f, panelRect.y + 440f, 140f, 38f), "Enter Arena"))
        {
            TryStartBattle();
        }

        if (showMissingGameSceneMessage)
        {
            GUI.Label(
                new Rect(panelRect.x + 78f, panelRect.y + 486f, panelRect.width - 156f, 24f),
                "GameScene is not added yet. The battle button will connect automatically.",
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
