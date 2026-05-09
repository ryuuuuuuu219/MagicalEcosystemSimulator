# 3.世代更新と遺伝

## 目的

世代更新時の評価、選抜、交叉、突然変異、DNA 入出力、世代ログを統一し、phase / magic / organ 拡張を受けられる形にする。

## 現状

- `AdvanceGenerationController` に世代更新、評価、crossover、mutation がある。
- `GenerationLog` / `GenomeLogger` がある。
- 評価軸は mana / health / random が中心。
- organ checkpoint 評価が追加され、最良個体の最良 checkpoint から次世代 `AIComponentSet` を作れる。
- `herbivoreManager` / `predatorManager` の `nextGenerationComponentSet` を通して、次世代 spawn に organ set を渡せる。
- `GenerationLog` は organ checkpoint reason、active organ、vestigial organ、generation mutation summary を記録する。
- DNA / genome serializer への organ gene 永続保存はまだ残っている。

## スコープ

- 世代更新 controller。
- genome 評価軸。
- DNA 表示・注入。
- 世代ログと検証用ログ。
- organ checkpoint の評価候補化。
- 世代更新時 mutation の呼び出し。
- `ValueGene` と `AIComponentGene` の同時 crossover / mutation。
- `GeneDataManager` への次世代値保存。

## 実装済み

- 生存中 mutation checkpoint を世代更新時の評価候補に含める。
- checkpoint score に active organ、vestigial organ、mutation event の小さな補正を入れる。
- 最良 checkpoint から `AIComponentSet.CreateGenerationMutatedCopy()` を呼ぶ。
- manager に次世代 component set を設定し、spawn 時に `OrganFoundation` へ導入する。
- generation log に organ mutation summary を残す。

## 残り

- DNA / genome serializer に `AIComponentSet` を保存・復元する。
- `GeneDataSnapshot` として `ValueGene` / `AIComponentGene` を保存・復元する。
- crossover で `ValueGene` と organ gene を複数親から同時に混ぜる。
- 生存中 mutation は10秒間隔、世代更新 mutation は世代更新時に実行し、どちらも `ValueGene` と `AIComponentGene` を同時に扱う。
- 親にない organ の `ValueGene` は `GeneDataManager` の初期値 `genes_v` から補完する。
- organ checkpoint score の重みを実機結果で調整する。
- magic aptitude、phase、dominant 判定を同じ評価・ログ系に統合する。
- DNA 注入時の validation と失敗時表示を整理する。

## 対象スクリプト

- `Assets/script/Ingame/UI/Menu/ingame/AdvanceGenerationController.cs`
- `Assets/script/Ingame/Genome/GenerationLog.cs`
- `Assets/script/Ingame/Diagnostics/GenomeLogger.cs`
- `Assets/script/Ingame/Genome/GenomeSerializer.cs`
- `Assets/script/Ingame/Creatures/after/organ/Core/OrganFoundation.cs`
- `Assets/script/Ingame/Creatures/after/organ/Core/AIComponentSet.cs`
- `Assets/script/Ingame/Creatures/before/Herbivore/herbivoreManager.cs`
- `Assets/script/Ingame/Creatures/before/Predator/predatorManager.cs`

## 完了条件

- 世代更新の入力、評価軸、出力が README から追える。
- organ gene の継承と mutation がログで確認できる。
- 草食・捕食の genome 形式差による表示・注入事故が起きない。
- phase / magic / organ の追加評価が既存更新処理を壊さずに追加できる。
