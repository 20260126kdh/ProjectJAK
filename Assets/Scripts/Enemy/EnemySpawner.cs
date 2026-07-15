using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EnemyBattleData를 기준으로 전투에 등장할 메인 적들을 생성하고 관리합니다.
///
/// 적 수에 따른 배치:
///
/// 1마리:
/// - Single Spawn Point
///
/// 2마리:
/// - Double Left Spawn Point
/// - Double Right Spawn Point
///
/// 3마리:
/// - Triple Left Spawn Point
/// - Triple Center Spawn Point
/// - Triple Right Spawn Point
///
/// 메인 적이 사망하면 목록에서 제거하며,
/// 모든 메인 적이 사망했을 때만 전투 종료 검사를 요청합니다.
/// 장송의 원혼과 같은 전투 중 소환 개체는 이 목록에 포함하지 않습니다.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("현재 전투 데이터")]
    [SerializeField]
    private EnemyBattleData currentBattleData;

    [Header("1마리 배치 위치")]
    [SerializeField]
    private Transform singleSpawnPoint;

    [Header("2마리 배치 위치")]
    [SerializeField]
    private Transform doubleLeftSpawnPoint;

    [SerializeField]
    private Transform doubleRightSpawnPoint;

    [Header("3마리 배치 위치")]
    [SerializeField]
    private Transform tripleLeftSpawnPoint;

    [SerializeField]
    private Transform tripleCenterSpawnPoint;

    [SerializeField]
    private Transform tripleRightSpawnPoint;

    [Header("현재 생성된 메인 적")]
    [SerializeField]
    private List<Enemy> spawnedEnemies =
        new List<Enemy>();

    [Header("Battle Manager")]
    [SerializeField]
    private BattleManager battleManager;

    [Header("자동 생성")]
    [SerializeField]
    private bool spawnOnStart;

    /// <summary>
    /// 현재 설정된 전투 데이터입니다.
    /// </summary>
    public EnemyBattleData CurrentBattleData =>
        currentBattleData;

    /// <summary>
    /// 현재 생성된 메인 적 목록입니다.
    /// </summary>
    public IReadOnlyList<Enemy> SpawnedEnemies =>
        spawnedEnemies;

    /// <summary>
    /// 현재 살아 있는 메인 적 수입니다.
    /// 장송의 원혼과 같은 소환 개체는 포함하지 않습니다.
    /// </summary>
    public int ActiveEnemyCount
    {
        get
        {
            RemoveInvalidEnemies();

            return spawnedEnemies.Count;
        }
    }

    private void Awake()
    {
        spawnedEnemies.Clear();

        TryFindBattleManager();
    }

    private void Start()
    {
        if (!spawnOnStart)
        {
            return;
        }

        if (currentBattleData == null)
        {
            Debug.LogWarning(
                "[EnemySpawner] Current Battle Data가 없어 " +
                "적을 자동 생성하지 않았습니다.",
                this
            );

            return;
        }

        SpawnBattle(currentBattleData);
    }

    /// <summary>
    /// 지정된 전투 데이터를 사용하여
    /// 전투에 등장할 메인 적들을 생성합니다.
    ///
    /// 생성에 성공하면 true를 반환합니다.
    /// </summary>
    public bool SpawnBattle(
        EnemyBattleData battleData)
    {
        if (battleData == null)
        {
            Debug.LogError(
                "[EnemySpawner] 생성할 EnemyBattleData가 없습니다.",
                this
            );

            return false;
        }

        currentBattleData = battleData;

        ClearEnemies();

        List<GameObject> validEnemyPrefabs =
            GetValidEnemyPrefabs(battleData);

        if (validEnemyPrefabs.Count == 0)
        {
            Debug.LogError(
                $"[EnemySpawner] 전투 데이터 '{battleData.name}'에 " +
                "유효한 적 프리팹이 없습니다.",
                battleData
            );

            return false;
        }

        if (validEnemyPrefabs.Count > 3)
        {
            Debug.LogError(
                "[EnemySpawner] 현재 전투는 최대 3마리까지만 " +
                $"지원합니다. 등록된 적 수: {validEnemyPrefabs.Count}",
                battleData
            );

            return false;
        }

        List<Transform> selectedSpawnPoints =
            GetSpawnPointsByEnemyCount(
                validEnemyPrefabs.Count
            );

        if (selectedSpawnPoints.Count !=
            validEnemyPrefabs.Count)
        {
            Debug.LogError(
                "[EnemySpawner] 적 수에 맞는 생성 위치를 " +
                "가져오지 못했습니다.",
                this
            );

            return false;
        }

        for (int i = 0;
             i < validEnemyPrefabs.Count;
             i++)
        {
            GameObject enemyPrefab =
                validEnemyPrefabs[i];

            Transform spawnPoint =
                selectedSpawnPoints[i];

            if (enemyPrefab == null)
            {
                Debug.LogError(
                    $"[EnemySpawner] {i}번째 적 프리팹이 없습니다.",
                    this
                );

                ClearEnemies();
                return false;
            }

            if (spawnPoint == null)
            {
                Debug.LogError(
                    $"[EnemySpawner] {i}번째 적 생성 위치가 없습니다.",
                    this
                );

                ClearEnemies();
                return false;
            }

            GameObject spawnedObject =
                Instantiate(
                    enemyPrefab,
                    spawnPoint.position,
                    spawnPoint.rotation
                );

            spawnedObject.transform.SetParent(
                spawnPoint,
                true
            );

            /*
             * 부모 Transform의 스케일 영향을 최소화하고
             * 프리팹의 원래 월드 크기를 유지합니다.
             */
            spawnedObject.transform.position =
                spawnPoint.position;

            spawnedObject.transform.rotation =
                spawnPoint.rotation;

            Enemy spawnedEnemy =
                spawnedObject.GetComponent<Enemy>();

            if (spawnedEnemy == null)
            {
                Debug.LogError(
                    $"[EnemySpawner] 생성된 오브젝트 " +
                    $"'{spawnedObject.name}'에 Enemy 컴포넌트가 없습니다.",
                    spawnedObject
                );

                Destroy(spawnedObject);
                ClearEnemies();

                return false;
            }

            /*
             * 적이 사망할 때 자신을 생성한 스포너에
             * 사망 사실을 전달할 수 있도록 연결합니다.
             */
            spawnedEnemy.SetOwnerSpawner(this);

            spawnedEnemies.Add(spawnedEnemy);

            Debug.Log(
                $"[EnemySpawner] 메인 적 생성 완료 / " +
                $"{i + 1}/{validEnemyPrefabs.Count} / " +
                $"{spawnedObject.name}",
                spawnedObject
            );
        }

        Debug.Log(
            $"[EnemySpawner] 전투 적 생성 완료 / " +
            $"Battle ID: {battleData.BattleId} / " +
            $"메인 적 수: {spawnedEnemies.Count}",
            this
        );

        return true;
    }

    /// <summary>
    /// 현재 전투 데이터를 설정합니다.
    /// 설정만 수행하며 즉시 적을 생성하지는 않습니다.
    /// </summary>
    public void SetBattleData(
        EnemyBattleData battleData)
    {
        currentBattleData = battleData;
    }

    /// <summary>
    /// 현재 설정된 전투 데이터를 사용해
    /// 적을 생성합니다.
    /// </summary>
    public bool SpawnCurrentBattle()
    {
        if (currentBattleData == null)
        {
            Debug.LogError(
                "[EnemySpawner] 현재 전투 데이터가 없습니다.",
                this
            );

            return false;
        }

        return SpawnBattle(currentBattleData);
    }

    /// <summary>
    /// 기존 코드와의 호환을 위한 메서드입니다.
    /// 현재 전투 데이터를 사용해 적을 생성합니다.
    /// </summary>
    public void SpawnEnemy()
    {
        SpawnCurrentBattle();
    }

    /// <summary>
    /// 메인 적 한 마리가 사망했음을 등록합니다.
    ///
    /// 생성 목록에서 해당 적을 제거한 뒤,
    /// 남아 있는 메인 적이 없다면 전투 종료 검사를 요청합니다.
    /// </summary>
    public void NotifyEnemyDefeated(
        Enemy defeatedEnemy)
    {
        if (defeatedEnemy == null)
        {
            Debug.LogWarning(
                "[EnemySpawner] 사망한 적 참조가 없습니다.",
                this
            );

            return;
        }

        bool removed =
            spawnedEnemies.Remove(defeatedEnemy);

        if (!removed)
        {
            Debug.LogWarning(
                $"[EnemySpawner] 사망한 적이 메인 적 목록에 없습니다: " +
                $"{defeatedEnemy.name}",
                defeatedEnemy
            );

            return;
        }

        RemoveInvalidEnemies();

        int remainingEnemyCount =
            spawnedEnemies.Count;

        Debug.Log(
            $"[EnemySpawner] 메인 적 사망 등록 / " +
            $"사망 적: {defeatedEnemy.name} / " +
            $"남은 메인 적: {remainingEnemyCount}",
            this
        );

        /*
         * 메인 적이 한 마리 이상 남아 있다면
         * 전투를 계속 진행합니다.
         */
        if (remainingEnemyCount > 0)
        {
            return;
        }

        Debug.Log(
            "[EnemySpawner] 모든 메인 적 처치 / " +
            "전투 종료 검사를 요청합니다.",
            this
        );

        TryFindBattleManager();

        if (battleManager == null)
        {
            Debug.LogError(
                "[EnemySpawner] BattleManager를 찾지 못해 " +
                "전투 종료를 처리할 수 없습니다.",
                this
            );

            return;
        }

        battleManager.CheckBattleEnd();
    }

    /// <summary>
    /// 현재 생성된 모든 메인 적을 제거합니다.
    ///
    /// 다음 전투 준비를 위한 제거이므로
    /// 전투 종료 검사는 실행하지 않습니다.
    /// </summary>
    public void ClearEnemies()
    {
        for (int i = spawnedEnemies.Count - 1;
             i >= 0;
             i--)
        {
            Enemy enemy =
                spawnedEnemies[i];

            if (enemy != null)
            {
                Destroy(enemy.gameObject);
            }
        }

        spawnedEnemies.Clear();

        Debug.Log(
            "[EnemySpawner] 기존 메인 적 전체 제거",
            this
        );
    }

    /// <summary>
    /// 현재 생성된 메인 적 목록을 복사하여 반환합니다.
    ///
    /// 외부에서 목록을 순회하는 동안
    /// 원본 목록이 변경되는 문제를 방지합니다.
    /// </summary>
    public List<Enemy> GetActiveEnemies()
    {
        RemoveInvalidEnemies();

        return new List<Enemy>(
            spawnedEnemies
        );
    }

    /// <summary>
    /// 전투 데이터에서 null이 아닌 적 프리팹만 가져옵니다.
    /// </summary>
    private List<GameObject> GetValidEnemyPrefabs(
        EnemyBattleData battleData)
    {
        List<GameObject> validPrefabs =
            new List<GameObject>();

        IReadOnlyList<GameObject> enemyPrefabs =
            battleData.EnemyPrefabs;

        for (int i = 0;
             i < enemyPrefabs.Count;
             i++)
        {
            GameObject enemyPrefab =
                enemyPrefabs[i];

            if (enemyPrefab == null)
            {
                continue;
            }

            validPrefabs.Add(enemyPrefab);
        }

        return validPrefabs;
    }

    /// <summary>
    /// 적 수에 맞는 배치 위치 목록을 반환합니다.
    /// </summary>
    private List<Transform> GetSpawnPointsByEnemyCount(
        int enemyCount)
    {
        List<Transform> result =
            new List<Transform>();

        switch (enemyCount)
        {
            case 1:
                result.Add(singleSpawnPoint);
                break;

            case 2:
                result.Add(doubleLeftSpawnPoint);
                result.Add(doubleRightSpawnPoint);
                break;

            case 3:
                result.Add(tripleLeftSpawnPoint);
                result.Add(tripleCenterSpawnPoint);
                result.Add(tripleRightSpawnPoint);
                break;

            default:
                Debug.LogError(
                    $"[EnemySpawner] 지원하지 않는 적 수입니다: " +
                    $"{enemyCount}",
                    this
                );
                break;
        }

        return result;
    }

    /// <summary>
    /// 파괴되었거나 비활성화된 메인 적을
    /// 생성 목록에서 제거합니다.
    /// </summary>
    private void RemoveInvalidEnemies()
    {
        for (int i = spawnedEnemies.Count - 1;
             i >= 0;
             i--)
        {
            Enemy enemy =
                spawnedEnemies[i];

            if (enemy == null ||
                !enemy.gameObject.activeInHierarchy)
            {
                spawnedEnemies.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// 현재 씬에서 BattleManager를 찾습니다.
    /// </summary>
    private void TryFindBattleManager()
    {
        if (battleManager != null)
        {
            return;
        }

        battleManager =
            FindFirstObjectByType<BattleManager>();
    }

    /// <summary>
    /// 현재 전투 데이터를 Inspector에서 테스트 생성합니다.
    /// </summary>
    [ContextMenu("현재 전투 적 생성 테스트")]
    private void TestSpawnCurrentBattle()
    {
        SpawnCurrentBattle();
    }

    /// <summary>
    /// 생성된 메인 적을 Inspector에서 모두 제거합니다.
    /// </summary>
    [ContextMenu("생성된 적 전체 제거 테스트")]
    private void TestClearEnemies()
    {
        ClearEnemies();
    }
}