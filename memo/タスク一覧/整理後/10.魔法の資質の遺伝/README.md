# 10.魔法の資質の遺伝

## 目的

魔法適性、属性傾向、発動条件、クールダウン、mana cost を genome へ接続する。

## 現状

魔法実装と世代更新はあるが、魔法資質 gene は未統合。

## 残タスク

phase / dominant / species と魔法資質の関係を整理し、継承・変異させる。

## 対象スクリプト

- `Assets/script/Ingame/Magic`
- `Assets/script/Ingame/UI/Menu/ingame/AdvanceGenerationController.cs`
- `Assets/script/Ingame/Creatures/before/Predator/PredatorGenome.cs`

## 移植元

- `7.属性魔法拡張`
- `1.遺伝子設計変更`
- `9.学習と進化`
- `X-4`

## 完了条件

- 目的に書いた責務が実行時に観測できる。
- 対象スクリプトと README の参照パスが一致している。
- 旧資料から必要な内容がこのフォルダの README へ移植済み。
