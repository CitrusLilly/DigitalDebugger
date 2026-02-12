using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
/// <summary>
/// ゲーム中のメニュー全般の処理クラス
/// UIControllerBaseを継承
/// </summary>
public class MenuManager : UIControllerBase
{
    [Header("SystemText")]
    [SerializeField] private TextMeshProUGUI systemText;

    // メニューの開閉状態
    private bool isMenuOpen = false;
    // メニュー開閉操作が出来るかどうか
    private bool canMenuOpen = true;

    protected override void Start() {
        base.Start();
        // 初期設定
        UIManager.Instance.HideUI(UIType.MenuUI);
        UIManager.Instance.HideUI(UIType.GameOverUI);
        systemText.text = "";
    }

    /// <summary>
    /// システムテキストの表示
    /// </summary>
    public void ShowSystemMessage(string _text) {
        systemText.text = _text;
    }

    /// <summary>
    /// メニュー開閉(InputSystemイベント経由)
    /// </summary>
    public void ToggleMenu(InputAction.CallbackContext context) {
        // UIボタンでの使用時はDisabledになるのでperformedを無視
        if (context.performed || context.phase == InputActionPhase.Disabled) {
            if (!canMenuOpen) return;

            // 表示状態を反転させる
            isMenuOpen = !isMenuOpen;
            // 操作モード、メニュー画面の表示、一時停止の切替え
            if (isMenuOpen) {
                GameManager.Instance.EnterUIMode();
                UIManager.Instance.ShowUI(UIType.MenuUI);
                firstSelectMain.Select();
                Time.timeScale = 0;
            } else {
                GameManager.Instance.EnterPlayerMode();
                UIManager.Instance.HideUI(UIType.MenuUI);
                Time.timeScale = 1f;
            }

            // 決定音再生
            PlaySelectSE();

            // カーソルのセット状態変更
            GameManager.Instance.SetCursorVisible(isMenuOpen);
        }
    }

    /// <summary>
    /// メニュー開閉(UIボタン経由)
    /// </summary>
    public void ToggleMenu() {
        // デフォルトを渡して処理を呼び出す
        ToggleMenu(default);
    }
}
