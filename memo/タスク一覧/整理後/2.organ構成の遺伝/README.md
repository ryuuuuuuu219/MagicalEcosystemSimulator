# 2.organ構成の遺伝

## 目的

個体が持つ organ 構成を親から子へ継承できる形にし、固定構成だけではなく個体差として扱えるようにする。

## 現状

- `AIComponentGene` / `AIComponentSet` は実装済み。
- `AIComponentGene` は `enabled`、`level`、`weight`、`installChance`、`mutationChanceT`、`mutationChanceG`、`isVitalOrgan`、`isVestigialOrgan` を持つ。
- `AIComponentSet` は gene の登録、更新、preset 適用、runtime mutation、generation mutation copy、vestigial 化、clone を持つ。
- `OrganPresetLibrary` は species / phase ごとの preset を `AIComponentSet` として生成する。
- `herbivoreManager` / `predatorManager` は `nextGenerationComponentSet` を保持し、spawn 時に `OrganFoundation.InstallComponentSet()` へ渡す。
- `AdvanceGenerationController` は最良 checkpoint から次世代 component set を作り、世代更新時 mutation を通して manager へ渡す。
- ただし、永続的な DNA / genome 表現への保存形式はまだ未確定。

## スコープ

- `AIComponentGene` と `AIComponentSet` の表現仕様。
- preset と親由来 component set の合成。
- required / optional / vital / vestigial の分類。
- manager 経由の次世代 spawn 反映。

## 実装済み

- required organ は `isVitalOrgan` と preset で保護する。
- optional organ は `enabled` / `level` / `weight` / mutation chance を持つ。
- 依存 organ は `OrganRelationLibrary.EnsureDependencies()` で補完する。
- mutation なしでも、checkpoint 由来の organ set を次世代へ渡す経路ができた。
- 生存中 mutation で変化した organ set も、checkpoint 評価を通過すれば次世代候補に含まれる。

## 残り

- `AIComponentSet` を DNA / genome serializer へ保存する形式を決める。
- crossover で複数親の `AIComponentGene` をどう混ぜるか定義する。
- UI / log 上で organ gene の差分を見やすくする。
- `level` / `weight` の意味を全 organ 共通にするか、organ ごとの解釈にするかを実機結果で決める。

## 対象スクリプト

- `Assets/script/Ingame/Creatures/after/organ/Core/AIComponentGene.cs`
- `Assets/script/Ingame/Creatures/after/organ/Core/AIComponentSet.cs`
- `Assets/script/Ingame/Creatures/after/organ/Core/OrganFoundation.cs`
- `Assets/script/Ingame/Creatures/after/organ/Core/OrganPresetLibrary.cs`
- `Assets/script/Ingame/Creatures/after/organ/Core/OrganRelationLibrary.cs`
- `Assets/script/Ingame/Creatures/before/Herbivore/herbivoreManager.cs`
- `Assets/script/Ingame/Creatures/before/Predator/predatorManager.cs`
- `Assets/script/Ingame/UI/Menu/ingame/AdvanceGenerationController.cs`

## 完了条件

- organ 構成が次世代個体へ再現可能に渡る。
- required organ と個体差 organ の導入結果を再現できる。
- mutation 後の organ 差分が実行時の AI 構成へ反映される。
- DNA / genome 表現に保存しても、spawn 時に同じ `AIComponentSet` を復元できる。
