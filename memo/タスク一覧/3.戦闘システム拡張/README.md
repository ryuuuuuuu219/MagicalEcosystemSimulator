# 3.戦闘システム拡張

参照元: `git show f5a7322:memo/ROADMAP.md`

## 目的

- 単純追跡攻撃から、関係性ベースの段階判断戦闘へ拡張する。

## 現状（git参照時点）

- 近接攻撃、噛みつき、チャージ、攻撃トレースは拡張済み。
- 攻撃クロック、脅威パルスは実装済み。
- `speciesID` / `factionID` / IFF は未整備。

## 作業区分

### 設計資料の校正

1. 戦闘判定ルールの更新
- 相・IFF・撤退条件の整合を取り、判定順序を定義する。
- 対象ファイル:
  - `memo/タスク一覧/3.戦闘システム拡張/設定：相の定義.txt`
  - `memo/設定/IFF定義.txt`

2. 攻撃モデルの仕様更新
- 近接攻撃仕様と実装可能範囲を同期する。
- 対象ファイル:
  - `memo/タスク一覧/3.戦闘システム拡張/設定：近接攻撃.txt`
  - `memo/タスク一覧/0.遺伝子設計変更/現状の遺伝子構造まとめ.txt`

### 本実装

1. 攻撃行動と戦闘状態遷移
- 攻撃行動と停止/撤退の遷移を実装する。
- 対象ファイル:
  - `Assets/script/Ingame/AI/PredatorCombatLibrary.cs`
  - `Assets/script/Ingame/behaviour/predator/predatorBehaviour.cs`
  - `Assets/script/Ingame/behaviour/herbivore/herbivoreBehaviour.cs`

2. IFF・識別軸の導入
- species/faction に基づく判定を戦闘に反映する。
- 対象ファイル:
  - `Assets/script/Library/Enums/SimulationEnums.cs`
  - `Assets/script/Ingame/AI/AnimalAICommon.cs`
  - `Assets/script/Ingame/behaviour/predator/predatorManager.cs`

3. 戦闘可視化の整備
- 攻撃トレースと観測表示を整備する。
- 対象ファイル:
  - `Assets/script/Ingame/AI/AttackTraceLibrary.cs`
  - `Assets/script/Ingame/UI/CommonAttackVisualUIManager.cs`

## 完了条件

- 同種/異種/同派閥/敵対の判定が戦闘結果に反映される。
- 待ち伏せ・追跡放棄・撤退の発生条件が再現可能。

## 参照

- `../0.遺伝子設計変更/現状の遺伝子構造まとめ.txt`
- `./設定：近接攻撃.txt`
- `./設定：相の定義.txt`
