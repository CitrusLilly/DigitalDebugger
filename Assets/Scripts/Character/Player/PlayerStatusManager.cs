using UnityEngine;
/// <summary>
/// プレイヤーのステータス管理クラス
/// CharacterStatusManagerを継承
/// PlayerStatusSOからステータスを読み込む
/// </summary>
public class PlayerStatusManager : CharacterStatusManager {
    // インスペクターからセット
    [SerializeField] PlayerStatusSO statusSO;

    private void Awake() {
        MaxHp = statusSO.HP;
        CurrentHp = MaxHp;
        Attack = statusSO.ATK;
    }

}