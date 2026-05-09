# GeneDataManager設計方針

## 目的

`ValueGene` と `AIComponentGene` を一元保存し、spawn、phase up、mutation、DNA注入、世代更新へ配布する入口を固定する。

## 基本方針

- `GeneDataManager` は遺伝子データの配布元にする。
- 内部数値遺伝子は `genes_v` に保存する。
- organ構造遺伝子は `genes_s` に保存する。
- 欠落している対応表や初期値は、実装時に現行コードから生成する。
- phase4 までの作業では、この manager を通して genome / organ / phase up の接続を進める。

## データ構造

```csharp
public static class GeneDataManager
{
    public static List<ValueGene> genes_v;
    public static List<AIComponentGene> genes_s;
}
```

`JsonUtility` は static class 自体や裸の `List<T>` 保存が苦手なため、保存・復元時は serializable な包みを使う。

```csharp
[System.Serializable]
public class GeneDataSnapshot
{
    public List<ValueGene> genes_v = new();
    public List<AIComponentGene> genes_s = new();
}
```

方針:

- runtime配布: `GeneDataManager`
- JSON保存: `GeneDataSnapshot`
- DNA文字列化: `GeneDataSnapshot` を `JsonUtility.ToJson()` する
- DNA復元: JSONから `GeneDataSnapshot` を復元し、`GeneDataManager` へ流し込む

## 初期値

親にない organ の `ValueGene` は、`GeneDataManager` に保持する初期値用 `genes_v` から補完する。

補完順:

1. DNA / 世代更新結果にある `ValueGene`
2. 旧 `HerbivoreGenome` / `PredatorGenome` から生成した `ValueGene`
3. `GeneDataManager` の初期値 `genes_v`
4. organ 側の既定値

## 適用タイミング

| タイミング | 方針 |
| --- | --- |
| spawn時 | `GeneDataManager` から `ValueGene` と `AIComponentGene` を適用する |
| phase up時 | phase用 preset 合成後に再適用する |
| mutation後 | 10秒間隔の生存中 mutation 後に即反映する |
| 世代更新時 | 世代更新 mutation 後、次世代 manager 値として保存する |
| DNA注入時 | JSONから `GeneDataSnapshot` を復元し、`GeneDataManager` へ入れてから spawn へ渡す |

## 適用処理

適用処理は個別 organ に散らさず、専用の適用層へ寄せる。

候補:

- `GeneDataApplier`
- `GenomeValueApplier`
- `OrganValueBinder`

基本処理:

```csharp
if (target.TryGetComponent<PredatorVisionSense>(out var organ))
{
    organ.threatDetectDistance = valueGene.PredatorVisionSense.threatDetectDistance;
}
```

## Serializer方針

- 旧DNAは維持する。
- 草食は `HG:`、捕食は `PGJ:` を維持する。
- 新しい `GeneDataManager` 情報は JSON ブロックとして追加保存する。
- 新prefixを必須にはしない。
- ただし将来の判別用に、JSON内に version と schema 名を持たせる。

例:

```csharp
[System.Serializable]
public class GeneDataSnapshot
{
    public int version = 1;
    public string schema = "GeneDataManager";
    public List<ValueGene> genes_v = new();
    public List<AIComponentGene> genes_s = new();
}
```

## Crossover / Mutation

- `ValueGene` と `AIComponentGene` は同時に変異する。
- 生存中 mutation は10秒間隔で実行し、変異後に即反映する。
- 世代更新 mutation は世代更新時に実行し、次世代へ保存する。
- `ValueGene` の mutation は数値範囲ごとの clamp を持つ。
- `AIComponentGene` の mutation は既存の `mutationChanceT/G` を使う。
- 対応 organ がない `ValueGene` は消さずに保持する。

## phase4までの実装範囲

phase4までで実装する範囲:

1. `ValueGene` の構造体定義。
2. 旧 genome から `ValueGene` への対応表生成。
3. `GeneDataManager` / `GeneDataSnapshot` の定義。
4. spawn時の `ValueGene` / `AIComponentGene` 適用。
5. DNA注入時の復元。
6. 生存中 mutation 後の即反映。
7. 世代更新時の同時 crossover / mutation。
8. phase up時の preset 合成と再適用。

phase5以降へ送る範囲:

- 支配種専用 organ の詳細実装。
- 支配種役割定義に基づく勝利条件。
- 魔法・群知性・課題ステージとの高度な統合。
