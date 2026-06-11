# 作業履歴

## プロジェクト概要

ThinQuest は、対話を通じて思考を鍛える RPG 風プロトタイプ。

ユーザーがテーマやアイデアを入力し、敵キャラクターからの質問・反論・突っ込みに答えながら、自分の考えを整理していく体験を目指す。

MVP ではオンライン AI やローカル LLM は使わず、固定テキストまたはルールベースのテンプレートで進める方針。

## 現在あるシーン

- `TitleScene`
- `HomeScene`
- `ThemeInputScene`
- `EnemySelectScene`
- `BattleScene`
- `ResultScene`

## 今日進めた内容

### バトル画面名の整理

MVP 仕様に合わせて、`GameScene` を `BattleScene` にリネームした。

変更内容:

- `Assets/Scenes/GameScene.unity` -> `Assets/Scenes/BattleScene.unity`
- `Assets/Scripts/GameSceneController.cs` -> `Assets/Scripts/BattleSceneController.cs`
- `GameSceneController` クラス名を `BattleSceneController` に変更
- `EnemySelectSceneController` の遷移先を `BattleScene` に変更
- `ProjectSettings/EditorBuildSettings.asset` の登録シーンを `BattleScene.unity` に変更
- `Assets/Scenes/EnemySelectScene.unity` の serialized field を `battleSceneName: BattleScene` に変更

この変更は `b131100 Add guild hall background` でコミット済み。

### ギルドホール背景対応

`Assets/Scripts/HomeSceneController.cs` に背景画像対応を追加した。

実装内容:

- `guildHallBackground` フィールドを追加
- `Resources.Load<Texture2D>("Backgrounds/GuildHallBackground")` で背景画像を自動ロード
- 背景画像を画面全体に cover 表示
- 背景の上に薄い暗幕を重ねて、中央パネルの文字を読みやすくした
- 配置用フォルダ `Assets/Resources/Backgrounds/` を作成
- 生成したギルドホール背景画像を配置

画像配置先:

`Assets/Resources/Backgrounds/GuildHallBackground.png`

### タイトル画面フォント

`TitleSceneController.cs` の uGUI テキストフォントを一度 `Arial.ttf` に変更したが、Unity 側で以下の例外が出た。

`ArgumentException: Arial.ttf is no longer a valid built in font. Please use LegacyRuntime.ttf`

そのため、最終的に `LegacyRuntime.ttf` に戻した。

現在の指定:

`Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")`

## 確認済み

`dotnet build thinquest.sln --no-restore` は成功。

結果:

- 警告 0
- エラー 0

## 直近で追加・変更した主なファイル

- `Assets/Scripts/HomeSceneController.cs`
- `Assets/Scripts/EnemySelectSceneController.cs`
- `Assets/Scripts/BattleSceneController.cs`
- `Assets/Scenes/BattleScene.unity`
- `Assets/Scenes/EnemySelectScene.unity`
- `ProjectSettings/EditorBuildSettings.asset`
- `Assets/Resources/Backgrounds/`
- `AGENT.md`

## 作業記録ルール

`AGENT.md` に、ユーザーから「ここまでの作業をまとめてください」と指示された場合は `log_next_feature.md` と `history.md` の 2 ファイルにまとめて出力する、というルールを追記した。
