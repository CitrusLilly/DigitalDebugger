using System;
using UnityEngine;
/// <summary>
/// ゲームループ処理全般のシングルトン
/// </summary>
public class GameManager : MonoBehaviour
{
    // 定数 シーン名
    private const string TITLE_SCENE_NAME = "TitleScene";

    public static GameManager Instance { get; private set; }

    [Header("Component")]
    [SerializeField] private InputModeController inputModeManager;  // 入力モードの切り替え用
    [SerializeField] private MenuManager menuManager;               // UIの制御用

    [Header("Performance")]
    [SerializeField] private int frameRate = 60;    // フレームレート

    [Header("Controllers")]
    [SerializeField] private BugFieldTracker bugTracker;
    [SerializeField] private CutsceneManager cutsceneManager;
    [SerializeField] private FadeController fadeController;
    [SerializeField] private GameOverController gameOverController;

    [Header("Player")]
    [SerializeField] private Transform initialPlayerTransform;  // プレイヤーの初期位置
    public Vector3 SpawnPoint { get; set; } // プレイヤーのスポーン位置

    void Awake() {
        // シングルトンインスタンスの初期化
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // フレームレート制限
        Application.targetFrameRate = frameRate;
    }

    private void Start() {
        InitGameState();
        // フェードインから始まる
        StartCoroutine(fadeController.FadeInBlack());
    }

    /// <summary>
    /// 初期設定
    /// </summary>
    private void InitGameState() {
        Time.timeScale = 1f;
        SetCursorVisible(false);
        SpawnPoint = initialPlayerTransform.position; // 初期位置をスポーン地点として記憶
    }

    // 残りフィールド数変更時イベント中継
    public event Action<int> OnBugCountChanged {
        add => bugTracker.OnBugCountChanged += value;
        remove => bugTracker.OnBugCountChanged -= value;
    }

    /// <summary> バグフィールドの初期表示 </summary>
    public void ShowBugCount() => bugTracker.ShowBugCount();

    /// <summary> バグフィールドをクリア時の処理 </summary>
    public void OnBugCleared() => bugTracker.ClearBug();

    /// <summary> チュートリアル終了時のカットシーン </summary>
    public void StartCutscene() => cutsceneManager.PlayCutscene();

    /// <summary> ゲームクリア時の演出 </summary>
    public void PlayClearGame() => cutsceneManager.PlayClearGame();

    /// <summary> システムメッセージを表示 </summary>
    public void ShowSystemMessage(string _text) => menuManager.ShowSystemMessage(_text);

    /// <summary> マウスカーソルの表示 </summary>
    public void SetCursorVisible(bool visible) => menuManager.SetCursorVisible(visible);

    /// <summary> UI操作モード </summary>
    public void EnterUIMode() => inputModeManager.EnterUIMode();

    /// <summary> プレイヤー操作モード </summary>
    public void EnterPlayerMode() => inputModeManager.EnterPlayerMode();

    /// <summary> 操作を受け付けないモード </summary>
    public void DisableInput() => inputModeManager.DisableInput();

    /// <summary> ゲームオーバー </summary>
    public void OnPlayerDied() => gameOverController.PlayGameOver();

    /// <summary> ゲームリトライ </summary>
    public void RetryGame() => cutsceneManager.ReloadCurrentScene();

    /// <summary> タイトル画面に戻る </summary>
    public void ReturnTitle() => cutsceneManager.LoadScene(TITLE_SCENE_NAME);
}