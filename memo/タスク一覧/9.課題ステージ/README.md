# 9.課題ステージ

## 目的

- 通常課題ステージの仕様・実装を管理する。

## 作業区分

### 設計資料の校正

1. ステージ仕様の確定
- クリア条件、敵対勢力、報酬を仕様として確定する。
- 対象ファイル:
  - `memo/タスク一覧/9.課題ステージ/設定：ゲーム設計.txt`
  - `memo/タスク一覧/9.課題ステージ/敵対勢力.txt`
  - `memo/タスク一覧/9.課題ステージ/報酬.txt`
  - `memo/タスク一覧/9.課題ステージ/魔法.txt`

### 本実装

1. ステージ進行と判定
- 課題ステージの進行、勝敗、報酬付与を実装する。
- 対象ファイル:
  - `Assets/Scenes/Ingame.unity`
  - `Assets/script/Ingame/UI/Menu/ingame/AdvanceGenerationController.cs`
  - `Assets/script/Ingame/UI/WorldUIManager.cs`

2. ステージ観測UI
- ステージ条件と結果を観測できる導線を追加する。
- 対象ファイル:
  - `Assets/script/Ingame/UI/Menu/ingame/WorldUIManager.StateView.cs`
  - `Assets/script/Ingame/UI/Menu/ingame/WorldUIManager.ObjectList.cs`

## 完了条件

- 課題ステージのクリア条件が実行時に判定される。
- 出現勢力と報酬が仕様どおりに適用される。

## 参照

- `./設定：ゲーム設計.txt`
- `./敵対勢力.txt`
- `./報酬.txt`
- `./魔法.txt`
