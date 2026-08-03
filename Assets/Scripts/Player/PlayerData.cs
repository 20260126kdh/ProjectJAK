using UnityEngine;

/// <summary>
/// 플레이어의 기본 데이터를 관리하는 클래스
///
/// 관리 내용
/// - 선택한 클래스 정보
/// - 체력 관리
/// - (추후) 기타 플레이어 정보
///
/// 이 클래스는 데이터를 저장하는 역할만 하며,
/// 게임의 로직은 처리하지 않는다.
/// </summary>
public class PlayerData : MonoBehaviour
{
    #region Player Class

    [Header("Player Class")]

    [Tooltip("현재 선택한 클래스")]
    [SerializeField]
    private PlayerClass playerClass = PlayerClass.None;

    #endregion

    #region HP

    [Header("HP")]

    [Tooltip("현재 체력")]
    [SerializeField]
    private int currentHP = 100;

    [Tooltip("최대 체력")]
    [SerializeField]
    private int maxHP = 100;

    #endregion

    #region Property

    /// <summary>
    /// 현재 선택한 클래스
    /// </summary>
    public PlayerClass PlayerClass => playerClass;

    /// <summary>
    /// 현재 체력
    /// </summary>
    public int CurrentHP => currentHP;

    /// <summary>
    /// 최대 체력
    /// </summary>
    public int MaxHP => maxHP;

    #endregion

    #region Public Methods

    /// <summary>
    /// 플레이어 클래스를 설정한다.
    /// </summary>
    public void SetClass(PlayerClass newClass)
    {
        playerClass = newClass;

        Debug.Log($"플레이어 클래스 설정 : {playerClass}");
    }

    /// <summary>
    /// 플레이어의 최대 체력과 현재 체력을 설정한다.
    /// 게임 시작 시 클래스의 초기 체력 설정에 사용된다.
    /// </summary>
    public void SetHP(int hp)
    {
        maxHP = hp;
        currentHP = hp;
    }

    /// <summary>
    /// 저장 데이터에서 플레이어 체력을 복원합니다.
    ///
    /// 최대 체력과 현재 체력을 각각 설정하며,
    /// 현재 체력은 0부터 최대 체력 범위로 제한합니다.
    /// </summary>
    /// <param name="savedCurrentHP">복원할 현재 체력</param>
    /// <param name="savedMaxHP">복원할 최대 체력</param>
    public void RestoreHP(
        int savedCurrentHP,
        int savedMaxHP)
    {
        if (savedMaxHP <= 0)
        {
            Debug.LogError(
                $"[PlayerData] 복원할 최대 체력이 올바르지 않습니다: " +
                $"{savedMaxHP}"
            );

            return;
        }

        maxHP = savedMaxHP;

        currentHP =
            Mathf.Clamp(
                savedCurrentHP,
                0,
                maxHP
            );

        Debug.Log(
            $"[PlayerData] 체력 복원 완료: " +
            $"{currentHP}/{maxHP}"
        );
    }

    /// <summary>
    /// 피해를 받는다.
    /// </summary>
    public void TakeDamage(int damage)
    {
        currentHP -= damage;

        if (currentHP < 0)
            currentHP = 0;
    }

    /// <summary>
    /// 체력을 회복한다.
    /// </summary>
    public void Heal(int amount)
    {
        currentHP += amount;

        if (currentHP > maxHP)
            currentHP = maxHP;
    }

    #endregion
}