using UnityEngine;

/// <summary>
/// BattleScene 시작 시 적을 생성하는 컴포넌트입니다.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Point")]
    [SerializeField]
    private Transform spawnPoint;

    [Header("Enemy Prefab")]
    [SerializeField]
    private GameObject enemyPrefab;

    /// <summary>
    /// 생성된 적
    /// </summary>
    public GameObject SpawnedEnemy { get; private set; }

    private void Start()
    {
        SpawnEnemy();
    }

    /// <summary>
    /// 적을 생성합니다.
    /// </summary>
    private void SpawnEnemy()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("Enemy Prefab이 지정되지 않았습니다.");
            return;
        }

        SpawnedEnemy = Instantiate(
            enemyPrefab,
            spawnPoint.position,
            Quaternion.identity);
    }
}