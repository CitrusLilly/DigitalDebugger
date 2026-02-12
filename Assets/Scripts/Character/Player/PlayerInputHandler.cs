using System;
using UnityEngine;
using UnityEngine.InputSystem;
/// <summary>
/// プレイヤーの入力管理クラス
/// </summary>
public class PlayerInputHandler : MonoBehaviour
{
    private Player player;

    // 移動方向入力値
    public Vector2 MoveInput { get; private set; }

    // 各アクションボタンの押下状態
    public bool AttackPressed { get; private set; }
    public bool GuardPressed { get; private set; }
    public bool DodgePressed { get; private set; }
    public bool SkillPressed { get; private set; }

    /// <summary>
    /// 各アクションボタンの押下イベント
    /// </summary>
    // PlayerUI.UpdateActionFrameで購読
    public event Action<Player.ActionButton, bool> OnActionButton;

    private void Start() {
        player = GetComponent<Player>();
    }

    /// <summary>
    /// 移動入力処理
    /// </summary>
    public void OnMove(InputAction.CallbackContext context) {
        MoveInput = context.ReadValue<Vector2>();
    }

    /// <summary>
    /// 攻撃入力処理(CanAttackがtrueの時のみ有効)
    /// </summary>
    public void OnAttack(InputAction.CallbackContext context) {
        if (context.performed && player.CanAttack) {
            AttackPressed = true;
            OnActionButton?.Invoke(Player.ActionButton.Attack, true);
        } else if (context.canceled) {
            AttackPressed = false;
            OnActionButton?.Invoke(Player.ActionButton.Attack, false);
        }
    }

    /// <summary>
    /// ガード入力処理(押している間ガードをし続ける)
    /// </summary>
    public void OnGuard(InputAction.CallbackContext context) {
        if (context.performed) {
            GuardPressed = true;
            OnActionButton?.Invoke(Player.ActionButton.Guard, true);
        } else if (context.canceled) {
            GuardPressed = false;
            OnActionButton?.Invoke(Player.ActionButton.Guard, false);
        }
    }

    /// <summary>
    /// 回避入力処理(CanDodgeがtrueの時のみ有効)
    /// </summary>
    public void OnDodge(InputAction.CallbackContext context) {
        if (context.performed && player.CanDodge) {
            DodgePressed = true;
            OnActionButton?.Invoke(Player.ActionButton.Dodge, true);
        } else if (context.canceled) {
            DodgePressed = false;
            OnActionButton?.Invoke(Player.ActionButton.Dodge, false);
        }
    }

    /// <summary>
    /// スキル入力処理
    /// </summary>
    public void OnSkill(InputAction.CallbackContext context) {
        if (context.performed) {
            SkillPressed = true;
            OnActionButton?.Invoke(Player.ActionButton.Skill, true);
        } else if (context.canceled) {
            SkillPressed = false;
            OnActionButton?.Invoke(Player.ActionButton.Skill, false);
        }
    }

    /// <summary>
    /// 攻撃、回避、スキルの入力状態をリセットする
    /// <para>※ガードは長押しで継続するため、意図的に除外</para>
    /// </summary>
    public void ResetAllPressed() {
        AttackPressed = false;
        DodgePressed = false;
        SkillPressed = false;
        //GuardPressedは手動、もしくはガード継続時間終了で解除
    }

}
