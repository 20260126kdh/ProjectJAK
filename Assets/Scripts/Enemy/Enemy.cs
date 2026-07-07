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

        StatusEffectHandler statusEffectHandler = GetComponent<StatusEffectHandler>();

        if (statusEffectHandler != null && statusEffectHandler.HasStatusEffect(StatusEffectType.Weaken))
        {
            int reducedDamage = Mathf.FloorToInt(finalDamage * 0.6f);

            Debug.Log($"[Enemy] 약화 적용 : {finalDamage} → {reducedDamage}");

            finalDamage = reducedDamage;
        }

        Debug.Log($"[Enemy] 플레이어에게 {finalDamage} 피해");

        playerCombat.LoseHealth(finalDamage);
    }

    public void TakeDamage(int damage)
    {
        if (currentHP <= 0)
        {
            return;
        }

        StatusEffectHandler statusEffectHandler = GetComponent<StatusEffectHandler>();

        if (statusEffectHandler != null && statusEffectHandler.HasStatusEffect(StatusEffectType.Vulnerable))
        {
            int increasedDamage = Mathf.FloorToInt(damage * 1.4f);

            Debug.Log($"[Enemy] 취약 적용 : {damage} → {increasedDamage}");

            damage = increasedDamage;
        }

        currentHP -= damage;

        if (currentHP < 0)
        {
            currentHP = 0;
        }

        Debug.Log($"[Enemy] 데미지 받음 : {damage} / 현재 체력 : {currentHP}");

        if (currentHP <= 0)
        {
            Die();
        }
    }

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