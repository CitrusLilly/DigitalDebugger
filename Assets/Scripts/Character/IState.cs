/// <summary>
/// プレイヤーエネミー共通のステートパターン用インターフェース
/// </summary>
public interface IState
{
    /// <summary>開始時処理</summary>
    void OnStateEnter();

    /// <summary>更新処理</summary>
    void OnStateUpdate();

    /// <summary>物理演算更新処理</summary>
    void OnFixedUpdate();

    /// <summary>終了処理</summary>
    void OnStateExit(); 
}
