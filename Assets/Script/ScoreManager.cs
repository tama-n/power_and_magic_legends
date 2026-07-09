using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("---右上のスコア表示UI---")]
    [Tooltip("左目用/右目用など、複数カメラそれぞれのCanvasに置いたTextMeshProUGUIをここに登録")]
    [SerializeField] private TMP_Text[] scoreTexts;

    [Header("---スコア初期値---")]
    [Tooltip("即死（開始直後のマイナス判定）を避けるための初期スコア")]
    [SerializeField] private int startingScore = 1000;

    private int score = 0;
    private bool isGameOverTriggered = false; // ゲームオーバーの二重発火防止

    // 他のスクリプト（GameOver画面など）から現在のスコアを取得できるように公開
    public int CurrentScore => score;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            // 万が一シーン内に複数存在した場合の重複防止
            Destroy(gameObject);
            return;
        }

        score = startingScore; // 初期スコアをセット
        UpdateScoreUI();
    }

    public void AddScore(int amount)
    {
        score += amount;
        Debug.Log($"スコア加点！ 現在のスコア: {score}");
        UpdateScoreUI();
    }

    public void DecreaseScore(int amount)
    {
        score -= amount;
        Debug.Log($"スコア減点！ 現在のスコア: {score}");

        // スコアがマイナスになったらゲームオーバー
        if (score < 0)
        {
            score = 0; // 表示上は0で止める（マイナス表示にしたくない場合）
            UpdateScoreUI();

            if (!isGameOverTriggered)
            {
                isGameOverTriggered = true;

                if (WaveManager.Instance != null)
                {
                    WaveManager.Instance.TriggerGameOver();
                }
            }
            return;
        }

        UpdateScoreUI();
    }

    // 登録されている全てのスコアテキスト（左目用・右目用カメラのCanvasなど）を更新
    private void UpdateScoreUI()
    {
        if (scoreTexts == null) return;

        foreach (TMP_Text text in scoreTexts)
        {
            if (text != null)
            {
                text.text = $"SCORE: {score}";
            }
        }
    }
}
