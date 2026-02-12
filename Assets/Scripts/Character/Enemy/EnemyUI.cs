using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// エネミーのUI管理クラス
/// </summary>
public class EnemyUI : MonoBehaviour
{
    [Header("Component")] // インスペクターから設定
    [SerializeField] private EnemyStatusManager statusManager; // HP参照用

    [Header("HP UI")]
    [SerializeField] private Slider hpSlider;

    private void Start() {
        // 参照確認
        if (statusManager == null) {
            Debug.LogError("ステータスを参照していません。");
            return;
        }

        // イベント追加
        statusManager.OnHpChanged += UpdateHpSlider;
        // 初期HPを反映
        UpdateHpSlider(statusManager.CurrentHp, statusManager.MaxHp);
    }

    /// <summary>
    /// ワールド空間に表示しているため、UIをカメラの方へ向ける
    /// </summary>
    private void LateUpdate() {
        if (Camera.main != null) {
            transform.rotation = Camera.main.transform.rotation;
        }
    }

    /// <summary>
    /// HP更新時に呼ばれ、スライダーUIを更新する
    /// </summary>
    private void UpdateHpSlider(int currentHp, int maxHp) {
        hpSlider.maxValue = maxHp;
        hpSlider.value = currentHp;

        // HP0でHPバーを非表示にする
        if (hpSlider.value <= 0) {
            hpSlider.gameObject.SetActive(false);
        } else if (!hpSlider.gameObject.activeSelf) {
            // 0より大きくて非表示時に更新があれば、復活したということなので表示する
            hpSlider.gameObject.SetActive(true);
        }
    }

    // メモリリーク対策
    private void OnDestroy() {
        if (statusManager != null) {
            statusManager.OnHpChanged -= UpdateHpSlider;
        }
    }
}
