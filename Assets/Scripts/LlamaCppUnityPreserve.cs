using System;
using LLMUnity;
using UnityEngine.Scripting;

[Preserve]
internal static class LlamaCppUnityPreserve
{
    [Preserve]
    private static readonly Type[] RuntimeTypes =
    {
        typeof(LLM),
        typeof(LLMAgent),
        typeof(LLMCharacter),
        typeof(LLMClient),
        typeof(LLMUnitySetup)
#if ENABLE_IL2CPP
        ,
        typeof(IL2CPP_Logging),
        typeof(IL2CPP_Completion)
#endif
    };
}
