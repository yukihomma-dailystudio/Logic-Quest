using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class HomeSceneController : MonoBehaviour
{
    [SerializeField] private string titleSceneName = "TitleScene";
    [SerializeField] private string themeInputSceneName = "ThemeInputScene";

    private bool showMissingThemeInputSceneMessage;

    private void OnGUI()
    {
        DrawBackground();
        DrawHomePanel();
    }

    private void DrawBackground()
    {
        var previousColor = GUI.color;
        GUI.color = new Color(0.09f, 0.11f, 0.09f, 1f);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = previousColor;
    }

    private void DrawHomePanel()
    {
        const float panelWidth = 560f;
        const float panelHeight = 360f;

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
            fontSize = 34,
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(0.2f, 0.11f, 0.05f) }
        };

        var bodyStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 17,
            wordWrap = true,
            normal = { textColor = new Color(0.25f, 0.17f, 0.09f) }
        };

        var messageStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 14,
            wordWrap = true,
            normal = { textColor = new Color(0.55f, 0.22f, 0.16f) }
        };

        GUI.Label(new Rect(panelRect.x, panelRect.y + 32f, panelRect.width, 48f), "ギルドホール", titleStyle);
        GUI.Label(
            new Rect(panelRect.x + 48f, panelRect.y + 96f, panelRect.width - 96f, 72f),
            "巻物に主張を記し、挑戦者を選び、思考の試練へ向かいましょう。",
            bodyStyle);

        if (GUI.Button(new Rect(panelRect.x + 180f, panelRect.y + 190f, 200f, 42f), "クエスト開始"))
        {
            TryLoadThemeInputScene();
        }

        if (GUI.Button(new Rect(panelRect.x + 180f, panelRect.y + 244f, 200f, 36f), "門へ戻る"))
        {
            SceneManager.LoadScene(titleSceneName);
        }

        if (showMissingThemeInputSceneMessage)
        {
            GUI.Label(
                new Rect(panelRect.x + 48f, panelRect.y + 298f, panelRect.width - 96f, 42f),
                "ThemeInputScene がまだ登録されていません。次の試作段階でつながります。",
                messageStyle);
        }
    }

    private void TryLoadThemeInputScene()
    {
        if (Application.CanStreamedLevelBeLoaded(themeInputSceneName))
        {
            SceneManager.LoadScene(themeInputSceneName);
            return;
        }

        showMissingThemeInputSceneMessage = true;
        Debug.LogWarning($"Scene '{themeInputSceneName}' is not available yet.");
    }
}
