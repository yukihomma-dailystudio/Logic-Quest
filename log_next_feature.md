# 次の作業内容

## 現在の状態

`enemy-select-adjustments` ブランチを `main` から作成した。

`EnemySelectScene` の見えにくい問題の解決を次に行う。

現在の実装状況:

- `EnemySelectSceneController.cs` を更新し、敵選択画面を背景つきの敵別表示に調整
- 敵ごとのフィールド背景を `Assets/Resources/Backgrounds/EnemyField*.png` として追加
- 敵ごとの立ち絵を `Assets/Resources/Characters/Enemies/*.png` として追加
- `EnemySelectBaseBackground.png` を追加
- 左右ボタンとスワイプで敵を切り替える UI を追加
- 選択中の敵名、説明、枚数表示を表示
- 最後に選択した敵を `PlayerPrefs` に保存する流れを維持

直近コミット:

- `9df2016 Adjust enemy select swipe UI`
- `91fbac0 Add enemy select base background`
- `daaecc4 Add beginner slime enemy select art`
- `45f59dd Add logic knight enemy select art`
- `2907316 Add realist merchant enemy select art`
- `30f9312 Add harsh reviewer enemy select art`
- `39f01da Add ethics guardian enemy select art`
- `ac11610 Add investment lord enemy select art`

未確認:

- Unity Editor 上で `EnemySelectScene` の実表示確認
- 実機でのスワイプ操作確認
- 各敵背景・立ち絵が画面内に自然に収まるか
- `相手を選ぶ` から `EnemySelectScene` へ入り、`闘技場へ` で `BattleScene` に進めるか

次に優先して確認・調整する内容:

- `Assets/Scripts/EnemySelectSceneController.cs` の描画順、文字色、背景暗幕、情報プレートを確認する
- 敵名・説明・現在位置表示が背景や立ち絵に埋もれていないか確認する
- 左右ボタン、`戻る`、`闘技場へ` が背景上で視認しやすいか確認する
- 敵立ち絵と文字 UI が重なって読みにくくなっていないか確認する
- 必要なら半透明パネル、文字の縁取り、暗幕、配置調整で読みやすさを優先する
- ゲーム機能やバトルロジックには触らず、敵選択画面の視認性改善に範囲を絞る

`phase4_home_scene_adjustments` は実装分をコミット・push 済み。

- 最新コミット: `787447f Adjust home scene layout`
- Draft PR: `https://github.com/yukihomma-dailystudio/Logic-Quest/pull/2`
- 確認済み: `dotnet build thinquest.sln --no-restore`
- 確認済み: `git diff --check`

## 優先して確認すること

1. Unity Editor で `HomeScene` を開き、現在のホーム背景が表示されるか確認する。

   確認対象:

   - `Assets/Resources/Backgrounds/GuildHallHomeBackground.png`
   - 上部ステータスフレーム
   - 下部ナビゲーションフレーム
   - 右下のクラリス半身絵
   - 左下 3 ボタンのサイズと位置が背景フレーム内で自然に揃っているか

2. Play Mode でホーム画面の UI 表示を確認する。

   確認対象:

   - 上部ステータス文字がフレーム内に収まるか
   - `クエスト開始` が押せるか
   - `本日の討伐依頼` が押せるか
   - `門へ戻る` が見切れず押せるか
   - 左下 3 ボタンのクリック領域が小さすぎないか
   - クラリスの半身絵が右下に自然に収まるか
   - `ClarisseThinking` / `ClarisseNormal` のランダム表示で背景の四角が出ないか

3. Play Mode で以下の導線を確認する。

   `TitleScene -> HomeScene -> ThemeInputScene -> EnemySelectScene -> BattleScene -> ResultScene`

4. `GameScene` から `BattleScene` へのリネーム後、Unity のシーン参照や Build Settings が壊れていないか確認する。

5. Draft PR #2 の差分を確認し、Unity Editor 上の見た目に問題がなければ ready for review にする。

## 次の実装候補

- Unity Editor 上でホーム画面の上部ステータス、下部ナビ、クラリス半身絵の位置とサイズを必要に応じて微調整する。
- `本日の討伐依頼` の遷移先や専用機能を実装する。
- クラリスの表情差分を増やし、状態や日替わりで表示を切り替える。
- `BattleScene` の文言を、より闘技場や試練らしい表現に寄せる。
- 結果画面にレベルアップ演出や能力値 EXP の見せ方を追加する。
- 他シーンにも背景画像を段階的に追加する。
- バトル評価をもう少し敵ごとに個性が出るようにする。
