using UnityEngine;
/// <summary>
/// 落下検知用クラス
/// プレイヤーやエネミーが落下した際の処理を行う
/// </summary>
public class FallDetector : MonoBehaviour
{
    // 定数 SE名
    private const string FALL_SE_NAME = "shock";

    [Header("Setting")]
    [SerializeField] private int fallDamage = 3;            // プレイヤーが落下時に受けるダメージ
    [SerializeField] private float hitStopDuration = 0.5f;  // 落下時のヒットストップ時間(秒)

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag(Player.TAG_NAME)) {
            PlayerFall(other);
        } else if(other.CompareTag(Enemy.TAG_NAME)) {
            EnemyFall(other);
        }
    }

    /// <summary>
    /// プレイヤー落下時処理
    /// </summary>
    private void PlayerFall(Collider playerCollider) {
        // プレイヤーはダメージを受けて設定されているスポーンポイントまで戻る
        IDamageable player = playerCollider.gameObject.GetComponent<IDamageable>();

        if (player != null) {
            // ダメージ処理
            var result = player.TakeDamage(fallDamage);

            if (result == DamageReaction.Damaged) {
                // ヒットストップ
                if (playerCollider.TryGetComponent(out IHitStop hitStop)) {
                    hitStop.HitStop(hitStopDuration);
                }
            }

            // スポーン位置に戻す
            playerCollider.gameObject.transform.position = GameManager.Instance.SpawnPoint;
            // 落下時SE再生
            AudioManager.Instance.PlaySE(FALL_SE_NAME);
        }
    }

    /// <summary>
    /// エネミー落下時処理(基本は落ちないようにしている)
    /// </summary>
    private void EnemyFall(Collider enemyCollider) {
        // エネミーをスポーン地点まで戻す
        Enemy enemy = enemyCollider.gameObject.GetComponent<Enemy>();
        if (enemy != null) {
            enemy.Warp();
        }
    }
}
