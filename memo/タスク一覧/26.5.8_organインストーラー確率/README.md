# 26.5.8 organ インストーラー確率

作成日: 2026-05-08

## 目的

`AnimalAIInstaller` が各 organ コンポーネントを有効化する確率を、実装前に把握できるようにする。

現時点のコードでは、`AnimalAIInstaller.InstallDefaultOrgans()` は以下だけを常時導入する。

```text
AnimalBrain
AIMemoryStore
GroundMotor
CreatureMotorBootstrap
```

このフォルダでは、今後 `AIComponentGene.enabled` を生成するときの基準確率を整理する。

## ファイル

- `organ_definition.md`
  - organ の定義と、ここまでの読み取り事項。
- `component_probability_table.md`
  - コンポーネントごとの導入確率表。
- `installer_probability_policy.md`
  - 確率設計のルール、固定導入、自動導入、species別 default set の考え方。

## 前提

- 確率は初期案。実装後にシミュレーション結果を見て調整する。
- Core / Motor / Memory の一部は生存に必要なので、確率抽選ではなく固定導入にする。
- `ThreatPulseEmitter` は gene の直接対象ではなく、攻撃 organ が入った場合に自動導入する。
- `ZigzagEvasionSteering` は廃止予定なので対象外。
