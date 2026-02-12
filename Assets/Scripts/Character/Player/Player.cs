using System.Collections;
using UnityEngine;
/// <summary>
/// プレイヤーの管理クラス
/// </summary>
public class Player : Character, IDamageable,IHitStop
{
    // ========================定数==========================
    // ===タグ名===
    public const string TAG_NAME    = "Player";
    // ===レイヤー名===
    public const string LAYER_NAME  = "Player";
    // ===SE名===
    public const string HIT_SE_NAME         = "hit_player";
    public const string ATTACK_SE_NAME      = "attack_player";
    public const string SKILL_SE_NAME       = "skill_player";
    public const string GUARD_SE_NAME       = "guard_player";
    public const string GUARD_ON_SE_NAME    = "guard_on_player";
    public const string DODGE_SE_NAME       = "dodge_player";
    public const string DAMAGED_SE_NAME     = "damaged_player";
    public const string SMASH_SE_NAME       = "smash_player";
    // ===アニメーション===
    public const string ANIM_TRIGGER_ATTACK     = "Attack";
    public const string ANIM_TRIGGER_DODGE      = "Dodge";
    public const string ANIM_TRIGGER_SKILL      = "Skill";
    public const string ANIM_TRIGGER_KNOCKBACK  = "KnockBack";
    public const string ANIM_TRIGGER_DIE        = "Die";
    public const string ANIM_BOOL_FIRSTATTACK   = "FirstAttack";
    public const string ANIM_BOOL_GUARD         = "Guard";
    public const string ANIM_BOOL_MOVE          = "isMove";
    // ======================================================

    // ==================依存コンポーネント====================
    public StateMachine PlayerStateMachine { get; private set; }    // Stateパターンで管理
    public PlayerStatusManager Status { get; private set; }         // ステータス
    public RecastManager Recast { get; private set; }               // スキルなどのリキャスト
    public PlayerInputHandler InputHandler { get; private set; }    // 入力管理
    public Animator PlayerAnimator { get; private set; }
    public Rigidbody Rb { get; private set; }   // 移動はRigidbodyを使用
    public PlayerAttackHitBox Weapon;           // 武器
    // ======================================================

    // ==================各ステート===========================
    public IState IdleState { get; private set; }
    public IState MoveState { get; private set; }
    public IState AttackState { get; private set; }
    public IState SkillState { get; private set; }
    public IState DamagedState { get; private set; }
    public IState GuardState { get; private set; }
    public IState DieState { get; private set; }
    public IState DodgeState { get; private set; }
    // ======================================================

    // ==================移動関連=============================
    [Header("Move")]
    public float MoveSpeed = 9f; // 移動速度
    [SerializeField] private float RotationSpeed = 1200f; // プレイヤー回転角速度
    // ======================================================

    // ==================攻撃関連=============================
    [Header("Attack")]
    public int CurrentCombo = 0;                                // 現在のコンボ段階
    public int MaxCombo = 2;                                    // 最大コンボ
    public bool CanAttack;                                      // 攻撃可能かどうか
    public bool AttackStateFinish;                              // 攻撃アニメーションが終了したかどうか
    public bool IsAttackMove;                                   // 攻撃時の前進が可能かどうか
    public float AttackMoveSpeed = 8f;                          // 攻撃時の前進速度
    public float AttackMoveTime = 0.08f;                        // 攻撃時の前進時間(秒)
    public int AutoTurnMaxCnt = 5;                              // オートターンの持続フレーム数
    [SerializeField] private float followMaxDistance = 4f;      // オートターンの振り向く最長距離
    // ======================================================

    // ==================スキル関連===========================
    [Header("Skill")]
    public bool SkillStateFinish;   // スキルアニメーションが終了したかどうか
    public bool CanSkill;           // スキルが使えるかどうか
    // ======================================================

    // ==================ガード関連===========================
    [Header("Guard")]
    public bool IsGuard;                                                // ガード中フラグ
    // ---ガード継続
    public float GuardTime = 3f;                                        // ガード継続時間
    public float GuardWarningTime = 2f;                                 // ガードが切れる警告表示を出す時間
    private Coroutine GuardWarningCoroutine;                            // ガード時の連続点滅を避けるために現在のコルーチンを保持
    [SerializeField] private Renderer GuardRenderer;                    // キャラクター点滅用のレンダラー(インスペクターからセット)
    // ---ガード拡大縮小
    [SerializeField] private float guardEffectInitialScale = 0.5f;      // ガードエフェクトの初期サイズ
    [SerializeField] private float guardEffectActiveScale = 1.8f;       // ガード使用中のエフェクトサイズ
    [SerializeField] private float guardEffectScaleDuration = 0.1f;     // ガードエフェクトのサイズ変更時間(秒)
    private Coroutine guardEffectScaleCoroutine;                        // 拡大と縮小を同時に起こさない為にサイズ変更コルーチンを保持
    // ======================================================

    // ==================被ダメージ関連=======================
    [Header("Invincible")]
    public bool Invincible;                                     // 無敵状態フラグ
    public float DamagedStateDuration { get; private set; }     // ダメージステートの継続時間(秒) 相手から値を受け取る
    private Renderer[] characterRenderer;                       // ダメージ時の点滅処理に使用するキャラクターのレンダラー
    [SerializeField] private float blinkDuration = 0.05f;       // 点滅切替え間隔
    [SerializeField] private float damagedInvincibleTime = 2f;  // 被弾時の無敵時間
    // ======================================================

    // ==================回避関連=============================
    [Header("Dodge")]
    public bool CanDodge;                       // 回避状態フラグ
    public float DodgeTime = 1f;                // 回避ステートの継続時間(秒)
    public float DodgeInvincibleTime = 0.4f;    // 回避時の無敵時間
    public float DodgeMoveSpeed = 8f;           // 回避時の前転移動速度
    public float DodgeMoveDuration = 0.7f;      // 回避が始まってから移動が終了するタイミング(秒)
    public bool DodgeStateFinish;               // 回避ステート終了フラグ
    // ======================================================

    // ==================エフェクト関連=======================
    [Header("Effect")]
    [SerializeField] private ParticleSystem[] slashEffects;     // 攻撃やスキルのスラッシュエフェクト(再生)
    [SerializeField] private ParticleSystem attackHitEffect;    // 攻撃ヒット時のエフェクト(生成)
    [SerializeField] private ParticleSystem guardSuccessEffect; // ガード成功時エフェクト(再生)
    [SerializeField] private GameObject guardEffect;            // ガード時のエフェクト(有効化)
    [SerializeField] private GameObject downEffect;             // 死亡時のエフェクト(生成)
    private ParticleSystem playingAttackEffect;                 // 現在再生中の攻撃エフェクト
    // ======================================================

    /// <summary>
    /// アクションボタン種別
    /// </summary>
    public enum ActionButton
    {
        Attack, // 攻撃
        Guard,  // ガード
        Dodge,  // 回避
        Skill   // スキル
    }

    private void Awake() {
        InitComponents();
        InitState();
    }

    private void Start() {
        // 初期状態を設定
        ChangeState(IdleState);

        // 武器に攻撃力を伝える
        Weapon.BaseAttack = Status.Attack;
        // 被ダメージの点滅用にキャラクターのレンダラーを保持
        characterRenderer = GetComponentsInChildren<Renderer>();
        // オブジェクトを初期状態にセット
        guardEffect.SetActive(false);
    }

    private void Update() => PlayerStateMachine.Update();

    private void FixedUpdate() => PlayerStateMachine.FixedUpdate();

    /// <summary>
    /// 各コンポーネント初期化
    /// </summary>
    private void InitComponents() {
        if (PlayerAnimator == null) {
            PlayerAnimator = GetComponent<Animator>();
        }
        if (Rb == null) {
            Rb = GetComponent<Rigidbody>();
        }
        if (Status == null) {
            Status = GetComponent<PlayerStatusManager>();
        }
        if (Recast == null) {
            Recast = GetComponent<RecastManager>();
        }
        if (InputHandler == null) {
            InputHandler = GetComponent<PlayerInputHandler>();
        }

        PlayerStateMachine = new StateMachine();
    }

    /// <summary>
    /// 各ステートの初期化
    /// </summary>
    private void InitState() {
        // 自分自身を渡してアクセス可能にする
        IdleState = new PlayerIdleState(this);
        MoveState = new PlayerMoveState(this);
        AttackState = new PlayerAttackState(this);
        SkillState = new PlayerSkillState(this);
        DamagedState = new PlayerDamagedState(this);
        GuardState = new PlayerGuardState(this);
        DieState = new PlayerDieState(this);
        DodgeState = new PlayerDodgeState(this);
    }

    /// <summary>
    /// プレイヤーの状態を変更
    /// </summary>
    /// <param name="newState"> 遷移先ステート </param>
    public void ChangeState(IState newState) => PlayerStateMachine.ChangeState(newState);

    /// <summary>
    /// 攻撃、回避、スキルの入力状態をリセットする
    /// </summary>
    public void ResetAllPressed() => InputHandler.ResetAllPressed();

    #region プレイヤーの移動・回転

    /// <summary>
    /// 移動処理。
    /// </summary>
    /// <param name="speed"> 動く速度 </param>
    public void Move(float speed) {
        // カメラ基準の移動方向を取得
        Vector3 moveDir = GetCameraMoveDirection();

        // MoveStateで呼び出した場合はカメラから見た方向に進む
        if (PlayerStateMachine.CurrentState == MoveState) {
            RotateTowards(moveDir);         // 回転処理
            MoveDirection(moveDir, speed);  // 移動処理
        } else {
            // それ以外(回避・攻撃)で呼び出した場合はキャラクターの前方へ進む
            MoveForward(speed);             // 移動処理
        }
    }

    /// <summary>
    /// 入力方向へ回転させる。
    /// </summary>
    private void RotateTowards(Vector3 moveDir) {
        if (moveDir.sqrMagnitude < 0.001f) return; // ほぼ無入力なら回転しない

        Quaternion targetRotation = GetCameraTargetRotation(moveDir);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, RotationSpeed * Time.fixedDeltaTime);
    }

    /// <summary>
    /// カメラ基準の移動方向を返す
    /// </summary>
    private Vector3 GetCameraMoveDirection() {
        // カメラの前方向と右方向を取得
        Vector3 forward = Camera.main.transform.forward;
        Vector3 right = Camera.main.transform.right;
        // 上方向（Y軸）を無視
        forward.y = 0;
        right.y = 0;
        // 正規化して方向ベクトルを計算
        forward.Normalize();
        right.Normalize();
        // 移動入力値をVector3に入れて正規化
        Vector3 moveDir = (forward * InputHandler.MoveInput.y + right * InputHandler.MoveInput.x).normalized;
        return moveDir;
    }

    /// <summary>
    /// カメラ基準の回転方向を返す
    /// </summary>
    private Quaternion GetCameraTargetRotation(Vector3 moveDir) {
        return Quaternion.LookRotation(moveDir, Vector3.up);
    }

    /// <summary>
    /// 指定方向へ移動する
    /// </summary>
    private void MoveDirection(Vector3 direction, float speed) {
        Vector3 movement = direction * (speed * Time.fixedDeltaTime);
        Rb.MovePosition(Rb.position + movement);
    }

    /// <summary>
    /// 現在の前方方向へ移動する(攻撃・回避用)
    /// </summary>
    private void MoveForward(float speed) {
        Vector3 movement = transform.forward * (speed * Time.fixedDeltaTime);
        Rb.MovePosition(Rb.position + movement);
    }

    /// <summary>
    /// 即時振り向き処理(指定方向への回避に使用)
    /// </summary>
    public void Turn() => transform.rotation = Quaternion.LookRotation(GetCameraMoveDirection());

    /// <summary>
    /// 最長範囲内までの一番近い敵にオートターン
    /// </summary>
    public void AutoTurn() {
        Transform nearest = EnemyManager.Instance.GetNearestEnemy(transform.position, followMaxDistance);

        if (nearest != null) {      // エネミーが近くにいる場合の処理
            // 上下を見ないように制御
            Vector3 lookVector = new Vector3(nearest.position.x, transform.position.y, nearest.position.z);
            transform.LookAt(lookVector);

        } else if (MoveCheck()) {   // エネミーが近くにいなくて入力があった場合の処理
            // カメラ基準で入力方向への回転
            RotateTowards(GetCameraMoveDirection());
        }
    }

    /// <summary>
    /// 移動入力が入っているかチェック
    /// </summary>
    public bool MoveCheck() => InputHandler.MoveInput.magnitude > 0.01f;

    #endregion

    #region ダメージ処理とノックバック

    // ダメージ処理
    public DamageReaction TakeDamage(int damage) {
        // 既に死んでいる場合はDeadで返す
        if (PlayerStateMachine.CurrentState == DieState) {
            return DamageReaction.Down;
        }

        // ガード中ならGuardedで返す
        if (IsGuard) {
            GuardSuccess();
            return DamageReaction.Guarded;
        }

        // 無敵中ならInvincibleで返す
        if (Invincible) {
            return DamageReaction.Invincible;
        }

        // それ以外はダメージ処理
        if (Status.HpCalc(damage)) {
            // HP0で死亡処理 Deadで返す
            CharacterDie();
            return DamageReaction.Down;
        }

        // プレイヤー点滅と無敵時間の終了コルーチン
        StartCoroutine(DamagedCoroutine());
        // ダメージが通ったことを返す
        return DamageReaction.Damaged;
    }

    // ノックバック処理
    public void KnockBack(KnockbackRequest request) {
        // 被弾ステートの時間をセット
        DamagedStateDuration = request.DownDuration;
        // 被弾ステートに遷移
        ChangeState(DamagedState);

        switch (request.Type) {
            case AttackHitType.Attack:
                // 攻撃相手の方を向く
                Vector3 lookPos = request.Source.position;
                lookPos.y = 0f;
                transform.LookAt(lookPos);

                // 攻撃された場合は、相手から自身へのベクトルへノックバック
                Vector3 direction = transform.position - request.Source.position;
                direction.y = 0f;
                direction.Normalize();
                Rb.linearVelocity = direction * request.Power;
                break;
            case AttackHitType.WallHit:
                // バグの壁に接触した場合は、指定ベクトルにノックバック
                Rb.linearVelocity = request.Direction * request.Power;
                break;
        }
    }

    // 自身の攻撃がヒット
    public override void OnAttackHit(Vector3 hitPos, AttackHitType type) {
        // プレイヤーの向いている方向向いてエフェクト生成
        Quaternion spawnRot = transform.rotation * attackHitEffect.transform.rotation;
        ParticleSystem effect = Instantiate(attackHitEffect, hitPos, spawnRot);
        Destroy(effect.gameObject, attackHitEffect.main.duration);

        // 攻撃ヒットSE再生
        AudioManager.Instance.PlaySE(HIT_SE_NAME);
    }

    /// <summary>
    /// 被ダメージ処理
    /// </summary>
    private IEnumerator DamagedCoroutine() {
        float invincibleTimer = 0;
        while (invincibleTimer < damagedInvincibleTime) {

            Blink(false);
            yield return new WaitForSeconds(blinkDuration);
            invincibleTimer += blinkDuration;
            Blink(true);
            yield return new WaitForSeconds(blinkDuration);
            invincibleTimer += blinkDuration;
        }

        Invincible = false;       // 無敵解除
    }

    /// <summary>
    /// 被ダメージ時の点滅処理
    /// </summary>
    private void Blink(bool enabled) {
        // レンダラーの表示切り替え
        for (int i = 0; i < characterRenderer.Length; i++) {
            characterRenderer[i].enabled = enabled;
        }
    }

    // 死亡処理
    public override void CharacterDie() {
        // 死亡状態に変更
        ChangeState(DieState);
        // ゲームマネージャーの死亡時処理呼び出し
        GameManager.Instance.OnPlayerDied();
    }

    #endregion

    #region ヒットストップ

    // ヒットストップ実行部分
    public void HitStop(float duration) {
        if (PlayerAnimator != null) {
            StartCoroutine(HitStopCoroutine(duration));
        }
    }

    // 見た目だけ止めるヒットストップ実行
    public void HitStopVisualOnly(float duration) {
        if (PlayerAnimator != null) {
            StartCoroutine(HitStopVisualOnlyCoroutine(duration));
        }
    }

    /// <summary>
    /// ヒットストップ処理部分
    /// </summary>
    /// <param name="duration"> 停止時間(秒) </param>
    private IEnumerator HitStopCoroutine(float duration) {
        // 停止前の状態を保存
        Vector3 storedVelocity = Rb.linearVelocity;

        // 停止処理
        Rb.linearVelocity = Vector3.zero;
        Rb.isKinematic = true;
        PlayerAnimator.speed = 0f;

        SetDamadedEffect(true);     // 被ダメージ表現ON

        yield return new WaitForSeconds(duration);

        // 復帰処理
        Rb.isKinematic = false;
        Rb.linearVelocity = storedVelocity;
        PlayerAnimator.speed = 1f;

        SetDamadedEffect(false);    // 被ダメージ表現OFF
    }

    /// <summary>
    /// 見た目だけ止めるヒットストップ処理部分
    /// </summary>
    /// <param name="duration"> 停止時間(秒) </param>
    private IEnumerator HitStopVisualOnlyCoroutine(float duration) {
        PlayerAnimator.speed = 0f;
        TogglePauseAttackEffect();  // 再生中エフェクトの一時停止
        yield return new WaitForSeconds(duration);
        PlayerAnimator.speed = 1f;
        TogglePauseAttackEffect();  // 一時停止中エフェクトの再開
    }

    #endregion

    #region ガード時処理

    /// <summary>
    /// ガードエフェクトの表示切り替え
    /// </summary>
    /// <param name="isActive"> true:有効化 false:無効化 </param>
    public void ToggleGuardEffect(bool isActive) { 
        // まだ表示処理中なら破棄して新しく処理を開始
        if(guardEffectScaleCoroutine != null) {
            StopCoroutine(guardEffectScaleCoroutine);
        }

       guardEffectScaleCoroutine = StartCoroutine(ToggleGuardEffectCoroutine(isActive));
    }

    /// <summary>
    /// ガードエフェクト表示切り替えコルーチン
    /// </summary>
    private IEnumerator ToggleGuardEffectCoroutine(bool isActive) {
        // trueなら有効化
        if (isActive) {
            guardEffect.SetActive(true);
        }

        float timer = 0f;
        // 開始サイズと終了サイズを定義
        Vector3 startScale, endScale;
        if (isActive) {
            startScale = Vector3.one * guardEffectInitialScale;
            endScale = Vector3.one * guardEffectActiveScale;
        } else {
            startScale = guardEffect.transform.localScale;
            endScale = Vector3.one * guardEffectInitialScale;
        }

        // 徐々にサイズ変更
        while (timer < guardEffectScaleDuration) {
            guardEffect.transform.localScale = Vector3.Lerp(startScale, endScale, timer / guardEffectScaleDuration);
            timer += Time.deltaTime;
            yield return null;
        }

        // 最終サイズを確定
        guardEffect.transform.localScale = endScale;

        // falseなら無効化
        if (!isActive) {
            guardEffect.SetActive(false);
        }
        // コルーチンを破棄
        guardEffectScaleCoroutine = null;
    }


    /// <summary>
    /// ガードが切れそうになった時に点滅してもうすぐガードが切れることを警告する
    /// </summary>
    public void FlashGuardWarning() {
        if (GuardWarningCoroutine == null) {
            GuardWarningCoroutine = StartCoroutine(GuardBlink());
        }
    }

    /// <summary>
    /// ガードエフェクトのレンダラー切り替え
    /// </summary>
    private IEnumerator GuardBlink() {
        float blinkTimer = 0;
        while (blinkTimer < GuardWarningTime) {
            GuardRenderer.enabled = false;
            yield return new WaitForSeconds(blinkDuration);
            blinkTimer += blinkDuration;

            GuardRenderer.enabled = true;
            yield return new WaitForSeconds(blinkDuration);
            blinkTimer += blinkDuration;
        }
    }

    /// <summary>
    /// ガード警告の点滅を停止し、レンダラーを有効化
    /// </summary>
    public void GuardCoroutineFinish() {
        if (GuardWarningCoroutine != null) {
            StopCoroutine(GuardWarningCoroutine);
        }
        GuardWarningCoroutine = null;
        GuardRenderer.enabled = true;
    }

    /// <summary>
    /// ガード成功時処理
    /// </summary>
    private void GuardSuccess() {
        // ガード成功時エフェクト再生
        guardSuccessEffect.Play();
        // ガード成功時SE再生
        AudioManager.Instance.PlaySE(GUARD_SE_NAME);
    }

    #endregion

    #region エフェクト処理

    /// <summary>
    /// 再生中の攻撃スラッシュエフェクトを全て停止
    /// </summary>
    public void SlashEffectStop() {
        foreach (ParticleSystem effect in slashEffects) {
            if (effect.isPlaying) {
                effect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }
    }

    /// <summary>
    /// 被ダメージポストエフェクトを呼び出す
    /// </summary>
    public void SetDamadedEffect(bool enabled) {
        if (ShaderManager.Instance != null) {
            ShaderManager.Instance.SetDamagedPostEffect(enabled);
        }
    }

    /// <summary>
    /// 再生中の攻撃エフェクトのポーズ切り替え
    /// </summary>
    private void TogglePauseAttackEffect() {
        if (playingAttackEffect == null) return;
        
        if (playingAttackEffect.isPaused) {
            playingAttackEffect.Play();     // ポーズ中なら再生
        } else if(playingAttackEffect.isPlaying) {
            playingAttackEffect.Pause();    // 再生中ならポーズ
        }
    }

    #endregion

    #region アニメーションイベント

    /// <summary>
    /// 攻撃入力切替え 0:無効化 1:有効化
    /// </summary>
    void OnAttackInputToggleEvent(int enable) {
        if(enable == 0) {
            CanAttack = false;
        } else {
            CanAttack = true;
        }
    }

    /// <summary>
    /// 攻撃エフェクト表示
    /// 通常攻撃は0~2のコンボ数、スキルは最大コンボ+1を使用する
    /// </summary>
    void OnSlashEffectShowEvent(int attackPhase) {
        if (PlayerStateMachine.CurrentState == AttackState) {
            // 通常攻撃
            slashEffects[attackPhase].Play();
            playingAttackEffect = slashEffects[attackPhase]; // 再生エフェクトをキャッシュ
        } else if (PlayerStateMachine.CurrentState == SkillState) {
            // スキルエフェクトは最大コンボ＋１の枠に収納
            slashEffects[MaxCombo + 1].Play();
            playingAttackEffect = slashEffects[MaxCombo + 1];
        }
    }

    /// <summary> 攻撃当たり判定有効化 </summary>
    void OnEnableAttackCollisionEvent() => Weapon.EnableCollider();

    /// <summary> スキル当たり判定有効化 </summary>
    void OnEnableSkillCollisionEvent() => Weapon.SkillColliderEnabled();

    /// <summary> 攻撃判定無効化 </summary>
    public void OnDisableCollisionEvent() => Weapon.DisableCollider();

    /// <summary> 攻撃時の前進フラグON </summary>
    void OnAttackMoveEvent() => IsAttackMove = true;

    /// <summary> 攻撃ステート終了フラグON </summary>
    void OnAttackEndEvent() => AttackStateFinish = true;

    /// <summary> スキルステート終了フラグON </summary>
    void OnSkillEndEvent() => SkillStateFinish = true;

    /// <summary> 回避ステート終了フラグON </summary>
    void OnDodgeFinishEvent() => DodgeStateFinish = true;

    #endregion
}
