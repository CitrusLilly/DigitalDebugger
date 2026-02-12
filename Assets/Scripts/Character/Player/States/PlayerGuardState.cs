using UnityEngine;
/// <summary>
/// プレイヤーのガードステート
/// <para> Move,Idle,Attack,Skillから遷移可能 </para>
/// </summary>
public class PlayerGuardState : IState
{
    private Player player;
    private float guardTimer; // ガード状態の経過時間

    public PlayerGuardState(Player player) {
        this.player = player;
    }

    public void OnStateEnter() {
        // ガードアニメーション開始
        player.PlayerAnimator.SetBool(Player.ANIM_BOOL_GUARD, true);
        // ガード時のエフェクト有効化
        player.ToggleGuardEffect(true);
        // ガード展開SE再生
        AudioManager.Instance.PlaySE(Player.GUARD_ON_SE_NAME);

        player.IsGuard = true;
        player.CanDodge = false; // ガード中は回避不可
        player.CanAttack = true; // ガード中に攻撃可能

        guardTimer = 0; // タイマーリセット
    }

    public void OnStateUpdate() {
        guardTimer += Time.deltaTime;

        // ガード終了間際の警告点滅
        if (guardTimer >= player.GuardWarningTime) {
            player.FlashGuardWarning();
        }

        // ガードボタンを離すか、ガード時間切れで移動か待機状態に遷移
        if (!player.InputHandler.GuardPressed || guardTimer >= player.GuardTime) {
            if (player.MoveCheck()) {
                player.ChangeState(player.MoveState);
            } else {
                player.ChangeState(player.IdleState);
            }
        }

        // ガード中でも攻撃入力があれば遷移
        if (player.InputHandler.AttackPressed) {
            player.ChangeState(player.AttackState);
        }
    }

    public void OnFixedUpdate() {
        // 未使用
    }

    public void OnStateExit() {
        // ガードアニメーション終了
        player.PlayerAnimator.SetBool(Player.ANIM_BOOL_GUARD, false);
        // エフェクト無効化
        player.ToggleGuardEffect(false);

        player.IsGuard = false;
        
        player.Recast.ResetGuardRecast();   // ガード終了でリキャストに入る
        player.GuardCoroutineFinish();      // エフェクトの点滅処理を停止
        player.ResetAllPressed();           // 入力状態リセット
    }
}
