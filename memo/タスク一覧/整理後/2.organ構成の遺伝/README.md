# 2.organ構成の遺伝

## 目的

organ の固定導入、自動導入、確率導入を genome と結びつける。

## 現状

AIComponentGene と OrganPresetLibrary はあるが、世代更新との接続は薄い。

## 残タスク

AIComponentGene を DNA 表現、世代更新、OrganPresetLibrary へ接続する。

## 対象スクリプト

- `Assets/script/Ingame/Creatures/after/organ/Core/AIComponentGene.cs`
- `Assets/script/Ingame/Creatures/after/organ/Core/OrganPresetLibrary.cs`
- `Assets/script/Ingame/UI/Menu/ingame/AdvanceGenerationController.cs`

## 移植元

- `1.遺伝子設計変更`
- `0.phase0_organ設計`
- `X-4`

## 完了条件

- 目的に書いた責務が実行時に観測できる。
- 対象スクリプトと README の参照パスが一致している。
- 旧資料から必要な内容がこのフォルダの README へ移植済み。
