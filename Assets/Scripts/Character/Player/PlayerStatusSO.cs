using UnityEngine;
/// <summary>
/// プレイヤーのScriptableObject定義クラス
/// </summary>
[CreateAssetMenu(fileName = "PlayerStatus", menuName = "ScriptableObject/PlayerStatusSO")]
public class PlayerStatusSO : ScriptableObject
{
    // プレイヤーのステータス
    [SerializeField] private string playerName; // ゲーム内では使用しなかった
    [SerializeField] private int hp;
    [SerializeField] private int atk;

    public string PLAYER_NAME { get => playerName; }
    public int HP { get => hp; }
    public int ATK { get => atk; }

}

