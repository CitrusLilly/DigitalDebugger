/// <summary>
/// プレイヤーの静止ステート
/// <para> Skill,Move,Dodge,Guard,Damaged,Attackから遷移可能 </para>
/// </summary>
public class PlayerIdleState : IState
{
    private Player player;

    public PlayerIdleState(Player player) {
        this.player = player;
    }

    public void OnStateEnter() {
        // 攻撃と回避を可能にする
        player.CanAttack = true;
        player.CanDodge = true;
    }

    public void OnStateUpdate() {
        // 回避入力があれば回避状態に遷移
        if (player.InputHandler.DodgePressed) {
            player.ChangeState(player.DodgeState);
            return;
        }

        // 攻撃入力があれば攻撃状態に遷移
        if (player.InputHandler.AttackPressed) {
            player.ChangeState(player.AttackState);
            return;
        }

        // ガード入力&リキャスト済でガード状態に遷移
        if (player.InputHandler.GuardPressed && player.Recast.IsGuardReady()) {
            player.ChangeState(player.GuardState);
            return;
        }

        // スキル入力&リキャスト済でスキル状態に遷移
        if (player.InputHandler.SkillPressed && player.Recast.IsSkillReady()) {
            player.ChangeState(player.SkillState);
            return;
        }

        // 移動入力があれば移動状態に遷移
        if (player.MoveCheck()) {
            player.ChangeState(player.MoveState);
            return;
        }
    }

    public void OnFixedUpdate() {
        // 未使用
    }

    public void OnStateExit() {
        player.ResetAllPressed(); // 入力状態リセット
    }
}
