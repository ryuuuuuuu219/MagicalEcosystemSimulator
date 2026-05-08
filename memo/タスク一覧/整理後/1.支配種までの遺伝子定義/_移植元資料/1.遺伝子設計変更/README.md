# 1.遺伝子設計変更

## 目的

- 現行実装の遺伝子構造を把握し、次の設計変更の起点にする。
- 草食・捕食で分かれているゲノム項目、DNA入出力、世代継承処理の差分を整理する。
- 将来の群知性・魔法・支配種拡張を入れる前に、既存遺伝子の責務と不足を明文化する。

## 現状

- 実装上の主要ゲノムは `HerbivoreGenome` と `PredatorGenome` の2系統。
- 波形遺伝子として `WaveGene` を共有している。
- 捕食の攻撃範囲は `AttackArcSettings` をゲノム内に持つ。
- 草食DNAは `GenomeSerializer` による `HG:` + Base64 バイナリ形式。
- 捕食DNAは `AdvanceGenerationController` 内の `PGJ:` + JSON 形式。
- 世代継承、交叉、突然変異は主に `AdvanceGenerationController` が担当する。

## 作業区分

1. 現状構造の固定
- 対象ファイル:
  - `Assets/script/Ingame/Creatures/Herbivore/herbivoreBehaviour.cs`
  - `Assets/script/Ingame/Creatures/Predator/predatorBehaviour.cs`
  - `Assets/script/Ingame/Genome/GenomeSerializer.cs`
  - `Assets/script/Ingame/UI/Menu/ingame/AdvanceGenerationController.cs`
- まとめ:
  - `現状の遺伝子構造まとめ.txt`

2. 設計変更候補の整理
- 草食・捕食で同じ意味の項目名を揃えるか検討する。
- `WaveGene` / `AttackArcSettings` / `Vector3` を含む複合遺伝子の扱いを統一する。
- DNA形式を草食・捕食で共通化するか検討する。
- 群れ、魔法、支配種に入れる遺伝子を現行構造へどう接続するか決める。

3. 旧UI整理メモの扱い
- 旧 `0.UI整理` 由来の `メニュー整備予定(長期目標).txt` は履歴資料として残す。
- UI実装タスクとして再利用する場合は、`../1.戦闘可視化強化/README.md` 側へ移動してから更新する。

## 完了条件

- 現行ゲノム項目、DNA形式、世代継承処理の担当箇所が1つのメモで確認できる。
- 次に変更する遺伝子構造の判断材料がこのフォルダに集約されている。

## 資料

- `現状の遺伝子構造まとめ.txt`
- `メニュー整備予定(長期目標).txt`
