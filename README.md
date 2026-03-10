# 魔法環境シミュ

Unity で開発している、生態系と魔法進化を扱うシミュレーターです。

この `README.md` は、現時点で実装済みの要素を確認するための基準文書です。  
今後の予定や拡張順は [memo/ROADMAP.md](/c:/魔法環境シミュ/memo/ROADMAP.md) に分離しています。

## 現在の実装範囲

現在は、ランダム地形上で植物、草食動物、肉食動物、資源循環、世代交代を観察できる段階です。

実装済みの主な要素:

- 地形生成
- 植物相
- 草食動物 AI
- 肉食動物 AI
- 炭素ベースの資源循環
- 死亡と分解
- 世代交代 UI
- DNA ビューア
- 草食 DNA 文字列の注入スポーン
- 対象追跡 UI
- 状態表示 UI
- パフォーマンス監視

## 実装済みシステム

### ワールド

- `WorldGenerator.cs`
  - seed ベース地形生成
  - water 配置
  - 壁、床、天井の生成
- `grassland.cs`
  - 草地パッチ生成
  - 傾斜、水位を見た植物生成

### 資源循環

- `Resource.cs`
  - carbon / maxCarbon の保持
  - 摂食による炭素移動
- `ResourceDispenser.cs`
  - 総炭素量の管理
  - 草、草食、肉食への初期配布
  - 草の再生
  - 分解で戻った炭素の回収
  - energy 系パラメータ管理

### 草食動物

- `herbivoreManager.cs`
  - 草食個体の生成、管理
  - manager genome / next generation genome
- `herbivoreBehaviour.cs`
  - 草探索
  - predator 脅威記憶
  - 近接回避
  - zigzag evasion
  - health 管理
  - 死亡後分解

### 肉食動物

- `predatorManager.cs`
  - 肉食個体の生成、管理
  - manager genome / next generation genome
- `predatorBehaviour.cs`
  - prey 記憶
  - threat 記憶
  - 接近、攻撃
  - 死体摂食
  - health 管理
  - 死亡後分解

### 世代交代

- `AdvanceGenerationController.cs`
  - 一斉世代交代
  - 評価軸 `Random / Carbon / Health / Selection`
  - 入力元 `Population / SavedGenome / ManagerGenome`
  - 交差
  - 突然変異
  - generation phase 切替
  - DNA viewer
  - 草食 DNA 注入スポーン

### UI

- `WorldUIManager.cs`
  - オブジェクト選択
  - follow 表示
  - ステータス表示
- `WorldUIManager.StateView.cs`
  - vision / wander wave 表示
  - 草食 DNA copy
- `FreeCameraController.cs`
  - フリーカメラ操作

### 補助実装

- `threatmap_calc.cs`
  - 脅威スコアグリッド
  - low threat direction probe
  - field evaluation
- `ThreatMapsGenerator.cs`
  - threatmap 可視化
- `PerformanceBudgetMonitor.cs`
  - 個体数、フレーム負荷の監視

## 実装状況の整理

実装済み:

- 地形
- 植物
- 草食
- 肉食
- 草食への攻撃
- 草食の防御反応
- 炭素循環
- 死亡と分解
- 世代交代 UI

部分実装:

- 巡回
  - wander ベース徘徊はある
  - 縄張り、拠点巡回は未実装
- 肉食戦闘
  - 草食追跡、攻撃、死体摂食はある
  - 対肉食、包囲、撤退は未実装
- 脅威度マップ
  - スクリプトはある
  - AI 主導線への全面統合は未完

未実装:

- 菌類
- 森
- 視界隠蔽
- 群知性
- 魔素の場
- 属性魔法
- 空間魔法統合
- 支配種進化

## ドキュメント

- 実装済み基準: [README.md](/c:/魔法環境シミュ/README.md)
- 今後の計画: [memo/ROADMAP.md](/c:/魔法環境シミュ/memo/ROADMAP.md)
- 詳細メモ: [memo/README.md](/c:/魔法環境シミュ/memo/README.md)
- 編集ルール: [memo/AI_RULES.md](/c:/魔法環境シミュ/memo/AI_RULES.md)

## 注意

- `threatmap` 系は存在しますが、現行 AI の中心ロジックではありません。
- predator の DNA 文字列 export は草食ほど整備されていません。
