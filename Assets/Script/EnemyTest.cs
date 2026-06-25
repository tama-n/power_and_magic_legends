using UnityEngine;

public class EnemyTest : MonoBehaviour
{
    [SerializeField] private EnemyHP enemyHP;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            enemyHP.Damage(10);
        }
    }
}