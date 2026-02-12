using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using TMPro;
/// <summary>
/// プレイヤーのUI管理クラス
/// </summary>
public class PlayerUI : MonoBehaviour
{
    [Header("Component")] // インスペクターから設定
    [SerializeField] private PlayerStatusManager statusManager;     // HPイベント用
    [SerializeField] private RecastManager recastManager;           // リキャストイベント用
    [SerializeField] private Player player;                         // アクション入力イベント用

    [Header("HP UI")]
    [SerializeField] private GameObject hpBar;                  // HPアイコン表示領域
    [SerializeField] private Image hpIconPrefab;                // HPアイコン画像プレハブ
    [SerializeField] private float warningHp = 3;               // HPアイコンを警告色にするしきい値
    [SerializeField] private Color warningColor = Color.red;    // HPアイコンの警告色
    private List<Image> hpIconList = new();                     // HPアイコンをリストで管理
    private bool isWarningHp = false;                           // HPが警告値以下かどうか

    [Header("Skill UI")]
    [SerializeField] private Slider skillIcon;  // リキャストをスライダーで表示
    [SerializeField] private Image skillFrame;  // 入力時の点灯フレーム

    [Header("Guard UI")]
    [SerializeField] private Slider guardIcon;
    [SerializeField] private Image guardFrame;

    [Header("Attack UI")]
    [SerializeField] private Image attackFrame;

    [Header("Dodge UI")]
    [SerializeField] private Image dodgeFrame;

    [Header("SmashCount")]
    [SerializeField] private TextMeshProUGUI smashCountText;    // スマッシュ数のカウント表示先
    [SerializeField] private float smashCountFontsize = 100f;   // スマッシュ数のテキストサイズ

    [Header("Score")]
    [SerializeField] private TextMeshProUGUI scoreText;         // スコア表示先

    [Header("Progress")]
    [SerializeField] private GameObject progressUI;             // 目標表示用のUIオブジェクト
    [SerializeField] private TextMeshProUGUI progressText;      // 目標テキスト表示先

    private void Start() {
        if(!ValidateReferences()) return;

        SubscribeEvents();
        InitializeUI();
    }

    /// <summary>
    /// 必要な参照が正しく設定されているか確認
    /// </summary>
    private bool ValidateReferences() {
        if (statusManager == null) {
            Debug.LogError("PlayerStatusManagerが設定されていません。");
            return false;
        }

        if (recastManager == null) {
            Debug.LogError("RecastManagerが設定されていません。");
            return false;
        }

        if (player == null) {
            Debug.LogError("Playerが設定されていません。");
            return false;
        }

        return true;
    }

    /// <summary>
    /// イベントに関数を登録
    /// </summary>
    private void SubscribeEvents() {
        statusManager.OnHpChanged += UpdateHpIcons;
        recastManager.OnSkillRecast += UpdateSkillCooldown;
        recastManager.OnGuardRecast += UpdateGuardCooldown;
        player.InputHandler.OnActionButton += UpdateActionFrame;
        SmashController.Instance.OnSmashCountChanged += UpdateSmashCount;
        ScoreManager.Instance.OnScoreChanged += UpdateScore;
        GameManager.Instance.OnBugCountChanged += UpdateProgress;
    }

    /// <summary>
    /// 初期UIのセットアップ
    /// </summary>
    private void InitializeUI() {
        CreateHpIcons(statusManager.MaxHp);

        UpdateSkillCooldown(recastManager.SkillTimer, recastManager.SkillRecast);
        UpdateGuardCooldown(recastManager.GuardTimer, recastManager.GuardRecast);

        SetAllActionFrames(false);

        UpdateSmashCount(0); 
        UpdateScore(0);

        progressText.text = "";
        progressUI.SetActive(false);
    }

    /// <summary>
    /// 指定された最大HP分のHPアイコンを生成
    /// </summary>
    private void CreateHpIcons(int _maxHp) {
        var layout = hpBar.GetComponent<HorizontalLayoutGroup>();
        layout.enabled = true; // 一時的に有効化

        for (int i = 0; i < _maxHp; i++) {
            Image icon = Instantiate(hpIconPrefab, hpBar.transform);
            icon.name = $"HPIcon_{i}";
            hpIconList.Add(icon);
        }

        // 少し待たないと整列されないので少し待つ
        StartCoroutine(DisableLayoutNextFrame(layout));
    }
    // 1フレーム待ってからレイアウトグループを無効化(整列のため)
    IEnumerator DisableLayoutNextFrame(HorizontalLayoutGroup layout) {
        yield return null; 
        layout.enabled = false;
    }


    /// <summary>
    /// HP更新時に呼ばれるUI処理
    /// </summary>
    private void UpdateHpIcons(int currentHp, int maxHp) {
        for (int i = currentHp; i < maxHp; i++) {
            if (hpIconList[i].enabled) {
                // HPが減った分を無効化する
                hpIconList[i].enabled = false;
            } else {
                break;
            }
        }

        // HP警告値以下ならアイコンの色を1回だけ変更処理
        if (!isWarningHp && currentHp <= warningHp) {
            for (int i = 0; i < warningHp; i++) {
                hpIconList[i].color = warningColor;
            }
            isWarningHp = true;
        }
    }

    /// <summary>
    /// スマッシュカウントの表示更新
    /// </summary>
    /// <param name="count"> 現在のスマッシュカウント数 </param>
    private void UpdateSmashCount(int count) {
        // カウント0で非表示
        if (count == 0) {
            smashCountText.enabled = false;
            smashCountText.text = string.Empty;
            return;
        }

        // 表示更新
        smashCountText.text = $"<size={smashCountFontsize}>{count}</size> Smash";
        smashCountText.enabled = true;
    }

    /// <summary>
    /// トータルスコア表示更新
    /// </summary>
    /// <param name="total"> 現在のトータルスコア </param>
    private void UpdateScore(int total) {
        scoreText.text = total.ToString();
    }

    /// <summary>
    /// 目的として残りエリア数をUI表示に反映させる。
    /// </summary>
    private void UpdateProgress(int currentBug) {
        if (!progressUI.activeSelf) {
            progressUI.SetActive(true);
        }
        progressText.text = $"残りエリア数： {currentBug}";
    }

    /// <summary>
    /// スキルのクールダウン表示を更新
    /// </summary>
    private void UpdateSkillCooldown(float currentTime, float colldownTime) {
        skillIcon.maxValue = colldownTime;
        skillIcon.value = currentTime;
    }
    /// <summary>
    /// ガードのクールダウン表示を更新
    /// </summary>
    private void UpdateGuardCooldown(float currentTime, float colldownTime) {
        guardIcon.maxValue = colldownTime;
        guardIcon.value = currentTime;
    }

    /// <summary>
    /// プレイヤーの入力に応じて対応するフレームを点灯・消灯
    /// </summary>
    /// <param name="button"></param>
    /// <param name="pressed"></param>
    private void UpdateActionFrame(Player.ActionButton button, bool pressed) {
        switch (button) {
            case Player.ActionButton.Attack:
                attackFrame.enabled = pressed;
                break;
            case Player.ActionButton.Guard:
                guardFrame.enabled = pressed;
                break;
            case Player.ActionButton.Dodge:
                dodgeFrame.enabled = pressed;
                break;
            case Player.ActionButton.Skill:
                skillFrame.enabled = pressed;
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 全ての入力フレームの表示を設定
    /// </summary>
    private void SetAllActionFrames(bool enabled) {
        attackFrame.enabled = enabled;
        guardFrame.enabled = enabled;
        dodgeFrame.enabled = enabled;
        skillFrame.enabled = enabled;
    }

    /// <summary>
    /// メモリリーク対策
    /// </summary>
    private void OnDestroy() {
        if (statusManager != null) {
            statusManager.OnHpChanged -= UpdateHpIcons;
        }

        if (player != null) {
            player.InputHandler.OnActionButton -= UpdateActionFrame;
        }

        if (recastManager != null) {
            recastManager.OnSkillRecast -= UpdateSkillCooldown;
            recastManager.OnGuardRecast -= UpdateGuardCooldown;
        }

        if (SmashController.Instance != null) {
            SmashController.Instance.OnSmashCountChanged -= UpdateSmashCount;
        }

        if (ScoreManager.Instance != null) {
            ScoreManager.Instance.OnScoreChanged -= UpdateScore;
        }

        if (GameManager.Instance != null) {
            GameManager.Instance.OnBugCountChanged -= UpdateProgress;
        }
    }
}
