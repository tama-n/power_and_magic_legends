using UnityEngine;
using System.Collections.Generic;

public class EnemyPoolManager : MonoBehaviour
{
    public GameObject enemyPrefab; // 敵のPrefab（Cube）
    public int poolSize = 15;      // 同時に存在する最大数

    private List<GameObject> enemyPool = new List<GameObject>();

    void Start()
    {
        // 最初に指定した数だけ敵を作って、非表示にしてプールに貯める
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(enemyPrefab);
            obj.SetActive(false); // 非アクティブにする
            enemyPool.Add(obj);   // リストで管理
        }

        // テスト用：2秒ごとに敵を1体出現させる（必要に応じて呼ぶ）
        InvokeRepeating("SpawnEnemy", 0f, 2f);
    }

    // プールから使われていない敵を探して画面に出すメソッド
    void SpawnEnemy()
    {
        GameObject enemy = GetPooledEnemy();

        if (enemy != null)
        {
            // 出現位置を設定（例：奥側のランダムな位置）
            enemy.transform.position = new Vector3(Random.Range(-5f, 5f), 0f, 10f);
            enemy.SetActive(true); // アクティブにして出現させる
        }
    }

    // リストの中から、現在使われていない（非アクティブな）敵を見つける関数
    GameObject GetPooledEnemy()
    {
        foreach (GameObject enemy in enemyPool)
        {
            if (!enemy.activeInHierarchy)
            {
                return enemy; // 見つかったらそれを返す
            }
        }
        return null; // もし全エネミーが出撃中なら何もしない
    }
}