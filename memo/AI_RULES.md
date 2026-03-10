# AI_RULES

この文書は、`memo/` 配下の文書整理とコード編集時に、この会話で確定した運用ルールをまとめたものです。

## 基本文書の役割

- `README.md`
  - 実装済みデータの基準文書として扱う
  - 現在存在する機能、部分実装、未実装を整理する
- `memo/ROADMAP.md`
  - 今後のロードマップ専用文書として扱う
  - 実装済み説明は最小限にし、今後の計画と優先順に集中する
- `memo/README.md`
  - 詳細メモ、補助説明、履歴寄りの情報を保持する

## 編集ルール

1. 関数記述後に XML 形式の説明を書く

対象:

- 新規追加または大きく修正した関数
- 仕様が読み取りにくい関数

基本形:

```xml
/// <summary>
/// 関数の目的を簡潔に説明する。
/// </summary>
/// <param name="name">引数の意味。</param>
/// <returns>戻り値の意味。</returns>
```

補足:

- 既存コードの流儀を壊さない範囲で付与する
- 自明すぎる説明は避ける
- C# では XML ドキュメントコメントとして書く

2. 編集後は `README.md` と `memo/ROADMAP.md` も必要に応じて修正する

基準:

- 実装済み要素が増減したなら `README.md` を更新する
- 今後の優先順位や計画が変わったなら `memo/ROADMAP.md` を更新する
- 役割分離を崩さない

3. `memo` 内の `.txt` メモも必要に応じて修正する

対象:

- 実装状況と矛盾する古い記述
- git 未導入など、状態変化で古くなった記述
- 既に整理済み文書と食い違う予定表、仕様メモ

4. 文書更新前に、実装済みスクリプトを確認する

基準:

- 推測だけで実装済み扱いしない
- 主要スクリプトを読んで確認する
- 実装済み / 部分実装 / 未実装を分ける

主な確認対象:

- `Assets/script/herbivore/herbivoreBehaviour.cs`
- `Assets/script/predator/predatorBehaviour.cs`
- `Assets/script/UI/Menu/AdvanceGenerationController.cs`
- `Assets/script/UI/WorldUIManager.cs`
- `Assets/script/UI/Menu/WorldUIManager.StateView.cs`
- `Assets/script/ResourceDispenser.cs`
- `Assets/script/AI/threatmap_calc.cs`
- `Assets/script/AI/ThreatMapsGenerator.cs`

5. 文書間の定義を統一する

基準:

- `README.md` は現状
- `memo/ROADMAP.md` は将来計画
- `memo/実装予定マップまとめ.txt` は中間整理
- `memo/予定表.txt` は簡易な一覧

同じ内容を重複させる場合も、役割に応じて粒度を変える

6. 実装状況に揺れがある場合は、差分を明記して補正する

例:

- `threatmap` はコードありだが主導線未統合
- 巡回は wander ベースで部分実装
- 死亡後分解と炭素返却は実装済み

7. リポジトリ状態の変化をメモへ反映する

例:

- `.git` 導入後は git 未導入前提の記述を修正する

8. 変更後は関連文書を再照合する

最低限確認するもの:

- 編集対象ファイル
- `README.md`
- `memo/ROADMAP.md`
- 必要なら `memo/README.md`

## この会話で確定した運用

- 実装済みとロードマップは分離して管理する
- 文書更新時は `memo` の元メモとコード実装を両方参照する
- パフォーマンス課題や未統合機能は「部分実装」として書く
- 実装の裏取りが取れていない要素は、README に断定的に書かない
