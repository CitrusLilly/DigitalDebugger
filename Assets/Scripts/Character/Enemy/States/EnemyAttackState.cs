/// <summary>
/// エネミーの攻撃中のステート
/// <para> Battle,Downから遷移 </para>
/// </summary>
public class EnemyAttackState : IState
{
    private Enemy enemy;

    public EnemyAttackState(Enemy enemy) {
        this.enemy = enemy;
    }

    public void OnStateEnter() {
        // ナビメッシュの移動を停止
        if (enemy.Agent.isOnNavMesh) {
            enemy.Agent.isStopped = true;
        }
        enemy.CanMove = false;          // 移動を不可
        enemy.CanAttackingLook = true;  // ターゲットを方を向く挙動を有効化
        enemy.Attack();                 // 攻撃行動を開始
    }

    public void OnStateUpdate() {
        // TargetがいなくなるとIdleに戻る
        if (enemy.Target == null) {
            enemy.ChangeState(enemy.IdleState);
            return;
        }

        // ターゲットが攻撃範囲内にいれば攻撃を継続
        if (enemy.IsInAttackRange()) {
            enemy.Attack();
        } else if(enemy.CanMove) {
            // 攻撃範囲外かつ移動可能なら、BattleState(追跡)状態に遷移
            // CanMoveはアニメーションイベントで制御している
            enemy.ChangeState(enemy.BattleState);
        }
    }

    public void OnFixedUpdate() {
        // 未使用
    }

    public void OnStateExit() {
        if (enemy.EnemyAnimator != null) {
            enemy.EnemyAnimator.SetBool(Enemy.ANIM_BOOL_ATTACK, false); // 攻撃アニメーションの終了
        }
        enemy.AttackEffectOff();        // 攻撃エフェクトの停止
        enemy.CanAttackingLook = false; // 攻撃中の振り向きを無効化
    }
}
