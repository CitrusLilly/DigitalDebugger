using System;
using UnityEngine;
using UnityEngine.Events;
/// <summary>
/// プレイヤーが触れた時にイベント実行やオブジェクト操作・生成を行うトリガークラス
/// </summary>
public class EventTrigger : MonoBehaviour
{
    [Header("Event")] // インスペクターからイベント(メソッド)登録
    [SerializeField] private UnityEvent triggerEvent;

    [Header("ObjectHandling")] // インスペクターから追加
    [SerializeField] private GameObject[] destroyObjects;   // 削除対象
    [SerializeField] private GameObject[] showObjects;      // 表示対象
    [SerializeField] private GameObject[] hideObjects;      // 非表示対象
    [SerializeField] private GameObject[] initHideObjects;  // 初期非表示対象
    [SerializeField] private PopObjectInfo[] popObjects;    // 生成対象情報

    [Header("EnemySetting")] // 移動可能エリアを指定
    [SerializeField] private NavMeshArea moveArea;

    [Header("SpawnUpdate")] // プレイヤーのスポーン地点の更新有無
    [SerializeField] private bool isSpawnUpdate = false;

    private void Start() {
        // 初期非表示
        foreach (GameObject obj in initHideObjects) {
            if (obj != null) {
                obj.SetActive(false);
            }
        }
    }

    private void OnTriggerEnter(Collider other) {
        // プレイヤーだけ検知
        if (!other.CompareTag(Player.TAG_NAME)) return;
        Player player = other.GetComponent<Player>();

        // １回接触すればコライダーを無効化させる
        GetComponent<Collider>().enabled = false;

        // 登録されたイベントを実行
        triggerEvent?.Invoke();

        // インスペクターで指定されたオブジェクトの操作
        DestroyAllObject();
        ShowAllObject();
        HideAllObject();
        PopAllObject(player);

        // プレイヤーのスポーン位置の更新
        if (isSpawnUpdate) { 
            GameManager.Instance.SpawnPoint = transform.position;
        }
    }
    
    /// <summary>
    /// 指定オブジェクトを削除
    /// </summary>
    private void DestroyAllObject() {
        foreach(GameObject obj in destroyObjects) {
            if (obj != null) {
                Destroy(obj);
            }
        }
    }

    /// <summary>
    /// 指定オブジェクトを表示
    /// </summary>
    private void ShowAllObject() {
        foreach (GameObject obj in showObjects) {
            if (obj != null) {
                obj.SetActive(true);
            }
        }
    }

    /// <summary>
    /// 指定オブジェクトを非表示
    /// </summary>
    private void HideAllObject() {
        foreach (GameObject obj in hideObjects) {
            if(obj != null) {
                obj.SetActive(false);
            }
        }
    }

    /// <summary>
    /// オブジェクトを生成
    /// </summary>
    private void PopAllObject(Player player) {
        bool isEnemy = false;   // エネミーを出現させたかフラグ

        foreach (PopObjectInfo obj in popObjects) {
            GameObject instance = Instantiate(obj.PopObject,obj.SpawnPoint);

            // 生成対象がエネミーの場合はエネミー生成の処理
            if (obj.IsEnemy) {
                isEnemy = true;
                EnemyManager.Instance.EnemySetup(instance, player,moveArea);
            }
        }

        // エネミー出現フラグが立っていたら出現SE再生
        if (isEnemy) {
            EnemyManager.Instance.PlayPopSE();
        }
    }
}

/// <summary>
/// 生成オブジェクトの情報クラス
/// </summary>
[Serializable]
public class PopObjectInfo {
    public GameObject PopObject;    // 生成オブジェクト
    public Transform SpawnPoint;    // 生成座標
    public bool IsEnemy;            // 生成対象がエネミーかどうか
}