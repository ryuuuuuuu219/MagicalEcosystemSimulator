# 6.戦闘システム拡張

## 目的

攻撃判定、IFF、撤退、追跡放棄、待ち伏せを整理する。

## 現状

PredatorCombatLibrary に charge / bite / melee と mana cost がある。

## 残タスク

CreatureRelationResolver と speciesID の戦闘判定を完成させる。

## 対象スクリプト

- `Assets/script/Ingame/Combat/PredatorCombatLibrary.cs`
- `Assets/script/Ingame/Creatures/after/organ/Sense/CreatureRelationResolver.cs`
- `Assets/script/Ingame/Creatures/before/Predator/predatorBehaviour.cs`

## 移植元

- `5.戦闘システム拡張`
- `X-2`
- `X-3`

## 完了条件

- 目的に書いた責務が実行時に観測できる。
- 対象スクリプトと README の参照パスが一致している。
- 旧資料から必要な内容がこのフォルダの README へ移植済み。
