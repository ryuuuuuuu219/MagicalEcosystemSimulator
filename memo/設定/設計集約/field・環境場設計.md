# field・環境場設計

## 目的

- threat map、heat field、mana field、属性場、テンソル場を同じ設計軸で扱う。
- `memo/設定` 全体の読み順を field 優先へ寄せる。
- 実装時に、既存の field 基盤を重複実装せず、参照・更新・可視化を分けて扱えるようにする。

## 正本

- `../設定1：field設計まとめ.txt`
- `../設定4：スケール.txt`
- `../設定4：経済.txt`
- `../設定共通：状態機械まとめ（IFF前段階）.txt`
- `../設定5：魔法：属性の考察.txt`
- `../設定5：魔法一覧.txt`

## 全体構造

```text
World
  ├─ scalar field
  │   ├─ threat score
  │   ├─ heat
  │   └─ mana
  ├─ vector field
  │   ├─ low threat direction
  │   ├─ heat gradient
  │   ├─ mana flow
  │   └─ wind vector
  ├─ tensor field
  │   ├─ directional resistance
  │   ├─ attribute flow route
  │   └─ spatial distortion
  └─ observers
      ├─ individual AI
      ├─ combat / magic
      └─ UI / camera
```

## 場の分類

### threat map

用途:
- 危険地帯
- 攻撃パルス
- 低脅威方向探索
- 戦闘観測 UI の背景情報

現状:
- `threatmap_calc` が存在する。
- `AddThreatPulse`、`GetThreatScore`、`GetLowThreatDirection`、`EvaluateBestDirection` がある。
- `ThreatMapsGenerator` による debug 可視化がある。
- 攻撃時 threat pulse は `PredatorCombatLibrary` から発生している。

未完:
- 草食・肉食の主移動ロジックへの全面統合。
- 警告行動、待ち伏せ、縄張り侵入を threat 発生源に含める整理。

### heat field

用途:
- 環境熱量
- 火属性 / 氷属性との接続
- 危険温度からの回避
- 将来の熱ダメージ、鈍足、属性耐性

現状:
- `HeatFieldManager` が存在する。
- heat grid、拡散、減衰、debug 描画がある。
- 代謝と死骸分解から heat が場へ加算される。

未完:
- `SampleHeat` を個体 AI の移動判断へ接続すること。
- 熱ダメージ、鈍足、属性耐性、恒星由来入熱。
- 適温誘引まで含めるか、危険温度回避に絞るかの決定。

### mana field

用途:
- 魔素濃度
- 魔法発動コスト
- 魔法補正
- 支配種進化
- 空間魔法の痕跡

現状:
- 未実装。
- 設計上は拡散する scalar field。
- 魔法使用で消費し、死骸分解や転移で増加する。

最小実装:
- `ManaFieldManager` を作る。
- grid 解像度と world 座標変換を定義する。
- 更新式は `Δmana = diffusionRate * (neighborAverage - self)` から始める。
- 減衰なし。
- 世代更新で reset。

### 属性場

用途:
- 火 / 氷 / 風 / 空間の field 連動。
- 雷は field として保持せず、着弾・連鎖・短時間 pulse の event として扱う。
- 属性魔法の地形・個体・場への継続影響。

設計:
- 火と氷は heat field と接続する。
- 空間は mana field と接続する。
- 風は三次元 vector field として実装する。
- 雷の scalar field は設計しない。
- 属性場を常時 grid として持つ対象は、heat / mana / wind vector を優先する。
- 雷はイベント残留を持たせるとしても、field ではなく短時間の攻撃・誘導イベントとして扱う。

### テンソル場

用途:
- 方向性
- 勾配
- 異方性
- 魔力流
- 空間魔法の歪み
- 戦闘可視化

設計:
- scalar field は値。
- vector field は方向と勾配。
- tensor field は方向ごとの抵抗や流れ方の違い。
- 最初は UI / debug 可視化から入り、シミュレーション本体への影響は後段にする。

## 接続先

### 個体 AI

- `2.生態系コア強化` で扱う。
- AI は field を単独の状態遷移条件にせず、距離、視認、記憶、境界、エネルギーと合成して判断する。
- 初期接続は threat と heat を回避・離脱寄りに使う。

### 戦闘可視化

- `1.戦闘可視化強化` で扱う。
- 指定個体と目標のカメラに、周辺 field 値、勾配、攻撃パルスを重ねる。
- UI は field を表示するだけで、更新責務を持たない。

### 魔素基盤

- `5.魔素基盤構築` で扱う。
- mana field は新規実装対象。
- heat field や threat map の実装を再利用する場合も、責務を混ぜない。

### 属性魔法

- `6.属性魔法拡張` で扱う。
- 属性効果は field へ残留させるか、hit event として処理するかを分ける。
- 火 / 氷は heat、空間は mana を優先接続先にする。

## 実装原則

- field 更新、field 参照、field 可視化を分ける。
- 既存の `ThreatMapsGenerator` と `HeatFieldManager` を再実装しない。
- mana field は未実装なので最初にデータ型、更新周期、世代 reset を決める。
- scalar / vector / tensor の共通表示 API を検討する。
- debug 表示は戦闘観測カメラからも利用できるようにする。

## 次に決めること

1. `ManaFieldManager` を `HeatFieldManager` と同型にするか、共通 field 基底へ寄せるか。
2. `SampleHeat` / `GetThreatScore` / `GetLowThreatDirection` を AI 側でどう読むか。
3. scalar / vector / tensor の debug 表示 API を共通化するか。
4. 風の三次元 vector field の解像度、更新周期、可視化方法を決める。
5. 世代更新時に reset する field と、継続する field を分ける。
