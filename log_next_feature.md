# 次の作業内容

## 現在の状態

作業ブランチは `codex-local-llm-clarisse`。

ローカルと `origin/codex-local-llm-clarisse` は一致しており、作業ツリーは clean。

直近で push 済みの主なコミット:

- `ad60df8d Add Clarisse dialogue smoke test tooling`
- `d0598614 Add anger guidance for Clarisse`
- `f32fc136 Add failure reply prefix for Clarisse`
- `5864912f Add achievement reply prefix for Clarisse`
- `3e0c514e Add low motivation reply prefix for Clarisse`
- `aeed8248 Add hesitation guidance for Clarisse`
- `979684e4 Add anxiety reply prefix for Clarisse`
- `3327da0e Fix Clarisse player address`
- `a3cc3aa6 Add tired reply prefix for Clarisse`
- `8689034a Improve Clarisse role context`
- `cabbad2c Update local Clarisse LLM integration`

## クラリスLLM対応の整理結果

HomeScene のクラリス会話は `ClarisseLlmService` 経由でローカルLLM生成する形になっている。

主な内容:

- クラリスをタップした時、入力送信時にローカルLLM生成を試す
- 生成中は `考え中...` を表示し、入力欄や送信ボタンをロックする
- 入力は 50 文字、出力は 80 文字に制限
- NGワード検出時は `・・・` を返す
- 直近4ターンの会話履歴をプロンプトに含める
- 最新のユーザー入力への返答を優先するようプロンプトで指示
- クラリスは自分自身の名前であり、ユーザーをクラリスと呼ばないよう指示
- ユーザーへの呼びかけは `冒険者さん` に固定
- LLMUnity 経由で `StreamingAssets/Models/Qwen3-0.6B-Q4_K_M.gguf` を利用する想定
- `LlamaCppUnityGateway` は LLMUnity の `LLM` / `LLMAgent` をリフレクション経由で生成して呼び出す

## 特殊ワード対応

特殊ワードに当たっても基本的にはLLMを呼ぶ。

接頭句を付ける系統:

- 疲労系: `疲れた`, `しんどい`, `眠い`, `だるい`, `休みたい` など
  - 接頭句: `お疲れ様ですわ、`
- 不安系: `不安`, `怖い`, `心配`, `自信ない`, `できるかな` など
  - 接頭句: `大丈夫ですわ、冒険者さん。`
- やる気低下系: `やる気ない`, `めんどい`, `面倒`, `無理`, `やりたくない` など
  - 接頭句: `今日は小さくで十分ですわ、`
- 達成系: `できた`, `やれた`, `終わった`, `クリア`, `勝った`, `達成`, `成功` など
  - 接頭句: `見事ですわ、冒険者さん。`
- 失敗系: `失敗`, `負けた`, `できなかった`, `ミスった`, `間違えた` など
  - 接頭句: `そこまで試したのが大事ですわ、`

追加方針をプロンプトに渡す系統:

- 迷い系: `迷う`, `決められない`, `わからない`, `悩む` など
  - 選択肢を増やさず、最初の一歩を一つだけ返す
- 怒り系: `むかつく`, `腹立つ`, `イライラ`, `嫌い` など
  - 反論せず、感情を一度受けてから、何が引っかかったのかに戻す

## CLI確認

`tools/run_clarisse_dialogue_smoke.ps1` を追加済み。

目的:

- プロダクトコードにテストUIを組み込まず、CLIから固定入力を流してクラリスの返答ログを見る
- 出力は人間が見て、プロンプトや特殊ワード設定の改善点を判断する

使い方:

```powershell
.\tools\run_clarisse_dialogue_smoke.ps1
```

注意:

- Unity Editor で同じプロジェクトを開いたままだと batchmode が起動できない
- 実行時だけ `Assets/Scripts/ClarisseDialogueCliSmokeTest.generated.cs` を生成し、終了後に削除する

## 次に行う作業

次は ResultScene の実機表示問題を直す。

問題:

- リザルト画面の結果部分が実機だと小さく見える
- PC / Editor 上の見え方だけでなく、スマホ実機の解像度・アスペクト比・safe area で確認する必要がある

見るべきファイル:

- `Assets/Scripts/ResultSceneController.cs`

優先方針:

- 結果本文、獲得EXP、能力値、講評、ボタンの表示サイズを実機基準で見直す
- 固定px感の強い IMGUI レイアウトやスケール計算を確認する
- 画面の横幅・縦幅・safe area に対して、結果パネルが小さくなりすぎない制約を入れる
- 店主や背景の見た目より、まず結果情報の読みやすさを優先する

作業後に確認すること:

- `dotnet build thinquest.sln --no-restore`
- `git diff --check`
- Editor の Game view で複数アスペクト比確認
- 可能なら Android 実機で ResultScene の結果部分の文字サイズとボタンサイズを確認
