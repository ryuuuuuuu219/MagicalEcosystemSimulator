# 10.魔法の資質の遺伝

## 仕様目的

魔法適性、属性傾向、発動条件、cooldown、mana cost を genome と世代更新へ接続し、魔法を使う個体差を継承・変異できるようにする。

## スコープ

- 魔法属性 gene。
- 魔法発動適性。
- mana cost / cooldown / damage scale の遺伝。
- phase / dominant / species と魔法資質の関係。

## 現状

- `MagicElement` は Fire / Ice / Lightning / Wind / Space を持つ。
- 魔法発射と damage 基盤は存在する。
- `PredatorGenome` には通常戦闘系の値があるが、魔法資質 gene はまだ薄い。
- 世代更新の評価軸に魔法使用実績は入っていない。

## 仕様

- 魔法資質は「使用可能属性」「発動確率」「mana cost 補正」「cooldown 補正」「damage 補正」で扱う。
- phase が上がるほど魔法資質が有効化されやすい。
- dominant は複数属性または上位属性を扱える候補にする。
- 魔法を撃つかどうかの行動判断は `9.魔法の使用`、資質の継承はこのタスクで扱う。

## 実装タスク

- `PredatorGenome` または新 genome 形式に magic gene を追加する。
- mutation / crossover で magic gene が変化するルールを決める。
- `MagicProjectileAttackAction` へ genome 由来の cost / cooldown / element を渡す。
- 世代ログに magic 使用実績または magic gene を出せるようにする。

## 対象スクリプト

- `Assets/script/Ingame/Magic/MagicElement.cs`
- `Assets/script/Ingame/Creatures/before/Predator/PredatorGenome.cs`
- `Assets/script/Ingame/Creatures/after/organ/MagicActionEvasion/MagicProjectileAttackAction.cs`
- `Assets/script/Ingame/UI/Menu/ingame/AdvanceGenerationController.cs`
- `Assets/script/Ingame/Genome/GenerationLog.cs`

## 完了条件

- magic gene が genome / 世代更新 / action に接続されている。
- 個体差として属性・cost・cooldown の違いが観測できる。
- 継承・変異後に魔法資質の差が次世代へ反映される。

## 移植元

- `7.属性魔法拡張`
- `1.遺伝子設計変更`
- `9.学習と進化`
- `X-4.やりたいこと_遺伝子拡張候補`
