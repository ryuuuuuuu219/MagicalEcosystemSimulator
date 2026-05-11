# 4.相の進化

## 目的

捕食者が `predator` から `highpredator` へ進む phase up 条件と実行責務を固定する。

`highpredator -> dominant` は相そのものの突然変異ではなく、通常属性魔法の会得数による支配種化として `5.支配種仕様・役割定義` に置く。

## 現状

- `predatorBehaviour.TryPhaseEvolution()` が mana field 由来の確率で phase up する。
- `PredatorPhaseEvolutionAction` に organ 側の phase up 実装がある。
- どちらの phase up 実装も `predator -> highpredator` までに制限済み。
- highpredator 化時に通常属性魔法がなければ、`MagicElementAffinityState` が一種を保証する。
- phase up 後は `OrganPresetLibrary.CreatePredatorPreset(category, phase)` 由来の `AIComponentSet` を `OrganFoundation` へ導入する。
- `OrganFoundation` は phase 変化時に checkpoint を保存する。
- UI は highpredator / dominant を表示対象に含めている。

## スコープ

- phase up 条件。
- phase 変化時の category / speciesID / organ set / checkpoint。
- 旧 `predatorBehaviour.TryPhaseEvolution()` と `PredatorPhaseEvolutionAction` の責務整理。
- 支配種仕様の前提になる phase 表現。

## スコープ外

- genome の項目設計、`ValueGene`、`AIComponentGene` の保存場所は `1.支配種までの遺伝子定義` に置く。
- `highpredator -> dominant` の到達条件、支配種仕様、追加付与 organ、役割、勝利条件、維持時間、制圧判定は `5.支配種仕様・役割定義` に置く。

## 仕様

- phase up は `predator -> highpredator` の単方向を基本とする。
- `highpredator -> dominant` は phase up ではなく、通常属性魔法二種以上による ascension として扱う。
- `predator -> highpredator` の phase up 条件は、mana field 補正つきの確率処理を現行実装とする。
- phase up 時は category、speciesID、organ set、UI 表示対象を同期する。
- phase up 時の organ set は preset と既存 gene 差分を合成し、必要な依存 organ を再解決する。
- phase up 時は `GeneDataManager` から `ValueGene` と `AIComponentGene` を再適用する。
- phase up snapshot は、世代更新時の organ checkpoint 評価候補に含める。

## 実装状態

- README とコード上の支配種到達責務は一致済み。`predator -> highpredator` は相進化、`highpredator -> dominant` は支配種化処理。
- `TryPhaseEvolution()` と `PredatorPhaseEvolutionAction` の重複は残るが、どちらも `predator -> highpredator` のみに制限されているため、支配種への抜け道ではない。
- phase up 後の魔法 organ / 戦闘 organ 追加は `OrganPresetLibrary` と `OrganRelationLibrary` 経由で扱う。

## 検証待ち

- `TryPhaseEvolution()` と `PredatorPhaseEvolutionAction` の正本統合。
- speciesID の採番、継承、表示ルールの整理。
- phase up 後の magic / combat organ 追加と UI 表示を実機で確認する。
- phase up 後に `GeneDataManager` の値を再配布し、organ 内変数が古いまま残らないようにする。

## 対象スクリプト

- `Assets/script/Ingame/Creatures/before/Predator/predatorBehaviour.cs`
- `Assets/script/Ingame/Creatures/after/organ/Action/PredatorPhaseEvolutionAction.cs`
- `Assets/script/Ingame/Creatures/after/organ/Core/OrganFoundation.cs`
- `Assets/script/Ingame/Creatures/after/organ/Core/OrganPresetLibrary.cs`
- `Assets/script/Ingame/Creatures/after/organ/Core/AIComponentSet.cs`
- `Assets/script/Ingame/Creatures/after/organ/MagicActionEvasion/MagicElementAffinityState.cs`
- `Assets/script/Shared/Enums/SimulationEnums.cs`
- `Assets/script/Ingame/UI/Menu/ingame/UImanager/WorldUIManager.PhasePopulation.cs`

## 完了条件

- phase up が再現可能な条件で発生する。
- phase 変化後の category / speciesID / organ / UI 表示にずれがない。
- phase up checkpoint が世代更新時の評価候補として追跡できる。
- 支配種仕様タスクが、この phase 仕様を前提として参照できる。
