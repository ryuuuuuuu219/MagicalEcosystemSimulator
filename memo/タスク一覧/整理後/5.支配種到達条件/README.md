# 5.支配種到達条件

## 目的

dominant の到達条件、表示、行動差分、勝利条件との関係を固定する。

## 現状

category.dominant と UI 表示はあるが、支配条件は弱い。

## 残タスク

支配種を相進化の最終段階、ステージ勝利条件、上位行動差分のどれとして扱うか決める。

## 対象スクリプト

- `Assets/script/Ingame/UI/Menu/ingame/UImanager/WorldUIManager.ObjectList.cs`
- `Assets/script/Ingame/UI/Menu/ingame/UImanager/WorldUIManager.PhasePopulation.cs`
- `Assets/script/Ingame/Creatures/before/Predator/predatorBehaviour.cs`

## 移植元

- `10.支配種到達条件`
- `X-5`

## 完了条件

- 目的に書いた責務が実行時に観測できる。
- 対象スクリプトと README の参照パスが一致している。
- 旧資料から必要な内容がこのフォルダの README へ移植済み。
