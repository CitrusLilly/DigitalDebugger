using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
/// <summary>
/// バグフィールドの管理クラス
/// BugFieldHitクラスからヒット通知を受け取り処理する
/// </summary>
public class BugFieldManager : MonoBehaviour
{
    // ========================定数==========================
    // ===シェーダー===
    private const string SHADER_PROP_BASE_COLOR     = "_BaseColor";
    private const string SHADER_PROP_EMISSION_COLOR = "_EmissionColor";
    // ======================================================

    // インスペクターからフィールドごとに指定
    [Header("Setting")] 
    [SerializeField] private int maxHealth = 2;                 // バグフィールドの最大耐久値
    [SerializeField] private int pulseDamage = 3;               // プレイヤーが触れた時に与えるダメージ
    [SerializeField] private float hitStopDuration = 0.5f;      // 与えるヒットストップ時間(秒)
    [SerializeField] private KnockbackRequest knockbackRequest; // ノックバックリクエストパラメータ
    [SerializeField] private float playerDamageInterval = 2f;   // プレイヤーの連続ダメージ間隔                               
    [SerializeField] private float fadeDuration = 0.8f;         // フェードにかける時間(秒)
    public float FadeDuration => fadeDuration;                  // 外部参照用

    [Header("UI")]
    [SerializeField] Slider hpSlider; // 耐久値表示UI

    [Header("FloorMaterial")]
    [SerializeField] private Renderer fieldRenderer;        // マテリアルの色変更のために取得
    [SerializeField] private Color bugBaseColor;            // バグフィールドの色(赤)
    [SerializeField, ColorUsage(true, true)] private Color bugHDRColor;
    [SerializeField] private Color afterBaseColor;          // バグフィールドクリア時に変化する正常な色(緑)
    [SerializeField, ColorUsage(true, true)] private Color afterHDRColor;

    [Header("wall")]
    [SerializeField] private Color wallChangeColor;             // 接触した壁が変化する色
    [SerializeField] private float changeColorDuration = 1.0f;  // 色が変わっている時間(秒)
    [SerializeField] private ParticleSystem enemyHitEffect;     // エネミーが壁にヒットした際に表示するエフェクト

    [Header("FieldBreakEvent")] // インスペクターから指定
    [SerializeField] private UnityEvent onBreakEvent;       // フィールド破壊時イベント
    [SerializeField] private GameObject[] hideObjects;      // 非表示にするオブジェクト
    [SerializeField] private GameObject[] destroyObjects;   // 破壊するオブジェクト

    private bool isActive = true;           // ダメージ・判定が有効か
    private int currentHealth;              // 現在耐久値
    private float lastPlayerDamageTime;     // 最後のプレイヤーがダメージを受けた時間
    private int playerLayer;
    private int enemyRollLayer;

    private void Start() {
        // レイヤーを記憶
        playerLayer = LayerMask.NameToLayer(Player.LAYER_NAME);
        enemyRollLayer = LayerMask.NameToLayer(Enemy.BALL_LAYER_NAME);
        // 初期化
        currentHealth = maxHealth;
        lastPlayerDamageTime = Time.time;
        UpdateHpUI();
        // 床をバグの色にする
        if (fieldRenderer != null) {
            SetFieldColor(bugBaseColor,bugHDRColor);
        } 
    }

    /// <summary>
    /// 当たっている間の処理呼び出し (プレイヤー判定用)
    /// </summary>
    /// <param name="collision"> 衝突対象のコリジョン </param>
    public void OnHitStay(Collision collision) {
        // 壁のアクティブ状態チェック
        if (!isActive) return;
        // ダメージ間隔チェック
        if (Time.time < lastPlayerDamageTime + playerDamageInterval) return;
        // プレイヤー以外は無視
        if (collision.gameObject.layer != playerLayer) return;
        // ダメージを受けられない相手は無視
        if (!collision.gameObject.TryGetComponent<IDamageable>(out var damageable)) return;

        // ダメージ処理
        var result = damageable.TakeDamage(pulseDamage);

        // 与えた時間の更新
        lastPlayerDamageTime = Time.time;

        if (result == DamageReaction.Damaged) {
            // ノックバック方向計算
            ContactPoint contact = collision.GetContact(0);
            Vector3 hitNormal = contact.normal;
            hitNormal.y = 0f;
            hitNormal = -hitNormal.normalized;

            knockbackRequest.Direction = hitNormal;
            damageable.KnockBack(knockbackRequest);

            // 演出
            PlayPlayerHitSE();
            if (collision.gameObject.TryGetComponent(out IHitStop hitStop)) {
                hitStop.HitStop(hitStopDuration);
            }
        }
    }

    /// <summary>
    /// 当たった瞬間の処理呼び出し (エネミー判定用)
    /// </summary>
    /// <param name="collision"> 衝突対象のコリジョン </param>
    /// <param name="hit"> 衝突したオブジェクトのBugFieldHitインスタンス </param>
    public void OnHitEnter(Collision collision,BugFieldHit hit) {
        // 衝突対象のレイヤー取得
        int layer = collision.gameObject.layer;

        if (layer == enemyRollLayer) {
            // エネミーが触れるとバグフィールド耐久値を減らす処理
            currentHealth--;
            UpdateHpUI();
            // バグフィールドの耐久値がなくなると登録されているイベントを実行してフィールド破壊
            if (currentHealth == 0) {
                isActive = false;
                onBreakEvent?.Invoke();
                StartCoroutine(BreakField());
                return;
            }

            // ヒットエフェクト・SE再生
            ShowEnemyHitEffect(collision);
            PlayEnemyHitSE();

            // どの壁にエネミーが接触したか分かりやすいように一定時間、壁の色を変更
            hit.ChangeColor(wallChangeColor, changeColorDuration);
        }
    }

    /// <summary>
    /// 耐久値UIの更新
    /// </summary>
    private void UpdateHpUI() {
        if (hpSlider != null) {
            hpSlider.maxValue = maxHealth;
            hpSlider.value = Mathf.Clamp(currentHealth, 0, hpSlider.maxValue);
        }
    }

    /// <summary>
    /// エネミーがヒットした時のエフェクトを出す
    /// </summary>
    /// <param name="collision"> 接触したエネミーのCollision </param>
    private void ShowEnemyHitEffect(Collision collision) {
        // すべての接触点から1つ取る
        ContactPoint contact = collision.GetContact(0);

        Vector3 hitPosition = contact.point;    // 当たったワールド座標
        Vector3 hitNormal = contact.normal;     //当たった面の法線ベクトル

        // 法線方向 * 元の角度に回転
        Quaternion rot = Quaternion.LookRotation(hitNormal) * enemyHitEffect.transform.rotation;

        // エフェクト生成 & 自動破棄
        ParticleSystem effect = Instantiate(enemyHitEffect, hitPosition, rot);
        Destroy(effect.gameObject, enemyHitEffect.main.duration);
    }

    /// <summary>
    /// フィールドの色を変更
    /// </summary>
    private void SetFieldColor(Color baseColor, Color emissionColor) {
        if (fieldRenderer != null) {
            fieldRenderer.material.SetColor(SHADER_PROP_BASE_COLOR, baseColor);
            fieldRenderer.material.SetColor(SHADER_PROP_EMISSION_COLOR, emissionColor);
        }
    }

    /// <summary>
    /// 耐久が0になった際のフィールド削除処理
    /// </summary>
    private IEnumerator BreakField() {
        // 子のBugFieldHitのフェードアウト処理を全て呼ぶ
        BugFieldHit[] bughits = GetComponentsInChildren<BugFieldHit>();

        foreach (BugFieldHit hit in bughits) {
            hit.FadeOut();
        }

        // フェード後にオブジェクト削除
        yield return new WaitForSeconds(fadeDuration);
        Destroy(gameObject);
    }

    /// <summary> エネミーが壁にヒットした際のSEを再生する </summary>
    private void PlayEnemyHitSE() => AudioManager.Instance.PlaySE(Enemy.WALL_HIT_SE_NAME);

    /// <summary> プレイヤーが壁にヒットした際のSEを再生する </summary>
    private void PlayPlayerHitSE() => AudioManager.Instance.PlaySE(Player.DAMAGED_SE_NAME);

    // ==========================
    // =フィールド破壊時イベント=
    // ==========================

    /// <summary>
    /// エネミーの全滅
    /// </summary>
    public void DestroyAllEnemies() {
        EnemyManager.Instance.DestroyAllEnemies();
    }

    /// <summary>
    /// 床を正常な色に戻す処理
    /// </summary>
    public void ReturnFloorColor() {
        SetFieldColor(afterBaseColor, afterHDRColor);
    }

    /// <summary>
    /// オブジェクト非表示
    /// </summary>
    public void HideObjects() {
        foreach (var obj in hideObjects) {
            if (obj != null) {
                obj.SetActive(false);
            }
        }
    }

    /// <summary>
    /// オブジェクト削除
    /// </summary>
    public void DestroyObjects() {
        foreach (var obj in destroyObjects) {
            if (obj != null) {
                Destroy(obj);
            }
        }
    }
}
