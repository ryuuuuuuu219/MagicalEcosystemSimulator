# 5.支配種到達条件

## 仕様目的

`dominant` を支配種として扱う条件、表示、上位行動差分、課題ステージ勝利条件との関係を固定する。

## スコープ

- 支配種の定義。
- dominant 到達条件。
- 支配種の UI 表示と観測指標。
- 課題ステージ側の勝利条件との接続点。

## 現状

- `category.dominant` は enum に存在する。
- phase up により `highpredator` から `dominant` へ進む可能性がある。
- `WorldUIManager.ObjectList` と `WorldUIManager.PhasePopulation` は dominant を表示できる。
- dominant になった後の固有行動・勝利条件はまだ薄い。

## 仕様

- 支配種は phase 進化の最終段階 `dominant` として扱う。
- ステージ勝利条件として使う場合は、課題ステージ controller 側で dominant 数・維持時間・敵対勢力制圧などを評価する。
- 支配種固有行動は、魔法使用・群知性・戦闘拡張の成果を参照する。
- `dominant` の表示は phase population と object list の両方で確認できる。

## 実装タスク

- dominant 到達後の最低限の行動差分を決める。
- 課題ステージの勝利条件が参照する dominant 指標を定義する。
- 支配種到達ログと UI 表示を整理する。
- phase 仕様との重複を避け、到達前工程は `4.相の進化` に置く。

## 対象スクリプト

- `Assets/script/Shared/Enums/SimulationEnums.cs`
- `Assets/script/Ingame/Creatures/before/Predator/predatorBehaviour.cs`
- `Assets/script/Ingame/Creatures/after/organ/Action/PredatorPhaseEvolutionAction.cs`
- `Assets/script/Ingame/UI/Menu/ingame/UImanager/WorldUIManager.ObjectList.cs`
- `Assets/script/Ingame/UI/Menu/ingame/UImanager/WorldUIManager.PhasePopulation.cs`

## 完了条件

- 支配種が何を満たした個体か、README とコードから一致して読める。
- dominant 到達が UI / log / stage 評価で確認できる。
- 支配種到達条件と相進化条件が混同されていない。

## 移植元

- `10.支配種到達条件`
- `X-5.やりたいこと_将来拡張`
