# 現行genome項目対応表

## 目的

phase1 の棚卸し結果として、現行 `HerbivoreGenome` / `PredatorGenome` / 共有構造が持つ項目を用途別に整理する。

この表は「旧 genome に残すか」「organ gene に読み替えるか」「表示・ログだけに出すか」を判断するための入力資料とする。

## 共有構造

| 構造 | 項目 | 用途 | 備考 |
| --- | --- | --- | --- |
| `WaveGene` | `frequency` | 波形の周期 | 視界揺らぎ、徘徊、将来の行動判断波形に使用 |
| `WaveGene` | `amplitude` | 波形の振幅 | 影響量。値の意味は利用先ごとに変わる |
| `WaveGene` | `phase` | 波形の位相 | 個体差を作るためのずれ |
| `AttackArcSettings` | `radius` | 攻撃判定半径 | 捕食の複合攻撃で使用 |
| `AttackArcSettings` | `arcDegrees` | 攻撃弧角度 | 捕食の複合攻撃で使用 |
| `AttackArcSettings` | `length` | 攻撃判定長 | 捕食の複合攻撃で使用 |
| `AttackArcSettings` | `startOffset` | 攻撃開始位置 | 捕食の複合攻撃で使用 |
| `AttackArcSettings` | `localDirection` | 攻撃方向 | `Vector3` のため交叉・変異時に専用処理が必要 |

## HerbivoreGenome

| 分類 | 項目 | 現在の意味 | phase1での扱い候補 |
| --- | --- | --- | --- |
| 移動 | `forwardForce` | 前進力 | `GroundMotor` 系 organ の level/weight へ移行候補 |
| 移動 | `turnForce` | 旋回力 | `GroundMotor` 系 organ の level/weight へ移行候補 |
| 感覚 | `visionAngle` | 視野角 | `FoodVisionSense` / `PredatorVisionSense` / `ThreatVisionSense` へ移行候補 |
| 感覚 | `visionturnAngle` | 視界旋回角 | 命名を `visionTurnAngle` に正規化する候補 |
| 感覚 | `visionDistance` | 視界距離 | vision sense 系 organ へ移行候補 |
| 感覚 | `visionWaves` | 視覚揺らぎ | `WaveGene` として継続。表示・DNA対象 |
| 代謝・摂食 | `metabolismRate` | 代謝率 | 旧 genome に残すか、mana消費系 organ に移行 |
| 代謝・摂食 | `eatspeed` | 摂食速度 | `GrassEatAction` / `CorpseEatAction` へ移行候補 |
| 脅威認識 | `threatWeight` | 脅威評価重み | `ThreatAvoidanceDesire` へ移行候補 |
| 脅威認識 | `threatDetectDistance` | 脅威検知距離 | `ThreatVisionSense` へ移行候補 |
| 記憶 | `memorytime` | 記憶保持時間 | `FoodMemory` / `ThreatMemory` へ移行候補 |
| 逃走・回避 | `runAwayDistance` | 逃走開始距離 | `ThreatAvoidanceDesire` へ移行候補 |
| 逃走・回避 | `contactEscapeDistance` | 接触時逃走距離 | `DamageAvoidanceDesire` / `RandomEvasionAction` へ移行候補 |
| 逃走・回避 | `evasionAngle` | 回避角度 | `RandomEvasionAction` へ移行候補 |
| 逃走・回避 | `evasionDuration` | 回避時間 | `RandomEvasionAction` へ移行候補 |
| 逃走・回避 | `evasionCooldown` | 回避間隔 | `RandomEvasionAction` へ移行候補 |
| 逃走・回避 | `evasionDistance` | 回避距離 | `RandomEvasionAction` へ移行候補 |
| 逃走・回避 | `predictIntercept` | 先読み回避 | bool 遺伝子として扱うか organ enabled に寄せる |
| 逃走・回避 | `zigzagFrequency` | ジグザグ周期 | `RandomEvasionAction` の内部パラメータ候補 |
| 逃走・回避 | `zigzagAmplitude` | ジグザグ振幅 | `RandomEvasionAction` の内部パラメータ候補 |
| 行動重み | `foodWeight` | 食物志向 | `FoodDesire` へ移行候補 |
| 行動重み | `predatorWeight` | 捕食者警戒 | `ThreatAvoidanceDesire` へ移行候補 |
| 行動重み | `corpseWeight` | 死骸志向 | `CorpseEatAction` / desire 系へ移行候補 |
| 行動重み | `fearThreshold` | 恐怖閾値 | `ThreatAvoidanceDesire` へ移行候補 |
| 行動重み | `escapeThreshold` | 逃走閾値 | `ThreatAvoidanceDesire` へ移行候補 |
| 行動重み | `curiosity` | 探索性 | `WanderDesire` / `ManaFieldAttractionDesire` へ移行候補 |
| 徘徊 | `wanderWaves` | 徘徊波形 | `WaveGene` として継続。表示・DNA対象 |

## PredatorGenome

| 分類 | 項目 | 現在の意味 | phase1での扱い候補 |
| --- | --- | --- | --- |
| 移動 | `forwardForce` | 前進力 | `GroundMotor` 系 organ の level/weight へ移行候補 |
| 移動 | `turnForce` | 旋回力 | `GroundMotor` 系 organ の level/weight へ移行候補 |
| 感覚 | `visionAngle` | 視野角 | `PreyVisionSense` / `ThreatVisionSense` へ移行候補 |
| 感覚 | `visionTurnAngle` | 視界旋回角 | 草食側の `visionturnAngle` と命名統一候補 |
| 感覚 | `visionDistance` | 視界距離 | vision sense 系 organ へ移行候補 |
| 感覚 | `visionWaves` | 視覚揺らぎ | `WaveGene` として継続。表示・DNA対象 |
| 代謝・摂食 | `metabolismRate` | 代謝率 | 旧 genome に残すか、mana消費系 organ に移行 |
| 代謝・摂食 | `eatspeed` | 摂食速度 | 捕食・死骸摂食側の action へ移行候補 |
| 追跡・認識 | `chaseWeight` | 追跡重み | `PreyChaseDesire` へ移行候補 |
| 追跡・認識 | `preyDetectDistance` | 獲物検知距離 | `PreyVisionSense` へ移行候補 |
| 脅威認識 | `threatWeight` | 脅威評価重み | `ThreatAvoidanceDesire` へ移行候補 |
| 脅威認識 | `threatDetectDistance` | 脅威検知距離 | `ThreatVisionSense` へ移行候補 |
| 記憶 | `memorytime` | 記憶保持時間 | `PreyMemory` / `ThreatMemory` へ移行候補 |
| 追跡調整 | `preferredChaseDistance` | 望ましい追跡距離 | 位置調整用。命中判定からは分離 |
| 追跡調整 | `disengageDistance` | 離脱距離 | `PreyChaseDesire` へ移行候補 |
| 追跡調整 | `stopMoveThreshold` | 停止閾値 | motor / chase desire へ移行候補 |
| 追跡調整 | `resumeMoveThreshold` | 再移動閾値 | motor / chase desire へ移行候補 |
| 旧攻撃 | `attackDistance` | 旧攻撃距離 | 廃止候補。攻撃弧・接触・確率評価へ寄せる |
| 旧攻撃 | `attackDamage` | 旧攻撃力 | `BiteAttackAction` / `MeleeAttackAction` の既定値へ読み替え |
| 旧攻撃 | `attackCooldown` | 旧攻撃間隔 | 各攻撃 action の clock/cost に読み替え |
| 突進 | `chargeArc` | 突進攻撃範囲 | `ChargeAttackAction` へ移行候補 |
| 突進 | `chargeDamageScale` | 突進ダメージ倍率 | `ChargeAttackAction` へ移行候補 |
| 突進 | `chargeManaCost` | 突進mana消費 | `ChargeAttackAction` へ移行候補 |
| 突進 | `chargeContactPadding` | 接触補正 | `ChargeAttackAction` へ移行候補 |
| 突進 | `chargeAttackClock` | 突進攻撃時計 | `ChargeAttackAction` へ移行候補 |
| 噛みつき | `biteArc` | 噛みつき範囲 | `BiteAttackAction` へ移行候補 |
| 噛みつき | `biteDamage` | 噛みつきダメージ | `BiteAttackAction` へ移行候補 |
| 噛みつき | `biteManaCost` | 噛みつきmana消費 | `BiteAttackAction` へ移行候補 |
| 噛みつき | `biteAttackClock` | 噛みつき攻撃時計 | `BiteAttackAction` へ移行候補 |
| 近接 | `meleeArc` | 近接攻撃範囲 | `MeleeAttackAction` へ移行候補 |
| 近接 | `meleeDamage` | 近接ダメージ | `MeleeAttackAction` へ移行候補 |
| 近接 | `meleeManaCost` | 近接mana消費 | `MeleeAttackAction` へ移行候補 |
| 近接 | `meleeAttackClock` | 近接攻撃時計 | `MeleeAttackAction` へ移行候補 |
| 脅威場 | `attackThreatPulseScore` | 攻撃時脅威スコア | `ThreatPulseEmitter` へ移行候補 |
| 脅威場 | `attackThreatPulseRadius` | 脅威パルス半径 | `ThreatPulseEmitter` へ移行候補 |
| 痕跡 | `attackTraceScale` | 攻撃痕跡規模 | 表示・field連携候補 |
| 痕跡 | `attackTraceDuration` | 攻撃痕跡時間 | 表示・field連携候補 |
| 痕跡 | `attackTraceDepth` | 攻撃痕跡深度 | 表示・field連携候補 |
| 徘徊 | `wanderWaves` | 徘徊波形 | `WaveGene` として継続。表示・DNA対象 |

## Manager保持値

| 場所 | 項目 | 役割 |
| --- | --- | --- |
| `herbivoreManager` | `genome` | 通常スポーン時の基準 genome |
| `herbivoreManager` | `nextGenerationGenome` | 世代更新後に使う genome |
| `herbivoreManager` | `genomes` | phase/page ごとの保存DNAプール |
| `predatorManager` | `genome` | 通常スポーン時の基準 genome |
| `predatorManager` | `nextGenerationGenome` | 世代更新後に使う genome |
| `predatorManager` | `genomes` | phase/page ごとの保存DNAプール |

## 現状の注意点

- 草食と捕食で同じ意味の項目名が揃っていない。代表例は `visionturnAngle` と `visionTurnAngle`。
- 草食DNAは `HG:` + Base64 binary、捕食DNAは `PGJ:` + JSON で形式が分かれている。
- 捕食には旧攻撃パラメータと複合攻撃パラメータが併存している。
- `health` / `maxHealth` などの状態値は旧 genome の正本ではない。
- `Resource.mana` / `maxMana` / `resourceCategory` / `speciesID` は個体資源・相・系統側の正本として扱われている。
