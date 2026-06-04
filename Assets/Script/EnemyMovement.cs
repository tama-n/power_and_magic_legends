using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("移動速度")]
    public float speed = 5f;

    [Header("消滅するZ座標（手前の限界位置）")]
    public float despawnZ = -10f;

    void Update()
    {
        // 手前（Z軸マイナス方向）に向かって移動
        transform.Translate(Vector3.back * speed * Time.deltaTime);

        // 一定ライン（手前）を越えたら非アクティブにする（プールへ返却と同義）
        if (transform.position.z < despawnZ)
        {
            gameObject.SetActive(false);
        }
    }
}