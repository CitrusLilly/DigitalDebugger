using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
/// <summary>
/// カットシーンの処理クラス
/// </summary>
public class CutsceneManager : MonoBehaviour
{
    // ========================定数==========================
    // ===BGM===
    private const string STAGE_BGM_NAME = "stage_サイバー・ストーリー";
    // ===テキストファイル名===
    private const string SCENARIO_TEXTFILE_NAME = "Scenario";
    // ===クリア時テキスト===
    private const string CLEAR_SYTEM_MESSAGE = "Thank You For Playing";
    // ======================================================

    [Header("Setting")]
    [SerializeField] private Light directionalLight;        // エリア全体のライト
    [SerializeField] private FadeController fadeController; // フェード制御
    [SerializeField] private float returnTitleDelay = 3f;   // タイトルに戻るまでの待機時間

    private void Start() {
        // 最初はステージを暗くするためライトを切り、フィールドの発光を切る
        directionalLight.enabled = false;
        ShaderManager.Instance.SetEmissionGlobal(false);
        // フォグを有効化
        RenderSettings.fog = true;
    }

    /// <summary>
    /// 現在のシーンを再読み込み
    /// </summary>
    public void ReloadCurrentScene() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// シーンを読み込む
    /// </summary>
    /// <param name="sceneName"> シーン名 </param>
    public void LoadScene(string sceneName) {
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// 最初の壁破壊後の演出
    /// </summary>
    public void PlayCutscene() {
        StartCoroutine(CutsceneSequence());
    }

    /// <summary>
    /// ゲームクリア時の演出
    /// </summary>
    public void PlayClearGame() {
        StartCoroutine(ClearGame());
    }

    /// <summary>
    /// 最初の壁破壊後の演出
    /// </summary>
    private IEnumerator CutsceneSequence() {
        // 誤操作防止のために操作を受け付けない
        GameManager.Instance.DisableInput();

        // 白フェードアウト → 光ON → 白フェードイン
        yield return fadeController.FadeOutWhite();
        directionalLight.enabled = true;
        ShaderManager.Instance.SetEmissionGlobal(true);
        RenderSettings.fog = false;
        yield return fadeController.FadeInWhite();

        // BGMと会話ダイアログの再生
        AudioManager.Instance.PlayBGM(STAGE_BGM_NAME);
        yield return DialogueManager.Instance.PlayDialogue(SCENARIO_TEXTFILE_NAME);

        // 目標のUI表示
        GameManager.Instance.ShowBugCount();
        // スコア加算有効化
        ScoreManager.Instance.EnableScore();
    }

    /// <summary>
    /// ゲームクリア時の演出
    /// </summary>
    private IEnumerator ClearGame() {
        // 誤操作防止のために操作を受け付けない
        GameManager.Instance.DisableInput();

        // 暗転後にスコアとクリアテキストを表示
        yield return fadeController.FadeOutBlack();
        string scoreText = $"スコア：{ScoreManager.Instance.TotalScore}\n";
        GameManager.Instance.ShowSystemMessage(scoreText + CLEAR_SYTEM_MESSAGE);

        // 数秒待機後にタイトルに戻る
        yield return new WaitForSeconds(returnTitleDelay);
        GameManager.Instance.ReturnTitle();
    }
}
