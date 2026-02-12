using UnityEngine;
/// <summary>
/// プレイヤーの回避ステート
/// <para> Move,Idle,Attack,Skillから遷移可能 </para>
/// </summary>
public class PlayerDodgeState : IState {
    private Player player;
    private float dodgeTimer; // 回避の経過時間

    public PlayerDodgeState(Player player) {
        this.player = player;
    }

    public void OnStateEnter() {
        // 回避アニメーション再生
        player.PlayerAnimator.SetTrigger(Player.ANIM_TRIGGER_DODGE);

        player.DodgeStateFinish = false; // フラグのリセット
        player.Invincible = true;   // 回避中は無敵
        player.CanDodge = false;    // 連続回避防止
        player.CanAttack = true;    // 回避後攻撃に遷移可能

        // 入力があれば指定の向きに振り向く
        if (player.MoveCheck()) {
            player.Turn();
        }

        // 回避SE再生
        AudioManager.Instance.PlaySE(Player.DODGE_SE_NAME);

        // タイマーリセット
        dodgeTimer = 0;
    }

    public void OnStateUpdate() {
        // 未使用
    }

    public void OnFixedUpdate() {
        dodgeTimer += Time.fixedDeltaTime;

        // 回避中の移動処理
        if (dodgeTimer <= player.DodgeMoveDuration) {
            player.Move(player.DodgeMoveSpeed);
        }

        // 無敵時間を超えた場合に１回だけ無敵解除処理
        if (player.Invincible && dodgeTimer >= player.DodgeInvincibleTime) {
            player.Invincible = false;
        }

        // 回避終了フラグで攻撃入力優先で各ステートに遷移
        if (player.DodgeStateFinish) {

            if (player.InputHandler.AttackPressed) {
                player.ChangeState(player.AttackState);

            } else if (player.MoveCheck()) {
                player.ChangeState(player.MoveState);

            } else {
                player.ChangeState(player.IdleState);
            }

        }
    }

    public void OnStateExit() {
        // 回避アニメーションのトリガーリセット
        player.PlayerAnimator.ResetTrigger(Player.ANIM_TRIGGER_DODGE);

        player.Invincible = false;  // 無敵解除
        player.DodgeStateFinish = false; // フラグのリセット

        player.ResetAllPressed();   // 入力状態リセット
    }
}
