using UnityEngine;
using UnityEngine.UI;
using TMPro; // 制限時間の文字を表示するために必要

public class WaveManager : MonoBehaviour
{
    [Header("--- タイム設定 ---")]
    [SerializeField] private float battleDuration = 25f; // 戦闘時間
    [SerializeField] private float upgradeDuration = 5f;  // 強化選択の時間（5秒）

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


    private PlayerController player;

    void Start()
    {
        // 最初は25秒の戦闘からスタート
        timer = battleDuration;
        upgradePanel.SetActive(false);
        Time.timeScale = 1f;

        //PlayerControllerのスクリプトがアタッチされてるオブジェクト(プレイヤー)を記憶しておく
        player = FindObjectOfType<PlayerController>();
    }

    void Update()
    {
        if (!isUpgrading)
        {
            // 【戦闘中】25秒のカウントダウン
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                StartUpgradePhase();
            }
        }
        else
        {
            // 【強化中】Time.timeScale = 0でも動く特殊な時間(UnscaledDeltaTime)で5秒を測る
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

        // Unityの世界の時間を止める（敵やプレイヤーの動きをストップ）
        Time.timeScale = 0f;
    }

    //UIボタンから呼ばれる関数
    //ボタンのインスペクター（OnClick）にこれを登録します
    public void OnSelectUpgradeButton(string upgradeType)
    {
        // すでに時間切れになっていたら処理しない安全対策
        if (!isUpgrading) return;

        EndUpgradePhase(upgradeType);
    }

    // 強化フェーズの終了とゲーム再開
    private void EndUpgradePhase(string choiceResult)
    {
        Debug.Log($"【結果】: {choiceResult} が選ばれました！ゲームを再開します。");

        // ここに「攻撃力アップ」などの実際の強化処理を今後書きます

        upgradePanel.SetActive(false); // 強化画面を隠す
        timer = battleDuration;        // タイマーを25秒にリセット
        isUpgrading = false;
        
        // Unityの世界の時間を動き出させる（ポーズ解除）
        Time.timeScale = 1f;

        // もしプレイヤーが見つからなければ、強化処理をスキップ（エラー防止）
        if (player == null) return;

        // 💡文字列（引数）に応じて、プレイヤーの窓口を呼び分ける！
        switch (choiceResult)
        {
            case "AttackUp":
                player.BoostAttack(attackUpgradeAmount); // 攻撃力をアップ！
                break;

            case "CriticalUp":
                player.BoostCriticalChance(criticalUpgradeAmount); // クリティカル率をアップ！
                break;

            case "CloseRangeUp": 
                player.BoostCloseRange(closeRangeUpgradeAmount); // 近距離攻撃のリーチをアップ！
                break;

            case "RangeDistUp":  
                player.BoostRangeAttackDistance(rangeAttackDistUpgradeAmount); // 遠距離攻撃の飛距離をアップ！
                break;

            case "TimeUp":
                Debug.Log("時間切れ！強化は獲得できませんでした。");
                break;
        }
    }
}