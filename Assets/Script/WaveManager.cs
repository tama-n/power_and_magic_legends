using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro; 
using System.Collections.Generic;

public class WaveManager : MonoBehaviour
{

    public static WaveManager Instance { get; private set; } //シングルトン(https://jp-seemore.com/sys/17625/)

    [Header("--- タイム設定 ---")]
    [SerializeField] private float battleDuration = 25f; //戦闘時間
    [SerializeField] private float upgradeDuration = 5f;  //強化選択の時間

    private float timer = 0f;
    private bool isUpgrading = false;

    [Header("--- UI設定 ---")]
    [SerializeField] private GameObject[] upgradePanels;
    [SerializeField] private TextMeshProUGUI[] timerTexts; //5秒をカウントダウンする文字用

    [SerializeField] private GameObject[] upgradeButtons; //強化ボタンを登録

    [Header("--- 強化の上昇値設定 ---")]
    [SerializeField] private int attackUpgradeAmount = 30;       //攻撃力の上昇値
    [SerializeField] private float criticalUpgradeAmount = 10f;  //クリティカル率の上昇値（%）
    [SerializeField] private float closeRangeUpgradeAmount = 1.0f; //近距離攻撃のリーチの上昇値
    [SerializeField] private float rangeAttackDistUpgradeAmount = 10.0f; //遠距離攻撃の飛距離の上昇値
    [SerializeField] private float magicCooldownReduceAmount = 0.5f; //魔法のクールダウン短縮の上昇値（秒）

    [Header("---ウェーブ数---")]
    [SerializeField] private int maxWaves = 8;
    private int currentWave = 1;

    [Header("敵の強化")]
    [SerializeField] private float speedIncreasePerWave = 0.1f;
    [SerializeField] private float spawnIntervalDecreasePerWave = 0.15f;

    [Header("--- ゲームオーバー画面 ---")]
    [Tooltip("左目用/右目用カメラなど、各Canvasに置いたゲームオーバーパネルを登録")]
    [SerializeField] private GameObject[] gameOverPanels;
    [Tooltip("「GAME CLEAR」「GAME OVER」など見出しを表示するテキスト（任意。使わない場合は空でOK）")]
    [SerializeField] private TextMeshProUGUI[] resultTitleTexts;
    [Tooltip("左目用/右目用カメラなど、各Canvasに置いた最終スコア表示テキストを登録")]
    [SerializeField] private TextMeshProUGUI[] finalScoreTexts;

    [Tooltip("クリア時に表示する見出し文言")]
    [SerializeField] private string clearTitle = "GAME CLEAR!";
    [Tooltip("ゲームオーバー時に表示する見出し文言")]
    [SerializeField] private string gameOverTitle = "GAME OVER...";

    private bool isGameOver = false; // ゲーム終了（クリア/オーバー問わず）の二重発火防止

    [Header("--- デバッグ用 ---")]
    [Tooltip("ONにすると、Pキーで強制クリア、Oキーで強制ゲームオーバーをテストできる")]
    [SerializeField] private bool enableDebugKeys = true;

    private PlayerController player;

    public int GetCurrentWave()
    {
        return currentWave;
    }

    public float GetSpeedIncreasePerWave()
    {
        return speedIncreasePerWave;
    }

    public float GetSpawnIntervalDecreasePerWave()
    {
        return spawnIntervalDecreasePerWave;
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        //最初は25秒の戦闘からスタート
        timer = battleDuration;
        isGameOver = false;

        //upgradePanel.SetActive(false); //強化画面は非表示(戦闘中)
        SetUpgradePanelsActive(false);

        if (gameOverPanels != null)
        {
            foreach (GameObject panel in gameOverPanels)
            {
                if (panel != null) panel.SetActive(false); //ゲームオーバー画面も最初は非表示
            }
        }

        Time.timeScale = 1f; //ゲーム再生

        //PlayerControllerのスクリプトがアタッチされてるオブジェクト(プレイヤー)を記憶しておく
        player = FindObjectOfType<PlayerController>();

        Debug.Log($"ゲーム開始。全{maxWaves}ウェーブ");
    }

    void Update()
    {
        if (enableDebugKeys)
        {
            HandleDebugKeys();
        }

        if (isGameOver) return; // ゲーム終了後はカウントダウン等を止める

        if (!isUpgrading)
        {
            //戦闘中の25秒のカウントダウン
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                StartUpgradePhase();
            }
        }
        else
        {
            //強化中5秒を測る
            timer -= Time.unscaledDeltaTime;

            //画面に「残り 4.2秒」のように整数で表示
            //if (timerText != null)
            //{
            //    timerText.text = $"残り時間: {Mathf.CeilToInt(timer)}秒";
            //}
            if (timerTexts != null)
            {
                string textContent = $"残り時間: {Mathf.CeilToInt(timer)}秒";
                foreach (TextMeshProUGUI text in timerTexts)
                {
                    if (text != null)
                    {
                        text.text = textContent;
                    }
                }
            }

            if (timer <= 0f)
            {
                EndUpgradePhase("時間切れ（強化なし）");
            }
        }
    }

    //デバッグ用：Pキーで強制クリア、Oキーで強制ゲームオーバー
    private void HandleDebugKeys()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.pKey.wasPressedThisFrame)
        {
            Debug.Log("[デバッグ] Pキー：強制ゲームクリア");
            EndGame(true);
        }

        if (keyboard.oKey.wasPressedThisFrame)
        {
            Debug.Log("[デバッグ] Oキー：強制ゲームオーバー");
            EndGame(false);
        }
    }

    //強化フェーズの開始（25秒経ったら呼ばれる）
    private void StartUpgradePhase()
    {
        isUpgrading = true;

        //upgradePanel.SetActive(true); //強化画面を表示
        SetUpgradePanelsActive(true);

        timer = upgradeDuration;       //タイマーを5秒にセット

        //敵やプレイヤーの動きをストップ
        Time.timeScale = 0f;

        //強化ボタンをランダムに3つ選んで表示する
        SelectRandomButtons();
    }

    //強化ボタンの中からランダムに3つ選んで表示する関数
    private void SelectRandomButtons()
    {
        foreach(GameObject btn in upgradeButtons)
        {
            btn.SetActive(false); 
        }

        List<int> indexList = new List<int> { 0, 1, 2, 3, 4 };

        for (int i = 0; i < 3; i++)
        {
            //残っているインデックスの中からランダムに1つ位置を選ぶ
            int randomIndex = Random.Range(0, indexList.Count);

            //選ばれた位置にあるボタンの本来のインデックスを取得
            int chosenButtonIndex = indexList[randomIndex];

            //そのボタンを表示
            upgradeButtons[chosenButtonIndex].SetActive(true);

            indexList.RemoveAt(randomIndex);
        }
    }


    //UIボタンから呼ばれる関数
    //ボタンのインスペクター（OnClick）にこれを登録
    public void OnSelectUpgradeButton(string upgradeType)
    {
        //すでに時間切れになっていたら処理しない
        if (!isUpgrading) return;

        EndUpgradePhase(upgradeType);
    }

    //強化フェーズの終了とゲーム再開
    private void EndUpgradePhase(string choiceResult)
    {
        Debug.Log($"【結果】: {choiceResult} が選ばれました！ゲームを再開します。");


        //upgradePanel.SetActive(false); //強化画面を隠す
        SetUpgradePanelsActive(false);

        //timer = battleDuration;        //タイマーを25秒にリセット
        isUpgrading = false;
        
        //ポーズ解除
        Time.timeScale = 1f;

        //もしプレイヤーが見つからなければ、強化をスキップ
        if (player == null) return;

        //文字列（引数）に応じて強化
        switch (choiceResult)
        {
            case "AttackUp":
                player.BoostAttack(attackUpgradeAmount); 
                break;

            case "CriticalUp":
                player.BoostCriticalChance(criticalUpgradeAmount); 
                break;

            case "CloseRangeUp": 
                player.BoostCloseRange(closeRangeUpgradeAmount); 
                break;

            case "RangeDistUp":  
                player.BoostRangeAttackDistance(rangeAttackDistUpgradeAmount); 
                break;

            case "MagicCooldownReduce":
                player.ReduceMagicCooldown(magicCooldownReduceAmount); 
                break;

            case "TimeUp":
                Debug.Log("時間切れ！強化は獲得できませんでした。");
                break;
        }

        if(currentWave >= maxWaves)
        {
            FinishGame();
        }
        else
        {
            currentWave++;
            timer = battleDuration;

            float nextInterval = 1.5f - (currentWave - 1) * spawnIntervalDecreasePerWave;
            float finalInterval = Mathf.Max(nextInterval, 0.4f); 

            
            float speedMultiple = 1.0f + (currentWave - 1) * speedIncreasePerWave;
            float finalSpeed = 5.0f * speedMultiple;

            // 3. コンソールに大きく色付きで表示する
            Debug.Log($"敵の移動速度: {finalSpeed} (倍率: {speedMultiple}倍) / 出現間隔: {finalInterval}秒");
        }
    }

    private void SetUpgradePanelsActive(bool isActive)
    {
        if (upgradePanels == null) return;

        foreach (GameObject panel in upgradePanels)
        {
            if (panel != null)
            {
                panel.SetActive(isActive);
            }
        }
    }

    private void FinishGame()
    {
        EndGame(true); // 全ウェーブクリア
    }

    // スコアがマイナスになった時などにScoreManagerから呼ばれる（敗北）
    public void TriggerGameOver()
    {
        if (isGameOver) return; // 既に終了処理済みなら何もしない
        EndGame(false);
    }

    // クリア/ゲームオーバー共通の終了処理。isCleared で表示を出し分ける
    private void EndGame(bool isCleared)
    {
        if (isGameOver) return;
        isGameOver = true;

        Time.timeScale = 0f; //ゲーム世界を完全にストップ

        Debug.Log(isCleared ? "ゲームクリア" : "ゲームオーバー");

        //スコア画面を表示（左目用・右目用の両方のCanvas）
        //if (gameOverPanels != null)
        //{
        //    foreach (GameObject panel in gameOverPanels)
        //    {
        //        if (panel != null) panel.SetActive(true);
        //    }
        //}

        ////見出し（GAME CLEAR / GAME OVER）を反映（左目用・右目用の両方）
        //if (resultTitleTexts != null)
        //{
        //    string titleString = isCleared ? clearTitle : gameOverTitle;
        //    foreach (TextMeshProUGUI text in resultTitleTexts)
        //    {
        //        if (text != null) text.text = titleString;
        //    }
        //}

        ////最終スコアをテキストに反映（左目用・右目用の両方）
        //if (finalScoreTexts != null && ScoreManager.Instance != null)
        //{
        //    string scoreString = $"SCORE: {ScoreManager.Instance.CurrentScore}";
        //    foreach (TextMeshProUGUI text in finalScoreTexts)
        //    {
        //        if (text != null) text.text = scoreString;
        //    }
        //}
        if (ScoreManager.Instance != null)
        {
            string titleString = isCleared ? clearTitle : gameOverTitle;
            ScoreManager.Instance.ShowFinalResults(isCleared, titleString, clearTitle, gameOverTitle);
        }

        // 大見出し（GAME CLEAR / GAME OVER）のテキスト一括変更
        if (resultTitleTexts != null)
        {
            string titleString = isCleared ? clearTitle : gameOverTitle;
            foreach (TextMeshProUGUI text in resultTitleTexts)
            {
                if (text != null) text.text = titleString;
            }
        }
    }
}