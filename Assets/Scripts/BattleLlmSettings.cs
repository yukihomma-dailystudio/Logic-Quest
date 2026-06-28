internal static class BattleLlmSettings
{
    public const int ObjectionMaxTokens = 96;
    public const int EvaluationMaxTokens = 128;
    public const int ObjectionCharacterLimit = 110;
    public const int EvaluationCharacterLimit = 180;
    public const float Temperature = 0.65f;

    private const string SystemPrompt =
        "あなたは思考トレーニングRPG ThinQuestの論争相手です。" +
        "ユーザーの代わりに答えを作らず、主張の弱点を確かめる質問や反論を返してください。" +
        "厳しくても侮辱、断定的な人格否定、医療的な効能表現は使いません。" +
        "出力は自然な日本語だけにし、思考過程、見出し、引用符は出力しません。";

    public static string GetSystemPrompt()
    {
        return SystemPrompt;
    }

    public static string BuildObjectionPrompt(string theme, string enemyName)
    {
        return
            "/no_think\n" +
            $"テーマ: {Trim(theme, 220)}\n" +
            $"敵: {enemyName}\n" +
            $"敵の役割: {GetEnemyRole(enemyName)}\n" +
            "この敵として、テーマの弱点を突く最初の質問を一つだけ返してください。" +
            $"{ObjectionCharacterLimit}文字以内。改善案や模範解答は書かないでください。";
    }

    public static string BuildEvaluationPrompt(string theme, string enemyName, string objection, string answer)
    {
        return
            "/no_think\n" +
            $"テーマ: {Trim(theme, 220)}\n" +
            $"敵: {enemyName}\n" +
            $"敵の役割: {GetEnemyRole(enemyName)}\n" +
            $"敵の問い: {Trim(objection, ObjectionCharacterLimit)}\n" +
            $"ユーザーの返答: {Trim(answer, 220)}\n" +
            "返答を論争相手として講評してください。良かった点を一つ、まだ弱い点を一つ、" +
            $"次に考える短い問いを一つ含め、{EvaluationCharacterLimit}文字以内の一段落で返してください。" +
            "点数、人格評価、模範解答は書かないでください。";
    }

    private static string GetEnemyRole(string enemyName)
    {
        switch (enemyName)
        {
            case "論理の騎士":
                return "論理の飛躍、矛盾、根拠不足を見つける";
            case "現実商人":
                return "実用性、需要、継続性を問い直す";
            case "辛口審査官":
                return "利用者が不満に感じる点や離脱理由を指摘する";
            case "倫理の守護者":
                return "安全性、誤解、倫理上の問題を確かめる";
            case "冷徹な投資卿":
                return "成長性、収益性、差別化を厳しく確かめる";
            default:
                return "説明が明確で具体的かを確かめる";
        }
    }

    private static string Trim(string value, int limit)
    {
        var trimmed = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        return trimmed.Length <= limit ? trimmed : trimmed.Substring(0, limit);
    }
}
