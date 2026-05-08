# 1.支配種までの遺伝子定義

## 仕様目的

草食、捕食、上位捕食、支配種までの genome 項目を定義し、相進化と世代更新で扱う値の正本を決める。

## スコープ

- 草食 genome と捕食 genome の現行項目整理。
- `predator` / `highpredator` / `dominant` の phase と genome の対応。
- 旧 genome、将来 genome、organ gene の境界定義。

## 現状

- `HerbivoreGenome` / `PredatorGenome` / `WaveGene` がある。
- `GenomeSerializer` は草食 genome の DNA 文字列を扱う。
- 捕食側は JSON 表示・収集があるが、草食と形式が統一されていない。
- 支配種専用の genome 定義はまだ薄い。

## 仕様

- genome は「身体性能」「感覚」「移動」「戦闘」「mana」「相進化」「魔法資質」の区分で整理する。
- 支配種は `dominant` phase として扱い、勝利条件や上位行動差分は別タスクで定義する。
- 旧 genome の項目は削除せず、対応表を作ってから段階的に統合する。
- UI 表示・DNA 入出力に出す項目と、内部専用項目を分ける。

## 実装タスク

- 草食・捕食 genome の項目対応表を作る。
- phase / speciesID / dominant 到達に必要な genome 項目を定義する。
- 旧 JSON / DNA 表示と将来形式の移行ルールを決める。
- UI 長期メモに残る調整項目は、active task ではなく保留資料として扱う。

## 対象スクリプト

- `Assets/script/Ingame/Creatures/before/Herbivore/HerbivoreGenome.cs`
- `Assets/script/Ingame/Creatures/before/Predator/PredatorGenome.cs`
- `Assets/script/Ingame/Creatures/before/Common/WaveGene.cs`
- `Assets/script/Ingame/Genome/GenomeSerializer.cs`
- `Assets/script/Ingame/UI/Menu/ingame/AdvanceGenerationController.cs`

## 完了条件

- 草食・捕食・上位相・支配種で必要な genome 項目が一覧化されている。
- 現行 genome から新仕様への読み替えができる。
- 世代更新、相進化、魔法資質、organ 構成へ渡す値の境界が明記されている。

## 移植元

- `1.遺伝子設計変更`
- `10.支配種到達条件`
- `X-4.やりたいこと_遺伝子拡張候補`
