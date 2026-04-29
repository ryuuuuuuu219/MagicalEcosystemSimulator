# 1.戦闘可視化強化

## 位置づけ

- 指定個体と、その個体が現在追っている目標を観測しやすくするための UI / カメラ / 場可視化タスク。
- 既存の本線に割り込み、以降のタスク番号を 1 つ後ろへシフトした。
- `4.戦闘システム拡張`、`5.魔素基盤構築`、`6.属性魔法拡張` の検証に使う観測基盤を先に整える。

## 目的

- 指定個体と、その個体の目標・敵対対象・攻撃範囲を見失わないカメラ導線を作る。
- 戦闘時の判断、距離、角度、ターゲット切り替え、攻撃発生を UI で追えるようにする。
- 設計に存在する魔力場、属性場、その他テンソル場を実装・観測できる入口を作る。

## 現状

- 攻撃トレースとダメージ表示は `AttackTraceLibrary` / `DamageNumberLibrary` に実装済み。
- `WorldUIManager` 系には個体リスト、状態表示、仮想ゲージ、選択導線が存在する。
- `ThreatMapsGenerator` はシーン配置済みで、threat map の可視化もある。
- `HeatFieldManager` は heat field の生成・拡散・デバッグ描画を持つ。
- mana / magic / 属性場 / 魔力テンソル場は、設計メモ上の存在が中心で実装は未整理。

## 作業区分

### 1. 指定個体カメラ

- 選択中の個体を中心に追尾するカメラモードを追加する。
- 指定個体と、その個体の `currentTarget` / `trackedPrey` / 目標座標が同時に見えるカメラ位置を計算する。
- ポケモン風に、指定個体を左下、目標個体を右上へ置く追跡カメラを優先する。
- 指定個体から一定距離の球面上にカメラを置き、指定個体・カメラ・目標個体の見かけ角を camera FOV の `0.7f` に近づける。
- カメラの Z 軸回転は使わない。
- 戦闘時は攻撃範囲、対象、移動方向が画面から外れにくい距離・角度へ補正する。
- 詳細: `追跡カメラ設計.md`
- 対象候補:
  - `Assets/script/Ingame/UI/Menu/ingame/UImanager/WorldUIManager.Selection.cs`
  - `Assets/script/Ingame/UI/Menu/ingame/UImanager/WorldUIManager.StateView.cs`
  - `Assets/script/Ingame/Creatures/Herbivore/herbivoreBehaviour.cs`
  - `Assets/script/Ingame/Creatures/Predator/predatorBehaviour.cs`

### 2. 戦闘観測 UI

- 指定個体、現在目標、攻撃可否、クールダウン、距離、角度、脅威値を表示する。
- ログ表示は優先度を下げ、`../X-2.やりたいこと_判断ログ/README.md` へ退避する。
- 攻撃トレース、ダメージ表示、状態表示を同じ観測文脈で見られるようにする。
- 対象候補:
  - `Assets/script/Ingame/Presentation/CombatVisuals/AttackTraceLibrary.cs`
  - `Assets/script/Ingame/Presentation/CombatVisuals/CommonAttackVisualUIManager.cs`
  - `Assets/script/Ingame/Combat/PredatorCombatLibrary.cs`
  - `Assets/script/Ingame/UI/Menu/ingame/UImanager/WorldUIManager.StateView.cs`

### 3. 魔力場・テンソル場の基盤

- 魔力場、属性場、方向性を持つ場、勾配を持つ場をまとめて扱うためのデータ表現を決める。
- 最初は scalar field、vector field、tensor field を分け、UI 表示とシミュレーション更新の責務を混ぜない。
- heat field / threat map と重複する生成処理を増やさず、共通化できる参照・可視化 API を検討する。
- 対象候補:
  - `Assets/script/Ingame/Environment/HeatFieldManager.cs`
  - `Assets/script/Ingame/AI/ThreatMap/threatmap_calc.cs`
  - `Assets/script/Ingame/Environment/ResourceDispenser.cs`
  - 新規: `Assets/script/Ingame/Environment/Fields/`

### 4. 魔力場の戦闘連動

- 魔力場を戦闘や魔法発動の前提値として参照できるようにする。
- 指定個体の周囲の場の値、勾配、影響方向を UI で確認できるようにする。
- 属性魔法導入時に、属性場・魔力場・threat / heat の関係を比較できるようにする。
- 対象候補:
  - `Assets/script/Ingame/Combat/PredatorCombatLibrary.cs`
  - `Assets/script/Ingame/Creatures/Predator/predatorBehaviour.cs`
  - `Assets/script/Ingame/Creatures/Herbivore/herbivoreBehaviour.cs`
  - `Assets/script/Ingame/UI/Menu/ingame/UImanager/WorldUIManager.StateView.cs`

## 作業単位

1. 選択中個体と目標の取得経路を整理する。
2. 指定個体・目標を同時に収めるカメラ位置計算を追加する。
3. 指定個体を左下、目標個体を右上に置く追跡カメラ配置を実装する。
4. 戦闘観測 UI に現在目標、距離、角度、攻撃可否を追加する。
5. 魔力場・属性場・テンソル場のデータ型と更新責務を設計する。
6. 最小の魔力場を実装し、指定個体周辺の値を UI で確認できるようにする。
7. 魔力場の勾配・方向性を戦闘観測カメラと重ねて確認できるようにする。

## 完了条件

- 指定個体と、その個体の現在目標がカメラ上で同時に追える。
- 指定個体が画面左下、目標個体が画面右上に配置される。
- 指定個体と目標個体の見かけ角が camera FOV の `0.7f` 付近になる。
- カメラの Z 軸回転を使わずに構図が成立する。
- 戦闘時の攻撃距離、角度、クールダウン、現在目標が観測できる。
- 既存の attack trace / damage number / state view と矛盾しない表示になっている。
- 魔力場またはテンソル場の最小実装があり、指定個体周辺の値・方向・勾配を確認できる。
- heat field / threat map と重複する場生成処理を追加していない。

## 関連タスク

- `../2.生態系コア強化/README.md`
- `../4.戦闘システム拡張/README.md`
- `../5.魔素基盤構築/README.md`
- `../6.属性魔法拡張/README.md`
- `../X-1.やりたいこと_戦術演出/README.md`
- `../X-2.やりたいこと_判断ログ/README.md`
