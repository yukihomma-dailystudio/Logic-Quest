#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class ThinQuestPlayModeBootstrap
{
    private const string StartScenePath = "Assets/Scenes/TitleScene.unity";

    [InitializeOnLoadMethod]
    private static void ConfigurePlayModeStartScene()
    {
        var startScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(StartScenePath);
        if (startScene == null)
        {
            Debug.LogWarning($"ThinQuest start scene is missing: {StartScenePath}");
            return;
        }

        EditorSceneManager.playModeStartScene = startScene;
    }
}
#endif
