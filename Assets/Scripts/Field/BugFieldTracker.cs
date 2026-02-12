using UnityEngine;
using System;
/// <summary>
/// バグフィールドの管理クラス
/// </summary>
public class BugFieldTracker : MonoBehaviour
{
    // 定数 テキストファイル名
    private const string CLEAR_TEXTFILE_NAME = "ClearText";

    [Header("Setting")]
    [SerializeField] private int initBugField = 5;      // 開始時のバグフィールドの数
    [SerializeField] private GameObject clearPoint;     // 接触するとゲームクリアとなるオブジェクト

    private int currentBugField;    // 現在のバグフィールド数

    // 残りフィールド数変更時イベント
    // ProgressUI.UpdateProgressを購読
    // GameManagerでイベント購読を中継
    public event Action<int> OnBugCountChanged;

    private void Awake() {
        currentBugField = initBugField;
    }

    private void Start() {
        // ゲームクリアポイントの非アクティブ化
        clearPoint.SetActive(false);
    }

    /// <summary>
    /// 現在の残りエリア数を通知(初期化時に使用)
    /// </summary>
    public void ShowBugCount() {
        OnBugCountChanged?.Invoke(currentBugField);
    }

    /// <summary>
    /// バグフィールドを１つクリア時処理
    /// </summary>
    public void ClearBug() {
        currentBugField--;
        OnBugCountChanged?.Invoke(currentBugField);

        if (currentBugField <= 0) {
            OnAllBugsCleared();
        }
    }

    /// <summary>
    /// 全てのバグフィールドをクリアした時のイベント
    /// </summary>
    private void OnAllBugsCleared() {
        // ゲームクリアポイントを有効化
        clearPoint.SetActive(true);

        // クリア時ダイアログ再生
        StartCoroutine(DialogueManager.Instance.PlayDialogue(CLEAR_TEXTFILE_NAME));
    }
}

