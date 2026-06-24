using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IntroTutorial : MonoBehaviour
{

    [SerializeField] private TutorialPageManager pageManager;

    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Image chuojiImage;

    [SerializeField] private Sprite normal;
    [SerializeField] private Sprite surprised;
    [SerializeField] private Sprite longSurprised;

    [SerializeField] private TextAnimation textAnimation;

    private string[] lines =
    {
        "こんにちは！ぼくはちゅうおうじ！",
        "オープンキャンパスに来てくれてありがとう！！",
        "わっ！！",
        "どうやら悪いやつらが来たみたい！",
        "ぼくたちで力を合わせておっぱらおう！！"
    };

    private Sprite[] faces;

    void Start()
    {
        faces = new Sprite[]
        {
            normal,
            normal,
            longSurprised,
            surprised,
            normal
        };

        StartCoroutine(PlayDialogue());
    }

   private IEnumerator PlayDialogue()
{
    for (int i = 0; i < lines.Length; i++)
    {
        dialogueText.text = lines[i];
        chuojiImage.sprite = faces[i];

        textAnimation.Play();

        yield return new WaitUntil(() => textAnimation.IsFinished);

        yield return new WaitForSeconds(1f);
    }

    pageManager.ShowAttackPage();
}

}