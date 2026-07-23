using UnityEngine;

/// <summary>
/// CSV에서 읽은 적 패턴 행동을 실제 전투 효과로 실행합니다.
///
/// 지원 행동:
/// - DealDamage
/// - GainBlock
/// - ApplyStatus
/// - Heal
/// - SummonFuneralSpirit
/// </summary>
public class EnemyPatternExecutor : MonoBehaviour
{
    [Header("행동 주체")]
    [SerializeField]
    private Enemy ownerEnemy;

    [Header("플레이어")]
    [SerializeField]
    private PlayerCombat playerCombat;

    [Header("장송의 원혼 스포너")]
    [SerializeField]
    private FuneralSpiritSpawner funeralSpiritSpawner;

    private void Awake()
    {
        TryFindReferences();
    }

    /// <summary>
    /// 패턴 행동 하나를 실제로 실행합니다.
    ///
    /// 실행에 성공하면 true,
    /// 참조 누락이나 지원하지 않는 행동이면 false를 반환합니다.
    /// </summary>
    public bool ExecuteAction(
        EnemyPatternData patternData)
    {
        if (patternData == null)
        {
            Debug.LogWarning(
                "[EnemyPatternExecutor] 실행할 패턴 데이터가 없습니다.",
                this
            );

            return false;
        }

        TryFindReferences();

        switch (patternData.actionType)
        {
            case EnemyPatternActionType.DealDamage:
                return ExecuteDealDamage(patternData);

            case EnemyPatternActionType.GainBlock:
                return ExecuteGainBlock(patternData);

            case EnemyPatternActionType.ApplyStatus:
                return ExecuteApplyStatus(patternData);

            case EnemyPatternActionType.Heal:
                return ExecuteHeal(patternData);

            case EnemyPatternActionType.SummonFuneralSpirit:
                return ExecuteSummonFuneralSpirit(patternData);

            case EnemyPatternActionType.NoAction:
                return ExecuteNoAction();

            case EnemyPatternActionType.ReadyToStrongAttack:
                return ExecuteReadyToStrongAttack();

            case EnemyPatternActionType.None:
                Debug.LogWarning(
                    "[EnemyPatternExecutor] ActionType이 None입니다.",
                    this
                );

                return false;

            default:
                Debug.LogWarning(
                    "[EnemyPatternExecutor] " +
                    $"지원하지 않는 행동입니다: " +
                    $"{patternData.actionType}",
                    this
                );

                return false;
        }
    }

    /// <summary>
    /// 플레이어에게 피해를 반복 적용합니다.
    ///
    /// 행동 주체인 적에게 약화가 있다면
    /// 각 타격의 피해를 40% 감소시킵니다.
    /// </summary>
    /// <summary>
    /// 플레이어에게 피해를 지정된 횟수만큼 개별 적용합니다.
    /// </summary>
    private bool ExecuteDealDamage(
        EnemyPatternData patternData)
    {
        if (playerCombat == null)
        {
            Debug.LogWarning(
                "[EnemyPatternExecutor] " +
                "PlayerCombat이 없어 피해를 적용할 수 없습니다.",
                this
            );

            return false;
        }

        int baseDamage =
            Mathf.Max(
                0,
                patternData.value
            );

        int repeatCount =
            Mathf.Max(
                1,
                patternData.repeatCount
            );

        Debug.Log(
            $"[EnemyPatternExecutor] 다단 공격 시작 : " +
            $"기본 피해 {baseDamage} / " +
            $"반복 횟수 {repeatCount}",
            this
        );

        for (int i = 0;
             i < repeatCount;
             i++)
        {
            /*
             * 각 타격마다 약화를 계산합니다.
             * 현재는 같은 공격 중 상태가 바뀌지 않지만,
             * 개별 타격 처리라는 의미를 명확하게 유지합니다.
             */
            int finalDamage =
                CalculateOutgoingDamage(
                    baseDamage
                );

            Debug.Log(
                $"[EnemyPatternExecutor] 다단 공격 적용 : " +
                $"{i + 1}/{repeatCount} / " +
                $"피해 {finalDamage}",
                this
            );

            playerCombat.ReceiveAttackDamage(
                finalDamage
            );
        }

        Debug.Log(
            $"[EnemyPatternExecutor] 다단 공격 완료 : " +
            $"{baseDamage} × {repeatCount}",
            this
        );

        return true;
    }

    /// <summary>
    /// 행동 주체인 적이 방어도를 획득합니다.
    /// </summary>
    private bool ExecuteGainBlock(
        EnemyPatternData patternData)
    {
        if (ownerEnemy == null)
        {
            Debug.LogWarning(
                "[EnemyPatternExecutor] " +
                "Owner Enemy가 없어 방어도를 획득할 수 없습니다.",
                this
            );

            return false;
        }

        int blockAmount =
            Mathf.Max(
                0,
                patternData.value
            );

        ownerEnemy.GainBlock(blockAmount);

        Debug.Log(
            $"[EnemyPatternExecutor] 방어도 획득 실행 : " +
            $"+{blockAmount}",
            this
        );

        return true;
    }

    /// <summary>
    /// 지정된 대상에게 상태효과를 부여합니다.
    /// </summary>
    private bool ExecuteApplyStatus(
        EnemyPatternData patternData)
    {
        if (!patternData.hasStatusType)
        {
            Debug.LogWarning(
                "[EnemyPatternExecutor] " +
                "ApplyStatus 행동에 StatusType이 지정되지 않았습니다.",
                this
            );

            return false;
        }

        StatusEffectHandler targetStatusHandler =
            GetTargetStatusEffectHandler(
                patternData.targetType
            );

        if (targetStatusHandler == null)
        {
            Debug.LogWarning(
                "[EnemyPatternExecutor] " +
                $"상태효과 대상이 없습니다. 대상: " +
                $"{patternData.targetType}",
                this
            );

            return false;
        }

        int value =
            Mathf.Max(
                0,
                patternData.value
            );

        int duration =
            Mathf.Max(
                0,
                patternData.duration
            );

        targetStatusHandler.AddStatusEffect(
            patternData.statusType,
            value,
            duration,
            patternData.isPermanent
        );

        Debug.Log(
            $"[EnemyPatternExecutor] 상태효과 실행 : " +
            $"{patternData.statusType} / " +
            $"수치 {value} / " +
            $"지속 {duration} / " +
            $"대상 {patternData.targetType}",
            this
        );

        return true;
    }

    /// <summary>
    /// 행동 주체인 적의 체력을 회복합니다.
    /// </summary>
    private bool ExecuteHeal(
        EnemyPatternData patternData)
    {
        if (ownerEnemy == null)
        {
            Debug.LogWarning(
                "[EnemyPatternExecutor] " +
                "Owner Enemy가 없어 회복할 수 없습니다.",
                this
            );

            return false;
        }

        int healAmount =
            Mathf.Max(
                0,
                patternData.value
            );

        ownerEnemy.Heal(healAmount);

        Debug.Log(
            $"[EnemyPatternExecutor] 체력 회복 실행 : " +
            $"+{healAmount}",
            this
        );

        return true;
    }

    /// <summary>
    /// 장송의 원혼 소환을 요청합니다.
    ///
    /// 최대 수량에 도달했거나 스포너 설정에 문제가 있으면
    /// 소환이 실패합니다.
    /// </summary>
    private bool ExecuteSummonFuneralSpirit(
        EnemyPatternData patternData)
    {
        TryFindFuneralSpiritSpawner();

        if (funeralSpiritSpawner == null)
        {
            Debug.LogWarning(
                "[EnemyPatternExecutor] " +
                "FuneralSpiritSpawner가 없어 원혼을 소환할 수 없습니다.",
                this
            );

            return false;
        }

        bool spawned =
            funeralSpiritSpawner.SpawnFuneralSpirit();

        Debug.Log(
            $"[EnemyPatternExecutor] 원혼 소환 실행 결과 : " +
            $"{spawned}",
            this
        );

        return spawned;
    }

    /// <summary>
    /// 이번 패턴 턴에 아무 행동도 하지 않습니다.
    /// 패턴 행동 자체는 정상적으로 실행된 것으로 처리합니다.
    /// </summary>
    private bool ExecuteNoAction()
    {
        Debug.Log(
            "[EnemyPatternExecutor] " +
            "적이 이번 턴에 아무 행동도 하지 않습니다.",
            this
        );

        return true;
    }

    /// <summary>
    /// 다음 턴의 강력한 공격을 준비합니다.
    ///
    /// 이번 턴에는 실제 피해나 상태 변화가 없으며,
    /// Intent UI에 강공격 준비 행동을 표시하기 위한 데이터입니다.
    /// </summary>
    private bool ExecuteReadyToStrongAttack()
    {
        Debug.Log(
            "[EnemyPatternExecutor] " +
            "적이 다음 턴의 강력한 공격을 준비합니다.",
            this
        );

        return true;
    }

    /// <summary>
    /// 행동 주체에게 적용된 약화를 반영하여
    /// 최종 공격 피해를 계산합니다.
    /// </summary>
    private int CalculateOutgoingDamage(
        int baseDamage)
    {
        int finalDamage =
            Mathf.Max(
                0,
                baseDamage
            );

        if (ownerEnemy == null)
        {
            return finalDamage;
        }

        StatusEffectHandler statusEffectHandler =
            ownerEnemy.GetComponent<StatusEffectHandler>();

        if (statusEffectHandler == null)
        {
            return finalDamage;
        }

        if (!statusEffectHandler.HasStatusEffect(
                StatusEffectType.Weaken
            ))
        {
            return finalDamage;
        }

        int reducedDamage =
            Mathf.FloorToInt(
                finalDamage * 0.6f
            );

        Debug.Log(
            $"[EnemyPatternExecutor] 약화 적용 : " +
            $"{finalDamage} → {reducedDamage}",
            this
        );

        return reducedDamage;
    }

    /// <summary>
    /// 패턴의 대상 종류에 맞는
    /// StatusEffectHandler를 반환합니다.
    /// </summary>
    private StatusEffectHandler GetTargetStatusEffectHandler(
        EnemyPatternTargetType targetType)
    {
        switch (targetType)
        {
            case EnemyPatternTargetType.Self:
                if (ownerEnemy == null)
                {
                    return null;
                }

                return ownerEnemy.GetComponent<StatusEffectHandler>();

            case EnemyPatternTargetType.Player:
                if (playerCombat == null)
                {
                    return null;
                }

                return playerCombat.GetComponent<StatusEffectHandler>();

            default:
                return null;
        }
    }

    /// <summary>
    /// 실행에 필요한 공통 참조를 자동으로 찾습니다.
    /// </summary>
    private void TryFindReferences()
    {
        if (ownerEnemy == null)
        {
            ownerEnemy =
                GetComponent<Enemy>();
        }

        if (playerCombat == null)
        {
            playerCombat =
                FindFirstObjectByType<PlayerCombat>();
        }

        TryFindFuneralSpiritSpawner();
    }

    /// <summary>
    /// 현재 씬에서 장송의 원혼 스포너를 찾습니다.
    /// </summary>
    private void TryFindFuneralSpiritSpawner()
    {
        if (funeralSpiritSpawner != null)
        {
            return;
        }

        funeralSpiritSpawner =
            FindFirstObjectByType<FuneralSpiritSpawner>();
    }

    /// <summary>
    /// 외부에서 행동 주체인 적을 연결합니다.
    /// </summary>
    public void SetOwnerEnemy(Enemy enemy)
    {
        ownerEnemy = enemy;
    }

    /// <summary>
    /// 외부에서 PlayerCombat을 연결합니다.
    /// </summary>
    public void SetPlayerCombat(
        PlayerCombat targetPlayerCombat)
    {
        playerCombat = targetPlayerCombat;
    }

    /// <summary>
    /// 외부에서 원혼 스포너를 연결합니다.
    /// </summary>
    public void SetFuneralSpiritSpawner(
        FuneralSpiritSpawner spawner)
    {
        funeralSpiritSpawner = spawner;
    }
}