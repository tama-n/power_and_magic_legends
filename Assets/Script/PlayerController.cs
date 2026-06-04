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
    [SerializeField] private float closeRange = 2.0f;     // 近距離攻撃の届く距離
    [SerializeField] private float rangeAttackDistance = 50.0f; // ★追加：遠距離攻撃のリーチ（飛距離）

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

    // 近距離攻撃 (E)
    private void PerformCloseAttack()
    {
        Debug.Log("<color=cyan>【システム】近距離攻撃ボタンが押されました</color>");
        Collider[] hitEnemies = Physics.OverlapSphere(transform.position + transform.forward, closeRange);

        if (hitEnemies.Length == 0)
        {
            Debug.Log("近距離攻撃：範囲内にオブジェクトがありません（空振り）");
        }

        foreach (Collider enemyCollider in hitEnemies)
        {
            Debug.Log($"近距離攻撃が接触しました: {enemyCollider.name}");

            EnemyHealth enemy = enemyCollider.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                int finalDamage = CalculateDamage(closeAttackDamage);
                enemy.TakeDamage(finalDamage);
            }
        }
    }

    // 遠距離攻撃 (Q)
    private void PerformRangeAttack()
    {
        Debug.Log("<color=magenta>【システム】遠距離攻撃ボタンが押されました</color>");
        RaycastHit hit;

        Vector3 origin = transform.position;
        if (attackPoint != null)
        {
            if (attackPoint == transform)
            {
                origin = transform.position + transform.forward * 1.5f;
            }
            else
            {
                origin = attackPoint.position;
            }
        }

        // ★新機能：Unityの画面（Sceneビュー）に、攻撃レーザーの軌跡を1秒間「赤い線」で描画します
        Debug.DrawRay(origin, transform.forward * rangeAttackDistance, Color.red, 1.0f);

        // レーザーを飛ばす（飛距離をインスペクターの変数に連動）
        if (Physics.Raycast(origin, transform.forward, out hit, rangeAttackDistance))
        {
            // ★重要：何にレーザーがぶつかったかを名前でログに出す
            Debug.Log($"<color=green>遠距離攻撃がヒットしました！ 衝突対象: {hit.collider.name}</color>");

            EnemyHealth enemy = hit.collider.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                int finalDamage = CalculateDamage(rangeAttackDamage);
                enemy.TakeDamage(finalDamage);
            }
            else
            {
                Debug.Log($"衝突した {hit.collider.name} には EnemyHealth がついていません。");
            }
        }
        else
        {
            // 何にも当たらなかった場合
            Debug.Log($"遠距離攻撃：距離 {rangeAttackDistance}m 以内に何も検知しませんでした（空振り）");
        }
    }

    private int CalculateDamage(int baseDamage)
    {
        if (Random.Range(0f, 100f) <= criticalChance)
        {
            Debug.Log("<color=red>💥 クリティカルヒット！ 💥</color>");
            return Mathf.RoundToInt(baseDamage * criticalMultiplier);
        }
        return baseDamage;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + transform.forward, closeRange);
    }
}