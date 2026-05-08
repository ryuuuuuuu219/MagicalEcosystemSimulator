# 4.挙動AI分離

## 目的

- 既存の行動AIを機能コンポーネントへ分割し、2.生態系コア強化と4.戦闘システム拡張の間に独立工程を置く。

## 作業区分

### 設計資料の校正

1. 分離対象と依存関係の整理
- 感覚・欲求・攻撃の責務境界を確定する。
- 対象ファイル:
  - `memo/設定/設定2：挙動AI分離.txt`
  - `memo/設定/設定2：挙動AI管理.txt`

2. 遺伝・生成フローとの接続設計
- node 構造の配布と適用タイミングを明文化する。
- 対象ファイル:
  - `memo/設定/設定7：遺伝システム.txt`
  - `memo/設定/設定共通：ゲノム構造設計.txt`

### 本実装

1. 挙動機能の責務分離
- 制御AI直書きロジックをモジュール単位へ切り出す。
- 対象ファイル:
  - `Assets/script/Ingame/AI/AnimalAICommon.cs`
  - `Assets/script/Ingame/Creatures/Herbivore/herbivoreBehaviour.cs`
  - `Assets/script/Ingame/Creatures/Predator/predatorBehaviour.cs`

2. 生成・適用基盤の実装
- 分離挙動の配布/有効化フローを組み込む。
- 対象ファイル:
  - `Assets/script/Ingame/UI/Menu/ingame/AdvanceGenerationController.cs`
  - `Assets/script/Ingame/Creatures/Herbivore/herbivoreManager.cs`
  - `Assets/script/Ingame/Creatures/Predator/predatorManager.cs`
  - `Assets/script/Ingame/Environment/Resource.cs`

## 完了条件

- 主要な行動機能が責務単位で分離され、依存関係が明文化されている。
- 2.生態系コア強化と4.戦闘システム拡張の間で再利用できるAI基盤として扱える。

・
phase0　organ設計への移行　も完了条件に含め

## 参照

- `../../設定/設定2：挙動AI分離.txt`
- `../../設定/設定2：挙動AI管理.txt`
- `../2.生態系コア強化/README.md`
- `../4.戦闘システム拡張/README.md`
