using UnityEngine;

/// <summary>
/// 壊れているエネミー(吹き飛ばしオブジェクト)
/// 一定時間転がった後、元の位置に戻る
/// </summary>
public class BrokenEnemy : Enemy 
{
    [Header("BrokenSetting")]
    [SerializeField] private float rollDuration = 3f;       // 転がっている時間(秒)
    [SerializeField] private float detectionRadius = 7f;    // プレイヤー検知範囲
    [SerializeField] private LayerMask playerLayer;         // 検知に使用するプレイヤーのレイヤー

    private float rollTimer = 0;
    private bool IsRolling = false;     // 現在転がっているか
    private bool isDetection = false;   // 検知したか

    // 自立して動かないので必要最低限だけの設定
    protected override void Start() {
        Rb = GetComponent<Rigidbody>();

        IsReboot = false;       // 再起動なしのダウンから復活しないエネミー
        ChangeState(DownState);

        initialPosition = transform.position;
    }

    // エネミーの通常処理 + プレイヤー検知処理
    protected override void Update() {
        base.Update();

        // 検知されるまで処理を実行
        if (isDetection) return;

        // プレイヤーが一定距離に入ると自身をエネミーリストに登録        
        if (Physics.CheckSphere(transform.position, detectionRadius, playerLayer)) {
            EnemyManager.Instance.AddEnemy(this);
            isDetection = true;
        }
    }

    // エネミーの通常処理 + 転がり状態の処理
    protected override void FixedUpdate() {
        // 通常処理の前に転がされた判定を拾って転がり開始状態にする
        if (IsRolled) {
            IsRolling = true;
        }

        base.FixedUpdate();

        if (IsRolling) {
            rollTimer += Time.fixedDeltaTime;
            
            // 転がってから指定時間後に初期位置に戻す
            if(rollTimer >= rollDuration) {
                transform.position = initialPosition;
                transform.rotation = Quaternion.identity; // 回転も戻さないと埋まってしまう

                Rb.isKinematic = true;

                gameObject.layer = LayerMask.NameToLayer(NORMAL_LAYER_NAME); // レイヤーを戻す

                IsRolling = false;
                rollTimer = 0;
            }
        }
    }

#if UNITY_EDITOR
    // 検知範囲の可視化(デバッグ用)
    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
#endif

}
