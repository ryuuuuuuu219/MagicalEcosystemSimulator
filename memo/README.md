# memo README

このファイルを、このプロジェクトの基準 README として扱う。
以降、ルート `README.md` ではなく `memo/README.md` を参照する。

## 目的

- 開発者向けの基準文書の入口を一本化する
- 仕様メモ、実装方針、ロードマップの参照先を明確にする

## 読む順番

1. `memo/README.md`（このファイル）
2. `memo/ROADMAP.md`
3. `memo/AI_RULES.md`
4. 必要な個別メモ（`memo/設定/`、`memo/タスク一覧/`）

## 運用ルール

- 実装済み基準や方針の更新は、まず `memo/README.md` と `memo/ROADMAP.md` の整合を確認する。
- ユーザー向け文書は `document/` に置き、開発者向け整理は `memo/` に置く。
- `memo/チェンジログ.txt` は不使用とし、変更履歴は各タスク文書またはコミット履歴で管理する。

