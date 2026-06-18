using UnityEngine;
using System.Collections.Generic;

public class JoyconTest : MonoBehaviour
{
    List<Joycon> joycons;

    void Start()
    {
        joycons = JoyconManager.Instance.j;
        Debug.Log("JoyCon数: " + joycons.Count);
    }

    void Update()
    {
        if (joycons == null || joycons.Count == 0) return;

        Joycon jc = joycons[0];

        if (jc.GetButtonDown(Joycon.Button.DPAD_DOWN))
        {
            Debug.Log("下ボタン！");
        }

        float[] stick = jc.GetStick();

        Debug.Log(
            "X:" + stick[0].ToString("F2") +
            " Y:" + stick[1].ToString("F2")
        );
    }
}