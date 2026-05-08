# 7.魔法のダメージ判定システム

## 目的

MagicProjectile の damage、mana回収、field反映を戦闘ダメージ系として整理する。

## 現状

MagicProjectile は属性ごとの着弾、damage、mana回収、field反映を持つ。

## 残タスク

field2AI の damage / hazard 計算を魔法ダメージ判定に接続する。

## 対象スクリプト

- `Assets/script/Ingame/Magic/MagicProjectile.cs`
- `Assets/script/Ingame/Magic/ImpactEffects/Magic2FieldManager.cs`
- `Assets/script/Ingame/AI/Fields/field2AI.cs`

## 移植元

- `3.生態系コア強化`
- `7.属性魔法拡張`
- `X-7`

## 完了条件

- 目的に書いた責務が実行時に観測できる。
- 対象スクリプトと README の参照パスが一致している。
- 旧資料から必要な内容がこのフォルダの README へ移植済み。
