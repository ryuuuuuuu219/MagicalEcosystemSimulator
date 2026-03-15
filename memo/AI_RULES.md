# AI_RULES

このファイルは、このリポジトリ内で AI が参照する作業ルールの統合版です。
特に `ai/orchestrator.py` を使った最小 CLI 運用を前提にしています。

## 基本方針

- まず現状コードを確認してから変更する。
- Unity / C# プロジェクトとして整合する実装を優先する。
- 既存挙動を壊さない。仕様変更が必要な場合は変更点を明示する。
- 変更後は、コードとドキュメントの整合を取る。
- 判断材料が `memo` 配下にある場合は、関連文書を先に読む。

## 優先して参照するファイル

- `README.md`
  - プロジェクト全体の概要と実装方針を確認する。
- `memo/ROADMAP.md`
  - 今後の機能計画、未実装項目、優先順位を確認する。
- `memo/README.md`
  - `memo` 配下の文書の役割を把握する。
- `ai/AGENTS.md`
  - Unity 実装時の制約を確認する。

## 実装ルール

- C# の関数には必要に応じて XML コメントを付ける。
- 既存コードの責務を尊重し、不要な大規模リネームは避ける。
- `Update` と `FixedUpdate` の責務を混同しない。
- 反射は使わない。
- 可能な範囲で ECS フレンドリーな構造を優先する。
- アロケーションとメモリ churn を増やさない。
- git 未管理の一時ファイルや不要ファイルを勝手に追加しない。

## ドキュメント更新ルール

- 実装内容が仕様に影響する場合は `README.md` と `memo/ROADMAP.md` の更新要否を確認する。
- システム理解に必要な知見は `memo` 配下へ追記する。
- 一時メモは乱立させず、既存文書に統合できるなら統合する。

## 重点確認対象

以下のファイルは影響が大きいため、変更時は依存関係と挙動を慎重に確認する。

- `Assets/script/herbivore/herbivoreBehaviour.cs`
- `Assets/script/predator/predatorBehaviour.cs`
- `Assets/script/UI/Menu/AdvanceGenerationController.cs`
- `Assets/script/UI/WorldUIManager.cs`
- `Assets/script/UI/Menu/WorldUIManager.StateView.cs`
- `Assets/script/ResourceDispenser.cs`
- `Assets/script/AI/threatmap_calc.cs`
- `Assets/script/AI/ThreatMapsGenerator.cs`

## AI オーケストレーター運用

このプロジェクトには、ローカルの Codex を段階実行する最小 CLI として
`ai/orchestrator.py` を用意している。

### 役割

- `inspector`
  - 関連ファイルと現状の実装ポイントを洗い出す。
- `planner`
  - 実装方針を `CODEX TASK` 形式に落とし込む。
- `executor`
  - 実装コード案を生成する。
- `risk_review`
  - バグ、性能、Unity ライフサイクル、メモリ観点でレビューする。

### 基本コマンド

```powershell
python ai\orchestrator.py "タスク内容"
python ai\orchestrator.py --last
python ai\orchestrator.py --list-logs
```

### ログ保存先

- 最新ログ: `ai/log.json`
- 実行ごとの保存: `ai/logs/*.json`
- 追記履歴: `ai/log_history.jsonl`

### この会話での運用ルール

この会話で AI オーケストレーターを使いたい場合、次のように指示する。

- `オーケストレーターで実行して`
- `ai/orchestrator.py で回して`
- `このタスクをオーケストレーターに通して`
- `--last を見て`
- `--list-logs を見て`

上記の指示がある場合、この会話では次を行う。

- `ai/orchestrator.py` を実行する。
- 必要に応じて `ai/log.json` または `ai/logs/*.json` を読む。
- 結果を要約し、必要ならそのまま追加修正を行う。

## 会話での優先動作

- ユーザーが通常の実装依頼をした場合:
  - まずコードを直接確認して作業する。
- ユーザーが「オーケストレーターで」実行するよう依頼した場合:
  - `ai/orchestrator.py` を使って結果を取得し、その結果も参照して作業する。
- ユーザーがレビューを依頼した場合:
  - `risk_review` の観点も含めて、バグと回帰リスクを優先して確認する。

## 補足

- このファイルは会話の自動人格切り替えを行うものではない。
- ただし、このファイルに従って「オーケストレーターで」と指示すれば、
  この会話内でローカル CLI を起動して結果を参照できる。
