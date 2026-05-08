# 9.魔法の使用

## 仕様目的

個体 AI が Ingame 上で魔法を選択・発動し、対象、mana cost、cooldown、通常戦闘との差分を観測できるようにする。

## スコープ

- 魔法攻撃 action。
- 発動条件。
- 対象選択。
- mana cost と cooldown。
- 通常戦闘 AI との優先順位。

## 現状

- `MagicAttackAction` / `MagicProjectileAttackAction` / `MagicCooldownState` がある。
- `MagicLaunchApi` と `MagicProjectile` で projectile を発射できる。
- 上位相には `OrganPresetLibrary` から魔法 organ が配布される。
- 実験場では確認できるが、本編通常 AI で自然に魔法選択される段階はまだ弱い。

## 仕様

- 魔法は原則 `highpredator` 以上の phase で使用可能にする。
- 発動には必要 mana と cooldown を満たす必要がある。
- 対象は通常戦闘の IFF と同じ対象判定を使う。
- 魔法攻撃と通常攻撃は同時に発火しない。
- 魔法の属性や適性の遺伝は `10.魔法の資質の遺伝` で扱う。

## 実装タスク

- 魔法使用の優先順位を通常 melee / bite / charge と比較して決める。
- `MagicProjectileAttackAction` の target 選択を通常戦闘の memory / relation と接続する。
- mana cost / cooldown が UI または debug log で追えるようにする。
- Ingame シーン上で個体が自律的に魔法を撃つ検証手順を作る。

## 対象スクリプト

- `Assets/script/Ingame/Creatures/after/organ/MagicActionEvasion/MagicAttackAction.cs`
- `Assets/script/Ingame/Creatures/after/organ/MagicActionEvasion/MagicProjectileAttackAction.cs`
- `Assets/script/Ingame/Creatures/after/organ/MagicActionEvasion/MagicCooldownState.cs`
- `Assets/script/Ingame/Magic/MagicLaunchApi.cs`
- `Assets/script/Ingame/Magic/MagicProjectile.cs`
- `Assets/script/Ingame/Creatures/after/organ/Core/OrganPresetLibrary.cs`

## 完了条件

- Ingame 上の個体が条件を満たしたときに自律的に魔法を使用する。
- magic cost / cooldown / target / damage が通常戦闘と矛盾しない。
- 魔法使用と魔法資質の遺伝が別仕様として分離されている。

## 移植元

- `7.属性魔法拡張`
- `0.phase0_organ設計`
- `5.戦闘システム拡張`
