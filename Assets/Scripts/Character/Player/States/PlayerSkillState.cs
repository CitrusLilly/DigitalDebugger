/// <summary>
/// プレイヤーのスキルステート
/// <para> Move,Idle,Attackから遷移可能 </para>
/// </summary>
public class PlayerSkillState : IState
{
    private Player player;

    public PlayerSkillState(Player player) {
        this.player = player;
    }

    public void OnStateEnter() {
        // スキルアニメーション再生
        player.PlayerAnimator.SetTrigger(Player.ANIM_TRIGGER_SKILL);

        // スキル状態のフラグ初期化
        player.SkillStateFinish = false;
        player.CanSkill = false;

        player.CanDodge = true; // スキル中は回避可能
        player.Invincible = true; // スキル中は無敵

        // スキル使用を武器に通知()
        player.Weapon.SetCurrentCombo(player.CurrentCombo, true);
        // スキルSE再生
        AudioManager.Instance.PlaySE(Player.SKILL_SE_NAME);
    }

    public void OnStateUpdate() {
        // 回避入力があれば回避状態に遷移
        if (player.InputHandler.DodgePressed) {
            player.ChangeState(player.DodgeState);
        }

        // ガード入力&リキャスト済でガードに遷移
        if (player.InputHandler.GuardPressed && player.Recast.IsGuardReady()) {
            player.ChangeState(player.GuardState);
            return;
        }

        // スキルアニメーション終了で各ステートに遷移
        if (player.SkillStateFinish) {

            // 移動入力の有無で各ステートに遷移
            if (player.MoveCheck()) {
                player.ChangeState(player.MoveState);
            } else {
                player.ChangeState(player.IdleState);
            }
        }
    }
    public void OnFixedUpdate() {
        // 未使用
    }

    public void OnStateExit() {
        player.Invincible = false;        // 無敵解除
        player.SkillStateFinish = false;  // スキル状態をリセット

        // アニメーショントリガーのリセット
        player.PlayerAnimator.ResetTrigger(Player.ANIM_TRIGGER_SKILL); 

        // 攻撃判定・エフェクトの停止
        player.OnDisableCollisionEvent();
        player.SlashEffectStop();

        player.Recast.ResetSkillRecast();  // スキルのリキャストに入る
        player.ResetAllPressed();   // 入力状態のリセット
    }
}
