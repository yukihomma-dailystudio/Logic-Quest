param(
    [string]$UnityPath = "F:\unity\2022.3.62f3\Editor\Unity.exe",
    [string]$ProjectRoot = (Resolve-Path "$PSScriptRoot\..").Path,
    [string[]]$Inputs = @(
        "今日は何から考えればいい？",
        "疲れた",
        "それってどういう意味？",
        "さっきの話をもう少し短く言うと？",
        "クラリス、僕の呼び方は？",
        "明日の目標が決められない",
        "じゃあ最初の一歩は？"
    )
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path -LiteralPath $UnityPath)) {
    throw "Unity.exe was not found: $UnityPath"
}

$assetsScripts = Join-Path $ProjectRoot "Assets\Scripts"
$logsDir = Join-Path $ProjectRoot "Logs"
$tempDir = Join-Path $ProjectRoot "Temp\ClarisseDialogueSmoke"
$generatedScript = Join-Path $assetsScripts "ClarisseDialogueCliSmokeTest.generated.cs"
$generatedMeta = "$generatedScript.meta"
$inputFile = Join-Path $tempDir "inputs.txt"
$logFile = Join-Path $logsDir "ClarisseDialogueSmokeTest.log"

New-Item -ItemType Directory -Force -Path $logsDir | Out-Null
New-Item -ItemType Directory -Force -Path $tempDir | Out-Null
$Inputs | Set-Content -LiteralPath $inputFile -Encoding UTF8

$source = @'
#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class ClarisseDialogueCliSmokeTest
{
    private const string InputsArg = "-clarisseDialogueInputs";
    private const double TimeoutSeconds = 180.0;
    private static readonly string[] DefaultInputs =
    {
        "今日は何から考えればいい？",
        "疲れた",
        "それってどういう意味？",
        "さっきの話をもう少し短く言うと？",
        "クラリス、僕の呼び方は？",
        "明日の目標が決められない",
        "じゃあ最初の一歩は？"
    };

    private static ClarisseLlmService service;
    private static Stack<IEnumerator> activeStack;
    private static int inputIndex;
    private static string[] inputs;
    private static double startedAt;

    public static void Run()
    {
        service = new ClarisseLlmService();
        inputs = LoadInputs();
        inputIndex = -1;
        startedAt = EditorApplication.timeSinceStartup;

        Debug.Log("=== Clarisse dialogue CLI smoke test start ===");
        EditorApplication.update += Tick;
        StartTapLine();
    }

    private static string[] LoadInputs()
    {
        var args = Environment.GetCommandLineArgs();
        for (var i = 0; i + 1 < args.Length; i++)
        {
            if (args[i] == InputsArg && File.Exists(args[i + 1]))
            {
                return File.ReadAllLines(args[i + 1]);
            }
        }

        return DefaultInputs;
    }

    private static void Tick()
    {
        if (EditorApplication.timeSinceStartup - startedAt > TimeoutSeconds)
        {
            Finish(2, "Clarisse dialogue CLI smoke test timed out.");
            return;
        }

        if (activeStack == null)
        {
            StartNextTurn();
            return;
        }

        try
        {
            if (!MoveActiveStack())
            {
                activeStack = null;
            }
        }
        catch (Exception exception)
        {
            Debug.LogError(exception);
            Finish(1, "Clarisse dialogue CLI smoke test failed.");
        }
    }

    private static bool MoveActiveStack()
    {
        while (activeStack.Count > 0)
        {
            var current = activeStack.Peek();
            if (!current.MoveNext())
            {
                activeStack.Pop();
                continue;
            }

            if (current.Current is IEnumerator nested)
            {
                activeStack.Push(nested);
                continue;
            }

            return true;
        }

        return false;
    }

    private static void StartTapLine()
    {
        string reply = null;
        StartRoutine(RunAndLog(
            service.GenerateTapLine(nextReply => reply = nextReply),
            () => Debug.Log($"[ClarisseSmoke][Tap]\nClarisse: {reply}")));
    }

    private static void StartNextTurn()
    {
        inputIndex++;
        if (inputIndex >= inputs.Length)
        {
            Finish(0, "=== Clarisse dialogue CLI smoke test end ===");
            return;
        }

        var input = inputs[inputIndex];
        string reply = null;
        StartRoutine(RunAndLog(
            service.GenerateReply(input, nextReply => reply = nextReply),
            () => Debug.Log($"[ClarisseSmoke][Turn {inputIndex + 1}]\nPlayer: {input}\nClarisse: {reply}")));
    }

    private static void StartRoutine(IEnumerator routine)
    {
        activeStack = new Stack<IEnumerator>();
        activeStack.Push(routine);
    }

    private static IEnumerator RunAndLog(IEnumerator routine, Action log)
    {
        yield return routine;
        log();
    }

    private static void Finish(int exitCode, string message)
    {
        Debug.Log(message);
        EditorApplication.update -= Tick;
        EditorApplication.Exit(exitCode);
    }
}
#endif
'@

try {
    Set-Content -LiteralPath $generatedScript -Value $source -Encoding UTF8

    $unityArgs = @(
        "-batchmode",
        "-projectPath", $ProjectRoot,
        "-logFile", $logFile,
        "-executeMethod", "ClarisseDialogueCliSmokeTest.Run",
        "-clarisseDialogueInputs", $inputFile
    )
    $process = Start-Process -FilePath $UnityPath -ArgumentList $unityArgs -NoNewWindow -PassThru -Wait
    $exitCode = $process.ExitCode
    if (Test-Path -LiteralPath $logFile) {
        Select-String -Path $logFile -Pattern "\[ClarisseSmoke\]|=== Clarisse dialogue|timed out|failed|fatal error|Multiple Unity|Aborting batchmode|Licensing" | ForEach-Object {
            $_.Line
        }
    }

    exit $exitCode
}
finally {
    if (Test-Path -LiteralPath $generatedScript) {
        Remove-Item -LiteralPath $generatedScript -Force
    }
    if (Test-Path -LiteralPath $generatedMeta) {
        Remove-Item -LiteralPath $generatedMeta -Force
    }
}
