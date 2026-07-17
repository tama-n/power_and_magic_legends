using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections.Generic;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; } //シングルトン

    [Header("--- タイム設定 ---")]
    [SerializeField] private float battleDuration = 25f; //戦闘時間
    [SerializeField] private float upgradeDuration = 5f;  //強化選択の時間

    private float timer = 0f;
    private bool isUpgrading = false;
    public bool IsUpgrading => isUpgrading;

    [Header("--- UI設定 ---")]
    [SerializeField] private TextMeshProUGUI[] timerTexts; //5秒をカウントダウンする文字用
    [SerializeField] private GameObject[] upgradePanels;
    [SerializeField] private GameObject[] upgradeButtons; //強化ボタンを登録

    [Header("--- 選択中の大きさ設定（左右パネル共通） ---")]
    [Tooltip("選択されているボタンの大きさ（倍率）")]
    [SerializeField] private float selectedScale = 1.2f;
    [Tooltip("選択されていないボタンの大きさ")]
    [SerializeField] private float normalScale = 1.0f;

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

    [Header("--- 左右分割UIボタン ---")]
    [SerializeField] private GameObject[] upgradeButtonsL;
    [SerializeField] private GameObject[] upgradeButtonsR;

    [Header("--- ジョイコンモーション設定 ---")]
    [Tooltip("ジョイコンを振ったと判定する閾値（大きいほど強く振る必要がある。1.5〜3.0あたりで調整）")]
    [SerializeField] private float shakeThreshold = 2.0f;

    // ===== 🕹️ JoyconLib用内部変数 =====
    private List<Joycon> joycons;
    private Joycon rightJoycon;
    private int currentSelectedIndex = -1; // 現在選択されているボタンの配列インデックス(0~4)

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

        // 🕹️ Joy-Con的初期化取得
        if (JoyconManager.Instance != null)
        {
            joycons = JoyconManager.Instance.j;
        }

        Debug.Log($"ゲーム開始。全{maxWaves}ウェーブ");
    }

    void Update()
    {
        // 🕹️ 毎フレーム右Joy-Conの接続をチェック・確保
        FetchRightJoycon();

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
                if (currentWave >= maxWaves)
                {
                    FinishGame();
                }
                else
                {
                    StartUpgradePhase();
                }
            }
        }
        else
        {
            // Joy-Conでの選択操作は常に受け付ける
            HandleJoyconUIInput();

            // チュートリアル中はタイマーを減らさない
            if (isTutorialUpgrade)
            {
                return;
            }

            // 通常ゲームの強化選択だけ5秒カウントする
            timer -= Time.unscaledDeltaTime;

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
                SelectFirstUpgradeOnTimeout();
            }
        }
    }

    private void SelectFirstUpgradeOnTimeout()
    {
        // 左側のボタンを優先
        foreach (GameObject buttonObject in upgradeButtonsL)
        {
            if (buttonObject != null && buttonObject.activeSelf)
            {
                Debug.Log("時間切れ：左端の強化を自動選択");

                Button button = buttonObject.GetComponent<Button>();

                if (button != null)
                {
                    button.onClick.Invoke();
                }

                return;
            }
        }

        // 左側が無ければ右側
        foreach (GameObject buttonObject in upgradeButtonsR)
        {
            if (buttonObject != null && buttonObject.activeSelf)
            {
                Button button = buttonObject.GetComponent<Button>();

                if (button != null)
                {
                    button.onClick.Invoke();
                }

                return;
            }
        }

        // ボタンが1つも無い場合
        EndUpgradePhase("TimeUp");
    }

    // 🕹️ 右Joy-Conを特定して参照を持つ
    private void FetchRightJoycon()
    {
        if (joycons == null || joycons.Count == 0) return;
        if (rightJoycon != null) return; // すでに取得済みならスルー

        foreach (var j in joycons)
        {
            if (j.isLeft == false) // 左ではない＝右
            {
                rightJoycon = j;
                Debug.Log("右Joy-ConをUI操作用に認識しました。");
                break;
            }
        }
    }

    // 🕹️ 強化フェーズ中のJoy-Con入力（選択・振り）を処理する
    private void HandleJoyconUIInput()
    {
        if (rightJoycon == null) return;

        // 【Yボタン（左側ボタン）が押されたら選択を左へ】
        if (rightJoycon.GetButtonDown(Joycon.Button.DPAD_LEFT))
        {
            MoveSelection(-1);
        }

        // 【Aボタン（右側ボタン）が押されたら選択を右へ】
        if (rightJoycon.GetButtonDown(Joycon.Button.DPAD_RIGHT))
        {
            MoveSelection(1);
        }

        // 【決定：右ジョイコンを振る動き（加速度）を検知】
        Vector3 accel = rightJoycon.GetAccel();
        if (accel.magnitude > shakeThreshold)
        {
            Debug.Log($"[Joy-Con振りを検知] 勢い: {accel.magnitude} -> 決定します！");
            SubmitSelection();
        }
    }

    // 🕹️ ランダムに出現している（Activeな）ボタンだけを対象にループ移動させる
    private void MoveSelection(int direction)
    {
        if (currentSelectedIndex == -1) return;

        int checkIndex = currentSelectedIndex;

        for (int i = 0; i < upgradeButtonsL.Length; i++)
        {
            checkIndex += direction;

            // 配列の端に達したら逆側にループさせる
            if (checkIndex >= upgradeButtonsL.Length) checkIndex = 0;
            if (checkIndex < 0) checkIndex = upgradeButtonsL.Length - 1;

            // アクティブなボタンが見つかったらそこに決定
            if (upgradeButtonsL[checkIndex].activeSelf)
            {
                currentSelectedIndex = checkIndex;
                UpdateVisualSelection();
                break;
            }
        }
    }

    // 🕹️ 選択されているボタンの見た目（スケール・EventSystemフォーカス）を更新する
    private void UpdateVisualSelection()
    {
        // 1. まずすべてのボタン（LとRの両方）を「通常サイズ」にリセットする
        ResetAllButtonScales();

        // 2. 現在選択されているインデックスのボタン（L/Rとも）を大きくする
        if (currentSelectedIndex >= 0 && currentSelectedIndex < upgradeButtonsL.Length)
        {
            // --- L側のサイズ変更 ---
            SetButtonScale(upgradeButtonsL[currentSelectedIndex], selectedScale);

            // --- R側のサイズ変更（完全に同期） ---
            if (upgradeButtonsR != null && currentSelectedIndex < upgradeButtonsR.Length)
            {
                SetButtonScale(upgradeButtonsR[currentSelectedIndex], selectedScale);
            }

            // EventSystemのフォーカスも一応合わせておく
            GameObject targetButton = upgradeButtonsL[currentSelectedIndex];
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(targetButton);
            }
        }
    }

    // すべてのボタンのサイズを初期状態（標準サイズ）に戻す処理
    private void ResetAllButtonScales()
    {
        if (upgradeButtonsL != null)
        {
            foreach (GameObject button in upgradeButtonsL)
            {
                SetButtonScale(button, normalScale);
            }
        }

        if (upgradeButtonsR != null)
        {
            foreach (GameObject button in upgradeButtonsR)
            {
                SetButtonScale(button, normalScale);
            }
        }
    }

    // 対象のGameObjectのサイズ(Scale)を変更するヘルパー関数
    private void SetButtonScale(GameObject buttonObject, float scaleAmount)
    {
        if (buttonObject == null) return;

        RectTransform rectTransform = buttonObject.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.localScale = new Vector3(scaleAmount, scaleAmount, 1f);
        }
    }

    // 🕹️ ジョイコンが振られた時に、現在選ばれているUIボタンのOnClickイベントを強制実行する
    private void SubmitSelection()
    {
        if (currentSelectedIndex == -1) return;

        GameObject activeLeftButton = upgradeButtonsL[currentSelectedIndex];
        Button btn = activeLeftButton.GetComponent<Button>();

        if (btn != null && activeLeftButton.activeSelf)
        {
            btn.onClick.Invoke(); // インスペクターで設定したOnClick関数（OnSelectUpgradeButton）を呼び出す
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

        SetUpgradePanelsActive(true);

        timer = upgradeDuration;       //タイマーを5秒にセット

        //敵やプレイヤーの動きをストップ
        Time.timeScale = 0f;

        //強化ボタンをランダムに3つ選んで表示する
        SelectRandomButtons();

        // 🕹️ 最初の選択フォーカス位置を自動決定
        SelectFirstActiveButton();
    }

    private void SelectFirstActiveButton()
    {
        currentSelectedIndex = -1;
        for (int i = 0; i < upgradeButtonsL.Length; i++)
        {
            if (upgradeButtonsL[i].activeSelf)
            {
                currentSelectedIndex = i; // 最初に見つかった有効ボタンのインデックスを保存
                UpdateVisualSelection();
                break;
            }
        }
    }

    //強化ボタンの中からランダムに3つ選んで表示する関数
    private void SelectRandomButtons()
    {
        for (int i = 0; i < upgradeButtonsL.Length; i++)
        {
            upgradeButtonsL[i].SetActive(false);
            upgradeButtonsR[i].SetActive(false);
        }

        List<int> indexList = new List<int> { 0, 1, 2, 3, 4 };

        for (int i = 0; i < 3; i++)
        {
            int randomPosition = Random.Range(0, indexList.Count);
            int chosenIndex = indexList[randomPosition];

            upgradeButtonsL[chosenIndex].SetActive(true);
            upgradeButtonsR[chosenIndex].SetActive(true);

            indexList.RemoveAt(randomPosition);
        }
    }

    
    public void OnSelectUpgradeButton(string upgradeType)
{
    if (!isUpgrading) return;

    // チュートリアル中は強化だけ適用して画面は閉じない
    if (isTutorialUpgrade)
    {
        Debug.Log($"チュートリアル：{upgradeType} を選択");

        switch (upgradeType)
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
        }

        return;
    }

    // 通常ゲーム
    EndUpgradePhase(upgradeType);
}

    //強化フェーズの終了とゲーム再開
    private void EndUpgradePhase(string choiceResult)
    {
        Debug.Log($"【結果】: {choiceResult} が選ばれました！ゲームを再開します。");

        // ★画面を閉じる際、すべてのボタンのサイズを綺麗に通常状態へ戻す
        ResetAllButtonScales();

        SetUpgradePanelsActive(false);

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

        if (currentWave >= maxWaves)
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

            // コンソールに表示
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

        StartCoroutine(AutoReturnCoroutine());
    }

    private System.Collections.IEnumerator AutoReturnCoroutine()
    {
        //10秒待つ
        yield return new UnityEngine.WaitForSecondsRealtime(10f);

        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("TitleScene");
    }

    // チュートリアル用の強化画面を開いているか
private bool isTutorialUpgrade = false;

/// <summary>
/// チュートリアル用：強化選択画面を開く
/// UpgradePageが表示されている間は閉じない
/// </summary>
public void OpenTutorialUpgrade()
{   

    Debug.Log("OpenTutorialUpgrade が呼ばれました");
    isTutorialUpgrade = true;
    isUpgrading = true;

    // 強化パネルを表示
    SetUpgradePanelsActive(true);

    // 強化候補をランダムに3つ表示
    SelectRandomButtons();

    // 最初の候補を選択状態にする
    SelectFirstActiveButton();

    // ゲーム内の動きだけ止める
    Time.timeScale = 0f;

    Debug.Log("チュートリアル用の強化画面を開きました");
}

/// <summary>
/// チュートリアル用：強化選択画面を閉じる
/// </summary>
public void CloseTutorialUpgrade()
{
    isTutorialUpgrade = false;
    isUpgrading = false;

    ResetAllButtonScales();
    SetUpgradePanelsActive(false);

    Time.timeScale = 1f;

    Debug.Log("チュートリアル用の強化画面を閉じました");
}
}