using UnityEngine;

/// <summary>
/// 모르바엘 전용 안식 상태를 관리합니다.
///
/// 안식이 최대 수치보다 낮을 때 모르바엘의 체력이 0이 되면
/// 사망하지 않고 체력 1로 버틸 수 있도록 사망 방지를 요청합니다.
///
/// 사망 방지가 발동하면 다음 적 턴 시작 시
/// 지정된 수치만큼 체력을 회복하도록 대기 상태가 됩니다.
///
/// 장송의 원혼이 처치될 때마다 안식이 1 증가하며,
/// 안식이 최대 수치에 도달하면 모르바엘을 정상적으로 처치할 수 있습니다.
/// </summary>
public class RIPController : MonoBehaviour
{
    [Header("최대 안식 수치")]
    [SerializeField]
    private int maxRIPValue = 3;

    [Header("현재 안식 수치")]
    [SerializeField]
    private int currentRIPValue;

    [Header("사망 방지 후 회복량")]
    [SerializeField]
    private int reviveHealAmount = 30;

    [Header("다음 적 턴 회복 대기 여부")]
    [SerializeField]
    private bool isWaitingForHeal;

    /// <summary>
    /// 현재 안식 수치를 반환합니다.
    /// </summary>
    public int CurrentRIPValue =>
        currentRIPValue;

    /// <summary>
    /// 최대 안식 수치를 반환합니다.
    /// </summary>
    public int MaxRIPValue =>
        maxRIPValue;

    /// <summary>
    /// 다음 적 턴 회복을 기다리고 있는지 반환합니다.
    /// </summary>
    public bool IsWaitingForHeal =>
        isWaitingForHeal;

    /// <summary>
    /// 안식이 최대 수치에 도달했는지 반환합니다.
    /// </summary>
    public bool HasReachedMaxRIP =>
        currentRIPValue >= maxRIPValue;

    private void Awake()
    {
        Initialize();
    }

    /// <summary>
    /// 전투 시작 상태로 초기화합니다.
    /// </summary>
    public void Initialize()
    {
        maxRIPValue =
            Mathf.Max(1, maxRIPValue);

        currentRIPValue = 0;
        isWaitingForHeal = false;

        Debug.Log(
            $"[RIPController] 안식 초기화 : " +
            $"{currentRIPValue}/{maxRIPValue}"
        );
    }

    /// <summary>
    /// 장송의 원혼이 처치됐을 때 안식을 1 증가시킵니다.
    /// 최대 수치를 초과하지 않습니다.
    /// </summary>
    public void AddRIP()
    {
        AddRIP(1);
    }

    /// <summary>
    /// 지정한 수치만큼 안식을 증가시킵니다.
    /// </summary>
    public void AddRIP(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        if (HasReachedMaxRIP)
        {
            Debug.Log(
                "[RIPController] 안식이 이미 최대치입니다."
            );

            return;
        }

        int previousRIPValue =
            currentRIPValue;

        currentRIPValue =
            Mathf.Min(
                maxRIPValue,
                currentRIPValue + amount
            );

        Debug.Log(
            $"[RIPController] 안식 증가 : " +
            $"{previousRIPValue} → {currentRIPValue} / " +
            $"최대 {maxRIPValue}"
        );

        if (HasReachedMaxRIP)
        {
            /*
             * 안식이 최대치에 도달하면 더 이상
             * 사망 방지 후 회복하지 않습니다.
             */
            isWaitingForHeal = false;

            Debug.Log(
                "[RIPController] 안식 최대치 도달 : " +
                "이제 모르바엘을 처치할 수 있습니다."
            );
        }
    }

    /// <summary>
    /// 체력이 0이 됐을 때 사망 방지를 시도합니다.
    ///
    /// 안식이 최대치 미만이면 다음 적 턴 회복을 예약하고
    /// true를 반환합니다.
    ///
    /// 안식이 최대치라면 false를 반환하여
    /// 일반 사망 처리가 진행되도록 합니다.
    /// </summary>
    public bool TryPreventDeath()
    {
        if (HasReachedMaxRIP)
        {
            Debug.Log(
                "[RIPController] 안식 최대치이므로 " +
                "사망을 방지하지 않습니다."
            );

            return false;
        }

        /*
         * 이미 사망 방지가 발동하여 회복을 기다리고 있다면
         * 다시 예약하지 않습니다.
         */
        if (isWaitingForHeal)
        {
            Debug.LogWarning(
                "[RIPController] 이미 다음 적 턴 회복을 " +
                "기다리고 있습니다."
            );

            return true;
        }

        isWaitingForHeal = true;

        Debug.Log(
            $"[RIPController] 안식 발동 : " +
            $"체력 1로 버티고 다음 적 턴에 " +
            $"{reviveHealAmount} 회복 예정"
        );

        return true;
    }

    /// <summary>
    /// 다음 적 턴에 회복 효과를 실행할 수 있는지 반환합니다.
    /// </summary>
    public bool CanProcessPendingHeal()
    {
        return
            isWaitingForHeal &&
            !HasReachedMaxRIP;
    }

    /// <summary>
    /// 예약된 회복량을 반환합니다.
    /// 회복 대기 상태가 아니라면 0을 반환합니다.
    /// </summary>
    public int GetPendingHealAmount()
    {
        if (!CanProcessPendingHeal())
        {
            return 0;
        }

        return Mathf.Max(
            0,
            reviveHealAmount
        );
    }

    /// <summary>
    /// 예약된 회복이 완료됐음을 기록합니다.
    /// 중복 회복을 방지하기 위해 대기 상태를 해제합니다.
    /// </summary>
    public void CompletePendingHeal()
    {
        if (!isWaitingForHeal)
        {
            return;
        }

        isWaitingForHeal = false;

        Debug.Log(
            "[RIPController] 안식 회복 처리 완료"
        );
    }

    /// <summary>
    /// 현재 안식 상태를 초기 상태로 되돌립니다.
    /// 새로운 전투 또는 테스트 초기화에 사용합니다.
    /// </summary>
    public void ResetState()
    {
        Initialize();
    }
}