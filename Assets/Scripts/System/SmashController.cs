using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
/// <summary>
/// ボール状態のエネミーを弾き飛ばすシングルトン
/// </summary>
public class SmashController : MonoBehaviour
{
    public static SmashController Instance { get; private set; }

    [Header("Setting")]
    [SerializeField] private float smashForce = 23f;                // 吹き飛ばし力
    [SerializeField] private float hitStopDuration = 0.2f;          // ヒットストップ時間(秒)
    [SerializeField] private ParticleSystem smashHitEffect;         // スマッシュヒット時のエフェクト(生成)
    [SerializeField] private Vector3 effectOffset = Vector3.zero;   // エフェクト位置の調整
    public float HitStopDuration => hitStopDuration;                // ヒットストップ時間外部参照
    public float SmashForce => smashForce;                          // 吹き飛ばし力の外部参照

    [Header("Vibration")]
    [SerializeField] private float vibrationLow = 0.2f;         // 低周波
    [SerializeField] private float vibrationHigh = 0.2f;        // 高周波
    [SerializeField] private float vibrationDuration = 0.1f;    // 振動時間(秒)

    [Header("Count")]
    [SerializeField] private float smashCountResetTime = 5.0f;  // スマッシュカウントのリセット時間
    private int smashCount = 0;             // スマッシュ数のカウント
    private Coroutine smashResetCoroutine;  // リセット時間の管理

    [Header("Score")]
    [SerializeField] private int baseSmashScore = 1000;   // スマッシュ時の基本スコア

    // スマッシュカウントが変化したら通知
    // PlayerUI.UpdateSmashCount を購読
    public event Action<int> OnSmashCountChanged;

    void Awake() {
        // シングルトンインスタンスの初期化
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// 対象を自身の正面に飛ばす
    /// </summary>
    /// <param name="enemy"> 飛ばすエネミー </param>
    /// <param name="direction"> 飛ばすベクトル </param>
    public void ApplySmash(Enemy enemy,Vector3 direction) {
        // 見た目のみこちらで処理
        // スマッシュエフェクト生成
        ParticleSystem effectInstance = Instantiate(
            smashHitEffect,
            enemy.transform.position + effectOffset,
            Quaternion.identity
            );
        Destroy(effectInstance.gameObject, smashHitEffect.main.duration);
        // 吹き飛ばしSE再生
        AudioManager.Instance.PlaySE(Player.SMASH_SE_NAME);
        // コントローラーの振動
        GamePadVibration.Instance.PlayVibration(vibrationLow, vibrationHigh, vibrationDuration);

        // スマッシュカウント増加
        SmashCountUp();
        // ヒットストップ
        if (enemy.gameObject.TryGetComponent(out IHitStop hitstop)) {
            hitstop.HitStop(hitStopDuration);
        }
        // 実際の飛ばす処理
        StartCoroutine(SmashRoutine(enemy, direction));
    }

    /// <summary>
    /// 飛ばす処理の実行部分
    /// </summary>
    /// <param name="enemy"> 飛ばすエネミー </param>
    /// <param name="direction"> 飛ばすベクトル </param>
    public IEnumerator SmashRoutine(Enemy enemy,Vector3 direction) {
        // ヒットストップ分時間を待ってから実際に飛ばす
        yield return new WaitForSeconds(hitStopDuration);

        // エネミーの状態を変更
        enemy.OnSmashed(direction * smashForce);
    }

    /// <summary>
    /// スマッシュカウントとスコアの処理
    /// </summary>
    private void SmashCountUp() {
        // カウントを増加し、カウント変更時イベントを実行
        smashCount++;
        OnSmashCountChanged?.Invoke(smashCount);

        // スコア追加
        ScoreManager.Instance.AddScore(SmashScoreCalculate());

        // リセットコルーチンの再起動
        if (smashResetCoroutine != null) {
            StopCoroutine(smashResetCoroutine);
        }
        smashResetCoroutine = StartCoroutine(SmashCountResetCoroutine());
    }

    /// <summary>
    /// スコア計算
    /// ベーススコア * スマッシュ数
    /// </summary>
    private int SmashScoreCalculate() {
        return baseSmashScore * smashCount;
    }

    /// <summary>
    /// 一定時間後にスマッシュのコンボをリセットする
    /// </summary>
    private IEnumerator SmashCountResetCoroutine() {
        yield return new WaitForSeconds(smashCountResetTime);

        smashCount = 0;
        smashResetCoroutine = null;
        OnSmashCountChanged?.Invoke(smashCount); 
    }
}
