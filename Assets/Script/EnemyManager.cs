using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [Header("敵のプレハブ")]
    public GameObject enemyPrefab;
    public GameObject middleBossPrefab;

    [Header("プールの設定")]
    public int poolSize = 30;         //用意する敵の数
    public float spawnInterval = 1.5f; //出現する間隔（秒）

    [Header("出現位置の設定")]
    public float spawnZ = 20f;        //奥の出現Z座標
    public float[] laneXPositions = { -5f, 0f, 5f }; //左・中央・右レーンのX座標

    private Queue<GameObject> enemyPool;
    private float timer;

    private bool hasSpawnedBoss = false;
    private GameObject middleBoss = null;

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

        //中ボスの出現
        if (WaveManager.Instance != null)
        {
            int wave = WaveManager.Instance.GetCurrentWave();

            if (wave == 4 && hasSpawnedBoss == false)
            {
                SpawnMiddleBoss(); 
                hasSpawnedBoss = true; 
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

    //スライムを出現させる関数
    void SpawnEnemy()
    {
        if (enemyPool.Count == 0) return;

        //ランダムにレーンを決定
        int randomLane = Random.Range(0, laneXPositions.Length);

        //中ボスがいるとき、スライムは真ん中のレーンに出現しない
        if (middleBoss != null && randomLane == 1)
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

    //中ボスを出現させる関数
    void SpawnMiddleBoss()
    {
        if (middleBossPrefab == null) return;

        int middleLaneIndex = 1;
        float spawnX = laneXPositions[middleLaneIndex];

        Vector3 spawnPosition = new Vector3(spawnX, 1f, spawnZ);
        middleBoss = Instantiate(middleBossPrefab, spawnPosition, Quaternion.identity);

        middleBoss.SetActive(true);
        Debug.Log("中ボスが出現");
    }
}