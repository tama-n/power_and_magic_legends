using UnityEngine;

public class TutorialPageManager : MonoBehaviour
{
    [SerializeField] private GameObject introPage;
    [SerializeField] private GameObject movePage;
    [SerializeField] private GameObject attackPage;
    [SerializeField] private GameObject upgradePage;
    [SerializeField] private GameObject transGamePage;

    private void Start()
    {
        ShowIntroPage();
    }

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

    public void ShowUpgradePage()
    {
        HideAllPages();
        upgradePage.SetActive(true);
    }

    public void ShowTransGamePage()
    {
        HideAllPages();
        transGamePage.SetActive(true);
    }

    private void HideAllPages()
    {
        introPage.SetActive(false);
        movePage.SetActive(false);
        attackPage.SetActive(false);
        upgradePage.SetActive(false);
        transGamePage.SetActive(false);
    }
}