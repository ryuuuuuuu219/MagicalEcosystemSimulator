# 9.支配種到達条件

参照元: `git show f5a7322:memo/ROADMAP.md`

## 目的

- 支配種を上位到達段階として成立させる。

## 現状（git参照時点）

- コンセプト定義は存在。
- 到達条件と実装条件は未固定。

## 作業区分

### 設計資料の校正

1. 支配種条件の定量化
- 到達条件と検証シナリオを定義する。
- 対象ファイル:
  - `memo/設定/設定8：支配種の定義.txt`
  - `memo/設定/設定8：ゲノム構造（支配種拡張）.txt`

2. 上位戦闘仕様の整理
- 空間魔法を含む上位相戦闘仕様を定義する。
- 対象ファイル:
  - `memo/設定/設定5：魔法一覧.txt`
  - `memo/設定/設定3：相の定義.txt`

### 本実装

1. 到達判定と遷移処理
- 支配種遷移判定を世代更新へ組み込む。
- 対象ファイル:
  - `Assets/script/Ingame/UI/Menu/ingame/AdvanceGenerationController.cs`
  - `Assets/script/Ingame/Creatures/Predator/predatorManager.cs`
  - `Assets/script/Shared/Enums/SimulationEnums.cs`

2. 支配種行動差分
- 上位個体の挙動差分を行動/戦闘に実装する。
- 対象ファイル:
  - `Assets/script/Ingame/Creatures/Predator/predatorBehaviour.cs`
  - `Assets/script/Ingame/Combat/PredatorCombatLibrary.cs`
  - `Assets/script/Ingame/AI/ThreatMap/threatmap_calc.cs`

## 完了条件

- 既定条件を満たした個体が支配種へ遷移する。
- 支配種行動の差分が通常個体と比較して確認できる。

## 参照

- `../0.遺伝子設計変更/現状の遺伝子構造まとめ.txt`
- `../../設定/設定8：支配種の定義.txt`
- `../../設定/設定8：ゲノム構造（支配種拡張）.txt`
