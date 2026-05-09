# 旧genome to organ gene対応表

## 目的

旧 `HerbivoreGenome` / `PredatorGenome` の実数値を、削除ではなく `ValueGene` と `AIComponentGene` / `AIComponentSet` へ段階的に読み替えるための対応表。

phase1 では正本の移動先を決めるだけに留め、実際の継承・交叉・serializer 接続は後続の `2.organ構成の遺伝` と `3.世代更新と遺伝` で扱う。

## 基本方針

- organ の内部数値は `ValueGene` で保持する。
- organ の有無は `AIComponentGene.enabled` で表す。
- organ の休眠・退化は `AIComponentGene.isVestigialOrgan` で表す。
- organ の大まかな性能段階は `AIComponentGene.level` を使う。
- 行動選択における優先度や寄与率は `AIComponentGene.weight` を使う。
- 生存中の可塑的変化は `mutationChanceT`、世代更新時の変異は `mutationChanceG` に分ける。
- vital organ は `isVitalOrgan = true` とし、無効化しない。
- organ が発現していない場合でも `ValueGene` は値を保持し、後から発現したときに適用できるようにする。

## 適用方針

`ValueGene` は organ から参照しやすい形にまとめる。

例:

```csharp
valueGene.PredatorVisionSense.threatDetectDistance
valueGene.BiteAttackAction.biteDamage
valueGene.RandomEvasionAction.evasionDistance
```

実体への適用は以下を基本にする。

```csharp
if (target.TryGetComponent<PredatorVisionSense>(out var organ))
{
    organ.threatDetectDistance = valueGene.PredatorVisionSense.threatDetectDistance;
}
```

## 共通読み替え

| 旧項目群 | 移行先 organ | level | weight | enabled |
| --- | --- | --- | --- | --- |
| `forwardForce`, `turnForce` | `GroundMotor` / `CreatureMotorBootstrap` | 移動性能 | 移動欲求からの反映率 | vital 扱い |
| `visionDistance`, `visionAngle`, `visionTurnAngle` | 各種 vision sense | 感覚性能 | 対象カテゴリの重要度 | 視覚器官の有無 |
| `memorytime` | `FoodMemory` / `PreyMemory` / `ThreatMemory` | 記憶保持時間 | 記憶参照の強さ | 記憶器官の有無 |
| `wanderWaves` | `WanderDesire` | 徘徊変化量 | 徘徊選択重み | 徘徊欲求の有無 |
| `visionWaves` | 各種 vision sense | 感覚揺らぎ | 感覚入力の変動量 | 視覚器官の有無 |
| `metabolismRate` | 未確定 | mana消費率候補 | なし | 旧genome残置候補 |
| `eatspeed` | 摂食 action | 摂食速度 | 摂食選択重み | 摂食行動の有無 |

## 草食

| 旧項目 | 移行先候補 | 読み替え |
| --- | --- | --- |
| `foodWeight` | `FoodDesire` | `weight` |
| `corpseWeight` | `CorpseEatAction` または corpse desire | `weight` |
| `predatorWeight` | `ThreatAvoidanceDesire` | `weight` |
| `threatWeight` | `ThreatAvoidanceDesire` | `weight` |
| `threatDetectDistance` | `ThreatVisionSense` | `level` |
| `runAwayDistance` | `ThreatAvoidanceDesire` | `level` |
| `fearThreshold` | `ThreatAvoidanceDesire` | 内部パラメータとして残す候補 |
| `escapeThreshold` | `ThreatAvoidanceDesire` | 内部パラメータとして残す候補 |
| `contactEscapeDistance` | `DamageAvoidanceDesire` | `level` |
| `evasionAngle` | `RandomEvasionAction` | 内部パラメータ |
| `evasionDuration` | `RandomEvasionAction` | 内部パラメータ |
| `evasionCooldown` | `RandomEvasionAction` | 内部パラメータ |
| `evasionDistance` | `RandomEvasionAction` | `level` |
| `predictIntercept` | `RandomEvasionAction` / 回避補助 organ | `enabled` または bool補助遺伝子 |
| `zigzagFrequency` | `RandomEvasionAction` | 内部パラメータ |
| `zigzagAmplitude` | `RandomEvasionAction` | 内部パラメータ |
| `curiosity` | `WanderDesire` / `ManaFieldAttractionDesire` | `weight` |

## 捕食

| 旧項目 | 移行先候補 | 読み替え |
| --- | --- | --- |
| `chaseWeight` | `PreyChaseDesire` | `weight` |
| `preyDetectDistance` | `PreyVisionSense` | `level` |
| `preferredChaseDistance` | `PreyChaseDesire` | 追跡・位置調整用の内部パラメータ |
| `disengageDistance` | `PreyChaseDesire` / `ThreatAvoidanceDesire` | 離脱判断の内部パラメータ |
| `stopMoveThreshold` | `PreyChaseDesire` / `GroundMotor` | 移動制御の内部パラメータ |
| `resumeMoveThreshold` | `PreyChaseDesire` / `GroundMotor` | 移動制御の内部パラメータ |
| `attackDistance` | 廃止候補 | 攻撃成立判定は arc / contact / energy / mana へ寄せる |
| `attackDamage` | `BiteAttackAction` / `MeleeAttackAction` | 既定ダメージに読み替え |
| `attackCooldown` | 各攻撃 action | `attackClock` 系へ読み替え |
| `chargeArc` | `ChargeAttackAction` | 攻撃範囲設定 |
| `chargeDamageScale` | `ChargeAttackAction` | `level` または内部パラメータ |
| `chargeManaCost` | `ChargeAttackAction` | 内部コスト |
| `chargeContactPadding` | `ChargeAttackAction` | 内部パラメータ |
| `chargeAttackClock` | `ChargeAttackAction` | 内部パラメータ |
| `biteArc` | `BiteAttackAction` | 攻撃範囲設定 |
| `biteDamage` | `BiteAttackAction` | `level` |
| `biteManaCost` | `BiteAttackAction` | 内部コスト |
| `biteAttackClock` | `BiteAttackAction` | 内部パラメータ |
| `meleeArc` | `MeleeAttackAction` | 攻撃範囲設定 |
| `meleeDamage` | `MeleeAttackAction` | `level` |
| `meleeManaCost` | `MeleeAttackAction` | 内部コスト |
| `meleeAttackClock` | `MeleeAttackAction` | 内部パラメータ |
| `attackThreatPulseScore` | `ThreatPulseEmitter` | `level` |
| `attackThreatPulseRadius` | `ThreatPulseEmitter` | `level` |
| `attackTraceScale` | 未確定 | field / 表示連携候補 |
| `attackTraceDuration` | 未確定 | field / 表示連携候補 |
| `attackTraceDepth` | 未確定 | field / 表示連携候補 |

## phase別presetの扱い

| phase | preset方針 |
| --- | --- |
| `herbivore` | `OrganPresetLibrary.CreateHerbivorePreset()` を基準にする |
| `predator` | `CreatePredatorPreset(category.predator)` を基準にする。突進・魔法は optional |
| `highpredator` | `CreatePredatorPreset(category.highpredator)` を基準にする。突進・魔法は active |
| `dominant` | 現状は highpredator 以上の強化presetとして扱える。専用 organ は `5.支配種仕様・役割定義` で判断 |

## 移行時の注意

- 既存プレイを壊さないため、旧 genome 項目は即削除しない。
- serializer は旧DNAを読める状態を維持し、読み込み時に organ gene へ補完する。
- `level` と `weight` は単純な同値コピーではなく、項目ごとに正規化範囲を決める。
- `AttackArcSettings` と `WaveGene[]` は構造体として残し、必要に応じて action / sense の内部パラメータに渡す。
