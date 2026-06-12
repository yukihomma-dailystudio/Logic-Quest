# 次の作業内容

## 優先して確認すること

1. Unity Editor で `HomeScene` を開き、現在のホーム背景が表示されるか確認する。

   確認対象:

   - `Assets/Resources/Backgrounds/GuildHallHomeBackground.png`
   - 上部ステータスフレーム
   - 下部ナビゲーションフレーム
   - 右下のクラリス半身絵

2. Play Mode でホーム画面の UI 表示を確認する。

   確認対象:

   - 上部ステータス文字がフレーム内に収まるか
   - `クエスト開始` が押せるか
   - `本日の討伐依頼` が押せるか
   - `門へ戻る` が見切れず押せるか
   - クラリスの半身絵が右下に自然に収まるか
   - `ClarisseThinking` / `ClarisseNormal` のランダム表示で背景の四角が出ないか

3. Play Mode で以下の導線を確認する。

   `TitleScene -> HomeScene -> ThemeInputScene -> EnemySelectScene -> BattleScene -> ResultScene`

4. `GameScene` から `BattleScene` へのリネーム後、Unity のシーン参照や Build Settings が壊れていないか確認する。

5. `AGENT.md` の作業記録ルールに従い、作業まとめを更新するときは `history.md` と `log_next_feature.md` の両方を更新する。

## 次の実装候補

- Unity Editor 上でホーム画面の上部ステータス、下部ナビ、クラリス半身絵の位置とサイズを微調整する。
- `本日の討伐依頼` の遷移先や専用機能を実装する。
- クラリスの表情差分を増やし、状態や日替わりで表示を切り替える。
- `BattleScene` の文言を、より闘技場や試練らしい表現に寄せる。
- 結果画面にレベルアップ演出や能力値 EXP の見せ方を追加する。
- 他シーンにも背景画像を段階的に追加する。
- バトル評価をもう少し敵ごとに個性が出るようにする。
