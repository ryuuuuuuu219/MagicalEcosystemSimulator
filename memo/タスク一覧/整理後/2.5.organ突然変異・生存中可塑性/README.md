# 2.5.organ突然変異・生存中可塑性

## 目的

organ 構成の変化を `2.organ構成の遺伝` と `3.世代更新と遺伝` から分離し、生存中 mutation と世代更新時 mutation を安全に扱う。

## 決定済み方針

- 生存中 mutation は `OrganFoundation` の 10 秒 counter と `AIComponentGene.mutationChanceT` で抽選する。
- 世代更新時 mutation は `AIComponentGene.mutationChanceG` で抽選する。
- mutation chance は component ごとに持たせる。phase ごとの差分は X 系設計メモへ退避する。
- 生存中 mutation で獲得した organ / 魔法資質も、評価を通過した場合は次世代へ遺伝可能にする。
- 魔法資質は magic gene として分離せず、organ gene に含める。
- component の即時 Remove は初期実装では避ける。失われた organ は `isVestigialOrgan` と `VestigialOrgans` で痕跡器官として保持する。
- vital organ は mutation による着脱対象から除外する。
- mutation 後は `OrganRelationLibrary` で依存 organ を再解決する。

## 現状

- `AIComponentGene` に `isVitalOrgan`、`isVestigialOrgan`、`weight`、`installChance`、`mutationChanceT`、`mutationChanceG`、`minLevel`、`maxLevel` が入った。
- `AIComponentSet` に runtime mutation、generation mutation copy、vestigial 化、gene clone が入った。
- `OrganFoundation` に `enableRuntimeMutation`、`mutationInterval = 10f`、`runtimeMutationChanceScale`、checkpoint、mutation event、`VestigialOrgans` が入った。
- `OrganFoundation` は phase 変化時と organ 変化時に checkpoint を保存する。
- `OrganFoundation` には手動確認用の `Organ/Record Checkpoint` と `Organ/Force Runtime Mutation Tick` がある。
- `AnimalBrain` は inactive / vestigial gene を中央でスキップする。
- `AdvanceGenerationController` は最良 checkpoint を評価候補に含め、世代更新時 mutation 後の component set を次世代 manager へ渡す。
- `GenerationLog` に organ checkpoint / active organ / vestigial organ / generation mutation summary が追加された。

## 生存中 mutation の扱い

生存中 mutation は一代限りの runtime state で終わらせない。変化が起きた瞬間の `OrganFoundation` 状態を checkpoint として保存し、世代更新時の評価候補に含める。

checkpoint は以下を残す。

- active organ 一覧。
- vestigial organ 一覧。
- `AIComponentSet` の snapshot。
- mutation event の summary。
- checkpoint reason。

## 痕跡器官

organ が失われた場合、初期実装では component を `Destroy()` しない。

- `AIComponentGene.enabled == false`
- `AIComponentGene.isVestigialOrgan == true`
- `AIComponentGene.level <= 0`
- `AIComponentGene.weight <= 0`

上記のような状態を inactive とみなし、`AnimalBrain` 側で早期 return する。

## 依存 organ

mutation 後は `OrganRelationLibrary.EnsureDependencies()` を通して不足 organ を補う。

親 organ を無効化した場合、依存 organ を即時削除するのではなく、他の active organ から参照されていない依存 organ を `GetUnusedDependencyIdsAfterDisable()` で判定し、痕跡器官化の候補にする。

## 実装済み

- required / vital organ が mutation で外れない。
- optional organ の enabled / level / weight が確率的に変化する。
- 生存中 mutation が 10 秒間隔で抽選される。
- mutation 後に checkpoint が残る。
- 痕跡器官が overlay / log から追える。
- 生存中 mutation checkpoint が世代更新時の評価候補に含まれる。
- 世代更新時 mutation が `mutationChanceG` に基づいて次世代 component set に反映される。
- 生存中 mutation と世代更新時 mutation が event / log 上で区別できる。

## 残り

- 実機で runtime mutation、痕跡器官化、次世代継承を確認する。
- mutation 発生頻度と `runtimeMutationChanceScale` を調整する。
- `level` / `weight` の意味を全 organ 共通にするか、organ ごとに読むか決める。
- checkpoint score を「最後の状態」寄りにするか「最良状態」寄りにするか、実機ログで調整する。
- component を本当に外す段階をいつ解禁するか決める。

## 対象スクリプト

- `Assets/script/Ingame/Creatures/after/organ/Core/AIComponentGene.cs`
- `Assets/script/Ingame/Creatures/after/organ/Core/AIComponentSet.cs`
- `Assets/script/Ingame/Creatures/after/organ/Core/OrganFoundation.cs`
- `Assets/script/Ingame/Creatures/after/organ/Core/OrganRelationLibrary.cs`
- `Assets/script/Ingame/Creatures/after/organ/Core/OrganPresetLibrary.cs`
- `Assets/script/Ingame/UI/Menu/ingame/AdvanceGenerationController.cs`
- `Assets/script/Ingame/Genome/GenerationLog.cs`

## 完了条件

- required organ が mutation で外れない。
- mutation 後も依存 organ 欠落が起きない。
- 生存中 mutation の checkpoint が世代更新時評価候補に含まれる。
- 生存中 mutation で獲得した organ / 魔法資質が、評価後に次世代へ遺伝可能である。
- 生存中 mutation と世代更新時 mutation がログ上で区別できる。
- 実機で mutation 後の個体が動作破綻しない。
