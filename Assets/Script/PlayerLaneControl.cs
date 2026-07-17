using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Playerのレーン移動をするプログラム将来的にはジョイコンのジャイロで
[RequireComponent(typeof(AudioSource))]
public class PlayerLaneControl : MonoBehaviour
{
    [Header("効果音")]
    [SerializeField] private AudioClip moveSound; // レーン移動音

    private AudioSource audioSource;

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
        audioSource = GetComponent<AudioSource>();

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
                    PlaySound(moveSound);
                }
            }

            if (keyboard.dKey.wasPressedThisFrame)
            {
                if (currentLane < 2)
                {
                    currentLane++;
                    PlaySound(moveSound);
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
                    PlaySound(moveSound);
                }
            }

            if (accel.x > threshold && !tiltedRight)
            {
                if (currentLane < 2)
                {
                    currentLane++;
                    tiltedRight = true;
                    tiltedLeft = true;
                    PlaySound(moveSound);
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

    //効果音再生の共通処理
    private void PlaySound(AudioClip clip)
    {
        if (clip == null || audioSource == null) return;
        audioSource.PlayOneShot(clip);
    }
}
