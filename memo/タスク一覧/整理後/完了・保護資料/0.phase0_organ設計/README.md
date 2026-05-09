# 0.phase0_organ設計

## 目的

organ 化した AI 部品を本編個体へ配布し、旧 `behaviour` と `AnimalBrain` の責務境界を固定する。

## 現状

- `OrganFoundation` が追加され、`Update()` から `AnimalBrain.TickBrain()` を呼ぶ本流になった。
- `herbivoreBehaviour` / `predatorBehaviour` は互換 API、状態保持、manager / UI / field / generation 接続の入口として残す。
- 旧 `ComputeTotalVector()` / `ApplyMovement()` と organ 側移動が二重実行されないよう、foundation runner 側のガードを入れた。
- `OrganPresetLibrary` は固定 Ensure 群から、`AIComponentSet` preset 生成と導入へ移行した。
- `OrganRelationLibrary` が component id から Type を解決し、依存 organ の補完と reverse dependency 判定を持つ。
- `AnimalBrain` は `OrganFoundation.IsOrganActive()` を通して disabled / vestigial / weight 0 の organ を中央でスキップする。
- 実機プレイで移動、追跡、逃走、攻撃が旧 behaviour と二重実行されないことを確認済み。
- プレイ時に気になった旋回処理は、速度比に応じた旋回慣性として `AnimalAICommon.ApplyMovement()` / `GroundMotor` 側で対処済み。

## スコープ

- `Sense` / `Desire` / `Action` / `Memory` / `Motor` の責務定義。
- 草食、捕食、上位捕食の初期 organ 配布。
- 旧 behaviour を互換窓口として残しながら、AI 実行主体を `OrganFoundation` + `AnimalBrain` へ寄せる。
- 依存 organ の自動導入と、不整合時の補完。

## 実装済み

- `OrganFoundation.cs` を core に追加。
- `OrganFoundation.Update()` で `AnimalBrain.TickBrain()` を実行。
- `OrganFoundation.InstallComponentSet()` から `AIComponentSet` を導入。
- `OrganPresetLibrary.CreateHerbivorePreset()` / `CreatePredatorPreset(category, phase)` を追加。
- `AnimalAIInstaller` 経由の component Ensure と、`OrganRelationLibrary` の whitelist 検索を接続。
- active / vestigial 判定を `AnimalBrain` の senses / desires / actions 実行へ反映。

## 残り

- 旧 behaviour 側に残る死亡、HP、target、memory の正本をどこまで organ 側へ移すか決める。

## 資料整理

- `_移植元資料/統合_挙動AI分離_organ設計` に、旧 `0.phase0_organ設計` と `4.挙動AI分離` の資料を統合済み。

## 対象スクリプト

- `Assets/script/Ingame/Creatures/after/organ/Core/OrganFoundation.cs`
- `Assets/script/Ingame/Creatures/after/organ/Core/AnimalBrain.cs`
- `Assets/script/Ingame/Creatures/after/organ/Core/AnimalAIInstaller.cs`
- `Assets/script/Ingame/Creatures/after/organ/Core/OrganPresetLibrary.cs`
- `Assets/script/Ingame/Creatures/after/organ/Core/OrganRelationLibrary.cs`
- `Assets/script/Ingame/Creatures/after/organ/Core/AIComponentGene.cs`
- `Assets/script/Ingame/Creatures/after/organ/Core/AIComponentSet.cs`
- `Assets/script/Ingame/Creatures/before/Herbivore/herbivoreBehaviour.cs`
- `Assets/script/Ingame/Creatures/before/Predator/predatorBehaviour.cs`

## 完了条件

- 個体生成時と相進化時に、期待する organ set が再現可能に付与される。
- AI 実行主体が `OrganFoundation` + `AnimalBrain` に定まり、二重移動・二重攻撃が起きない。
- 依存 organ が不足した状態で action / desire だけが有効化されない。
- README 上で、旧 behaviour に残す責務と organ 側へ委譲した責務が追える。

## 判定

phase0 organ 設計・基盤実装は完了扱い。以後の死亡、HP、target、memory 正本移行は次フェーズの設計対象とする。
