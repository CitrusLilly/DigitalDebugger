using UnityEngine;
/// <summary>
/// プレイヤーの攻撃ステート
/// 攻撃関係のフラグはアニメーションイベントで制御
/// <para> Move,Idle,Dodge,Guard,Damagedから遷移可能 </para>
/// </summary>
public class PlayerAttackState : IState
{
    private Player player;
    private float moveTimer;    // 攻撃時の移動時間
    private int autoTurnCnt;    // オートターンのフレーム数をカウント

    public PlayerAttackState(Player player) {
        this.player = player;
    }

    public void OnStateEnter() {
        // 1段目フラグの有効化と攻撃アニメーションの再生
        player.PlayerAnimator.SetBool(Player.ANIM_BOOL_FIRSTATTACK,true);
        player.PlayerAnimator.SetTrigger(Player.ANIM_TRIGGER_ATTACK);

        player.CanAttack = false;           // 連打防止
        player.CanDodge = true;             // 攻撃中は回避可能
        player.AttackStateFinish = false;   // フラグのリセット
        player.CurrentCombo = 0;            // コンボリセット
        moveTimer = 0;                      // タイマーリセット
        autoTurnCnt = 0;                    // オートターンカウントリセット

        // 武器に初段コンボを通知
        player.Weapon.SetCurrentCombo(player.CurrentCombo, false);

        // 攻撃SE再生
        AudioManager.Instance.PlaySE(Player.ATTACK_SE_NAME);
    }

    public void OnStateUpdate() {
        // 指定フレーム数だけ近くの敵にオートターン
        if (autoTurnCnt < player.AutoTurnMaxCnt) {
            player.AutoTurn();
            autoTurnCnt++;
        }

        // 回避入力があれば回避状態に遷移
        if (player.InputHandler.DodgePressed) {
            player.ChangeState(player.DodgeState);
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

        // 再度攻撃入力があった場合コンボに派生する (攻撃入力タイミングあり)
        if (player.InputHandler.AttackPressed) {
            NextComboStep();
            return;
        }

        // 攻撃モーションが最後まで流れた場合に、移動か待機へ遷移
        if (player.AttackStateFinish) {
            // 移動入力の有無で遷移
            if (player.MoveCheck()) {
                player.ChangeState(player.MoveState);
                return;
            } else {
                player.ChangeState(player.IdleState);
                return;
            }
        }
    }

    // アセットにルートモーションが設定されていなかったため攻撃中の前進処理はここで行う
    public void OnFixedUpdate() {
        if (player.IsAttackMove) {
            moveTimer += Time.fixedDeltaTime;

            if (moveTimer < player.AttackMoveTime) {
                player.Move(player.AttackMoveSpeed);
            } else {
                // 指定時間終了で移動終了
                player.IsAttackMove = false;
                moveTimer = 0;
            }
        }
    }

    public void OnStateExit() {
        // アニメーションフラグのリセット
        player.PlayerAnimator.SetBool(Player.ANIM_BOOL_FIRSTATTACK, false);
        player.PlayerAnimator.ResetTrigger(Player.ANIM_TRIGGER_ATTACK);

        // 攻撃判定とエフェクトの停止
        player.OnDisableCollisionEvent();
        player.SlashEffectStop();

        player.AttackStateFinish = false;   // フラグリセット
        
        player.CurrentCombo = 0;            // コンボ段階初期化
        player.ResetAllPressed();           // 入力状態リセット
    }

    /// <summary>
    /// コンボ段階を次へ進める処理
    /// </summary>
    private void NextComboStep() {
        // 初段フラグを解除
        if (player.PlayerAnimator.GetBool(Player.ANIM_BOOL_FIRSTATTACK)) {
            player.PlayerAnimator.SetBool(Player.ANIM_BOOL_FIRSTATTACK, false);
        }

        // 次のコンボアニメーション再生
        player.PlayerAnimator.SetTrigger(Player.ANIM_TRIGGER_ATTACK);

        // コンボ段階更新
        player.CurrentCombo = Mathf.Clamp(player.CurrentCombo + 1, 0, player.MaxCombo);
        player.Weapon.SetCurrentCombo(player.CurrentCombo, false);

        // 攻撃SE再生
        AudioManager.Instance.PlaySE(Player.ATTACK_SE_NAME);

        autoTurnCnt = 0;                    // オートターンカウントリセット
        player.CanAttack = false;           // 連打防止
        player.AttackStateFinish = false;   // フラグリセット
        player.ResetAllPressed();           // 入力状態リセット
    }
}
