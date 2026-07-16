using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;
using TMPro;

public class PlayerController : MonoBehaviour
{
    [Header("攻撃力設定")]
    [SerializeField] private int closeAttackDamage = 100;
    [SerializeField] private int rangeAttackDamage = 100;

    [Header("クリティカル設定")]
    [Range(0f, 100f)]
    [SerializeField] private float criticalChance = 5f; //クリティカル率
    [SerializeField] private float criticalMultiplier = 2f; //クリティカル倍率

    [Header("攻撃の範囲")]
    [SerializeField] private float closeRange = 2.0f;
    [SerializeField] private float rangeAttackDistance = 50.0f; //魔法の飛距離

    [Header("パンチ設定")]
    [SerializeField] private Transform fistModel;
    [SerializeField] private FistHitbox fistHitbox;

    [SerializeField] private float punchDistance = 1.5f;
    [SerializeField] private float punchForwardDuration = 0.08f;
    [SerializeField] private float punchReturnDuration = 0.12f;
    [SerializeField] private float punchCooldown = 0.35f;

    private Vector3 fistDefaultLocalPosition;
    private bool isPunching;
    private float punchCooldownTimer;

    [Header("魔法攻撃の大きさ")]
    [SerializeField] private float rangeAttackRadius = 1.0f;

    [SerializeField] private Transform attackPoint;

    [Header("魔法弾設定")]
    [SerializeField] private GameObject magicProjectilePrefab;
    [SerializeField] private float magicProjectileSpeed = 20f;

    [Header("魔法のクールタイム(秒)")]
    [SerializeField] private float magicCooldown = 3.0f;
    private float magicCooldownTimer = 0f;
    [Header("魔法クールタイム可視化")]
    [Tooltip("CooldownGauge")]
    [SerializeField] private Image cooldownGauge;

    [Tooltip("CooldownText")]
    [SerializeField] private TextMeshProUGUI cooldowntext;

    private Joycon rightJoycon;

    [SerializeField] private float swingThreshold = 2.5f;

    [Header("杖モーション")]
    [SerializeField] private Transform staffModel;

    [SerializeField] private float staffRaiseAngle = -40f;
    [SerializeField] private float staffSwingAngle = 90f;

    [SerializeField] private float staffRaiseTime = 0.1f;
    [SerializeField] private float staffSwingTime = 0.12f;
    [SerializeField] private float staffReturnTime = 0.15f;

    private Quaternion staffDefaultRotation;
    private bool isStaffSwinging;
    [Header("リーチの可視化")]
    [SerializeField] private LineRenderer meleeReachVis;
    [SerializeField] private LineRenderer magicRangeVis;
    private int meleeCircleSegments = 50;

    [Header("攻撃モードUI")]
    [SerializeField] private Image attackModeIcon;
    [SerializeField] private Sprite closeModeIcon;
    [SerializeField] private Sprite rangeModeIcon;

    [Header("武器表示")]
    [SerializeField] private GameObject fistObject;
    [SerializeField] private GameObject staffObject;

    private enum AttackMode
    {
        Close,
        Range
    }

    private AttackMode currentMode = AttackMode.Close;

    void Start()
    {
        InitializeVisualization();
        UpdateAttackModeIcon();
        UpdateWeaponDisplay();

        if (fistModel != null)
        {
            fistDefaultLocalPosition = fistModel.localPosition;
        }

        if (fistHitbox != null)
        {
            fistHitbox.EndAttack();
        }

        if (staffModel != null)
        {
            staffDefaultRotation = staffModel.localRotation;
        }

        ClearCooldownUI();

        if (JoyconManager.Instance == null) return;

        foreach (Joycon j in JoyconManager.Instance.j)
        {
            if (!j.isLeft)
            {
                rightJoycon = j;
                Debug.Log("攻撃用：右Joy-Con取得");
                break;
            }
        }
    }

    void Update()
    {
        if (punchCooldownTimer > 0f)
        {
            punchCooldownTimer -= Time.deltaTime;
        }

        if (magicCooldownTimer > 0f)
        {
            magicCooldownTimer -= Time.deltaTime;

            if (magicCooldownTimer <= 0f)
            {
                magicCooldownTimer = 0f;
                ClearCooldownUI(); // タイマーが0以下になったらUIを消す
            }
            else
            {
                UpdateCooldownUI(); //残り時間に合わせて更新
            }
        }

        updateMeleeCircle();
        updateMagicLine();

        if (rightJoycon != null)
        {
            // Rボタンで近距離モード
            if (rightJoycon.GetButtonDown(Joycon.Button.SHOULDER_1))
            {
                currentMode = AttackMode.Close;
                UpdateAttackModeIcon();
                UpdateWeaponDisplay();
                Debug.Log("近距離モード");
            }

            // ZRボタンで遠距離モード
            if (rightJoycon.GetButtonDown(Joycon.Button.SHOULDER_2))
            {
                currentMode = AttackMode.Range;
                UpdateAttackModeIcon();
                UpdateWeaponDisplay();
                Debug.Log("遠距離モード");
            }
        }

        Keyboard keyboard = Keyboard.current;
        if (keyboard != null || rightJoycon != null)
        {
            //Qキーで遠距離攻撃、Eキーで近距離攻撃(将来的にはジョイコン)
            if (keyboard.qKey.wasPressedThisFrame)
            {

                currentMode = AttackMode.Range;
                UpdateAttackModeIcon();
                UpdateWeaponDisplay();
                Debug.Log("遠距離モード");

                if (magicCooldownTimer <= 0f)
                {
                    PerformRangeAttack();
                }
                else
                {
                    Debug.Log($"魔法攻撃はクールタイム中です。残り時間: {magicCooldownTimer:F1}秒");
                }
            }
            if (keyboard.eKey.wasPressedThisFrame || (currentMode == AttackMode.Close && IsJoyconSwing()))
            {
                currentMode = AttackMode.Close;
                UpdateAttackModeIcon();
                UpdateWeaponDisplay();
                Debug.Log("近距離モード");
                PerformCloseAttack();
            }
        }
    }

    private bool IsJoyconSwing()
    {
        if (rightJoycon == null) return false;

        Vector3 accel = rightJoycon.GetAccel();

        return accel.magnitude >= swingThreshold;
    }

    private void UpdateAttackModeIcon()
    {
        if (attackModeIcon == null) return;

        if (currentMode == AttackMode.Close)
        {
            attackModeIcon.sprite = closeModeIcon;
        }
        else
        {
            attackModeIcon.sprite = rangeModeIcon;
        }
    }

    private void UpdateWeaponDisplay()
    {
        bool isCloseMode = currentMode == AttackMode.Close;

        if (fistObject != null)
        {
            fistObject.SetActive(isCloseMode);
        }

        if (staffObject != null)
        {
            staffObject.SetActive(!isCloseMode);
        }
    }
    //攻撃範囲の可視化
    private void InitializeVisualization()
    {
        if (meleeReachVis != null)
        {
            meleeReachVis.enabled = true;
            meleeReachVis.positionCount = meleeCircleSegments + 1;
            meleeReachVis.useWorldSpace = true;
            meleeReachVis.endWidth = 0.1f;
            meleeReachVis.startWidth = meleeReachVis.endWidth;
        }

        if (magicRangeVis != null)
        {
            magicRangeVis.enabled = true;
            magicRangeVis.positionCount = 2;
            magicRangeVis.useWorldSpace = true;
            magicRangeVis.endWidth = rangeAttackRadius * 2f;
            magicRangeVis.startWidth = magicRangeVis.endWidth;
        }
        updateMeleeCircle();
        updateMagicLine();
    }

    //近距離攻撃の範囲円の描画
    private void updateMeleeCircle()
    {
        if (meleeReachVis == null) return;

        float groundY = transform.position.y + 0.02f;
        CapsuleCollider capsule = GetComponent<CapsuleCollider>();
        if (capsule != null)
        {
            groundY = transform.position.y + capsule.center.y - (capsule.height / 2f) + 0.02f;
        }

        Vector3 center = transform.position + transform.forward;
        center.y = groundY;

        for (int i = 0; i <= meleeCircleSegments; i++)
        {
            float angle = (float)i / meleeCircleSegments * Mathf.PI * 2f;
            float x = Mathf.Cos(angle) * closeRange;
            float z = Mathf.Sin(angle) * closeRange;

            Vector3 pointPosition = new Vector3(center.x + x, center.y, center.z + z);
            meleeReachVis.SetPosition(i, pointPosition);
        }
    }

    //魔法の範囲描画
    private void updateMagicLine()
    {
        if (magicRangeVis == null) return;

        Vector3 origin = transform.position;
        if (attackPoint != null)
        {
            origin = (attackPoint == transform) ? transform.position + transform.forward * 1.5f : attackPoint.position;
        }

        //魔法が届く限界の座標
        Vector3 targetCenter = origin + transform.forward * rangeAttackDistance;
        Vector3 rightDirection = transform.right;
        float totalLineWidth = 20.0f;

        float groundY = transform.position.y + 0.005f;
        CapsuleCollider capsule = GetComponent<CapsuleCollider>();
        if (capsule != null)
        {
            groundY = transform.position.y + capsule.center.y - (capsule.height / 2f) + 0.005f;
        }

        Vector3 lineLeft = targetCenter - rightDirection * totalLineWidth;
        Vector3 lineRight = targetCenter + rightDirection * totalLineWidth;

        lineLeft.y = groundY;
        lineRight.y = groundY;

        magicRangeVis.SetPosition(0, lineLeft);
        magicRangeVis.SetPosition(1, lineRight);

        magicRangeVis.endWidth = 0.1f;
        magicRangeVis.startWidth = magicRangeVis.endWidth;
    }

    private void PerformCloseAttack()
    {
        if (isPunching || punchCooldownTimer > 0f)
        {
            return;
        }

        if (fistModel == null || fistHitbox == null)
        {
            Debug.LogError(
                "Fist ModelまたはFist Hitboxが設定されていません。"
            );
            return;
        }

        Debug.Log("近距離攻撃をしました");

        int finalDamage =
            CalculateDamage(closeAttackDamage);

        punchCooldownTimer = punchCooldown;

        StartCoroutine(
            PunchCoroutine(finalDamage)
        );
    }
    /*private void PerformCloseAttack()
    {
        Debug.Log("近距離攻撃をしました");
        Collider[] hitEnemies = Physics.OverlapSphere(transform.position + transform.forward, closeRange);
        foreach (Collider enemyCollider in hitEnemies)
        {
            EnemyHealth enemy = enemyCollider.GetComponent<EnemyHealth>(); //コライダーに含まれるオブジェクトが敵ならenemyという変数に入れる。
            if (enemy != null)
            {
                int finalDamage = CalculateDamage(closeAttackDamage);
                enemy.TakeDamage(finalDamage);
            }
        }
    }*/

    private void PerformRangeAttack()
    {
        if (isStaffSwinging)
        {
            return;
        }

        if (staffModel == null)
        {
            Debug.LogError("Staff Modelが設定されていません。");
            return;
        }

        magicCooldownTimer = magicCooldown;

        StartCoroutine(StaffSwingCoroutine());
    }

    //拳の動き
    private IEnumerator PunchCoroutine(int damage)
    {
        isPunching = true;

        Vector3 startPosition = fistDefaultLocalPosition;
        Vector3 endPosition =
            startPosition + Vector3.forward * punchDistance;

        fistHitbox.BeginAttack(damage);

        // 拳を前へ出す
        float timer = 0f;

        while (timer < punchForwardDuration)
        {
            timer += Time.deltaTime;

            float t = punchForwardDuration > 0f
                ? Mathf.Clamp01(timer / punchForwardDuration)
                : 1f;

            float easedT = t * t;

            fistModel.localPosition =
                Vector3.Lerp(
                    startPosition,
                    endPosition,
                    easedT
                );

            yield return null;
        }

        fistModel.localPosition = endPosition;

        // 伸び切った状態で少しだけ判定を残す
        yield return new WaitForSeconds(0.05f);

        fistHitbox.EndAttack();

        // 拳を元へ戻す
        timer = 0f;

        while (timer < punchReturnDuration)
        {
            timer += Time.deltaTime;

            float t = punchReturnDuration > 0f
                ? Mathf.Clamp01(timer / punchReturnDuration)
                : 1f;

            float easedT =
                1f - Mathf.Pow(1f - t, 2f);

            fistModel.localPosition =
                Vector3.Lerp(
                    endPosition,
                    startPosition,
                    easedT
                );

            yield return null;
        }

        fistModel.localPosition = startPosition;
        fistHitbox.EndAttack();
        isPunching = false;
    }

    private IEnumerator StaffSwingCoroutine()
    {
        isStaffSwinging = true;

        Quaternion startRot =
            staffDefaultRotation;

        Quaternion raiseRot =
            staffDefaultRotation *
            Quaternion.Euler(staffRaiseAngle, 0, 0);

        Quaternion swingRot =
            staffDefaultRotation *
            Quaternion.Euler(staffSwingAngle, 0, 0);

        // 振り上げ
        float timer = 0f;

        while (timer < staffRaiseTime)
        {
            timer += Time.deltaTime;

            float t = timer / staffRaiseTime;

            staffModel.localRotation =
                Quaternion.Slerp(
                    startRot,
                    raiseRot,
                    t);

            yield return null;
        }

        // 振り下ろし
        timer = 0f;

        while (timer < staffSwingTime)
        {
            timer += Time.deltaTime;

            float t = timer / staffSwingTime;

            staffModel.localRotation =
                Quaternion.Slerp(
                    raiseRot,
                    swingRot,
                    t);

            yield return null;
        }

        // 魔法弾をここで生成すると自然
        SpawnProjectile();

        // 元へ戻る
        timer = 0f;

        while (timer < staffReturnTime)
        {
            timer += Time.deltaTime;

            float t = timer / staffReturnTime;

            staffModel.localRotation =
                Quaternion.Slerp(
                    swingRot,
                    startRot,
                    t);

            yield return null;
        }

        staffModel.localRotation =
            startRot;

        isStaffSwinging = false;
    }
    private void SpawnProjectile()
    {
        if (magicProjectilePrefab == null)
        {
            Debug.LogError("Magic Projectile Prefabが設定されていません。");
            return;
        }

        Vector3 origin;
        Quaternion rotation;

        if (attackPoint != null)
        {
            origin = attackPoint == transform
                ? transform.position + transform.forward * 1.5f
                : attackPoint.position;

            rotation = attackPoint == transform
                ? transform.rotation
                : attackPoint.rotation;
        }
        else
        {
            origin = transform.position + transform.forward * 1.5f;
            rotation = transform.rotation;
        }

        int finalDamage = CalculateDamage(rangeAttackDamage);

        GameObject projectileObject = Instantiate(
            magicProjectilePrefab,
            origin,
            rotation
        );

        MagicProjectile projectile =
            projectileObject.GetComponent<MagicProjectile>();

        if (projectile == null)
        {
            Debug.LogError(
                "魔法弾PrefabのルートにMagicProjectile.csが付いていません。"
            );

            Destroy(projectileObject);
            return;
        }

        projectile.Initialize(
            finalDamage,
            magicProjectileSpeed,
            rangeAttackDistance,
            gameObject
        );
    }


    private int CalculateDamage(int baseDamage)
    {
        if (Random.Range(0f, 100f) <= criticalChance)
        {
            Debug.Log("<color=red>クリティカル</color>");
            return Mathf.RoundToInt(baseDamage * criticalMultiplier);
        }
        return baseDamage;
    }

    //クールタイムのゲージと秒数を連動
    private void UpdateCooldownUI()
    {
        if (cooldownGauge != null)
        {
            cooldownGauge.fillAmount = magicCooldownTimer / magicCooldown;
        }

        if (cooldowntext != null)
        {
            int remainingSeconds = Mathf.CeilToInt(magicCooldownTimer);
            cooldowntext.text = remainingSeconds.ToString();
        }
    }

    //UI表示をリセットする
    private void ClearCooldownUI()
    {
        if (cooldownGauge != null) cooldownGauge.fillAmount = 0f;
        if (cooldowntext != null) cooldowntext.text = ""; 
    }

    private void OnDrawGizmosSelected()
    {
        //近距離攻撃の範囲
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + transform.forward, closeRange);

        //デバック用(遠距離レーザーの太さ確認)
        if (attackPoint != null)
        {
            Gizmos.color = Color.magenta;
            Vector3 startPoint = (attackPoint == transform) ? transform.position + transform.forward * 1.5f : attackPoint.position;
            Gizmos.DrawWireSphere(startPoint, rangeAttackRadius);
        }
    }

    //攻撃力強化
    public void BoostAttack(int amount)
    {
        closeAttackDamage += amount;
        rangeAttackDamage += amount;
        Debug.Log($"プレイヤーの攻撃力が {amount} アップした！ (近接:{closeAttackDamage} / 遠距離:{rangeAttackDamage})");
    }

    //クリティカル確率をアップ
    public void BoostCriticalChance(float amount)
    {
        //100%を超えないようにする
        criticalChance = Mathf.Min(criticalChance + amount, 100f);
        Debug.Log($"プレイヤーのクリティカル率が {amount}% アップした！ (現在:{criticalChance}%)");
    }

    //近距離攻撃のリーチ強化
    public void BoostCloseRange(float amount)
    {
        closeRange += amount;
        Debug.Log($"近距離攻撃のリーチが {amount} アップした！ (現在: {closeRange})");
        updateMeleeCircle();
    }

    //魔法攻撃の射程（飛距離）を強化
    public void BoostRangeAttackDistance(float amount)
    {
        rangeAttackDistance += amount;
        Debug.Log($"魔法攻撃の射程が {amount} 伸びた！ (現在: {rangeAttackDistance})");
        updateMagicLine();
    }

    //魔法攻撃のクールタイムを短縮(強化)
    public void ReduceMagicCooldown(float amount)
    {
        magicCooldown = Mathf.Max(magicCooldown - amount, 0.1f);
        Debug.Log($"魔法攻撃のクールタイムが {amount} 秒短縮された！ (現在: {magicCooldown}秒)");
    }
}
