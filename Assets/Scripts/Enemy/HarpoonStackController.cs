using UnityEngine;

/// <summary>
/// 적에게 적용된 작살 스택을 관리합니다.
///
/// 작살 스택 규칙:
/// - 최대 999까지 중첩됩니다.
/// - 턴이 지나도 유지됩니다.
/// - 적이 새로 생성되면 0으로 시작합니다.
/// - 작살 추가 피해는 현재 스택 × 1.5이며,
///   계산 결과의 소수점은 버립니다.
/// - 추가 피해가 발동한 뒤 작살 스택 1을 소비합니다.
/// </summary>
public class HarpoonStackController : MonoBehaviour
{
    [Header("최대 작살 스택")]
    [SerializeField]
    [Min(1)]
    private int maxHarpoonStack = 999;

    [Header("현재 작살 스택")]
    [SerializeField]
    [Min(0)]
    private int currentHarpoonStack;

    /// <summary>
    /// 현재 작살 스택입니다.
    /// </summary>
    public int CurrentHarpoonStack =>
        currentHarpoonStack;

    /// <summary>
    /// 최대 작살 스택입니다.
    /// </summary>
    public int MaxHarpoonStack =>
        maxHarpoonStack;

    /// <summary>
    /// 현재 작살 스택이 한 개 이상 있는지 반환합니다.
    /// </summary>
    public bool HasHarpoonStack =>
        currentHarpoonStack > 0;

    private void Awake()
    {
        ResetHarpoonStack();
    }

    /// <summary>
    /// 지정한 수치만큼 작살 스택을 추가합니다.
    /// 최대 스택을 초과하지 않습니다.
    /// </summary>
    public int AddHarpoonStack(int amount)
    {
        if (amount <= 0)
        {
            return 0;
        }

        int previousStack =
            currentHarpoonStack;

        currentHarpoonStack =
            Mathf.Min(
                maxHarpoonStack,
                currentHarpoonStack + amount
            );

        int actualAddedAmount =
            currentHarpoonStack - previousStack;

        Debug.Log(
            $"[HarpoonStackController] 작살 스택 추가 : " +
            $"{previousStack} → {currentHarpoonStack} / " +
            $"실제 추가 {actualAddedAmount}",
            this
        );

        return actualAddedAmount;
    }

    /// <summary>
    /// 현재 작살 스택을 기준으로
    /// 작살 추가 피해를 계산합니다.
    ///
    /// 추가 피해 = 현재 작살 스택 × 1.5
    /// 소수점은 버립니다.
    /// </summary>
    public int CalculateBonusDamage()
    {
        if (currentHarpoonStack <= 0)
        {
            return 0;
        }

        int bonusDamage =
            Mathf.FloorToInt(
                currentHarpoonStack * 1.5f
            );

        return Mathf.Max(
            0,
            bonusDamage
        );
    }

    /// <summary>
    /// 현재 작살 스택을 기준으로 추가 피해를 계산한 뒤
    /// 작살 스택 1을 소비합니다.
    ///
    /// 반환값은 이번에 적용할 작살 추가 피해입니다.
    /// </summary>
    public int CalculateBonusDamageAndConsume()
    {
        if (currentHarpoonStack <= 0)
        {
            return 0;
        }

        int stackBeforeConsume =
            currentHarpoonStack;

        int bonusDamage =
            CalculateBonusDamage();

        currentHarpoonStack =
            Mathf.Max(
                0,
                currentHarpoonStack - 1
            );

        Debug.Log(
            $"[HarpoonStackController] 작살 발동 : " +
            $"스택 {stackBeforeConsume} / " +
            $"추가 피해 {bonusDamage} / " +
            $"남은 스택 {currentHarpoonStack}",
            this
        );

        return bonusDamage;
    }

    /// <summary>
    /// 작살 스택을 지정한 수치만큼 제거합니다.
    /// 실제 제거된 수치를 반환합니다.
    /// </summary>
    public int RemoveHarpoonStack(int amount)
    {
        if (amount <= 0 ||
            currentHarpoonStack <= 0)
        {
            return 0;
        }

        int previousStack =
            currentHarpoonStack;

        currentHarpoonStack =
            Mathf.Max(
                0,
                currentHarpoonStack - amount
            );

        int removedAmount =
            previousStack - currentHarpoonStack;

        Debug.Log(
            $"[HarpoonStackController] 작살 스택 제거 : " +
            $"{previousStack} → {currentHarpoonStack}",
            this
        );

        return removedAmount;
    }

    /// <summary>
    /// 작살 스택을 0으로 초기화합니다.
    /// </summary>
    public void ResetHarpoonStack()
    {
        currentHarpoonStack = 0;

        Debug.Log(
            "[HarpoonStackController] 작살 스택 초기화",
            this
        );
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        maxHarpoonStack =
            Mathf.Max(
                1,
                maxHarpoonStack
            );

        currentHarpoonStack =
            Mathf.Clamp(
                currentHarpoonStack,
                0,
                maxHarpoonStack
            );
    }
#endif
}