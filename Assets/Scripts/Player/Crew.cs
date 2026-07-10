using UnityEngine;

/// <summary>
/// 캡틴이 소환하는 선원 1명의 전투 데이터를 관리합니다.
/// 체력 초기화, 턴 성장, 피해 처리, 사망 처리를 담당합니다.
/// </summary>
public class Crew : MonoBehaviour
{
    [Header("소환수 현재 체력")]
    [SerializeField]
    private int currentHP;

    [Header("소환수 최대 체력")]
    [SerializeField]
    private int maxHP;

    /// <summary>
    /// 이 소환수를 관리하는 CrewManager입니다.
    /// </summary>
    private CrewManager crewManager;

    /// <summary>
    /// 현재 체력을 반환합니다.
    /// </summary>
    public int CurrentHP => currentHP;

    /// <summary>
    /// 최대 체력을 반환합니다.
    /// </summary>
    public int MaxHP => maxHP;

    /// <summary>
    /// 현재 살아있는지 반환합니다.
    /// </summary>
    public bool IsAlive => currentHP > 0;

    /// <summary>
    /// 소환 직후 선원을 초기화합니다.
    /// 현재 체력과 최대 체력은 모두 1로 시작합니다.
    /// </summary>
    public void Initialize(CrewManager manager)
    {
        crewManager = manager;

        maxHP = 1;
        currentHP = 1;

        Debug.Log(
            $"[Crew] 소환수 초기화 : {currentHP}/{maxHP}"
        );
    }

    /// <summary>
    /// 플레이어 턴 시작 시 최대 체력과 현재 체력을
    /// 각각 1씩 증가시킵니다.
    /// </summary>
    public void GrowAtPlayerTurnStart()
    {
        if (!IsAlive)
        {
            return;
        }

        maxHP++;
        currentHP++;

        Debug.Log(
            $"[Crew] 턴 성장 : {currentHP}/{maxHP}"
        );
    }

    /// <summary>
    /// 피해를 받고 체력을 초과한 남은 피해량을 반환합니다.
    ///
    /// 예:
    /// 현재 체력 2, 받는 피해 5
    /// → 선원이 2 피해를 받고 사망
    /// → 남은 피해 3 반환
    /// </summary>
    public int TakeDamage(int damage)
    {
        if (!IsAlive)
        {
            return damage;
        }

        if (damage <= 0)
        {
            return 0;
        }

        int absorbedDamage = Mathf.Min(currentHP, damage);

        currentHP -= absorbedDamage;

        int remainingDamage = damage - absorbedDamage;

        Debug.Log(
            $"[Crew] 피해 받음 : {absorbedDamage} / " +
            $"현재 체력 : {currentHP}/{maxHP} / " +
            $"남은 피해 : {remainingDamage}"
        );

        if (currentHP <= 0)
        {
            Die();
        }

        return remainingDamage;
    }

    /// <summary>
    /// 현재 체력과 최대 체력을 지정합니다.
    /// 이후 최대 체력 설정 카드에서 사용할 수 있습니다.
    /// </summary>
    public void SetHealth(int currentHealth, int maxHealth)
    {
        maxHP = Mathf.Max(1, maxHealth);
        currentHP = Mathf.Clamp(currentHealth, 0, maxHP);

        Debug.Log(
            $"[Crew] 체력 설정 : {currentHP}/{maxHP}"
        );

        if (currentHP <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// 최대 체력을 지정하고 현재 체력이 최대 체력을 넘으면 제한합니다.
    /// </summary>
    public void SetMaxHealth(int newMaxHP)
    {
        maxHP = Mathf.Max(1, newMaxHP);

        if (currentHP > maxHP)
        {
            currentHP = maxHP;
        }

        Debug.Log(
            $"[Crew] 최대 체력 설정 : {currentHP}/{maxHP}"
        );
    }

    /// <summary>
    /// 선원의 사망 처리를 수행합니다.
    /// CrewManager 목록에서 먼저 제거한 뒤 오브젝트를 삭제합니다.
    /// </summary>
    private void Die()
    {
        currentHP = 0;

        Debug.Log("[Crew] 소환수 사망");

        if (crewManager != null)
        {
            crewManager.RemoveCrew(this);
        }
        else
        {
            Debug.LogWarning(
                "[Crew] CrewManager가 연결되지 않아 " +
                "목록 제거를 처리하지 못했습니다."
            );
        }

        Destroy(gameObject);
    }
}