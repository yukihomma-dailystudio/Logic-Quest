using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class AndroidBuildSettingsFixer
{
    private const AndroidSdkVersions RecommendedMinimumApi = AndroidSdkVersions.AndroidApiLevel32;

    [MenuItem("Tools/Android/Fix Android Build Settings")]
    public static void FixAndroidBuildSettings()
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);

        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

        if (PlayerSettings.Android.minSdkVersion < RecommendedMinimumApi)
        {
            Debug.LogWarning(
                $"Android Minimum API Level is {PlayerSettings.Android.minSdkVersion}. " +
                $"Consider using {RecommendedMinimumApi} or newer for this project.");
        }

        if (PlayerSettings.Android.targetSdkVersion != AndroidSdkVersions.AndroidApiLevelAuto &&
            PlayerSettings.Android.targetSdkVersion < RecommendedMinimumApi)
        {
            Debug.LogWarning(
                $"Android Target API Level is {PlayerSettings.Android.targetSdkVersion}. " +
                "Consider using Automatic Highest Installed or a recent installed API level.");
        }

        LogCurrentSettings();
    }

    [MenuItem("Tools/Android/Log Android Build Settings")]
    public static void LogCurrentSettings()
    {
        Debug.Log(
            "Android build settings:\n" +
            $"- Active Build Target: {EditorUserBuildSettings.activeBuildTarget}\n" +
            $"- Scripting Backend: {PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android)}\n" +
            $"- Target Architectures: {PlayerSettings.Android.targetArchitectures}\n" +
            $"- Minimum API Level: {PlayerSettings.Android.minSdkVersion}\n" +
            $"- Target API Level: {PlayerSettings.Android.targetSdkVersion}");
    }
}

public sealed class AndroidBuildSettingsLogger : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        if (report.summary.platform != BuildTarget.Android)
        {
            return;
        }

        AndroidBuildSettingsFixer.LogCurrentSettings();
    }
}
