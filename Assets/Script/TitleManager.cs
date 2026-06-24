using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("MainScene");
    }

    public void OpenControllerConnect()
    {
        SceneManager.LoadScene("ConnectController");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}