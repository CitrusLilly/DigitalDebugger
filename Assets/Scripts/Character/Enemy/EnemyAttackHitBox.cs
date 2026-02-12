using UnityEngine;
/// <summary>
/// エネミーの攻撃判定処理クラス
/// AttackHitBoxを継承
/// </summary>
public class EnemyAttackHitBox : AttackHitBox
{
    [Header("EnemyKnockBack")]  // ガードされた際とボールが当たった際のノックバック処理用データ
    [SerializeField] private KnockbackRequest guardedRequest;
    [SerializeField] private KnockbackRequest ballRequest;

    [Header("BallSetting")]
    [SerializeField] private int ballAttack = 10;   // ボール状態で与えるダメージ

    protected override void OnTriggerEnter(Collider other) {
        // 通常状態
        if (ownerCharacter.gameObject.layer == LayerMask.NameToLayer(Enemy.NORMAL_LAYER_NAME)) {
            // プレイヤーに当たれば攻撃処理
            if (other.gameObject.layer == LayerMask.NameToLayer(Player.LAYER_NAME)) {
                PlayerHit(other);
            }
            // ボール状態
        } else if (ownerCharacter.gameObject.layer == LayerMask.NameToLayer(Enemy.BALL_LAYER_NAME)) {
            // エネミーに当たれば攻撃処理
            if (other.gameObject.layer == LayerMask.NameToLayer(Enemy.NORMAL_LAYER_NAME)) {
                EnemyHit(other);
            }
        }
    }

    /// <summary>
    /// プレイヤーに対してのヒット処理
    /// </summary>
    private void PlayerHit(Collider other) {
        // 多重ヒットは処理しない
        if (hitList.Contains(other.gameObject)) return;

        // １回の攻撃での多重ヒットをなくす
        hitList.Add(other.gameObject);

        // ダメージ処理インターフェースでダメージ処理を呼び出す
        IDamageable damageable = other.GetComponent<IDamageable>();
        if(damageable == null) return;

        var result = damageable.TakeDamage(BaseAttack);

        Vector3 hitPos = other.ClosestPoint(attackCollider.bounds.center); // 攻撃hit位置
        // 結果
        switch (result) {
            case DamageReaction.Damaged:
                // ヒット通知
                ownerCharacter.OnAttackHit(hitPos, knockbackRequest.Type);

                // 相手をノックバック
                damageable.KnockBack(knockbackRequest);

                // 自身にヒットストップ
                ownerHitStop.HitStop(hitStopDuration);
                // 相手にヒットストップ
                if (other.TryGetComponent(out IHitStop hitstop)) {
                    hitstop.HitStop(hitStopDuration);
                }

                break;
            case DamageReaction.Guarded:
                // ガードされていたら自分自身のノックバック処理を呼び出して自分がのけぞる
                if(ownerCharacter.TryGetComponent(out IDamageable myDamageable)) {
                    // 起点を相手(プレイヤー)にする
                    guardedRequest.Source = other.transform;
                    myDamageable.KnockBack(guardedRequest);
                }

                break;
            case DamageReaction.Down:
                // ヒット通知
                ownerCharacter.OnAttackHit(hitPos, knockbackRequest.Type);

                break;
            case DamageReaction.Invincible:
                // 無敵は処理しない
                break;
        }
    }

    /// <summary>
    /// エネミーに対してのヒット処理
    /// </summary>
    private void EnemyHit(Collider other) {
        // 多重ヒットは処理しない
        if (hitList.Contains(other.gameObject)) return;

        // １回の攻撃での多重ヒットをなくす
        hitList.Add(other.gameObject);

        // ダメージ処理インターフェースでダメージ処理を呼び出す
        IDamageable damageable = other.GetComponent<IDamageable>();
        if(damageable == null) return;

        var result = damageable.TakeDamage(ballAttack);

        Vector3 hitPos = other.ClosestPoint(attackCollider.bounds.center); // 攻撃hit位置
        // 結果
        switch (result) {
            case DamageReaction.Damaged:
                // ヒット通知
                ownerCharacter.OnAttackHit(hitPos,ballRequest.Type);

                // ノックバック
                damageable.KnockBack(ballRequest);
                break;
            case DamageReaction.DamagedOnly:
            case DamageReaction.Down:
                // エフェクト発生
                ownerCharacter.OnAttackHit(hitPos, ballRequest.Type);

                break;
            case DamageReaction.Smash:
                // スマッシュは処理なし
                break;

        }

    }

    // デバッグ用のギズモ
    private void OnDrawGizmos() {
        // 攻撃用コライダーが有効な場合にのみ当たり判定を可視化
        if (attackCollider != null && attackCollider.enabled) {
            var sphere = GetComponent<SphereCollider>();
            if (sphere == null || this == null) return;     // 消えてから読み込まないようにnullチェック

            Gizmos.color = new Color(1, 0, 0, 0.3f);
            // ギズモの座標をコライダーに合わせる
            Gizmos.matrix = Matrix4x4.TRS(sphere.transform.position, sphere.transform.rotation, sphere.transform.lossyScale);

            // 当たり判定範囲を半透明で塗りつぶして描画
            Gizmos.DrawSphere(sphere.center, sphere.radius);
            // ワイヤーを赤色で描画
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(sphere.center, sphere.radius);
        }
    }

}
