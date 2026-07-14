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

    [Header("적 현재 방어도")]
    [SerializeField]
    private int currentBlock;

    [Header("다음 적 턴 기절 여부")]
    [SerializeField]
    private bool isStunnedNextTurn;

    [Header("독오름 다음 부여 효과")]
    [SerializeField]
    private bool applyParalyzeNext = true;

    [Header("불어터진 피부 다음 부여 효과")]
    [SerializeField]
    private bool applyNoBlockNext = true;

    [Header("Battle Manager")]
    [SerializeField]
    private BattleManager battleManager;

    public int CurrentHP => currentHP;
    public int MaxHP => maxHP;

    public int CurrentBlock => currentBlock;
    public bool IsStunnedNextTurn => isStunnedNextTurn;

    private void Awake()
    {
        currentHP = maxHP;
        applyParalyzeNext = true;
        applyNoBlockNext = true;
        isStunnedNextTurn = false;
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

        if (isStunnedNextTurn)
        {
            isStunnedNextTurn = false;

            Debug.Log(
                $"[Enemy] {name} 기절 발동 : " +
                "이번 적 턴 행동을 건너뜁니다."
            );

            return;
        }

        Debug.Log(
            $"[Enemy] 플레이어에게 {finalDamage} 피해"
        );

        playerCombat.ReceiveAttackDamage(finalDamage);

        ProcessDToxinSwitch(playerCombat);
        ProcessFFesteredSkin(playerCombat);
    }

    /// <summary>
    /// 적이 DToxinSwitch 패시브를 보유하고 있다면
    /// 공격 후 플레이어에게 Paralyze와 Toxic을 번갈아 부여합니다.
    /// 패시브의 value를 부여 수치로 사용합니다.
    /// </summary>
    private void ProcessDToxinSwitch(
        PlayerCombat playerCombat)
    {
        if (playerCombat == null)
        {
            return;
        }

        StatusEffectHandler enemyStatusEffectHandler =
            GetComponent<StatusEffectHandler>();

        if (enemyStatusEffectHandler == null)
        {
            return;
        }

        if (!enemyStatusEffectHandler.HasStatusEffect(
            StatusEffectType.DToxinSwitch
        ))
        {
            return;
        }

        StatusEffectHandler playerStatusEffectHandler =
            playerCombat.GetComponent<StatusEffectHandler>();

        if (playerStatusEffectHandler == null)
        {
            Debug.LogWarning(
                "[Enemy] 플레이어에게 StatusEffectHandler가 없어 " +
                "독오름 효과를 부여할 수 없습니다."
            );

            return;
        }

        int applyValue =
            Mathf.Max(
                1,
                enemyStatusEffectHandler.GetStatusValue(
                    StatusEffectType.DToxinSwitch
                )
            );

        if (applyParalyzeNext)
        {
            playerStatusEffectHandler.AddStatusEffect(
                StatusEffectType.Paralyze,
                applyValue,
                0,
                true
            );

            Debug.Log(
                $"[Enemy] 독오름 발동 : " +
                $"플레이어에게 마비 {applyValue} 부여"
            );
        }
        else
        {
            playerStatusEffectHandler.AddStatusEffect(
                StatusEffectType.Toxic,
                applyValue,
                0,
                false
            );

            Debug.Log(
                $"[Enemy] 독오름 발동 : " +
                $"플레이어에게 중독 {applyValue} 부여"
            );
        }

        applyParalyzeNext = !applyParalyzeNext;
    }

    /// <summary>
    /// 적이 FFesteredSkin 패시브를 보유하고 있다면
    /// 정상 공격 후 플레이어에게 NoBlock과 Broken을
    /// 번갈아 부여합니다.
    /// 패시브의 value를 부여 수치로 사용합니다.
    /// </summary>
    private void ProcessFFesteredSkin(
        PlayerCombat playerCombat)
    {
        if (playerCombat == null)
        {
            return;
        }

        StatusEffectHandler enemyStatusEffectHandler =
            GetComponent<StatusEffectHandler>();

        if (enemyStatusEffectHandler == null)
        {
            return;
        }

        if (!enemyStatusEffectHandler.HasStatusEffect(
            StatusEffectType.FFesteredSkin
        ))
        {
            return;
        }

        StatusEffectHandler playerStatusEffectHandler =
            playerCombat.GetComponent<StatusEffectHandler>();

        if (playerStatusEffectHandler == null)
        {
            Debug.LogWarning(
                "[Enemy] 플레이어에게 StatusEffectHandler가 없어 " +
                "불어터진 피부 효과를 부여할 수 없습니다."
            );

            return;
        }

        int applyValue =
            Mathf.Max(
                1,
                enemyStatusEffectHandler.GetStatusValue(
                    StatusEffectType.FFesteredSkin
                )
            );

        if (applyNoBlockNext)
        {
            playerStatusEffectHandler.AddStatusEffect(
                StatusEffectType.NoBlock,
                applyValue,
                1,
                false
            );

            Debug.Log(
                $"[Enemy] 불어터진 피부 발동 : " +
                $"플레이어에게 미끄러짐 {applyValue} 부여"
            );
        }
        else
        {
            playerStatusEffectHandler.AddStatusEffect(
                StatusEffectType.Broken,
                applyValue,
                1,
                false
            );

            Debug.Log(
                $"[Enemy] 불어터진 피부 발동 : " +
                $"플레이어에게 부러짐 {applyValue} 부여"
            );
        }

        applyNoBlockNext = !applyNoBlockNext;
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
        int remainingDamage = damage;
        int actualBlockDamage = 0;

        /*
         * 피해를 받기 전 방어도가 있었는지 저장합니다.
         * KShellguard는 이번 피해로 방어도가 전부 소진된 경우에만
         * 발동해야 합니다.
         */
        bool hadBlockBeforeDamage =
            currentBlock > 0;

        /*
         * 적 방어도가 체력보다 먼저 피해를 받습니다.
         */
        if (remainingDamage > 0 &&
            currentBlock > 0)
        {
            actualBlockDamage =
                Mathf.Min(currentBlock, remainingDamage);

            currentBlock -= actualBlockDamage;
            remainingDamage -= actualBlockDamage;

            Debug.Log(
                $"[Enemy] 방어도 피해 흡수 : " +
                $"{actualBlockDamage} / " +
                $"남은 방어도 {currentBlock} / " +
                $"남은 피해 {remainingDamage}"
            );
        }

        /*
         * 피해 전에는 방어도가 있었고,
         * 이번 피해로 방어도가 정확히 0이 되었다면
         * KShellguard를 발동합니다.
         */
        if (hadBlockBeforeDamage &&
            currentBlock <= 0)
        {
            TryActivateKShellguard();
        }

        /*
         * 방어도를 통과한 피해만 체력에 적용합니다.
         */
        int actualHealthDamage =
            Mathf.Min(currentHP, remainingDamage);

        currentHP -= actualHealthDamage;

        if (currentHP < 0)
        {
            currentHP = 0;
        }

        Debug.Log(
            $"[Enemy] 체력 피해 : {actualHealthDamage} / " +
            $"현재 체력 : {currentHP}"
        );

        /*
         * 흡혈 등에서는 방어도 피해가 아닌
         * 실제 체력 감소량만 피해량으로 취급합니다.
         */
        int actualDamage = actualHealthDamage;

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
    /// 적이 KShellguard 패시브를 보유하고 있다면
    /// 다음 적 턴 행동을 한 번 건너뛰도록 설정합니다.
    /// </summary>
    private void TryActivateKShellguard()
    {
        StatusEffectHandler statusEffectHandler =
            GetComponent<StatusEffectHandler>();

        if (statusEffectHandler == null)
        {
            return;
        }

        if (!statusEffectHandler.HasStatusEffect(
            StatusEffectType.KShellguard
        ))
        {
            return;
        }

        isStunnedNextTurn = true;

        Debug.Log(
            $"[Enemy] 등껍질 방패 발동 : " +
            $"{name}은 다음 적 턴에 행동하지 않습니다."
        );
    }

    /// <summary>
    /// 적이 방어도를 획득합니다.
    /// </summary>
    public void GainBlock(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        currentBlock += amount;

        Debug.Log(
            $"[Enemy] 방어도 획득 : " +
            $"+{amount} / 현재 방어도 {currentBlock}"
        );
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