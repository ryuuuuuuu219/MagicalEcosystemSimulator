# 5.支配種仕様・役割定義

状態: 完了

## 目的

`dominant` を支配種として扱うための定義を、この README に一本化する。

ここでは、捕食者から支配種までの相、通常属性魔法の獲得経路、支配種化条件、空間魔法、魔法 organ 保護、世代スポーン上限を固定する。

## 基本定義

| 相 | 定義 | 魔法条件 |
| --- | --- | --- |
| 捕食者 | デフォルトの戦闘種。草食動物とは隔絶した系統として扱う。 | 通常属性魔法なしでも成立する。 |
| 上位捕食者 | 捕食者から相進化した個体。 | 通常属性魔法を一種以上持つ。進化時に未所持なら一種だけ自動付与する。 |
| 支配種 | 上位捕食者が複数属性魔法を会得して到達する最終相。 | 通常属性魔法二種以上 + 支配種専用の空間魔法を持つ。 |

空間魔法は通常属性数に含めない。支配種の判定は「通常属性魔法二種以上」と「空間魔法の付与」を分ける。

## 到達経路

支配種への到達経路は、魔法獲得、相進化、世代スポーンの三層に分けて扱う。

### 通常属性魔法の獲得経路

- 実装済み: organ 突然変異による魔法獲得。
- 未実装: 被弾覚醒による魔法獲得。
- 仕様追加: 上位捕食者へ相進化した瞬間、通常属性魔法を持っていなければ一種のみ自動付与する。

ここでいう突然変異は、mana field などで補正される organ 着脱処理を指す。

### 相進化経路

- `predator -> highpredator` は、突然変異または mana field 補正つき相進化によって発生する。
- `predator -> highpredator` 時点で通常属性魔法がなければ、一種だけ自動付与する。
- `highpredator -> dominant` は、相そのものの突然変異では発生しない。
- `highpredator -> dominant` は、通常属性魔法を二種以上会得したことを条件に発生する。
- `dominant` 化した瞬間、支配種専用の空間魔法を自動導入する。

整理すると、突然変異で相が変わるのは `predator -> highpredator` まで。`highpredator -> dominant` は魔法会得数で決定する。

### 相進化 organ の責務

- `PredatorPhaseEvolutionAction` は `predator -> highpredator` 専用の action とする。
- `PredatorPhaseEvolutionAction` は highpredator になった瞬間に痕跡器官化・削除・無効化しない。
- `PredatorPhaseEvolutionAction` は `Resource.resourceCategory == category.predator` の場合だけ phase up を試みる。
- `PredatorPhaseEvolutionAction` から `highpredator -> dominant` の `currentRank + 1` 処理は外す。
- `highpredator -> dominant` は別処理へ分離する。候補名は `DominantAscensionAction` または `DominantEvolutionAction`。
- dominant 化専用処理は、通常属性魔法二種以上の判定、`category.dominant` への更新、空間魔法の抽選付与、到達ログ、自然到達数の記録を担当する。

### 世代スポーン

- `dominantCountPerGeneration` は、新規に支配種を作る数ではない。
- `dominantCountPerGeneration` は、前世代までに自然到達した dominant 個体数を上限とする再配置枠として扱う。
- 世代更新で dominant をスポーンする場合も、自然到達した系統・個体数の継承として扱い、任意生成の抜け道にしない。

## 支配種化条件

支配種として扱うための最小条件は以下。

1. 個体の `Resource.resourceCategory` が `category.highpredator` である。
2. 個体が通常属性魔法を二種以上会得している。
3. 支配種化処理により `Resource.resourceCategory` が `category.dominant` へ更新される。
4. dominant 化時に空間魔法が一種自動付与される。
5. dominant 化後も捕食系主要 organ、魔法攻撃 organ、mana 系 organ が active である。

通常属性魔法二種以上の判定は `MagicElementAffinityState` を正本とする。`MagicProjectileAttackAction` は固定属性ではなく、`MagicElementAffinityState` の会得済み通常属性から発射属性を解決する。

## 空間魔法

空間魔法は支配種専用とする。

- 通常の mutation では新規獲得しない。
- 世代遺伝では新規獲得しない。
- 被弾覚醒では新規獲得しない。
- optional preset には入れない。
- `dominant` 化した瞬間の自動導入でのみ獲得できる。

導入時に以下のどちらかを抽選する。

| 種別 | 仕様 |
| --- | --- |
| 空間歪曲 | 一点への座標攻撃。高ダメージ。命中地点または周辺にマナを生成する。 |
| 空間転移 | 二点への座標攻撃。オブジェクト座標操作を伴う。 |

## 魔法 organ 保護

- 魔法 organ は痕跡器官化しない。
- `MagicAttackAction`、`MagicProjectileAttackAction`、`MagicCooldownState`、属性選択・属性保持 organ、空間魔法 organ は mutation による欠落対象から除外する。
- `isVitalOrgan` とは別に、進化段階や魔法獲得を守る `isProtectedOrgan` を使う。
- `isVitalOrgan` は AI 基盤や生命維持系、protected organ は上位捕食者・支配種の定義を守る用途として分ける。

## 実装状態

- `predatorBehaviour.TryPhaseEvolution()` と `PredatorPhaseEvolutionAction` は、どちらも `predator -> highpredator` までに制限済み。二系統の正本統合は将来整理だが、支配種到達条件の抜け道ではない。
- `highpredator -> dominant` は `DominantAscensionAction` が担当する。条件は `category.highpredator` かつ通常属性魔法二種以上。
- `DominantAscensionAction` は dominant 化、空間魔法 organ の導入、通常属性二種の保証、到達ログ、自然到達数の記録を担当する。
- `MagicElementAffinityState` は通常属性の会得状態を保持し、上位捕食者への相進化時に最低一種、支配種化時に最低二種を保証する。
- `SpaceMagicAction` は支配種専用 organ として導入され、初回導入時に空間歪曲または空間転移の mode を抽選する。
- `MagicAttackAction` から `MagicProjectileAttackAction` を直接 `AddComponent` する経路は、`AnimalAIInstaller` / `OrganFoundation` 経由に整理済み。
- `dominantCountPerGeneration` は `DominantLineageTracker` が記録する自然到達数を上限にして、世代更新時の dominant 直接スポーン数を制限する。
- 魔法 organ 保護は `AIComponentGene.isProtectedOrgan` と `AIComponentSet` 側の mutation / vestigial 除外で扱う。

## 残る確認

- 実ゲーム内で `highpredator` が通常属性二種を会得した瞬間に `dominant` 化するか確認する。
- 空間歪曲 / 空間転移の mode 差分は導入済み。座標攻撃、マナ生成、座標操作の演出・効果量は今後の調整対象とする。
- `DominantLineageTracker` は実行中の自然到達数を扱う。セーブをまたぐ永続化が必要になった場合は別タスクで扱う。

## 対象スクリプト

- `Assets/script/Shared/Enums/SimulationEnums.cs`
- `Assets/script/Ingame/Creatures/before/Predator/predatorBehaviour.cs`
- `Assets/script/Ingame/Creatures/after/organ/Action/PredatorPhaseEvolutionAction.cs`
- `Assets/script/Ingame/Creatures/after/organ/Action/DominantAscensionAction.cs`
- `Assets/script/Ingame/Creatures/after/organ/Core/AIComponentGene.cs`
- `Assets/script/Ingame/Creatures/after/organ/Core/AIComponentSet.cs`
- `Assets/script/Ingame/Creatures/after/organ/Core/DominantLineageTracker.cs`
- `Assets/script/Ingame/Creatures/after/organ/Core/OrganPresetLibrary.cs`
- `Assets/script/Ingame/Creatures/after/organ/Core/OrganRelationLibrary.cs`
- `Assets/script/Ingame/Creatures/after/organ/MagicActionEvasion/MagicElementAffinityState.cs`
- `Assets/script/Ingame/Creatures/after/organ/MagicActionEvasion/MagicAttackAction.cs`
- `Assets/script/Ingame/Creatures/after/organ/MagicActionEvasion/MagicProjectileAttackAction.cs`
- `Assets/script/Ingame/Creatures/after/organ/MagicActionEvasion/SpaceMagicAction.cs`
- `Assets/script/Ingame/UI/Menu/ingame/AdvanceGenerationController.cs`

## 完了条件

- 捕食者、上位捕食者、支配種の定義がこの README だけで読める。
- 通常属性魔法の獲得経路が、突然変異、被弾覚醒、相進化時の自動付与に整理されている。
- `predator -> highpredator` と `highpredator -> dominant` の条件差が明確である。
- 空間魔法が支配種専用で、dominant 化時の自動導入に限定されている。
- `dominantCountPerGeneration` が自然到達数を上限とする再配置枠として定義されている。
- コード上では `DominantAscensionAction`、`MagicElementAffinityState`、`SpaceMagicAction`、`DominantLineageTracker` により、上記仕様の MVP が実装済みである。
