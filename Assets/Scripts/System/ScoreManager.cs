using System;
using UnityEngine;
/// <summary>
/// ゲームのスコア管理をするシングルトン
/// </summary>
public class ScoreManager : MonoBehaviour {
    public static ScoreManager Instance { get; private set; }
    public int TotalScore { get; private set; }
    private bool canAddScore = false;   // スコア追加有効フラグ
    // スコアが増加したら通知
    // PlayerUIで購読
    public event Action<int> OnScoreChanged;

    private void Awake() {
        // シングルトンインスタンスの初期化
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary> スコア追加を有効化 </summary>
    public void EnableScore() => canAddScore = true;
    /// <summary> スコア追加を無効化 </summary>
    public void DisableScore() => canAddScore = false;

    /// <summary>
    /// トータルスコアに加算する
    /// </summary>
    /// <param name="amount"> 追加スコア </param>
    public void AddScore(int amount) {
        if(!canAddScore) return; // 無効なら加算しない
        TotalScore += amount;
        OnScoreChanged?.Invoke(TotalScore);
    }
}
