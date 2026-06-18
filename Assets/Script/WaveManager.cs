using UnityEngine;
using UnityEngine.UI;

public class WaveManager : MonoBehaviour
{
    [Header("--- タイム設定 ---")]
    [SerializeField] private float battleDuration = 25f; // 戦闘時間
    private float timer = 0f;

    [Header("--- UI設定 ---")]
    [SerializeField] private GameObject upgradePanel; // ステップ1で作ったPanelをドラッグ＆ドロップ

    private bool isUpgrading = false;

    void Start()
    {
        // ゲーム開始時はタイマーリセット、UIは隠す、時間は動かす
        timer = battleDuration;
        upgradePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    void Update()
    {
        // 強化選択中はタイマーを進めない
        if (isUpgrading) return;

        // タイマーを減算
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            StartUpgradePhase();
        }
    }

    // 強化フェーズの開始（25秒経ったら呼ばれる）
    private void StartUpgradePhase()
    {
        isUpgrading = true;
        upgradePanel.SetActive(true); // 強化画面を表示

        // ★最重要：Unityの世界の時間を止める
        Time.timeScale = 0f;

        Debug.Log("25秒経過！ゲームを一時停止して強化画面を開きました。");
    }

    // 強化が選ばれたら呼び出す（ゲーム再開）
    public void SelectUpgrade(string upgradeType)
    {
        // ここでプレイヤーのステータスを強化する（後ほど実装）
        Debug.Log($"{upgradeType} が選択されました！ゲームを再開します。");

        upgradePanel.SetActive(false); // 強化画面を隠す
        timer = battleDuration;        // タイマーを25秒にリセット
        isUpgrading = false;

        // ★最重要：Unityの世界の時間を動き出させる
        Time.timeScale = 1f;
    }
}