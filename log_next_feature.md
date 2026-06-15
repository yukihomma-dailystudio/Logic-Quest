# 次の作業内容

## 現在の状態

作業ブランチは `codex-local-llm-clarisse`。

`main` / `origin/main` は以下のコミットが先端。

- `edef476 Improve enemy select hints and update logs`

別ブランチ `result-scene-enhancement` は push 済み。

- `32cd134 Enhance result scene presentation`
- 内容: ResultScene の戦利品交換商店背景、羊皮紙リザルトUI、獲得EXPの光るアニメーション、AGENT.md のレイアウト方針追記

現在のブランチには未コミット変更がある。

## 未コミットの変更

### クラリスのローカルLLM対応

追加ファイル:

- `Assets/Scripts/ClarisseLlmSettings.cs`
- `Assets/Scripts/ClarisseLlmSettings.cs.meta`
- `Assets/Scripts/ClarisseLlmResult.cs`
- `Assets/Scripts/ClarisseLlmResult.cs.meta`
- `Assets/Scripts/ClarisseLlmService.cs`
- `Assets/Scripts/ClarisseLlmService.cs.meta`
- `Assets/Scripts/LlamaCppUnityGateway.cs`
- `Assets/Scripts/LlamaCppUnityGateway.cs.meta`
- `Assets/StreamingAssets.meta`
- `Assets/StreamingAssets/Models/README.txt`
- `Assets/StreamingAssets/Models/README.txt.meta`

変更ファイル:

- `Assets/Scripts/HomeSceneController.cs`
- `ProjectSettings/ProjectSettings.asset`

主な内容:

- ホーム画面のクラリス会話を固定文言から `ClarisseLlmService` 経由に変更
- クラリスをタップしたときも、入力送信時もローカルLLM生成を試す
- 生成中は `考え中...` を表示し、ボタンや入力欄を一時的にロックする
- 入力は 50 文字、出力は 80 文字に制限
- NGワードが含まれる入力には `・・・` を返す
- `StreamingAssets/Models/Qwen3-0.6B-Q4_K_M.gguf` を想定
- 実行時は `Application.persistentDataPath/Models` へモデルをコピーして使う設計
- `LlamaCppUnityGateway` は `LLAMA_CPP_UNITY` シンボルが未定義なら無効化メッセージを返すスタブ
- `ProjectSettings.asset` では `AndroidTargetArchitectures` が `3` から `2` に変更されている

注意:

- `LlamaCppUnityGateway.GenerateWithLlamaCppUnity` はまだ具体的な llama.cpp Unity パッケージ API に接続していない
- モデル本体 `.gguf` はリポジトリには追加されていない
- `StreamingAssets/Models/README.txt` だけが追加されている
- `LLAMA_CPP_UNITY` の define 設定はまだ確認が必要

### タイトル画面の空・雲ループ試作

追加ファイル:

- `Assets/Resources/Backgrounds/GuildEntranceSkyLoop.png`
- `Assets/Resources/Backgrounds/GuildEntranceSkyLoop.png.meta`

変更ファイル:

- `Assets/Scripts/TitleSceneController.cs`

主な内容:

- 以前の白い雲スプライトを前面で流す実装を削除
- `SkyLoopLayer` を追加し、`GuildEntranceSkyLoop.png` を 2 枚横に並べて背面でループ移動する実装へ変更
- 既存の `CloudLayer` がシーンに残っている場合は非表示にする
- `Background` は前景として `SkyLoopLayer` より前、UIパネルは最前面になるよう sibling order を調整

注意:

- `GuildEntranceBackground.png` の空部分を透過して前景化する試行をしたが、建物や旗まわりの切り抜きが不自然だったため撤回済み
- 現在の `GuildEntranceBackground.png` は透過前の元画像に戻っている
- そのため現状のままだと、背面の `SkyLoopLayer` は前景画像に隠れてほぼ見えない
- タイトル画面の雲ループ方針は、前景画像をきれいに分離できる方法を決め直す必要がある

## 次に行う作業

優先は `codex-local-llm-clarisse` の整理。

1. クラリスLLM対応をこのブランチで続けるか、タイトル雲背景試作を別ブランチへ分けるか決める。
2. `TitleSceneController.cs` と `GuildEntranceSkyLoop.png` を残すか、いったん撤回するか決める。
3. クラリスLLMについて、`LlamaCppUnityGateway` を実際の llama.cpp Unity パッケージ API に接続する。
4. `LLAMA_CPP_UNITY` define と Android の target architecture 変更が必要か確認する。
5. モデル配置手順を `StreamingAssets/Models/README.txt` に十分書けているか確認する。
6. `dotnet build thinquest.sln --no-restore` と `git diff --check` を実行する。

## タイトル雲背景についての次案

今の元背景画像から自動マスクで空だけを抜くのは、遠景建物・旗・雲の境界が複雑で破綻しやすい。

次にやるなら以下のどれかがよい。

- 元背景を透過加工するのではなく、前景専用画像を生成し直す
- ギルド建物と街並みを含む前景レイヤー、空雲レイヤーを最初から分けて生成する
- タイトル背景全体を作り直し、空の領域を広く取りつつ建物を右寄せにする
- いったん雲アニメーションは保留し、現在の静止背景へ戻す

## 作業後に確認すること

- `dotnet build thinquest.sln --no-restore`
- `git diff --check`
- HomeScene でクラリスをタップした時の表示
- HomeScene でクラリス入力を送った時の表示
- モデル未配置時に分かりやすいエラーが出ること
- LLM生成中に入力欄やボタンが二重操作されないこと
- TitleScene の背景レイヤー順序が壊れていないこと
