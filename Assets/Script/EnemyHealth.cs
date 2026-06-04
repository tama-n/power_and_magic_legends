using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("--- HP設定 (enemy.txtより統合) ---")]
    [SerializeField] private int maxHp = 100; // インスペクターから変更可能な最大HP
    private int hp;

    // オブジェクトプールで復活（アクティブ化）した瞬間に呼ばれる
    void OnEnable()
    {
        hp = maxHp; // HPを満タンにリセット
    }

    // プレイヤーの攻撃（PlayerController）から呼ばれるダメージ処理
    public void TakeDamage(int damage)
    {
        hp -= damage;
        Debug.Log($"{gameObject.name} に {damage} ダメージ！ 残りHP: {hp}");

        if (hp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} を倒した！");

        // 敵を倒したのでスコア+100
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(100);
        }

        // 重要：オブジェクトプールへ返却（非アクティブ化）
        gameObject.SetActive(false);
    }

    // プレイヤーに接触したときの減点処理
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.DecreaseScore(100);
            }
            // ぶつかった時もプールに戻る
            gameObject.SetActive(false);
        }
    }
}