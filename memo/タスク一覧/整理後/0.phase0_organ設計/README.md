# 0.phase0_organ設計

## 仕様目的

organ 化した AI 部品を本編個体へ配布し、旧 `behaviour` と `AnimalBrain` の責務境界を固定する。

## スコープ

- `Sense` / `Desire` / `Action` / `Memory` / `Motor` の責務定義。
- 草食、捕食、上位捕食、支配種ごとの organ 配布。
- 旧 `herbivoreBehaviour` / `predatorBehaviour` を互換窓口として残すか、実行主体を `AnimalBrain` へ移すかの方針決定。

## 現状

- `OrganPresetLibrary` が草食・捕食・上位相の organ set を配布している。
- 旧 behaviour の `Start()` と phase 変化時に organ が付与される。
- `AnimalBrain.TickBrain()` は実装済みだが、主移動はまだ旧 behaviour の `ComputeTotalVector()` が中心。

## 仕様

- 旧 behaviour と `AnimalBrain` が同時に移動・攻撃を実行しない。
- organ は「検知」「欲求評価」「行動」「記憶」「移動適用」の単位で増減できる。
- `OrganPresetLibrary` は phase / species の初期構成を決める正本にする。
- 旧 behaviour は、移行完了までは manager / UI / field / 世代更新から参照される互換 API を維持する。

## 実装タスク

- `AnimalBrain.TickBrain()` を本流にするか、旧 behaviour 本流を維持するかを明文化する。
- 本流切り替え時の排他フラグを追加する。
- `GroundMotor` と旧 `ApplyMovement()` の二重移動を防ぐ。
- organ 追加時に必要依存 component を自動導入する規則を固定する。

## 対象スクリプト

- `Assets/script/Ingame/Creatures/after/organ/Core/AnimalBrain.cs`
- `Assets/script/Ingame/Creatures/after/organ/Core/AnimalAIInstaller.cs`
- `Assets/script/Ingame/Creatures/after/organ/Core/OrganPresetLibrary.cs`
- `Assets/script/Ingame/Creatures/before/Herbivore/herbivoreBehaviour.cs`
- `Assets/script/Ingame/Creatures/before/Predator/predatorBehaviour.cs`

## 完了条件

- 個体生成時と相進化時に、期待する organ set が再現可能に付与される。
- AI 実行主体が旧 behaviour と `AnimalBrain` のどちらか一方に定まり、二重移動・二重攻撃が起きない。
- 旧 behaviour の互換責務と、organ 側へ委譲した責務が README 上で追跡できる。

## 移植元

- `0.phase0_organ設計`
- `4.挙動AI分離`
- `26.5.8_挙動AI分離_現状機能と設計方針.md`
- `26.5.8_挙動AI分離_機能一覧_設計後.md`
