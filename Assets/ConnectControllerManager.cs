using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class ConnectControllerManager : MonoBehaviour
{
    public TMP_Text statusText;

    void Start()
    {
        List<Joycon> joycons = JoyconManager.Instance.j;

        if (joycons == null || joycons.Count == 0)
        {
            statusText.text = "No Controller";
            Debug.Log("Joy-Con未接続");
        }
        else
        {
            statusText.text = "Joy-Con Connected : " + joycons.Count;

            Debug.Log("Joy-Con接続数: " + joycons.Count);

            foreach (Joycon j in joycons)
            {
                Debug.Log(j.isLeft ? "Left Joy-Con" : "Right Joy-Con");
            }
        }
    }

    public void BackToTitle() {
        SceneManager.LoadScene("TitleScene");
    }

    public void Reload() {
        this.Start();
    }
}