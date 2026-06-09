using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class GameSceneController : MonoBehaviour
{
    [SerializeField] private string homeSceneName = "HomeScene";

    private readonly string[] responses =
    {
        "Clarify the claim on the scroll.",
        "Answer with a stronger supporting reason.",
        "Concede the weak point and forge a better claim."
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

        GUI.Label(new Rect(panelRect.x, panelRect.y + 26f, panelRect.width, 42f), "Trial 1", titleStyle);

        var contentX = panelRect.x + 48f;
        var contentWidth = panelRect.width - 96f;

        GUI.Label(new Rect(contentX, panelRect.y + 88f, contentWidth, 24f), "Quest Scroll", sectionStyle);
        GUI.Label(
            new Rect(contentX, panelRect.y + 116f, contentWidth, 52f),
            selectedTheme,
            bodyStyle);

        GUI.Label(new Rect(contentX, panelRect.y + 178f, contentWidth, 24f), selectedEnemy, sectionStyle);
        GUI.Label(
            new Rect(contentX, panelRect.y + 206f, contentWidth, 52f),
            enemyObjection,
            bodyStyle);

        GUI.Label(new Rect(contentX, panelRect.y + 268f, contentWidth, 24f), "Choose a reply", sectionStyle);

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
                "Reply sworn. A later phase will award EXP and rank progress.",
                resultStyle);
        }

        if (GUI.Button(new Rect(panelRect.x + 24f, panelRect.y + 24f, 110f, 32f), "Guild"))
        {
            SceneManager.LoadScene(homeSceneName);
        }
    }

    private string GetEnemyObjection(string enemyName)
    {
        switch (enemyName)
        {
            case "Logic Knight":
                return "What is the strongest proof that this claim should stand?";
            case "Realist Merchant":
                return "Who would return to this market stall, and why would they pay attention again?";
            case "Harsh Reviewer":
                return "Where will travelers tire of this path before the habit forms?";
            case "Ethics Guardian":
                return "Could this spell be misunderstood or turned toward harmful use?";
            case "Cold Investor":
                return "What makes this banner strong enough to win against rival houses?";
            default:
                return "Can you explain this claim in one plain sentence?";
        }
    }
}
