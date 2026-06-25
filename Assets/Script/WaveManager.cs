using UnityEngine;
using UnityEngine.UI;
using TMPro; 

public class WaveManager : MonoBehaviour
{
    [Header("--- タイム設定 ---")]
    [SerializeField] private float battleDuration = 25f; // 戦闘時間
    [SerializeField] private float upgradeDuration = 5f;  // 強化選択の時間

    private float timer = 0f;
    private bool isUpgrading = false;

    [Header("--- UI設定 ---")]
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private TextMeshProUGUI timerText; // 5秒をカウントダウンする文字用

    [Header("--- 強化の上昇値設定 ---")]
    [SerializeField] private int attackUpgradeAmount = 30;       // 攻撃力の上昇値
    [SerializeField] private float criticalUpgradeAmount = 10f;  // クリティカル率の上昇値（%）
    [SerializeField] private float closeRangeUpgradeAmount = 1.0f; // 近距離攻撃のリーチの上昇値
    [SerializeField] private float rangeAttackDistUpgradeAmount = 10.0f; // 遠距離攻撃の飛距離の上昇値
    [SerializeField] private float magicCooldownReduceAmount = 0.5f; // 魔法のクールダウン短縮の上昇値（秒）

    [Header("---ウェーブ数---")]
    [SerializeField] private int maxWaves = 8;
    private int currentWave = 1;

    private PlayerController player;

    void Start()
    {
        // 最初は25秒の戦闘からスタート
        timer = battleDuration;
        upgradePanel.SetActive(false); //強化画面は非表示(戦闘中)
        Time.timeScale = 1f; //ゲーム再生

        //PlayerControllerのスクリプトがアタッチされてるオブジェクト(プレイヤー)を記憶しておく
        player = FindObjectOfType<PlayerController>();

        Debug.Log($"ゲーム開始。全{maxWaves}ウェーブ");
    }

    void Update()
    {
        if (!isUpgrading)
        {
            // 戦闘中の25秒のカウントダウン
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                StartUpgradePhase();
            }
        }
        else
        {
            // 強化中5秒を測る
            timer -= Time.unscaledDeltaTime;

            // 画面に「残り 4.2秒」のように整数で表示
            if (timerText != null)
            {
                timerText.text = $"残り時間: {Mathf.CeilToInt(timer)}秒";
            }

            if (timer <= 0f)
            {
                // 5秒経っても選ばなかったら、強制的に再開
                EndUpgradePhase("時間切れ（強化なし）");
            }
        }
    }

    // 強化フェーズの開始（25秒経ったら呼ばれる）
    private void StartUpgradePhase()
    {
        isUpgrading = true;
        upgradePanel.SetActive(true); // 強化画面を表示
        timer = upgradeDuration;       // タイマーを5秒にセット

        // 敵やプレイヤーの動きをストップ
        Time.timeScale = 0f;
    }

    //UIボタンから呼ばれる関数
    //ボタンのインスペクター（OnClick）にこれを登録
    public void OnSelectUpgradeButton(string upgradeType)
    {
        // すでに時間切れになっていたら処理しない
        if (!isUpgrading) return;

        EndUpgradePhase(upgradeType);
    }

    // 強化フェーズの終了とゲーム再開
    private void EndUpgradePhase(string choiceResult)
    {
        Debug.Log($"【結果】: {choiceResult} が選ばれました！ゲームを再開します。");

        // ここに「攻撃力アップ」などの実際の強化処理を今後書く

        upgradePanel.SetActive(false); // 強化画面を隠す
        //timer = battleDuration;        // タイマーを25秒にリセット
        isUpgrading = false;
        
        // ポーズ解除
        Time.timeScale = 1f;

        // もしプレイヤーが見つからなければ、強化をスキップ
        if (player == null) return;

        // 文字列（引数）に応じて、プレイヤーの強化を呼び分け
        switch (choiceResult)
        {
            case "AttackUp":
                player.BoostAttack(attackUpgradeAmount); // 攻撃力をアップ
                break;

            case "CriticalUp":
                player.BoostCriticalChance(criticalUpgradeAmount); // クリティカル率をアップ
                break;

            case "CloseRangeUp": 
                player.BoostCloseRange(closeRangeUpgradeAmount); // 近距離攻撃のリーチをアップ
                break;

            case "RangeDistUp":  
                player.BoostRangeAttackDistance(rangeAttackDistUpgradeAmount); // 遠距離攻撃の飛距離をアップ
                break;

            case "MagicCooldownReduce":
                player.ReduceMagicCooldown(magicCooldownReduceAmount); // 魔法のクールダウンを短縮
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
        }
    }
    private void FinishGame()
    {
        Time.timeScale = 0f; // ゲーム世界を完全にストップ

        Debug.Log("ゲーム終了");

        // 今後ここにゲームクリア画面などを表示？
    }
}