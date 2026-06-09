using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class ThemeInputSceneController : MonoBehaviour
{
    public const string LastThemeKey = "ThinQuest.LastTheme";

    [SerializeField] private string homeSceneName = "HomeScene";
    [SerializeField] private string gameSceneName = "GameScene";

    private string themeText = "A short daily thinking quest";
    private bool showMissingThemeMessage;
    private bool showMissingGameSceneMessage;

    private void Start()
    {
        themeText = PlayerPrefs.GetString(LastThemeKey, themeText);
    }

    private void OnGUI()
    {
        DrawBackground();
        DrawThemePanel();
    }

    private void DrawBackground()
    {
        var previousColor = GUI.color;
        GUI.color = new Color(0.11f, 0.13f, 0.18f, 1f);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = previousColor;
    }

    private void DrawThemePanel()
    {
        const float panelWidth = 640f;
        const float panelHeight = 420f;

        var panelRect = new Rect(
            (Screen.width - panelWidth) * 0.5f,
            (Screen.height - panelHeight) * 0.5f,
            panelWidth,
            panelHeight);

        var previousColor = GUI.color;
        GUI.color = new Color(0.94f, 0.95f, 0.98f, 1f);
        GUI.Box(panelRect, GUIContent.none);
        GUI.color = previousColor;

        var titleStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 32,
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(0.14f, 0.18f, 0.28f) }
        };

        var bodyStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 16,
            wordWrap = true,
            normal = { textColor = new Color(0.27f, 0.31f, 0.42f) }
        };

        var inputStyle = new GUIStyle(GUI.skin.textArea)
        {
            fontSize = 16,
            wordWrap = true
        };

        var messageStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 14,
            wordWrap = true,
            normal = { textColor = new Color(0.55f, 0.2f, 0.16f) }
        };

        GUI.Label(new Rect(panelRect.x, panelRect.y + 30f, panelRect.width, 44f), "Theme", titleStyle);
        GUI.Label(
            new Rect(panelRect.x + 54f, panelRect.y + 92f, panelRect.width - 108f, 48f),
            "Write the idea you want to defend. The enemy will challenge this claim in the next scene.",
            bodyStyle);

        GUI.SetNextControlName("ThemeInput");
        themeText = GUI.TextArea(
            new Rect(panelRect.x + 70f, panelRect.y + 158f, panelRect.width - 140f, 96f),
            themeText,
            180,
            inputStyle);

        if (GUI.Button(new Rect(panelRect.x + 170f, panelRect.y + 282f, 140f, 40f), "Back"))
        {
            SceneManager.LoadScene(homeSceneName);
        }

        if (GUI.Button(new Rect(panelRect.x + 330f, panelRect.y + 282f, 140f, 40f), "Start Battle"))
        {
            TryStartBattle();
        }

        if (showMissingThemeMessage)
        {
            GUI.Label(
                new Rect(panelRect.x + 70f, panelRect.y + 344f, panelRect.width - 140f, 24f),
                "Enter a theme before starting the battle.",
                messageStyle);
        }

        if (showMissingGameSceneMessage)
        {
            GUI.Label(
                new Rect(panelRect.x + 70f, panelRect.y + 344f, panelRect.width - 140f, 40f),
                "GameScene is not added yet. The battle button will connect automatically.",
                messageStyle);
        }
    }

    private void TryStartBattle()
    {
        themeText = themeText.Trim();
        showMissingThemeMessage = string.IsNullOrEmpty(themeText);
        showMissingGameSceneMessage = false;

        if (showMissingThemeMessage)
        {
            return;
        }

        PlayerPrefs.SetString(LastThemeKey, themeText);
        PlayerPrefs.Save();

        if (Application.CanStreamedLevelBeLoaded(gameSceneName))
        {
            SceneManager.LoadScene(gameSceneName);
            return;
        }

        showMissingGameSceneMessage = true;
        Debug.LogWarning($"Scene '{gameSceneName}' is not available yet.");
    }
}
