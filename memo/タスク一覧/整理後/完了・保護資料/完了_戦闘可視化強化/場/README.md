# 場

## 目的

- 魔力場、属性場、方向性を持つ場、テンソル場の実装単位を整理する。
- 既存の `heat field` / `threat map` と重複しない形で、場データの参照・更新・可視化の責務を分ける。

## 対象

- 魔力場
- 属性場
- vector field
- tensor field
- heat field / threat map との接続

## 方針

- 場の生成・更新はシミュレーション側の責務にする。
- UI は場データを直接生成せず、参照と表示に寄せる。
- 雷のスカラー場は作らない。
- 風は三次元ベクトル場として扱う。

## 対象候補

- `Assets/script/Ingame/Environment/HeatFieldManager.cs`
- `Assets/script/Ingame/AI/ThreatMap/threatmap_calc.cs`
- `Assets/script/Ingame/Environment/ResourceDispenser.cs`
- 新規: `Assets/script/Ingame/Environment/Fields/`
