using System;
using UnityEngine;
/// <summary>
/// プレイヤーの再使用時間のある行動の管理クラス
/// </summary>
public class RecastManager : MonoBehaviour
{
    // 各リキャスト時間初期値
    [SerializeField] private float skillRecastTime = 5f;
    [SerializeField] private float guardRecastTime = 5f;

    // 外部公開のリキャスト時間
    public float SkillRecast => skillRecastTime;
    public float GuardRecast => guardRecastTime;

    // 内部リキャストタイマー
    private float skillTimer;
    private float guardTimer;

    // 外部公開のリキャストタイマー
    public float SkillTimer => skillTimer;
    public float GuardTimer => guardTimer;

    /// <summary>
    /// スキルリキャスト時イベント
    /// </summary>
    // PlayerUI.UpdateSkillCooldown を購読
    public event Action<float,float> OnSkillRecast;
    /// <summary>
    /// ガードリキャスト時イベント
    /// </summary>
    // PlayerUI.UpdateGuardCooldown を購読
    public event Action<float,float> OnGuardRecast;

    private void Awake() {
        // 初期状態でガードとスキルを使えるようにする
        skillTimer = SkillRecast;
        guardTimer = GuardRecast;
    }

    private void Update() {
        // スキルのリキャスト
        if(skillTimer <= SkillRecast) {
            skillTimer = Mathf.Clamp(skillTimer + Time.deltaTime, 0, SkillRecast);
            OnSkillRecast?.Invoke(skillTimer,SkillRecast);
        }
        // ガードのリキャスト
        if (guardTimer <= GuardRecast) {
            guardTimer = Mathf.Clamp(guardTimer + Time.deltaTime, 0, GuardRecast);
            OnGuardRecast?.Invoke(guardTimer,GuardRecast);
        }
    }

    /// <summary> スキルが使えるかどうかを返す </summary>
    public bool IsSkillReady() => skillTimer >= SkillRecast;

    /// <summary> ガードが使えるかどうかを返す </summary>
    public bool IsGuardReady() => guardTimer >= GuardRecast;

    /// <summary>
    /// スキルのリキャストに入る
    /// </summary>
    public void ResetSkillRecast() {
        skillTimer = 0f;
        OnSkillRecast?.Invoke(skillTimer, SkillRecast);
    }

    /// <summary>
    /// ガードのリキャストに入る
    /// </summary>
    public void ResetGuardRecast() {
        guardTimer = 0f;
        OnGuardRecast?.Invoke(guardTimer, GuardRecast);
    }
}
