# 1.支配種までの遺伝子定義

## 目的

phase0 で organ 実行基盤が完了したため、phase1 では草食、捕食、上位捕食、支配種までで参照される遺伝子の正本を定義する。

ここでの目的は、旧 `HerbivoreGenome` / `PredatorGenome`、`ValueGene`、`speciesID` / `phase`、新しい `AIComponentGene` / `AIComponentSet` の境界を固定し、以後の organ 構成遺伝、世代更新、相進化、支配種仕様が同じデータを参照できるようにすること。

phase1 はデータ定義のタスクであり、phase up 条件や支配種の総合仕様は決めない。それらは `4.相の進化` と `5.支配種仕様・役割定義` に置く。

## phase0 完了後の前提

- AI 実行主体は `OrganFoundation` + `AnimalBrain`。
- organ の有無、強度、変異は `AIComponentGene` / `AIComponentSet` で表現できる。
- `OrganPresetLibrary` は草食、捕食、上位捕食、支配種相の organ preset を生成できる。
- `predator` -> `highpredator` -> `dominant` の phase 表現は既にコード上にある。
- 旧 genome はまだ身体性能、感覚距離、移動力、戦闘値などの実値を多く持っている。

## phase1 で決めること

- 草食 genome と捕食 genome の現行項目を一覧化し、用途別に分類する。
- `phase`、`speciesID`、`category.dominant` を genome 上でどう扱うか決める。
- 旧 genome に残す値と、organ gene へ移す値を分ける。
- 遺伝させる内部数値は `ValueGene` として一元管理し、organ ごとの構造体にまとめる。
- `AIComponentGene.level` / `weight` / `enabled` と旧 genome 数値の対応方針を決める。
- DNA / JSON / UI 表示に出す項目と、内部計算だけで使う項目を分ける。
- 支配種仕様側が参照する遺伝・phase・organ のデータ項目を定義する。

## genome正本方針

phase1 以降の genome は、内部数値遺伝子と organ 構造遺伝子を分けて保存する。

- 内部数値遺伝子:
  - `ValueGene` に一元管理する。
  - 基本的に遺伝させる数値はすべて保持する。
  - 可視性を上げるため、organ ごとの入れ子構造体にまとめる。
  - 例: `valueGene.PredatorVisionSense.threatDetectDistance`
- organ 構造遺伝子:
  - `public List<AIComponentGene> genes` で保存する。
  - organ の有無、退化、必須器官、level、weight、変異率を表す。
- 適用方法:
  - 実体化済み個体に対して `TryGetComponent<各organ>()` を行う。
  - 対応する organ が存在する場合、`ValueGene` の対応変数を organ 内変数へ流し込む。
  - organ 側は自分に対応する数値だけを参照する。
  - organ が存在しない場合も `ValueGene` 側の値は保持し、後から organ が発現したときに使えるようにする。

## 作業順

1. 現行 genome 棚卸し
   - `HerbivoreGenome`
   - `PredatorGenome`
   - `WaveGene`
   - manager が保持する `nextGenerationGenome`
   - `AIComponentSet` / `AIComponentGene`

2. 項目分類表を作る
   - 身体: HP、サイズ、死亡/分解、mana 容量。
   - 感覚: 視界距離、視野角、記憶時間、対象識別。
   - 移動: forward force、turn force、旋回慣性、回避系。
   - 戦闘: 攻撃力、攻撃範囲、突進、脅威パルス。
   - mana: 吸収、field 感知、mana 誘引。
   - phase: predator / highpredator / dominant。
   - organ: organ の有無、level、weight、mutation chance。
   - 表示: DNA、JSON、UI、generation log。

3. 正本の置き場所を決める
   - 旧 genome に残すもの。
   - `AIComponentGene` に移すもの。
   - `Resource` / `category` / `speciesID` に残すもの。
   - `GenerationLog` にだけ残すもの。

4. 旧 genome から新仕様への読み替え表を作る
   - 既存プレイを壊さずに移行できるよう、削除ではなく対応表を先に作る。
   - organ gene に移す値は、対応 organ と既定値を明記する。

5. 支配種仕様側へ渡す最小データ仕様を固定する
   - `dominant` は phase の最上位として扱う。
   - dominant 専用 organ を作る場合に必要な保存場所を示す。
   - 支配種仕様タスクへ渡す判定材料を一覧化する。

## 成果物

- `現行genome項目対応表.md`
- `旧genome_to_organ_gene対応表.md`
- `ValueGene設計方針.md`
- `GeneDataManager設計方針.md`
- `phase_species_dominant定義.md`
- `DNA_JSON_UI表示方針.md`

## 他タスクとの境界

- `2.organ構成の遺伝`
  - phase1 で定義した organ gene 仕様を、実際の継承、crossover、serializer に接続する。
- `2.5.organ突然変異・生存中可塑性`
  - mutation chance と level 変化の仕様を受け取り、生存中/世代更新時の変異へ接続する。
- `3.世代更新と遺伝`
  - phase1 の正本定義を使って、次世代 genome / component set を保存・復元する。
- `4.相の進化`
  - phase と category の保存場所を参照し、phase up 条件と実行処理を決める。
- `5.支配種仕様・役割定義`
  - dominant 判定に必要な遺伝・phase・organ 情報を参照し、到達条件・追加付与 organ・役割・勝利条件を決める。

## 対象スクリプト

- `Assets/script/Ingame/Creatures/before/Herbivore/HerbivoreGenome.cs`
- `Assets/script/Ingame/Creatures/before/Predator/PredatorGenome.cs`
- `Assets/script/Ingame/Creatures/before/Common/WaveGene.cs`
- `Assets/script/Ingame/Genome/GenomeSerializer.cs`
- `Assets/script/Ingame/Genome/GenerationLog.cs`
- `Assets/script/Ingame/UI/Menu/ingame/AdvanceGenerationController.cs`
- `Assets/script/Ingame/Creatures/after/organ/Core/AIComponentGene.cs`
- `Assets/script/Ingame/Creatures/after/organ/Core/AIComponentSet.cs`
- `Assets/script/Ingame/Creatures/after/organ/Core/OrganPresetLibrary.cs`
- `Assets/script/Ingame/Environment/Resource.cs`

## 完了条件

- 草食、捕食、上位捕食、支配種で必要な genome 項目が一覧化されている。
- 旧 genome、`Resource`、`speciesID`、`AIComponentSet` の責務境界が明記されている。
- 旧 genome から organ gene への読み替え表がある。
- DNA / JSON / UI / generation log に出す項目が決まっている。
- 支配種仕様タスクが参照できるデータ項目がまとまっている。

## 移植元

- `1.遺伝子設計変更`
- `10.支配種到達条件`
- `X-4.やりたいこと_遺伝子拡張候補`
