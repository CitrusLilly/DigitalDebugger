/// <summary>
/// エネミーのバトルステート。追跡行動をする。
/// <para> Idle,Attack,Downから遷移 </para>
/// </summary>
public class EnemyBattleState : IState
{
    private Enemy enemy;

    public EnemyBattleState(Enemy enemy) {
        this.enemy = enemy;
    }

    public void OnStateEnter() {
        enemy.EnemyAnimator.SetBool(Enemy.ANIM_BOOL_BATTLE, true); // バトルアニメーションON
        enemy.Agent.speed = enemy.MoveSpeed;    // 通常移動速度に変更
        enemy.Agent.isStopped = false;          // 移動停止を解除
        enemy.CanMove = true;                   // 移動を許可
    }

    public void OnStateUpdate() {
        // TargetがいなくなるとIdleに戻る
        if (enemy.Target == null) {
            enemy.ChangeState(enemy.IdleState);
            return;
        }

        // ターゲットとの距離が攻撃範囲内ならAttackStateに遷移
        if (enemy.IsInAttackRange()) {
            enemy.ChangeState(enemy.AttackState);
        } else {
            // 攻撃範囲外ならターゲットに向かって追跡
            enemy.Agent.destination = enemy.Target.transform.position;
        }
    }

    public void OnFixedUpdate() {
        // 未使用
    }
    public void OnStateExit() {
        // 未使用
    }
}
