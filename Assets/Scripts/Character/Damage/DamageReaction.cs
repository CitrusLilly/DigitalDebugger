/// <summary>
/// 攻撃を受けた時に相手に返すリアクション
/// </summary>
public enum DamageReaction
{
    Damaged,    // ダメージ + ノックバック
    DamagedOnly,// ダメージのみ
    Smash,      // 吹き飛ばし
    Guarded,    // ガードされる
    Invincible, // 無敵中
    Down        // ダウン
}