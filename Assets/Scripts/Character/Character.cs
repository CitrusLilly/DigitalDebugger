using UnityEngine;

/// <summary>
/// プレイヤーとエネミー共通仮想クラス
/// </summary>
public abstract class Character : MonoBehaviour
{
    /// <summary>
    /// 自身の攻撃のヒット通知
    /// </summary>
    /// <param name="hitPos"> 攻撃ヒット部分の座標を渡す </param>
    /// <param name="type"> 攻撃ヒットタイプ </param>
    public abstract void OnAttackHit(Vector3 hitPos, AttackHitType type);

    /// <summary>
    /// 死亡時処理
    /// </summary>
    public abstract void CharacterDie();
}
