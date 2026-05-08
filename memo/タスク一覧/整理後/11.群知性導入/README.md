# 11.群知性導入

## 目的

GroupIntent、リーダー、共有ターゲット、役割分担を実装する。

## 現状

manager は個体リストを持つが、群れ専用ロジックは薄い。

## 残タスク

戦術演出や陣形は必要分だけ active task へ昇格する。

## 対象スクリプト

- `Assets/script/Ingame/Creatures/before/Herbivore/herbivoreManager.cs`
- `Assets/script/Ingame/Creatures/before/Predator/predatorManager.cs`
- `Assets/script/Ingame/AI/AnimalAICommon.cs`

## 移植元

- `8.群知性導入`
- `X-1`
- `X-3`
- `X-6`

## 完了条件

- 目的に書いた責務が実行時に観測できる。
- 対象スクリプトと README の参照パスが一致している。
- 旧資料から必要な内容がこのフォルダの README へ移植済み。
