# 6.魔素基盤構築

参照元: `git show f5a7322:memo/ROADMAP.md`

## 目的

- 群知性や多対多戦闘より前に、魔法実装の前提となる魔素を環境場として成立させる。

## 現状（git参照時点）

- heat field は存在。
- mana field は未実装。
- 魔法側への接続経路未整備。

## 作業区分

### 設計資料の校正

1. 魔素経済モデルの定義
- 魔素の拡散・吸収・消費の式を確定する。
- 対象ファイル:
  - `memo/設定/設定4：経済.txt`
  - `memo/設定/設定4：スケール.txt`

2. heat と mana の役割分離
- 既存 heat 系との境界を設計上で明確化する。
- 対象ファイル:
  - `memo/タスク一覧/0.遺伝子設計変更/現状の遺伝子構造まとめ.txt`
  - `memo/設定/設定1：移動についてのメモ.txt`

### 本実装

1. 環境場の実装
- mana field グリッドと更新ループを追加する。
- 対象ファイル:
  - `Assets/script/Ingame/Environment/ResourceDispenser.cs`
  - `Assets/script/Ingame/AI/ThreatMap/threatmap_calc.cs`
  - `Assets/script/Ingame/AI/ThreatMap/ThreatMapsGenerator.cs`

2. 生体行動との相互作用
- 個体能力補正を場データと接続する。
- 対象ファイル:
  - `Assets/script/Ingame/Creatures/Herbivore/herbivoreBehaviour.cs`
  - `Assets/script/Ingame/Creatures/Predator/predatorBehaviour.cs`
  - `Assets/script/Ingame/Environment/Resource.cs`

## 完了条件

- 魔素濃度が時間変化し、個体能力へ反映される。
- 同条件再生で濃度分布を再現できる。

## 参照

- `../0.遺伝子設計変更/現状の遺伝子構造まとめ.txt`
- `../../設定/設定4：経済.txt`
- `../../設定/設定4：スケール.txt`
