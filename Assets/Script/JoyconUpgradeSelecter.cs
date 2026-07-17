using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;



public class JoyconUpgradeSelector : MonoBehaviour
{
    [Header("強化ボタン")]
    [SerializeField] private GameObject[] upgradeButtons;

    [Header("連続入力防止")]
    [SerializeField] private float inputCooldown = 0.2f;

    private List<Joycon> joycons;
    private Joycon rightJoycon;

    private float lastInputTime = -10f;

    [Header("選択中の見た目")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.yellow;

    private int selectedIndex = 0;

    private bool wasUpgrading = false;

    void Start()
    {
        if (JoyconManager.Instance == null)
        {
            Debug.LogWarning("JoyconManagerがシーンにありません。キーボードのみ使用します。");
            return;
        }

        List<Joycon> joycons = JoyconManager.Instance.j;

        if (joycons == null)
        {
            Debug.LogWarning("Joy-Con一覧を取得できませんでした。");
            return;
        }

        foreach (Joycon joycon in joycons)
        {
            if (joycon != null && !joycon.isLeft)
            {
                rightJoycon = joycon;
                Debug.Log("右Joy-Conを取得しました");
                break;
            }
        }

        if (rightJoycon == null)
        {
            Debug.LogWarning("右Joy-Conが見つかりません。キーボードのみ使用します。");
        }
    }

    void Update()
    {
        if (WaveManager.Instance == null)
        {
            return;
        }

        bool isUpgrading = WaveManager.Instance.IsUpgrading;

        // 強化画面が開いた瞬間
        if (isUpgrading && !wasUpgrading)
        {
            InitializeSelection();
        }

        wasUpgrading = isUpgrading;

        if (!isUpgrading)
        {
            return;
        }

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

        if (Time.unscaledTime - lastInputTime < inputCooldown)
        {
            return;
        }

        HandleButtonInput();
    }

    private void HandleButtonInput()
    {
        Keyboard keyboard = Keyboard.current;

        // キーボード操作
        if (keyboard != null)
        {
            if (keyboard.aKey.wasPressedThisFrame)
            {
                Debug.Log("Aキー：前へ");
                MoveSelection(-1);
                lastInputTime = Time.unscaledTime;
                return;
            }

            if (keyboard.dKey.wasPressedThisFrame)
            {
                Debug.Log("Dキー：次へ");
                MoveSelection(1);
                lastInputTime = Time.unscaledTime;
                return;
            }

            if (keyboard.fKey.wasPressedThisFrame)
            {
                Debug.Log("Fキー：決定");
                SubmitSelectedButton();
                lastInputTime = Time.unscaledTime;
                return;
            }
        }

        // Joy-Conがなければ、Joy-Con部分だけ実行しない
        if (rightJoycon == null)
        {
            return;
        }

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

    private void MoveSelection(int direction)
    {
        List<GameObject> activeButtons = GetActiveButtons();

        if (activeButtons.Count == 0)
        {
            Debug.LogWarning("表示中の強化ボタンがありません");
            return;
        }

        GameObject currentSelected = null;

        if (EventSystem.current != null)
        {
            currentSelected = EventSystem.current.currentSelectedGameObject;
        }

        int currentIndex = activeButtons.IndexOf(currentSelected);

        // 現在の選択が見つからなければ最初を選択
        if (currentIndex < 0)
        {
            selectedIndex = 0;
        }
        else
        {
            selectedIndex = currentIndex + direction;

            if (selectedIndex < 0)
            {
                selectedIndex = activeButtons.Count - 1;
            }
            else if (selectedIndex >= activeButtons.Count)
            {
                selectedIndex = 0;
            }
        }

        GameObject selectedObject = activeButtons[selectedIndex];

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(selectedObject);
        }

        Button selectedButton = selectedObject.GetComponent<Button>();

        if (selectedButton != null)
        {
            selectedButton.Select();
        }

        UpdateButtonColors(activeButtons, selectedObject);

        Debug.Log($"選択中: {selectedObject.name}");
    }

    private void UpdateButtonColors(
    List<GameObject> activeButtons,
    GameObject selectedObject)
    {
        foreach (GameObject buttonObject in activeButtons)
        {
            Image image = buttonObject.GetComponent<Image>();

            if (image == null)
            {
                continue;
            }

            if (buttonObject == selectedObject)
            {
                image.color = selectedColor;
            }
            else
            {
                image.color = normalColor;
            }
        }
    }

    private void SubmitSelectedButton()
    {
        List<GameObject> activeButtons = GetActiveButtons();

        if (activeButtons.Count == 0)
        {
            Debug.LogWarning("表示中の強化ボタンがありません");
            return;
        }

        GameObject selectedObject = null;

        if (EventSystem.current != null)
        {
            selectedObject =
                EventSystem.current.currentSelectedGameObject;
        }

        // EventSystemから取れなければselectedIndexを使う
        if (selectedObject == null ||
            !activeButtons.Contains(selectedObject))
        {
            selectedIndex =
                Mathf.Clamp(selectedIndex, 0, activeButtons.Count - 1);

            selectedObject = activeButtons[selectedIndex];
        }

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

        // OnClickを直接実行
        selectedButton.onClick.Invoke();
    }

    private void InitializeSelection()
    {
        List<GameObject> activeButtons = GetActiveButtons();

        if (activeButtons.Count == 0)
        {
            return;
        }

        selectedIndex = 0;

        GameObject firstButton = activeButtons[0];

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstButton);
        }

        Button button = firstButton.GetComponent<Button>();

        if (button != null)
        {
            button.Select();
        }

        UpdateButtonColors(activeButtons, firstButton);

        Debug.Log($"最初の選択: {firstButton.name}");
    }

    private List<GameObject> GetActiveButtons()
    {
        List<GameObject> activeButtons = new List<GameObject>();

        if (WaveManager.Instance == null)
        {
            return activeButtons;
        }

        GameObject[] buttons = WaveManager.Instance.GetUpgradeButtons();

        if (buttons == null)
        {
            return activeButtons;
        }

        foreach (GameObject button in buttons)
        {
            if (button != null && button.activeSelf)
            {
                activeButtons.Add(button);
            }
        }

        Debug.Log($"選択対象ボタン数: {activeButtons.Count}");

        return activeButtons;
    }

}