using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class EnemySelectSceneController : MonoBehaviour
{
    public const string LastEnemyKey = "ThinQuest.LastEnemy";

    [SerializeField] private string themeInputSceneName = "ThemeInputScene";
    [SerializeField] private string gameSceneName = "GameScene";

    private readonly EnemyOption[] enemies =
    {
        new EnemyOption("Beginner Slime", "Checks whether the idea is easy to explain."),
        new EnemyOption("Logic Knight", "Looks for weak reasons, contradictions, and missing proof."),
        new EnemyOption("Realist Merchant", "Tests practicality, demand, and whether people keep using it."),
        new EnemyOption("Harsh Reviewer", "Points out friction, drop-off risks, and user complaints."),
        new EnemyOption("Ethics Guardian", "Checks safety, misunderstanding risks, and harmful wording."),
        new EnemyOption("Cold Investor", "Challenges growth, differentiation, and the strongest reason to win.")
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
        GUI.color = new Color(0.12f, 0.12f, 0.16f, 1f);
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
        GUI.color = new Color(0.95f, 0.95f, 0.98f, 1f);
        GUI.Box(panelRect, GUIContent.none);
        GUI.color = previousColor;

        var titleStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 32,
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(0.16f, 0.16f, 0.24f) }
        };

        var bodyStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 15,
            wordWrap = true,
            normal = { textColor = new Color(0.28f, 0.28f, 0.36f) }
        };

        var selectedStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 16,
            fontStyle = FontStyle.Bold,
            wordWrap = true,
            normal = { textColor = new Color(0.18f, 0.28f, 0.48f) }
        };

        var messageStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 14,
            wordWrap = true,
            normal = { textColor = new Color(0.55f, 0.2f, 0.16f) }
        };

        GUI.Label(new Rect(panelRect.x, panelRect.y + 26f, panelRect.width, 42f), "Choose Enemy", titleStyle);
        GUI.Label(
            new Rect(panelRect.x + 58f, panelRect.y + 78f, panelRect.width - 116f, 42f),
            "Pick the kind of challenge you want for this theme.",
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

        if (GUI.Button(new Rect(panelRect.x + 406f, panelRect.y + 440f, 140f, 38f), "Start Battle"))
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
