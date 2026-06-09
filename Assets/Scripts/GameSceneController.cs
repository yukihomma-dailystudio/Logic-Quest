using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class GameSceneController : MonoBehaviour
{
    [SerializeField] private string homeSceneName = "HomeScene";

    private readonly string[] responses =
    {
        "Clarify the claim before answering.",
        "Counter with a stronger supporting reason.",
        "Concede the weak point and revise the idea."
    };

    private string selectedTheme = "A short daily thinking quest";
    private string selectedEnemy = "Beginner Slime";
    private string enemyObjection = "Can you explain this idea in one simple sentence?";
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
        GUI.color = new Color(0.13f, 0.12f, 0.16f, 1f);
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
        GUI.color = new Color(0.95f, 0.94f, 0.98f, 1f);
        GUI.Box(panelRect, GUIContent.none);
        GUI.color = previousColor;

        var titleStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 30,
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(0.18f, 0.14f, 0.25f) }
        };

        var sectionStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.UpperLeft,
            fontSize = 16,
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(0.22f, 0.18f, 0.3f) }
        };

        var bodyStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.UpperLeft,
            fontSize = 15,
            wordWrap = true,
            normal = { textColor = new Color(0.28f, 0.26f, 0.34f) }
        };

        var resultStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 15,
            wordWrap = true,
            normal = { textColor = new Color(0.16f, 0.32f, 0.24f) }
        };

        GUI.Label(new Rect(panelRect.x, panelRect.y + 26f, panelRect.width, 42f), "Challenge 1", titleStyle);

        var contentX = panelRect.x + 48f;
        var contentWidth = panelRect.width - 96f;

        GUI.Label(new Rect(contentX, panelRect.y + 88f, contentWidth, 24f), "Claim", sectionStyle);
        GUI.Label(
            new Rect(contentX, panelRect.y + 116f, contentWidth, 52f),
            selectedTheme,
            bodyStyle);

        GUI.Label(new Rect(contentX, panelRect.y + 178f, contentWidth, 24f), selectedEnemy, sectionStyle);
        GUI.Label(
            new Rect(contentX, panelRect.y + 206f, contentWidth, 52f),
            enemyObjection,
            bodyStyle);

        GUI.Label(new Rect(contentX, panelRect.y + 268f, contentWidth, 24f), "Choose a response", sectionStyle);

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
                "Response locked in. Phase 2 will turn this into scoring and progression.",
                resultStyle);
        }

        if (GUI.Button(new Rect(panelRect.x + 24f, panelRect.y + 24f, 110f, 32f), "Home"))
        {
            SceneManager.LoadScene(homeSceneName);
        }
    }

    private string GetEnemyObjection(string enemyName)
    {
        switch (enemyName)
        {
            case "Logic Knight":
                return "What is the strongest reason this claim should be true?";
            case "Realist Merchant":
                return "Who would use this repeatedly, and why would they come back?";
            case "Harsh Reviewer":
                return "Where will users feel friction or stop before the habit forms?";
            case "Ethics Guardian":
                return "Could this idea be misunderstood or used in a harmful way?";
            case "Cold Investor":
                return "What makes this different enough to win against existing options?";
            default:
                return "Can you explain this idea in one simple sentence?";
        }
    }
}
