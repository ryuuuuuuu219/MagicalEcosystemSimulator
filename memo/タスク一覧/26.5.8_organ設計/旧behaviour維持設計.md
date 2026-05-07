# organ 化後の旧 behaviour 維持設計

更新日: 2026-05-08

## 目的

`herbivoreBehaviour` / `predatorBehaviour` を削除せず、AI 本体ではなく「既存互換 API + 相進化時の organ Ensure 構成口」として維持する。

organ 化後は、個体の機能は AddComponent された organ が持つ。
旧 behaviour は、manager / UI / field / 魔法 / 世代交代など既存コードが参照する窓口を保ちつつ、相や種に応じた organ 構成を配布する。

## 方針

旧 behaviour は維持する。
ただし責務を変える。

旧:

- AI の本体
- 移動、視認、攻撃、摂食、死亡、相進化を直接実行

新:

- 既存コードとの互換窓口
- manager / UI / resource / genome 参照の保持
- 初期生成時と相進化時に `OrganPresetLibrary` を呼ぶ構成口
- 旧実装から organ 実装へ段階的に委譲する移行面

## 実装方針

`AnimalAIInstaller.Ensure<T>()` を土台にする。
相・種ごとの固定構成は `OrganPresetLibrary` に集約する。

役割:

| 要素 | 役割 |
| --- | --- |
| `AnimalAIInstaller` | AddComponent の実行器。`Ensure<T>()` を持つ。 |
| `OrganPresetLibrary` | species / phase ごとの organ 構成表。 |
| `herbivoreBehaviour` | herbivore 初期構成の Ensure 呼び出し口。 |
| `predatorBehaviour` | predator / highpredator / dominant 構成の Ensure 呼び出し口。 |
| `PredatorPhaseEvolutionAction` | organ 側で相進化した時に再 Ensure する。 |

## Ensure のタイミング

1. 個体生成直後
   - `herbivoreBehaviour.Start()`
   - `predatorBehaviour.Start()`

2. 相進化直後
   - `predatorBehaviour` 内の旧相進化処理
   - `PredatorPhaseEvolutionAction`

3. 将来の genome / installer 抽選後
   - `AnimalAIInstaller` が確率抽選した結果に固定構成と依存構成を合成する。

## 現在の Ensure 固定構成

### 共通

- `AnimalBrain`
- `AIMemoryStore`
- `GroundMotor`
- `CreatureMotorBootstrap`
- `CreatureRelationResolver`

### 草食

- `FoodMemory`
- `ThreatMemory`
- `FoodVisionSense`
- `PredatorVisionSense`
- `ThreatVisionSense`
- `FoodDesire`
- `ThreatAvoidanceDesire`
- `WanderDesire`
- `BoundaryAvoidanceDesire`
- `GrassEatAction`
- `CorpseEatAction`
- `FieldManaAbsorbAction`
- `ManaFieldSense`

### 捕食者

- `PreyMemory`
- `ThreatMemory`
- `PreyVisionSense`
- `ThreatVisionSense`
- `PreyChaseDesire`
- `ThreatAvoidanceDesire`
- `WanderDesire`
- `BoundaryAvoidanceDesire`
- `BiteAttackAction`
- `MeleeAttackAction`
- `ThreatPulseEmitter`
- `FieldManaAbsorbAction`
- `ManaFieldSense`
- `PredatorPhaseEvolutionAction`

### 上位捕食者以上

- `ChargeAttackAction`
- `MagicAttackAction`
- `MagicProjectileAttackAction`
- `MagicCooldownState`

## 切り替え時の禁止事項

- 旧 behaviour と `AnimalBrain` が同時に移動を実行しない。
- 旧 behaviour と organ が同じ攻撃を二重に判定しない。
- 死亡状態の正本を複数にしない。
- genome と organ gene の値を無同期で別々に更新しない。

## 段階移行

### Phase 1: Ensure 配布

旧 behaviour は残したまま、生成時と相進化時に必要 organ を配る。
この段階では旧 behaviour の AI 実行も残る。

### Phase 2: organ 実行へ委譲

旧 behaviour に「旧 AI を実行するか」「organ AI を実行するか」の排他フラグを入れる。
`AnimalBrain.TickBrain()` を実行主体へ移す。

### Phase 3: behaviour は構成ライブラリの入口になる

旧 behaviour は以下を維持する。

- manager 参照
- UI 互換 API
- genome / resource の互換窓口
- `EnsureOrgansForCurrentPhase()`

AI の判断・実行は organ が担う。

## 未決事項

- `AnimalBrain` の Tick を `Update()` に置くか `FixedUpdate()` に置くか。
- HP / 死亡を旧 behaviour から `CreatureLifecycleState` へ移す時期。
- `AIComponentGene` の確率構成と固定 Ensure 構成をどう合成するか。
- dominant 専用 organ をどこまで固定配布するか。
- 草食の organ 進化評価をどのスコアで決めるか。
