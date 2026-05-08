# 2.organ構成の遺伝

## 仕様目的

個体が持つ organ 構成を genome / DNA / 世代更新と接続し、固定構成だけでなく個体差として継承できるようにする。

## スコープ

- `AIComponentGene` と `AIComponentSet` の表現仕様。
- `OrganPresetLibrary` の固定配布と、遺伝による差分配布の境界。
- component 導入確率、依存 organ、自動導入ルール。

## 現状

- `AIComponentGene` / `AIComponentSet` は存在する。
- `OrganPresetLibrary` は phase ごとの固定 organ set を配布している。
- 世代更新、DNA 表現、mutation とはまだ接続されていない。

## 仕様

- phase ごとの必須 organ は固定導入する。
- 個体差として扱う organ は `AIComponentGene` で enabled / weight / priority を持つ。
- 依存 organ は installer が自動導入する。
- genome 由来の organ 差分は、既存 behaviour の互換動作を壊さない範囲から適用する。

## 実装タスク

- `AIComponentSet` を genome または DNA 表現に含める形式を決める。
- 必須 organ / 任意 organ / 将来 organ の分類表を作る。
- `OrganPresetLibrary` に固定構成と遺伝差分の合成手順を追加する。
- mutation 時に organ enabled / weight が変化するルールを定義する。

## 対象スクリプト

- `Assets/script/Ingame/Creatures/after/organ/Core/AIComponentGene.cs`
- `Assets/script/Ingame/Creatures/after/organ/Core/AIComponentSet.cs`
- `Assets/script/Ingame/Creatures/after/organ/Core/AnimalAIInstaller.cs`
- `Assets/script/Ingame/Creatures/after/organ/Core/OrganPresetLibrary.cs`
- `Assets/script/Ingame/UI/Menu/ingame/AdvanceGenerationController.cs`

## 完了条件

- organ 構成が genome / DNA / 世代更新のどこで保持されるか決まっている。
- 必須 organ と個体差 organ の導入結果を再現できる。
- mutation 後の organ 差分が実行時の AI 構成へ反映される。

## 移植元

- `1.遺伝子設計変更`
- `0.phase0_organ設計`
- `X-4.やりたいこと_遺伝子拡張候補`
