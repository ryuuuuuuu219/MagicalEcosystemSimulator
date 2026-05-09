# 整理後タスク一覧

`進捗確認.md` と `再構成前_問題点.md` をもとに、現在の実装軸へ合わせて再構成したタスクフォルダ。

## 現在の正本

- phase0 から organ mutation までの正本は、この階層直下の各 `README.md` と `ロードマップ.md`。
- `_移植元資料` は履歴確認用。本文中の実装状況やパスは古い可能性がある。
- とくに `Assets/script/Ingame/Creatures/Herbivore/...` と `Assets/script/Ingame/Creatures/Predator/...` は旧パスとして扱う。

## 現行パス読み替え

- 旧 `Assets/script/Ingame/Creatures/Herbivore/...`
  - 現 `Assets/script/Ingame/Creatures/before/Herbivore/...`
- 旧 `Assets/script/Ingame/Creatures/Predator/...`
  - 現 `Assets/script/Ingame/Creatures/before/Predator/...`
- organ core
  - `Assets/script/Ingame/Creatures/after/organ/Core/...`
- organ senses / desires / actions / memories / motors / steering
  - `Assets/script/Ingame/Creatures/after/organ/...`

## Active Tasks

表示順が環境によって `10` が `2` より前に出る場合があるため、作業順の正本は以下の並びとする。

- `ロードマップ.md`
- `1.支配種までの遺伝子定義`
- `2.organ構成の遺伝`
- `2.5.organ突然変異・生存中可塑性`
- `3.世代更新と遺伝`
- `4.相の進化`
- `5.支配種仕様・役割定義`
- `6.戦闘システム拡張`
- `7.魔法のダメージ判定システム`
- `8.manaへの誘引`
- `9.魔法の使用`
- `10.魔法の資質の遺伝`
- `11.群知性導入`
- `12.課題ステージ`
- `13.課題ステージ（環境外乱）`

## 1 / 4 / 5 の境界

- `1.支配種までの遺伝子定義`
  - genome の正本、`ValueGene`、`AIComponentGene`、`speciesID`、`phase` の保存場所を決める。
  - phase up 条件や勝利条件は決めない。
- `4.相の進化`
  - `predator` -> `highpredator` -> `dominant` へ進む条件、実行処理、同期処理を決める。
  - genome の項目設計そのものは `1` を参照する。
- `5.支配種仕様・役割定義`
  - `dominant` になった後、支配種としての設定、到達条件、追加付与 organ、役割、勝利接続、表示完了を決める。
  - phase up の発生条件は `4` に置く。

## Archive

- `完了・保護資料/`: 完了扱い、または active task から分離する資料。
- `保留・将来案/`: X 系の構想メモ。必要になった項目だけ active task へ移植する。
