using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class TitleSceneController : MonoBehaviour
{
    [SerializeField] private string nextSceneName = "HomeScene";

    private bool showMissingSceneMessage;

    private void OnGUI()
    {
        DrawBackground();
        DrawTitlePanel();
    }

    private void DrawBackground()
    {
        var previousColor = GUI.color;
        GUI.color = new Color(0.11f, 0.12f, 0.18f, 1f);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = previousColor;
    }

    private void DrawTitlePanel()
    {
        const float panelWidth = 520f;
        const float panelHeight = 320f;

        var panelRect = new Rect(
            (Screen.width - panelWidth) * 0.5f,
            (Screen.height - panelHeight) * 0.5f,
            panelWidth,
            panelHeight);

        var previousColor = GUI.color;
        GUI.color = new Color(0.92f, 0.94f, 0.98f, 1f);
        GUI.Box(panelRect, GUIContent.none);
        GUI.color = previousColor;

        var titleStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 42,
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(0.12f, 0.16f, 0.24f) }
        };

        var subtitleStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 18,
            wordWrap = true,
            normal = { textColor = new Color(0.28f, 0.32f, 0.42f) }
        };

        var messageStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 14,
            wordWrap = true,
            normal = { textColor = new Color(0.55f, 0.2f, 0.16f) }
        };

        GUI.Label(new Rect(panelRect.x, panelRect.y + 34f, panelRect.width, 56f), "ThinQuest", titleStyle);
        GUI.Label(
            new Rect(panelRect.x + 36f, panelRect.y + 102f, panelRect.width - 72f, 72f),
            "Think through enemy objections and strengthen your own ideas.",
            subtitleStyle);

        if (GUI.Button(new Rect(panelRect.x + 160f, panelRect.y + 204f, 200f, 42f), "Start"))
        {
            TryLoadNextScene();
        }

        if (showMissingSceneMessage)
        {
            GUI.Label(
                new Rect(panelRect.x + 36f, panelRect.y + 258f, panelRect.width - 72f, 44f),
                "HomeScene is not added yet. Create the next scene and the Start button will connect automatically.",
                messageStyle);
        }
    }

    private void TryLoadNextScene()
    {
        if (Application.CanStreamedLevelBeLoaded(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
            return;
        }

        showMissingSceneMessage = true;
        Debug.LogWarning($"Scene '{nextSceneName}' is not available yet.");
    }
}
