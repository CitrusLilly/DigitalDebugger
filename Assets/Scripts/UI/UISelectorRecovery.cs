using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
/// <summary>
/// InputSystemの入力を監視して選択状態に戻すクラス
/// </summary>
public class UISelectorRecovery : MonoBehaviour
{
    [Header("UIGroup")] // 上から優先的に選ばれる
    [SerializeField] private List<RecoverableUIGroup> uiGroups = new List<RecoverableUIGroup>();


    /// <summary>
    /// 非選択状態から選択状態に戻す
    /// </summary>
    public void OnRecovery(InputAction.CallbackContext context) {
        // 選択中は処理しない
        if (EventSystem.current.currentSelectedGameObject != null) return;

        // アクティブ状態のUIに選択を戻す
        foreach (var group in uiGroups) {
            if (group.uiRoot != null && group.uiRoot.activeInHierarchy) {
                if (group.recoverySelectable != null) {
                    group.recoverySelectable.Select();
                    return;
                }
            }
        }
    }
}


[Serializable]
public class RecoverableUIGroup {
    public GameObject uiRoot;               // このUIグループの親(有効状態チェックに使う)
    public Selectable recoverySelectable;   // 選択が外れたときに戻したいUI
}