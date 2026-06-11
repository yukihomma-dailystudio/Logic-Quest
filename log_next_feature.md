# 次の作業内容

## 優先して確認すること

1. Unity Editor で `HomeScene` を開き、背景画像が表示されるか確認する。

2. Play Mode で以下の導線を確認する。

   `TitleScene -> HomeScene -> ThemeInputScene -> EnemySelectScene -> BattleScene -> ResultScene`

3. `GameScene` から `BattleScene` へのリネーム後、Unity のシーン参照や Build Settings が壊れていないか確認する。

4. `AGENT.md` の作業記録ルールに従い、作業まとめを更新するときは `history.md` と `log_next_feature.md` の両方を更新する。

## 次の実装候補

- ギルドホール背景に合わせて、ホーム画面のパネル配置と読みやすさを Unity Editor 上で微調整する。
- `BattleScene` の文言を、より闘技場や試練らしい表現に寄せる。
- 結果画面にレベルアップ演出や能力値 EXP の見せ方を追加する。
- 他シーンにも背景画像を段階的に追加する。
- バトル評価をもう少し敵ごとに個性が出るようにする。
