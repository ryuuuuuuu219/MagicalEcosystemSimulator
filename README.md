# 魔法環境シミュ (Magical Ecosystem Simulator)

Unity で開発している、生態系と魔法進化を扱うシミュレーターです。
ランダム生成された地形の上で、草、草食動物、肉食動物、環境資源の循環を観察できます。

この `README.md` は、現時点で実装済みの要素とシステム構成を確認するための基準文書です。
今後の予定や拡張順は [memo/ROADMAP.md](/c:/魔法環境シミュ/memo/ROADMAP.md) を参照してください。

## Project Overview

このプロジェクトの現在の中心要素は以下です。

- Perlin Noise によるワールド生成
- 草地パッチの生成
- 環境全体の炭素管理
- 草食動物 AI
- 肉食動物 AI
- 体力、攻撃、死体分解を含む資源経済
- 世代交代 UI と遺伝子継承
- オブジェクト追跡 UI
- パフォーマンス監視

## 現状

現在は、ランダム地形上で植物、草食動物、肉食動物、資源循環、世代交代を観察できる段階です。

直近の実装状態は次のとおりです。

- 世代交代コントローラは `Random / Carbon / Health / Selection` の評価軸に対応
- 世代交代 UI で「世代進行」「DNA ビューア」「DNA 注入（文字列からスポーン）」を切り替え可能
- 遺伝子ビューアは `Phase` と `Generation page` のページング付きで、表示 DNA を manager バケットへ反映可能
- 草食動物は近接脅威に対して回避行動を実装済み
- UI 側は対象追従カメラ、オブジェクト一覧、ステータス表示を連携済み
- 草食、肉食とも死亡後は分解で炭素を環境へ返却するループを維持
- `threatmap` 系の補助実装とパフォーマンス監視を追加済み

## Main Systems

### World Generation

`WorldGenerator.cs`
- seed ベースで地形を生成
- Terrain と water を自動生成
- 見えない壁と天井、床を配置
- 地形色と水色を seed から決定

`grassland.cs`
- 草地パッチを表現
- 一定範囲に植物クアッドを配置
- 地形傾斜と水位を見て植物を生成

### Resource System

`Resource.cs`
- 各オブジェクトの `carbon / maxCarbon` を管理
- 摂食による炭素移動
- 炭素追加、炭素除去、空判定

`ResourceDispenser.cs`
- 環境全体の総炭素量を管理
- 草、草食動物、肉食動物に初期炭素を配布
- 草の再生を担当
- 分解で戻ってきた炭素を環境へ再回収
- 世代交代時の再スポーン資源初期化を担当
- energy 系パラメータを管理

### Herbivore System

`herbivoreManager.cs`
- 草食動物の生成と一覧管理
- manager genome / next generation genome を保持
- 世代交代用の切り替えに対応

`herbivoreBehaviour.cs`
- 草食動物 AI 本体
- 草を探索し、捕食者を回避しながら移動
- body carbon を消費しながら行動
- health を持ち、攻撃されると死亡
- 近接回避と zigzag evasion を実装
- 死亡後は分解され、carbon を環境へ返す

### Predator System

`predatorManager.cs`
- 肉食動物の生成と一覧管理
- manager genome / next generation genome を保持
- 第二世代用の genome 切り替えに対応

`predatorBehaviour.cs`
- 肉食動物 AI 本体
- 草食動物を探索して追跡
- 攻撃で herbivore の health を減らす
- 死亡した herbivore の死体から carbon を摂食
- threat 記憶を保持
- 自身も health を持ち、死亡後は分解される

### Generation System

`AdvanceGenerationController.cs`
- 専用 UI から一斉世代交代を実行
- 既存個体を全削除し、次世代個体を再生成
- 評価軸を選択可能
  - `Random`
  - `Carbon`
  - `Health`
  - `Selection`
- 交差の有無を選択可能
- 交差形式を選択可能
  - `Assign`
  - `Average`
  - `Interpolate`
  - `Mix`
- 突然変異の有無と確率を選択可能
- 突然変異範囲を選択可能
  - `GlobalRange`
  - `ParentRelative`
- 遺伝子入力元を選択可能
  - `Population`
  - `SavedGenome`
  - `ManagerGenome`
- 世代交代対象を選択可能
  - `Both`
  - `HerbivoreOnly`
  - `PredatorOnly`
- DNA viewer
- 草食 DNA 注入スポーン

### UI / Camera

`WorldUIManager.cs`
- オブジェクトクリック選択
- 選択対象の追従表示
- 対象の status 表示
  - `name`
  - `carbon`
  - `maxCarbon`
  - `category`
  - `health`
  - `dead`
- 草食、肉食の vision wave 可視化
- 世代交代ボタン `Onclickbutton2()` を提供

`WorldUIManager.StateView.cs`
- vision / wander wave 表示
- 草食 DNA copy

`FreeCameraController.cs`
- フリーフライカメラ操作
- 地面への埋まり防止
- 追従対象切り替え後の回転同期

### Support Systems

`threatmap_calc.cs`
- threat score grid
- low threat direction probe
- field evaluation

`ThreatMapsGenerator.cs`
- threatmap 可視化

`PerformanceBudgetMonitor.cs`
- 個体数、フレーム負荷の監視

## Economy Model

現在の資源経済は次の流れです。

### 草 → 草食動物

- herbivore が grass を食べる
- grass の carbon が減る
- herbivore の body carbon が増える

### 草食動物 → 肉食動物

- predator が herbivore を attack する
- herbivore の health が減る
- herbivore が死亡した後、predator が死体を eat する
- herbivore の carbon が predator に移動する

### 個体死亡

- `health <= 0` で死亡状態になる
- 死亡後は移動停止
- `Decompose()` により carbon が環境へ少しずつ戻る
- carbon が空になったら `Destroy(gameObject)`

### 環境

- `ResourceDispenser` が環境全体の炭素を保持
- 草の再生と、死体分解で戻る炭素の受け取りを担当

### Carbon Accounting (2026-03-13)

- 保存対象は `carbon` のみ
- `energy` と `heat` は保存則に含めない
- 世代交代時には炭素会計をリセットする
- 各世代の開始直後に存在する `Resource.carbon` の総和と `carbonPool` の合計を、その世代の `totalCarbon` とする
- その世代の進行中は `sum(Resource.carbon) + carbonPool = totalCarbon` を満たしていれば保存則成立とみなす
- 実装上は、世代更新の開始時に `carbonPool` と `totalCarbon` を 0 に戻し、草と動物の生成完了後に観測値で `totalCarbon` を再確定する

## AI Design

草食、肉食ともに、基本はベクトル合成型の行動設計です。

- 視界
- 記憶
- 食物、獲物方向
- 脅威回避
- 境界回避
- 徘徊

草食は grass を目標にし、肉食は herbivore を目標にします。
遺伝子構造は manager から個体へ配布され、世代交代時には交差、突然変異を通して次世代へ渡されます。

## 実装状況の整理

実装済み:

- 地形
- 植物相
- 草食動物
- 肉食動物
- 草食動物への攻撃
- 草食動物の防御反応
- 寿命と分解
- 炭素ベースのリソース循環
- 世代交代 UI
- DNA ビューア
- 草食 DNA 文字列の注入スポーン
- 対象追跡 UI
- 状態表示 UI
- パフォーマンス監視

部分実装:

- 巡回
  - 草食、肉食ともに wander ベクトルによる徘徊はある
  - 明示的な巡回ルートや縄張り巡回は未実装
- 肉食動物の戦闘アルゴリズム
  - herbivore への attack と死体摂食はある
  - 対肉食、囲い込み、距離管理、集団戦は未実装
- 世代交代
  - 一斉世代交代、交差、突然変異、入力元選択はある
  - UI からの保存、読込や複数親評価は未整備
- 脅威度マップ
  - スクリプトはある
  - AI 主導線への全面統合は未完

未実装:

- 森
- 視界隠蔽
- 群知性
- 魔素の場
- 魔素の拡散と濃度影響
- 属性魔法
- 空間魔法統合
- 支配種進化

## Current Notes

- `AdvanceGenerationController.cs` は世代交代用クラスです
- `WorldUIManager.cs` と組み合わせて UI ボタンから世代交代を実行します
- manager の個体リストは `OnDestroy()` で除去されるようにしています
- `Assembly-CSharp.csproj` はローカルの `dotnet build` 確認のため一時的に更新されることがあります
  - Unity による再生成で上書きされる可能性があります
- `threatmap` 系は存在しますが、現行 AI の中心ロジックではありません
- predator の DNA 文字列 export は草食ほど整備されていません

## Documents

- 実装済み基準: [README.md](/c:/魔法環境シミュ/README.md)
- 今後の計画: [memo/ROADMAP.md](/c:/魔法環境シミュ/memo/ROADMAP.md)
- 設計メモ: [memo/](/c:/魔法環境シミュ/memo)
- 共有用文書: [document/](/c:/魔法環境シミュ/document)
- 編集ルール: [memo/AI_RULES.md](/c:/魔法環境シミュ/memo/AI_RULES.md)
