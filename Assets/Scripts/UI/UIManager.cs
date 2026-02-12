using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// UIの表示管理を行うシングルトン
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [System.Serializable]
    public class UIPanel {
        public UIType uiType;       // どのUIに対応するか
        public GameObject uiRoot;   // UIのルートオブジェクト
    }

    // インスペクターからUIを指定
    [SerializeField] private List<UIPanel> uiList;
    // UI選択用に辞書を用意
    private Dictionary<UIType,GameObject> uiDict;

    void Awake() {
        // シングルトンインスタンスの初期化
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        // UIを辞書登録
        uiDict = new Dictionary<UIType,GameObject>();
        foreach (var uipanel in uiList) {
            uiDict[uipanel.uiType] = uipanel.uiRoot;
        }
    }

    /// <summary>
    /// 指定のUIを表示する
    /// </summary>
    /// <param name="type"> UI識別名 </param>
    public void ShowUI(UIType type) {
        if (uiDict.ContainsKey(type)) {
            uiDict[type].SetActive(true);
        }
    }

    /// <summary>
    /// 指定のUIを非表示にする
    /// </summary>
    /// <param name="type"> UI識別名 </param>
    public void HideUI(UIType type) {
        if (uiDict.ContainsKey(type)) {
            uiDict[type].SetActive(false);
        }
    }

    /// <summary>
    /// 指定のUIが現在開いているかどうかを返す
    /// </summary>
    /// <param name="type"> UI識別名 </param>
    public bool IsUIOpen(UIType type) {
        if (uiDict.ContainsKey(type)) {
            return uiDict[type].activeSelf;
        }
        return false;
    }
}

/// <summary>
/// UI識別名
/// </summary>
public enum UIType
{
    PlayerUI,   // プレイヤー操作画面のUI
    TitleUI,    // タイトル画面のUI
    DialogueUI, // ダイアログテキスト画面のUI
    GameOverUI, // ゲームオーバー画面のUI
    MenuUI,     // メニューUI
    OptionUI    // オプションUI
}
