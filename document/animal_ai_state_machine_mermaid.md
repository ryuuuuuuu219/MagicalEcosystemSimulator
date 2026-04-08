# 動物AI 状態機械フローチャート

`Mermaid Live Editor` / `Mermaid Live Editor (Fork)` にそのまま貼れるように、コード実装ベースで整理したフローチャートです。

参照元:

- `Assets/script/Ingame/behaviour/herbivore/herbivoreBehaviour.cs`
- `Assets/script/Ingame/behaviour/predator/predatorBehaviour.cs`
- `Assets/script/Ingame/AI/PredatorCombatLibrary.cs`

## 草食動物AI

```mermaid
flowchart TD
    A[Update開始] --> B{bodyResource == null?}
    B -- yes --> Z1[何もしない]
    B -- no --> C{IsDead?}
    C -- yes --> D[死亡状態\n移動停止\n分解処理]
    C -- no --> E[炭素をエネルギーへ変換]
    E --> F[視界更新]
    F --> G[脅威/食料/死骸キャッシュ更新]
    G --> H[ComputeTotalVector]

    H --> I{回避中 or\n近距離捕食者を検知?}
    I -- yes --> J[回避状態\nComputeEvasionVector]
    J --> K[evasionDirection へ移動]
    K --> Y[FixedUpdateで移動適用]

    I -- no --> L[ComputeFoodVector]
    L --> M{currentTargetに十分接近?\ndist < eatDistance}
    M -- yes --> N[採食状態\nEat]
    N --> O[移動ベクトルゼロ]
    O --> Y

    M -- no --> P[脅威/境界/徘徊ベクトル計算]
    P --> Q{脅威重み > escapeThreshold?}
    Q -- yes --> R[逃避優先\nfood,wander重みを減衰]
    Q -- no --> S[通常合成]
    R --> T[food + threat + boundary + wander を合成]
    S --> T
    T --> U{合成結果がほぼゼロ?}
    U -- yes --> V[好奇心ベースの徘徊へフォールバック]
    U -- no --> W[正規化して採用]
    V --> Y
    W --> Y

    Y --> AA{FixedUpdate\nbodyResourceあり && 生存中?}
    AA -- yes --> AB[ApplyMovement]
    AB --> AC[移動コスト消費]
    AC --> AD[ClampRotation]
    AA -- no --> AD
```

### 草食動物AIの見方

- 実装上は明示的な `enum State` ではなく、`ComputeTotalVector()` の優先分岐が状態機械になっています。
- 優先度は `回避 > 採食(接触中) > 重み付き移動合成` です。
- `回避状態` は `isEvading`, `evasionTimer`, `evasionCooldownTimer` で継続管理されています。
- `逃避優先` に入っても完全停止ではなく、境界回避と徘徊を弱めつつ合成移動します。

## 肉食動物AI

```mermaid
flowchart TD
    A[Update開始] --> B{bodyResource == null?}
    B -- yes --> Z1[何もしない]
    B -- no --> C{IsDead?}
    C -- yes --> D[死亡状態\n移動停止\n分解処理]
    C -- no --> E[炭素をエネルギーへ変換]
    E --> F[視界更新]
    F --> G[脅威記憶更新]
    G --> H[ComputeTotalVector]

    H --> I[ComputePreyVector]
    I --> J{追跡可能な prey がいる?}
    J -- no --> K[preyWeight=0\ncurrentTarget=null]
    J -- yes --> L[対象を追跡]
    L --> M{prey が生存中 かつ\nTryCombatActions 成功?}
    M -- yes --> N[攻撃状態\n移動ベクトルゼロ]
    N --> Y[FixedUpdateで移動適用]
    M -- no --> O{prey が死亡済み かつ\n距離 <= eatDistance?}
    O -- yes --> P[摂食状態\nEat]
    P --> Q[移動ベクトルゼロ]
    Q --> Y
    O -- no --> R[追跡ベクトル生成\n生存 prey には PN 補正]

    K --> S[脅威/境界/徘徊ベクトル計算]
    R --> S
    S --> T{脅威重み > 0.5?}
    T -- yes --> U[逃避優先\nprey,wander重みを減衰]
    T -- no --> V[通常合成]
    U --> W[prey + threat + boundary + wander を合成]
    V --> W
    W --> X{移動抑制中?}
    X -- yes --> X1{totalMagnitude > resumeThreshold?}
    X1 -- no --> X2[停止継続]
    X1 -- yes --> X3[移動再開]
    X -- no --> X4{totalMagnitude <= stopThreshold?}
    X4 -- yes --> X5[停止状態へ]
    X4 -- no --> X6[移動継続]
    X2 --> Y
    X3 --> Y
    X5 --> Y
    X6 --> Y

    Y --> AA{FixedUpdate\nbodyResourceあり && 生存中?}
    AA -- yes --> AB[ApplyMovement]
    AB --> AC[移動コスト消費]
    AC --> AD[ClampRotation]
    AA -- no --> AD
```

### 肉食動物AIの見方

- メイン状態は `追跡 / 攻撃 / 死体摂食 / 逃避 / 徘徊 / 停止` の混成です。
- こちらも明示的な状態 enum ではなく、`ComputePreyVector()` と `ComputeTotalVector()` の条件分岐で構成されています。
- `停止状態` は `isMovementSuppressed` で管理され、`stopMoveThreshold` / `resumeMoveThreshold` にヒステリシスがあります。

## 肉食動物の攻撃サブフロー

```mermaid
flowchart TD
    A[TryCombatActions] --> B{live prey を攻撃可能?\nenergy比 > 0.1}
    B -- no --> Z[攻撃しない]
    B -- yes --> C[Charge判定]

    C --> C1{clock経過\n前方象限\nchargeArc内\n接触あり?}
    C1 -- no --> D[Bite判定]
    C1 -- yes --> C2{確率成功?}
    C2 -- no --> D
    C2 -- yes --> C3[Charge実行\nダメージ=速度依存\nenergy消費]
    C3 --> Y[攻撃成功]

    D --> D1{clock経過\n前方象限\nbiteArc内?}
    D1 -- no --> E[Melee判定]
    D1 -- yes --> D2{確率成功?}
    D2 -- no --> E
    D2 -- yes --> D3[Bite実行\n固定ダメージ\n獲物速度を継承]
    D3 --> Y

    E --> E1{clock経過\n前方象限\nmeleeArc内?}
    E1 -- no --> Z
    E1 -- yes --> E2{確率成功?}
    E2 -- no --> Z
    E2 -- yes --> E3[Melee実行\n固定ダメージ\nenergy消費]
    E3 --> Y
```

## 実装メモ

- 草食の回避開始条件は `evasionDistance` 以内に捕食者が入ったときです。
- 草食の採食完了判定は `eatDistance` 未満です。
- 肉食は最寄りの記憶済み prey を選びますが、`disengageDistance` を超える対象は追跡しません。
- 肉食の生存 prey 追跡では `ComputeProportionalNavigationVector()` による迎撃補正を使います。
- 肉食の攻撃優先順は `Charge -> Bite -> Melee` です。

## 予定状態機械（仕様メモベース）

以下は [1.行動意思決定仕様の明文化.txt](C:/魔法環境シミュ/memo/タスク一覧/1.生態系コア強化/1.行動意思決定仕様の明文化.txt) と [設定：状態機械まとめ（IFF前段階）.txt](C:/魔法環境シミュ/memo/設定/設定：状態機械まとめ（IFF前段階）.txt) から拾った、実装予定の状態機械です。

現状実装よりも明示的な `enum` 状態を前提にしていて、主状態は次の 5 つです。

- `Search`
- `Escape`
- `Chase`
- `Attack`
- `Disengage`

### 予定図 全体フロー

```mermaid
flowchart TD
    A[Start / Tick] --> S[Search]

    S --> E[Escape]
    S --> C[Chase]

    E --> S
    E --> D[Disengage]

    C --> A2[Attack]
    C --> E
    C --> D
    C --> S

    A2 --> D
    A2 --> E
    A2 --> C

    D --> S
    D --> E

    S -. enter .-> S1[threatWeight < escapeThreshold
heatWeight < heatAvoidThreshold
有効targetなし or 探索優先]
    E -. enter .-> E1[threatWeight >= escapeThreshold
or evasionW > 0
or heatWeight >= heatAvoidThreshold
or boundaryWeight >= boundaryAvoidThreshold]
    C -. enter .-> C1[currentTargetあり or 新規target獲得
foodWeight > chaseEnterFoodThreshold
or preyWeight > chaseEnterPreyThreshold
and threatWeight < chaseBlockThreatThreshold]
    A2 -. enter .-> A1[Chase中に攻撃条件成立
距離 / 角度 / cooldown / エネルギー条件OK]
    D -. enter .-> D1[Chase or Attack中に
threatWeight >= disengageThreatThreshold
or heatWeight >= disengageHeatThreshold
or 近距離優勢が崩れる]
```

### 予定図 遷移条件詳細

```mermaid
flowchart LR
    Search[Search] -->|foodWeight > searchExitFoodThreshold| Chase[Chase]
    Search -->|preyWeight > searchExitPreyThreshold| Chase
    Search -->|threatWeight >= escapeThreshold| Escape[Escape]
    Search -->|heatWeight >= heatAvoidThreshold| Escape

    Escape -->|threatWeight < escapeThreshold - escapeHysteresis
and evasionTimer <= 0
and heatWeight < heatAvoidThreshold - heatHysteresis
and boundaryWeight < boundaryAvoidThreshold - boundaryHysteresis| Search
    Escape -->|逃走後も近距離圧力が残る| Disengage[Disengage]

    Chase -->|TryCombatActions可能| Attack[Attack]
    Chase -->|target喪失| Search
    Chase -->|threatWeight >= chaseBlockThreatThreshold| Escape
    Chase -->|heatWeight >= heatAvoidThreshold| Escape
    Chase -->|近距離危険 / 包囲 / 優勢崩れ| Disengage

    Attack -->|攻撃完了 / cooldown待ち| Chase
    Attack -->|threatWeight が高い| Escape
    Attack -->|近距離不利 / 反撃圧力 / エネルギー不足| Disengage

    Disengage -->|threatWeight < disengageThreatThreshold - disengageHysteresis
and heatWeight < disengageHeatThreshold - disengageHeatHysteresis
and 再交戦条件なし| Search
    Disengage -->|再び threat / heat / boundary 圧力増加| Escape
```

### 予定図 ベクトル統合との関係

```mermaid
flowchart TD
    A[知覚入力] --> B[food / prey]
    A --> C[threat]
    A --> D[boundary]
    A --> E[heat]
    A --> F[threat map]
    A --> G[wander]

    B --> H{State Decision}
    C --> H
    D --> H
    E --> H
    F --> H
    G --> H

    H --> S[Search]
    H --> X[Escape]
    H --> C2[Chase]
    H --> A2[Attack]
    H --> D2[Disengage]

    S --> VS[wander + boundary + weak target attract]
    X --> VX[threat + threat map + heat + boundary]
    C2 --> VC[target pursuit + memory + weak threat reject]
    A2 --> VA[attack arc / cooldown / local commit]
    D2 --> VD[threat + heat + boundary + energy recovery bias]

    VS --> M[AnimalAICommon.ApplyMovement]
    VX --> M
    VC --> M
    VA --> M
    VD --> M
```

### 仕様メモから読める意図

- 現状の「重み付き合成で擬似的に状態を作る」方式から、予定では `Search / Escape / Chase / Attack / Disengage` を明示状態として持つ方向です。
- `Escape`, `Disengage` にはヒステリシス付きの exit 条件があり、状態のバタつきを抑える想定です。
- `threat map` は主に `Escape` と `Disengage` の補助入力、`heat field` は将来的に `Escape` 側へ強く効かせる想定に見えます。
- `IFF` 導入後は `food/prey/threat` の解釈が所属関係で切り替わる前提です。
