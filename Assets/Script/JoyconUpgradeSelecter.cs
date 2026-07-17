using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class JoyconUpgradeSelector : MonoBehaviour
{
    [Header("強化ボタン")]
    [Tooltip("操作対象になるL側の強化ボタンを、強化内容の順番をそろえて登録（全5個）")]
    [SerializeField] private GameObject[] upgradeButtonsL;

    [Tooltip("表示同期用のR側の強化ボタンを、L側と同じ順番で登録（全5個）")]
    [SerializeField] private GameObject[] upgradeButtonsR;

    [Header("連続入力防止")]
    [SerializeField] private float inputCooldown = 0.2f;

    [Header("選択中の大きさ設定")]
    [Tooltip("選択されているボタンの大きさ（倍率）")]
    [SerializeField] private float selectedScale = 1.2f;
    [Tooltip("選択されていないボタンの大きさ（基本は1.0）")]
    [SerializeField] private float normalScale = 1.0f;

    private List<Joycon> joycons;
    private Joycon rightJoycon;

    private float lastInputTime = -10f;
    private int selectedIndex = 0; // 現在画面に出ているアクティブなボタンリストの中の何番目か(0～2)
    private bool wasUpgrading = false;
    private bool isInitializedForCurrentPhase = false;

    void Start()
    {
        FindRightJoycon();
    }

    void Update()
    {
        if (WaveManager.Instance == null) return;

        bool isUpgrading = WaveManager.Instance.IsUpgrading;

        // 強化画面が開いた瞬間、または開いているのに初期化されていない場合
        if (isUpgrading && (!wasUpgrading || !isInitializedForCurrentPhase))
        {
            if (rightJoycon == null) FindRightJoycon();

            // 選択インデックスを「0（一番左）」にリセット
            selectedIndex = 0;
            InitializeSelection();

            if (GetActiveButtonsL().Count > 0)
            {
                isInitializedForCurrentPhase = true;
            }
        }

        // 強化画面が閉じた瞬間のクリーンアップ（すべて等倍に戻す）
        if (!isUpgrading && wasUpgrading)
        {
            ResetAllButtonScales();
            isInitializedForCurrentPhase = false;
        }

        wasUpgrading = isUpgrading;

        if (!isUpgrading) return;

        // 連続入力の防止
        if (Time.unscaledTime - lastInputTime < inputCooldown) return;

        HandleButtonInput();
    }

    private void FindRightJoycon()
    {
        if (JoyconManager.Instance == null || JoyconManager.Instance.j == null) return;

        joycons = JoyconManager.Instance.j;
        foreach (Joycon joycon in joycons)
        {
            if (joycon != null && !joycon.isLeft)
            {
                rightJoycon = joycon;
                Debug.Log("[Joy-Con] 右Joy-Conを正常に認識しました");
                break;
            }
        }
    }

    private void HandleButtonInput()
    {
        Keyboard keyboard = Keyboard.current;

        // --- キーボード操作 ---
        if (keyboard != null)
        {
            if (keyboard.aKey.wasPressedThisFrame)
            {
                MoveSelection(-1);
                lastInputTime = Time.unscaledTime;
                return;
            }
            if (keyboard.dKey.wasPressedThisFrame)
            {
                MoveSelection(1);
                lastInputTime = Time.unscaledTime;
                return;
            }
            if (keyboard.fKey.wasPressedThisFrame)
            {
                SubmitSelectedButton();
                lastInputTime = Time.unscaledTime;
                return;
            }
        }

        if (rightJoycon == null) return;

        // --- 右Joy-Con操作 (SL/SRで移動、DPAD_DOWN/下ボタンで決定) ---
        if (rightJoycon.GetButtonDown(Joycon.Button.SL))
        {
            MoveSelection(-1);
            lastInputTime = Time.unscaledTime;
        }
        else if (rightJoycon.GetButtonDown(Joycon.Button.SR))
        {
            MoveSelection(1);
            lastInputTime = Time.unscaledTime;
        }
        else if (rightJoycon.GetButtonDown(Joycon.Button.DPAD_DOWN))
        {
            SubmitSelectedButton();
            lastInputTime = Time.unscaledTime;
        }
    }

    private void InitializeSelection()
    {
        List<GameObject> activeButtonsL = GetActiveButtonsL();
        if (activeButtonsL.Count == 0) return;

        selectedIndex = 0;
        UpdateButtonScales(activeButtonsL);
    }

    private void MoveSelection(int direction)
    {
        List<GameObject> activeButtonsL = GetActiveButtonsL();
        if (activeButtonsL.Count == 0) return;

        selectedIndex += direction;

        if (selectedIndex < 0) selectedIndex = activeButtonsL.Count - 1;
        else if (selectedIndex >= activeButtonsL.Count) selectedIndex = 0;

        UpdateButtonScales(activeButtonsL);
    }

    private void SubmitSelectedButton()
    {
        List<GameObject> activeButtonsL = GetActiveButtonsL();
        if (activeButtonsL.Count == 0) return;

        selectedIndex = Mathf.Clamp(selectedIndex, 0, activeButtonsL.Count - 1);
        GameObject selectedObject = activeButtonsL[selectedIndex];
        Button selectedButton = selectedObject.GetComponent<Button>();

        if (selectedButton != null && selectedButton.interactable)
        {
            selectedButton.onClick.Invoke();
        }
    }

    // 現在画面に表示されている（Activeな）L側のボタンのみを抽出
    private List<GameObject> GetActiveButtonsL()
    {
        List<GameObject> activeButtons = new List<GameObject>();
        if (upgradeButtonsL == null) return activeButtons;

        foreach (GameObject button in upgradeButtonsL)
        {
            if (button != null && button.activeSelf)
            {
                activeButtons.Add(button);
            }
        }
        return activeButtons;
    }

    // ボタンの大きさを更新する処理
    private void UpdateButtonScales(List<GameObject> activeButtonsL)
    {
        // 1. まずすべてのボタンを通常の大きさにリセット
        ResetAllButtonScales();

        // 2. 現在アクティブなボタンの中から、選択されているものだけを大きくする
        for (int activeIndex = 0; activeIndex < activeButtonsL.Count; activeIndex++)
        {
            GameObject leftObject = activeButtonsL[activeIndex];

            int originalIndex = System.Array.IndexOf(upgradeButtonsL, leftObject);
            if (originalIndex < 0) continue;

            // 選択中のインデックスなら大きく、それ以外は標準サイズ
            float targetScale = (activeIndex == selectedIndex) ? selectedScale : normalScale;

            // L側のサイズを変更
            SetButtonScale(upgradeButtonsL[originalIndex], targetScale);

            // R側のサイズを同期変更
            if (upgradeButtonsR != null && originalIndex < upgradeButtonsR.Length)
            {
                SetButtonScale(upgradeButtonsR[originalIndex], targetScale);
            }
        }
    }

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

    private void SetButtonScale(GameObject buttonObject, float scaleAmount)
    {
        if (buttonObject == null) return;

        // RectTransformを介してボタンのLocalScaleを変更する
        RectTransform rectTransform = buttonObject.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.localScale = new Vector3(scaleAmount, scaleAmount, 1f);
        }
    }
}