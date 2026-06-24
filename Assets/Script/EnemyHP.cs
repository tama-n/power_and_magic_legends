using UnityEngine;
using UnityEngine.UI;

public class EnemyHP : MonoBehaviour
{
    [SerializeField] private Slider hpSlider;

    private int hp = 100;

    void Start()
    {
        hpSlider.maxValue = 100;
        hpSlider.value = hp;
    }

    public void Damage(int value)
    {
        hp -= value;

        if (hp < 0)
        {
            hp = 0;
        }

        hpSlider.value = hp;
    }
}