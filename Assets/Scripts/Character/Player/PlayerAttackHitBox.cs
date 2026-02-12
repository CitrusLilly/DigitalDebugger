using UnityEngine;
using UnityEngine.AI;
/// <summary>
/// プレイヤーの攻撃判定処理クラス(武器)
/// AttackHitBoxを継承
/// </summary>
public class PlayerAttackHitBox : AttackHitBox
{
    [Header("Multiplier")]
    [SerializeField] private float[] comboMultipliers = { 1.0f, 1.3f, 1.7f};    // コンボ倍率
    [SerializeField] private float skillMultiplier = 2.0f;                      // スキル倍率
    [SerializeField] private float minRandomMultiplier = 0.8f;                  // 最小乱数倍率
    [SerializeField] private float maxRandomMultiplier = 1.2f;                  // 最大乱数倍率

    [Header("Collider")]
    [SerializeField] private Collider skillCollider;    // スキル用コライダー

    private bool isSkillUsed; // スキルが使えるかどうか
    private int currentCombo; // 現在のコンボ段階(ダメージ倍率適用のため)

    protected override void Start() {
        base.Start();
        skillCollider.enabled = false; // スキル用コライダーの初期無効化
    }

    protected override void OnTriggerEnter(Collider other) {
        // エネミー以外、または既にヒットした相手は処理しない
        if (!other.CompareTag(Enemy.TAG_NAME) || hitList.Contains(other.gameObject)) return;
        
        // 多重ヒットをなくす
        hitList.Add(other.gameObject);
        
        // ダメージ処理インターフェースでダメージ処理を呼び出す
        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable == null) return;

        int damage = DamageCalculate();
        var result = damageable.TakeDamage(damage);

        Vector3 hitPos = other.ClosestPoint(attackCollider.bounds.center); // 攻撃hit位置
        // 結果
        switch (result) {
            case DamageReaction.Damaged:
                // ヒット通知
                ownerCharacter.OnAttackHit(hitPos, knockbackRequest.Type);

                // 相手をノックバック
                damageable.KnockBack(knockbackRequest);

                // 自身にヒットストップ
                ownerHitStop.HitStopVisualOnly(hitStopDuration);
                // 相手にヒットストップ
                if (other.TryGetComponent(out IHitStop hitstop)) {
                    hitstop.HitStopVisualOnly(hitStopDuration);
                }

                break;
            case DamageReaction.DamagedOnly:
            case DamageReaction.Down:
                // ダメージのみとダウンはヒット通知のみ
                ownerCharacter.OnAttackHit(hitPos, knockbackRequest.Type);

                break;
            case DamageReaction.Smash:
                if(other.TryGetComponent(out Enemy enemy)) {
                    // 吹き飛ばしベクトル
                    Vector3 dir = ownerCharacter.transform.forward;                   
                    // プレイヤー自身にヒットストップ演出
                    ownerHitStop.HitStopVisualOnly(SmashController.Instance.HitStopDuration);
                    // 吹き飛ばし処理呼び出し
                    SmashController.Instance.ApplySmash(enemy, dir);
                }

                break;
        }
    }

    /// <summary>
    /// ダメージ計算
    /// </summary>
    /// <returns> ベース攻撃力 * 乱数 * 倍率 </returns>
    private int DamageCalculate() {
        // 乱数計算
        float randomMultiplier = Random.Range(minRandomMultiplier, maxRandomMultiplier);
        float damage = BaseAttack * randomMultiplier;

        // スキルかコンボの倍率を掛けて最終ダメージ
        if (isSkillUsed) {
            damage *= skillMultiplier;
        } else {
            damage *= comboMultipliers[currentCombo];
        }

        // 四捨五入してダメージを返す
        return Mathf.RoundToInt(damage);
    }

    /// <summary>
    /// スキル用コライダー有効化
    /// </summary>
    public void SkillColliderEnabled() {
        DisableCollider();  // 有効化前にリセット
        hitList.Clear();    // ヒット履歴クリア
        skillCollider.enabled = true;
    }

    /// <summary>
    /// 通常攻撃とスキルを含めたコライダー無効化
    /// </summary>
    public override void DisableCollider() {
        base.DisableCollider();
        skillCollider.enabled = false;
    }

    /// <summary>
    /// コンボ段階更新
    /// </summary>
    /// <param name="combo"> 現在のコンボ段階 </param>
    /// <param name="isSkill"> スキル使用の有無 </param>
    public void SetCurrentCombo(int combo,bool isSkill) {
        currentCombo = combo;
        isSkillUsed = isSkill;
    }
}
