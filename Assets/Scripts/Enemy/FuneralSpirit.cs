using UnityEngine;

/// <summary>
/// 모르바엘이 소환하는 장송의 원혼을 관리합니다.
///
/// 생성될 때 전달받은 체력으로 Enemy를 초기화하며,
/// 사망 시 자신을 생성한 FuneralSpiritSpawner에 알립니다.
/// </summary>
public class FuneralSpirit : MonoBehaviour
{
    [Header("Enemy")]
    [SerializeField]
    private Enemy enemy;

    [Header("생성한 원혼 스포너")]
    [SerializeField]
    private FuneralSpiritSpawner ownerSpawner;

    [Header("초기화 여부")]
    [SerializeField]
    private bool isInitialized;

    [Header("사망 처리 여부")]
    [SerializeField]
    private bool deathNotified;

    /// <summary>
    /// 원혼이 정상적으로 초기화되었는지 여부입니다.
    /// </summary>
    public bool IsInitialized => isInitialized;

    /// <summary>
    /// 사망 알림 처리가 완료됐는지 여부입니다.
    /// </summary>
    public bool DeathNotified => deathNotified;

    private void Awake()
    {
        if (enemy == null)
        {
            enemy = GetComponent<Enemy>();
        }

        if (enemy == null)
        {
            Debug.LogError(
                "[FuneralSpirit] 같은 오브젝트에서 " +
                "Enemy 컴포넌트를 찾지 못했습니다.",
                this
            );
        }
    }

    /// <summary>
    /// 원혼을 지정된 체력으로 초기화합니다.
    /// </summary>
    /// <param name="maxHealth">
    /// 이번 원혼의 최대 체력입니다.
    /// </param>
    /// <param name="spawner">
    /// 이 원혼을 생성한 FuneralSpiritSpawner입니다.
    /// </param>
    public void Initialize(
        int maxHealth,
        FuneralSpiritSpawner spawner)
    {
        if (enemy == null)
        {
            Debug.LogError(
                "[FuneralSpirit] Enemy 참조가 없어 " +
                "초기화할 수 없습니다.",
                this
            );

            return;
        }

        if (maxHealth <= 0)
        {
            Debug.LogWarning(
                $"[FuneralSpirit] 잘못된 체력이 전달되었습니다: " +
                $"{maxHealth}",
                this
            );

            return;
        }

        if (spawner == null)
        {
            Debug.LogWarning(
                "[FuneralSpirit] 생성한 스포너가 전달되지 않았습니다.",
                this
            );
        }

        ownerSpawner = spawner;

        enemy.InitializeHealth(maxHealth);

        isInitialized = true;
        deathNotified = false;

        Debug.Log(
            $"[FuneralSpirit] 원혼 초기화 완료 / " +
            $"체력 {maxHealth}",
            this
        );
    }

    /// <summary>
    /// 원혼 사망 시 한 번만 호출됩니다.
    ///
    /// 자신을 생성한 스포너에 사망 사실을 전달하여
    /// 활성 목록 제거와 모르바엘의 안식 증가를 처리합니다.
    /// </summary>
    public void NotifyDeath()
    {
        if (!isInitialized)
        {
            Debug.LogWarning(
                "[FuneralSpirit] 초기화되지 않은 원혼의 " +
                "사망 알림이 요청되었습니다.",
                this
            );

            return;
        }

        if (deathNotified)
        {
            return;
        }

        deathNotified = true;
        isInitialized = false;

        if (ownerSpawner == null)
        {
            Debug.LogWarning(
                "[FuneralSpirit] 원혼 스포너가 없어 " +
                "활성 목록 제거와 안식 증가를 처리하지 못했습니다.",
                this
            );

            return;
        }

        ownerSpawner.NotifySpiritDeath(this);

        Debug.Log(
            "[FuneralSpirit] 원혼 사망 알림 완료",
            this
        );
    }
}