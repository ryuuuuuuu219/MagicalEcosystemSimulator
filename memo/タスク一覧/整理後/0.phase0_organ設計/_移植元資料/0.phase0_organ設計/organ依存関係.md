# organ 依存関係メモ

更新日: 2026-05-08

organ を AddComponent で導入するときに、自動導入または前提確認が必要な関係をまとめる。

## Core

| organ | 必須/推奨 | 理由 |
| --- | --- | --- |
| AnimalBrain | AIContext, GroundMotor | desire / action の評価主体。移動結果は GroundMotor に渡す。 |
| GroundMotor | CreatureMotorBootstrap | 既存 Rigidbody / Collider / Transform を使った実移動。 |
| AIMemoryStore | FoodMemory, PreyMemory, ThreatMemory | 長期方針では AIMemoryStore に統合する。短期は個別 Memory を typed view として使う。 |

## Sense と Memory

| organ | 自動導入候補 | 未移植の前提 |
| --- | --- | --- |
| FoodVisionSense | FoodMemory | visionWaves, IsGrassSafe, TerrainCollider Raycast, memorytime |
| PredatorVisionSense | ThreatMemory | 既存 AI と同等の遮蔽、視界、記憶失効 |
| PreyVisionSense | PreyMemory | 既存 AI と同等の遮蔽、視界、記憶失効 |
| ThreatVisionSense | ThreatMemory | threat field / attack pulse との統合 |

## Desire

| organ | 前提 | 備考 |
| --- | --- | --- |
| FoodDesire | FoodMemory | 記憶なしなら移動意図なし。 |
| PreyChaseDesire | PreyMemory | 捕食対象の記憶を追う。 |
| ThreatAvoidanceDesire | ThreatMemory | 脅威記憶から離れる。 |
| RestDesire | BodyResource | 未実装。 |
| ReproductionDesire | genome / lifecycle | 未実装。 |

## Action

| organ | 前提 | 自動導入候補 |
| --- | --- | --- |
| BiteAttackAction | PreyMemory, PredatorCombatLibrary | ThreatPulseEmitter, GroundMotor |
| MeleeAttackAction | PreyMemory, PredatorCombatLibrary | ThreatPulseEmitter |
| ChargeAttackAction | PreyMemory, PredatorCombatLibrary | ThreatPulseEmitter, GroundMotor |
| CounterAttackAction | ThreatMemory | 未実装。反撃条件の定義が必要。 |
| CorpseEatAction | FoodMemory | 死体判定の正本を AnimalBrain 側へ移す予定。 |
| PredatorPhaseEvolutionAction | ManaFieldManager | 肉食相以上に限定。 |

## Magic

| organ | 前提 | 自動導入候補 |
| --- | --- | --- |
| MagicAttackAction | MagicProjectileAttackAction | MagicCooldownState |
| MagicProjectileAttackAction | MagicLaunchApi, MagicCooldownState | PreyMemory |
| MagicEvasionAction | MagicCooldownState | 未実装。 |
| MagicDefenseAction | MagicCooldownState | 未実装。 |

## インストーラーで必要な処理

1. species / phase 別の default organ set を追加する。
2. `コンポーネント導入確率表.md` の確率で抽選する。
3. 抽選結果を `AIComponentGene.enabled` / `AIComponentGene.level` に保存する。
4. 必須依存 organ を自動追加する。
5. 推奨依存 organ は default set または確率表で補う。
6. 旧 behaviour と organ runner が同時に移動を実行しないよう、実行主体を一つにする。

## 2026-05-08 時点の実装方針

固定構成は `OrganPresetLibrary` に集約する。
`herbivoreBehaviour` / `predatorBehaviour` は削除せず、生成時と相進化時に `EnsureOrgansForCurrentPhase()` を呼ぶ。
`OrganPresetLibrary` は `AnimalAIInstaller.Ensure<T>()` を使って、species / phase に必要な organ を AddComponent する。
