using UnityEngine;
/// <summary>
/// エネミーの被弾ステート
/// <para> ノックバック処理をトリガーに遷移 </para>
/// </summary>
public class EnemyDamagedState : IState 
{
    private Enemy enemy;

    public EnemyDamagedState(Enemy enemy) {
        this.enemy = enemy;
    }

    public void OnStateEnter() {
        // 攻撃判定とエフェクトを切る
        enemy.OnToggleHitBoxCollisionEvent(0);
        enemy.AttackEffectOff();

        // ナビメッシュエージェントが有効だとRigitbodyで動かないので一時的に切る
        enemy.Agent.enabled = false;
        // キネマティックにしているためここで一時的に外す
        enemy.Rb.isKinematic = false;

        // 移動・振り向きできないように
        enemy.CanMove = false;
        enemy.CanAttackingLook = false;

        // タイマーリセット
        enemy.DamagedTimer = 0;
    }

    public void OnStateUpdate() {
    }
    public void OnFixedUpdate() {
        // TargetがいなくなるとIdleに戻る
        if (enemy.Target == null) {
            enemy.ChangeState(enemy.IdleState);
            return;
        }

        enemy.DamagedTimer += Time.fixedDeltaTime;

        if (enemy.DamagedTimer >= enemy.DamagedStateDuration) {
            // ターゲットとの距離が攻撃範囲内ならAttackStateに遷移
            if (enemy.IsInAttackRange()) {
                enemy.ChangeState(enemy.AttackState);
            } else {
                enemy.ChangeState(enemy.BattleState);
            }
        }
        
    }
    public void OnStateExit() {
        // 移動可能に戻す
        enemy.Agent.enabled = true;
        enemy.Rb.isKinematic = true;
        enemy.CanMove = true;
        enemy.CanAttackingLook = true;
    }

}
