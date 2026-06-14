# 次の作業内容

## 現在の状態

`enemy-select-adjustments` ブランチを `main` から作成した。

次の作業対象は `EnemySelectScene` の見た目と操作感の調整。

確認・調整する内容:

- `Assets/Scripts/EnemySelectSceneController.cs` の現在の UI 構造を確認する
- 敵カードが画面サイズに対して見やすく並んでいるか確認する
- 敵の役割や個性が UI 文言から伝わるか確認する
- `戻る` と `闘技場へ` の導線が分かりやすいか確認する
- ホーム画面、タイトル画面で整えたファンタジー調 UI から大きく浮かないようにする
- ゲーム機能やバトルロジックには触らず、敵選択画面の調整に範囲を絞る

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
