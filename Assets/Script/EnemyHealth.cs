using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    [Header("---敵のHP設定----")]
    [SerializeField] private int maxHp = 100; //インスペクターから設定可
    private int hp;

    [SerializeField] private Slider hpSlider; //HPバーのスライダー

    //敵が出現もしくは復活するたびにHPを満タンにリセット
    void OnEnable()
    {
        hp = maxHp;
        hpSlider.maxValue = maxHp;
        hpSlider.value = hp;
    }

    //プレイヤーの攻撃から受けるダメージ処理
    public void TakeDamage(int damage)
    {
        hp -= damage;
        Debug.Log($"{gameObject.name} に {damage} ダメージ！ 残りHP: {hp}");

        if (hpSlider != null)
        {
            hpSlider.value = hp;
        }

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

        //敵を使いまわすために非アクティブ
        gameObject.SetActive(false);
    }

    //プレイヤーに接触したときのスコア減点処理
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.DecreaseScore(100);
            }
            //ぶつかった時もプールに戻る
            gameObject.SetActive(false);
        }
    }
}