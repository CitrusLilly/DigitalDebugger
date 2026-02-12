using UnityEngine;
/// <summary>
/// エネミーのダウン中のステート(ボール形態)
/// <para> HPが0になりダウン処理で呼ばれた際に遷移 </para>
/// </summary>
public class EnemyDownState : IState
{
    private Enemy enemy;
    private float downTimer; // ダウン中の経過時間タイマー

    public EnemyDownState(Enemy enemy) {
        this.enemy = enemy;
    }

    public void OnStateEnter() {
        // 攻撃判定とエフェクトを切る
        enemy.OnToggleHitBoxCollisionEvent(0);
        enemy.AttackEffectOff();

        // ダウン状態のアニメーション設定
        if (enemy.EnemyAnimator != null) {
            enemy.EnemyAnimator.SetTrigger(Enemy.ANIM_TRIGGER_DOWN);
            enemy.EnemyAnimator.ResetTrigger(Enemy.ANIM_TRIGGER_REBOOT);
        }

        // 軌跡を有効化
        enemy.SwitchTrail(true);

        enemy.Attacking = false;// 攻撃中をOFF
        enemy.CanMove = false;  // 自身での移動不可
        enemy.IsRolled = false; // この時点ではまだ転がされていない
        downTimer = 0;          // タイマーのリセット
        if (enemy.Agent != null) {
            enemy.Agent.enabled = false; // AIの無効化
        }
    }

    public void OnStateUpdate() {
        // 未使用
    }

    // 転がっている処理を物理演算で行うためこちらで処理
    public void OnFixedUpdate() {
        downTimer += Time.fixedDeltaTime;

        // 現在の速度を保存
        enemy.LastVelocity = enemy.Rb.linearVelocity;

        // 再度転がされたらタイマーをリセットして状態もリセット
        if (enemy.IsRolled) {
            downTimer = 0;
            enemy.IsRolled = false;
        }

        // 再起動機能のないエネミーは以降の復帰処理なし(BrokenEnemy)
        if (!enemy.IsReboot) return;

        // ボールになって指定時間が過ぎたら復活処理
        if (downTimer >= enemy.DownDuration) {
            // ターゲットとの距離に応じて各ステートに遷移
            if (enemy.IsInAttackRange()) {
                enemy.ChangeState(enemy.AttackState);
            } else {
                enemy.ChangeState(enemy.BattleState);
            }
        }
    }

    public void OnStateExit() {
        // 当たり判定を無効化
        enemy.OnToggleHitBoxCollisionEvent(0);

        // AIを有効にして移動可能にする
        if (enemy.Agent != null) {
            enemy.Agent.enabled = true;
        }
        // 回転を固定、物理挙動の影響を受けないようにする
        enemy.Rb.constraints = RigidbodyConstraints.FreezeRotation;
        enemy.Rb.isKinematic = true;

        // 転がり終了
        enemy.IsRolled = false;

        // 再起動アニメーション再生
        if (enemy.EnemyAnimator != null) {
            enemy.EnemyAnimator.SetTrigger(Enemy.ANIM_TRIGGER_REBOOT);
        }
        // 軌跡の無効化
        enemy.SwitchTrail(false);
        // HPのリセット
        enemy.OnReboot();
        // レイヤーを戻す(衝突判定の復帰)
        enemy.gameObject.layer = LayerMask.NameToLayer(Enemy.NORMAL_LAYER_NAME);
    }
}
