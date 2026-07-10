using UnityEngine;

/// <summary>
/// 전투 중 적의 체력을 관리하는 임시 Enemy 클래스입니다.
/// 데미지 받기, 사망 처리, 적 턴 행동을 담당합니다.
/// </summary>
public class Enemy : MonoBehaviour
{
    [Header("적 최대 체력")]
    [SerializeField]
    private int maxHP = 30;

    [Header("적 현재 체력")]
    [SerializeField]
    private int currentHP;

    [Header("적 기본 공격력")]
    [SerializeField]
    private int basicAttackDamage = 5;

    [Header("Battle Manager")]
    [SerializeField]
    private BattleManager battleManager;

    public int CurrentHP => currentHP;
    public int MaxHP => maxHP;

    private void Awake()
    {
        currentHP = maxHP;
    }

    private void Start()
    {
        if (battleManager == null)
        {
            battleManager = FindFirstObjectByType<BattleManager>();
        }
    }

    /// <summary>
    /// 적의 턴 행동을 실행합니다.
    /// 약화 효과를 반영한 후 플레이어에게 피해를 줍니다.
    /// </summary>
    public void TakeTurn(PlayerCombat playerCombat)
    {
        if (playerCombat == null)
        {
            Debug.LogWarning("[Enemy] PlayerCombat이 없습니다.");
            return;
        }

        if (currentHP <= 0)
        {
            return;
        }

        int finalDamage = basicAttackDamage;

        StatusEffectHandler statusEffectHandler =
            GetComponent<StatusEffectHandler>();

        if (statusEffectHandler != null &&
            statusEffectHandler.HasStatusEffect(StatusEffectType.Weaken))
        {
            int reducedDamage =
                Mathf.FloorToInt(finalDamage * 0.6f);

            Debug.Log(
                $"[Enemy] 약화 적용 : " +
                $"{finalDamage} → {reducedDamage}"
            );

            finalDamage = reducedDamage;
        }

        Debug.Log(
            $"[Enemy] 플레이어에게 {finalDamage} 피해"
        );

        playerCombat.ReceiveAttackDamage(finalDamage);
    }

    /// <summary>
    /// 적에게 피해를 적용하고 실제로 감소한 체력량을 반환합니다.
    /// 취약 효과와 적의 남은 체력을 반영합니다.
    /// </summary>
    public int TakeDamage(int damage)
    {
        if (currentHP <= 0)
        {
            return 0;
        }

        if (damage < 0)
        {
            damage = 0;
        }

        StatusEffectHandler statusEffectHandler =
            GetComponent<StatusEffectHandler>();

        if (statusEffectHandler != null &&
            statusEffectHandler.HasStatusEffect(
                StatusEffectType.Vulnerable
            ))
        {
            int increasedDamage =
                Mathf.FloorToInt(damage * 1.4f);

            Debug.Log(
                $"[Enemy] 취약 적용 : " +
                $"{damage} → {increasedDamage}"
            );

            damage = increasedDamage;
        }

        /*
         * 적의 남은 체력을 초과한 피해는 실제 피해량에 포함하지 않습니다.
         *
         * 예:
         * 적의 현재 체력 3
         * 최종 피해량 10
         * 실제 피해량 3
         */
        int actualDamage = Mathf.Min(currentHP, damage);

        currentHP -= actualDamage;

        if (currentHP < 0)
        {
            currentHP = 0;
        }

        Debug.Log(
            $"[Enemy] 데미지 받음 : {actualDamage} / " +
            $"현재 체력 : {currentHP}"
        );

        if (currentHP <= 0)
        {
            Die();
        }

        return actualDamage;
    }

    /// <summary>
    /// 적의 사망 처리를 실행합니다.
    /// </summary>
    private void Die()
    {
        Debug.Log("[Enemy] 적 사망");

        if (battleManager != null)
        {
            battleManager.CheckBattleEnd();
        }

        gameObject.SetActive(false);
    }
}