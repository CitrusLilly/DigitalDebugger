using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
/// <summary>
/// タイトルとゲーム中共通UI操作仮想クラス
/// </summary>
public abstract class UIControllerBase : MonoBehaviour
{
    // ========================定数==========================
    // ===SE名===
    private const string CURSOR_SE_NAME = "cursor";
    private const string CLICK_SE_NAME = "click";
    // ======================================================

    [Header("Option")]
    [SerializeField] protected Selectable firstSelectOption;    // 最初に選択するオプションUI

    [Header("MainUI")]
    [SerializeField] protected Selectable firstSelectMain;      // 最初に選択するメインUI

    protected bool isOptionOpen = false;
    protected GameObject lastSelected;

    protected virtual void Start() {
        UIManager.Instance.HideUI(UIType.OptionUI);
    }

    // Updateでカーソル移動を監視してSE再生処理
    private void Update() {
        // 現在選択中のUI
        var current = EventSystem.current.currentSelectedGameObject;

        if (current != null && current != lastSelected) {
            // カーソル移動音再生
            AudioManager.Instance.PlaySE(CURSOR_SE_NAME);

            lastSelected = current;
        }

        // UI選択が外れたときも検知
        if (current == null) {
            lastSelected = null;
        }
    }

    /// <summary>
    /// マウスカーソルの表示
    /// </summary>
    /// <param name="visible"> 表示有無 </param>
    public void SetCursorVisible(bool visible) {
        if (visible) {
            Cursor.lockState = CursorLockMode.None;
        } else {
            Cursor.lockState = CursorLockMode.Locked;
        }
        Cursor.visible = visible;
    }


    /// <summary>
    /// オプション画面の開閉
    /// </summary>
    public virtual void ToggleOption() {
        isOptionOpen = !isOptionOpen;       // 反転
        if (isOptionOpen) {                 // 開閉
            UIManager.Instance.ShowUI(UIType.OptionUI);
        } else {
            UIManager.Instance.HideUI(UIType.OptionUI);
        }

        // オプションを開くと指定のオプションを選択状態にする
        if (isOptionOpen) {
            firstSelectOption.Select();
        } else {
            // 閉じるとメインの指定UIを選択状態にする
            firstSelectMain.Select();
        }

        PlaySelectSE();
    }

    /// <summary> 決定音再生</summary>
    protected virtual void PlaySelectSE() => AudioManager.Instance.PlaySE(CLICK_SE_NAME);
}
