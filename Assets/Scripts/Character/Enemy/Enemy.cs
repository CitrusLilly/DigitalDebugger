using System.Collections;
using UnityEngine;
using UnityEngine.AI;
/// <summary>
/// エネミーの管理クラス
/// </summary>
public class Enemy : Character, IDamageable ,IHitStop
{
    // ========================定数==========================
    // ===タグ名===
    public const string TAG_NAME = "Enemy";
    // ===レイヤー名===
    public const string NORMAL_LAYER_NAME   = "Enemy";
    public const string BALL_LAYER_NAME     = "EnemyBall";
    // ===SE名===
    public const string ATTACK_SE_NAME      = "attack_enemy";
    public const string CHARGE_SE_NAME      = "charge_enemy";
    public const string HIT_SE_NAME         = "hit_enemy";
    public const string WALL_HIT_SE_NAME    = "wall_hit_enemy";
    public const string POP_SE_NAME         = "pop_enemy";
    public const string EXPLOSION_SE_NAME   = "explosion_enemy";
    // ===アニメーション===
    public const string ANIM_TRIGGER_KNOCKBACK  = "KnockBack";
    public const string ANIM_TRIGGER_DOWN       = "Down";
    public const string ANIM_TRIGGER_REBOOT     = "Reboot";
    public const string ANIM_BOOL_ATTACK        = "Attack";
    public const string ANIM_BOOL_BATTLE        = "Battle";
    public const string ANIM_FLOAT_KNOCKBACK    = "KnockBackSpeed";
    // ======================================================

    // ==================依存コンポーネント====================
    [Header("Component")]
    public NavMeshAgent Agent;          // 敵AIナビメッシュエージェント
    public EnemyAttackHitBox HitBox;    // 攻撃判定処理
    public Rigidbody Rb;                // ダウン時の物理挙動で使用
    public Animator EnemyAnimator { get; private set; }
    public EnemyStatusManager Status { get; private set; } // ステータス
    public StateMachine EnemyStateMachine { get; private set; } // Stateパターンで管理
    // ======================================================

    // ==================各ステート===========================
    public IState IdleState { get; private set; }
    public IState BattleState { get; private set; }
    public IState AttackState { get; private set; }
    public IState DamagedState { get; private set; }
    public IState DownState { get; private set; }
    // ======================================================

    // ==================移動関連=============================
    [Header("MoveSetting")]
    public float MoveSpeed { get; private set; }    // 通常移動速度
    protected Vector3 initialPosition;              // スポーン位置
    public Player Target { get; set; }              // 攻撃対象
    // ======================================================

    // ==================被ダメージ関連=======================
    [Header("DamagedSetting")]
    public float DamagedTimer { get; set; } = 0;
    public float DamagedStateDuration { get; private set; }
    [SerializeField] private float kbAnimSpeedPlayer = 0.2f;     // プレイヤーに攻撃された時にノックバックアニメーション速度
    [SerializeField] private float kbAnimSpeedGuarded = 0.067f;  // プレイヤーにガードされた時にノックバックアニメーション速度
    [SerializeField] private float kbAnimSpeedBall = 0.5f;       // ボールに当たった時のノックバックアニメーション速度
    // ======================================================

    // ==================ダウン関連===========================
    [Header("DownSetting")]
    public float DownDuration = 4f;                 // ダウン状態の時間(秒)
    public bool IsRolled;                           // 転がされたか
    public Vector3 LastVelocity;                    // 壁に衝突する前の速度
    public bool IsReboot = true;                    // 復帰するエネミーかどうか
    [SerializeField] private LayerMask bugLayer;    // ボール状態で衝突するバグの壁レイヤー
    [SerializeField] private LayerMask ballLayer;   // ダウン状態で衝突するボールのレイヤー
    // ======================================================

    // ==================攻撃関連=============================
    [Header("AttackSetting")]
    [SerializeField] private float attackDistance = 2.5f;   // 攻撃ステートに切り替わる距離
    [SerializeField] private float attackSpeed = 5f;        // 突進速度
    [SerializeField] private float attackMoveDistance = 3f; // 突進距離
    public bool CanMove;            // 移動可能フラグ
    public bool CanAttackingLook;   // 攻撃中にターゲットを向けるか
    public bool Attacking;          // 攻撃中フラグ
    // ======================================================

    // ==================エフェクト関連=======================
    [Header("Effect")]
    [SerializeField] private ParticleSystem[] AttackEffects;    // 攻撃エフェクト  Index:0 チャージ,1 突進
    [SerializeField] private ParticleSystem hitEffect;          // 被弾エフェクト
    [SerializeField] private TrailRenderer ballModeTrail;       // ボール状態の時に出す軌跡エフェクト
    [SerializeField] private GameObject dieEffectPrefab;        // 死亡時爆発エフェクトプレハブ
    // ======================================================


    private void Awake() {
        InitComponents();
        InitState();
    }

    protected virtual void Start() {
        // ナビメッシュエージェントの速度の値を通常速度として記憶
        MoveSpeed = Agent.speed;
        // 初期ステート
        ChangeState(IdleState);

        // 当たり判定に攻撃力を通知
        if (Status != null && HitBox != null) {
            HitBox.BaseAttack = Status.Attack;
        }

        // 初期位置を記憶
        initialPosition = transform.position;
    }

    protected virtual void Update() => EnemyStateMachine.Update();

    protected virtual void FixedUpdate() => EnemyStateMachine.FixedUpdate();

    /// <summary>
    /// コンポーネントの初期化
    /// </summary>
    private void InitComponents() {
        if (Agent == null) {
            Agent = GetComponent<NavMeshAgent>();
        }
        if (EnemyAnimator == null) {
            EnemyAnimator = GetComponent<Animator>();
        }
        if (Rb == null) {
            Rb = GetComponent<Rigidbody>();
        }
        if (Status == null) {
            Status = GetComponent<EnemyStatusManager>();
        }

        EnemyStateMachine = new StateMachine();
    }

    /// <summary>
    /// 各ステートの初期化
    /// </summary>
    private void InitState() {
        IdleState = new EnemyIdleState(this);
        BattleState = new EnemyBattleState(this);
        AttackState = new EnemyAttackState(this);
        DamagedState = new EnemyDamagedState(this);
        DownState = new EnemyDownState(this);
    }

    /// <summary>
    /// エネミーの状態を変更
    /// </summary>
    /// <param name="newState"> 遷移先ステート </param>
    public void ChangeState(IState newState) => EnemyStateMachine.ChangeState(newState);

    #region 行動ロジック

    /// <summary>
    /// ターゲットとの距離が攻撃範囲内かどうか返す
    /// </summary>
    public bool IsInAttackRange() {
        if (Target == null) return false;

        // ターゲットとの距離
        float distance = Vector3.Distance(transform.position, Target.transform.position);

        // 攻撃範囲内ならTrue
        return distance < attackDistance;
    }

    /// <summary>
    /// 対象を捕捉しながら攻撃を実行。
    /// </summary>
    public void Attack() {
        // ナビメッシュエージェントが有効な場合のみ攻撃
        if (!Agent.enabled) return;

        // 振り向きが有効な間だけ回転
        if (CanAttackingLook) {
            // ターゲットの方角に向きを合わせて狙いを定める
            Agent.destination = Target.transform.position;
            Vector3 direction = Target.transform.position - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = targetRotation;
        }
        if (EnemyAnimator != null) {
            EnemyAnimator.SetBool(ANIM_BOOL_ATTACK, true);
        }
    }

    /// <summary>
    /// 攻撃対象を指定
    /// </summary>
    /// <param name="target"> 対象のゲームオブジェクト </param>
    /// <param name="moveArea"> 移動可能なエリア </param>
    public void OnTargetDetected(Player target, NavMeshArea moveArea) {
        // ターゲット指定
        Target = target;
        // 特定のエリアだけ動けるように指定
        Agent.areaMask = 1 << NavMesh.GetAreaFromName(moveArea.ToString());
    }

    /// <summary>
    /// ターゲット情報を削除
    /// </summary>
    public void ClearTarget() {
        Target = null;
    }

    /// <summary>
    /// リスポーン処理
    /// </summary>
    public void Warp() {
        if (Agent != null) {
            Agent.Warp(initialPosition);
        }
    }

    #endregion

    #region ダメージ処理とノックバック

    // ダメージ処理
    public DamageReaction TakeDamage(int damage) {
        // ダウン時
        if (EnemyStateMachine.CurrentState == DownState) {
            return DamageReaction.Smash;
        }

        // 通常時はダメージ計算を行う
        if (Status.HpCalc(damage)) {
            // HP0ならダウン処理
            EnemyDown();
            return DamageReaction.Down;
        }

        // 攻撃中ならノックバックしない
        if (Attacking) {
            return DamageReaction.DamagedOnly;
        }

        // それ以外はダメージとノックバック
        return DamageReaction.Damaged;
    }

    // ノックバック処理
    public void KnockBack(KnockbackRequest request) {
        // タイマーリセット
        DamagedTimer = 0;
        // 被弾ステートの時間をセット
        DamagedStateDuration = request.DownDuration;
        // 被弾ステートに遷移
        ChangeState(DamagedState);
        // 攻撃相手の方を向く
        Vector3 lookPos = request.Source.position;
        lookPos.y = 0f;
        transform.LookAt(lookPos);
        // アニメーション速度の設定
        SetKnockBackSpeed(request.Type);
        // ノックバックベクトルの設定
        Vector3 direction = transform.position - request.Source.position;
        direction.y = 0f;
        direction.Normalize();
        // ノックバック
        Rb.linearVelocity = direction * request.Power;
    }

    // 攻撃Hit時処理
    public override void OnAttackHit(Vector3 hitPos,AttackHitType type) {
        // エネミーの向いている方向向いてエフェクト生成
        Quaternion spawnRot = transform.rotation * hitEffect.transform.rotation;
        ParticleSystem effect = Instantiate(hitEffect, hitPos, spawnRot);
        Destroy(effect.gameObject, hitEffect.main.duration);

        // 攻撃ヒットSE再生
        switch (type) {
            case AttackHitType.Attack:
                AudioManager.Instance.PlaySE(Player.DAMAGED_SE_NAME);
                break;
            case AttackHitType.BallHit:
                AudioManager.Instance.PlaySE(HIT_SE_NAME);
                break;
        }
        
    }

    /// <summary>
    /// Smashで吹き飛ばされる際の状態変更
    /// </summary>
    /// <param name="velocity"> 吹き飛ばされる速度ベクトル </param>
    public void OnSmashed(Vector3 velocity) {
        // 状態の変更
        IsRolled = true;
        OnToggleHitBoxCollisionEvent(1); // ボール攻撃判定をON

        // 物理挙動の設定
        Rb.isKinematic = false;
        Rb.constraints = RigidbodyConstraints.None; // 回転を許可

        // 吹き飛ばし
        Rb.linearVelocity = Vector3.zero; // 慣性をリセット
        Rb.linearVelocity = velocity;

        // レイヤー変更
        gameObject.layer = LayerMask.NameToLayer(BALL_LAYER_NAME);
    }

    /// <summary>
    /// ダウン状態の処理
    /// </summary>
    private void EnemyDown() {
        ChangeState(DownState);
    }

    // 死亡時の処理
    public override void CharacterDie() {
        // 爆発Effectを出して消滅
        Instantiate(dieEffectPrefab, transform.position, dieEffectPrefab.transform.rotation);
        Destroy(gameObject);
    }

    // バグフィールド衝突時の処理
    private void OnCollisionEnter(Collision collision) {
        // ダウン時のみ処理
        if (EnemyStateMachine.CurrentState != DownState) return;

        // バグフィールド接触時の反射
        if ((bugLayer.value & (1 << collision.gameObject.layer)) != 0) {
            // 衝突した壁の法線ベクトル
            Vector3 normal = collision.contacts[0].normal;

            // 現在の速度を法線ベクトルで反射
            Vector3 reflectedVelocity = Vector3.Reflect(LastVelocity, normal);

            // 反射後の速度を適用
            Rb.linearVelocity = reflectedVelocity;
        }
    }

    /// <summary>
    /// リブート処理呼び出し
    /// </summary>
    public void OnReboot() {
        Status.Reboot();
    }

    #endregion

    #region ヒットストップ

    // 速度も止めるヒットストップ実行部分
    public void HitStop(float duration) {
        if (EnemyAnimator != null) {
            StartCoroutine(HitStopCoroutine(duration));
        }
    }

    // 見た目だけ止めるヒットストップ実行
    public void HitStopVisualOnly(float duration) {
        if (EnemyAnimator != null) {
            StartCoroutine(HitStopVisualOnlyCoroutine(duration));
        }
    }

    /// <summary>
    /// ヒットストップ処理部分
    /// </summary>
    /// <param name="duration"> 停止時間(秒) </param>
    private IEnumerator HitStopCoroutine(float duration) {
        // アニメーションを止める
        EnemyAnimator.speed = 0f;

        Vector3 storedVelocity;

        if (EnemyStateMachine.CurrentState == DownState) {
            // 物理挙動を停止
            storedVelocity = Rb.linearVelocity;
            // ボール化したタイミングでは元々0なので0にしなくてもいい
            if (!Rb.isKinematic) {
                Rb.linearVelocity = Vector3.zero;
            }
        } else {
            // ナビメッシュを停止
            storedVelocity = Agent.velocity;
            Agent.isStopped = true;
            Agent.velocity = Vector3.zero;
        }

        yield return new WaitForSeconds(duration);

        // 動きの復帰処理
        if (EnemyStateMachine.CurrentState == DownState) {
            if (!Rb.isKinematic) {
                Rb.linearVelocity = storedVelocity;
            }
        } else {
            Agent.isStopped = false;
            Agent.velocity = storedVelocity;
        }

        // アニメーションを再開
        EnemyAnimator.speed = 1f;
    }

    /// <summary>
    /// 見た目だけ止めるヒットストップ処理部分
    /// </summary>
    /// <param name="duration"> 停止時間(秒) </param>
    private IEnumerator HitStopVisualOnlyCoroutine(float duration) {
        EnemyAnimator.speed = 0f;
        yield return new WaitForSeconds(duration);
        EnemyAnimator.speed = 1f;
    }

    #endregion

    #region エフェクト処理
    /// <summary>
    /// プレイヤーの攻撃などで割り込みが入った場合に全エフェクトを停止させる
    /// </summary>
    public void AttackEffectOff() {
        foreach (ParticleSystem effect in AttackEffects) {
            if (effect.isPlaying) {
                effect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }
    }

    /// <summary>
    /// 軌跡エフェクトの切り替え
    /// </summary>
    public void SwitchTrail(bool enabled) => ballModeTrail.enabled = enabled;

    #endregion

    #region アニメーション設定

    /// <summary>
    /// 攻撃に応じたアニメーション速度を適用する
    /// </summary>
    /// <param name="type"> 攻撃ヒットタイプ </param>
    private void SetKnockBackSpeed(AttackHitType type) {
        if (EnemyAnimator == null) return;

        float animSpeed = 0f;

        switch (type) {
            case AttackHitType.Attack:
                // プレイヤーからの攻撃
                animSpeed = kbAnimSpeedPlayer;
                break;
            case AttackHitType.BallHit:
                // ボール状態のエネミーからの攻撃
                animSpeed = kbAnimSpeedBall;
                break;
            case AttackHitType.Guarded:
                // 自分自身から呼ばれた（ガードされた）
                animSpeed = kbAnimSpeedGuarded;
                break;
        }

        EnemyAnimator.SetFloat(ANIM_FLOAT_KNOCKBACK, animSpeed);    // 速度設定
        EnemyAnimator.SetTrigger(ANIM_TRIGGER_KNOCKBACK);           // アニメーション再生
    }

    #endregion

    #region アニメーションイベント

    /// <summary>
    /// 攻撃の準備を行う
    /// </summary>
    void OnSetupAttack() {
        // ナビメッシュで追跡可能にして動きは一時的に移動は停止
        Agent.enabled = true;
        Agent.isStopped = true;
        CanMove = false;
    }

    /// <summary>
    /// 突進攻撃のチャージ演出表示
    /// </summary>
    void OnChargeEvent() {
        // エフェクトを再生してチャージSE再生
        AttackEffectOff();
        AttackEffects[0].Play();
        AudioManager.Instance.PlaySE(CHARGE_SE_NAME);
    }

    /// <summary>
    /// 移動可能
    /// </summary>
    void OnCanMoveEvent() {
        // 移動と振り向きを可能にする
        CanMove = true;
        CanAttackingLook = true;
        if (Agent.enabled) {
            Agent.isStopped = true;
        }
    }

    /// <summary>
    /// 向きを固定
    /// </summary>
    void OnNotLookEvent() {
        CanAttackingLook = false;
    }

    /// <summary>
    /// 当たり判定を持ちながら前方に突進攻撃
    /// </summary>
    void OnAttackMoveEvent() {
        Attacking = true;
        Agent.speed = attackSpeed;
        // 目標距離は前方 * attackMoveDistanceの距離
        Vector3 movePos = transform.forward * attackMoveDistance;
        // 移動はエージェントで処理
        if (Agent.enabled == true) {
            Agent.isStopped = false;
            // 突進中の到達目標地点をdestinationに指定
            Agent.destination = transform.position + movePos;
        }
    }

    /// <summary>
    /// 突進攻撃イベント
    /// </summary>
    void OnRushEvent() {
        // エフェクトを再生して攻撃SE再生
        AttackEffectOff();
        AttackEffects[1].Play();
        AudioManager.Instance.PlaySE(ATTACK_SE_NAME);
    }

    /// <summary>
    /// 攻撃終了
    /// </summary>
    void OnAttackEndEvent() {
        Attacking = false;
    }

    /// <summary>
    /// 被ダメージイベント
    /// </summary>
    void OnTakeDamageEvent() {
        Attacking = false;
        AttackEffectOff();
    }

    /// <summary>
    /// 攻撃判定切り替え 0:無効化 1:有効化
    /// </summary>
    public void OnToggleHitBoxCollisionEvent(int enable) {
        if (HitBox != null) {
            if (enable == 0) {
                HitBox.DisableCollider();
            } else {
                HitBox.EnableCollider();
            }
        }
    }

    #endregion
}
