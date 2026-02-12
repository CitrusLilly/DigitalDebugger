using UnityEngine;

/// <summary>
/// ノックバック処理用データ
/// </summary>
[System.Serializable]
public class KnockbackRequest
{
    public Transform Source;       // 攻撃者のTransform
    public Vector3 Direction;      // ノックバック方向を直接指定する場合
    public float Power;            // ノックバック値
    public float DownDuration;     // ダウン時間
    public AttackHitType Type;     // 攻撃ヒットタイプ
}
