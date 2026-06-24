using UnityEngine;

public class TutorialPageManager : MonoBehaviour
{
    [SerializeField] private GameObject introPage;
    [SerializeField] private GameObject movePage;
    [SerializeField] private GameObject attackPage;

    public void ShowIntroPage()
    {
        HideAllPages();
        introPage.SetActive(true);
    }

    public void ShowMovePage()
    {
        HideAllPages();
        movePage.SetActive(true);
    }

    public void ShowAttackPage()
    {
        HideAllPages();
        attackPage.SetActive(true);
    }

    private void HideAllPages()
    {
        introPage.SetActive(false);
        movePage.SetActive(false);
        attackPage.SetActive(false);
    }
}