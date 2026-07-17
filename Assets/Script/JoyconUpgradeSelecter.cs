using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class JoyconUpgradeSelector : MonoBehaviour
{
    [Header("強化ボタン")]
    [Tooltip("操作対象になるL側の強化ボタンを、強化内容の順番をそろえて登録")]
    [SerializeField] private GameObject[] upgradeButtonsL;

    [Tooltip("表示同期用のR側の強化ボタンを、L側と同じ順番で登録")]
    [SerializeField] private GameObject[] upgradeButtonsR;

    [Header("連続入力防止")]
    [SerializeField] private float inputCooldown = 0.2f;

    [Header("選択中の見た目")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.yellow;

    private List<Joycon> joycons;
    private Joycon rightJoycon;

    private float lastInputTime = -10f;
    private int selectedIndex = 0;
    private bool wasUpgrading = false;

    void Start()
    {
        FindRightJoycon();
    }

    void Update()
    {
        if (WaveManager.Instance == null)
        {
            return;
        }

        bool isUpgrading = WaveManager.Instance.IsUpgrading;

        // 強化画面が開いた瞬間だけ初期選択
        if (isUpgrading && !wasUpgrading)
        {
            InitializeSelection();
        }

        // 強化画面が閉じた瞬間に色を戻す
        if (!isUpgrading && wasUpgrading)
        {
            ResetAllButtonColors();
        }

        wasUpgrading = isUpgrading;

        if (!isUpgrading)
        {
            return;
        }

        // Joy-Conのボタン名を確認するためのデバッグ
        if (rightJoycon != null)
        {
            foreach (Joycon.Button button in
                     System.Enum.GetValues(typeof(Joycon.Button)))
            {
                if (rightJoycon.GetButtonDown(button))
                {
                    Debug.Log($"押されたJoy-Conボタン: {button}");
                }
            }
        }

        // 強化画面中はTime.timeScaleが0なのでunscaledTimeを使う
        if (Time.unscaledTime - lastInputTime < inputCooldown)
        {
            return;
        }

        HandleButtonInput();
    }

    private void FindRightJoycon()
    {
        if (JoyconManager.Instance == null)
        {
            Debug.LogWarning(
                "JoyconManagerがシーンにありません。キーボードのみ使用します。"
            );
            return;
        }

        joycons = JoyconManager.Instance.j;

        if (joycons == null)
        {
            Debug.LogWarning(
                "Joy-Con一覧を取得できませんでした。キーボードのみ使用します。"
            );
            return;
        }

        foreach (Joycon joycon in joycons)
        {
            if (joycon != null && !joycon.isLeft)
            {
                rightJoycon = joycon;
                Debug.Log("強化選択用の右Joy-Conを取得しました");
                break;
            }
        }

        if (rightJoycon == null)
        {
            Debug.LogWarning(
                "右Joy-Conが見つかりません。キーボードのみ使用します。"
            );
        }
    }

    private void HandleButtonInput()
    {
        Keyboard keyboard = Keyboard.current;

        // =========================
        // キーボード操作
        // =========================
        if (keyboard != null)
        {
            // A：前の強化へ
            if (keyboard.aKey.wasPressedThisFrame)
            {
                Debug.Log("Aキー：前へ");

                MoveSelection(-1);

                lastInputTime = Time.unscaledTime;
                return;
            }

            // D：次の強化へ
            if (keyboard.dKey.wasPressedThisFrame)
            {
                Debug.Log("Dキー：次へ");

                MoveSelection(1);

                lastInputTime = Time.unscaledTime;
                return;
            }

            // F：決定
            if (keyboard.fKey.wasPressedThisFrame)
            {
                Debug.Log("Fキー：決定");

                SubmitSelectedButton();

                lastInputTime = Time.unscaledTime;
                return;
            }
        }

        // Joy-Conがない場合、キーボードだけで終了
        if (rightJoycon == null)
        {
            return;
        }

        // =========================
        // 右Joy-Con操作
        // =========================

        // SL：前へ
        if (rightJoycon.GetButtonDown(Joycon.Button.SL))
        {
            MoveSelection(-1);

            lastInputTime = Time.unscaledTime;
        }
        // SR：次へ
        else if (rightJoycon.GetButtonDown(Joycon.Button.SR))
        {
            MoveSelection(1);

            lastInputTime = Time.unscaledTime;
        }
        // 決定ボタン
        else if (rightJoycon.GetButtonDown(Joycon.Button.DPAD_DOWN))
        {
            SubmitSelectedButton();

            lastInputTime = Time.unscaledTime;
        }
    }

    private void InitializeSelection()
    {
        List<GameObject> activeButtonsL = GetActiveButtonsL();

        if (activeButtonsL.Count == 0)
        {
            Debug.LogWarning("初期選択できる強化ボタンがありません");
            return;
        }

        selectedIndex = 0;

        GameObject firstButton = activeButtonsL[selectedIndex];

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstButton);
        }

        UpdateButtonColors(activeButtonsL);

        Debug.Log(
            $"最初の選択: {firstButton.name} " +
            $"選択位置: 1/{activeButtonsL.Count}"
        );
    }

    private void MoveSelection(int direction)
    {
        List<GameObject> activeButtonsL = GetActiveButtonsL();

        if (activeButtonsL.Count == 0)
        {
            Debug.LogWarning("表示中の強化ボタンがありません");
            return;
        }

        selectedIndex += direction;

        // 一番前からさらに前へ行ったら、一番後ろへ
        if (selectedIndex < 0)
        {
            selectedIndex = activeButtonsL.Count - 1;
        }
        // 一番後ろからさらに次へ行ったら、一番前へ
        else if (selectedIndex >= activeButtonsL.Count)
        {
            selectedIndex = 0;
        }

        GameObject selectedObject = activeButtonsL[selectedIndex];

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(selectedObject);
        }

        UpdateButtonColors(activeButtonsL);

        Debug.Log(
            $"選択位置: {selectedIndex + 1}/{activeButtonsL.Count} " +
            $"選択中: {selectedObject.name}"
        );
    }

    private void SubmitSelectedButton()
    {
        List<GameObject> activeButtonsL = GetActiveButtonsL();

        if (activeButtonsL.Count == 0)
        {
            Debug.LogWarning("表示中の強化ボタンがありません");
            return;
        }

        selectedIndex =
            Mathf.Clamp(selectedIndex, 0, activeButtonsL.Count - 1);

        GameObject selectedObject = activeButtonsL[selectedIndex];

        Button selectedButton =
            selectedObject.GetComponent<Button>();

        if (selectedButton == null)
        {
            Debug.LogError(
                $"{selectedObject.name} にButtonコンポーネントがありません"
            );
            return;
        }

        if (!selectedButton.interactable)
        {
            Debug.LogWarning(
                $"{selectedObject.name} は操作できない状態です"
            );
            return;
        }

        Debug.Log($"決定: {selectedObject.name}");

        // L側のButtonに設定されたOnClickだけを実行
        selectedButton.onClick.Invoke();
    }

    private List<GameObject> GetActiveButtonsL()
    {
        List<GameObject> activeButtons =
            new List<GameObject>();

        if (upgradeButtonsL == null)
        {
            return activeButtons;
        }

        foreach (GameObject button in upgradeButtonsL)
        {
            if (button != null && button.activeInHierarchy)
            {
                activeButtons.Add(button);
            }
        }

        return activeButtons;
    }

    private void UpdateButtonColors(
        List<GameObject> activeButtonsL)
    {
        // 先に左右すべて通常色へ戻す
        ResetAllButtonColors();

        for (int activeIndex = 0;
             activeIndex < activeButtonsL.Count;
             activeIndex++)
        {
            GameObject leftObject =
                activeButtonsL[activeIndex];

            // L側全体配列の何番目かを取得
            int originalIndex =
                System.Array.IndexOf(
                    upgradeButtonsL,
                    leftObject
                );

            if (originalIndex < 0)
            {
                continue;
            }

            Color targetColor =
                activeIndex == selectedIndex
                    ? selectedColor
                    : normalColor;

            // L側の色
            SetButtonColor(
                upgradeButtonsL[originalIndex],
                targetColor
            );

            // 同じ強化内容のR側の色
            if (upgradeButtonsR != null &&
                originalIndex < upgradeButtonsR.Length)
            {
                SetButtonColor(
                    upgradeButtonsR[originalIndex],
                    targetColor
                );
            }
        }
    }

    private void ResetAllButtonColors()
    {
        if (upgradeButtonsL != null)
        {
            foreach (GameObject button in upgradeButtonsL)
            {
                SetButtonColor(button, normalColor);
            }
        }

        if (upgradeButtonsR != null)
        {
            foreach (GameObject button in upgradeButtonsR)
            {
                SetButtonColor(button, normalColor);
            }
        }
    }

    private void SetButtonColor(
        GameObject buttonObject,
        Color color)
    {
        if (buttonObject == null)
        {
            return;
        }

        Button button =
            buttonObject.GetComponent<Button>();

        // ButtonのTarget Graphicを優先
        if (button != null &&
            button.targetGraphic != null)
        {
            button.targetGraphic.color = color;
            return;
        }

        // Target Graphicがない場合はImageを直接変更
        Image image =
            buttonObject.GetComponent<Image>();

        if (image != null)
        {
            image.color = color;
        }
    }
}