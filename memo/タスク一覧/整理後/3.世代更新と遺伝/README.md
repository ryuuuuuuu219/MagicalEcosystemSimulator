# 3.世代更新と遺伝

## 目的

評価、選抜、交叉、突然変異、DNA入出力を整理する。

## 現状

AdvanceGenerationController に crossover / mutation / DNA 表示・注入 / 世代ログがある。

## 残タスク

草食・捕食の genome 形式差と評価軸を整理する。

## 対象スクリプト

- `Assets/script/Ingame/UI/Menu/ingame/AdvanceGenerationController.cs`
- `Assets/script/Ingame/Genome/GenomeSerializer.cs`
- `Assets/script/Ingame/Genome/GenerationLog.cs`
- `Assets/script/Ingame/Diagnostics/GenomeLogger.cs`

## 移植元

- `9.学習と進化`
- `1.遺伝子設計変更`

## 完了条件

- 目的に書いた責務が実行時に観測できる。
- 対象スクリプトと README の参照パスが一致している。
- 旧資料から必要な内容がこのフォルダの README へ移植済み。
