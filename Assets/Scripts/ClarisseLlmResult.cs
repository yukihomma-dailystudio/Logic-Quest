internal readonly struct ClarisseLlmResult
{
    public ClarisseLlmResult(bool success, string text)
    {
        Success = success;
        Text = text;
    }

    public bool Success { get; }
    public string Text { get; }

    public static ClarisseLlmResult Ok(string text)
    {
        return new ClarisseLlmResult(true, text);
    }

    public static ClarisseLlmResult Error(string message)
    {
        return new ClarisseLlmResult(false, message);
    }
}
