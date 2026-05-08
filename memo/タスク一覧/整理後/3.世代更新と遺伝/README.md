# 3.世代更新と遺伝

## 仕様目的

世代更新時の評価、選抜、交叉、突然変異、DNA 入出力、世代ログを統一し、将来の phase / magic / organ 拡張を受けられる形にする。

## スコープ

- 世代更新 controller。
- genome 評価軸。
- DNA 表示・注入。
- 世代ログと検証用ログ。

## 現状

- `AdvanceGenerationController` に世代更新、評価、crossover、mutation がある。
- `GenerationLog` / `GenomeLogger` がある。
- 評価軸は mana / health / random が中心。
- 草食と捕食の genome 形式差が残っている。

## 仕様

- 世代更新は「収集」「評価」「選抜」「交叉」「突然変異」「再配置」「ログ記録」の順で実行する。
- 評価値は mana / health を基礎にし、phase / magic / organ は拡張項目として追加できるようにする。
- DNA 入出力は草食・捕食で形式差を明示し、統一前でも誤読しないようにする。
- 学習や経験値は、このタスクでは「世代更新に入れる評価入力候補」として扱う。

## 実装タスク

- 草食・捕食 genome の評価入力を分けて整理する。
- phase / dominant / magic 資質を評価軸へ入れる条件を定義する。
- 世代ログに phase population と magic / organ 指標を追加できる形にする。
- DNA 注入時の validation と失敗時表示を整える。

## 対象スクリプト

- `Assets/script/Ingame/UI/Menu/ingame/AdvanceGenerationController.cs`
- `Assets/script/Ingame/Genome/GenerationLog.cs`
- `Assets/script/Ingame/Diagnostics/GenomeLogger.cs`
- `Assets/script/Ingame/Genome/GenomeSerializer.cs`

## 完了条件

- 世代更新の入出力と評価軸が README から追える。
- 草食・捕食の genome 形式差による表示・注入事故が起きない。
- phase / magic / organ の追加評価が既存更新処理を壊さずに追加できる。

## 移植元

- `9.学習と進化`
- `1.遺伝子設計変更`
