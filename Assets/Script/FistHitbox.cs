using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class FistHitbox : MonoBehaviour
{
    [Header("ヒットエフェクト")]
    [SerializeField] private GameObject hitEffectPrefab;

    [Header("衝突対象")]
    [SerializeField] private LayerMask enemyLayers = ~0;

    private int damage;
    private bool canHit;

    // 1回のパンチで同じ敵に複数回当たるのを防ぐ
    private readonly HashSet<EnemyHealth> hitEnemies =
        new HashSet<EnemyHealth>();

    private Collider fistCollider;

    private void Awake()
    {
        fistCollider = GetComponent<Collider>();
        fistCollider.isTrigger = true;
        fistCollider.enabled = false;
    }

    public void BeginAttack(int damageValue)
    {
        damage = damageValue;
        canHit = true;
        hitEnemies.Clear();

        fistCollider.enabled = true;
    }

    public void EndAttack()
    {
        canHit = false;
        fistCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        TryHit(other);
    }

    private void OnTriggerStay(Collider other)
    {
        // 攻撃開始時点で既に敵と重なっている場合にも対応
        TryHit(other);
    }

    private void TryHit(Collider other)
    {
        if (!canHit) return;

        // 指定したLayer以外は無視
        if ((enemyLayers.value & (1 << other.gameObject.layer)) == 0)
        {
            return;
        }

        EnemyHealth enemy =
            other.GetComponentInParent<EnemyHealth>();

        if (enemy == null) return;

        // このパンチですでに当てた敵なら無視
        if (!hitEnemies.Add(enemy))
        {
            return;
        }

        enemy.TakeDamage(damage);

        Vector3 hitPosition =
            other.ClosestPoint(transform.position);

        if (hitEffectPrefab != null)
        {
            Instantiate(
                hitEffectPrefab,
                hitPosition,
                Quaternion.identity
            );
        }

        Debug.Log(
            $"<color=orange>パンチ命中：" +
            $"{other.name} / Damage={damage}</color>"
        );
    }
}