using UnityEngine;

/// <summary>
/// BattleScene에서 적을 생성하는 스크립트입니다.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Point")]
    [SerializeField]
    private Transform spawnPoint;

    [Header("Enemy Prefab")]
    [SerializeField]
    private GameObject enemyPrefab;

    public GameObject SpawnedEnemy { get; private set; }

    private void Start()
    {
        SpawnEnemy();
    }

    public void SpawnEnemy()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("[EnemySpawner] Enemy Prefab이 지정되지 않았습니다.");
            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogError("[EnemySpawner] Spawn Point가 지정되지 않았습니다.");
            return;
        }

        ClearEnemy();

        SpawnedEnemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);

        Debug.Log("[EnemySpawner] 적 생성 완료");
    }

    public void ClearEnemy()
    {
        if (SpawnedEnemy != null)
        {
            Destroy(SpawnedEnemy);
            SpawnedEnemy = null;

            Debug.Log("[EnemySpawner] 기존 적 제거");
        }
    }
}