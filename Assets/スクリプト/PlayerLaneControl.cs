using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Playerのレーン移動をするプログラム将来的にはジョイコンのジャイロで
public class PlayerLaneControl : MonoBehaviour
{
    private List<Joycon> joycons;

    public Vector3 accel;
    public Quaternion orientation;
    private bool tiltedRight = false;
    private bool tiltedLeft = false;
    public int jc_ind = 0;
    // 各レーンのX座標
    private float[] lanePositions = new float[] { 60f, 75f, 91.5f };

    // 現在のレーンインデックス（0: 左, 1: 中央, 2: 右）
    private int currentLane = 1;

    [SerializeField] private float moveSpeed = 10f;

    void Start()
    {
        // get the public Joycon array attached to the JoyconManager in scene
        joycons = JoyconManager.Instance.j;
    }
    void Update()
    {
        float threshold = 0.7f;

        if (joycons.Count > 0)
        {
            Joycon j = joycons[jc_ind];
            accel = j.GetAccel();
            // 左へ傾けたら左移動
            if (accel.x < -threshold && !tiltedLeft)
            {
                if (currentLane > 0)
                {
                    currentLane--;
                    tiltedLeft = true;
                }
            }

            // 右へ傾けたら右移動
            if (accel.x > threshold && !tiltedRight)
            {
                if (currentLane < 2)
                {
                    currentLane++;
                    tiltedRight = true;
                }
            }
        }

        // 中央付近に戻ったらリセット
        if (Mathf.Abs(accel.x) < 0.05f)
        {
            tiltedRight = false;
            tiltedLeft = false;
        }

        Vector3 targetPosition = new Vector3(lanePositions[currentLane], transform.position.y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * moveSpeed);
    }
}