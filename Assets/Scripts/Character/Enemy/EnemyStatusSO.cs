using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// エネミーのScriptableObject定義クラス
/// </summary>
[CreateAssetMenu(fileName = "EnemyStatus", menuName = "ScriptableObject/EnemyStatusSO")]
public class EnemyStatusSO : ScriptableObject
{
    // 複数のエネミーステータスを作れるようにリストで管理
    public List<EnemyStatus> enemyStatusList = new List<EnemyStatus>();

    // エネミーのステータス
    [System.Serializable]
    public class EnemyStatus {
        [SerializeField] private int enemyNumber;   // ステータスを番号指定で選択できる
        [SerializeField] private string enemyName;  // 本編では未使用
        [SerializeField] private int hp;
        [SerializeField] private int atk;

        public int ENEMY_NUMBER { get => enemyNumber;}
        public string ENEMY_NAME { get => enemyName;}
        public int HP { get => hp;}
        public int ATK { get => atk;}
    }
}


