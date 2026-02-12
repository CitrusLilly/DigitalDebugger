using UnityEngine;
using UnityEngine.InputSystem;
/// <summary>
/// プレイヤーの入力モードの切り替えを管理クラス
/// </summary>
public class InputModeController : MonoBehaviour
{
    // ========================定数==========================
    // UI操作時のアクションマップ名
    private const string ACTION_MAP_NAME_UI = "UI";
    // プレイヤー操作時のアクションマップ名
    private const string ACTION_MAP_NAME_PLAYER = "Player";
    // ======================================================

    // プレイヤーのInputSystemを参照
    [SerializeField] private PlayerInput playerInput;

    /// <summary>
    /// 操作モードの種類
    /// </summary>
    public enum InputMode
    {
        None,   // 入力を受け付けない
        Player, // プレイヤー操作モード
        UI      // UI操作モード
    }

    /// <summary>UI操作モード</summary>
    public void EnterUIMode() => SwitchActionMap(InputMode.UI);

    /// <summary>プレイヤー操作モード</summary>
    public void EnterPlayerMode() => SwitchActionMap(InputMode.Player);

    /// <summary>入力を無効化</summary>
    public void DisableInput() => SwitchActionMap(InputMode.None);

    /// <summary>
    /// 操作モードの切り替え
    /// </summary>
    private void SwitchActionMap(InputMode mode) {
        switch (mode) {
            case InputMode.None:
                // 入力を無効化
                playerInput.DeactivateInput();
                break;
            case InputMode.Player:
                // プレイヤー操作のアクションマップに切り替え
                playerInput.ActivateInput();
                playerInput.SwitchCurrentActionMap(ACTION_MAP_NAME_PLAYER);
                break;
            case InputMode.UI:
                // UI操作のアクションマップに切り替え
                playerInput.ActivateInput();
                playerInput.SwitchCurrentActionMap(ACTION_MAP_NAME_UI);
                break;
        }
    }
}
