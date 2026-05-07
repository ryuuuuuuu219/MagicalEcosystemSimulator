# 4.戦闘システム拡張

参照元: `git show f5a7322:memo/ROADMAP.md`

## 目的

- 単純追跡攻撃から、関係性ベースの段階判断戦闘へ拡張する。

## 現状（git参照時点）

- 近接攻撃、噛みつき、チャージ、攻撃トレースは拡張済み。
- 脅威パルスは実装済み。
- 個別周期系の攻撃パラメータは簡略化のため設計から削除し、クールダウンとエネルギーで攻撃頻度を扱う。
- `speciesID` / IFF は未整備。

## 作業区分

### 設計資料の校正

1. 戦闘判定ルールの更新
- 相・IFF・撤退条件の整合を取り、判定順序を定義する。
- 対象ファイル:
  - `memo/設定/設定3：相の定義.txt`
  - `memo/設定/設定共通：IFF定義.txt`

2. 攻撃モデルの仕様更新
- 近接攻撃仕様と実装可能範囲を同期する。
- 対象ファイル:
  - `memo/設定/設定3：攻撃設計.txt`
  - `memo/設定/設定3：近接攻撃.txt`
  - `memo/タスク一覧/0.遺伝子設計変更/現状の遺伝子構造まとめ.txt`

### 本実装

1. 攻撃行動と戦闘状態遷移
- 攻撃行動と停止/撤退の遷移を実装する。
- 対象ファイル:
  - `Assets/script/Ingame/Combat/PredatorCombatLibrary.cs`
  - `Assets/script/Ingame/Creatures/Predator/predatorBehaviour.cs`
  - `Assets/script/Ingame/Creatures/Herbivore/herbivoreBehaviour.cs`

2. IFF・識別軸の導入
- speciesID と敵対トリガーに基づく判定を戦闘に反映する。
- 対象ファイル:
  - `Assets/script/Shared/Enums/SimulationEnums.cs`
  - `Assets/script/Ingame/AI/AnimalAICommon.cs`
  - `Assets/script/Ingame/Creatures/Predator/predatorManager.cs`

3. 戦闘可視化の整備
- 攻撃トレースと観測表示を整備する。
- 対象ファイル:
  - `Assets/script/Ingame/Presentation/CombatVisuals/AttackTraceLibrary.cs`
  - `Assets/script/Ingame/Presentation/CombatVisuals/CommonAttackVisualUIManager.cs`

## 完了条件

- 同種/異種/中立/敵対の判定が戦闘結果に反映される。
- 待ち伏せ・追跡放棄・撤退の発生条件が再現可能。

## 参照

- `../0.遺伝子設計変更/現状の遺伝子構造まとめ.txt`
- `../../設定/設定3：攻撃設計.txt`
- `../../設定/設定3：近接攻撃.txt`
- `../../設定/設定3：相の定義.txt`
