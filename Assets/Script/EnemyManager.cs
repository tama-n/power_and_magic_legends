using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [Header("敵のプレハブ")]
    public GameObject enemyPrefab;
    public GameObject middleBossPrefab;
    public GameObject lastBossPrefab;

    [Header("地面の高さ")]
    public float groundY = 0f;

    [Header("プールの設定")]
    public int poolSize = 30;         //用意する敵の数
    public float spawnInterval = 1.5f; //出現する間隔（秒）

    [Header("出現位置の設定")]
    public float spawnZ = 20f;        //奥の出現Z座標
    public float[] laneXPositions = { -5f, 0f, 5f }; //左・中央・右レーンのX座標

    private Queue<GameObject> enemyPool;
    private float timer;

    private bool hasSpawnedBoss = false; //ボスを一体だけ出すため
    private bool isMiddleBossDefeated = false;
    private GameObject currentActiveBoss = null;
    private bool isCurrentBossMiddleBoss = false; //ボスが中ボスならtrue


    void Start()
    {
        enemyPool = new Queue<GameObject>();
        for (int i = 0; i < poolSize; i++)
        {
            GameObject enemy = Instantiate(enemyPrefab);
            enemy.SetActive(false);
            enemyPool.Enqueue(enemy);
        }
    }

    void Update()
    {
        timer += Time.deltaTime;
        float currentSpawnInterval = spawnInterval;

        CheckBossStatus();

        //4wave目と8wave目のボス出現について
        if (WaveManager.Instance != null)
        {
            int wave = WaveManager.Instance.GetCurrentWave();

            //4wave目
            if (wave == 4 && hasSpawnedBoss == false)
            {
                SpawnMiddleBoss();
                hasSpawnedBoss = true;
            }

            //8wave目
            if (wave == 8 && currentActiveBoss == null && hasSpawnedBoss == true)
            {
                if (isMiddleBossDefeated == true)
                {
                    SpawnLastBoss();
                }
                else
                {
                    SpawnMiddleBoss();
                }
                hasSpawnedBoss = false;
            }
        }

        //スライムの出現
        if (WaveManager.Instance != null)
        {
            int wave = WaveManager.Instance.GetCurrentWave();
            float calculatedInterval = spawnInterval - (wave - 1) * WaveManager.Instance.GetSpawnIntervalDecreasePerWave();
            currentSpawnInterval = Mathf.Max(calculatedInterval, 0.4f);
        }

        //一定時間ごとに敵を出現させる
        if (timer >= currentSpawnInterval)
        {
            SpawnEnemy();
            timer = 0f;
        }
    }

    //中ボスを倒したか判定
    void CheckBossStatus()
    {
        if (isCurrentBossMiddleBoss == true && currentActiveBoss != null && currentActiveBoss.activeSelf == false)
        {
            EnemyHealth bossHealth = currentActiveBoss.GetComponent<EnemyHealth>();
            if (bossHealth != null)
            {
                if (bossHealth.isDefeatedByPlayer == true)
                {
                    isMiddleBossDefeated = true;
                    Debug.Log("中ボスは倒された。");
                }
                else
                {
                    Debug.Log("中ボスは倒せなかった。");
                }
            }

            isCurrentBossMiddleBoss = false;
            currentActiveBoss = null;
        }
    }

    void SpawnEnemy()
    {
        if (enemyPool.Count == 0) return;

        int randomLane = Random.Range(0, laneXPositions.Length);

        //中ボスがいるとき、スライムは真ん中のレーンに出現しない
        if (currentActiveBoss != null && randomLane == 1)
        {
            if (Random.Range(0, 2) == 0)
            {
                randomLane = 0;
            }
            else
            {
                randomLane = 2;
            }
        }

        float spawnX = laneXPositions[randomLane];
        GameObject enemy = enemyPool.Dequeue();

        enemy.transform.position = new Vector3(spawnX, 1f, spawnZ);
        enemy.SetActive(true);

        enemyPool.Enqueue(enemy);
    }

    void SpawnMiddleBoss()
    {
        if (middleBossPrefab == null) return;

        float spawnX = laneXPositions[1];

        Vector3 spawnPosition =
            new Vector3(spawnX, groundY, spawnZ);

        currentActiveBoss = Instantiate(
            middleBossPrefab,
            spawnPosition,
            Quaternion.identity
        );

        PlaceOnGround(currentActiveBoss);

        isCurrentBossMiddleBoss = true;
        Debug.Log("中ボスが出現");
    }
    /*void SpawnMiddleBoss()
    {
        if (middleBossPrefab == null) return;

        float spawnX = laneXPositions[1];

        Vector3 spawnPosition = new Vector3(spawnX, 1f, spawnZ);

        currentActiveBoss = Instantiate(middleBossPrefab, spawnPosition, Quaternion.identity);
        currentActiveBoss.SetActive(true);
        isCurrentBossMiddleBoss = true;
        Debug.Log("中ボスが出現");
    }*/

    void SpawnLastBoss()
    {
        if (lastBossPrefab == null) return;

        float spawnX = laneXPositions[1];

        Vector3 spawnPosition =
            new Vector3(spawnX, groundY, spawnZ);

        currentActiveBoss = Instantiate(
            lastBossPrefab,
            spawnPosition,
            Quaternion.identity
        );

        PlaceOnGround(currentActiveBoss);

        Debug.Log("ラスボスが出現");
    }
    /*void SpawnLastBoss()
    {
        if (lastBossPrefab == null) return;

        float spawnX = laneXPositions[1];

        Vector3 spawnPosition = new Vector3(spawnX, 1f, spawnZ);

        currentActiveBoss = Instantiate(lastBossPrefab, spawnPosition, Quaternion.identity);
        currentActiveBoss.SetActive(true);
        Debug.Log("ラスボスが出現");
    }*/

    private void PlaceOnGround(GameObject target)
    {
        Collider targetCollider =
            target.GetComponentInChildren<Collider>();

        if (targetCollider == null)
        {
            Debug.LogWarning(
                $"{target.name} にColliderがないため、高さを自動調整できません。",
                target
            );
            return;
        }

        float bottomY = targetCollider.bounds.min.y;
        float correctionY = groundY - bottomY;

        target.transform.position +=
            Vector3.up * correctionY;
    }
}