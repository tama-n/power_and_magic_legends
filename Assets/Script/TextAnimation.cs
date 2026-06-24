using System.Collections;
using TMPro;
using UnityEngine;

public class TextAnimation : MonoBehaviour
{
    [SerializeField] private TMP_Text tmpText;

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
            yield return new WaitForSeconds(0.05f);
            tmpText.maxVisibleCharacters = i + 1;
        }

        IsFinished = true;
    }
}