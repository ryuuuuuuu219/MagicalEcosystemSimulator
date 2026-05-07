# organ インストーラー確率方針

## 現状

`Assets/script/Ingame/Creatures/organ/Core/AnimalAIInstaller.cs` は、まだ確率抽選を実装していない。

現在の `InstallDefaultOrgans()` は固定導入だけ。

| コンポーネント | 現在の導入 |
| --- | --- |
| `AnimalBrain` | 100% |
| `AIMemoryStore` | 100% |
| `GroundMotor` | 100% |
| `CreatureMotorBootstrap` | 100% |

## 確率設計の分類

### 1. 固定導入

個体AIの基盤として必須。遺伝的アルゴリズムで外さない。

```text
AnimalBrain
AIContext
AIMemoryStore
GroundMotor
CreatureMotorBootstrap
MovementTelemetry
CreatureDeathState
```

`AIContext` と `MovementTelemetry` は MonoBehaviour として AddComponent する対象ではないが、機能上は固定扱い。

### 2. 種別 default 導入

草食なら草食に必要な organ、捕食なら捕食に必要な organ を高確率または固定で導入する。

完全固定にすると個体差が出にくいため、最初は「生存必須は100%、性格差に関わるものは60-90%」にする。

### 3. 低確率変異導入

本来その種が持たない機能を低確率で入れる。

例:

- 草食が `DamageAvoidanceDesire` を強めに持つ。
- 捕食が `ManaFieldAttractionDesire` を強めに持つ。
- 魔法適性 organ をまれに持つ。

### 4. 自動導入

直接抽選しない。親 organ が有効になったら一緒に入れる。

| 親 organ | 自動導入 | 理由 |
| --- | --- | --- |
| `ChargeAttackAction` | `ThreatPulseEmitter` | 攻撃時の脅威度反映。 |
| `BiteAttackAction` | `ThreatPulseEmitter` | 攻撃時の脅威度反映。 |
| `MeleeAttackAction` | `ThreatPulseEmitter` | 攻撃時の脅威度反映。 |
| `MagicAttackAction` | `MagicCooldownState` | 魔法コストとクールダウン判定。 |
| `MagicProjectileAttackAction` | `MagicCooldownState` | 遠距離魔法のクールダウン判定。 |
| `MagicEvasionAction` | `MagicCooldownState` | 魔法回避のクールダウン判定。 |
| `MagicDefenseAction` | `MagicCooldownState` | 魔法防御のクールダウン判定。 |

## 推奨する抽選順

1. 固定導入 organ を追加する。
2. 種別 default organ を確率で追加する。
3. 低確率変異 organ を追加する。
4. 自動導入 organ を解決する。
5. `AIComponentGene.level` を生成する。

## level の初期値

確率で enabled が決まった後、level は 0.5-1.5 の範囲から始める。

```text
level = RandomRange(0.5, 1.5)
```

魔法系や phase evolution のような強い機能は、最初は 0.2-1.0 に抑える。

## 調整ルール

- 生存率が低すぎる場合:
  - `ThreatAvoidanceDesire`
  - `BoundaryAvoidanceDesire`
  - `FieldManaAbsorbAction`
  の確率または level を上げる。
- 捕食が弱すぎる場合:
  - `PreyVisionSense`
  - `PreyChaseDesire`
  - `BiteAttackAction`
  - `MeleeAttackAction`
  の確率または level を上げる。
- mana が過剰に増える場合:
  - `FieldManaAbsorbAction`
  - `ManaFieldAttractionDesire`
  - 魔法攻撃後の mana 回収係数
  を下げる。
- 個体差が少ない場合:
  - default organ の 100% を 80-95% へ落とす。
  - 低確率変異 organ を 3-10% へ上げる。
