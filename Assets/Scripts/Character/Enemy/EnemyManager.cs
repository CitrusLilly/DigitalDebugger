using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// エリアに出現するエネミーを管理するシングルトン
/// </summary>
public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    // エネミー管理用リスト
    private List<Enemy> activeEnemies = new List<Enemy>();
    // エネミー出現エフェクト
    [SerializeField] private ParticleSystem spawnEffect;

    private void Awake() {
        // シングルトンインスタンスの初期化
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// エネミーをリストに追加して管理
    /// </summary>
    public void AddEnemy(Enemy enemy) {
        if (enemy != null) {
            activeEnemies.Add(enemy);
        }
    }

    /// <summary>
    /// エネミー出現時のSEを再生
    /// </summary>
    public void PlayPopSE() {
        AudioManager.Instance.PlaySE(Enemy.POP_SE_NAME);
    }

    /// <summary>
    /// エネミーの初期化処理
    /// </summary>
    public void EnemySetup(GameObject instance, Player player,NavMeshArea area) {
        Enemy enemy = instance.GetComponent<Enemy>();
        if (enemy != null) {
            // プレイヤーの参照と移動可能エリアを渡してエネミーリストに追加
            enemy.OnTargetDetected(player, area);
            AddEnemy(enemy);
            // 出現エフェクト生成
            ShowSpawnEffect(instance.transform.position);
        }
    }

    /// <summary>
    /// エネミーの出現位置に出現エフェクトを出す
    /// </summary>
    /// <param name="popPos"> 出現位置 </param>
    private void ShowSpawnEffect(Vector3 popPos) {
        Vector3 pos = popPos + spawnEffect.transform.position;
        ParticleSystem effect = Instantiate(spawnEffect, pos, Quaternion.identity);
        Destroy(effect.gameObject, spawnEffect.main.duration);
    }

    /// <summary>
    /// 全てのエネミーを死亡させる&リストの初期化
    /// </summary>
    public void DestroyAllEnemies() {
        foreach (var enemy in activeEnemies) {
            if (enemy != null) {
                enemy.CharacterDie();
            }
        }
        activeEnemies.Clear();
        // エネミー爆発SE再生
        AudioManager.Instance.PlaySE(Enemy.EXPLOSION_SE_NAME);
    }

    /// <summary>
    /// 対象位置から一番近いエネミーを取得
    /// </summary>
    /// <param name="fromPosition"> 基準位置 </param>
    /// <param name="maxDistance"> 最長探索距離 </param>
    /// <returns> 一番近いエネミーのTransformを返す </returns>
    public Transform GetNearestEnemy(Vector3 fromPosition,float maxDistance) {
        Transform nearest = null;
        float minDistance = Mathf.Infinity;

        foreach (var enemy in activeEnemies) {
            if (enemy == null) continue;
            
            float distance = Vector3.Distance(fromPosition, enemy.transform.position);
            // 最長探索距離以内かつ、一番近いエネミーを探す
            if (distance < maxDistance && distance < minDistance) {
                minDistance = distance;
                nearest = enemy.gameObject.transform;
            }
        }

        return nearest;
    }

    /// <summary>
    /// ターゲットの死をリスト内のエネミーに伝達
    /// </summary>
    public void TargetDie() {
        foreach(var enemy in activeEnemies) {
            enemy.ClearTarget();
        }
    }
}
