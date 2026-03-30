# 0.UI整理

参照元: `git show f5a7322:memo/ROADMAP.md`

## 目的

- ユーザー観測導線を詰まらせないUI構成へ整理する。
- リリース後のUI拡張項目を、実装着手順に並べる。

## 現状

- `document/ユーザー向け/説明書.txt` は存在。
- 長期UI項目は `資料` 配下に分散。

## 作業区分

### 設計資料の校正

1. 説明書の正式化
- 文書名・本文から暫定表現を除去。
- 起動→生成→観察の1導線を維持。
- 対象ファイル:
  - `document/ユーザー向け/説明書.txt`
  - `document/ユーザー向け/README.md`

  終了扱い　以降各作業フォルダの進行完了時に編集する。

2. UI着手順位の固定
- 第一候補: 動物上ゲージ表示切替 UI
- 第二候補: 世代更新詳細設定 UI（相選択 + mutation on/off）
- 第三候補: カメラ追従方式切替 UI
- 対象ファイル:
  - `memo/タスク一覧/0.UI整理/資料/メニュー整備予定(長期目標).txt`
  - `memo/タスク一覧/0.UI整理/資料/実装機能候補一覧.txt`
  - `memo/タスク一覧/0.UI整理/資料/拡張方針.txt`

上記順序で決定

3. debug可視化の公開方針
- threat/heat を開発者専用か公開対象か明文化。
- 公開する場合は最小導線を定義。
- 対象ファイル:
  - `memo/タスク一覧/0.UI整理/資料/メニュー整備予定(長期目標).txt`
  - `Assets/script/Ingame/AI/threatmap_calc.cs`
  - `Assets/script/Ingame/AI/ThreatMapsGenerator.cs`
  - `Assets/script/Ingame/balance/ResourceDispenser.cs`

非公開　もしくはphase10以降に整備・公開

4. 設計メモのタスク化
- 各作業フォルダへ統合した `設定：*.txt` を「対象システム / 依存 / 完了条件」で再分解。
- 対象ファイル:
  - `memo/タスク一覧/*/設定：*.txt`
  - `memo/設定/設定：ゲノム構造設計.txt`

設定側の追記の可能性・修正の手間を考慮ししないものとする

### 本実装

2. Ingame UI基盤
- 対象ファイル:
  - `Assets/script/Ingame/UI/WorldUIManager.cs`
  - `Assets/script/Ingame/UI/Menu/ingame/WorldUIManager.MenuTree.cs`
  - `Assets/script/Ingame/UI/Menu/ingame/WorldUIManager.ObjectList.cs`
  - `Assets/script/Ingame/UI/Menu/ingame/WorldUIManager.StateView.cs`
  - `Assets/script/Ingame/UI/Menu/ingame/WorldUIManager.VirtualGauge.cs`

## 完了条件

- 上記 1〜4 のステータスが明記され、次担当が即着手できる。

## 資料

- `資料/メニュー整備予定(長期目標).txt`

