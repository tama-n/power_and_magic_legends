using System.Collections;
using TMPro;
using UnityEngine;

public class TextAnimation : MonoBehaviour {
    [SerializeField] private TMP_Text tmpText;

    private float ward_par_flame = 0.2f;

    void Start() {
        StartCoroutine(Simple());
    }

    private IEnumerator Simple() {
        tmpText.maxVisibleCharacters = 0;

        for(int i = 0; i < tmpText.text.Length; i++) {
            yield return new WaitForSeconds(ward_par_flame);
            tmpText.maxVisibleCharacters = i+1;
        }
    }
}