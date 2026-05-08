# 11.群知性導入

## 仕様目的

個体単位の AI に加えて、群れとしての共有目標、リーダー、役割分担を導入する。

## スコープ

- GroupIntent。
- leader / follower。
- shared target。
- 群れ単位の追跡・逃避・集合。
- 必要最低限の役割分担。

## スコープ外

戦術演出、複雑な陣形、縄張り、判断ログの詳細表示は保留資料に残す。必要な要素だけ active task へ昇格する。

## 現状

- `herbivoreManager` / `predatorManager` は個体リストを持つ。
- manager 経由で草食・捕食の集団参照はできる。
- `GroupIntent`、leader 選出、共有 target、役割分担の専用クラスはまだ見当たらない。

## 仕様

- 群知性は個体 AI を上書きせず、個体の desire / action へ補正入力を渡す。
- shared target は個体 memory より長く保持できる。
- leader は固定個体、評価上位、最寄り個体などから選ぶ方式を選択可能にする。
- 群れ判断は `AnimalBrain` 本流または旧 behaviour への補正として接続する。

## 実装タスク

- GroupIntent のデータ構造を定義する。
- manager が共有 target / leader / group state を保持できるようにする。
- 個体側が group state を移動・攻撃・逃避判断に使う接続点を作る。
- 陣形や縄張りをこのタスクの必須要件に含めるか、保留のままにするかを決める。

## 対象スクリプト

- `Assets/script/Ingame/Creatures/before/Herbivore/herbivoreManager.cs`
- `Assets/script/Ingame/Creatures/before/Predator/predatorManager.cs`
- `Assets/script/Ingame/AI/AnimalAICommon.cs`
- `Assets/script/Ingame/Creatures/after/organ/Core/AnimalBrain.cs`
- `Assets/script/Ingame/Creatures/after/organ/Memory`

## 完了条件

- 群れ単位の shared target または shared alert が実行時に観測できる。
- leader / follower または役割分担の最低仕様がある。
- 個体 AI と群知性の責務境界が README 上で分かる。

## 移植元

- `8.群知性導入`
- `X-1.やりたいこと_戦術演出`
- `X-3.やりたいこと_陣形`
- `X-6.やりたいこと_縄張り`
