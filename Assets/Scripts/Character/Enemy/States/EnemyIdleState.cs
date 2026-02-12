/// <summary>
/// エネミーの静止(ターゲットがいない時の)ステート
/// <para>
/// 初期値として入るが、基本はすぐにバトルに移行する。
/// ターゲットがいなくなればこのステートに入る。
/// </para>
/// </summary>
public class EnemyIdleState : IState
{
    private Enemy enemy;

    public EnemyIdleState(Enemy enemy) {
        this.enemy = enemy;
    }

    public void OnStateEnter() {
        if (enemy.EnemyAnimator != null) {
            enemy.EnemyAnimator.SetBool(Enemy.ANIM_BOOL_BATTLE, false); // バトル状態の解除
        }
    }

    public void OnStateUpdate() {
        // 攻撃対象が追加されるとバトル状態に移行
        if (enemy.Target != null) {
            enemy.ChangeState(enemy.BattleState);
        }
    }

    public void OnFixedUpdate() {
        // 未使用
    }
    public void OnStateExit() {
        // 未使用
    }
}
