using UnityEngine;

/// <summary>
/// 플레이어의 현재 데이터를 저장하는 클래스
///
/// 담당 역할
/// - 선택한 클래스 저장
/// - 체력 관리
/// - (추후) 덱 정보 저장
///
/// 이 클래스는 데이터를 저장하는 역할만 하며,
/// 게임의 진행을 제어하지 않는다.
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

        Debug.Log($"플레이어 클래스 선택 : {playerClass}");
    }

    /// <summary>
    /// 플레이어의 최대 체력과 현재 체력을 설정한다.
    /// 게임 시작 시 클래스별 초기 체력 설정에 사용된다.
    /// </summary>
    public void SetHP(int hp)
    {
        maxHP = hp;
        currentHP = hp;
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