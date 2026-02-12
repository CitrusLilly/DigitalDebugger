using UnityEngine;
/// <summary>
/// ダメージポップアップを生成するシングルトン
/// </summary>
public class DamagePopupGenerator : MonoBehaviour
{
    // 作ったがゲームにそぐわなかった為未使用
    public static DamagePopupGenerator Instance { get; private set; }

    [Header("Setting")]
    [SerializeField] private GameObject damagePopupPrefab;          // インスペクターでテキストオブジェクト指定
    [SerializeField] private Vector3 popupOffset = Vector3.zero;    // 表示位置を敵の中心からずらす

    private void Awake() {
        // シングルトンインスタンスの初期化
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// ダメージポップアップ処理。数値パターン
    /// </summary>
    /// <param name="position"> 表示位置 </param>
    /// <param name="damage"> 表示ダメージ値 </param>
    public void ShowPopup(Vector3 position, int damage) {
        // 生成とDamagePopupコンポーネントの初期化
        GameObject popupObj = Instantiate(damagePopupPrefab, position + popupOffset, Quaternion.identity);
        popupObj.GetComponent<DamagePopup>().Setup(damage);
    }

    /// <summary>
    /// ダメージポップアップ処理。文字パターン
    /// </summary>
    /// <param name="position"> 表示位置 </param>
    /// <param name="message"> 表示文字列 </param>
    public void ShowPopup(Vector3 position, string message) {
        // 生成とDamagePopupコンポーネントの初期化
        GameObject popupObj = Instantiate(damagePopupPrefab, position + popupOffset, Quaternion.identity);
        popupObj.GetComponent<DamagePopup>().Setup(message);
    }
}
