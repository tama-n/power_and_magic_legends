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

    [Header("攻撃の判定用設定")]
    [SerializeField] private float closeRange = 2.0f;
    [SerializeField] private float rangeAttackDistance = 50.0f; //魔法の飛距離

    [Header("魔法攻撃の大きさ")]
    [SerializeField] private float rangeAttackRadius = 1.0f;

    [SerializeField] private Transform attackPoint;

    [Header("魔法のクールタイム(秒)")]
    [SerializeField] private float magicCooldown = 3.0f;
    private float magicCooldownTimer = 0f;

    void Update()
    {
        if (magicCooldownTimer > 0f)
        {
            magicCooldownTimer -= Time.deltaTime;
        }

        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            // Qキーで遠距離攻撃、Eキーで近距離攻撃(将来的にはジョイコン)
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
            if (keyboard.eKey.wasPressedThisFrame) {
                PerformCloseAttack(); 
            }
        }
    }

    private void PerformCloseAttack()
    {
        Debug.Log("<color=cyan>近距離攻撃をしました</color>");
        Collider[] hitEnemies = Physics.OverlapSphere(transform.position + transform.forward, closeRange);
        foreach (Collider enemyCollider in hitEnemies)
        {
            EnemyHealth enemy = enemyCollider.GetComponent<EnemyHealth>(); //コライダー(オブジェクト)が敵ならenemyという変数に入れる。
            if (enemy != null)
            {
                int finalDamage = CalculateDamage(closeAttackDamage);
                enemy.TakeDamage(finalDamage);
            }
        }
    }

    private void PerformRangeAttack()
    {
        Debug.Log("<color=magenta>魔法攻撃をしました</color>");

        magicCooldownTimer = magicCooldown; //魔法攻撃のクールタイムをリセット

        RaycastHit hit; //魔法に当たったオブジェクトの情報を入れる変数

        Vector3 origin = transform.position; //魔法の発射位置(初期値はプレイヤーの位置)
        if (attackPoint != null)
        {
            // プレイヤー自身がAttackPointなら少し前から出して自爆防止
            origin = (attackPoint == transform) ? transform.position + transform.forward * 1.5f : attackPoint.position;
        }

        //赤い線を引く(デバック用)
        Debug.DrawRay(origin, transform.forward * rangeAttackDistance, Color.red, 1.0f);

        // originから半径rangeAttackRadiusの球を、正面に飛ばします
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
        // 近距離攻撃の範囲
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + transform.forward, closeRange);

        // ★遠距離レーザーの「太さ」をScene上で確認するためのガイドライン
        if (attackPoint != null)
        {
            Gizmos.color = Color.magenta;
            Vector3 startPoint = (attackPoint == transform) ? transform.position + transform.forward * 1.5f : attackPoint.position;
            // 球体判定のスタート地点をワイヤーフレームで表示
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

    // クリティカル確率をアップさせる窓口
    public void BoostCriticalChance(float amount)
    {
        // 100%を超えないように Mathf.Min で制限をかける
        criticalChance = Mathf.Min(criticalChance + amount, 100f);
        Debug.Log($"プレイヤーのクリティカル率が {amount}% アップした！ (現在:{criticalChance}%)");
    }

    // 近距離攻撃のリーチ強化
    public void BoostCloseRange(float amount)
    {
        closeRange += amount;
        Debug.Log($"近距離攻撃のリーチが {amount} アップした！ (現在: {closeRange})");
    }

    // 魔法攻撃の射程（飛距離）を強化
    public void BoostRangeAttackDistance(float amount)
    {
        rangeAttackDistance += amount;
        Debug.Log($"魔法攻撃の射程が {amount} 伸びた！ (現在: {rangeAttackDistance})");
    }

    // 魔法攻撃のクールタイムを短縮(強化)
    public void ReduceMagicCooldown(float amount)
    {
        magicCooldown = Mathf.Max(magicCooldown - amount, 0.1f); 
        Debug.Log($"魔法攻撃のクールタイムが {amount} 秒短縮された！ (現在: {magicCooldown}秒)");
    }
}