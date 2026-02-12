/// <summary>
/// プレイヤーの移動ステート
/// <para> Skill,Idle,Dodge,Guard,Damaged,Attackから遷移可能 </para>
/// </summary>
public class PlayerMoveState : IState
{
    private Player player;

    public PlayerMoveState(Player player) {
        this.player = player;
    }

    public void OnStateEnter() {
        // 移動アニメーション開始
        player.PlayerAnimator.SetBool(Player.ANIM_BOOL_MOVE, true);
        // 攻撃と回避に遷移可能にする
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

        // ガード入力 & リキャスト済でガード状態に遷移
        if (player.InputHandler.GuardPressed && player.Recast.IsGuardReady()) {
            player.ChangeState(player.GuardState);
            return;
        }

        // スキル入力 & リキャスト済でスキル状態に遷移
        if (player.InputHandler.SkillPressed && player.Recast.IsSkillReady()) {
            player.ChangeState(player.SkillState);
            return;
        }
    }

    public void OnFixedUpdate() {
        // 移動入力がある間は移動処理を継続
        if (player.MoveCheck()) {
            player.Move(player.MoveSpeed);
        } else {
            // 移動入力がなくなれば待機状態に遷移
            player.ChangeState(player.IdleState);
        }
    }

    public void OnStateExit() {
        // 移動アニメーション停止
        player.PlayerAnimator.SetBool(Player.ANIM_BOOL_MOVE, false);
        player.ResetAllPressed(); // 入力状態リセット
    }
}