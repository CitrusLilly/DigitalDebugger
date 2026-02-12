using System.Collections;
using UnityEngine;
/// <summary>
/// 壁オブジェクトが衝突した対象を親のBugFieldManagerに渡すクラス
/// </summary>
public class BugFieldHit : MonoBehaviour
{
    private BugFieldManager bugFieldManager;
    [SerializeField] private Renderer wallRenderer;     // オブジェクトのレンダラー
    [SerializeField] private Color baseColor;           // 壁の元の色

    private void Awake() {
        // 親から管理クラスを取得
        bugFieldManager = GetComponentInParent<BugFieldManager>();
    }

    // 当たっている間の処理
    private void OnCollisionStay(Collision collision) {
        bugFieldManager.OnHitStay(collision);
    }

    // 当たった瞬間の処理
    private void OnCollisionEnter(Collision collision) {
        bugFieldManager.OnHitEnter(collision,this);
    }

    /// <summary>
    /// このバグフィールドの壁の色を一時的に変える
    /// </summary>
    public void ChangeColor(Color afterColor,float duration) {
        StartCoroutine(ChangeColorCoroutine(afterColor, duration));
    }

    /// <summary>
    /// 色変えコルーチン
    /// </summary>
    private IEnumerator ChangeColorCoroutine(Color afterColor,float duration) {
        // 色を変更
        Material mat = wallRenderer.material;
        mat.color = afterColor;

        // 指定時間後に色を戻す
        yield return new WaitForSeconds(duration);
        if(mat != null) {
            mat.color = baseColor;
        }
    }

    // 有効化時の処理
    private void OnEnable() {
        // 壁は透明スタート
        Color c = wallRenderer.material.color;
        c.a = 0f;
        wallRenderer.material.color = c;

        StartCoroutine(FadeInCoroutine());
    }

    /// <summary>
    /// 壁をフェードインさせるコルーチン
    /// </summary>
    private IEnumerator FadeInCoroutine() {
        float timer = 0f;

        while (timer < bugFieldManager.FadeDuration) {
            timer += Time.deltaTime;

            // アルファ値を上げて徐々にフェードイン
            Color c = baseColor;
            c.a = Mathf.Lerp(0f, baseColor.a, timer / bugFieldManager.FadeDuration);
            wallRenderer.material.color = c;

            yield return null;
        }
        // 最終確定
        wallRenderer.material.color = baseColor;
    }

    /// <summary>
    /// 壁のフェードアウト
    /// </summary>
    public void FadeOut() {
        StartCoroutine(FadeOutCoroutine());
    }

    /// <summary>
    /// 壁を徐々にフェードアウトさせるコルーチン
    /// </summary>
    private IEnumerator FadeOutCoroutine() {
        float timer = 0f;

        while (timer < bugFieldManager.FadeDuration) {
            timer += Time.deltaTime;

            // アルファ値を徐々に下げてフェードイン
            Color c = baseColor;
            c.a = Mathf.Lerp(baseColor.a, 0f, timer / bugFieldManager.FadeDuration);
            wallRenderer.material.color = c;

            yield return null;
        }
    }

}
