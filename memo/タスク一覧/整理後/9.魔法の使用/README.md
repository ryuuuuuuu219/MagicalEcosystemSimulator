# 9.魔法の使用

## 目的

個体AIが魔法を使う条件、対象、クールダウン、mana cost を決める。

## 現状

MagicAttackAction / MagicProjectileAttackAction / MagicCooldownState はある。

## 残タスク

通常戦闘AIへ接続し、Ingame 上で魔法使用を観測可能にする。

## 対象スクリプト

- `Assets/script/Ingame/Creatures/after/organ/MagicActionEvasion`
- `Assets/script/Ingame/Magic/MagicLaunchApi.cs`
- `Assets/script/Ingame/Magic/MagicProjectile.cs`

## 移植元

- `7.属性魔法拡張`
- `0.phase0_organ設計`
- `5.戦闘システム拡張`

## 完了条件

- 目的に書いた責務が実行時に観測できる。
- 対象スクリプトと README の参照パスが一致している。
- 旧資料から必要な内容がこのフォルダの README へ移植済み。
