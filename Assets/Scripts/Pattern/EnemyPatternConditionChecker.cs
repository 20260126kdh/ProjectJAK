using UnityEngine;

/// <summary>
/// CSV 기반 적 패턴 행동의 실행 조건을 검사합니다.
///
/// 현재는 모르바엘의 장송의 원혼 관련 조건을 지원합니다.
///
/// 지원 조건:
/// - None
/// - HasActiveFuneralSpirit
/// - NoActiveFuneralSpirit
/// - FuneralSpiritCountBelow
/// </summary>
public class EnemyPatternConditionChecker : MonoBehaviour
{
    [Header("장송의 원혼 스포너")]
    [SerializeField]
    private FuneralSpiritSpawner funeralSpiritSpawner;

    /// <summary>
    /// 전달받은 패턴 행동의 조건이 충족됐는지 반환합니다.
    /// </summary>
    public bool IsConditionMet(
        EnemyPatternData patternData)
    {
        if (patternData == null)
        {
            Debug.LogWarning(
                "[EnemyPatternConditionChecker] " +
                "검사할 패턴 데이터가 없습니다.",
                this
            );

            return false;
        }

        switch (patternData.conditionType)
        {
            case EnemyPatternConditionType.None:
                return true;

            case EnemyPatternConditionType
                .HasActiveFuneralSpirit:

                return HasActiveFuneralSpirit();

            case EnemyPatternConditionType
                .NoActiveFuneralSpirit:

                return !HasActiveFuneralSpirit();

            case EnemyPatternConditionType
                .FuneralSpiritCountBelow:

                return IsFuneralSpiritCountBelow(
                    patternData.conditionValue
                );

            default:
                Debug.LogWarning(
                    "[EnemyPatternConditionChecker] " +
                    $"지원하지 않는 조건입니다: " +
                    $"{patternData.conditionType}",
                    this
                );

                return false;
        }
    }

    /// <summary>
    /// 현재 장송의 원혼이 한 마리 이상 존재하는지 반환합니다.
    /// </summary>
    private bool HasActiveFuneralSpirit()
    {
        TryFindFuneralSpiritSpawner();

        if (funeralSpiritSpawner == null)
        {
            /*
             * 원혼 스포너가 없는 전투에서는
             * 활성 원혼이 없는 것으로 판단합니다.
             */
            return false;
        }

        return funeralSpiritSpawner.HasActiveSpirit;
    }

    /// <summary>
    /// 현재 장송의 원혼 수가 지정된 값보다 적은지 반환합니다.
    ///
    /// 예:
    /// 현재 원혼 2마리, 조건값 3
    /// → 2 &lt; 3
    /// → true
    /// </summary>
    private bool IsFuneralSpiritCountBelow(
        int conditionValue)
    {
        if (conditionValue <= 0)
        {
            Debug.LogWarning(
                "[EnemyPatternConditionChecker] " +
                $"잘못된 원혼 수 조건입니다: {conditionValue}",
                this
            );

            return false;
        }

        TryFindFuneralSpiritSpawner();

        /*
         * 스포너가 없다면 현재 원혼 수는 0으로 판단합니다.
         *
         * 따라서 조건값이 3이라면:
         * 0 < 3 → true
         *
         * 실제 소환 행동 실행 단계에서는
         * 스포너가 없어서 소환이 실패하게 됩니다.
         */
        int currentSpiritCount =
            funeralSpiritSpawner != null
                ? funeralSpiritSpawner.ActiveSpiritCount
                : 0;

        bool result =
            currentSpiritCount < conditionValue;

        Debug.Log(
            "[EnemyPatternConditionChecker] " +
            $"원혼 수 조건 검사 : " +
            $"{currentSpiritCount} < {conditionValue} " +
            $"→ {result}",
            this
        );

        return result;
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

        if (funeralSpiritSpawner != null)
        {
            Debug.Log(
                "[EnemyPatternConditionChecker] " +
                "FuneralSpiritSpawner 자동 탐색 완료",
                this
            );
        }
    }

    /// <summary>
    /// 외부에서 장송의 원혼 스포너를 연결합니다.
    /// </summary>
    public void SetFuneralSpiritSpawner(
        FuneralSpiritSpawner spawner)
    {
        funeralSpiritSpawner = spawner;

        if (funeralSpiritSpawner == null)
        {
            Debug.LogWarning(
                "[EnemyPatternConditionChecker] " +
                "FuneralSpiritSpawner 연결에 실패했습니다.",
                this
            );

            return;
        }

        Debug.Log(
            "[EnemyPatternConditionChecker] " +
            "FuneralSpiritSpawner 연결 완료",
            this
        );
    }
}