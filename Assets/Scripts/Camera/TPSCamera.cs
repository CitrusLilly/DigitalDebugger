using UnityEngine;
using UnityEngine.InputSystem;
/// <summary>
/// プレイヤー用カメラクラス
/// </summary>
public class TPSCamera : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;  // カメラ追跡対象
    [SerializeField] private Vector3 offset = Vector3.zero;

    [Header("Setting")]
    [SerializeField] private float distance = 12f;      // カメラ距離
    [SerializeField] private float minDistance = 10f;   // 最短距離
    [SerializeField] private float maxDistance = 14f;   // 最長距離
    [SerializeField] private float sensitivity = 1.2f;  // カメラ感度
    [SerializeField] private float rangeUnderY = 0f;    // Y軸の下限
    [SerializeField] private float rangeTopY = 50f;     // Y軸の上限

    // インスペクターから初期角度を調整
    [SerializeField, Range(0,360)] private float xRotation = 0f;
    [SerializeField, Range(0,50)] private float yRotation = 0f;

    private Vector2 lookInput = Vector2.zero; // カメラ回転入力受け取り

    #region InputSystemの入力
    // カメラ回転
    public void OnLook(InputAction.CallbackContext context) {
        lookInput = context.ReadValue<Vector2>();
    }

    // カメラズーム
    public void OnZoom(InputAction.CallbackContext context) {
        if (context.performed) {
            float zoomInput = context.ReadValue<float>();
            distance = Mathf.Clamp(distance - zoomInput, minDistance, maxDistance);
        }
    }
    #endregion

    void LateUpdate() {
        if (target == null) return;
        
        // 入力値 * 感度 * 経過時間
        xRotation += lookInput.x * sensitivity * Time.deltaTime;
        yRotation -= lookInput.y * sensitivity * Time.deltaTime;
        // 上下の制限
        yRotation = Mathf.Clamp(yRotation, rangeUnderY, rangeTopY);

        // 回転の計算
        Quaternion rotation = Quaternion.Euler(yRotation, xRotation, 0f);
        // 注視点
        Vector3 lookAtPoint = target.position + offset;
        // カメラ位置 = 注視点 + 回転を適用した後方ベクトル * distance
        Vector3 position = lookAtPoint + rotation * (Vector3.back * distance);

        // カメラ移動
        transform.position = position;
        // カメラを注視点方向に向ける
        transform.LookAt(lookAtPoint);
    }
}