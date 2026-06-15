using UnityEngine;

internal static class ClarisseDialogue
{
    public const int InputCharacterLimit = 200;

    private static readonly string[] CharacterResourcePaths =
    {
        "Characters/ClarisseThinking",
        "Characters/ClarisseNormal"
    };

    private static readonly string[] TapLines =
    {
        "今日も少しだけ、考える練習をしていきましょうですわ。",
        "焦らなくて大丈夫ですわ。言葉は少しずつ整えていけばよいのです。",
        "強い考えは、最初から強いわけではありませんわ。磨くものです。",
        "本日の敵は、なかなか手ごわそうですわね。",
        "一問だけでも十分ですわ。継続こそ、いちばん上品な勝利です。",
        "今日はどんな考えを鍛えますの？",
        "小さな違和感も、立派なテーマになりますわ。",
        "迷ったら、最近もやっとしたことから始めてみましょう。"
    };

    private static readonly string[] TiredReplies =
    {
        "お疲れさまですわ。今日は一問だけでも十分です。言葉にする練習を、小さく整えるところから始めましょう。",
        "無理に戦わなくても大丈夫ですわ。小さく整える日も大切です。短い一言だけ、そっと置いてみましょう。"
    };

    private static readonly string[] ThemeReplies =
    {
        "最近もやっとしたことを一つ選ぶと、良いテーマになりますわ。大きく考えず、まず一文にしてみましょう。",
        "誰かに説明したいことや、少し引っかかった出来事がおすすめですわ。そこから問いを一つ作れますの。"
    };

    private static readonly string[] DifficultReplies =
    {
        "勝つことより、考えを少し整えることが大切ですわ。難しかった点を一つだけ言葉にしてみましょう。",
        "難しいと感じたところこそ、伸びる場所ですわ。答えを急がず、引っかかった理由を探してみましょう。"
    };

    private static readonly string[] MotivationReplies =
    {
        "今日は一言だけ書く、を勝利条件にしてしまいましょう。軽い一歩でも、考える習慣には十分ですわ。",
        "やる気が少ない日ほど、軽い一歩で十分ですわ。まずは気になる言葉を一つだけ選んでみましょう。"
    };

    private static readonly string[] DefaultReplies =
    {
        "その言葉、少しテーマにできそうですわね。何が気になったのか、一つだけ説明の練習をしてみましょう。",
        "なるほどですわ。では、それを一つの問いにしてみましょう。急がず、短い言葉から整えれば十分ですわ。",
        "よろしければ、その考えをもう少しだけ言葉にしてみましょう。小さな違和感も、立派な入口ですわ。"
    };

    public static Texture2D LoadRandomCharacter()
    {
        return LoadRandomCharacter(null);
    }

    public static Texture2D LoadRandomCharacter(Texture2D excludedCharacter)
    {
        var startIndex = Random.Range(0, CharacterResourcePaths.Length);
        Texture2D fallbackCharacter = null;

        for (var i = 0; i < CharacterResourcePaths.Length; i++)
        {
            var resourcePath = CharacterResourcePaths[(startIndex + i) % CharacterResourcePaths.Length];
            var character = Resources.Load<Texture2D>(resourcePath);
            if (character != null)
            {
                if (fallbackCharacter == null)
                {
                    fallbackCharacter = character;
                }

                if (character != excludedCharacter)
                {
                    return character;
                }

                continue;
            }

            Debug.LogWarning($"Character texture 'Resources/{resourcePath}' could not be loaded.");
        }

        return fallbackCharacter;
    }

    public static string ChooseTapLine()
    {
        return ChooseRandom(TapLines);
    }

    public static string CreateReply(string input)
    {
        if (ContainsAny(input, "疲れた", "つかれた", "しんどい"))
        {
            return ChooseRandom(TiredReplies);
        }

        if (ContainsAny(input, "テーマ", "何を", "なにを"))
        {
            return ChooseRandom(ThemeReplies);
        }

        if (ContainsAny(input, "勝てない", "むずかしい", "難しい"))
        {
            return ChooseRandom(DifficultReplies);
        }

        if (ContainsAny(input, "やる気", "めんどい", "面倒"))
        {
            return ChooseRandom(MotivationReplies);
        }

        return ChooseRandom(DefaultReplies);
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

    private static string ChooseRandom(string[] lines)
    {
        return lines[Random.Range(0, lines.Length)];
    }
}
