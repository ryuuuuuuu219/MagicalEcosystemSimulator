# 6.属性魔法拡張

参照元: `git show f5a7322:memo/ROADMAP.md`

## 目的

- 群知性設計に先立ち、見た目と効果が結びついた属性行動を個体単位で導入する。

## 現状（git参照時点）

- 属性リスト構想のみ。
- 実装・評価式・行動接続は未着手。

## 作業区分

### 設計資料の校正

1. 属性体系と相性の確定
- 火/氷/雷/風/空間の効果、相性、発生形式を確定する。
- 対象ファイル:
  - `memo/設定/設定5：魔法：属性の考察.txt`
  - `memo/設定/設定5：魔法一覧.txt`
  - `memo/設定/設定5：被弾による属性覚醒システム.txt`

2. 魔法ゲノム項目の整理
- 魔法適性を遺伝項目へ接続する。
- 対象ファイル:
  - `memo/設定/設定5：ゲノム構造（魔法拡張）.txt`
  - `memo/設定/設定共通：ゲノム構造設計.txt`

### 本実装

1. 属性行動の導入
- 発動条件、クールダウン、行動分岐を実装する。
- 対象ファイル:
  - `Assets/script/Ingame/Creatures/Predator/predatorBehaviour.cs`
  - `Assets/script/Ingame/Combat/PredatorCombatLibrary.cs`
  - `Assets/script/Ingame/AI/AnimalAICommon.cs`

2. 環境・UI連動
- 属性発動と場データ、可視化を接続する。
- 対象ファイル:
  - `Assets/script/Ingame/Environment/ResourceDispenser.cs`
  - `Assets/script/Ingame/UI/Menu/ingame/UImanager/WorldUIManager.StateView.cs`
  - `Assets/script/Ingame/Presentation/CombatVisuals/CommonAttackVisualUIManager.cs`

## 完了条件

- 属性ごとの差が戦闘結果または行動差として観測できる。
- 属性効果の重ね掛けルールが定義済み。

## 参照

- `../0.遺伝子設計変更/現状の遺伝子構造まとめ.txt`
- `../../設定/設定5：魔法：属性の考察.txt`
- `../../設定/設定5：被弾による属性覚醒システム.txt`
- `../../設定/設定5：ゲノム構造（魔法拡張）.txt`
