using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("--- 攻撃力設定 (Attack.txt) ---")]
    [SerializeField] private int closeAttackDamage = 100;
    [SerializeField] private int rangeAttackDamage = 100;

    [Header("--- クリティカル設定 ---")]
    [Range(0f, 100f)]
    [SerializeField] private float criticalChance = 5f;
    [SerializeField] private float criticalMultiplier = 2f;

    [Header("--- 攻撃の判定用設定 ---")]
    [SerializeField] private float closeRange = 2.0f;
    [SerializeField] private float rangeAttackDistance = 50.0f; // 飛距離

    // ★追加：レーザーの太さ（半径）をインスペクターから変えられるようにしました
    [SerializeField] private float rangeAttackRadius = 1.0f;

    [SerializeField] private Transform attackPoint;

    void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.qKey.wasPressedThisFrame) { PerformRangeAttack(); }
            if (keyboard.eKey.wasPressedThisFrame) { PerformCloseAttack(); }
        }
    }

    private void PerformCloseAttack()
    {
        Debug.Log("<color=cyan>【システム】近距離攻撃発動</color>");
        Collider[] hitEnemies = Physics.OverlapSphere(transform.position + transform.forward, closeRange);
        foreach (Collider enemyCollider in hitEnemies)
        {
            EnemyHealth enemy = enemyCollider.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                int finalDamage = CalculateDamage(closeAttackDamage);
                enemy.TakeDamage(finalDamage);
            }
        }
    }

    private void PerformRangeAttack()
    {
        Debug.Log("<color=magenta>【システム】太い遠距離レーザー発動</color>");
        RaycastHit hit;

        Vector3 origin = transform.position;
        if (attackPoint != null)
        {
            // プレイヤー自身がAttackPointなら少し前から出して自爆防止
            origin = (attackPoint == transform) ? transform.position + transform.forward * 1.5f : attackPoint.position;
        }

        // Sceneビューに赤い中心線を引く（1秒間表示）
        Debug.DrawRay(origin, transform.forward * rangeAttackDistance, Color.red, 1.0f);

        // ★Physics.SphereCast（太さのあるレーザー）を飛ばす
        // originから半径rangeAttackRadiusの球を、正面(forward)に飛ばします
        if (Physics.SphereCast(origin, rangeAttackRadius, transform.forward, out hit, rangeAttackDistance))
        {
            Debug.Log($"<color=green>🎯 ヒット！ 対象: {hit.collider.name} / 距離: {hit.distance}m</color>");

            EnemyHealth enemy = hit.collider.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                int finalDamage = CalculateDamage(rangeAttackDamage);
                enemy.TakeDamage(finalDamage);
            }
        }
        else
        {
            Debug.Log($"空振り：太さ {rangeAttackRadius * 2}m の範囲に敵がいませんでした。");
        }
    }

    private int CalculateDamage(int baseDamage)
    {
        if (Random.Range(0f, 100f) <= criticalChance)
        {
            Debug.Log("<color=red>💥 クリティカル！ 💥</color>");
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
}