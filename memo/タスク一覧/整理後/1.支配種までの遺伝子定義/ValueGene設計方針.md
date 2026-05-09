# ValueGene設計方針

## 目的

遺伝させる内部数値を `ValueGene` として一元管理する。

`AIComponentGene` / `AIComponentSet` は organ 構造遺伝子、`ValueGene` は organ 内部数値遺伝子として分ける。

## 基本方針

- 遺伝させる数値は基本的にすべて `ValueGene` に保持する。
- `ValueGene` は可視性を優先し、organ ごとの入れ子構造体にまとめる。
- organ が現在発現していなくても、対応する数値は保持してよい。
- organ が発現したとき、保持済みの `ValueGene` を organ 内変数へ適用する。
- 旧 `HerbivoreGenome` / `PredatorGenome` の数値は、段階的に `ValueGene` へ移す。

## 役割分担

| 種別 | 保存するもの | 例 |
| --- | --- | --- |
| `ValueGene` | organ 内部で使う具体的な数値 | 視界距離、検知距離、攻撃力、攻撃範囲、mana cost |
| `AIComponentGene` | organ の構造遺伝 | `enabled`, `level`, `weight`, `mutationChanceT/G` |
| `AIComponentSet` | organ 構造遺伝子の集合 | `public List<AIComponentGene> genes` |
| `Resource` | 生存中の相・資源・系統 | `resourceCategory`, `speciesID`, `mana` |

## 参照形

可視性を上げるため、数値は organ 名ごとに束ねる。

```csharp
valueGene.PredatorVisionSense.threatDetectDistance
valueGene.PreyVisionSense.preyDetectDistance
valueGene.BiteAttackAction.biteDamage
valueGene.ChargeAttackAction.chargeManaCost
valueGene.RandomEvasionAction.evasionDistance
```

この形にすると、どの organ の内部値なのかが名前から追いやすい。

## 適用形

個体生成または genome 適用時に、存在する organ へ `ValueGene` を流し込む。

```csharp
if (target.TryGetComponent<PredatorVisionSense>(out var organ))
{
    organ.threatDetectDistance = valueGene.PredatorVisionSense.threatDetectDistance;
}
```

方針:

- 適用は `TryGetComponent<各organ>()` を基本にする。
- organ が存在する場合だけ対応値を代入する。
- organ が存在しない場合、`ValueGene` は値を保持したままにする。
- 後から organ が発現・復活したとき、その値を使えるようにする。

## 旧genomeからの移行

旧 genome の数値は次のように移す。

| 旧項目 | `ValueGene`側の候補 |
| --- | --- |
| `visionDistance` | `FoodVisionSense.visionDistance`, `PreyVisionSense.visionDistance` など |
| `threatDetectDistance` | `ThreatVisionSense.threatDetectDistance` |
| `preyDetectDistance` | `PreyVisionSense.preyDetectDistance` |
| `memorytime` | `FoodMemory.memoryTime`, `PreyMemory.memoryTime`, `ThreatMemory.memoryTime` |
| `forwardForce` | `GroundMotor.forwardForce` |
| `turnForce` | `GroundMotor.turnForce` |
| `eatspeed` | `GrassEatAction.eatSpeed`, `CorpseEatAction.eatSpeed` |
| `runAwayDistance` | `ThreatAvoidanceDesire.runAwayDistance` |
| `evasionDistance` | `RandomEvasionAction.evasionDistance` |
| `biteDamage` | `BiteAttackAction.biteDamage` |
| `biteManaCost` | `BiteAttackAction.biteManaCost` |
| `meleeDamage` | `MeleeAttackAction.meleeDamage` |
| `meleeManaCost` | `MeleeAttackAction.meleeManaCost` |
| `chargeDamageScale` | `ChargeAttackAction.chargeDamageScale` |
| `chargeManaCost` | `ChargeAttackAction.chargeManaCost` |

## AIComponentGeneとの関係

`ValueGene` と `AIComponentGene` は重複ではなく、粒度が違う。

| 判断 | 使う遺伝子 |
| --- | --- |
| organ があるか | `AIComponentGene.enabled` |
| organ が退化しているか | `AIComponentGene.isVestigialOrgan` |
| organ が必須器官か | `AIComponentGene.isVitalOrgan` |
| organ の大まかな強さ | `AIComponentGene.level` |
| 行動選択上の重み | `AIComponentGene.weight` |
| organ 内部の具体値 | `ValueGene` |

例:

- `AIComponentGene` で `PreyVisionSense` が active。
- `ValueGene.PreyVisionSense.preyDetectDistance` が実際の検知距離。
- `AIComponentGene.weight` は、視覚情報をどれくらい行動判断へ反映するかに使う。

## 保存方針

Genome の保存・配布は `GeneDataManager` に寄せる。

```csharp
public static class GeneDataManager
{
    public static List<ValueGene> genes_v;
    public static List<AIComponentGene> genes_s;
}
```

`ValueGene` は内部数値遺伝子、`AIComponentGene` は organ 構造遺伝子として同時に扱う。

既存移行中は、旧 `HerbivoreGenome` / `PredatorGenome` を残したまま、読み替えで `ValueGene` を生成する。

親にない organ の `ValueGene` は、`GeneDataManager` に保持する初期値用 `genes_v` から補完する。

## phase1で決めること

- `ValueGene` の最初の構造体分割。
- 旧 genome 項目から `ValueGene` への対応表。
- `GeneDataManager` と organ 適用処理の置き場所。
- DNA / JSON に `ValueGene` と `AIComponentSet` をどう保存するか。
- UIで `ValueGene` を organ単位に表示する方針。
