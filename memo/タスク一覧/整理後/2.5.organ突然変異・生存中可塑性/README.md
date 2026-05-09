# 2.5.organ突然変異・生存中可塑性

## 仕様目的

- 世代更新時 mutation による organ gene の変化。
- 生存中の低確率 mutation による runtime organ 構成変化。
- organ の着脱、または安全な無効化。
- organ weight / level の増減。
- mutation 後の依存 organ 再解決。
- required organ の保護。
- organ 着脱時の `OrganFoundation` 状態 checkpoint 化。
- 生存中 mutation の遺伝候補化。
- 10 秒間隔の生存中 mutation 抽選。
- 痕跡器官 `isVestigialOrgan` と `VestigialOrgans` による organ 変化履歴保持。

## スコープ

- `AIComponentGene.enabled` の確率変化。
- `AIComponentGene.level` の増減。
- `AIComponentGene.mutationChanceT` による生存中 mutation 抽選。
- `AIComponentGene.mutationChanceG` による世代更新時 mutation 抽選。
- optional organ の追加候補化。
- optional organ の無効化候補化。
- 生存中 mutation と世代更新時 mutation の分離。
- mutation log の出力。
- `OrganRelationLibrary` による依存 organ の補完。
- `AnimalBrain.RefreshOrgans()` の呼び出しタイミング。
- `OrganFoundation` の checkpoint を世代更新時評価候補に含める。
- 相進化時と organ 変化時の snapshot 保存。
- `VestigialOrgans` による痕跡器官の記録。

## スコープ外

- phase / species ごとの固定 organ preset 定義。これは `0.phase0_organ設計` で扱う。
- 親から子への organ 構成継承。これは `2.organ構成の遺伝` で扱う。
- 世代更新全体の評価、選抜、交叉、再配置。これは `3.世代更新と遺伝` で扱う。
- organ の戦闘・魔法・mana 行動そのものの詳細仕様。

## 現状

- `AIComponentGene` / `AIComponentSet` は存在する。
- `OrganFoundation` は organ 導入入口と `AnimalBrain` runner を持つ。
- `OrganRelationLibrary` は依存 organ のホワイトリストを持つ。
- `OrganPresetLibrary` はまだ固定 Ensure に近い。
- mutation による `AIComponentGene.enabled` / `level` 変化は未実装。
- 生存中 mutation と世代更新時 mutation のログ分離は未実装。
- organ 着脱時の checkpoint 保存は未実装。
- 生存中 mutation を次世代へ遺伝候補として渡す経路は未実装。
- 10 秒間隔 mutation counter は未実装。
- `mutationChanceT` / `mutationChanceG` は未実装。
- `isVestigialOrgan` と `VestigialOrgans` は未実装。

## 仕様

- required organ は mutation の着脱対象にしない。
- optional organ は `enabled` と `level` を mutation 対象にする。
- 生存中 mutation は `OrganFoundation` の counter で 10 秒間隔に抽選する。
- 抽選率は `AIComponentGene.mutationChanceT` を使う。
- 世代更新時 mutation は `AIComponentGene.mutationChanceG` を使う。
- 生存中 mutation は runtime 状態だけで終わらせず、`OrganFoundation` の checkpoint として保存する。
- checkpoint は世代更新時に評価候補へ含める。
- 世代更新時 mutation は genome / `AIComponentSet` に保存し、次世代へ遺伝可能にする。
- 生存中 mutation で獲得した魔法資質や organ 構成も、評価を通過した場合は遺伝候補に含める。
- checkpoint は「最後の状態」だけでなく、相進化時と organ 変化時に snapshot として残す。
- 魔法資質は magic gene として分離せず、organ gene に含める。
- 生存中 mutation 後は `OrganRelationLibrary` で依存 organ を再解決する。
- organ の物理削除は初期実装では避け、`enabled=false`、`gene.enabled=false`、または `weight == 0` 相当の早期 return を優先する。
- mutation で失われた organ は即時削除せず、`isVestigialOrgan` を true にして痕跡器官として保持する。
- `OrganFoundation` は `public List<string> VestigialOrgans = new();` を持ち、痕跡器官 ID を記録する。
- organ 実装側は、無効化状態を読んだ場合に副作用を出さず早期 return できる形にする。
- Add / Remove / enable 状態変更後は `AnimalBrain.RefreshOrgans()` を呼ぶ。
- mutation は「何が」「なぜ」「いつ」変化したかをログに残す。
- `AIComponentGene.isVitalOrgan` が true の organ は mutation による着脱対象から除外する。
- `AnimalBrain`、`OrganFoundation`、`GroundMotor`、`AIMemoryStore`、`CreatureMotorBootstrap`、`CreatureRelationResolver` は vital organ として扱う。
- 依存 organ の削除は、親 organ を無効化しただけで機械的に依存先を全削除しない。
- 依存 organ は他の有効 organ から参照されていない場合にだけ無効化候補にする。

## 実装タスク

- `AIComponentGene` に required / installChance / mutationChance / minLevel / maxLevel などの必要項目を追加するか決める。
- `AIComponentGene` に `isVitalOrgan` を追加する。
- `AIComponentGene` に `isVestigialOrgan` を追加する。
- `AIComponentGene` に `mutationChanceT` / `mutationChanceG` を追加する。
- `AIComponentGene` に weight / level 0 の無効化判定を入れるか決める。
- required / optional organ の分類表を作る。
- `OrganFoundation` に `InstallComponentSet()` と `ApplyMutationResult()` の入口を作る。
- `OrganFoundation` に 10 秒間隔の mutation counter を追加する。
- `OrganFoundation` に `VestigialOrgans` を追加する。
- `OrganFoundation` に organ 構成 checkpoint を保存する仕組みを作る。
- 相進化時と organ 変化時に snapshot を保存する。
- checkpoint を世代更新時の評価候補へ渡す接続点を作る。
- `AIComponentSet` に gene の追加、更新、enabled 判定、level clamp を追加する。
- 生存中 mutation runner をどこで呼ぶか決める。
- 世代更新時 mutation は `AdvanceGenerationController` から呼べる形にする。
- mutation 後に `OrganRelationLibrary` を通して依存 organ を補完する。
- parent organ 無効化後に、不要になった依存 organ だけを無効化候補にする reverse dependency 判定を作る。
- mutation log と generation log の出力形式を分ける。

## 対象スクリプト

- `Assets/script/Ingame/Creatures/after/organ/Core/AIComponentGene.cs`
- `Assets/script/Ingame/Creatures/after/organ/Core/AIComponentSet.cs`
- `Assets/script/Ingame/Creatures/after/organ/Core/OrganFoundation.cs`
- `Assets/script/Ingame/Creatures/after/organ/Core/OrganRelationLibrary.cs`
- `Assets/script/Ingame/Creatures/after/organ/Core/OrganPresetLibrary.cs`
- `Assets/script/Ingame/Creatures/after/organ/Core/AnimalBrain.cs`
- `Assets/script/Ingame/UI/Menu/ingame/AdvanceGenerationController.cs`
- `Assets/script/Ingame/Genome/GenerationLog.cs`

## 完了条件

- required organ が mutation で外れない。
- optional organ の enabled / level が確率的に変化する。
- 生存中 mutation が 10 秒間隔で `mutationChanceT` に基づいて抽選される。
- 世代更新時 mutation が `mutationChanceG` に基づいて抽選される。
- weight / level 0 の organ が早期 return し、副作用を出さない。
- 生存中 mutation 後も `AnimalBrain` の organ キャッシュが更新される。
- mutation 後に依存 organ 欠落が起きない。
- 失われた organ が削除ではなく痕跡器官として保持される。
- `VestigialOrgans` から過去に失われた organ が追跡できる。
- parent organ 無効化時に、他 organ が使っている共有依存 organ は残る。
- 生存中 mutation の checkpoint が世代更新時評価候補に含まれる。
- 相進化時と organ 変化時の snapshot が残る。
- 生存中 mutation と世代更新時 mutation がログ上で区別できる。
- 生存中 mutation で獲得した organ / 魔法資質が、評価後に次世代へ遺伝可能である。

## 追加設計メモ

### OrganFoundation checkpoint

organ 着脱または weight / level 変更が起きたとき、その時点の `AIComponentSet` と導入済み organ 一覧を `OrganFoundation` 側で checkpoint として保持する。

checkpoint は生存中の一時状態ではなく、世代更新時に fitness 評価候補として参照する。これにより、生存中 mutation で魔法資質や有利な organ 構成を獲得した個体が、次世代へその構成を渡す機会を失わない。

checkpoint は相進化時と organ 変化時に snapshot として残す。評価時には、最後の状態だけでなく、相進化や organ 変化の瞬間に成立していた構成も候補に含められるようにする。

### 無効化方針

初期実装では component の即時削除を避ける。

- `AIComponentGene.enabled == false`
- `AIComponentGene.level <= 0`
- organ 側の weight が 0

上記のいずれかを無効状態として扱い、organ の `TickSense()` / `Evaluate()` / `TryAct()` / `Steer()` は副作用なしで早期 return する。

### mutation chance

`AIComponentGene` には mutation 抽選用の値を 2 種類持たせる。

- `mutationChanceT`: 生存中 mutation 用。`OrganFoundation` の 10 秒 counter で抽選する。
- `mutationChanceG`: 世代更新時 mutation 用。`AdvanceGenerationController` など世代更新側から抽選する。

この段階では、mutation chance はコンポーネントごとに持たせる。phase ごとの差分は後回しにし、X 系設計メモへ退避する。

### vital organ

`AIComponentGene.isVitalOrgan` を追加し、true の organ は mutation による着脱対象から除外する。

初期 vital organ:

- `OrganFoundation`
- `AnimalBrain`
- `AnimalAIInstaller`
- `AIMemoryStore`
- `GroundMotor`
- `CreatureMotorBootstrap`
- `CreatureRelationResolver`

### 痕跡器官

mutation で organ が失われた場合、component を即時削除せず、`AIComponentGene.isVestigialOrgan` を true にして痕跡器官として保持する。

`OrganFoundation` は以下の一覧を持つ。

```csharp
public List<string> VestigialOrgans = new();
```

`VestigialOrgans` には痕跡器官化した organ の componentId を記録する。これにより、過去の organ 変化、退化、再獲得候補を追跡できる。

### 依存 organ の削除方針

`OrganRelationLibrary.DependencyWhitelist` の item1 を無効化した場合、item2 を即時にすべて消すのは危険。

同じ依存 organ を別の有効 organ が使っている可能性があるため、削除または無効化候補にする前に reverse dependency を確認する。

例:

- `MagicAttackAction` を無効化する。
- `MagicProjectileAttackAction` と `MagicCooldownState` は依存候補。
- ただし他の有効 organ が `MagicCooldownState` を使っている場合、`MagicCooldownState` は残す。

このため、依存先の無効化は「現在有効な parent organ のどれからも参照されなくなった場合」に限定する。

### 遺伝方針

生存中 mutation で獲得した organ / 魔法資質も、すべて遺伝候補に含める。

ただし無条件で genome に確定保存するのではなく、checkpoint として保持し、世代更新時の評価・選抜を通った場合に次世代 `AIComponentSet` へ反映する。

魔法資質は magic gene として分離せず、organ gene に含める。魔法 organ の enabled / level / mutation chance が魔法資質の発現と継承に関与する。

## 移植元

- `0.phase0_organ設計`
- `2.organ構成の遺伝`
- `3.世代更新と遺伝`
- `organ依存関係.md`
- `コンポーネント導入確率表.md`

## 実装メモ

### 2026-05-09

- `AIComponentGene` に `isVitalOrgan` / `isVestigialOrgan` / `weight` / `mutationChanceT` / `mutationChanceG` を追加。
- `AIComponentSet` に runtime mutation と generation mutation copy の入口を追加。
- `OrganFoundation` に 10 秒 mutation counter、checkpoint、mutation event、`VestigialOrgans` を追加。
- `OrganFoundation` に手動 checkpoint / 手動 mutation tick の ContextMenu を追加。
- `OrganPresetLibrary` を `AIComponentSet` preset 生成へ移行。
- `AnimalBrain` で inactive / vestigial organ を中央スキップ。
- `OrganRelationLibrary` に componentId 解決と reverse dependency 判定を追加。
- `herbivoreManager` / `predatorManager` に `nextGenerationComponentSet` を追加。
- `AdvanceGenerationController` から checkpoint を評価候補に含め、世代更新時 mutation を通した organ set を次世代へ渡す。
- `GenerationLog` に organ checkpoint / active organ / vestigial organ / generation mutation summary を追加。

残り:

- 実機で runtime mutation、痕跡器官化、次世代継承を確認する。
- checkpoint ごとの独立 fitness 軸を必要に応じて追加する。
- genome / DNA への organ gene 永続化は `2.organ構成の遺伝` 側で扱う。
