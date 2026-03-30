# 1.生態系コア強化

参照元: `git show f5a7322:memo/ROADMAP.md`

## 目的

- wander の見え方を改善し、地形や環境差に応じた行動差を増やす。

## 現状（git参照時点）

- 餌・脅威・境界・wander の加重合成は実装済み。
- 地形法線補正、evasion、zigzag、追跡停止閾値は一部実装済み。
- threat map の主移動ロジック統合は未完。
- heat field を安全行動へ使うロジックは未実装。

## 作業区分

### 設計資料の校正

1. 行動意思決定仕様の明文化
- threat map / heat field の統合方針を仕様として確定する。
- 対象ファイル:
  - `memo/タスク一覧/1.生態系コア強化/設定：動物の移動・場（map）のまとめ.txt`
  - `memo/タスク一覧/1.生態系コア強化/設定：ゲノム構造（基礎拡張）.txt`

2. 行動パラメータ設計の更新
- 地形、遮蔽、記憶の設計値を実装項目に対応づける。
- 対象ファイル:
  - `memo/設定/設定：ゲノム構造設計.txt`
  - `memo/タスク一覧/0.UI整理/資料/拡張方針.txt`

### 本実装

1. 行動ベクトル統合
- threat / heat を既存の移動合成に反映する。
- 対象ファイル:
  - `Assets/script/Ingame/behaviour/herbivore/herbivoreBehaviour.cs`
  - `Assets/script/Ingame/behaviour/predator/predatorBehaviour.cs`
  - `Assets/script/Ingame/AI/AnimalAICommon.cs`

2. 場の評価・可視化連携
- threat / heat の場データを行動判断に接続する。
- 対象ファイル:
  - `Assets/script/Ingame/AI/threatmap_calc.cs`
  - `Assets/script/Ingame/AI/ThreatMapsGenerator.cs`
  - `Assets/script/Ingame/balance/ResourceDispenser.cs`

3. 地形・境界条件との整合
- 境界回避、地形依存移動との整合を取る。
- 対象ファイル:
  - `Assets/script/Ingame/WorldGenerator.cs`
  - `Assets/script/Ingame/behaviour/grassland.cs`

## 完了条件

- 地形条件の違いで移動パターンが目視で判別できる。
- 追跡開始・継続・放棄の遷移根拠をログで確認できる。

## 参照

- `../0.UI整理/資料/拡張方針.txt`
- `./設定：動物の移動・場（map）のまとめ.txt`
- `./設定：ゲノム構造（基礎拡張）.txt`
