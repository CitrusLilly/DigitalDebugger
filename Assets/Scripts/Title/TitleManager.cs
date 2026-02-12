using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
/// <summary>
/// タイトル画面の処理全般クラス
/// UIControllerBaseを継承
/// </summary>
public class TitleManager : UIControllerBase
{
    // ========================定数==========================
    // ===BGM===
    private const string TITLE_BGM_NAME = "title_Ancient_memories";
    // ===シーン名===
    private const string GAME_SCENE_NAME = "GameScene";
    // ===アニメーション===
    private const string ANIM_TRIGGER_TVIN = "TV_IN";
    // ======================================================

    [Header("UI,Input")]
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private Button[] titleButtons;
    [SerializeField] private Canvas titleUI;

    [Header("Performance")]
    [SerializeField] private int frameRate = 60;

    // 機能
    [SerializeField] private FadeController fadeController;

    private void Awake() {
        // フレームレート制限
        Application.targetFrameRate = frameRate;
    }

    protected override void Start() {
        base.Start();

        SetupTitle();
        StartCoroutine(PlayTitleIntro());
    }

    /// <summary>
    /// 初期設定
    /// </summary>
    private void SetupTitle() {
        // 誤操作防止のため操作無効化
        playerInput.DeactivateInput();
        // 時間の流れを初期化(ポーズ中に遷移した場合の影響を排除)
        Time.timeScale = 1f;
        // タイトル画面BGMの再生
        AudioManager.Instance.PlayBGM(TITLE_BGM_NAME);
    }

    /// <summary>
    /// フェード後に操作できる
    /// </summary>
    private IEnumerator PlayTitleIntro() {
        yield return StartCoroutine(fadeController.FadeInBlack());

        // 操作有効化
        playerInput.ActivateInput();
        // 最初に選択されるボタンをセット
        firstSelectMain.Select();
        // マウスカーソルの表示
        SetCursorVisible(true);
    }

    /// <summary>
    /// ゲーム開始処理
    /// </summary>
    public void StartGame() {
        PlaySelectSE();
        DisableUIInteraction();
        titleUI.enabled = false;
        SetCursorVisible(false);
        animator.SetTrigger(ANIM_TRIGGER_TVIN);
    }

    /// <summary>
    /// ゲームシーンをロード
    /// </summary>
    public void LoadGameScene() {
        SceneManager.LoadScene(GAME_SCENE_NAME);
    }

    /// <summary>
    /// 入力・ボタン無効化
    /// </summary>
    private void DisableUIInteraction() {
        // 選択解除して操作受付なし
        EventSystem.current.SetSelectedGameObject(null);
        playerInput.DeactivateInput();
        // ボタンを無効化してスタートボタンの連打防止
        foreach (var button in titleButtons) {
            if (button != null)
                button.interactable = false;
        }
    }

    /// <summary>
    /// ゲーム終了処理
    /// </summary>
    public void EndGame() {
        // 決定音再生
        PlaySelectSE();

#if UNITY_EDITOR
        // エディター
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // ビルド
        Application.Quit();
#endif
    }
}
