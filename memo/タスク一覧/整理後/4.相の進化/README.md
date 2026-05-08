# 4.相の進化

## 目的

predator -> highpredator -> dominant の phase up 条件を整理する。

## 現状

PredatorPhaseEvolutionAction と旧 predatorBehaviour.TryPhaseEvolution() がある。

## 残タスク

phase 変化時の organ 配布、speciesID、UI表示、mana条件を統一する。

## 対象スクリプト

- `Assets/script/Ingame/Creatures/after/organ/Action/PredatorPhaseEvolutionAction.cs`
- `Assets/script/Ingame/Creatures/before/Predator/predatorBehaviour.cs`
- `Assets/script/Shared/Enums/SimulationEnums.cs`

## 移植元

- `10.支配種到達条件`
- `0.phase0_organ設計`

## 完了条件

- 目的に書いた責務が実行時に観測できる。
- 対象スクリプトと README の参照パスが一致している。
- 旧資料から必要な内容がこのフォルダの README へ移植済み。
