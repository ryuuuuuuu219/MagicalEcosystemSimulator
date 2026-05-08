# 13.課題ステージ（環境外乱）

## 仕様目的

課題ステージへ期間イベントとして環境外乱を適用し、死亡率、復帰率、phase population などの観測指標を記録する。

## スコープ

- 外乱イベント定義。
- 外乱の開始・継続・終了。
- 外乱パラメータの適用。
- 観測指標の記録。
- 課題ステージ controller との接続。

## 現状

- Ingame には Disturbance UI の入口がある。
- `WorldUIManager` から disturbance branch を開閉できる。
- phase population 表示はある。
- 外乱を期間イベントとして実際に適用する controller はまだ薄い。

## 仕様

- 外乱は stage の sub event として扱う。
- 外乱は開始時刻、継続時間、対象 field / resource / population、強度を持つ。
- 外乱中は死亡率、復帰率、phase population、mana field、heat / wind などを観測できる。
- 外乱結果は `12.課題ステージ` の勝敗判定や generation log に渡せる。

## 実装タスク

- DisturbanceEvent のデータ構造を定義する。
- stage controller から外乱を開始・終了できるようにする。
- field manager / resource / population へ外乱パラメータを適用する。
- 外乱前後の観測指標を log に残す。

## 対象スクリプト

- `Assets/Scenes/Ingame.unity`
- `Assets/script/Ingame/UI/Menu/ingame/UImanager/WorldUIManager.ButtonBindings.cs`
- `Assets/script/Ingame/UI/Menu/ingame/UImanager/WorldUIManager.MenuFlow.cs`
- `Assets/script/Ingame/UI/Menu/ingame/UImanager/WorldUIManager.PhasePopulation.cs`
- `Assets/script/Ingame/Genome/GenerationLog.cs`
- `Assets/script/Ingame/Diagnostics/PerformanceBudgetMonitor.cs`

## 完了条件

- 外乱が一定期間だけ適用され、終了後に元の stage 進行へ戻る。
- 外乱による population / field / death 指標の変化が記録される。
- 課題ステージの勝敗判定が外乱結果を参照できる。

## 移植元

- `12.課題ステージ（環境外乱）`
