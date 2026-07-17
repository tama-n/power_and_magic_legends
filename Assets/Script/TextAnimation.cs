using System.Collections;
using TMPro;
using UnityEngine;

public class TextAnimation : MonoBehaviour
{
    [SerializeField] private TMP_Text tmpText;

    [Header("文字表示速度")]
    [SerializeField] private float textSpeed = 0.05f;

    [Header("全文表示後の待ち時間")]
    [SerializeField] private float waitAfterFinish = 1.0f;

    public bool IsFinished { get; private set; }

    public void Play()
    {
        StopAllCoroutines();
        StartCoroutine(Simple());
    }

    private IEnumerator Simple()
    {
        IsFinished = false;

        tmpText.maxVisibleCharacters = 0;

        for (int i = 0; i < tmpText.text.Length; i++)
        {
            yield return new WaitForSeconds(textSpeed);
            tmpText.maxVisibleCharacters = i + 1;
        }

        // ← 全文表示後に待機
        yield return new WaitForSeconds(waitAfterFinish);

        IsFinished = true;
    }
}