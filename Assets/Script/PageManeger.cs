using UnityEngine;

public class TutorialPageManager : MonoBehaviour
{
    [Header("--- L側（左目用）のページ ---")]
    [SerializeField] private GameObject introPageL;
    [SerializeField] private GameObject movePageL;
    [SerializeField] private GameObject attackPageL;
    [SerializeField] private GameObject upgradePageL;
    [SerializeField] private GameObject transGamePageL;

    [Header("--- R側（右目用）のページ ---")]
    [SerializeField] private GameObject introPageR;
    [SerializeField] private GameObject movePageR;
    [SerializeField] private GameObject attackPageR;
    [SerializeField] private GameObject upgradePageR;
    [SerializeField] private GameObject transGamePageR;

    private void Start()
    {
        ShowIntroPage();
    }

    public void ShowIntroPage()
    {
        HideAllPages();
        if (introPageL != null) introPageL.SetActive(true);
        if (introPageR != null) introPageR.SetActive(true);
    }

    public void ShowMovePage()
    {
        HideAllPages();
        if (movePageL != null) movePageL.SetActive(true);
        if (movePageR != null) movePageR.SetActive(true);
    }

    public void ShowAttackPage()
    {
        HideAllPages();
        if (attackPageL != null) attackPageL.SetActive(true);
        if (attackPageR != null) attackPageR.SetActive(true);
    }

    public void ShowUpgradePage()
    {
        HideAllPages();
        if (upgradePageL != null) upgradePageL.SetActive(true);
        if (upgradePageR != null) upgradePageR.SetActive(true);
    }

    public void ShowTransGamePage()
    {
        HideAllPages();
        if (transGamePageL != null) transGamePageL.SetActive(true);
        if (transGamePageR != null) transGamePageR.SetActive(true);
    }

    private void HideAllPages()
    {
        if (introPageL != null) introPageL.SetActive(false);
        if (movePageL != null) movePageL.SetActive(false);
        if (attackPageL != null) attackPageL.SetActive(false);
        if (upgradePageL != null) upgradePageL.SetActive(false);
        if (transGamePageL != null) transGamePageL.SetActive(false);

        if (introPageR != null) introPageR.SetActive(false);
        if (movePageR != null) movePageR.SetActive(false);
        if (attackPageR != null) attackPageR.SetActive(false);
        if (upgradePageR != null) upgradePageR.SetActive(false);
        if (transGamePageR != null) transGamePageR.SetActive(false);
    }
}