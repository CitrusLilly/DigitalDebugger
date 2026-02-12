using UnityEngine;
/// <summary>
/// プレイヤーの被弾ステート　動かせない
/// <para> ノックバック処理をトリガーに遷移 </para>
/// </summary>
public class PlayerDamagedState : IState
{
    private Player player;
    private float knockBackTimer; // ノックバック時間計測

    public PlayerDamagedState(Player player) {
        this.player = player;
    }

    public void OnStateEnter() {
        // 被弾アニメーション再生
        player.PlayerAnimator.SetTrigger(Player.ANIM_TRIGGER_KNOCKBACK);
        player.Invincible = true;   // 無敵を有効にして連続被弾を避ける
        player.CanDodge = false;    // 回避行動不可
        knockBackTimer = 0;         // タイマーリセット
    }

    public void OnStateUpdate() {
        knockBackTimer += Time.deltaTime;

        // ノックバック時間が過ぎたら次のステートを判定
        // player.KnockBackTimeは攻撃相手から受け取ったノックバック時間となる
        if (knockBackTimer >= player.DamagedStateDuration) {
            if(player.InputHandler.AttackPressed){
                player.ChangeState(player.AttackState);
            } else if (player.MoveCheck()) {
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
        // 回避行動を可能にして入力状態をリセット
        player.CanDodge = true;
        player.ResetAllPressed();
    }
}
