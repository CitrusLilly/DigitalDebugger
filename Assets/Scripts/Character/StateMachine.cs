/// <summary>
/// プレイヤー・エネミー共通の状態管理クラス
/// </summary>
public class StateMachine {
    public IState CurrentState { get; private set; }

    /// <summary>
    /// 状態の変更
    /// </summary>
    /// <param name="newState"> 変更後の状態 </param>
    public void ChangeState(IState newState) {
        // 同じ状態かnullなら変更しない
        if (newState == null || newState == CurrentState) return;

        CurrentState?.OnStateExit(); // 終了時処理
        CurrentState = newState;
        CurrentState.OnStateEnter(); // 開始時処理
    }

    /// <summary>
    /// 現在のステートのアップデート処理
    /// </summary>
    public void Update() => CurrentState?.OnStateUpdate();

    /// <summary>
    /// 現在のステートのアップデート処理(物理演算用)
    /// </summary>
    public void FixedUpdate() => CurrentState?.OnFixedUpdate();
}
