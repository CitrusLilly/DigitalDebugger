/// <summary>
/// プレイヤーの死亡ステート　ここから遷移するステートは無い
/// <para> プレイヤー死亡処理をトリガーに遷移 </para>
/// </summary>
public class PlayerDieState : IState
{
    private Player player;

    public PlayerDieState(Player player) {
        this.player = player;
    }

    public void OnStateEnter() {
        // 死亡時アニメーションに遷移
        player.PlayerAnimator.SetTrigger(Player.ANIM_TRIGGER_DIE);
        // 死体を押されないように物理挙動を止める
        player.Rb.isKinematic = true;
        // 死亡時にもダメージ表現
        player.SetDamadedEffect(true);
    }

    public void OnStateUpdate() {
        // 未使用
    }
    public void OnFixedUpdate() {
        // 未使用
    }
    public void OnStateExit() {
        // 未使用
    }
}
