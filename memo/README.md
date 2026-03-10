# 魔法環境シミュ (Magical Ecosystem Simulator)

Unity で作成している生態系シミュレーションです。
ランダム生成された地形の上で、草、草食動物、肉食動物、環境資源の循環を観察できます。

---

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

---

## 現状 (2026-03-08)

直近の実装状態は次のとおりです。

- 世代交代コントローラは `Random / Carbon / Health / Selection` の評価軸に対応
- 世代交代 UI で「世代進行」「DNA ビューア」「DNA 注入（文字列からスポーン）」を切り替え可能
- 遺伝子ビューアは `Phase` と `Generation page` のページング付きで、表示 DNA を manager バケットへ反映可能
- 草食動物は近接脅威に対して回避行動（evasion angle/duration/cooldown、zigzag）を実装済み
- UI 側は対象追従カメラ、オブジェクト一覧、ステータス表示（carbon/health/dead）を連携済み
- 草食・肉食とも死亡後は分解で炭素を環境へ返却するループを維持
- ルートディレクトリに `.git` は無く、現時点で git の変更差分管理は未利用

---

## Main Systems

### World Generation

`WorldGenerator.cs`
- seed ベースで地形を生成
- Terrain と water を自動生成
- 見えない壁と天井/床を配置
- 地形色と水色を seed から決定

`grassland.cs`
- 草地パッチを表現
- 一定範囲に植物クアッドを配置
- 地形傾斜と水位を見て植物を生成

### Resource System

`Resource.cs`
- 各オブジェクトの carbon / maxCarbon を管理
- 摂食による炭素移動
- 炭素追加、炭素除去、空判定

`ResourceDispenser.cs`
- 環境全体の総炭素量を管理
- 草、草食動物、肉食動物に初期炭素を配布
- 草の再生を担当
- 分解で戻ってきた炭素を環境へ再回収
- 世代交代時の再スポーン資源初期化を担当

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
- 自身も health を持ち、死亡後は分解される

### Generation System

`AdvanceGenerationController.cs`
- 専用 UI から一斉世代交代を実行
- 既存個体を全削除し、次世代個体を再生成
- 評価軸を選択可能
  - Random
  - Carbon
  - Health
- 交差の有無を選択可能
- 交差形式を選択可能
  - Assign
  - Average
  - Interpolate
  - Mix
- 突然変異の有無と確率を選択可能
- 突然変異範囲を選択可能
  - GlobalRange
  - ParentRelative
- 遺伝子入力元を選択可能
  - Population
  - SavedGenome
  - ManagerGenome
- 世代交代対象を選択可能
  - Both
  - HerbivoreOnly
  - PredatorOnly

### UI / Camera

`WorldUIManager.cs`
- オブジェクトクリック選択
- 選択対象の追従表示
- 対象の status 表示
  - name
  - carbon
  - maxCarbon
  - category
  - health
  - dead
- 草食/肉食の vision wave 可視化
- 世代交代ボタン `Onclickbutton2()` を提供

`FreeCameraController.cs`
- フリーフライカメラ操作
- 地面への埋まり防止
- 追従対象切り替え後の回転同期

---

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
- health <= 0 で死亡状態になる
- 死亡後は移動停止
- `Decompose()` により carbon が環境へ少しずつ戻る
- carbon が空になったら `Destroy(gameObject)`

### 環境
- `ResourceDispenser` が環境全体の炭素を保持
- 草の再生と、死体分解で戻る炭素の受け取りを担当

---

## AI Design

草食・肉食ともに、基本はベクトル合成型の行動設計です。

- 視界
- 記憶
- 食物/獲物方向
- 脅威回避
- 境界回避
- 徘徊

草食は grass を目標にし、肉食は herbivore を目標にします。
遺伝子構造は manager から個体へ配布され、世代交代時には交差・突然変異を通して次世代へ渡されます。

---

## Current Notes

- `AdvanceGenerationController.cs` は新規追加された世代交代用クラスです
- `WorldUIManager.cs` と組み合わせて UI ボタンから世代交代を実行します
- manager の個体リストは `OnDestroy()` で除去されるようにしています
- `Assembly-CSharp.csproj` はローカルの `dotnet build` 確認のため一時的に更新されています
  - Unity による再生成で上書きされる可能性があります

## 実装確認メモ (2026-03-11)

主要スクリプトを再確認した結果、次を実装済みとして扱ってよいです。

- `herbivoreBehaviour.cs`
  - 草探索
  - predator 脅威記憶
  - 近接回避
  - zigzag evasion
  - 死亡後分解
- `predatorBehaviour.cs`
  - prey 記憶
  - threat 記憶
  - 接近、攻撃、死体摂食
  - 死亡後分解
- `AdvanceGenerationController.cs`
  - `Random / Carbon / Health / Selection`
  - `Population / SavedGenome / ManagerGenome`
  - DNA viewer
  - 草食 DNA 注入スポーン
- `WorldUIManager.cs`
  - follow UI
  - object list
  - state view
  - herbivore DNA copy
- `threatmap_calc.cs`
  - threat score grid
  - low threat direction probe
  - field evaluation
  - ただし主 AI 導線への全面統合は未完

---

## Roadmap

現在の予定表を、実装状況ベースで整理すると次のようになります。

### 実装済み

- 地形 ✓
- 植物相 ✓
- 草食動物 ✓
- 肉食動物 ✓
- 草食動物への攻撃 ✓
- 草食動物の防御反応 ✓
- 寿命と分解 ✓
- リソース ✓

### 部分実装

- 巡回
  - 草食 / 肉食ともに wander ベクトルによる徘徊はある
  - 明示的な巡回ルートや縄張り巡回は未実装
- 肉食動物の戦闘アルゴリズム
  - herbivore への attack と死体摂食はある
  - 対肉食、囲い込み、距離管理、集団戦は未実装
- 世代交代
  - 一斉世代交代、交差、突然変異、入力元選択はある
  - UI からの保存/読込や複数親評価は未整備

### 未実装

- 菌類
- 魔法
- 支配種への進化
- 支配種の行動系
- 空間魔法によるスタック脱出
- 空間魔法を含めた戦闘アルゴリズム
- 魔素の拡散
- 魔素の濃度と影響

---

## Recommended Next Steps

現状から次に入れる価値が高い順に並べると、以下が推奨です。

### 1. 巡回を「記憶ベース」から「縄張りベース」に拡張

- 個体ごとにホーム座標を持つ
- 一定半径内を巡回する
- 食料不足や脅威時だけ巡回を中断する

これを入れると、wander が単なるノイズ移動ではなくなり、行動観察がかなり分かりやすくなります。

### 2. 死体フェーズを明示化

- `Alive`
- `Dying`
- `Corpse`
- `Decomposed`

今は `health <= 0` で即分解フェーズに入っていますが、死体状態を明示すると以下がやりやすくなります。

- 肉食動物の優先摂食
- 菌類や腐食者の追加
- 見た目変更
- UI 表示

### 3. 肉食動物の戦闘アルゴリズムを段階化

- 接近
- 攻撃
- クールダウン
- 再接近
- 離脱

さらに以下を追加候補とするのが自然です。

- 低体力時の離脱
- 複数 predator 時のターゲット共有
- 死体優先と生体優先の切り替え
- 対 predator 戦闘

### 4. 菌類を「死体処理者」として追加

菌類はこのプロジェクトにかなり相性が良いです。

- 死体の近くで発生
- carbon を環境へ返す速度を上げる
- 草地に nutrient ボーナスを返す
- 魔素と結びつける媒介層にする

最初の最小構成はこれで十分です。

- `fungusManager`
- `fungusBehaviour`
- `Corpse` を検知して分解速度を上げる

### 5. 魔素システムを先に「場」として作る

魔法を個体行動へ直接入れる前に、魔素の場を作る方が設計が安定します。

推奨順:

1. 地形上に `mana density` を持つ 2D グリッドを作る
2. 拡散
3. 吸収
4. 個体能力への補正
5. 魔法発動

これなら以下を全部同じ基盤に乗せられます。

- 支配種進化
- 魔法戦闘
- 空間魔法
- 環境濃度差

---

## Magic Notes

現時点の見た目案を、実装しやすさベースで整理すると次のようになります。

### 氷

- 半透明メッシュ
- 六角柱 + 六角錐 2 個の結晶構造
- Shader Graph でフレネルと透過を付けるのが扱いやすい

推奨追記:

- 接触時に移動速度低下
- 足元に氷面判定
- `carbon` ではなく `temperature` や `freeze stack` を持たせる案

### 火

- 重力を負数にしたパーティクル
- 上昇炎として見せやすい

推奨追記:

- 範囲継続ダメージ
- 分解速度上昇
- 草地への延焼

### 雷

- LineRenderer
- 瞬間攻撃に向く

推奨追記:

- 即時ダメージ
- スタン
- 近傍連鎖

### 風

- 不可視でも成立する
- 実体より力ベクトルで表現する方が自然

推奨追記:

- ノックバック
- 匂い / 視界 / 魔素の拡散補正
- 群れの隊列崩し

### 空間

- カスタムシェーダー案は相性が良い
- 見た目だけでなく「位置補正」とセットで使うべき

推奨追記:

- スタック脱出専用の短距離テレポート
- Nav 回避ではなく物理押し出しの最終手段
- 戦闘用には「対象との距離を強制変更する魔法」として拡張

---

## Recommended Implementation Order

魔法まで見据えるなら、次の順が安定です。

1. 巡回を縄張り化
2. 死体フェーズの明示化
3. 菌類追加
4. 肉食戦闘アルゴリズムの強化
5. 魔素グリッドの追加
6. 魔素の拡散と濃度影響
7. 空間魔法によるスタック脱出
8. 属性魔法の追加
9. 支配種進化

---

## Current Notes

- `AdvanceGenerationController.cs` は新規追加された世代交代用クラスです
- `WorldUIManager.cs` と組み合わせて UI ボタンから世代交代を実行します
- manager の個体リストは `OnDestroy()` で除去されるようにしています
- `Assembly-CSharp.csproj` はローカルの `dotnet build` 確認のため一時的に更新されています
  - Unity による再生成で上書きされる可能性があります
