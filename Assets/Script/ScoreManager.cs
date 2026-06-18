using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }
    private int score = 0;

    void Awake() { Instance = this; }

    public void AddScore(int amount)
    {
        score += amount;
        Debug.Log($"スコア加点！ 現在のスコア: {score}");
    }

    public void DecreaseScore(int amount)
    {
        score -= amount;
        Debug.Log($"スコア減点！ 現在のスコア: {score}");
    }
}