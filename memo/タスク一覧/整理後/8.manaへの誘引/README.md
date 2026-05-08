# 8.manaへの誘引

## 目的

mana field へ寄る、避ける、吸う行動を本編AIへ接続する。

## 現状

ManaFieldManager、ManaFieldSense、FieldManaAbsorbAction、ManaFieldAttractionDesire はある。

## 残タスク

旧 behaviour または AnimalBrain 本流へ接続し、行動差として見える状態にする。

## 対象スクリプト

- `Assets/script/Ingame/Environment/Fields/ManaFieldManager.cs`
- `Assets/script/Ingame/Creatures/after/organ/Sense/ManaFieldSense.cs`
- `Assets/script/Ingame/Creatures/after/organ/Desire/ManaFieldAttractionDesire.cs`
- `Assets/script/Ingame/Creatures/after/organ/Action/FieldManaAbsorbAction.cs`

## 移植元

- `6.魔素基盤構築`
- `3.生態系コア強化`
- `0.phase0_organ設計`

## 完了条件

- 目的に書いた責務が実行時に観測できる。
- 対象スクリプトと README の参照パスが一致している。
- 旧資料から必要な内容がこのフォルダの README へ移植済み。
