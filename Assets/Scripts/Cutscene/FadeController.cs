using UnityEngine;
using UnityEngine.UI;
using System.Collections;
/// <summary>
/// フェード処理クラス(黒・白オーバーレイを必要に応じて使える)
/// </summary>
public class FadeController : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField] private GameObject blackOverlay;   // フェード用黒Image
    [SerializeField] private GameObject whiteOverlay;   // フェード用白Image
    [SerializeField] private float fadeDuration = 1.5f; // フェードにかける時間(秒)

    /// <summary> 白フェードアウト </summary>
    public IEnumerator FadeOutWhite() => Fade(whiteOverlay, 0f, 1f);
    /// <summary> 白フェードイン </summary>
    public IEnumerator FadeInWhite() => Fade(whiteOverlay, 1f, 0f);
    /// <summary> 黒フェードアウト </summary>
    public IEnumerator FadeOutBlack() => Fade(blackOverlay, 0f, 1f);
    /// <summary> 黒フェードイン </summary>
    public IEnumerator FadeInBlack() => Fade(blackOverlay, 1f, 0f);

    /// <summary>
    /// フェード処理
    /// </summary>
    /// <param name="overlay"> 白黒Imageオブジェクト </param>
    /// <param name="start"> フェード開始時のアルファ値(0～1) </param>
    /// <param name="end"> フェード終了時のアルファ値(0～1) </param>
    private IEnumerator Fade(GameObject overlay, float start, float end) {
        if (overlay == null) yield break; // オーバーレイが未指定ならスキップ

        overlay.SetActive(true);
        var image = overlay.GetComponent<Image>();
        var color = image.color;
        float timer = 0f;

        // アルファ値を補間して徐々に変化
        while (timer < fadeDuration) {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(start, end, timer / fadeDuration);
            image.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }
        // 最終状態確定
        image.color = new Color(color.r, color.g, color.b, end);

        // フェードイン時は最後に非アクティブ化
        if (end == 0f) overlay.SetActive(false);
    }
}
