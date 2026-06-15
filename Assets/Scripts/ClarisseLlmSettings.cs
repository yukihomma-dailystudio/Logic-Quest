using System.IO;
using System.Text;
using UnityEngine;

internal static class ClarisseLlmSettings
{
    public const string ModelFileName = "Qwen3-0.6B-Q4_K_M.gguf";
    public const string ModelsDirectoryName = "Models";
    public const int InputCharacterLimit = 50;
    public const int OutputCharacterLimit = 80;
    public const int MaxGeneratedTokens = 48;
    public const float Temperature = 0.7f;
    public const int ContextSize = 1024;
    public const int BatchSize = 128;
    public const string ThinkingMessage = "考え中...";
    public const string NgReply = "・・・";
    public const string LlamaCppUnityDisabledMessage = "ローカルLLM機能がまだ有効化されていません。";
    public const string GenerationFailedMessage = "うまく考えをまとめられませんでした。もう一度お試しください。";
    public const string BusyMessage = "クラリスはまだ考え中です。";

    public static readonly string[] NgWords =
    {
        "死ね",
        "しね",
        "殺す",
        "ころす",
        "自殺",
        "差別",
        "エロ",
        "セックス",
        "暴力"
    };

    public static string BuildTapPrompt()
    {
        return
            "あなたは思考トレーニングRPG ThinQuest の案内役クラリスです。\n" +
            "ユーザーに答えを与えず、今日の考える練習をやさしく促してください。\n" +
            "日本語で一言だけ、80文字以内で返してください。";
    }

    public static string GetSystemPrompt()
    {
        return
            "あなたは思考トレーニングRPG ThinQuest の案内役クラリスです。\n" +
            "答えを代わりに作らず、ユーザーが自分で考えるための短い一言だけを返します。\n" +
            "日本語で、やさしく上品に、80文字以内で返してください。";
    }

    public static string GetMissingModelMessage()
    {
        return $"モデルファイルが見つかりません。StreamingAssets/Models/{ModelFileName} を配置してください。";
    }

    public static string BuildReplyPrompt(string userInput)
    {
        return
            "あなたは思考トレーニングRPG ThinQuest の案内役クラリスです。\n" +
            "ユーザーに答えを与えすぎず、考えを少し整えるための短い返答をしてください。\n" +
            "日本語で80文字以内。質問か軽い励ましを一つだけ。\n" +
            $"ユーザーの一言: {userInput}";
    }

    public static string GetStreamingAssetsModelPath()
    {
        return CombineStreamingAssetsPath(ModelsDirectoryName, ModelFileName);
    }

    public static string GetPersistentModelDirectory()
    {
        return Path.Combine(Application.persistentDataPath, ModelsDirectoryName);
    }

    public static string GetPersistentModelPath()
    {
        return Path.Combine(GetPersistentModelDirectory(), ModelFileName);
    }

    public static string NormalizeForNgCheck(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        var normalized = input.Normalize(NormalizationForm.FormKC).ToLowerInvariant();
        var builder = new StringBuilder(normalized.Length);
        foreach (var character in normalized)
        {
            if (!char.IsWhiteSpace(character))
            {
                builder.Append(character);
            }
        }

        return builder.ToString();
    }

    private static string CombineStreamingAssetsPath(string directoryName, string fileName)
    {
        var path = Path.Combine(Application.streamingAssetsPath, directoryName, fileName);
        return path.Replace('\\', '/');
    }
}
