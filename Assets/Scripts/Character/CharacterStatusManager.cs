using System;
using UnityEngine;
/// <summary>
/// プレイヤーとエネミーの共通ステータスクラス
/// </summary>
public abstract class CharacterStatusManager : MonoBehaviour 
{
    // 定数 HP下限値
    protected const int MIN_HP = 0;

    public int MaxHp { get; protected set; }// HP上限値
    protected int _currentHp;               // HP現在値

    public int Attack { get; protected set; } // 攻撃力

    /// <summary>
    /// HP変更時イベント　主にUIの更新用
    /// </summary>
    // Player: PlayerUI.UpdateHpIconで購読
    // Enemy : EnemyUI.UpdateHpSliderで購読
    public event Action<int, int> OnHpChanged;

    public int CurrentHp {
        get => _currentHp;
        protected set {
            // 値変化時のみイベントを発火
            if (_currentHp != value) {
                _currentHp = value;
                OnHpChanged?.Invoke(_currentHp, MaxHp);
            }
        }
    }

    /// <summary>
    /// HPからダメージを引く減算処理
    /// </summary>
    /// <param name="damage"> 与えられたダメージ量 </param>
    /// <returns> HP0以下になり倒された場合trueを返す </returns>
    public virtual bool HpCalc(int damage) {
        // マイナスダメージは計算しない
        if (damage < MIN_HP) return false;
        CurrentHp = Mathf.Clamp(CurrentHp - damage, MIN_HP, MaxHp);
        return CurrentHp <= MIN_HP;
    }
}