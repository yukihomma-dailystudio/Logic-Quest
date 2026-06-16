using System.IO;
using System.Text;
using UnityEngine;

internal static class ClarisseLlmSettings
{
    public const string ModelFileName = "Qwen3-0.6B-Q4_K_M.gguf";
    public const string ModelsDirectoryName = "Models";
    public const int InputCharacterLimit = 50;
    public const int OutputCharacterLimit = 80;
    public const int ConversationHistoryTurnLimit = 4;
    public const int MaxGeneratedTokens = 48;
    public const float Temperature = 0.7f;
    public const int ContextSize = 1024;
    public const int BatchSize = 128;
    public const string ThinkingMessage = "考え中...";
    public const string NgReply = "・・・";
    public const string LlamaCppUnityDisabledMessage = "ローカルLLM機能がまだ有効化されていません。";
    public const string GenerationFailedMessage = "うまく考えをまとめられませんでした。もう一度お試しください。";
    public const string BusyMessage = "クラリスはまだ考え中です。";
    public const string TiredReplyPrefix = "お疲れ様ですわ、";
    public const string AnxietyReplyPrefix = "大丈夫ですわ、冒険者さん。";
    public const string HesitationReplyInstruction = "選択肢を増やさず、最初の一歩を一つだけ返してください。";
    private const string BasePrompt =
        "あなたは思考トレーニングRPG ThinQuest のギルド受付、クラリスです。" +
        "クラリスはあなた自身の名前です。ユーザーやプレイヤーをクラリスと呼んではいけません。" +
        "ユーザーやプレイヤーに呼びかける時の呼び名は必ず「冒険者さん」だけにしてください。" +
        "クラリスは丁寧で少しお嬢様口調ですが、話しすぎません。" +
        "ユーザーに答えを与えるのではなく、考えを一歩だけ前に進める短い問いを返します。" +
        "返答は日本語で80文字以内。" +
        "「ですわ」「ですの」は自然な時だけ使い、毎回使いすぎないでください。" +
        "「」はつけない、。はつけない" +
        "返答は、共感ひとこと + 小さな問い、または小さな次の一歩にしてください。";
    private const string ConversationPrompt =
        "直近の会話履歴がある場合は文脈として使います。" +
        "ただし最新のユーザー入力への返答を最優先にし、前の話を無視した別話題にしないでください。" +
        "最新のユーザー入力が質問なら、まずその質問に直接返してください。" +
        "代名詞や「それ」「さっき」などは会話履歴から補ってください。";

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

    public static readonly string[] TiredWords =
    {
        "疲れた",
        "つかれた",
        "疲れ",
        "しんどい",
        "しんど",
        "だるい",
        "休みたい",
        "眠い"
    };

    public static readonly string[] AnxietyWords =
    {
        "不安",
        "怖い",
        "こわい",
        "心配",
        "自信ない",
        "自信がない",
        "できるかな"
    };

    public static readonly string[] HesitationWords =
    {
        "迷う",
        "迷って",
        "決められない",
        "決まらない",
        "わからない",
        "分からない",
        "悩む",
        "悩んで"
    };

    public static string BuildTapPrompt(string conversationContext)
    {
        return
            BasePrompt + "\n" +
            ConversationPrompt + "\n" +
            FormatConversationContext(conversationContext) +
            "最新の状況: ユーザーがクラリスに話しかけようとしています。";
    }

    public static string GetSystemPrompt()
    {
        return BasePrompt;
    }

    public static string GetMissingModelMessage()
    {
        return $"モデルファイルが見つかりません。StreamingAssets/Models/{ModelFileName} を配置してください。";
    }

    public static string BuildReplyPrompt(string userInput, string conversationContext, string replyInstruction)
    {
        return
            BasePrompt + "\n" +
            ConversationPrompt + "\n" +
            FormatConversationContext(conversationContext) +
            FormatReplyInstruction(replyInstruction) +
            $"最新のユーザー入力: {userInput}";
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

    private static string FormatConversationContext(string conversationContext)
    {
        if (string.IsNullOrEmpty(conversationContext))
        {
            return "会話履歴: なし\n";
        }

        return $"会話履歴:\n{conversationContext}\n";
    }

    private static string FormatReplyInstruction(string replyInstruction)
    {
        if (string.IsNullOrEmpty(replyInstruction))
        {
            return string.Empty;
        }

        return $"今回の追加方針: {replyInstruction}\n";
    }
}
