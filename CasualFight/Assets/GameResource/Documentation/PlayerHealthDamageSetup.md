# プレイヤーのHPとダメージ処理実装 手順書

このドキュメントは、プレイヤーのHP管理、ダメージ時の処理、およびUI更新を実装するための手順をまとめたものです。

## 1. プレイヤーコントローラー (`PlayerController.cs`) の拡張

`PlayerController.cs` にHPに関連する変数と処理を追加します。

### 1-1. 変数の追加
クラスのメンバ変数として以下を追加します：

```csharp
[Header("HP設定")]
[SerializeField] int m_MaxHP = 100;
int m_CurrentHP;

[Header("ダメージ設定")]
[SerializeField] float m_InvincibleTime = 1.0f; // ダメージ後の無敵時間
bool m_IsInvincible = false;

[SerializeField] HPBarController m_HPBar; // UI参照
```

### 1-2. 初期化 (`Awake` または `Start`)
現在のHPを最大値に設定します：

```csharp
private void Start()
{
    m_CurrentHP = m_MaxHP;
}
```

### 1-3. `TakeDamage` メソッドの更新
既存の `TakeDamage` メソッドを以下のように拡張します：

```csharp
public void TakeDamage(int damage)
{
    if (m_IsInvincible || m_CurrentHP <= 0) return;

    // 武器を構える（既存処理）
    m_WeaponSwitch?.DrawWeapon();

    // HP減少
    m_CurrentHP = Mathf.Max(m_CurrentHP - damage, 0);

    // UI更新 (0.0 ～ 1.0 の割合で渡す)
    if (m_HPBar != null)
    {
        m_HPBar.OnTakeDamage((float)m_CurrentHP / m_MaxHP);
    }

    // 死亡判定
    if (m_CurrentHP <= 0)
    {
        Die();
    }
    else
    {
        // 無敵時間開始
        StartInvincibility().Forget();
    }
}

private void Die()
{
    Debug.Log("Player Died");
    m_Animator.SetTrigger("Die"); // 死亡アニメーションがある場合
    // ゲームオーバー処理など
}

private async UniTaskVoid StartInvincibility()
{
    m_IsInvincible = true;
    await UniTask.Delay(System.TimeSpan.FromSeconds(m_InvincibleTime));
    m_IsInvincible = false;
}
```

---

## 2. UIのセットアップ (`HPBarController.cs`)

`HPBarController` はスライダー（ForegroundとBackground）を制御します。

1. **Hierarchy上の配置**: `Canvas` 内にHPバーのUIオブジェクトを配置します。
2. **コンポーネントのアタッチ**: オブジェクトに `HPBarController` をアタッチします。
3. **参照の設定**: `Foreground Bar` と `Background Bar` にそれぞれの `Slider` コンポーネントをドラッグ＆ドロップします。
4. **PlayerControllerへの紐付け**: `PlayerController` の `m_HPBar` フィールドに、この `HPBarController` をアタッチしたオブジェクトを設定します。

---

## 3. 動作確認 (`DamageTester.cs`)

テスト用の `DamageTester` を使用して、キー入力でダメージが発生するか確認します。

1. **テスト用オブジェクト**: `DamageTester` コンポーネントを持つ空のGameObjectを作成します。
2. **参照の設定**: `m_HealthBarController` にUIの `HPBarController` を設定します。
3. **実行**: ゲーム再生中に `P` キー（または設定したキー）を押し、以下の点を確認します：
   - HPバーが減る（前方バーが即座に、後方バーが遅れて減る）。
   - プレイヤーが武器を抜く（`TakeDamage` の既存処理）。

---

## 4. 注意事項

- **無敵時間の表現**: 無敵時間中にプレイヤーを点滅させるなどの視覚的フィードバックを追加することを検討してください。
- **ダメージ値のバランス**: 敵の攻撃力に合わせて `m_MaxHP` を調整してください。
- **レイヤー設定**: 敵の攻撃判定（Collider/Trigger）がプレイヤーの `TakeDamage` を適切に呼び出せるよう、TagやLayerの設定を確認してください。
