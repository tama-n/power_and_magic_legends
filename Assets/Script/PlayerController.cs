using UnityEngine;
using UnityEngine.InputSystem;

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

    [Header("魔法攻撃の大きさ")]
    [SerializeField] private float rangeAttackRadius = 1.0f;

    [SerializeField] private Transform attackPoint;

    [Header("魔法のクールタイム(秒)")]
    [SerializeField] private float magicCooldown = 3.0f;
    private float magicCooldownTimer = 0f;

    private Joycon rightJoycon;

    [SerializeField] private float swingThreshold = 2.5f;

    [Header("リーチの可視化")]
    [SerializeField] private LineRenderer meleeReachVis;
    [SerializeField] private LineRenderer magicRangeVis;
    private int meleeCircleSegments = 50;

    private enum AttackMode
    {
        Close,
        Range
    }

    private AttackMode currentMode = AttackMode.Close;

    void Start()
    {
        InitializeVisualization();
    
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
        if (magicCooldownTimer > 0f)
        {
            magicCooldownTimer -= Time.deltaTime;
        }

        updateMeleeCircle();
        updateMagicLine();

        if(rightJoycon != null) {
            // Rボタンで近距離モード
            if (rightJoycon.GetButtonDown(Joycon.Button.SHOULDER_1))
            {
                currentMode = AttackMode.Close;
                Debug.Log("近距離モード");
            }

            // ZRボタンで遠距離モード
            if (rightJoycon.GetButtonDown(Joycon.Button.SHOULDER_2))
            {
                currentMode = AttackMode.Range;
                Debug.Log("遠距離モード");
            }
        }

        Keyboard keyboard = Keyboard.current;
        if (keyboard != null || rightJoycon != null)
        {
            //Qキーで遠距離攻撃、Eキーで近距離攻撃(将来的にはジョイコン)
            if (keyboard.qKey.wasPressedThisFrame) {
                if (magicCooldownTimer <= 0f)
                {
                    PerformRangeAttack();
                }
                else
                {
                    Debug.Log($"魔法攻撃はクールタイム中です。残り時間: {magicCooldownTimer:F1}秒");
                }
            }
            if (keyboard.eKey.wasPressedThisFrame || (currentMode == AttackMode.Close && IsJoyconSwing())) {
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

    //攻撃範囲の可視化
    private void InitializeVisualization()
    {
        if(meleeReachVis != null)
        {
            meleeReachVis.enabled = true; 
            meleeReachVis.positionCount = meleeCircleSegments + 1;
            meleeReachVis.useWorldSpace = true;
            meleeReachVis.endWidth = 0.1f;
            meleeReachVis.startWidth = meleeReachVis.endWidth;
        }

        if(magicRangeVis != null)
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
    }

    private void PerformRangeAttack()
    {
        Debug.Log("魔法攻撃をしました");

        magicCooldownTimer = magicCooldown; //魔法攻撃のクールタイムをリセット

        RaycastHit hit; //魔法に当たったオブジェクトの情報を入れる変数

        Vector3 origin = transform.position; 
        if (attackPoint != null)
        {
            origin = (attackPoint == transform) ? transform.position + transform.forward * 1.5f : attackPoint.position;
        }

        //赤い線を引く(デバック用)
        Debug.DrawRay(origin, transform.forward * rangeAttackDistance, Color.red, 1.0f);

        //originから半径rangeAttackRadiusの球を、正面に飛ばします
        if (Physics.SphereCast(origin, rangeAttackRadius, transform.forward, out hit, rangeAttackDistance))
        {
            Debug.Log($"<color=green>魔法ヒット。 対象: {hit.collider.name} / 距離: {hit.distance}m</color>");

            EnemyHealth enemy = hit.collider.GetComponent<EnemyHealth>(); //コライダー(オブジェクト)が敵(EnemyHealthスクリプトを持ってる)ならenemyという変数に入れる。
            if (enemy != null)
            {
                int finalDamage = CalculateDamage(rangeAttackDamage);
                enemy.TakeDamage(finalDamage);
            }
        }
        else
        {
            Debug.Log($"魔法は当たっていません");
        }
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
