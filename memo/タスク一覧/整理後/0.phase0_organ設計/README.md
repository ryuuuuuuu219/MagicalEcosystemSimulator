# 0.phase0_organ設計

## 目的

organ 定義、配布、旧 behaviour との責務分担を固定する。

## 現状

OrganPresetLibrary と AnimalBrain 系は存在し、旧 behaviour の Start() で organ が付与される。

## 残タスク

AnimalBrain.TickBrain() を本流へつなぐか、旧 behaviour との併用ルールを固定する。

## 対象スクリプト

- `Assets/script/Ingame/Creatures/after/organ/Core`
- `Assets/script/Ingame/Creatures/before/Herbivore/herbivoreBehaviour.cs`
- `Assets/script/Ingame/Creatures/before/Predator/predatorBehaviour.cs`

## 移植元

- `0.phase0_organ設計`
- `4.挙動AI分離`

## 完了条件

- 目的に書いた責務が実行時に観測できる。
- 対象スクリプトと README の参照パスが一致している。
- 旧資料から必要な内容がこのフォルダの README へ移植済み。
