using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
/// <summary>
/// ゲームパッドの振動管理シングルトン
/// </summary>
public class GamePadVibration : MonoBehaviour
{
    public static GamePadVibration Instance { get; private set; }

    private Gamepad currentPad; // 現在使用中のゲームパッド

    private Coroutine vibrationCoroutine; // 振動中のコルーチン参照(同時起動防止用)

    void Awake() {
        // シングルトンインスタンスの初期化
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// ゲームパッドの振動を発生させる
    /// </summary>
    /// <param name="lowFrequency"> 低周波(0~1) </param>
    /// <param name="highFrequency"> 高周波(0~1) </param>
    /// <param name="duration"> 振動時間(秒) </param>
    public void PlayVibration(float lowFrequency, float highFrequency, float duration) {
        currentPad = Gamepad.current;
        if (currentPad == null) return; // パッドが接続されていなければ振動させない

        // 振動中なら前のコルーチンを止める
        if (vibrationCoroutine != null) {
            StopCoroutine(vibrationCoroutine);
        }
        // 新しく振動コルーチンを始める
        vibrationCoroutine = StartCoroutine(VibrationRoutine(lowFrequency, highFrequency, duration));
    }

    /// <summary>
    /// 振動処理を行うコルーチン
    /// </summary>
    private IEnumerator VibrationRoutine(float low, float high, float duration) {
        if (currentPad == null) yield break;  // 振動前にチェック

        currentPad.SetMotorSpeeds(low, high); // モーター起動

        yield return new WaitForSecondsRealtime(duration);

        if (currentPad != null) { // 二重チェック
            currentPad.SetMotorSpeeds(0, 0);  //モーター停止
        }  
        vibrationCoroutine = null;
    }

    // ゲームオブジェクト破棄時に振動を必ず止める
    private void OnDestroy() {
        if (currentPad != null) {
            currentPad.SetMotorSpeeds(0, 0);
        }
    }
}
