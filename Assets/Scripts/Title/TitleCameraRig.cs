using UnityEngine;
/// <summary>
/// タイトル画面でのTitleManagerの仲介クラス
/// </summary>
public class TitleCameraRig : MonoBehaviour
{
    [SerializeField] private TitleManager titleManager;

    // ゲームシーンをロード
    public void OnLoadGameScene() {
        titleManager.LoadGameScene();
    }
}
