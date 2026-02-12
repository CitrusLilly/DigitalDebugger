using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
/// <summary>
/// テキストダイアログの管理を行うシングルトン
/// </summary>
public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }
    
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI dialogueTextUI;    // テキスト表示先
    [SerializeField] private GameObject indicator;              // 入力待ちを示す表示

    [Header("Setting")] 
    [SerializeField] private float textSpeed = 0.07f;   // 文字が表示される間隔(秒)

    private bool inputTriggered = false;    // プレイヤーの入力が行われたかどうか

    void Awake() {
        // シングルトンインスタンスの初期化
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start() {
        // 初期ではダイアログ関係は非表示にする
        dialogueTextUI.text = "";
        indicator.SetActive(false);
        ChangeDialogueUI(false);
    }

    /// <summary>
    /// ダイアログイベントを開始する。
    /// </summary>
    /// <param name="fileName"> Resourceに入っているファイル名を指定 </param>
    public IEnumerator PlayDialogue(string fileName) {
        // ダイアログUIに切り替え
        ChangeDialogueUI(true);

        // jsonファイルを読み込む
        TextAsset jsonFile = Resources.Load<TextAsset>(fileName);
        if (jsonFile == null) {
            Debug.LogError($"Jsonファイル {fileName} が見つかりません");
            ChangeDialogueUI(false); // UI戻しておく
            yield break;
        }

        // DialogueDataクラスにオブジェクト変換
        DialogueData dialogueData = JsonUtility.FromJson<DialogueData>(jsonFile.text);

        // セリフを順番に表示
        foreach (string text in dialogueData.texts) {
            indicator.SetActive(false);
            dialogueTextUI.text = "";

            // １文字ずつ表示
            foreach (char c in text) {
                dialogueTextUI.text += c;
                if (inputTriggered) { // 途中で入力が入ると全文表示
                    dialogueTextUI.text = text;
                    break; 
                }
                yield return new WaitForSeconds(textSpeed);
            }
            
            inputTriggered = false;

            // １行全て表示しきったら入力待ち状態へ
            indicator.SetActive(true);
            yield return new WaitUntil(() => inputTriggered);
            inputTriggered = false;
        }

        // 全文表示後にプレイヤーUIに戻す終了処理
        dialogueTextUI.text = "";
        ChangeDialogueUI(false);
    }

    /// <summary>
    /// プレイヤーUIとダイアログUIの切り替えと操作モードを切り替える
    /// </summary>
    /// <param name="isDialogue"> true:ダイアログ表示中 / false:プレイヤー操作中 </param>
    private void ChangeDialogueUI(bool isDialogue) {
        if (isDialogue) {
            UIManager.Instance.HideUI(UIType.PlayerUI);
            UIManager.Instance.ShowUI(UIType.DialogueUI);
            GameManager.Instance.EnterUIMode();
        } else {
            UIManager.Instance.HideUI(UIType.DialogueUI);
            UIManager.Instance.ShowUI(UIType.PlayerUI);
            GameManager.Instance.EnterPlayerMode();
        }
    }

    // InputSytemの入力受け取り
    public void OnNext(InputAction.CallbackContext context) {
        // ダイアログUIが表示されている場合のみ処理
        if (!UIManager.Instance.IsUIOpen(UIType.DialogueUI)) return;

        if (context.performed) {
            inputTriggered = true;
        }
    }

}

/// <summary>
/// JSONデータ構造体。セリフを順に格納
/// </summary>
[Serializable]
public class DialogueData {
    public string[] texts;
}