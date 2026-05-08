# 6.戦闘システム拡張

## 仕様目的

捕食者の通常戦闘を、攻撃判定、対象判定、mana cost、threat pulse、speciesID ベースの IFF まで含めて整理する。

## スコープ

- `PredatorCombatLibrary` の charge / bite / melee。
- `CreatureRelationResolver` と speciesID による同種・異種判定。
- 攻撃結果からの damage / mana drain / mana cost / threat pulse。
- 追跡対象の選別と攻撃許可。

## スコープ外

高度判断のうち、撤退、追跡放棄、待ち伏せ、戦術演出、陣形、詳細な判断ログは active task から外す。必要になったものだけ `保留・将来案/X-1.やりたいこと_戦術演出` または関連 X 系から昇格する。

## 現状

- `PredatorCombatLibrary` に charge / bite / melee と mana cost がある。
- 旧 `predatorBehaviour` で live prey への攻撃、damage 適用、mana 回収、攻撃 cost 消費を行う。
- `ThreatPulseEmitter` と `threatmap_calc.AddThreatPulse()` で存在・攻撃由来の脅威を場へ出せる。
- speciesID による上位相同士の同種判定は始まっている。

## 仕様

- 通常戦闘の正本は `PredatorCombatLibrary` に置く。
- 攻撃対象判定は category rank と speciesID を使い、同 species の上位捕食者は原則攻撃対象から外す。
- 攻撃成功時は target へ damage、attacker へ damage 分 mana 回収、攻撃ごとの mana cost 消費を行う。
- threat pulse は攻撃結果と存在感の両方から発生できる。

## 実装タスク

- 旧 behaviour と organ action の攻撃処理の重複を整理する。
- `CreatureRelationResolver` の判定を通常戦闘へ一貫して使う。
- IFF の仕様を README に固定する。
- threat pulse の強度・半径・発生タイミングを combat result と対応させる。

## 対象スクリプト

- `Assets/script/Ingame/Combat/PredatorCombatLibrary.cs`
- `Assets/script/Ingame/Creatures/before/Predator/predatorBehaviour.cs`
- `Assets/script/Ingame/Creatures/after/organ/Sense/CreatureRelationResolver.cs`
- `Assets/script/Ingame/Creatures/after/organ/Action/PredatorOrganCombatUtility.cs`
- `Assets/script/Ingame/Creatures/after/organ/Action/ThreatPulseEmitter.cs`

## 完了条件

- 通常戦闘の対象判定、damage、mana cost、mana drain が一貫している。
- speciesID による IFF が旧 behaviour と organ action の両方で同じ意味になる。
- 撤退・待ち伏せなどの高度判断が、このタスクの必須要件として混ざっていない。

## 移植元

- `5.戦闘システム拡張`
- `X-2.やりたいこと_判断ログ`
- `X-3.やりたいこと_陣形`
