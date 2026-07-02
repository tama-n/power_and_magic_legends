using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Playerのレーン移動をするプログラム将来的にはジョイコンのジャイロで
public class PlayerLaneControl : MonoBehaviour
{
    private Joycon leftJoycon;

    public Vector3 accel;
    public Quaternion orientation;
    private bool tiltedRight = false;
    private bool tiltedLeft = false;
    public int jc_ind = 0;
    // 各レーンのX座標
    private float[] lanePositions = new float[] { 60f, 75f, 91f };

    // 現在のレーンインデックス（0: 左, 1: 中央, 2: 右）
    private int currentLane = 1;

    [SerializeField] private float moveSpeed = 10f;

    void Start()
    {
        if (JoyconManager.Instance == null) return;

        foreach (Joycon j in JoyconManager.Instance.j)
        {
            if (j.isLeft)
            {
                leftJoycon = j;
                Debug.Log("移動用：左Joy-Con取得");
                break;
            }
        }
    }

    void Update()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard != null)
        {
            if (keyboard.aKey.wasPressedThisFrame)
            {
                if (currentLane > 0)
                {
                    currentLane--;
                }
            }

            if (keyboard.dKey.wasPressedThisFrame)
            {
                if (currentLane < 2)
                {
                    currentLane++;
                }
            }
        }
        
        float threshold = 0.7f;

        if (leftJoycon != null)
        {
            accel = leftJoycon.GetAccel();

            if (accel.x < -threshold && !tiltedLeft)
            {
                if (currentLane > 0)
                {
                    currentLane--;
                    tiltedLeft = true;
                    tiltedRight = true;
                }
            }

            if (accel.x > threshold && !tiltedRight)
            {
                if (currentLane < 2)
                {
                    currentLane++;
                    tiltedRight = true;
                    tiltedLeft = true;
                }
            }

            if (Mathf.Abs(accel.x) < 0.05f)
            {
                tiltedRight = false;
                tiltedLeft = false;
            }
        }

        Vector3 targetPosition = new Vector3(lanePositions[currentLane], transform.position.y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * moveSpeed);
    }
}
