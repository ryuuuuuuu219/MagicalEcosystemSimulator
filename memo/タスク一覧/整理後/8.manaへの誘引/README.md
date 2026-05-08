# 8.manaへの誘引

## 仕様目的

mana field を個体の行動判断へ接続し、寄る、避ける、吸収する行動差として観測できるようにする。

## スコープ

- mana field の sample / consume / absorb。
- `ManaFieldSense` による場の検知。
- `ManaFieldAttractionDesire` による移動誘引。
- `FieldManaAbsorbAction` による場からの mana 回収。

## 現状

- `ManaFieldManager` は grid、sample、consume、debug 描画を持つ。
- 草食・捕食は旧 behaviour 内で field mana を吸収できる。
- organ 側に `ManaFieldSense` / `ManaFieldAttractionDesire` / `FieldManaAbsorbAction` がある。
- mana へ寄る行動は本流移動へまだ薄い。

## 仕様

- mana field は「資源」「危険場」「魔法源」のどれとして扱うかを phase / species ごとに分けられる。
- 草食は基本的に安全な mana へ寄り、過剰 hazard は避ける。
- 捕食・上位相は mana を phase up や魔法使用の燃料として評価する。
- 吸収処理は field の値を減らし、個体 `Resource.mana` を増やす。

## 実装タスク

- `ManaFieldAttractionDesire` を本流移動へ接続する。
- 旧 behaviour の field absorb と organ action の重複を整理する。
- mana hazard と mana attraction の閾値を分ける。
- phase ごとの mana 行動差を UI / debug で確認できるようにする。

## 対象スクリプト

- `Assets/script/Ingame/Environment/Fields/ManaFieldManager.cs`
- `Assets/script/Ingame/Environment/Resource.cs`
- `Assets/script/Ingame/Creatures/after/organ/Sense/ManaFieldSense.cs`
- `Assets/script/Ingame/Creatures/after/organ/Desire/ManaFieldAttractionDesire.cs`
- `Assets/script/Ingame/Creatures/after/organ/Action/FieldManaAbsorbAction.cs`
- `Assets/script/Ingame/Creatures/before/Herbivore/herbivoreBehaviour.cs`
- `Assets/script/Ingame/Creatures/before/Predator/predatorBehaviour.cs`

## 完了条件

- mana field の分布に応じて個体の移動方向が変わる。
- field mana 吸収で field 値と個体 mana が同期して変化する。
- mana attraction と mana hazard の判定が分離されている。

## 移植元

- `6.魔素基盤構築`
- `3.生態系コア強化`
- `0.phase0_organ設計`
