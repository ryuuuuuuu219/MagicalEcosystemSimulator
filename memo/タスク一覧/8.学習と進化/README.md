# 8.学習と進化

参照元: `git show f5a7322:memo/ROADMAP.md`

## 目的

- 習得・強化・進化のループを構築し、世代差を明確化する。

## 現状（git参照時点）

- 学習系詳細は未実装。
- 遺伝システムとの接続は未整備。

## 作業区分

### 設計資料の校正

1. 学習状態と進化指標の定義
- 使用確率、解放状態、感情パラメータを定義する。
- 対象ファイル:
  - `memo/設定/設定7：遺伝システム.txt`
  - `memo/設定/設定7：感情パラメーター.txt`

2. ゲノム構造との接続整理
- 継承項目の責務を整理する。
- 対象ファイル:
  - `memo/設定/設定共通：ゲノム構造設計.txt`
  - `memo/設定/設定6：ゲノム構造（群れ拡張）.txt`

### 本実装

1. 遺伝処理・世代更新
- 学習結果を世代更新へ反映する。
- 対象ファイル:
  - `Assets/script/Ingame/UI/Menu/ingame/AdvanceGenerationController.cs`
  - `Assets/script/Ingame/Genome/GenomeSerializer.cs`
  - `Assets/script/Ingame/Diagnostics/GenomeLogger.cs`

2. 個体挙動への反映
- 状態量を行動選択へ反映する。
- 対象ファイル:
  - `Assets/script/Ingame/Creatures/Herbivore/herbivoreBehaviour.cs`
  - `Assets/script/Ingame/Creatures/Predator/predatorBehaviour.cs`
  - `Assets/script/Ingame/Genome/GenerationLog.cs`

## 完了条件

- 個体学習結果が世代更新で再現可能な形で継承される。
- 状態量の変化が行動選択へ反映される。

## 参照

- `../0.遺伝子設計変更/現状の遺伝子構造まとめ.txt`
- `../../設定/設定7：遺伝システム.txt`
- `../../設定/設定共通：ゲノム構造設計.txt`
- `../../設定/設定7：感情パラメーター.txt`
