using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [Header("敵のプレハブ")]
    public GameObject enemyPrefab;

    [Header("プールの設定")]
    public int poolSize = 30;         //用意する敵の数
    public float spawnInterval = 1.5f; //出現する間隔（秒）

    [Header("出現位置の設定")]
    public float spawnZ = 20f;        //奥の出現Z座標
    public float[] laneXPositions = { -5f, 0f, 5f }; //左・中央・右レーンのX座標

    private Queue<GameObject> enemyPool;
    private float timer;

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

        if(WaveManager.Instance != null)
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

    void SpawnEnemy()
    {
        // キューの先頭から敵を取り出す
        GameObject enemy = enemyPool.Dequeue();

        // もしその敵が非アクティブ（現在使われていない）なら再利用する
        if (!enemy.activeInHierarchy)
        {
            // レーンをランダムに決定
            int randomLaneIndex = Random.Range(0, laneXPositions.Length);
            float spawnX = laneXPositions[randomLaneIndex];

            // 位置をリセット（Y座標はフィールドの高さに合わせて微調整してください）
            enemy.transform.position = new Vector3(spawnX, 1f, spawnZ);

            // アクティブにして画面に表示
            enemy.SetActive(true);
        }

        // 再びキューの最後尾に追加（次に順番が回ってくる頃には手前で非アクティブになっている想定）
        enemyPool.Enqueue(enemy);
    }
}