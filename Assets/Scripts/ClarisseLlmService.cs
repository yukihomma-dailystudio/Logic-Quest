using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

internal sealed class ClarisseLlmService
{
    private readonly LlamaCppUnityGateway gateway = new LlamaCppUnityGateway();
    private bool isGenerating;

    public IEnumerator GenerateTapLine(Action<string> onComplete)
    {
        yield return Generate(ClarisseLlmSettings.BuildTapPrompt(), onComplete);
    }

    public IEnumerator GenerateReply(string input, Action<string> onComplete)
    {
        var trimmedInput = TrimToLimit(input);
        if (ContainsNgWord(trimmedInput))
        {
            onComplete?.Invoke(ClarisseLlmSettings.NgReply);
            yield break;
        }

        yield return Generate(ClarisseLlmSettings.BuildReplyPrompt(trimmedInput), onComplete);
    }

    private IEnumerator Generate(string prompt, Action<string> onComplete)
    {
        if (isGenerating)
        {
            onComplete?.Invoke(ClarisseLlmSettings.BusyMessage);
            yield break;
        }

        isGenerating = true;

        var modelReady = false;
        yield return EnsureModelReady(ready => modelReady = ready);

        if (!modelReady)
        {
            isGenerating = false;
            onComplete?.Invoke(ClarisseLlmSettings.GetMissingModelMessage());
            yield break;
        }

        ClarisseLlmResult result = default;
        var receivedResult = false;
        yield return gateway.Generate(
            ClarisseLlmSettings.GetPersistentModelPath(),
            prompt,
            ClarisseLlmSettings.MaxGeneratedTokens,
            ClarisseLlmSettings.Temperature,
            nextResult =>
            {
                result = nextResult;
                receivedResult = true;
            });

        isGenerating = false;

        if (!receivedResult)
        {
            onComplete?.Invoke(ClarisseLlmSettings.GenerationFailedMessage);
            yield break;
        }

        onComplete?.Invoke(result.Success ? TrimOutput(result.Text) : result.Text);
    }

    private static IEnumerator EnsureModelReady(Action<bool> onComplete)
    {
        var persistentModelPath = ClarisseLlmSettings.GetPersistentModelPath();
        if (File.Exists(persistentModelPath))
        {
            onComplete?.Invoke(true);
            yield break;
        }

        var persistentModelDirectory = ClarisseLlmSettings.GetPersistentModelDirectory();
        if (!Directory.Exists(persistentModelDirectory))
        {
            Directory.CreateDirectory(persistentModelDirectory);
        }

        var streamingAssetsModelPath = ClarisseLlmSettings.GetStreamingAssetsModelPath();
#if UNITY_ANDROID && !UNITY_EDITOR
        using (var request = UnityWebRequest.Get(streamingAssetsModelPath))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"Could not copy GGUF model from StreamingAssets: {request.error}");
                onComplete?.Invoke(false);
                yield break;
            }

            File.WriteAllBytes(persistentModelPath, request.downloadHandler.data);
        }
#else
        if (!File.Exists(streamingAssetsModelPath))
        {
            onComplete?.Invoke(false);
            yield break;
        }

        File.Copy(streamingAssetsModelPath, persistentModelPath, false);
        yield return null;
#endif

        onComplete?.Invoke(File.Exists(persistentModelPath));
    }

    private static string TrimToLimit(string input)
    {
        var trimmed = string.IsNullOrEmpty(input) ? string.Empty : input.Trim();
        if (trimmed.Length <= ClarisseLlmSettings.InputCharacterLimit)
        {
            return trimmed;
        }

        return trimmed.Substring(0, ClarisseLlmSettings.InputCharacterLimit);
    }

    private static string TrimOutput(string output)
    {
        var trimmed = string.IsNullOrEmpty(output) ? ClarisseLlmSettings.GenerationFailedMessage : output.Trim();
        if (trimmed.Length <= ClarisseLlmSettings.OutputCharacterLimit)
        {
            return trimmed;
        }

        return trimmed.Substring(0, ClarisseLlmSettings.OutputCharacterLimit);
    }

    private static bool ContainsNgWord(string input)
    {
        var normalizedInput = ClarisseLlmSettings.NormalizeForNgCheck(input);
        foreach (var ngWord in ClarisseLlmSettings.NgWords)
        {
            var normalizedNgWord = ClarisseLlmSettings.NormalizeForNgCheck(ngWord);
            if (!string.IsNullOrEmpty(normalizedNgWord) && normalizedInput.Contains(normalizedNgWord))
            {
                return true;
            }
        }

        return false;
    }
}
