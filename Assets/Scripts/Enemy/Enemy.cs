using UnityEngine;

/// <summary>
/// 전투 중 적의 체력과 공통 전투 기능을 관리합니다.
///
/// 데미지, 방어도, 회복, 사망 처리와 함께
/// 적 턴에서 패턴 컨트롤러를 실행합니다.
/// </summary>
public class Enemy : MonoBehaviour
{
    [Header("적 최대 체력")]
    [SerializeField]
    private int maxHP = 30;

    [Header("적 현재 체력")]
    [SerializeField]
    private int currentHP;

    [Header("패턴이 없는 적의 기본 공격력")]
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

    /// <summary>
    /// 현재 체력입니다.
    /// </summary>
    public int CurrentHP => currentHP;

    /// <summary>
    /// 최대 체력입니다.
    /// </summary>
    public int MaxHP => maxHP;

    /// <summary>
    /// 현재 방어도입니다.
    /// </summary>
    public int CurrentBlock => currentBlock;

    /// <summary>
    /// 다음 적 턴 기절 여부입니다.
    /// </summary>
    public bool IsStunnedNextTurn => isStunnedNextTurn;

    private void Awake()
    {
        currentHP = maxHP;
        currentBlock = 0;

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
    ///
    /// 처리 순서:
    /// 1. 안식 예약 회복
    /// 2. 침몰 폭발
    /// 3. 행동 가능 여부 확인
    /// 4. 기절 처리
    /// 5. 적 전용 패턴 실행
    /// 6. 패턴이 없다면 기본 공격 실행
    /// </summary>
    public void TakeTurn(PlayerCombat playerCombat)
    {
        if (playerCombat == null)
        {
            Debug.LogWarning(
                "[Enemy] PlayerCombat이 없습니다.",
                this
            );

            return;
        }

        /*
         * 모르바엘이 안식 효과로 사망을 방지했다면
         * 다음 적 턴 시작 시 예약된 체력 회복을 먼저 처리합니다.
         */
        ProcessRIPPendingHeal();

        /*
         * 침몰 상태라면 다른 행동보다 먼저
         * 침몰 피해를 처리한 뒤 사망합니다.
         */
        if (TryExecuteUnderWaterTurn(playerCombat))
        {
            return;
        }

        /*
         * 행동 가능한 체력이 아니라면
         * 패턴과 기본 공격을 실행하지 않습니다.
         */
        if (currentHP <= 0)
        {
            return;
        }

        /*
         * 기절은 패턴 또는 기본 공격보다 먼저 처리합니다.
         *
         * 기절한 턴에는 패턴 컨트롤러를 호출하지 않으므로
         * 현재 패턴 순서도 넘어가지 않습니다.
         */
        if (isStunnedNextTurn)
        {
            isStunnedNextTurn = false;

            Debug.Log(
                $"[Enemy] {name} 기절 발동 : " +
                "이번 적 턴 행동을 건너뜁니다.",
                this
            );

            return;
        }

        /*
         * 전용 패턴 컨트롤러가 있다면
         * 해당 적의 현재 패턴 행동을 실행합니다.
         
        EnemyPatternController patternController =
            GetComponent<EnemyPatternController>();

        if (patternController != null)
        {
            patternController.ExecuteTurn(playerCombat);
            return;
        }

        /*
         * 패턴 컨트롤러가 없는 적은
         * 기존 기본 공격을 실행합니다.
         */
        ExecuteBasicAttack(playerCombat);
    }

    /// <summary>
    /// 패턴 컨트롤러가 없는 적의 기본 공격을 실행합니다.
    /// 약화 효과를 반영하여 플레이어에게 피해를 줍니다.
    /// </summary>
    private void ExecuteBasicAttack(
        PlayerCombat playerCombat)
    {
        int finalDamage =
            CalculateOutgoingDamage(
                basicAttackDamage
            );

        Debug.Log(
            $"[Enemy] 기본 공격 : " +
            $"플레이어에게 {finalDamage} 피해",
            this
        );

        playerCombat.ReceiveAttackDamage(finalDamage);

        /*
         * 정상적으로 공격한 후
         * 공격 연동 패시브를 처리합니다.
         */
        ProcessDToxinSwitch(playerCombat);
        ProcessFFesteredSkin(playerCombat);
    }

    /// <summary>
    /// 적이 플레이어에게 주는 피해에
    /// 약화 효과를 반영하여 최종 피해를 반환합니다.
    ///
    /// 적 전용 패턴에서도 이 메서드를 사용할 수 있습니다.
    /// </summary>
    public int CalculateOutgoingDamage(int baseDamage)
    {
        if (baseDamage < 0)
        {
            baseDamage = 0;
        }

        int finalDamage = baseDamage;

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
                $"{finalDamage} → {reducedDamage}",
                this
            );

            finalDamage = reducedDamage;
        }

        return finalDamage;
    }

    /// <summary>
    /// 적의 공격 피해를 플레이어에게 적용합니다.
    ///
    /// 적의 약화 효과가 자동으로 반영됩니다.
    /// 전용 패턴 컨트롤러에서 사용할 수 있습니다.
    /// </summary>
    public void DealDamageToPlayer(
        PlayerCombat playerCombat,
        int baseDamage)
    {
        if (playerCombat == null)
        {
            Debug.LogWarning(
                "[Enemy] PlayerCombat이 없어 피해를 적용할 수 없습니다.",
                this
            );

            return;
        }

        int finalDamage =
            CalculateOutgoingDamage(baseDamage);

        Debug.Log(
            $"[Enemy] 플레이어 공격 : " +
            $"{finalDamage} 피해",
            this
        );

        playerCombat.ReceiveAttackDamage(finalDamage);
    }

    /// <summary>
    /// 적의 공격 피해를 플레이어에게 여러 번 적용합니다.
    ///
    /// 각 공격은 개별 피해로 처리되므로
    /// 플레이어 방어도와 선원 피해 분배도 매 타격마다 적용됩니다.
    /// </summary>
    public void DealRepeatedDamageToPlayer(
        PlayerCombat playerCombat,
        int baseDamage,
        int repeatCount)
    {
        if (playerCombat == null)
        {
            Debug.LogWarning(
                "[Enemy] PlayerCombat이 없어 다단 피해를 적용할 수 없습니다.",
                this
            );

            return;
        }

        if (repeatCount <= 0)
        {
            return;
        }

        for (int i = 0; i < repeatCount; i++)
        {
            DealDamageToPlayer(
                playerCombat,
                baseDamage
            );
        }
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

        if (!underGroundController
                .CanExecuteUnderWaterExplosion())
        {
            return false;
        }

        int sinkDamage =
            underGroundController.GetUnderWaterDamage();

        Debug.Log(
            $"[Enemy] {name} 침몰 발동 : " +
            $"피해 {sinkDamage}",
            this
        );

        if (sinkDamage > 0)
        {
            playerCombat.ReceiveAttackDamage(
                sinkDamage
            );
        }

        /*
         * 중복 발동을 방지한 뒤
         * 보스를 사망 처리합니다.
         */
        underGroundController
            .CompleteUnderWaterExplosion();

        Debug.Log(
            $"[Enemy] {name} 침몰 피해 처리 완료 : " +
            "보스를 사망 처리합니다.",
            this
        );

        Die();

        return true;
    }

    /// <summary>
    /// 적이 DToxinSwitch 패시브를 보유하고 있다면
    /// 공격 후 플레이어에게 Paralyze와 Toxic을 번갈아 부여합니다.
    ///
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
                "독오름 효과를 부여할 수 없습니다.",
                this
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
                $"플레이어에게 마비 {applyValue} 부여",
                this
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
                $"플레이어에게 중독 {applyValue} 부여",
                this
            );
        }

        applyParalyzeNext = !applyParalyzeNext;
    }

    /// <summary>
    /// 적이 FFesteredSkin 패시브를 보유하고 있다면
    /// 정상 공격 후 플레이어에게 NoBlock과 Broken을
    /// 번갈아 부여합니다.
    ///
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
                "불어터진 피부 효과를 부여할 수 없습니다.",
                this
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
                $"플레이어에게 미끄러짐 {applyValue} 부여",
                this
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
                $"플레이어에게 부러짐 {applyValue} 부여",
                this
            );
        }

        applyNoBlockNext = !applyNoBlockNext;
    }

    /// <summary>
    /// 모르바엘에게 장송의 원혼이 존재한다면
    /// 받는 피해를 25% 감소시킵니다.
    ///
    /// RIPController가 없는 일반 적과 원혼에는 적용되지 않습니다.
    /// 모든 퍼센트 계산의 소수점은 버립니다.
    /// </summary>
    private int ApplyFuneralProtection(
        int damage)
    {
        if (damage <= 0)
        {
            return 0;
        }

        RIPController ripController =
            GetComponent<RIPController>();

        /*
         * RIPController는 모르바엘에게만 붙기 때문에
         * 일반 적과 원혼에는 장송의 가호가 적용되지 않습니다.
         */
        if (ripController == null)
        {
            return damage;
        }

        if (!ripController.HasActiveFuneralSpirit)
        {
            return damage;
        }

        int reducedDamage =
            Mathf.FloorToInt(
                damage * 0.75f
            );

        Debug.Log(
            $"[Enemy] 장송의 가호 적용 : " +
            $"{damage} → {reducedDamage}",
            this
        );

        return reducedDamage;
    }

    /// <summary>
    /// 적에게 피해를 적용하고
    /// 실제로 감소한 체력량을 반환합니다.
    ///
    /// 취약, 방어도, 현재 체력을 모두 반영합니다.
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
                $"{damage} → {increasedDamage}",
                this
            );

            damage = increasedDamage;
        }

        /*
         * 모르바엘에게 장송의 원혼이 존재한다면
         * 피해를 25% 감소시킵니다.
         */
        damage =
            ApplyFuneralProtection(damage);

        int remainingDamage = damage;

        /*
         * 피해를 받기 전에 방어도가 있었는지 저장합니다.
         *
         * KShellguard와 ProfanedHalo는
         * 이번 피해로 방어도가 전부 소진될 때 발동합니다.
         */
        bool hadBlockBeforeDamage =
            currentBlock > 0;

        /*
         * 적 방어도가 체력보다 먼저 피해를 받습니다.
         */
        if (remainingDamage > 0 &&
            currentBlock > 0)
        {
            int actualBlockDamage =
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
                $"남은 피해 {remainingDamage}",
                this
            );
        }

        /*
         * 피해 전에는 방어도가 있었고
         * 이번 피해로 방어도가 0이 되었다면
         * 관련 패시브를 발동합니다.
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
            $"현재 체력 : {currentHP}",
            this
        );

        /*
         * 흡혈 등에서 사용하는 실제 피해량은
         * 방어도 피해가 아닌 체력 감소량입니다.
         */
        int actualDamage = actualHealthDamage;

        if (currentHP <= 0)
        {
            Debug.Log(
                $"[Enemy] 체력 0 도달 : {name}",
                this
            );

            /*
             * 아스피도켈의 UnderGround 사망 방지를 먼저 검사합니다.
             */
            bool preventedDeath =
                TryPreventDeathWithUnderGround();

            /*
             * UnderGround가 발동하지 않았다면
             * 모르바엘의 RIP 사망 방지를 검사합니다.
             */
            if (!preventedDeath)
            {
                preventedDeath =
                    TryPreventDeathWithRIP();
            }

            /*
             * 어떤 사망 방지 효과도 발동하지 않았다면
             * 일반 사망 처리합니다.
             */
            if (!preventedDeath)
            {
                Debug.Log(
                    $"[Enemy] 일반 사망 처리 실행 : {name}",
                    this
                );

                Die();
            }
        }

        return actualDamage;
    }

    /// <summary>
    /// UnderGroundController가 붙어 있는 적이
    /// 체력 0이 되었을 때 UnderWater 상태 전환을 시도합니다.
    ///
    /// 전환에 성공하면 true를 반환하여 일반 사망을 막습니다.
    /// </summary>
    private bool TryPreventDeathWithUnderGround()
    {
        UnderGroundController underGroundController =
            GetComponent<UnderGroundController>();

        if (underGroundController == null)
        {
            return false;
        }

        Debug.Log(
            $"[Enemy] UnderGround 사망 방지 검사 : {name} / " +
            $"UnderGround = {underGroundController.IsUnderGround} / " +
            $"UnderWater = {underGroundController.IsUnderWater} / " +
            $"침몰 수치 = {underGroundController.CurrentSinkValue}",
            this
        );

        if (!underGroundController.TryEnterUnderWater())
        {
            return false;
        }

        /*
         * 체력은 0으로 유지하지만 오브젝트는 비활성화하지 않습니다.
         * 이후 적 턴에 침몰 폭발을 실행합니다.
         */
        Debug.Log(
            $"[Enemy] {name} UnderWater 상태 전환 성공 / " +
            $"예정 침몰 피해 " +
            $"{underGroundController.CurrentSinkValue}",
            this
        );

        return true;
    }

    /// <summary>
    /// RIPController가 붙은 적의 체력이 0이 되었을 때
    /// 안식이 최대치 미만이라면 체력 1로 생존시킵니다.
    ///
    /// 사망 방지에 성공하면
    /// 다음 적 턴 회복이 예약됩니다.
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
            $"{ripController.MaxRIPValue}",
            this
        );

        if (!ripController.TryPreventDeath())
        {
            return false;
        }

        /*
         * 안식이 최대치 미만이면
         * 체력 1로 버팁니다.
         */
        currentHP = 1;

        Debug.Log(
            $"[Enemy] 안식 발동으로 사망 방지 : {name} / " +
            $"현재 체력 {currentHP}",
            this
        );

        return true;
    }

    /// <summary>
    /// 안식 효과로 예약된 체력 회복을
    /// 적 턴 시작 시 처리합니다.
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
            $"현재 체력 {currentHP}/{maxHP}",
            this
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
            $"{name}은 다음 적 턴에 행동하지 않습니다.",
            this
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
            $"{name}에게 취약 2 부여",
            this
        );
    }

    /// <summary>
    /// 적의 최대 체력과 현재 체력을 설정합니다.
    ///
    /// 소환되는 원혼처럼 생성 시점에
    /// 체력이 결정되는 적에게 사용합니다.
    /// </summary>
    public void InitializeHealth(int health)
    {
        if (health <= 0)
        {
            Debug.LogWarning(
                $"[Enemy] 잘못된 초기 체력입니다 : {health}",
                this
            );

            return;
        }

        maxHP = health;
        currentHP = health;
        currentBlock = 0;

        Debug.Log(
            $"[Enemy] 체력 초기화 : " +
            $"{currentHP}/{maxHP}",
            this
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
                $"{name}",
                this
            );

            return 0;
        }

        int previousHP = currentHP;

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
            $"{currentHP}/{maxHP}",
            this
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
            $"+{amount} / 현재 방어도 {currentBlock}",
            this
        );
    }

    /// <summary>
    /// 적의 사망 처리를 실행합니다.
    ///
    /// 장송의 원혼은 메인 적이 아니므로
    /// BattleManager의 전투 종료 검사를 실행하지 않습니다.
    /// </summary>
    private void Die()
    {
        Debug.Log(
            $"[Enemy] 적 사망 : {name}",
            this
        );

        FuneralSpirit funeralSpirit =
            GetComponent<FuneralSpirit>();

        /*
         * 장송의 원혼 사망 처리
         *
         * 원혼은 메인 적이 아니므로
         * 원혼 하나가 죽었다고 전투 종료를 검사하지 않습니다.
         */
        if (funeralSpirit != null)
        {
            funeralSpirit.NotifyDeath();

            gameObject.SetActive(false);

            Debug.Log(
                $"[Enemy] 장송의 원혼 사망 처리 완료 : {name}",
                this
            );

            return;
        }

        /*
         * 일반 적 또는 보스가 사망한 경우에만
         * 기존 전투 종료 검사를 실행합니다.
         */
        if (battleManager != null)
        {
            battleManager.CheckBattleEnd();
        }

        gameObject.SetActive(false);
    }
}