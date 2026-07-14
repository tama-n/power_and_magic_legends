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

    private Rigidbody rb;
    private Collider projectileCollider;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        projectileCollider = GetComponent<Collider>();

        rb.useGravity = false;
        rb.isKinematic = true;
        rb.collisionDetectionMode =
            CollisionDetectionMode.ContinuousSpeculative;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        projectileCollider.isTrigger = true;
    }

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

        startPosition = rb.position;
        hasHit = false;
        isInitialized = true;

        IgnoreOwnerCollisions();
    }

    private void IgnoreOwnerCollisions()
    {
        if (owner == null) return;

        Transform ownerRoot = owner.transform.root;

        Collider[] ownerColliders =
            ownerRoot.GetComponentsInChildren<Collider>();

        foreach (Collider ownerCollider in ownerColliders)
        {
            if (ownerCollider != null)
            {
                Physics.IgnoreCollision(
                    projectileCollider,
                    ownerCollider,
                    true
                );
            }
        }
    }

    private void FixedUpdate()
    {
        if (!isInitialized || hasHit) return;

        Vector3 movement =
            transform.forward * speed * Time.fixedDeltaTime;

        rb.MovePosition(rb.position + movement);

        float traveledDistance =
            Vector3.Distance(startPosition, rb.position);

        if (traveledDistance >= maxDistance)
        {
            Debug.Log("魔法弾が最大飛距離に到達");
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isInitialized || hasHit) return;

        // 魔法弾自身を無視
        if (other.transform == transform ||
            other.transform.IsChildOf(transform))
        {
            return;
        }

        // プレイヤー自身を無視
        if (owner != null)
        {
            Transform ownerRoot = owner.transform.root;

            if (other.transform == ownerRoot ||
                other.transform.IsChildOf(ownerRoot))
            {
                return;
            }
        }

        Debug.Log(
            $"魔法弾が接触: {other.name} / " +
            $"Tag: {other.tag} / Layer: {other.gameObject.layer}"
        );

        EnemyHealth enemy =
            other.GetComponentInParent<EnemyHealth>();

        // 敵以外は通過
        if (enemy == null)
        {
            return;
        }

        hasHit = true;

        enemy.TakeDamage(damage);

        if (hitEffectPrefab != null)
        {
            Vector3 hitPosition =
                other.ClosestPoint(rb.position);

            Instantiate(
                hitEffectPrefab,
                hitPosition,
                Quaternion.identity
            );
        }

        Destroy(gameObject);
    }
}