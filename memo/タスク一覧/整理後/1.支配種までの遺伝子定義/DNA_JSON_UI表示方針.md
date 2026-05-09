# DNA JSON UI表示方針

## 目的

DNA / JSON / UI / generation log に出す項目と、内部計算だけで使う項目を分ける。

phase1 では表示仕様の正本を決め、実装タスクは後続の UI・serializer 整備へ渡す。

## 現行DNA形式

| 種別 | prefix | 形式 | 実装箇所 | 備考 |
| --- | --- | --- | --- | --- |
| 草食 | `HG:` | Base64 binary | `GenomeSerializer` | version 管理あり。現行 version 2 |
| 捕食 | `PGJ:` | JSON | `AdvanceGenerationController` | `JsonUtility.ToJson(PredatorGenome)` |

## phase1方針

- 草食・捕食のDNA形式はすぐには統一しない。
- 旧DNAを読めることを優先し、移行は追加形式または読み替え層で行う。
- 遺伝させる内部数値は `ValueGene` として保存する。
- 新 organ gene の保存形式は `AIComponentGene` のリストとして扱う。
- runtime配布は `GeneDataManager.genes_v` / `GeneDataManager.genes_s` を使う。
- JSON保存は `GeneDataSnapshot` のような serializable wrapper を使う。
- DNA表示は「再スポーン・世代更新に必要な正本」に限定する。
- UI表示は調整しやすさを優先し、内部計算専用値をすべて露出しない。

## DNAに含める項目

| 項目 | 扱い |
| --- | --- |
| 旧 `HerbivoreGenome` / `PredatorGenome` | 移行期間中は保存対象 |
| `ValueGene` | 内部数値遺伝子として保存対象 |
| `GeneDataSnapshot.genes_v` | `ValueGene` 保存リスト |
| `GeneDataSnapshot.genes_s` | `AIComponentGene` 保存リスト |
| `WaveGene[]` | 個体差として保存対象 |
| `AttackArcSettings` | 捕食攻撃の個体差として保存対象 |
| `AIComponentSet.genes` | 新正本として保存対象 |
| `AIComponentGene.componentId` | 保存対象 |
| `AIComponentGene.enabled` | 保存対象 |
| `AIComponentGene.isVitalOrgan` | 保存対象 |
| `AIComponentGene.isVestigialOrgan` | 保存対象 |
| `AIComponentGene.level` | 保存対象 |
| `AIComponentGene.weight` | 保存対象 |
| `AIComponentGene.installChance` | 保存対象 |
| `AIComponentGene.mutationChanceT` | 保存対象 |
| `AIComponentGene.mutationChanceG` | 保存対象 |
| `AIComponentGene.minLevel` / `maxLevel` | 保存対象 |

## DNA本体に含めない項目

| 項目 | 理由 | 代替 |
| --- | --- | --- |
| 現在HP | 生存中状態値 | runtime state / log |
| 現在mana | 生存中状態値 | `Resource` / log |
| 位置・速度 | 生存中状態値 | scene state |
| `Resource.manaLog` | 履歴情報 | UI / log |
| `Resource.resourceCategory` | phase正本だがDNA本体ではない | DNAメタ情報・保存bucket |
| `Resource.speciesID` | 系統メタ情報 | DNAメタ情報・log |
| 世代評価スコア | 世代更新結果 | `GenerationLog` |

## JSON表示

JSON表示は以下の用途に分ける。

| 用途 | 表示対象 |
| --- | --- |
| 再現用 | DNA本体、organ gene、version |
| デバッグ用 | 旧 genome 全項目、organ gene 全項目 |
| UI調整用 | level、weight、enabled、mutationChance |
| ログ用 | phase、speciesID、評価スコア、採用理由 |

## Serializer方針

- 旧 `HG:` / `PGJ:` は維持する。
- 旧DNAの後方互換を壊さない。
- `GeneDataManager` の中身は `GeneDataSnapshot` に変換し、`JsonUtility.ToJson()` で文字列化する。
- 新prefixは必須にしない。
- JSON内部には version / schema を持たせ、将来の拡張に備える。

## UI表示

Genome Viewer では以下を優先する。

- species: 草食 / 捕食 / 上位捕食 / 支配種。
- phase: 保存bucketまたは現在の `Resource.resourceCategory`。
- number: bucket内の番号。
- DNA code: コピー・注入可能な文字列。
- organ summary: active / optional / vestigial の数。
- 主要性能: 移動、感覚、摂食、回避、攻撃、mana、phase。

詳細表示では以下を折りたたみ単位にする。

- Body / Movement
- Sense / Memory
- Desire / Decision
- Action / Combat
- Mana / Field
- Organ Genes
- Phase / Species

## GenerationLog

`GenerationLog` は「世代更新結果の要約」を残す場所とする。

含める候補:

- generation number。
- 対象 phase。
- 入力モード。
- 評価軸。
- 親DNAの識別情報。
- best genome DNA。
- 採用された `AIComponentSet` の要約。
- phase up / speciesID 変化の記録。
- 支配種候補に関係する checkpoint。

含めない候補:

- 全個体の全runtime状態。
- 毎フレームのmana log全文。
- UIレイアウト状態。

## 移植元UIメモの扱い

`_移植元資料/1.遺伝子設計変更/メニュー整備予定(長期目標).txt` は phase1 の正本ではなく、UI長期整備メモとして扱う。

phase1で拾う内容は以下に限定する。

- Genome Viewer / Injector の導線。
- phaseページ切替。
- 世代更新設定 UI。
- DNA export / import。

カメラ、ゲージ、地形、タイムスケールなどは別タスクへ送る。
