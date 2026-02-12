using TMPro;
using UnityEngine;
/// <summary>
/// ダメージポップアップテキストの制御クラス
/// </summary>
public class DamagePopup : MonoBehaviour
{
    // 作ったがゲームにそぐわなかった為未使用
    [Header("Setting")]
    [SerializeField] private float moveSpeed = 1f; // 上昇速度
    [SerializeField] private float lifeTime = 1f;  // ポップアップの寿命
    [SerializeField] private float fadeSpeed = 2f; // フェード速度

    private TextMeshPro textMesh;
    private Color textColor;
    private float timer;

    private void Awake() {
        textMesh = GetComponent<TextMeshPro>();
    }

    /// <summary>
    /// 数値をセット
    /// </summary>
    /// <param name="damage"> 被ダメージ値 </param>
    public void Setup(int damage) {
        SetText(damage.ToString());
    }

    /// <summary>
    /// 文字をセット
    /// </summary>
    /// <param name="message"> 表示テキスト </param>
    public void Setup(string message) {
        SetText(message);
    }

    // 共通処理
    private void SetText(string text) {
        textMesh.text = text;
        textColor = textMesh.color;
    }

    void Update() {
        // 上に移動
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        // フェードアウト
        timer += Time.deltaTime;
        if (timer > lifeTime) {
            textColor.a -= fadeSpeed * Time.deltaTime;
            textMesh.color = textColor;
            // 完全に透明になると破棄
            if (textColor.a <= 0f) {
                Destroy(gameObject);
            }
        }

        // カメラの方を向く
        if (Camera.main != null) {
            transform.LookAt(Camera.main.transform);
            transform.Rotate(0, 180, 0);
        }
    }
}
