# 11.課題ステージ（期間イベント/環境外乱つき）

## 目的

- 期間イベントと環境外乱を組み合わせた課題ステージを管理する。

## 作業区分

### 設計資料の校正

1. 外乱イベント仕様の確定
- 外乱セット、適用条件、観測指標を確定する。
- 対象ファイル:
  - `memo/設定/設定10：環境外乱.txt`
  - `memo/設定/設定9：ゲーム設計.txt`

### 本実装

1. 期間イベント適用
- 期間ごとの外乱適用ロジックを実装する。
- 対象ファイル:
  - `Assets/Scenes/Ingame.unity`
  - `Assets/script/Ingame/Environment/ResourceDispenser.cs`
  - `Assets/script/Ingame/Creatures/Herbivore/herbivoreBehaviour.cs`
  - `Assets/script/Ingame/Creatures/Predator/predatorBehaviour.cs`

2. 外乱観測と検証
- 死亡率・復帰率等の観測を記録・表示する。
- 対象ファイル:
  - `Assets/script/Ingame/Genome/GenerationLog.cs`
  - `Assets/script/Ingame/Diagnostics/PerformanceBudgetMonitor.cs`
  - `Assets/script/Ingame/UI/Menu/ingame/UImanager/WorldUIManager.StateView.cs`

## 完了条件

- 期間イベントごとの差分パラメータが適用される。
- 外乱発生時の観測指標を再現可能に取得できる。

## 参照

- `../../設定/設定10：環境外乱.txt`
