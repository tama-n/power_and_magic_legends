using System.Collections.Generic;
using TMPro;
using UnityEngine;

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

    [Header("ランキングUI設定")]
    [SerializeField] private TextMeshProUGUI[] rankingTexts;
    [Header("ランキングの見出し")]
    [SerializeField] private string rankingTitle = "スコアランキング"; 

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

        score = startingScore; 
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

        if (gameOverPanels != null)
        {
            foreach (GameObject panel in gameOverPanels)
            {
                if (panel != null) panel.SetActive(true);
            }
        }

        if (finalScoreTexts != null)
        {
            foreach (TMP_Text text in finalScoreTexts)
            {
                if (text != null) text.text = $"SCORE: {score}";
            }
        }

        UpdateAndShowRanking();
    }

    //今回のスコアを保存し、Top3を更新する関数
    public void UpdateAndShowRanking()
    {
        int currentScore = CurrentScore; 

        //現在のTop3をPlayerPrefsから読み込む
        List<int> highScores = new List<int>();
        highScores.Add(PlayerPrefs.GetInt("HighScore1", 0));
        highScores.Add(PlayerPrefs.GetInt("HighScore2", 0));
        highScores.Add(PlayerPrefs.GetInt("HighScore3", 0));

        highScores.Add(currentScore);
        highScores.Sort((a, b) => b.CompareTo(a)); 

        PlayerPrefs.SetInt("HighScore1", highScores[0]);
        PlayerPrefs.SetInt("HighScore2", highScores[1]);
        PlayerPrefs.SetInt("HighScore3", highScores[2]);
        PlayerPrefs.Save(); 

        string rankingString = $"{rankingTitle}\n";
        rankingString += $"1st : {highScores[0]}\n";
        rankingString += $"2nd : {highScores[1]}\n";
        rankingString += $"3rd : {highScores[2]}\n";

        if (rankingTexts != null)
        {
            foreach (TextMeshProUGUI text in rankingTexts)
            {
                if (text != null)
                {
                    text.text = rankingString;
                }
            }
        }
    }
}
