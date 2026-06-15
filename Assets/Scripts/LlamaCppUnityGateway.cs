using System.Collections;
using System;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

internal sealed class LlamaCppUnityGateway
{
    private const string RuntimeRootName = "ClarisseLocalLlmRuntime";
    private const string LlmTypeName = "LLMUnity.LLM";
    private const string LlmAgentTypeName = "LLMUnity.LLMAgent";
    private const string LlmUnitySetupTypeName = "LLMUnity.LLMUnitySetup";

    private Component llm;
    private Component llmAgent;
    private string loadedModelPath;

    public IEnumerator Generate(string modelPath, string prompt, int maxTokens, float temperature, System.Action<ClarisseLlmResult> onComplete)
    {
        yield return GenerateWithLlamaCppUnity(modelPath, prompt, maxTokens, temperature, onComplete);
    }

    private IEnumerator GenerateWithLlamaCppUnity(
        string modelPath,
        string prompt,
        int maxTokens,
        float temperature,
        System.Action<ClarisseLlmResult> onComplete)
    {
        yield return EnsureRuntime(modelPath, maxTokens, temperature);

        if (llm == null || llmAgent == null)
        {
            onComplete?.Invoke(ClarisseLlmResult.Error(ClarisseLlmSettings.LlamaCppUnityDisabledMessage));
            yield break;
        }

        if (GetBoolProperty(llm, "failed"))
        {
            onComplete?.Invoke(ClarisseLlmResult.Error(ClarisseLlmSettings.GenerationFailedMessage));
            yield break;
        }

        var chatTask = InvokeChat(prompt);
        if (chatTask == null)
        {
            onComplete?.Invoke(ClarisseLlmResult.Error(ClarisseLlmSettings.GenerationFailedMessage));
            yield break;
        }

        yield return WaitForTask(chatTask);

        if (chatTask.IsFaulted || chatTask.IsCanceled)
        {
            onComplete?.Invoke(ClarisseLlmResult.Error(ClarisseLlmSettings.GenerationFailedMessage));
            yield break;
        }

        onComplete?.Invoke(ClarisseLlmResult.Ok(chatTask.Result));
    }

    private IEnumerator EnsureRuntime(string modelPath, int maxTokens, float temperature)
    {
        if (!IsLlmUnityAvailable())
        {
            yield break;
        }

        yield return WaitForLlmUnitySetup();

        if (llm != null && GetBoolProperty(llm, "failed"))
        {
            DestroyRuntime();
        }

        if (llm != null && llmAgent != null && loadedModelPath == modelPath && GetBoolProperty(llm, "started"))
        {
            yield break;
        }

        if (llm != null && loadedModelPath != modelPath)
        {
            DestroyRuntime();
        }

        if (llm == null || llmAgent == null)
        {
            if (!CreateRuntime(modelPath, maxTokens, temperature))
            {
                yield break;
            }

            yield return null;
        }

        var readyTask = InvokeTask(llm, "WaitUntilReady");
        if (readyTask != null)
        {
            yield return WaitForTask(readyTask);
        }
    }

    private void DestroyRuntime()
    {
        if (llm != null)
        {
            UnityEngine.Object.Destroy(llm.transform.root.gameObject);
        }

        llm = null;
        llmAgent = null;
        loadedModelPath = null;
    }

    private static bool IsLlmUnityAvailable()
    {
        return FindType(LlmTypeName) != null &&
            FindType(LlmAgentTypeName) != null &&
            FindType(LlmUnitySetupTypeName) != null;
    }

    private static IEnumerator WaitForLlmUnitySetup()
    {
        var setupType = FindType(LlmUnitySetupTypeName);
        if (setupType == null)
        {
            yield break;
        }

        var progressField = setupType.GetField("libraryProgress", BindingFlags.Static | BindingFlags.Public);
        if (progressField == null)
        {
            yield break;
        }

        while ((float)progressField.GetValue(null) < 1f)
        {
            yield return null;
        }
    }

    private bool CreateRuntime(string modelPath, int maxTokens, float temperature)
    {
        var llmType = FindType(LlmTypeName);
        var llmAgentType = FindType(LlmAgentTypeName);
        if (llmType == null || llmAgentType == null)
        {
            return false;
        }

        var runtimeObject = new GameObject(RuntimeRootName);
        runtimeObject.SetActive(false);
        UnityEngine.Object.DontDestroyOnLoad(runtimeObject);

        llm = runtimeObject.AddComponent(llmType);
        SetMember(llm, "model", modelPath);
        SetMember(llm, "numThreads", -1);
        SetMember(llm, "numGPULayers", 0);
        SetMember(llm, "parallelPrompts", 1);
        SetMember(llm, "contextSize", ClarisseLlmSettings.ContextSize);
        SetMember(llm, "batchSize", ClarisseLlmSettings.BatchSize);
        SetMember(llm, "dontDestroyOnLoad", true);

        llmAgent = runtimeObject.AddComponent(llmAgentType);
        SetMember(llmAgent, "llm", llm);
        SetMember(llmAgent, "systemPrompt", ClarisseLlmSettings.GetSystemPrompt());
        SetMember(llmAgent, "numPredict", maxTokens);
        SetMember(llmAgent, "temperature", temperature);
        SetMember(llmAgent, "topK", 40);
        SetMember(llmAgent, "topP", 0.9f);
        SetMember(llmAgent, "repeatPenalty", 1.1f);
        SetMember(llmAgent, "cachePrompt", false);

        loadedModelPath = modelPath;
        runtimeObject.SetActive(true);
        return true;
    }

    private Task<string> InvokeChat(string prompt)
    {
        var method = llmAgent.GetType().GetMethod(
            "Chat",
            new[] { typeof(string), typeof(Action<string>), typeof(Action), typeof(bool) });

        if (method == null)
        {
            return null;
        }

        return method.Invoke(llmAgent, new object[] { prompt, null, null, false }) as Task<string>;
    }

    private static Task InvokeTask(Component target, string methodName)
    {
        var method = target.GetType().GetMethod(methodName, Type.EmptyTypes);
        return method?.Invoke(target, null) as Task;
    }

    private static Type FindType(string typeName)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var type = assembly.GetType(typeName);
            if (type != null)
            {
                return type;
            }
        }

        return null;
    }

    private static bool GetBoolProperty(Component target, string propertyName)
    {
        var property = target.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
        return property != null && property.PropertyType == typeof(bool) && (bool)property.GetValue(target);
    }

    private static void SetMember(Component target, string memberName, object value)
    {
        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        var property = target.GetType().GetProperty(memberName, flags);
        if (property != null && property.CanWrite)
        {
            property.SetValue(target, value);
            return;
        }

        var field = target.GetType().GetField(memberName, flags);
        if (field != null)
        {
            field.SetValue(target, value);
        }
    }

    private static IEnumerator WaitForTask(Task task)
    {
        while (!task.IsCompleted)
        {
            yield return null;
        }
    }
}
