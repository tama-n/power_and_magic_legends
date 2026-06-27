using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using System.Collections.Generic;

public class WaveManager : MonoBehaviour
{
    [Header("--- タイム設定 ---")]
    [SerializeField] private float battleDuration = 25f; //戦闘時間
    [SerializeField] private float upgradeDuration = 5f;  //強化選択の時間

    private float timer = 0f;
    private bool isUpgrading = false;

    [Header("--- UI設定 ---")]
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private TextMeshProUGUI timerText; //5秒をカウントダウンする文字用

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

    private PlayerController player;

    void Start()
    {
        //最初は25秒の戦闘からスタート
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
            if (timerText != null)
            {
                timerText.text = $"残り時間: {Mathf.CeilToInt(timer)}秒";
            }

            if (timer <= 0f)
            {
                EndUpgradePhase("時間切れ（強化なし）");
            }
        }
    }

    //強化フェーズの開始（25秒経ったら呼ばれる）
    private void StartUpgradePhase()
    {
        isUpgrading = true;
        upgradePanel.SetActive(true); //強化画面を表示
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


        upgradePanel.SetActive(false); //強化画面を隠す
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
        }
    }
    private void FinishGame()
    {
        Time.timeScale = 0f; //ゲーム世界を完全にストップ

        Debug.Log("ゲーム終了");
    }
}