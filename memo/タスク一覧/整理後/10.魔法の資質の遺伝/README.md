# 10.魔法の資質の遺伝

## 目的

魔法属性、発動傾向、cooldown、mana cost、damage scale を世代更新へ接続し、魔法を使う個体差を継承・変異できるようにする。

## 現状

- `MagicElement` は Fire / Ice / Lightning / Wind / Space を持つ。
- 魔法発射と damage 基盤は存在する。
- 魔法 action organ は organ 構成の一部として扱える。
- 方針変更により、魔法資質は独立した magic gene ではなく organ gene に含める。
- 生存中 mutation で魔法 organ を獲得した場合も、checkpoint 評価を通れば次世代へ遺伝候補として渡せる。
- ただし、mana cost / cooldown / damage scale などの細かな魔法パラメータを organ gene の `level` / `weight` で読むか、別パラメータとして持つかは未確定。

## スコープ

- 魔法 organ の enabled / level / weight / mutation chance。
- phase / dominant / species と魔法資質の関係。
- mana cost / cooldown / damage scale の継承方針。
- 魔法使用実績の generation log 反映。

## 仕様

- 魔法資質は `AIComponentGene` の organ gene として扱う。
- 魔法 organ の例は `MagicProjectileAttackAction`、`MagicCooldownState`、魔法回避・防御系 action など。
- phase が上がるほど魔法 organ が有効化されやすい preset / mutation chance を検討する。
- dominant は複数属性または上位属性を扱える候補にする。
- 魔法を撃つかどうかの行動判断は `9.魔法の使用`、資質の継承はこのタスクで扱う。

## 残り

- 魔法 organ ごとの `level` / `weight` の意味を決める。
- `MagicProjectileAttackAction` へ organ gene 由来の cost / cooldown / element 補正を渡す。
- 世代ログに magic 使用実績または magic organ gene を出せるようにする。
- mutation / crossover 後に魔法資質の差が次世代へ反映されるか検証する。

## 対象スクリプト

- `Assets/script/Ingame/Magic/MagicElement.cs`
- `Assets/script/Ingame/Creatures/after/organ/Core/AIComponentGene.cs`
- `Assets/script/Ingame/Creatures/after/organ/Core/AIComponentSet.cs`
- `Assets/script/Ingame/Creatures/after/organ/Core/OrganFoundation.cs`
- `Assets/script/Ingame/Creatures/after/organ/MagicActionEvasion/MagicProjectileAttackAction.cs`
- `Assets/script/Ingame/UI/Menu/ingame/AdvanceGenerationController.cs`
- `Assets/script/Ingame/Genome/GenerationLog.cs`

## 完了条件

- 魔法資質が organ gene / 世代更新 / action に接続されている。
- 個体差として属性、cost、cooldown、damage の違いを観測できる。
- 継承・変異後に魔法資質の差が次世代へ反映される。
