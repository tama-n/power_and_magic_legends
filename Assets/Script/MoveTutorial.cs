using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MoveTutorial : MonoBehaviour
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
        "体を傾けると左右のレーンに移動できるよ！！",
        "敵の前に移動して攻撃しよう！！",
        "もし敵とぶつかったらスコアが減っちゃうよ！！"
    };

    private Sprite[] faces;

    void OnEnable()
    {
        faces = new Sprite[]
        {
            normal,
            normal,
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

    pageManager.ShowMovePage();
}

}