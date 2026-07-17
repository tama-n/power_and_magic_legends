using UnityEngine;
using UnityEngine.EventSystems;

public class TitleButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Vector3 defaultScale;
    [SerializeField] private float hoverScaleMultiplier = 1.05f; // ホバー時に5%大きくする
    [SerializeField] private float scaleSpeed = 10f;

    private Vector3 targetScale;

    void Start()
    {
        defaultScale = transform.localScale;
        targetScale = defaultScale;
    }

    void Update()
    {
        // 毎フレーム、ターゲットの大きさに滑らかに変化させる
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * scaleSpeed);
    }

    // マウスがボタンに乗ったとき
    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = defaultScale * hoverScaleMultiplier;
    }

    // マウスがボタンから離れたとき
    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = defaultScale;
    }

    // 有効化/無効化時のリセット処理
    void OnDisable()
    {
        transform.localScale = defaultScale;
    }
}