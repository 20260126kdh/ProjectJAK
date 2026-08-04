using UnityEngine;

/// <summary>
/// 플레이어가 선택한 클래스의 기본 패시브를 관리합니다.
///
/// 담당 기능:
/// - 전투 시작 시 클래스 패시브 실행
/// - 전투 종료 시 클래스 패시브 실행
/// - 이후 작살 및 선원 관련 클래스 패시브 확장
/// </summary>
public class ClassPassiveController : MonoBehaviour
{
    [Header("피지크 패시브")]

    [Tooltip("피지크가 전투 종료 시 추가로 회복하는 체력입니다.")]
    [SerializeField]
    private int physiqueBattleEndHealAmount = 7;

    [Header("캡틴 패시브")]

    [Tooltip("전투 시작 시 캡틴이 소환하는 선원 수입니다.")]
    [SerializeField]
    [Min(0)]
    private int captainStartingCrewCount = 2;

    [Tooltip("캡틴 패시브로 소환되는 선원의 초기 체력입니다.")]
    [SerializeField]
    [Min(1)]
    private int captainStartingCrewHealth = 12;

    [Header("Crew Manager")]
    [SerializeField]
    private CrewManager crewManager;

    /// <summary>
    /// 현재 플레이어 클래스를 반환합니다.
    /// </summary>
    private PlayerClass GetCurrentPlayerClass()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning(
                "[ClassPassiveController] GameManager.Instance가 없습니다."
            );

            return PlayerClass.None;
        }

        if (GameManager.Instance.PlayerData == null)
        {
            Debug.LogWarning(
                "[ClassPassiveController] PlayerData가 없습니다."
            );

            return PlayerClass.None;
        }

        return GameManager.Instance.PlayerData.PlayerClass;
    }

    /// <summary>
    /// 전투 시작 시 현재 클래스의 기본 패시브를 실행합니다.
    ///
    /// 이후 캡틴 전투 시작 선원 소환 등을 이곳에 추가합니다.
    /// </summary>
    public void OnBattleStarted()
    {
        PlayerClass playerClass =
            GetCurrentPlayerClass();

        switch (playerClass)
        {
            case PlayerClass.Physique:
                /*
                 * 피지크는 전투 시작 패시브가 없습니다.
                 */
                break;

            case PlayerClass.Technician:
                /*
                 * 이후 테크니션 패시브를 연결합니다.
                 */
                break;

            case PlayerClass.Captain:
                ApplyCaptainBattleStartPassive();
                break;

            case PlayerClass.None:
            case PlayerClass.All:
            default:
                break;
        }
    }

    /// <summary>
    /// 전투 종료 시 현재 클래스의 기본 패시브를 실행합니다.
    /// </summary>
    public void OnBattleEnded()
    {
        PlayerClass playerClass =
            GetCurrentPlayerClass();

        switch (playerClass)
        {
            case PlayerClass.Physique:
                ApplyPhysiqueBattleEndPassive();
                break;

            case PlayerClass.Technician:
            case PlayerClass.Captain:
            case PlayerClass.None:
            case PlayerClass.All:
            default:
                break;
        }
    }

    /// <summary>
    /// 피지크 패시브를 실행합니다.
    ///
    /// 전투 종료 시 플레이어 체력을
    /// 설정된 수치만큼 추가로 회복합니다.
    /// </summary>
    private void ApplyPhysiqueBattleEndPassive()
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.PlayerData == null)
        {
            Debug.LogWarning(
                "[ClassPassiveController] 피지크 패시브를 적용할 " +
                "PlayerData를 찾지 못했습니다."
            );

            return;
        }

        if (physiqueBattleEndHealAmount <= 0)
        {
            Debug.LogWarning(
                "[ClassPassiveController] 피지크 전투 종료 회복량이 " +
                "0 이하입니다."
            );

            return;
        }

        PlayerData playerData =
            GameManager.Instance.PlayerData;

        int previousHP =
            playerData.CurrentHP;

        playerData.Heal(
            physiqueBattleEndHealAmount
        );

        int actualHealAmount =
            playerData.CurrentHP - previousHP;

        Debug.Log(
            $"[ClassPassiveController] 피지크 패시브 발동 / " +
            $"전투 종료 체력 회복: {actualHealAmount} / " +
            $"현재 체력: {playerData.CurrentHP}/{playerData.MaxHP}"
        );
    }

    /// <summary>
    /// 캡틴의 전투 시작 패시브를 실행합니다.
    ///
    /// 전투 첫 플레이어 턴 준비가 완료된 뒤
    /// 지정된 체력의 선원 2명을 소환합니다.
    /// </summary>
    private void ApplyCaptainBattleStartPassive()
    {
        if (crewManager == null)
        {
            crewManager =
                FindFirstObjectByType<CrewManager>();
        }

        if (crewManager == null)
        {
            Debug.LogError(
                "[ClassPassiveController] 캡틴 패시브를 실행할 " +
                "CrewManager를 찾지 못했습니다."
            );

            return;
        }

        if (captainStartingCrewCount <= 0)
        {
            Debug.LogWarning(
                "[ClassPassiveController] 캡틴 시작 선원 수가 " +
                "0 이하입니다."
            );

            return;
        }

        if (captainStartingCrewHealth <= 0)
        {
            Debug.LogWarning(
                "[ClassPassiveController] 캡틴 시작 선원 체력이 " +
                "0 이하입니다."
            );

            return;
        }

        int summonedCount = 0;

        for (int i = 0;
             i < captainStartingCrewCount;
             i++)
        {
            bool summonSucceeded =
                crewManager.SummonCrewWithHealth(
                    captainStartingCrewHealth
                );

            if (!summonSucceeded)
            {
                Debug.LogWarning(
                    $"[ClassPassiveController] 캡틴 패시브 선원 소환 중단 / " +
                    $"성공: {summonedCount}명 / " +
                    $"목표: {captainStartingCrewCount}명"
                );

                break;
            }

            summonedCount++;
        }

        Debug.Log(
            $"[ClassPassiveController] 캡틴 패시브 발동 / " +
            $"선원 소환: {summonedCount}명 / " +
            $"초기 체력: {captainStartingCrewHealth}"
        );
    }

    /// <summary>
    /// 현재 클래스가 테크니션인지 반환합니다.
    /// </summary>
    public bool IsTechnician()
    {
        return GetCurrentPlayerClass() ==
               PlayerClass.Technician;
    }

    /// <summary>
    /// 테크니션이 공격할 때 작살 스택을
    /// 소비하지 않아야 하는지 반환합니다.
    /// </summary>
    public bool ShouldPreventHarpoonConsumption()
    {
        return IsTechnician();
    }

    /// <summary>
    /// 현재 클래스가 캡틴인지 반환합니다.
    /// </summary>
    public bool IsCaptain()
    {
        return GetCurrentPlayerClass() ==
               PlayerClass.Captain;
    }
}