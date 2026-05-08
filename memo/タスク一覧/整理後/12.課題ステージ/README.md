# 12.課題ステージ

## 仕様目的

シミュレーションにクリア条件、敵対勢力、報酬、勝敗判定を持つ課題ステージ controller を導入する。

## スコープ

- ステージ定義。
- クリア条件。
- 敵対勢力。
- 報酬。
- 勝敗判定。
- stage UI と generation log。

## 現状

- UI 側にステージ・外乱へつながる入口はある。
- `GenerationLog` と `PerformanceBudgetMonitor` に観測基盤がある。
- 専用の stage controller、勝敗判定、報酬付与ロジックはまだ見当たらない。

## 仕様

- 課題ステージは、開始条件、目標、制限、報酬、失敗条件を持つ。
- 勝敗判定は dominant 到達、敵対勢力制圧、一定世代生存などを条件候補にする。
- 報酬は genome / mana / spawn 権限 / UI unlock などから選ぶ。
- 環境外乱は `13.課題ステージ（環境外乱）` で扱い、このタスクは共通 stage controller を担当する。

## 実装タスク

- StageController の責務を定義する。
- クリア条件と失敗条件のデータ構造を決める。
- 敵対勢力 spawn / 管理 / 判定を整理する。
- 結果を UI と generation log に出す。

## 対象スクリプト

- `Assets/script/Ingame/UI/Menu/ingame/UImanager/WorldUIManager.MenuFlow.cs`
- `Assets/script/Ingame/UI/Menu/ingame/UImanager/WorldUIManager.PhasePopulation.cs`
- `Assets/script/Ingame/Genome/GenerationLog.cs`
- `Assets/script/Ingame/Diagnostics/PerformanceBudgetMonitor.cs`
- `Assets/script/Ingame/Environment/ResourceDispenser.cs`

## 完了条件

- ステージ開始、進行、勝敗判定、報酬付与の流れが実行時に確認できる。
- 結果が UI と generation log に残る。
- 環境外乱なしの課題ステージが単独で成立する。

## 移植元

- `11.課題ステージ`
