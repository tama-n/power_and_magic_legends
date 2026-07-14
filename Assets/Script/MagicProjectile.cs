using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class MagicProjectile : MonoBehaviour
{
    [Header("着弾エフェクト")]
    [SerializeField] private GameObject hitEffectPrefab;

    private int damage;
    private float speed;
    private float maxDistance;

    private Vector3 startPosition;
    private GameObject owner;

    private bool hasHit;
    private bool isInitialized;

    public void Initialize(
        int damageValue,
        float speedValue,
        float maxDistanceValue,
        GameObject ownerObject)
    {
        damage = damageValue;
        speed = speedValue;
        maxDistance = maxDistanceValue;
        owner = ownerObject;

        startPosition = transform.position;
        hasHit = false;
        isInitialized = true;

        IgnoreOwnerCollisions();
    }

    private void IgnoreOwnerCollisions()
    {
        if (owner == null) return;

        Collider projectileCollider = GetComponent<Collider>();

        // PlayerControllerが子に付いていても、プレイヤー全体を取得
        Transform ownerRoot = owner.transform.root;

        Collider[] ownerColliders =
            ownerRoot.GetComponentsInChildren<Collider>();

        foreach (Collider ownerCollider in ownerColliders)
        {
            Physics.IgnoreCollision(
                projectileCollider,
                ownerCollider,
                true
            );
        }
    }

    private void Update()
    {
        if (!isInitialized || hasHit) return;

        transform.position +=
            transform.forward * speed * Time.deltaTime;

        float traveledDistance =
            Vector3.Distance(startPosition, transform.position);

        if (traveledDistance >= maxDistance)
        {
            Debug.Log("魔法弾が最大飛距離に到達");
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isInitialized || hasHit) return;

        Debug.Log(
            $"魔法弾が接触: {other.name} / Tag: {other.tag}"
        );

        hasHit = true;

        EnemyHealth enemy =
            other.GetComponentInParent<EnemyHealth>();

        if (enemy != null)
        {
            enemy.TakeDamage(damage);
        }

        if (hitEffectPrefab != null)
        {
            Vector3 hitPosition =
                other.ClosestPoint(transform.position);

            Instantiate(
                hitEffectPrefab,
                hitPosition,
                Quaternion.identity
            );
        }

        Destroy(gameObject);
    }
}