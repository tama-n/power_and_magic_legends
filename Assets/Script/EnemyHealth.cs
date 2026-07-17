using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    [Header("---敵のHP設定----")]
    [SerializeField] private int maxHp = 100;

    public int hp;

    [Header("---HPバー設定----")]
    [SerializeField] private Slider hpSlider;

    [Header("倒したときのスコア加算量")]
    [SerializeField] private int getScoreAmount = 100;

    [Header("スコア減少量")]
    [SerializeField] private int decScoreAmount = 300;

    [HideInInspector]
    public bool isDefeatedByPlayer = false;

    private void Awake()
    {
        // Inspectorに登録されていない場合、
        // 敵の子オブジェクトからSliderを自動で探す
        if (hpSlider == null)
        {
            hpSlider = GetComponentInChildren<Slider>(true);
        }

        if (hpSlider == null)
        {
            Debug.LogError(
                $"{gameObject.name}の子オブジェクトにSliderが見つかりません。",
                gameObject
            );
        }
    }

    // 敵が出現するたびにHPを満タンにリセット
    private void OnEnable()
    {
        hp = maxHp;
        isDefeatedByPlayer = false;

        UpdateHpBar();
    }

    // プレイヤーの攻撃から受けるダメージ処理
    public void TakeDamage(int damage)
    {
        if (hp <= 0)
        {
            return;
        }

        hp -= damage;

        // HPが0未満にならないようにする
        hp = Mathf.Clamp(hp, 0, maxHp);

        UpdateHpBar();

        Debug.Log(
            $"{gameObject.name} に {damage} ダメージ！ 残りHP: {hp}"
        );

        if (hp <= 0)
        {
            isDefeatedByPlayer = true;
            Die();
        }
    }

    // HPバーの表示を更新
    private void UpdateHpBar()
    {
        if (hpSlider == null)
        {
            return;
        }

        hpSlider.minValue = 0;
        hpSlider.maxValue = maxHp;
        hpSlider.value = hp;
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} を倒した！");

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(getScoreAmount);
        }

        // 敵を使い回すため非アクティブにする
        gameObject.SetActive(false);
    }

    // プレイヤーにぶつかったらスコア減点
    private void OnCollisionEnter(Collision collision)
    {
        if (hp <= 0)
        {
            return;
        }

        if (collision.gameObject.CompareTag("Player"))
        {
            hp = 0;
            UpdateHpBar();

            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.DecreaseScore(decScoreAmount);
            }

            gameObject.SetActive(false);
        }
    }
}