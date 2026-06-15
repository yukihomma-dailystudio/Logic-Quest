# 次の作業内容

## 現在の状態

作業ブランチは `codex-continue-work`。

`origin/codex-continue-work` には以下のコミットまで push 済み。

- `e2d8f81 Refactor home scene controller`
- `aa9f58a Stabilize enemy select layout`
- `b6a2403 Document push confirmation rule`

`AGENT.md` には、ユーザーが明示的に依頼した場合だけ GitHub へ push するルールを追記済み。

## 未コミットの変更

`Assets/Scripts/EnemySelectSceneController.cs` に、敵選択画面の補足テキストを読みやすくする変更が入っている。

内容:

- 上部説明文のフォントを拡大して太字化
- 下部の `左右スワイプで相手を切り替え` を拡大して太字化
- 上部説明文と下部ヒントを薄い黒帯の上に表示
- 下部ヒントの高さを増やし、既存の重なり防止計算へ反映

確認済み:

- `dotnet build thinquest.sln --no-restore`
- `git diff --check`

まだコミットも push もしていない。

## 次に行う作業

次は `ResultScene` の強化を行う。

現在の `Assets/Scripts/ResultSceneController.cs` は IMGUI ベースの簡素な結果表示で、背景、報酬演出、能力値の伸び、次の行動導線がまだ弱い。

目的:

- バトル後に「試練を終えた」手応えを出す
- 獲得 EXP と伸びた能力値を分かりやすく見せる
- 回答、講評、冒険者記録を読みやすく整理する
- 次の行動として `ギルドへ戻る` と `次の巻物` を自然に選べるようにする

## リザルトシーン強化方針

まずは挙動を変えず、表示品質を上げる。

候補:

- 背景を単色からギルド・戦果報告風の見た目へ寄せる
- 中央パネルを結果カードとして再構成する
- `試練の戦果`、`獲得EXP`、`伸びた能力値` を上部で強く見せる
- `挑戦した巻物`、`対峙した相手`、`あなたの返答`、`試練の講評` を読みやすいブロックに分ける
- 冒険者記録に現在ランク、累計EXP、完了した試練を整理して表示する
- ボタンサイズと文言をファンタジー調の UI に合わせる

注意点:

- PlayerPrefs のキーや保存済みデータの読み込み挙動は変えない
- `BattleScene -> ResultScene -> HomeScene / ThemeInputScene` の導線は維持する
- MVP なので大きなシステム追加より、見た目と読みやすさの改善を優先する
- オンライン AI や追加評価ロジックはこの段階では入れない
- 変更後に `dotnet build thinquest.sln --no-restore` と `git diff --check` を行う

## 優先して確認すること

1. `ResultSceneController.cs` の現在の描画構成を整理する。
2. 画面サイズが変わっても見切れにくいレイアウトにする。
3. 報酬表示と講評表示の優先順位を決める。
4. 必要なら ResultScene 用の UI ヘルパーを小さく追加する。

## 作業後に確認すること

- `dotnet build thinquest.sln --no-restore`
- `git diff --check`
- `BattleScene -> ResultScene` で結果データが表示されること
- `ギルドへ戻る` と `次の巻物` のボタン導線
- 画面サイズ変更時にテキストやボタンが重ならないこと
