# organ コンポーネント導入確率表

## 表の見方

- 現在: 今の `AnimalAIInstaller` が導入する確率。
- 草食案: 草食個体生成時の初期案。
- 捕食案: 捕食個体生成時の初期案。
- 備考: 固定、自動導入、低確率変異など。

## Core

| コンポーネント | 現在 | 草食案 | 捕食案 | 備考 |
| --- | ---: | ---: | ---: | --- |
| `AnimalBrain` | 100% | 100% | 100% | 固定導入。 |
| `AIContext` | 内部生成 | 内部生成 | 内部生成 | AddComponent 対象ではない。 |
| `AIMoveIntent` | 構造体 | 構造体 | 構造体 | AddComponent 対象ではない。 |
| `AIComponentGene` | 構造体 | 構造体 | 構造体 | AddComponent 対象ではない。 |
| `AIComponentSet` | データ | データ | データ | Installer が参照する設定。 |
| `AnimalAIInstaller` | 手動付与 | 手動付与 | 手動付与 | 個体 prefab または manager が付ける。 |

## Memory

| コンポーネント | 現在 | 草食案 | 捕食案 | 備考 |
| --- | ---: | ---: | ---: | --- |
| `AIMemoryStore` | 100% | 100% | 100% | 固定導入。 |
| `FoodMemory` | 0% | 100% | 5% | 捕食側は変異候補。 |
| `ThreatMemory` | 0% | 95% | 90% | 生存判断に近いので高確率。 |
| `PreyMemory` | 0% | 0% | 100% | 捕食 default。 |
| `TargetTracker` | 0% | 0% | 90% | 比例航法や追跡維持に必要。 |

## Sense

| コンポーネント | 現在 | 草食案 | 捕食案 | 備考 |
| --- | ---: | ---: | ---: | --- |
| `FoodVisionSense` | 0% | 100% | 5% | 草食 default。 |
| `PredatorVisionSense` | 0% | 95% | 5% | 草食の生存用。 |
| `PreyVisionSense` | 0% | 0% | 100% | 捕食 default。 |
| `ThreatVisionSense` | 0% | 50% | 85% | 上位捕食者や敵対判定用。 |
| `ThreatMapSense` | 0% | 40% | 60% | field 本接続後に上げる。 |
| `ManaFieldSense` | 0% | 70% | 70% | mana field 誘引や吸収の観測用。 |
| `SafeFoodPathEvaluator` | 0% | 80% | 0% | 草食の餌経路安全判定。 |
| `CreatureRelationResolver` | 0% | 100% | 100% | IFF、speciesID、捕食対象判定用。 |

## Desire

| コンポーネント | 現在 | 草食案 | 捕食案 | 備考 |
| --- | ---: | ---: | ---: | --- |
| `FoodDesire` | 0% | 100% | 5% | 草食 default。 |
| `PreyChaseDesire` | 0% | 0% | 100% | 捕食 default。 |
| `ThreatAvoidanceDesire` | 0% | 95% | 70% | 草食は高確率、捕食も上位捕食者対策。 |
| `BoundaryAvoidanceDesire` | 0% | 100% | 100% | 固定寄り。 |
| `WanderDesire` | 0% | 90% | 85% | 目標喪失時の保険。 |
| `ManaFieldAttractionDesire` | 0% | 45% | 50% | mana field 経済の調整対象。 |
| `DamageAvoidanceDesire` | 0% | 40% | 35% | 被ダメージ反応。 |
| `RestDesire` | 0% | 0% | 0% | 未実装候補。 |
| `ReproductionDesire` | 0% | 0% | 0% | 未実装候補。 |

## Steering

| コンポーネント | 現在 | 草食案 | 捕食案 | 備考 |
| --- | ---: | ---: | ---: | --- |
| `ProportionalNavigationSteering` | 0% | 0% | 75% | 捕食の追尾補正。 |
| `TerrainVectorProjector` | 0% | 100% | 100% | 地形面への投影。 |

## Action

| コンポーネント | 現在 | 草食案 | 捕食案 | 備考 |
| --- | ---: | ---: | ---: | --- |
| `GrassEatAction` | 0% | 100% | 0% | 草食 default。 |
| `CorpseEatAction` | 0% | 0% | 100% | 捕食 default。 |
| `FieldManaAbsorbAction` | 0% | 80% | 80% | mana field 接続の主軸。 |
| `ChargeAttackAction` | 0% | 0% | 45% | 捕食攻撃の一種。 |
| `BiteAttackAction` | 0% | 0% | 80% | 捕食の基本攻撃。 |
| `MeleeAttackAction` | 0% | 0% | 65% | 捕食の基本攻撃。 |
| `CounterAttackAction` | 0% | 5% | 15% | 将来の反撃変異。 |
| `RandomEvasionAction` | 0% | 20% | 10% | 乱数回避。 |
| `ThreatPulseEmitter` | 0% | 自動 | 自動 | 攻撃 organ 導入時に自動導入。 |
| `PredatorPhaseEvolutionAction` | 0% | 0% | 60% | phase evolution 用。 |
| `CreatureDeathState` | 0% | 100% | 100% | 固定導入候補。 |

## Magic Action / Evasion

| コンポーネント | 現在 | 草食案 | 捕食案 | 備考 |
| --- | ---: | ---: | ---: | --- |
| `MagicAttackAction` | 0% | 3% | 8% | 魔法適性変異。 |
| `MagicProjectileAttackAction` | 0% | 2% | 6% | 遠距離魔法。 |
| `MagicImpactAction` | 0% | 自動 | 自動 | 魔法攻撃導入時に必要。 |
| `MagicEvasionAction` | 0% | 5% | 5% | 魔法回避変異。 |
| `MagicDefenseAction` | 0% | 3% | 5% | 魔法防御変異。 |
| `MagicCooldownState` | 0% | 自動 | 自動 | 魔法 organ 導入時に自動導入。 |

## Motor

| コンポーネント | 現在 | 草食案 | 捕食案 | 備考 |
| --- | ---: | ---: | ---: | --- |
| `GroundMotor` | 100% | 100% | 100% | 固定導入。 |
| `CreatureMotorBootstrap` | 100% | 100% | 100% | 固定導入。 |
| `MovementTelemetry` | 構造体 | 構造体 | 構造体 | `GroundMotor` が保持する。 |
