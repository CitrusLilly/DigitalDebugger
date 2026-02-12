using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// プレイヤーの手前の特定レイヤーを透過させるクラス
/// </summary>
public class CameraObstructionHandler : MonoBehaviour
{
    public Transform Target; // インスペクターから指定

    [Header("TransparentSetting")]
    [SerializeField] private LayerMask obstructionLayer;    // 透明化対象のレイヤー
    [Range(0f, 1f)]  
    [SerializeField] private float fadeAlpha = 0.25f;       // 透過度

    // 透過中オブジェクトのリスト
    private readonly List<Renderer> hiddenObjects = new List<Renderer>();

    void Update() {
        if (Target == null) return;

        // 前フレームで透明にしたオブジェクトを元に戻す
        foreach (Renderer r in hiddenObjects) {
            if (r != null) {
                Color c = r.material.color;
                c.a = 1f; // 元の不透明に戻す
                r.material.color = c;
            }
        }
        hiddenObjects.Clear();

        // カメラからプレイヤーへの方向
        Vector3 dir = Target.position - transform.position;
        Ray ray = new Ray(transform.position, dir);

        // 指定レイヤーだけを対象にRaycast
        RaycastHit[] hits = Physics.RaycastAll(ray, dir.magnitude, obstructionLayer);

        foreach (RaycastHit hit in hits) {
            Renderer r = hit.collider.GetComponent<Renderer>();
            if (r != null) {
                Color c = r.material.color;
                c.a = fadeAlpha; // 半透明にする
                r.material.color = c;
                hiddenObjects.Add(r);
            }
        }
    }
}
