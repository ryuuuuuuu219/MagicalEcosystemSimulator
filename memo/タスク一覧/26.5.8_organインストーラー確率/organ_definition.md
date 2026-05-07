# organ 定義

作成日: 2026-05-08

## 定義

organ は、生体器官を再現するコンポーネント、つまり `.cs` ファイルである。

個体は `AddComponent` によって organ を獲得する。  
この organ の獲得、欠落、強度変化を、進化の演算対象として扱う。

```text
organ = biological function component
evolution = AddComponent / remove component / component level change
```

## 設計上の意味

従来の `herbivoreBehaviour` / `predatorBehaviour` は、視覚、記憶、欲求、移動、攻撃、mana 吸収などをひとつの制御AIにまとめていた。

organ 分離後は、個体が持つ organ の組み合わせが、その個体の能力差になる。

例:

- `FoodVisionSense` を持つ個体は餌を視覚で見つけられる。
- `ThreatAvoidanceDesire` を持つ個体は脅威から逃げる欲求を持つ。
- `BiteAttackAction` を持つ個体は噛みつき攻撃を行える。
- `MagicEvasionAction` を持つ個体は魔法回避を行える。

## 進化として扱う単位

organ 進化では、以下を遺伝的アルゴリズムの対象にする。

| 対象 | 意味 |
| --- | --- |
| organ の有無 | その機能を獲得しているか。 |
| organ の level | その器官がどの程度強いか。 |
| organ の tuning | 距離、角度、コスト、クールダウンなど。 |
| organ の依存関係 | 攻撃 organ があるなら `ThreatPulseEmitter` も入る、など。 |

## AddComponent の意味

`AddComponent` は単なる実装手段ではなく、進化による機能獲得の表現である。

```text
AddComponent<FoodVisionSense>()
  -> 視覚による餌認識器官を獲得

AddComponent<BiteAttackAction>()
  -> 噛みつき攻撃器官を獲得

AddComponent<MagicCooldownState>()
  -> 魔法発動を制御する器官を獲得
```

## ここまでに読み取った追加情報

### 1. organ は AI 部品ではなく、生体機能の単位

単なるコード分割ではなく、個体が生まれつき持つ器官、後天的または世代更新で獲得する機能として扱うのが主目的。

そのため、`Sense`、`Desire`、`Action` というソフトウェア分類は内部整理であり、シミュレーション上は「視覚器官」「逃走反射」「攻撃器官」「魔法器官」のように読む。

### 2. installer は進化演算器に近い

`AnimalAIInstaller` は prefab 初期化係ではなく、個体の organ set を実体化する役割を持つ。

将来的には以下を行う。

1. genome から organ 候補を読む。
2. 確率で organ の有無を決める。
3. 依存 organ を補完する。
4. `AddComponent` で実際の organ を個体に付与する。
5. level と tuning を organ に反映する。

### 3. gene は数値パラメータだけでは足りない

現状の `HerbivoreGenome` / `PredatorGenome` は、既に全機能を持っている前提で数値だけを変える。

organ 設計では、進化対象は数値だけではなく「機能そのものの有無」になる。

```text
old genome: visionDistance = 50
new genome: FoodVisionSense enabled = true, level = 0.8
```

### 4. fixed organ と variable organ を分ける必要がある

すべてを確率化すると個体が成立しない。

`AnimalBrain`、`GroundMotor`、`CreatureMotorBootstrap`、`AIMemoryStore`、`CreatureDeathState` のような生存基盤は fixed organ として扱うのが自然。

一方で、餌視覚、追尾、攻撃、魔法、mana field 誘引などは variable organ として進化させる。

### 5. 自動導入 organ は遺伝対象と区別する

`ThreatPulseEmitter` のように、攻撃 organ があるなら必要になる機能は、独立した進化対象ではなく依存 organ として自動導入するほうがよい。

これにより「攻撃できるのに脅威度に反映されない」などの不整合を避けられる。

### 6. 魔法 organ は通常器官と別枠で扱う

魔法は攻撃、回避、防御、field 反映、cooldown をまたぐため、通常の攻撃 organ より複合的。

`MagicCooldownState` のような制御器官を自動導入し、`MagicAttackAction` や `MagicEvasionAction` は低確率変異として扱うのがよい。

### 7. organ は将来的に UI 表示・ログ表示の単位にもなる

個体詳細 UI で「この個体がどの organ を持つか」を出せると、進化結果が観察しやすい。

例:

```text
Organs:
- FoodVisionSense Lv0.84
- ThreatAvoidanceDesire Lv1.12
- FieldManaAbsorbAction Lv0.63
```

これは進化ログや世代比較にも使える。
