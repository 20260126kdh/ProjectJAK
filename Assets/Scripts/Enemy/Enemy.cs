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
            battleManager =
                FindFirstObjectByType<BattleManager>();
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
            Debug.LogWarning(
                "[Enemy] PlayerCombat이 없습니다."
            );

            return;
        }

        /*
         * 모르바엘이 안식 효과로 사망을 방지했다면
         * 다음 적 턴 시작 시 예약된 체력 회복을 먼저 처리합니다.
         */
        ProcessRIPPendingHeal();

        /*
         * 침몰 상태라면 일반 행동보다 먼저
         * 침몰 피해를 처리한 뒤 사망합니다.
         */
        if (TryExecuteUnderWaterTurn(playerCombat))
        {
            return;
        }

        if (currentHP <= 0)
        {
            return;
        }

        /*
         * 체력이 0인 일반 적은 행동하지 않습니다.
         */
        if (currentHP <= 0)
        {
            return;
        }

        int finalDamage = basicAttackDamage;

        StatusEffectHandler statusEffectHandler =
            GetComponent<StatusEffectHandler>();

        if (statusEffectHandler != null &&
            statusEffectHandler.HasStatusEffect(
                StatusEffectType.Weaken
            ))
        {
            int reducedDamage =
                Mathf.FloorToInt(
                    finalDamage * 0.6f
                );

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
    /// UnderWater 상태라면 다음 적 턴에 침몰 피해를 실행합니다.
    ///
    /// 침몰 피해는 플레이어의 일반 피격 처리 메서드를 사용하므로
    /// 방어도 → 선원 → 플레이어 순서로 적용됩니다.
    ///
    /// 침몰 피해 처리 후 아스피도켈은 사망합니다.
    /// 침몰 효과를 실행했다면 true를 반환합니다.
    /// </summary>
    private bool TryExecuteUnderWaterTurn(
        PlayerCombat playerCombat)
    {
        UnderGroundController underGroundController =
            GetComponent<UnderGroundController>();

        if (underGroundController == null)
        {
            return false;
        }

        if (!underGroundController.CanExecuteUnderWaterExplosion())
        {
            return false;
        }

        int sinkDamage =
            underGroundController.GetUnderWaterDamage();

        Debug.Log(
            $"[Enemy] {name} 침몰 발동 : " +
            $"피해 {sinkDamage}"
        );

        /*
         * 기존 적 공격 피해 처리 방식을 사용합니다.
         *
         * 적용 순서:
         * 플레이어 방어도 → 선원 → 플레이어 체력
         */
        if (sinkDamage > 0)
        {
            playerCombat.ReceiveAttackDamage(
                sinkDamage
            );
        }

        /*
         * 중복 발동을 막기 위해 침몰 효과 완료를 기록한 뒤
         * 해당 적을 사망 처리합니다.
         */
        underGroundController
            .CompleteUnderWaterExplosion();

        Debug.Log(
            $"[Enemy] {name} 침몰 피해 처리 완료 : " +
            "보스를 사망 처리합니다."
        );

        Die();

        return true;
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
        /*
         * 이미 체력이 0인 적은 추가 피해를 받지 않습니다.
         * UnderWater 상태의 보스도 여기에 포함됩니다.
         */
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
                Mathf.FloorToInt(
                    damage * 1.4f
                );

            Debug.Log(
                $"[Enemy] 취약 적용 : " +
                $"{damage} → {increasedDamage}"
            );

            damage = increasedDamage;
        }

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
                Mathf.Min(
                    currentBlock,
                    remainingDamage
                );

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
            TryActivateProfanedHalo();
        }

        /*
         * 방어도를 통과한 피해만 체력에 적용합니다.
         */
        int actualHealthDamage =
            Mathf.Min(
                currentHP,
                remainingDamage
            );

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
            Debug.Log(
                $"[Enemy] 체력 0 도달 : {name}"
            );

            /*
             * UnderGroundController가 붙은 보스라면
             * 즉시 사망하지 않고 UnderWater 상태로 전환합니다.
             */
            bool preventedDeath =
    TryPreventDeathWithUnderGround();

            if (!preventedDeath)
            {
                preventedDeath =
                    TryPreventDeathWithRIP();
            }

            if (!preventedDeath)
            {
                Debug.Log(
                    $"[Enemy] 일반 사망 처리 실행 : {name}"
                );

                Die();
            }
        }

        return actualDamage;
    }

    /// <summary>
    /// UnderGroundController가 붙어 있는 적이 체력 0이 되었을 때
    /// 즉시 사망하지 않고 UnderWater 상태로 전환을 시도합니다.
    ///
    /// 전환에 성공하면 true를 반환하여 일반 사망을 막습니다.
    /// 컨트롤러가 없거나 전환에 실패하면 false를 반환합니다.
    /// </summary>
    private bool TryPreventDeathWithUnderGround()
    {
        Debug.Log(
            $"[Enemy] UnderGround 사망 방지 검사 시작 : {name}"
        );

        UnderGroundController underGroundController =
            GetComponent<UnderGroundController>();

        if (underGroundController == null)
        {
            Debug.LogWarning(
                $"[Enemy] UnderGroundController 없음 : {name}"
            );

            return false;
        }

        Debug.Log(
            $"[Enemy] UnderGroundController 발견 : {name} / " +
            $"UnderGround = {underGroundController.IsUnderGround} / " +
            $"UnderWater = {underGroundController.IsUnderWater} / " +
            $"침몰 수치 = {underGroundController.CurrentSinkValue}"
        );

        if (!underGroundController.TryEnterUnderWater())
        {
            Debug.LogWarning(
                $"[Enemy] UnderWater 상태 전환 실패 : {name}"
            );

            return false;
        }

        /*
         * 체력은 0으로 유지하지만 오브젝트는 비활성화하지 않습니다.
         * 이후 적 턴에서 침몰 피해를 발동할 예정입니다.
         */
        Debug.Log(
            $"[Enemy] {name} 사망 방지 성공 : " +
            $"UnderWater 상태 전환 / " +
            $"예정 침몰 피해 " +
            $"{underGroundController.CurrentSinkValue}"
        );

        return true;
    }

    /// <summary>
    /// RIPController가 붙은 적의 체력이 0이 되었을 때
    /// 안식이 최대치 미만이라면 체력 1로 생존시킵니다.
    ///
    /// 사망 방지에 성공하면 다음 적 턴 체력 회복이 예약되며
    /// true를 반환합니다.
    /// </summary>
    private bool TryPreventDeathWithRIP()
    {
        RIPController ripController =
            GetComponent<RIPController>();

        if (ripController == null)
        {
            return false;
        }

        Debug.Log(
            $"[Enemy] 안식 사망 방지 검사 : {name} / " +
            $"안식 {ripController.CurrentRIPValue}/" +
            $"{ripController.MaxRIPValue}"
        );

        if (!ripController.TryPreventDeath())
        {
            return false;
        }

        /*
         * 안식 3 미만일 때 체력 1로 버팁니다.
         */
        currentHP = 1;

        Debug.Log(
            $"[Enemy] 안식 발동으로 사망 방지 : {name} / " +
            $"현재 체력 {currentHP}"
        );

        return true;
    }

    /// <summary>
    /// 안식 효과로 예약된 체력 회복을
    /// 적 턴 시작 시 처리합니다.
    ///
    /// 최대 체력을 초과하여 회복하지 않으며,
    /// 회복 완료 후 대기 상태를 해제합니다.
    /// </summary>
    private void ProcessRIPPendingHeal()
    {
        RIPController ripController =
            GetComponent<RIPController>();

        if (ripController == null)
        {
            return;
        }

        if (!ripController.CanProcessPendingHeal())
        {
            return;
        }

        int healAmount =
            ripController.GetPendingHealAmount();

        Heal(healAmount);

        ripController.CompletePendingHeal();

        Debug.Log(
            $"[Enemy] 안식 회복 발동 : {name} / " +
            $"{healAmount} 회복 / " +
            $"현재 체력 {currentHP}/{maxHP}"
        );
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
    /// 적이 ProfanedHalo 패시브를 보유한 상태에서
    /// 피해로 방어도가 전부 소진되면
    /// 자신에게 취약 2를 부여합니다.
    /// </summary>
    private void TryActivateProfanedHalo()
    {
        StatusEffectHandler statusEffectHandler =
            GetComponent<StatusEffectHandler>();

        if (statusEffectHandler == null)
        {
            return;
        }

        if (!statusEffectHandler.HasStatusEffect(
            StatusEffectType.ProfanedHalo
        ))
        {
            return;
        }

        statusEffectHandler.AddStatusEffect(
            StatusEffectType.Vulnerable,
            2,
            2,
            false
        );

        Debug.Log(
            $"[Enemy] 모독받은 후광 발동 : " +
            $"{name}에게 취약 2 부여"
        );
    }

    /// <summary>
    /// 적의 체력을 지정한 수치만큼 회복합니다.
    /// 최대 체력을 초과하지 않으며 실제 회복량을 반환합니다.
    /// </summary>
    public int Heal(int amount)
    {
        if (amount <= 0)
        {
            return 0;
        }

        if (currentHP <= 0)
        {
            Debug.LogWarning(
                $"[Enemy] 체력이 0인 적은 직접 회복할 수 없습니다 : " +
                $"{name}"
            );

            return 0;
        }

        int previousHP =
            currentHP;

        currentHP =
            Mathf.Min(
                maxHP,
                currentHP + amount
            );

        int actualHealAmount =
            currentHP - previousHP;

        Debug.Log(
            $"[Enemy] 체력 회복 : {name} / " +
            $"+{actualHealAmount} / " +
            $"{currentHP}/{maxHP}"
        );

        return actualHealAmount;
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
        Debug.Log(
            $"[Enemy] 적 사망 : {name}"
        );

        if (battleManager != null)
        {
            battleManager.CheckBattleEnd();
        }

        gameObject.SetActive(false);
    }
}