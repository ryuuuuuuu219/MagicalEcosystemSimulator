# phase species dominant定義

## 目的

`phase`、`speciesID`、`category.dominant` の責務境界を固定し、相進化・世代更新・支配種仕様が同じ定義を参照できるようにする。

## 用語

| 用語 | 正本 | 意味 |
| --- | --- | --- |
| phase | `Resource.resourceCategory` | 個体の現在の相。草食、捕食、上位捕食、支配種など |
| speciesID | `Resource.speciesID` | 系統・種分化の識別子 |
| dominant | `category.dominant` | phaseの最上位。支配種候補または支配種として扱う相 |
| organ phase | `OrganFoundation.phase` / checkpoint | organ実行側が記録する相情報 |
| genome phase bucket | `GenomePhaseBucket` | UI / 保存DNA上の相ページ |

## phase rank

現行コードでは以下の順位で扱う。

| category | rank | 用途 |
| --- | --- | --- |
| `grass` | 1 | 資源・食物 |
| `herbivore` | 2 | 草食動物 |
| `predator` | 3 | 捕食動物 |
| `highpredator` | 4 | 上位捕食者 |
| `dominant` | 5 | 支配種 |

## 責務境界

| 情報 | 持ち主 | 理由 |
| --- | --- | --- |
| 現在の相 | `Resource.resourceCategory` | 生存中に変化し、UI・関係判定・魔法コスト・phase up が参照するため |
| 系統ID | `Resource.speciesID` | 同種判定、相進化時の系統管理、支配種判定に使うため |
| organ構成 | `AIComponentSet` | organ の有無・強度・変異を表すため |
| phase別preset | `OrganPresetLibrary` | phaseに応じた初期organ構成を供給するため |
| 世代保存先 | manager の `genomes` / `nextGenerationGenome` | DNA保存・世代更新のため |
| 支配種仕様材料 | `Resource` + `AIComponentSet` + `ValueGene` + `GenerationLog` | 支配種仕様タスクが参照するため |

## phase up

現行の phase up は `PredatorPhaseEvolutionAction` と旧 `predatorBehaviour` 側に存在する。

- 対象は `predator` 以上、`dominant` 未満。
- field mana を確率へ変換し、成功時に phase rank を1つ上げる。
- phase up 時に `speciesID` を抽選し直す。
- phase up 後、`OrganPresetLibrary.EnsurePredator` で相に応じた preset を反映する。

## speciesID

`speciesID` は genome の内部項目ではなく、現状は `Resource` 側の個体メタ情報として扱う。

phase1では以下を採用する。

- `speciesID` は DNA本体ではなく、保存・表示・ログの付帯情報として扱う。
- 同種判定や上位相同士の関係判定は `Resource.speciesID` を参照する。
- phase up 時の `speciesID` 再抽選は、種分化イベントとして `GenerationLog` または mana log に残す候補にする。

## dominant

`dominant` は phase rank 5 の最上位相として扱う。

phase1時点では、dominant 専用 genome を新設せず、以下の最小仕様に留める。

- `category.dominant` は phase の正本値。
- organ preset は `highpredator` 以上の強化presetを流用できる。
- dominant 専用 organ、役割、到達条件、勝利条件は `5.支配種仕様・役割定義` で判断する。
- 支配種仕様には phase だけでなく、organ構成、ValueGene、mana、世代ログ、個体数などを組み合わせる。

## UI上の扱い

- phase ページは保存DNAの整理用であり、生存中の正本は `Resource.resourceCategory`。
- Genome Injector の phase 選択はスポーン対象カテゴリの選択として扱う。
- `grass` は genome injection 非対応。
- `dominant` の injection は将来的に許可可能だが、支配種仕様と矛盾しない制限が必要。
