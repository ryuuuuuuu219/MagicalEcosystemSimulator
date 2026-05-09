# 5.支配種仕様・役割定義

## 仕様目的

`dominant` を支配種として扱うための全体設定を固定する。

ここには、到達条件、追加付与 organ、支配種の仕様、役割定義、表示、ログ、課題ステージ勝利条件との接続点をまとめる。

## スコープ

- 支配種の定義。
- dominant 到達条件。
- dominant 化した個体へ追加付与する organ。
- 支配種固有の行動仕様。
- 支配種の生態系上の役割定義。
- 支配種の UI 表示と観測指標。
- 支配種到達ログ。
- 課題ステージ側の勝利条件との接続点。

## スコープ外

- genome の項目設計、`ValueGene`、`AIComponentGene` の保存場所は `1.支配種までの遺伝子定義` に置く。
- `predator` -> `highpredator` -> `dominant` の phase up 発生条件と実行処理は `4.相の進化` に置く。

## 現状

- `category.dominant` は enum に存在する。
- phase up により `highpredator` から `dominant` へ進む可能性がある。
- `WorldUIManager.ObjectList` と `WorldUIManager.PhasePopulation` は dominant を表示できる。
- dominant になった後の追加 organ、固有行動、役割、勝利条件はまだ薄い。

## 仕様

- 支配種は phase 進化の最終段階 `dominant` になった後、支配種仕様を付与される対象として扱う。
- 到達条件は、phase、organ、mana、個体数、維持時間、制圧状況などを組み合わせて定義する。
- 追加付与 organ は `AIComponentSet` と `ValueGene` で表現し、詳細は `支配種までの最小遺伝仕様.md` にまとめる。
- 支配種の役割は、単なる強個体ではなく、環境・同種・敵対勢力・課題ステージに影響する上位存在として定義する。
- ステージ勝利条件として使う場合は、課題ステージ controller 側で dominant 数・維持時間・敵対勢力制圧などを評価する。
- 支配種固有行動は、魔法使用・群知性・戦闘拡張の成果を参照する。
- `dominant` の表示は phase population と object list の両方で確認できる。

## 実装タスク

- dominant 到達条件を定義する。
- dominant 化時に追加付与する organ と preset 合成ルールを決める。
- dominant 到達後の最低限の行動差分を決める。
- 支配種の役割定義を、魔法・群知性・戦闘・課題ステージへ接続する。
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

- 支配種が何を満たした個体で、何の役割を持つか、README とコードから一致して読める。
- dominant 化時に追加付与する organ と値の扱いが決まっている。
- dominant 到達が UI / log / stage 評価で確認できる。
- 支配種仕様と相進化条件が混同されていない。

## 移植元

- `10.支配種到達条件`
- `X-5.やりたいこと_将来拡張`
