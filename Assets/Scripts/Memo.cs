using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 開発中に使うメモ
/// </summary>
public class Memo : MonoBehaviour
{
    [SerializeField] // 使用中アセット
    private List<Page> usedAssetsList = new List<Page>();
    [SerializeField] // メモ帳
    private List<Page> todoList = new List<Page>();

    [System.Serializable]
    public class Page
    {
        public string title;
        [TextArea(3, 10)]
        public string comment;
    }
}
