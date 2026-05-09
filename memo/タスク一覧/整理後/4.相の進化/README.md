# 4.相の進化

## 目的

捕食者が `predator` から `highpredator`、`dominant` へ進む phase up 条件と実行責務を固定する。

## 現状

- `predatorBehaviour.TryPhaseEvolution()` が mana field 由来の確率で phase up する。
- `PredatorPhaseEvolutionAction` に organ 側の phase up 実装がある。
- phase up 後は `OrganPresetLibrary.CreatePredatorPreset(category, phase)` 由来の `AIComponentSet` を `OrganFoundation` へ導入する。
- `OrganFoundation` は phase 変化時に checkpoint を保存する。
- UI は highpredator / dominant を表示対象に含めている。

## スコープ

- phase up 条件。
- phase 変化時の category / speciesID / organ set / checkpoint。
- 旧 `predatorBehaviour.TryPhaseEvolution()` と `PredatorPhaseEvolutionAction` の責務整理。
- 支配種到達条件の前提になる phase 表現。

## 仕様

- phase up は `predator` -> `highpredator` -> `dominant` の単方向を基本とする。
- phase up 条件は mana field、個体 mana、評価値、世代条件のどれを使うか明記する。
- phase up 時は category、speciesID、organ set、UI 表示対象を同期する。
- phase up 時の organ set は preset と既存 gene 差分を合成し、必要な依存 organ を再解決する。
- phase up snapshot は、世代更新時の organ checkpoint 評価候補に含める。

## 残り

- phase up 条件を README とコードで一致させる。
- `TryPhaseEvolution()` と `PredatorPhaseEvolutionAction` の重複を整理する。
- speciesID の採番、継承、表示ルールを決める。
- phase up 後の魔法 organ / 戦闘 organ 追加を実機で検証する。

## 対象スクリプト

- `Assets/script/Ingame/Creatures/before/Predator/predatorBehaviour.cs`
- `Assets/script/Ingame/Creatures/after/organ/Action/PredatorPhaseEvolutionAction.cs`
- `Assets/script/Ingame/Creatures/after/organ/Core/OrganFoundation.cs`
- `Assets/script/Ingame/Creatures/after/organ/Core/OrganPresetLibrary.cs`
- `Assets/script/Ingame/Creatures/after/organ/Core/AIComponentSet.cs`
- `Assets/script/Shared/Enums/SimulationEnums.cs`
- `Assets/script/Ingame/UI/Menu/ingame/UImanager/WorldUIManager.PhasePopulation.cs`

## 完了条件

- phase up が再現可能な条件で発生する。
- phase 変化後の category / speciesID / organ / UI 表示にずれがない。
- phase up checkpoint が世代更新時の評価候補として追跡できる。
- 支配種到達条件タスクが、この phase 仕様を前提として参照できる。
