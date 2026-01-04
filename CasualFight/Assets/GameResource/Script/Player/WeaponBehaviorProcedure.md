# 武器自動出し入れ機能 実装手順書

## 1. 概要
本ドキュメントは、プレイヤー（またはキャラクター）の武器装備挙動に関する仕様と実装手順をまとめたものです。
「戦闘中であっても攻撃を受けるまでは武器を出さず、戦闘中でも一定時間経過で武器をしまう」という挙動を実現します。

## 2. 仕様詳細

### 動作要件
1.  **初期状態 / 通常時**
    *   武器は背中に保持（または非表示）し、手には持たない。
2.  **武器を構える条件 (Equip)**
    *   **トリガー**: 敵からの攻撃を受ける (Damage判定)。
    *   戦闘状態に入っただけ（敵を発見しただけ）では武器を構えない。
3.  **武器をしまう条件 (Unequip)**
    *   **トリガー**: 最後の戦闘アクションから **5秒 ～ 10秒** 経過。
    *   戦闘中ステータスであっても、アクションがなければ自動でしまう。

### パラメータ設定案
*   **AutoSheatheTime**: `5.0f` 〜 `10.0f` (インスペクターで調整可能にする)

## 3. 実装ロジック案

### 必要な変数 (命名規則適用)
```csharp
// 武器を構えているかどうかのフラグ
// メンバ変数は m_ をつけ、bool型は大文字 Is から始める
private bool m_IsWeaponDrawn = false;

// 最後に戦闘アクション（攻撃・被弾）が発生した時刻
private float m_LastCombatActionTime;

// 武器をしまうまでの待機時間（秒）
[SerializeField] private float m_AutoSheatheDuration = 5.0f; // 5秒 or 10秒
```

### フローチャート

1.  **被弾時 (TakeDamage)**
    *   `m_IsWeaponDrawn` が `false` なら、武器を構える処理 (`DrawWeapon()`) を呼ぶ。
    *   `m_LastCombatActionTime` を `Time.time` で更新する。

2.  **攻撃時 (Attack)**
    *   （※仕様には明記されていませんが、自分が攻撃ボタンを押した際も武器を構えるのが自然です。必要に応じて実装）
    *   `m_LastCombatActionTime` を更新する。

3.  **毎フレーム更新 (Update)**
    *   `m_IsWeaponDrawn` が `true` の場合：
        *   ローカル変数 `elapsedTime` 等を用いて計算する場合:
            ```csharp
            float elapsedTime = Time.time - m_LastCombatActionTime;
            if (elapsedTime > m_AutoSheatheDuration) { ... }
            ```
        *   条件を満たせば、武器をしまう処理 (`SheatheWeapon()`) を呼ぶ。

## 4. 実装手順

### Step 1: 変数と設定の追加
対象スクリプト（例: `PlayerController.cs`）に、状態管理フラグとタイマー設定変数を追加します。

### Step 2: 武器表示切替関数の作成
武器の表示/非表示（あるいは親オブジェクトの変更による 背中<->手 の移動）を行う関数を用意します。
*   `void DrawWeapon()`: 手に武器を表示、背中の武器を非表示。アニメーションステートを「Battle」へ。
*   `void SheatheWeapon()`: 手の武器を非表示、背中に武器を表示。アニメーションステートを「Normal」へ。

### Step 3: ダメージ処理へのフック
既存のダメージ処理関数（例: `OnDamage`, `TakeDamage`）内で、`DrawWeapon()` を呼び出し、タイマーをリセットする処理を追加します。

### Step 4: Updateループでの監視
`Update()` メソッド内で経過時間を監視し、タイムアウト時に `SheatheWeapon()` を実行する処理を追加します。

### Step 5: 動作確認
*   敵に近づくだけでは武器を出さないことを確認。
*   攻撃を受けた瞬間に武器を構えることを確認。
*   そのまま放置して設定時間（5秒/10秒）経過後に武器をしまうことを確認。
