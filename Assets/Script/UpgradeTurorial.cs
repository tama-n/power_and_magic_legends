using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeTutorial : MonoBehaviour
{
    [SerializeField] private TutorialPageManager pageManager;

    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Image chuojiImage;

    [SerializeField] private Sprite normal;
    [SerializeField] private TextAnimation textAnimation;

    [Header("強化選択画面の画像")]
    [SerializeField] private GameObject upgradeSelectImage;

    private readonly string[] lines =
    {
        "一定時間ごとに、\n"
        + "自分を強化できるんだ！",

        "Joy-ConのAボタンで右、\n"
        + "Yボタンで左に\n"
        + "選択を動かそう！",

        "強化を選んだら、\n"
        + "Joy-Conを振って\n"
        + "決定だ！",

        "どの強化を選ぶかで\n",

        "戦い方が変わるよ！\n"
        + "いろいろ試してみよう！"
    };

    private Sprite[] faces;

    private void Awake()
    {
        faces = new Sprite[]
        {
            normal,
            normal,
            normal,
            normal,
            normal
        };

        if (upgradeSelectImage != null)
        {
            upgradeSelectImage.SetActive(false);
        }
    }

    private void OnEnable()
    {
        StopAllCoroutines();

        if (upgradeSelectImage != null)
        {
            upgradeSelectImage.SetActive(true);
        }

        StartCoroutine(PlayDialogue());
    }

    private void OnDisable()
    {
        StopAllCoroutines();

        if (upgradeSelectImage != null)
        {
            upgradeSelectImage.SetActive(false);
        }
    }

    private IEnumerator PlayDialogue()
    {
        for (int i = 0; i < lines.Length; i++)
        {
            dialogueText.text = lines[i];
            chuojiImage.sprite = faces[i];

            textAnimation.Play();

            yield return new WaitUntil(
                () => textAnimation.IsFinished
            );
        }

        pageManager.ShowTransGamePage();
    }
}