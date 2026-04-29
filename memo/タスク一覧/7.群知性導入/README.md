# 7.群知性導入

参照元: `git show f5a7322:memo/ROADMAP.md`

## 目的

- 個体AIと魔法手段を土台に、群れ意図と役割分担を持つ集団AIへ移行する。

## 現状（git参照時点）

- 現在は個体単位判断が中心。
- 群れ行動は設計段階で、コード基盤未整備。
- 魔法関連の前提設計を先に固める方針へ変更。

## 作業区分

### 設計資料の校正

1. 群れ意思決定モデルの確定
- GroupIntent、リーダー選出、共有規則を定義する。
- 対象ファイル:
  - `memo/設定/設定6：群知性.txt`
  - `memo/設定/設定共通：IFF定義.txt`

2. 群れ拡張ゲノムの確定
- 役割分担に必要な遺伝項目を確定する。
- 対象ファイル:
  - `memo/設定/設定6：ゲノム構造（群れ拡張）.txt`
  - `memo/設定/設定共通：ゲノム構造設計.txt`

### 本実装

1. 群れ管理と意図共有
- 群れ単位のターゲット/行動共有を実装する。
- 対象ファイル:
  - `Assets/script/Ingame/Creatures/Predator/predatorManager.cs`
  - `Assets/script/Ingame/Creatures/Herbivore/herbivoreManager.cs`
  - `Assets/script/Ingame/AI/AnimalAICommon.cs`

2. 戦闘・UI連動
- 群れ状態を戦闘判断と観測UIへ反映する。
- 対象ファイル:
  - `Assets/script/Ingame/Creatures/Predator/predatorBehaviour.cs`
  - `Assets/script/Ingame/UI/Menu/ingame/UImanager/WorldUIManager.StateView.cs`
  - `Assets/script/Ingame/UI/Menu/ingame/UImanager/WorldUIManager.ObjectList.cs`

## 完了条件

- 群れ単位で同一意図を共有し、個体が役割分担して行動する。
- リーダー喪失時の再編成が発生する。
- 魔法使用個体を含む役割分担が再現可能。

## 参照

- `../0.遺伝子設計変更/現状の遺伝子構造まとめ.txt`
- `../../設定/設定6：群知性.txt`
- `../../設定/設定6：ゲノム構造（群れ拡張）.txt`
