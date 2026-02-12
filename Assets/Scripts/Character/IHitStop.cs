/// <summary>
/// ヒットストップを実行するインターフェース
/// </summary>
public interface IHitStop
{
    /// <summary>
    /// ヒットストップ実行(物理挙動も止める)
    /// </summary>
    /// <param name="duration"> ヒットストップ時間 </param>
    void HitStop(float duration);

    /// <summary>
    /// ヒットストップ実行(物理挙動は止めない)
    /// </summary>
    /// <param name="duration"> ヒットストップ時間 </param>
    void HitStopVisualOnly(float duration);
}
