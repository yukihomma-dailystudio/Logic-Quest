using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

internal sealed class BattleLlmService
{
    private readonly LlamaCppUnityGateway gateway = new LlamaCppUnityGateway();
    private bool isGenerating;

    public IEnumerator GenerateObjection(
        string theme,
        string enemyName,
        Action<ClarisseLlmResult> onComplete)
    {
        var prompt = BattleLlmSettings.BuildObjectionPrompt(theme, enemyName);
        yield return Generate(prompt, BattleLlmSettings.ObjectionMaxTokens, BattleLlmSettings.ObjectionCharacterLimit, onComplete);
    }

    public IEnumerator GenerateEvaluation(
        string theme,
        string enemyName,
        string objection,
        string answer,
        Action<ClarisseLlmResult> onComplete)
    {
        var prompt = BattleLlmSettings.BuildEvaluationPrompt(theme, enemyName, objection, answer);
        yield return Generate(prompt, BattleLlmSettings.EvaluationMaxTokens, BattleLlmSettings.EvaluationCharacterLimit, onComplete);
    }

    private IEnumerator Generate(
        string prompt,
        int maxTokens,
        int characterLimit,
        Action<ClarisseLlmResult> onComplete)
    {
        if (isGenerating)
        {
            onComplete?.Invoke(ClarisseLlmResult.Error("敵はまだ考えをまとめています。"));
            yield break;
        }

        isGenerating = true;
        var modelReady = false;
        yield return EnsureModelReady(ready => modelReady = ready);

        if (!modelReady)
        {
            isGenerating = false;
            onComplete?.Invoke(ClarisseLlmResult.Error(ClarisseLlmSettings.GetMissingModelMessage()));
            yield break;
        }

        var result = ClarisseLlmResult.Error(ClarisseLlmSettings.GenerationFailedMessage);
        yield return gateway.Generate(
            ClarisseLlmSettings.GetPersistentModelPath(),
            BattleLlmSettings.GetSystemPrompt(),
            prompt,
            maxTokens,
            BattleLlmSettings.Temperature,
            nextResult => result = nextResult);

        isGenerating = false;
        if (!result.Success)
        {
            onComplete?.Invoke(result);
            yield break;
        }

        var cleaned = CleanOutput(result.Text, characterLimit);
        onComplete?.Invoke(string.IsNullOrEmpty(cleaned)
            ? ClarisseLlmResult.Error(ClarisseLlmSettings.GenerationFailedMessage)
            : ClarisseLlmResult.Ok(cleaned));
    }

    private static IEnumerator EnsureModelReady(Action<bool> onComplete)
    {
        var persistentPath = ClarisseLlmSettings.GetPersistentModelPath();
        if (File.Exists(persistentPath))
        {
            onComplete?.Invoke(true);
            yield break;
        }

        var directory = ClarisseLlmSettings.GetPersistentModelDirectory();
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var streamingPath = ClarisseLlmSettings.GetStreamingAssetsModelPath();
#if UNITY_ANDROID && !UNITY_EDITOR
        using (var request = UnityWebRequest.Get(streamingPath))
        {
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                onComplete?.Invoke(false);
                yield break;
            }

            File.WriteAllBytes(persistentPath, request.downloadHandler.data);
        }
#else
        if (!File.Exists(streamingPath))
        {
            onComplete?.Invoke(false);
            yield break;
        }

        File.Copy(streamingPath, persistentPath, false);
        yield return null;
#endif

        onComplete?.Invoke(File.Exists(persistentPath));
    }

    private static string CleanOutput(string output, int characterLimit)
    {
        if (string.IsNullOrWhiteSpace(output))
        {
            return string.Empty;
        }

        var cleaned = RemoveThinking(output).Replace("\r", " ").Replace("\n", " ").Trim();
        cleaned = cleaned
            .Replace("「", string.Empty)
            .Replace("」", string.Empty)
            .Replace("『", string.Empty)
            .Replace("』", string.Empty)
            .Replace("\"", string.Empty);

        var labels = new[] { "質問:", "質問：", "反論:", "反論：", "講評:", "講評：", "回答:", "回答：" };
        foreach (var label in labels)
        {
            if (cleaned.StartsWith(label, StringComparison.Ordinal))
            {
                cleaned = cleaned.Substring(label.Length).Trim();
                break;
            }
        }

        return cleaned.Length <= characterLimit ? cleaned : cleaned.Substring(0, characterLimit);
    }

    private static string RemoveThinking(string output)
    {
        var endIndex = output.LastIndexOf("</think>", StringComparison.OrdinalIgnoreCase);
        return endIndex >= 0 ? output.Substring(endIndex + "</think>".Length) : output;
    }
}
