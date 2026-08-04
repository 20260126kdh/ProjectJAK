using System;
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

    [Header("테크니션 패시브 누적")]

    [Tooltip(
    "테크니션이 이 적에게 부여한 작살 스택의 " +
    "4단위 계산 후 남은 누적값입니다."
)]
    [SerializeField]
    [Min(0)]
    private int technicianHarpoonApplyRemainder;

    /// <summary>
    /// 클래스 패시브를 확인하는 컨트롤러입니다.
    /// </summary>
    private ClassPassiveController classPassiveController;

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
    /// 현재 작살 스택이 변경됐을 때 호출됩니다.
    /// 변경된 현재 스택을 전달합니다.
    /// </summary>
    public event Action<int> HarpoonStackChanged;

    /// <summary>
    /// 현재 작살 스택이 한 개 이상 있는지 반환합니다.
    /// </summary>
    public bool HasHarpoonStack =>
        currentHarpoonStack > 0;

    private void Awake()
    {
        classPassiveController =
            FindFirstObjectByType<ClassPassiveController>();

        ResetHarpoonStack();
    }

    /// <summary>
    /// 지정한 수치만큼 작살 스택을 추가합니다.
    ///
    /// 테크니션은 이 적에게 작살 스택을
    /// 누적 4 부여할 때마다 추가로 1을 부여합니다.
    ///
    /// 예:
    /// - 4 부여 → 총 5 부여
    /// - 3 부여 후 1 부여 → 두 번째 부여에서 추가 1
    /// - 8 부여 → 추가 2
    /// </summary>
    public int AddHarpoonStack(int amount)
    {
        if (amount <= 0)
        {
            return 0;
        }

        int baseApplyAmount =
            amount;

        int passiveBonusAmount =
            CalculateTechnicianBonusStack(
                baseApplyAmount
            );

        int finalApplyAmount =
            baseApplyAmount +
            passiveBonusAmount;

        int previousStack =
            currentHarpoonStack;

        currentHarpoonStack =
            Mathf.Min(
                maxHarpoonStack,
                currentHarpoonStack + finalApplyAmount
            );

        int actualAddedAmount =
            currentHarpoonStack - previousStack;

        NotifyHarpoonStackChanged();

        Debug.Log(
            $"[HarpoonStackController] 작살 스택 추가 : " +
            $"{previousStack} → {currentHarpoonStack} / " +
            $"기본 부여 {baseApplyAmount} / " +
            $"테크니션 추가 {passiveBonusAmount} / " +
            $"실제 추가 {actualAddedAmount}",
            this
        );

        return actualAddedAmount;
    }

    /// <summary>
    /// 테크니션이 작살 스택을 누적 4 부여할 때마다
    /// 추가로 부여할 작살 스택 수를 계산합니다.
    ///
    /// 계산에 사용하고 남은 수치는
    /// 이 적에게 개별적으로 유지됩니다.
    /// </summary>
    private int CalculateTechnicianBonusStack(
        int appliedAmount)
    {
        if (appliedAmount <= 0)
        {
            return 0;
        }

        if (classPassiveController == null)
        {
            classPassiveController =
                FindFirstObjectByType<ClassPassiveController>();
        }

        if (classPassiveController == null)
        {
            return 0;
        }

        if (!classPassiveController.IsTechnician())
        {
            technicianHarpoonApplyRemainder = 0;
            return 0;
        }

        technicianHarpoonApplyRemainder +=
            appliedAmount;

        int bonusAmount =
            technicianHarpoonApplyRemainder / 4;

        technicianHarpoonApplyRemainder %=
            4;

        if (bonusAmount > 0)
        {
            Debug.Log(
                $"[HarpoonStackController] 테크니션 패시브 발동 / " +
                $"추가 작살 스택: {bonusAmount} / " +
                $"남은 부여 누적: {technicianHarpoonApplyRemainder}",
                this
            );
        }

        return bonusAmount;
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
    /// 현재 작살 스택을 기준으로 추가 피해를 계산합니다.
    ///
    /// 일반 클래스는 추가 피해 발동 후 스택 1을 소비합니다.
    /// 테크니션은 패시브 효과로 스택을 소비하지 않습니다.
    /// </summary>
    public int CalculateBonusDamageAndConsume()
    {
        if (currentHarpoonStack <= 0)
        {
            return 0;
        }

        int stackBeforeActivation =
            currentHarpoonStack;

        int bonusDamage =
            CalculateBonusDamage();

        bool preventConsumption =
            ShouldPreventHarpoonConsumption();

        if (!preventConsumption)
        {
            currentHarpoonStack =
                Mathf.Max(
                    0,
                    currentHarpoonStack - 1
                );

            NotifyHarpoonStackChanged();
        }

        Debug.Log(
            $"[HarpoonStackController] 작살 발동 : " +
            $"스택 {stackBeforeActivation} / " +
            $"추가 피해 {bonusDamage} / " +
            $"소비 여부 {!preventConsumption} / " +
            $"남은 스택 {currentHarpoonStack}",
            this
        );

        return bonusDamage;
    }

    /// <summary>
    /// 현재 클래스 패시브에 의해 작살 스택 소비가
    /// 방지되는지 반환합니다.
    /// </summary>
    private bool ShouldPreventHarpoonConsumption()
    {
        if (classPassiveController == null)
        {
            classPassiveController =
                FindFirstObjectByType<ClassPassiveController>();
        }

        if (classPassiveController == null)
        {
            return false;
        }

        return classPassiveController
            .ShouldPreventHarpoonConsumption();
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

        NotifyHarpoonStackChanged();

        Debug.Log(
            $"[HarpoonStackController] 작살 스택 제거 : " +
            $"{previousStack} → {currentHarpoonStack}",
            this
        );

        return removedAmount;
    }

    /// <summary>
    /// 현재 작살 스택과 테크니션 부여 누적값을 초기화합니다.
    /// </summary>
    public void ResetHarpoonStack()
    {
        currentHarpoonStack = 0;
        technicianHarpoonApplyRemainder = 0;

        NotifyHarpoonStackChanged();

        Debug.Log(
            "[HarpoonStackController] 작살 스택 초기화",
            this
        );
    }

    /// <summary>
    /// 현재 작살 스택을 UI 등 외부 시스템에 알립니다.
    /// </summary>
    private void NotifyHarpoonStackChanged()
    {
        HarpoonStackChanged?.Invoke(
            currentHarpoonStack
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

        technicianHarpoonApplyRemainder =
            Mathf.Clamp(
                technicianHarpoonApplyRemainder,
                0,
                3
            );
    }
#endif
}