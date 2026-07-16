using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("右上のスコア表示UI")]
    [Tooltip("左目用/右目用など、複数カメラそれぞれのCanvasに置いたTextMeshProUGUIをここに登録")]
    [SerializeField] private TMP_Text[] scoreTexts;

    [Header("左右それぞれの最終スコアテキスト")]
    [SerializeField] private TMP_Text[] finalScoreTexts;
    [Header("左右それぞれのゲームオーバー/リザルトパネル")]
    [SerializeField] private GameObject[] gameOverPanels;

    [Header("スコア初期値")]
    [SerializeField] private int startingScore = 0;

    private int score = 0;
    private bool isGameOverTriggered = false; 

    public int CurrentScore => score;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
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

        if (score < 0)
        {
            score = 0; 
        }
        Debug.Log($"スコア減点！ 現在のスコア: {score}");
        UpdateScoreUI();
    }

    //スコアテキストを更新
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

    public void ShowFinalResults(bool isCleared, string titleText, string clearTitle, string gameOverTitle)
    {
        // 1. ゲームオーバーパネルを左右同時に表示
        if (gameOverPanels != null)
        {
            foreach (GameObject panel in gameOverPanels)
            {
                if (panel != null) panel.SetActive(true);
            }
        }

        // 2. 最終スコアテキストを左右同時に更新
        if (finalScoreTexts != null)
        {
            foreach (TMP_Text text in finalScoreTexts)
            {
                if (text != null) text.text = $"SCORE: {score}";
            }
        }
    }
}
