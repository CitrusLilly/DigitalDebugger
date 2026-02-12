using UnityEngine;
/// <summary>
/// シェーダープロパティの切り替えシングルトン
/// </summary>
public class ShaderManager : MonoBehaviour
{
    public static ShaderManager Instance { get; private set; }

    // プロパティID
    private static readonly int IsGrayscale = Shader.PropertyToID("_isGrayscale");
    private static readonly int IsEmission  = Shader.PropertyToID("_isEmission");
    private static readonly int IsDamaged   = Shader.PropertyToID("_isDamaged");

    [Header("Material")]
    [SerializeField] private Material fullScreenMaterial;   // ポストエフェクト用マテリアル

    void Awake() {
        // シングルトンインスタンスの初期化
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// グレースケール化ポストエフェクトの切り替え
    /// </summary>
    public void SetGrayscalePostEffect(bool enabled) {
        if (fullScreenMaterial == null) return;

        if (enabled) {
            fullScreenMaterial.SetFloat(IsGrayscale, 1f);
        } else {
            fullScreenMaterial.SetFloat(IsGrayscale, 0f);
        }
    }

    /// <summary>
    /// フィールドオブジェクトの発光の切り替え (グローバル)
    /// </summary>
    public void SetEmissionGlobal(bool enabled) {
        if (enabled) {
            Shader.SetGlobalFloat(IsEmission, 1f);
        } else {
            Shader.SetGlobalFloat(IsEmission, 0f);
        }
    }

    /// <summary>
    /// 被ダメージポストエフェクト表示の切り替え
    /// </summary>
    public void SetDamagedPostEffect(bool enabled) {
        if (fullScreenMaterial == null) return;

        if (enabled) {
            fullScreenMaterial.SetFloat(IsDamaged, 1f);
        } else {
            fullScreenMaterial.SetFloat(IsDamaged, 0f);
        }
    }

    // オブジェクト無効化時に元に戻す
    private void OnDisable() {
        SetGrayscalePostEffect(false);
        SetEmissionGlobal(false);
        SetDamagedPostEffect(false);
    }
}