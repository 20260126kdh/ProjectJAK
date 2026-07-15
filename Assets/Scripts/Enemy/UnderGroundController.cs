using UnityEngine;

/// <summary>
/// 아스피도켈 전용 가라앉는 섬과 침몰 상태를 관리합니다.
///
/// 가라앉는 섬:
/// - 전투 시작 시 침몰 수치 10으로 시작합니다.
/// - 플레이어 턴 시작마다 침몰 수치가 4 증가합니다.
///
/// 침몰:
/// - 체력이 0이 되었을 때 즉시 사망하지 않고 침몰 상태로 전환됩니다.
/// - 다음 적 턴에 저장된 침몰 수치만큼 모든 상대에게 피해를 주고 사망합니다.
///
/// 현재 단계에서는 상태와 침몰 수치만 관리하며,
/// Enemy 및 TurnManager 연결은 이후 단계에서 진행합니다.
/// </summary>
public class UnderGroundController : MonoBehaviour
{
    [Header("침몰 시작 수치")]
    [SerializeField]
    private int initialSinkValue = 10;

    [Header("턴마다 증가하는 침몰 수치")]
    [SerializeField]
    private int sinkIncreasePerTurn = 4;

    [Header("현재 침몰 수치")]
    [SerializeField]
    private int currentSinkValue;

    [Header("현재 가라앉는 섬 상태 여부")]
    [SerializeField]
    private bool isUnderGround;

    [Header("현재 침몰 상태 여부")]
    [SerializeField]
    private bool isUnderWater;

    /// <summary>
    /// 현재 침몰 수치를 반환합니다.
    /// </summary>
    public int CurrentSinkValue =>
        currentSinkValue;

    /// <summary>
    /// 현재 가라앉는 섬 상태인지 반환합니다.
    /// </summary>
    public bool IsUnderGround =>
        isUnderGround;

    /// <summary>
    /// 현재 침몰 상태인지 반환합니다.
    /// </summary>
    public bool IsUnderWater =>
        isUnderWater;

    private void Awake()
    {
        Initialize();
    }

    /// <summary>
    /// 전투 시작 상태로 초기화합니다.
    /// 가라앉는 섬 상태로 시작하며 침몰 수치를 설정합니다.
    /// </summary>
    public void Initialize()
    {
        currentSinkValue =
            Mathf.Max(0, initialSinkValue);

        isUnderGround = true;
        isUnderWater = false;

        Debug.Log(
            $"[UnderGroundController] 가라앉는 섬 시작 : " +
            $"침몰 {currentSinkValue}"
        );
    }

    /// <summary>
    /// 가라앉는 섬 상태일 때 침몰 수치를 증가시킵니다.
    /// 침몰 상태로 전환된 뒤에는 증가하지 않습니다.
    /// </summary>
    public void IncreaseSink()
    {
        if (!isUnderGround)
        {
            return;
        }

        if (isUnderWater)
        {
            return;
        }

        int increaseAmount =
            Mathf.Max(0, sinkIncreasePerTurn);

        currentSinkValue += increaseAmount;

        Debug.Log(
            $"[UnderGroundController] 침몰 증가 : " +
            $"+{increaseAmount} / " +
            $"현재 침몰 {currentSinkValue}"
        );
    }

    /// <summary>
    /// 체력이 0이 되었을 때 침몰 상태로 전환을 시도합니다.
    ///
    /// 전환에 성공하면 true를 반환합니다.
    /// 이미 침몰 상태이거나 가라앉는 섬 상태가 아니면
    /// false를 반환합니다.
    /// </summary>
    public bool TryEnterUnderWater()
    {
        if (!isUnderGround)
        {
            return false;
        }

        if (isUnderWater)
        {
            return false;
        }

        isUnderGround = false;
        isUnderWater = true;

        Debug.Log(
            $"[UnderGroundController] 침몰 상태 전환 : " +
            $"다음 적 턴에 침몰 {currentSinkValue}만큼 " +
            "모든 상대에게 피해를 줍니다."
        );

        return true;
    }

    /// <summary>
    /// 침몰 폭발을 실행할 수 있는 상태인지 반환합니다.
    /// 실제 피해와 사망 처리는 이후 Enemy 및 TurnManager에서
    /// 연결합니다.
    /// </summary>
    public bool CanExecuteUnderWaterExplosion()
    {
        return isUnderWater;
    }

    /// <summary>
    /// 침몰 폭발에 사용할 피해량을 반환합니다.
    /// </summary>
    public int GetUnderWaterDamage()
    {
        if (!isUnderWater)
        {
            return 0;
        }

        return Mathf.Max(
            0,
            currentSinkValue
        );
    }

    /// <summary>
    /// 침몰 폭발 처리가 끝났음을 기록합니다.
    /// 중복 발동을 방지하기 위해 침몰 상태를 해제합니다.
    /// </summary>
    public void CompleteUnderWaterExplosion()
    {
        if (!isUnderWater)
        {
            return;
        }

        isUnderWater = false;

        Debug.Log(
            "[UnderGroundController] 침몰 폭발 처리 완료"
        );
    }

    /// <summary>
    /// 현재 상태와 침몰 수치를 초기 상태로 되돌립니다.
    /// 테스트 또는 새로운 전투 시작 시 사용할 수 있습니다.
    /// </summary>
    public void ResetState()
    {
        Initialize();
    }
}