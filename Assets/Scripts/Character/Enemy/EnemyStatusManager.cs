using UnityEngine;
/// <summary>
/// エネミーステータス管理クラス
/// CharacterStatusManagerを継承
/// EnemyStatusSOから番号指定でステータスを読み込む
/// </summary>
public class EnemyStatusManager : CharacterStatusManager
{
    // ========================定数==========================
    // デフォルトステータス(番号指定が範囲外の場合に使用)
    private const int DEFAULT_HP = 100;
    private const int DEFAULT_ATTACK = 2;
    // ======================================================

    // インスペクターからセット
    [Header("EnemyStatusSetting")]
    [SerializeField] private EnemyStatusSO statusSO;

    // 番号を指定してScriptableObjectのステータスを参照
    [SerializeField] private int enemyNumber = -1;

    private void Awake() {
        // 番号指定がエネミーリストの範囲内ならステータスを適用
        if (IndexCheck()) {
            var status = statusSO.enemyStatusList[enemyNumber];
            MaxHp = status.HP;
            Attack = status.ATK;
        } else { // 初期値と範囲外はデフォルトステータスを適用
            Debug.LogWarning("エネミー指定が無効です。デフォルトステータスを適用します。");
            MaxHp = DEFAULT_HP;
            Attack = DEFAULT_ATTACK;
        }
        CurrentHp = MaxHp;
    }

    /// <summary>
    /// 番号指定が有効化どうか
    /// </summary>
    private bool IndexCheck() {
        return statusSO != null &&
               statusSO.enemyStatusList != null &&
               enemyNumber >= 0 &&
               enemyNumber < statusSO.enemyStatusList.Count;
    }

    /// <summary>
    /// ダウンからの再起動処理。
    /// HPを最大値に戻す。
    /// </summary>
    public void Reboot() {
        CurrentHp = MaxHp;
    }
}