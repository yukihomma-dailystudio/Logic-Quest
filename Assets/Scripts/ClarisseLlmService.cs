using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

internal sealed class ClarisseLlmService
{
    private readonly LlamaCppUnityGateway gateway = new LlamaCppUnityGateway();
    private readonly List<ConversationTurn> conversationHistory = new List<ConversationTurn>();
    private bool isGenerating;

    public IEnumerator GenerateTapLine(Action<string> onComplete)
    {
        yield return Generate(ClarisseLlmSettings.BuildTapPrompt(BuildConversationContext()), null, onComplete);
    }

    public IEnumerator GenerateReply(string input, Action<string> onComplete)
    {
        var trimmedInput = TrimToLimit(input);
        if (ContainsNgWord(trimmedInput))
        {
            onComplete?.Invoke(ClarisseLlmSettings.NgReply);
            yield break;
        }

        yield return Generate(
            ClarisseLlmSettings.BuildReplyPrompt(trimmedInput, BuildConversationContext()),
            trimmedInput,
            onComplete);
    }

    private IEnumerator Generate(string prompt, string userInput, Action<string> onComplete)
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

        var response = result.Success ? CleanOutput(result.Text) : result.Text;
        if (result.Success)
        {
            AddConversationTurn(userInput, response);
        }

        onComplete?.Invoke(response);
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

    private static string CleanOutput(string output)
    {
        var trimmed = string.IsNullOrEmpty(output) ? ClarisseLlmSettings.GenerationFailedMessage : output.Trim();
        trimmed = ExtractDialogueLine(trimmed);
        trimmed = RemoveDialogueMarks(trimmed);

        if (trimmed.Length <= ClarisseLlmSettings.OutputCharacterLimit)
        {
            return trimmed;
        }

        return trimmed.Substring(0, ClarisseLlmSettings.OutputCharacterLimit);
    }

    private static string ExtractDialogueLine(string output)
    {
        var normalized = output.Replace("\r\n", "\n").Replace('\r', '\n');
        var lines = normalized.Split('\n');

        for (var i = lines.Length - 1; i >= 0; i--)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrEmpty(line))
            {
                continue;
            }

            var colonIndex = Mathf.Max(line.LastIndexOf(':'), line.LastIndexOf('：'));
            if (colonIndex >= 0 && colonIndex + 1 < line.Length)
            {
                line = line.Substring(colonIndex + 1).Trim();
            }

            if (!LooksLikeInstructionLabel(line))
            {
                return line;
            }
        }

        return normalized.Trim();
    }

    private static string RemoveDialogueMarks(string output)
    {
        return output
            .Replace("「", string.Empty)
            .Replace("」", string.Empty)
            .Replace("『", string.Empty)
            .Replace("』", string.Empty)
            .Replace("\"", string.Empty)
            .Replace("。", string.Empty)
            .Trim();
    }

    private string BuildConversationContext()
    {
        if (conversationHistory.Count == 0)
        {
            return string.Empty;
        }

        var builder = new StringBuilder();
        var startIndex = Mathf.Max(0, conversationHistory.Count - ClarisseLlmSettings.ConversationHistoryTurnLimit);
        for (var i = startIndex; i < conversationHistory.Count; i++)
        {
            var turn = conversationHistory[i];
            if (!string.IsNullOrEmpty(turn.UserInput))
            {
                builder.Append("ユーザー: ");
                builder.AppendLine(turn.UserInput);
            }

            if (!string.IsNullOrEmpty(turn.ClarisseReply))
            {
                builder.Append("クラリス: ");
                builder.AppendLine(turn.ClarisseReply);
            }
        }

        return builder.ToString().TrimEnd();
    }

    private void AddConversationTurn(string userInput, string clarisseReply)
    {
        conversationHistory.Add(new ConversationTurn(userInput, clarisseReply));
        while (conversationHistory.Count > ClarisseLlmSettings.ConversationHistoryTurnLimit)
        {
            conversationHistory.RemoveAt(0);
        }
    }

    private static bool LooksLikeInstructionLabel(string line)
    {
        if (line.Contains("？") || line.Contains("?"))
        {
            return false;
        }

        return line.Length <= 32 && ContainsAny(line, "返答", "出力", "問い", "共感ひとこと", "小さな次の一歩", "形式");
    }

    private static bool ContainsAny(string input, params string[] keywords)
    {
        foreach (var keyword in keywords)
        {
            if (input.Contains(keyword))
            {
                return true;
            }
        }

        return false;
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

    private readonly struct ConversationTurn
    {
        public ConversationTurn(string userInput, string clarisseReply)
        {
            UserInput = userInput;
            ClarisseReply = clarisseReply;
        }

        public string UserInput { get; }
        public string ClarisseReply { get; }
    }
}
