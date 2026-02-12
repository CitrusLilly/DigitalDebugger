using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// プレイヤーエネミー共通の攻撃判定用クラス
/// </summary>
public abstract class AttackHitBox : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] protected Character ownerCharacter;        // この攻撃を所有するキャラクター
    [SerializeField] protected Transform characterTransform;    // キャラクターのTransform
    [SerializeField] protected Collider attackCollider;         // 当たり判定をON/OFFするために取得
    protected IHitStop ownerHitStop;                            // 自身のヒットストップインターフェース

    [Header("hitStop")]
    [SerializeField] protected float hitStopDuration = 0.02f;   // 攻撃のヒットストップ時間(秒)

    [Header("KnockBack")]                                       // ノックバック処理用データ
    [SerializeField] protected KnockbackRequest knockbackRequest;

    public int BaseAttack { get; set; } // 基礎攻撃力

    // 既にヒットしたオブジェクトを入れる(多段ヒット防止用)
    protected List<GameObject> hitList = new();

    protected virtual void Start() {
        // 自身のヒットストップインターフェースを取得
        ownerHitStop = ownerCharacter.GetComponent<IHitStop>();
        // 攻撃判定を初期は無効にする
        attackCollider.enabled = false;
        hitList.Clear();
    }

    /// <summary>
    /// 攻撃判定有効化
    /// </summary>
    public void EnableCollider() {
        DisableCollider();
        hitList.Clear();
        attackCollider.enabled = true;
    }

    /// <summary>
    /// 攻撃判定無効化
    /// </summary>
    public virtual void DisableCollider() {
        attackCollider.enabled = false;
    }

    // ダメージ付与・ノックバック・ヒット通知などを行う(派生クラスで実装する)
    protected abstract void OnTriggerEnter(Collider other);

}
