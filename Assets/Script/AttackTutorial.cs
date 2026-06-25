using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AttackTutorial : MonoBehaviour
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
        "Joyconを振って迫りくる敵を攻撃だ！！",
        "敵との距離によって近接と魔法を使い分けよう！！",
        "ZRボタンで近接、Rボタンで魔法攻撃に切り替えられるよ！！",
        "敵を倒すと得点をもらえるよ！！"
    };

    private Sprite[] faces;

    void OnEnable()
    {
        faces = new Sprite[]
        {
            normal,
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