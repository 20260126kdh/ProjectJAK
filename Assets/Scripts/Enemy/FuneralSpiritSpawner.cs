using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 모르바엘 전투에서 장송의 원혼을 생성하고 관리합니다.
///
/// 외부에서 SpawnFuneralSpirit()를 호출하면
/// 활성 원혼 수와 누적 소환 횟수를 확인한 뒤 원혼을 생성합니다.
///
/// 원혼은 최대 3마리까지 동시에 존재할 수 있으며,
/// 체력은 누적 소환 횟수에 따라 38부터 1씩 증가합니다.
/// </summary>
public class FuneralSpiritSpawner : MonoBehaviour
{
    [Header("장송의 원혼 프리팹")]
    [SerializeField]
    private FuneralSpirit funeralSpiritPrefab;

    [Header("원혼 생성 위치")]
    [SerializeField]
    private List<Transform> spawnPoints =
        new List<Transform>();

    [Header("모르바엘 안식 컨트롤러")]
    [SerializeField]
    private RIPController ripController;

    [Header("원혼 설정")]
    [SerializeField]
    [Min(1)]
    private int maxActiveSpiritCount = 3;

    [SerializeField]
    [Min(1)]
    private int firstSpiritHealth = 38;

    [Header("현재 원혼 상태")]
    [SerializeField]
    private int totalSummonCount;

    [SerializeField]
    private List<FuneralSpirit> activeSpirits =
        new List<FuneralSpirit>();

    /// <summary>
    /// 현재 살아 있는 원혼 수입니다.
    /// </summary>
    public int ActiveSpiritCount
    {
        get
        {
            RemoveInvalidSpirits();
            return activeSpirits.Count;
        }
    }

    /// <summary>
    /// 이번 전투에서 원혼이 소환된 누적 횟수입니다.
    /// </summary>
    public int TotalSummonCount =>
        totalSummonCount;

    /// <summary>
    /// 현재 원혼이 한 마리 이상 존재하는지 반환합니다.
    /// </summary>
    public bool HasActiveSpirit
    {
        get
        {
            RemoveInvalidSpirits();
            return activeSpirits.Count > 0;
        }
    }

    private void Awake()
    {
        totalSummonCount = 0;
        activeSpirits.Clear();

        TryFindRIPController();
    }

    /// <summary>
    /// 외부에서 모르바엘의 RIPController를 연결합니다.
    /// </summary>
    public void SetRIPController(
    RIPController controller)
    {
        ripController = controller;

        if (ripController == null)
        {
            Debug.LogWarning(
                "[FuneralSpiritSpawner] " +
                "RIPController 연결에 실패했습니다.",
                this
            );

            return;
        }

        ripController.SetFuneralSpiritSpawner(this);

        Debug.Log(
            "[FuneralSpiritSpawner] " +
            "RIPController 연결 완료",
            this
        );
    }

    /// <summary>
    /// 장송의 원혼 한 마리를 생성합니다.
    ///
    /// 성공하면 true,
    /// 최대 수량 또는 설정 문제로 실패하면 false를 반환합니다.
    /// </summary>
    public bool SpawnFuneralSpirit()
    {
        RemoveInvalidSpirits();

        if (!ValidateSpawnSettings())
        {
            return false;
        }

        if (activeSpirits.Count >=
            maxActiveSpiritCount)
        {
            Debug.Log(
                $"[FuneralSpiritSpawner] 원혼 최대치 : " +
                $"{activeSpirits.Count}/" +
                $"{maxActiveSpiritCount}",
                this
            );

            return false;
        }

        int spawnPointIndex =
            GetAvailableSpawnPointIndex();

        if (spawnPointIndex < 0)
        {
            Debug.LogWarning(
                "[FuneralSpiritSpawner] " +
                "사용 가능한 생성 위치가 없습니다.",
                this
            );

            return false;
        }

        int spiritHealth =
            firstSpiritHealth + totalSummonCount;

        Transform selectedSpawnPoint =
            spawnPoints[spawnPointIndex];

        FuneralSpirit spawnedSpirit =
            Instantiate(
                funeralSpiritPrefab,
                selectedSpawnPoint.position,
                selectedSpawnPoint.rotation
            );

        spawnedSpirit.transform.SetParent(
            selectedSpawnPoint
        );

        spawnedSpirit.Initialize(
            spiritHealth,
            this
        );

        activeSpirits.Add(spawnedSpirit);
        totalSummonCount++;

        Debug.Log(
            $"[FuneralSpiritSpawner] 원혼 소환 완료 / " +
            $"누적 {totalSummonCount}회 / " +
            $"체력 {spiritHealth} / " +
            $"현재 {activeSpirits.Count}/" +
            $"{maxActiveSpiritCount}",
            this
        );

        return true;
    }

    /// <summary>
    /// 현재 원혼을 추가로 소환할 수 있는지 반환합니다.
    /// </summary>
    public bool CanSpawnFuneralSpirit()
    {
        RemoveInvalidSpirits();

        if (funeralSpiritPrefab == null)
        {
            return false;
        }

        if (spawnPoints == null ||
            spawnPoints.Count == 0)
        {
            return false;
        }

        return activeSpirits.Count <
               maxActiveSpiritCount;
    }

    /// <summary>
    /// 원혼 사망을 처리합니다.
    ///
    /// 활성 목록에서 제거하고
    /// 모르바엘의 안식을 1 증가시킵니다.
    /// </summary>
    public void NotifySpiritDeath(
        FuneralSpirit funeralSpirit)
    {
        if (funeralSpirit == null)
        {
            Debug.LogWarning(
                "[FuneralSpiritSpawner] " +
                "사망한 원혼 참조가 없습니다.",
                this
            );

            return;
        }

        bool removed =
            activeSpirits.Remove(funeralSpirit);

        if (!removed)
        {
            Debug.LogWarning(
                "[FuneralSpiritSpawner] " +
                "사망한 원혼이 활성 목록에 없습니다.",
                funeralSpirit
            );
        }

        TryFindRIPController();

        if (ripController != null)
        {
            ripController.AddRIP();
        }
        else
        {
            Debug.LogWarning(
                "[FuneralSpiritSpawner] " +
                "RIPController가 없어 안식이 증가하지 않았습니다.",
                this
            );
        }

        Debug.Log(
            $"[FuneralSpiritSpawner] 원혼 사망 처리 / " +
            $"현재 원혼 {activeSpirits.Count}/" +
            $"{maxActiveSpiritCount}",
            this
        );
    }

    /// <summary>
    /// 현재 씬에서 모르바엘의 RIPController를 찾습니다.
    /// </summary>
    /// <summary>
    /// 현재 씬에서 모르바엘의 RIPController를 찾고
    /// 서로의 참조를 연결합니다.
    /// </summary>
    private void TryFindRIPController()
    {
        if (ripController == null)
        {
            ripController =
                FindFirstObjectByType<RIPController>();
        }

        if (ripController == null)
        {
            return;
        }

        ripController.SetFuneralSpiritSpawner(this);

        Debug.Log(
            "[FuneralSpiritSpawner] " +
            "RIPController 연결 완료",
            this
        );
    }

    /// <summary>
    /// 사망하거나 비활성화된 원혼을 활성 목록에서 제거합니다.
    /// </summary>
    private void RemoveInvalidSpirits()
    {
        for (int i = activeSpirits.Count - 1;
             i >= 0;
             i--)
        {
            FuneralSpirit spirit =
                activeSpirits[i];

            if (spirit == null ||
                !spirit.gameObject.activeInHierarchy)
            {
                activeSpirits.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// 현재 사용되지 않는 생성 위치를 찾습니다.
    /// </summary>
    private int GetAvailableSpawnPointIndex()
    {
        for (int i = 0;
             i < spawnPoints.Count;
             i++)
        {
            Transform spawnPoint =
                spawnPoints[i];

            if (spawnPoint == null)
            {
                continue;
            }

            bool isOccupied = false;

            for (int j = 0;
                 j < activeSpirits.Count;
                 j++)
            {
                FuneralSpirit spirit =
                    activeSpirits[j];

                if (spirit == null)
                {
                    continue;
                }

                if (spirit.transform.parent ==
                    spawnPoint)
                {
                    isOccupied = true;
                    break;
                }
            }

            if (!isOccupied)
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// 원혼 생성에 필요한 설정을 검사합니다.
    /// </summary>
    private bool ValidateSpawnSettings()
    {
        TryFindRIPController();

        if (funeralSpiritPrefab == null)
        {
            Debug.LogError(
                "[FuneralSpiritSpawner] " +
                "원혼 프리팹이 지정되지 않았습니다.",
                this
            );

            return false;
        }

        if (spawnPoints == null ||
            spawnPoints.Count == 0)
        {
            Debug.LogError(
                "[FuneralSpiritSpawner] " +
                "원혼 생성 위치가 지정되지 않았습니다.",
                this
            );

            return false;
        }

        if (ripController == null)
        {
            Debug.LogWarning(
                "[FuneralSpiritSpawner] " +
                "RIPController가 연결되지 않았습니다. " +
                "원혼은 생성되지만 안식 증가는 처리되지 않습니다.",
                this
            );
        }

        return true;
    }

    /// <summary>
    /// 모든 원혼을 제거하고 누적 소환 횟수를 초기화합니다.
    /// </summary>
    public void ResetSpawner()
    {
        for (int i = activeSpirits.Count - 1;
             i >= 0;
             i--)
        {
            FuneralSpirit spirit =
                activeSpirits[i];

            if (spirit != null)
            {
                Destroy(spirit.gameObject);
            }
        }

        activeSpirits.Clear();
        totalSummonCount = 0;

        Debug.Log(
            "[FuneralSpiritSpawner] 초기화 완료",
            this
        );
    }

    /// <summary>
    /// Inspector에서 원혼 소환을 테스트합니다.
    /// </summary>
    [ContextMenu("장송의 원혼 소환 테스트")]
    private void TestSpawnFuneralSpirit()
    {
        SpawnFuneralSpirit();
    }
}