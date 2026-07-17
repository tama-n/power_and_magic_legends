using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class TransGame : MonoBehaviour
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
        "準備はいい？\nいよいよ戦闘開始だ！"
    };

    private Sprite[] faces;

    void Start()
    {
        faces = new Sprite[]
        {
            normal,
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

    SceneManager.LoadScene("GameScene");
}

}