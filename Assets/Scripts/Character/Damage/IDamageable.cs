/// <summary>
/// ダメージ処理インターフェース(攻撃者から呼び出す)
/// </summary>
public interface IDamageable
{
    /// <summary>
    /// ダメージ処理
    /// </summary>
    /// <param name="damage">与えるダメージ</param>
    /// <returns> ダメージを与えた結果をDamageReactionで返す </returns>
    DamageReaction TakeDamage(int damage);

    /// <summary>
    /// ノックバック処理
    /// </summary>
    /// <param name="request"> ノックバック処理データ </param>
    void KnockBack(KnockbackRequest request);
}