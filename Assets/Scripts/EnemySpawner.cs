using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    public GameObject enemyPrefab;
    public float spawnInterval = 5f;
    public Transform[] spawnPoints;
    
    [Header("Limits")]
    public int maxEnemies = 5;
    private int currentEnemies = 0;

    void Start()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("EnemyPrefab is not assigned in the EnemySpawner!");
            return;
        }

        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            // You can remove or modify the maxEnemies check if you want infinite spawning
            if (currentEnemies < maxEnemies)
            {
                SpawnEnemy();
            }
        }
    }

    void SpawnEnemy()
    {
        Transform spawnPoint = transform; // Default to spawner's position
        
        // If spawn points are assigned, pick a random one
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        }

        Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        currentEnemies++;
    }

    // Call this public method from your enemy script when it dies
    public void OnEnemyDied()
    {
        currentEnemies--;
    }
}
