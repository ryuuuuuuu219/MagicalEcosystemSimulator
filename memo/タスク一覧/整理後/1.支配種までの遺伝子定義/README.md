# 1.支配種までの遺伝子定義

## 目的

草食、捕食、上位捕食、支配種までの genome 項目を定義する。

## 現状

HerbivoreGenome / PredatorGenome / WaveGene は残っている。

## 残タスク

phase / species / dominant までを遺伝子定義上どう扱うか決める。

## 対象スクリプト

- `Assets/script/Ingame/Creatures/before/Herbivore/HerbivoreGenome.cs`
- `Assets/script/Ingame/Creatures/before/Predator/PredatorGenome.cs`
- `Assets/script/Shared/Enums/SimulationEnums.cs`

## 移植元

- `1.遺伝子設計変更`
- `10.支配種到達条件`
- `X-4`

## 完了条件

- 目的に書いた責務が実行時に観測できる。
- 対象スクリプトと README の参照パスが一致している。
- 旧資料から必要な内容がこのフォルダの README へ移植済み。
