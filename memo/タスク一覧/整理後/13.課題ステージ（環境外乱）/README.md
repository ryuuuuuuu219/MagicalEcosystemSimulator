# 13.課題ステージ（環境外乱）

## 目的

外乱パラメータを期間イベントとして適用する。

## 現状

Disturbance UI はあるが、外乱適用 controller は薄い。

## 残タスク

死亡率、復帰率、phase population などの観測指標を記録する。

## 対象スクリプト

- `Assets/Scenes/Ingame.unity`
- `Assets/script/Ingame/UI/Menu/ingame/UImanager/WorldUIManager.ButtonBindings.cs`
- `Assets/script/Ingame/UI/Menu/ingame/UImanager/WorldUIManager.PhasePopulation.cs`

## 移植元

- `12.課題ステージ（環境外乱）`

## 完了条件

- 目的に書いた責務が実行時に観測できる。
- 対象スクリプトと README の参照パスが一致している。
- 旧資料から必要な内容がこのフォルダの README へ移植済み。
