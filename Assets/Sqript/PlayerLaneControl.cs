using UnityEngine;
using UnityEngine.InputSystem;


//Playerのレーン移動をするプログラム将来的にはジョイコンのジャイロで
public class PlayerLaneControl : MonoBehaviour
{
    // 各レーンのX座標
    private float[] lanePositions = new float[] { 60f, 75f, 91.5f };

    // 現在のレーンインデックス（0: 左, 1: 中央, 2: 右）
    private int currentLane = 1; 

    [SerializeField] private float moveSpeed = 10f;

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

        Vector3 targetPosition = new Vector3(lanePositions[currentLane], transform.position.y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * moveSpeed);
    }
}