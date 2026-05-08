# 7.魔法のダメージ判定システム

## 仕様目的

魔法による damage を「直撃ダメージ」と「属性場への作用」に分け、本編個体の HP / mana / 回避判断へ接続する。

## スコープ

- 魔法 projectile の直撃 damage。
- damage による caster の mana 回収。
- 属性ごとの field 反映。
- `field2AI` が計算する damage / hazard を個体行動へ渡す接続点。

## 分割仕様

### 7-A. 直撃ダメージ

- `MagicProjectile` が collider hit または範囲判定で対象を検出する。
- 対象が `herbivoreBehaviour` / `predatorBehaviour` を持つ場合、`TakeDamage()` を呼ぶ。
- damage 実績に応じて caster が mana を回収する。
- 直撃 damage は通常戦闘の damage と同じく HP 減少・死亡処理へつながる。

### 7-B. 属性場への作用

- `Magic2FieldManager` が Fire / Ice / Lightning / Wind / Space などの属性効果を field へ反映する。
- heat / mana / wind / threat の場は `field2AI` から damage / hazard として参照できる。
- 場の作用は即時 HP damage と、行動判断用 hazard を分けて扱う。
- heat / mana / wind / threat の値がどの閾値で damage または回避判断へ変換されるかを固定する。

## 現状

- `MagicProjectile` は属性ごとの着弾、対象 damage、mana 回収を持つ。
- `Magic2FieldManager` は属性を field へ反映できる。
- `field2AI` は field 由来の damage / hazard を計算できる。
- ただし、field 由来 damage / hazard は主移動・通常 AI 判断へまだ十分接続されていない。

## 実装タスク

- 直撃 damage と field damage の優先順位を決める。
- `field2AI` の結果を旧 behaviour または `AnimalBrain` の回避判断へ接続する。
- 属性ごとの damage / hazard 閾値を仕様化する。
- magic damage の mana 回収が通常攻撃の mana 回収と矛盾しないようにする。

## 対象スクリプト

- `Assets/script/Ingame/Magic/MagicProjectile.cs`
- `Assets/script/Ingame/Magic/ImpactEffects/Magic2FieldManager.cs`
- `Assets/script/Ingame/AI/Fields/field2AI.cs`
- `Assets/script/Ingame/Environment/Fields/HeatFieldManager.cs`
- `Assets/script/Ingame/Environment/Fields/ManaFieldManager.cs`
- `Assets/script/Ingame/Environment/Fields/WindFieldManager.cs`
- `Assets/script/Ingame/Environment/Fields/ThreatMap/threatmap_calc.cs`

## 完了条件

- 直撃魔法で対象 HP が減り、damage に応じた mana 回収が確認できる。
- 属性場が field manager に反映され、`field2AI` の damage / hazard として読める。
- 個体 AI が field hazard を回避または判断材料として使う。
- 直撃 damage と属性場作用が README 上で混同されていない。

## 移植元

- `3.生態系コア強化`
- `7.属性魔法拡張`
- `X-7.熱量場関連`
