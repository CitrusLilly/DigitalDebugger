using System.Collections;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// ゲームオーバー演出処理クラス
/// </summary>
public class GameOverController : MonoBehaviour 
{
    // 定数 BGM
    private const string GAMEOVER_BGM_NAME = "gameover_electric_river";

    [Header("slow")]
    [SerializeField] private float gameOverSlowTime = 3f;   // ゲームオーバー時のスロー演出時間
    
    [Header("select")]
    [SerializeField] private Button firstSelectButton;      // ゲームオーバー時に最初に選択状態になるボタン

    /// <summary>
    /// ゲームオーバー処理
    /// </summary>
    public void PlayGameOver() {
        StartCoroutine(GameOver());
    }

    /// <summary>
    /// ゲームオーバー演出
    /// </summary>
    private IEnumerator GameOver() {
        GameManager.Instance.EnterUIMode();

        // スロー演出
        Time.timeScale = 0.2f;
        yield return new WaitForSecondsRealtime(gameOverSlowTime);
        Time.timeScale = 1f;

        ShaderManager.Instance.SetGrayscalePostEffect(true);  // 画面をグレースケール化
        EnemyManager.Instance.TargetDie();                  // エネミーの戦闘状態解除
        AudioManager.Instance.PlayBGM(GAMEOVER_BGM_NAME);   // ゲームオーバーBGMの再生
        // ゲームオーバーUI及び、カーソルの表示
        ShowGameOverUI();
        firstSelectButton.Select();
        GameManager.Instance.SetCursorVisible(true);
    }

    /// <summary>
    /// プレイヤーUIからゲームオーバーUIへの切替え
    /// </summary>
    private void ShowGameOverUI() {
        UIManager.Instance.HideUI(UIType.PlayerUI);
        UIManager.Instance.ShowUI(UIType.GameOverUI);
    }
}
