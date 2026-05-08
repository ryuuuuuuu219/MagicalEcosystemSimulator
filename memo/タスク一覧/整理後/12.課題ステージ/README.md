# 12.課題ステージ

## 目的

クリア条件、敵対勢力、報酬、勝敗判定を controller として実装する。

## 現状

UI入口と観測基盤はあるが、勝敗・報酬ロジックは薄い。

## 残タスク

stage UI と generation log に結果を出す。

## 対象スクリプト

- `Assets/script/Ingame/UI/Menu/ingame/UImanager/WorldUIManager.MenuFlow.cs`
- `Assets/script/Ingame/Genome/GenerationLog.cs`
- `Assets/script/Ingame/Diagnostics/PerformanceBudgetMonitor.cs`

## 移植元

- `11.課題ステージ`

## 完了条件

- 目的に書いた責務が実行時に観測できる。
- 対象スクリプトと README の参照パスが一致している。
- 旧資料から必要な内容がこのフォルダの README へ移植済み。
