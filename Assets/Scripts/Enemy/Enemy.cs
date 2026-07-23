using System;
using UnityEngine;

/// <summary>
/// 전투 중 적의 체력과 공통 전투 기능을 관리합니다.
///
/// 데미지, 방어도, 회복, 사망 처리와 함께
/// 적 턴에서 CSV 패턴 컨트롤러를 실행합니다.
///
/// EnemyBattleData를 통해 생성된 메인 적은
/// 사망 시 자신을 생성한 EnemySpawner에 알립니다.
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

    [Header("이 적을 생성한 Enemy Spawner")]
    [SerializeField]
    private EnemySpawner ownerSpawner;

    [Header("사망 처리 여부")]
    [SerializeField]
    private bool isDeathProcessed;

    /// <summary>
    /// 현재 체력입니다.
    /// </summary>
    public int CurrentHP => currentHP;

    /// <summary>
    /// 최대 체력입니다.
    /// </summary>
    public int MaxHP => maxHP;

    /// <summary>
    /// 적의 현재 체력 또는 최대 체력이 변경됐을 때 호출됩니다.
    /// 현재 체력과 최대 체력을 전달합니다.
    /// </summary>
    public event Action<int, int> HealthChanged;

    /// <summary>
    /// 현재 방어도입니다.
    /// </summary>
    public int CurrentBlock => currentBlock;

    /// <summary>
    /// 적의 현재 방어도가 변경됐을 때 호출됩니다.
    /// 현재 방어도 값을 전달합니다.
    /// </summary>
    public event Action<int> BlockChanged;

    /// <summary>
    /// 다음 적 턴 기절 여부입니다.
    /// </summary>
    public bool IsStunnedNextTurn =>
        isStunnedNextTurn;

    /// <summary>
    /// 이 적의 사망 처리가 완료됐는지 여부입니다.
    /// </summary>
    public bool IsDeathProcessed =>
        isDeathProcessed;

    private void Awake()
    {
        currentHP = maxHP;
        currentBlock = 0;

        applyParalyzeNext = true;
        applyNoBlockNext = true;

        isStunnedNextTurn = false;
        isDeathProcessed = false;
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
    /// 이 적을 생성한 EnemySpawner를 연결합니다.
    /// </summary>
    public void SetOwnerSpawner(
        EnemySpawner spawner)
    {
        ownerSpawner = spawner;

        if (ownerSpawner == null)
        {
            Debug.LogWarning(
                $"[Enemy] Owner Spawner 연결 실패: {name}",
                this
            );
        }
    }

    /// <summary>
    /// 적의 턴 행동을 실행합니다.
    ///
    /// 처리 순서:
    /// 1. 모르바엘 안식 예약 회복
    /// 2. 아스피도켈 침몰 폭발
    /// 3. 행동 가능 여부 확인
    /// 4. 기절 처리
    /// 5. CSV 패턴 실행
    /// 6. 패턴이 없는 적은 기본 공격 실행
    /// </summary>
    public void TakeTurn(
        PlayerCombat playerCombat)
    {
        if (playerCombat == null)
        {
            Debug.LogWarning(
                "[Enemy] PlayerCombat이 없습니다.",
                this
            );

            return;
        }

        if (isDeathProcessed)
        {
            return;
        }

        ProcessRIPPendingHeal();

        if (TryExecuteUnderWaterTurn(playerCombat))
        {
            return;
        }

        if (currentHP <= 0)
        {
            return;
        }

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

        EnemyPatternController patternController =
            GetComponent<EnemyPatternController>();

        if (patternController != null)
        {
            Debug.Log(
                $"[Enemy] CSV 패턴 실행 : {name} / " +
                $"현재 패턴 {patternController.CurrentPatternTurn}",
                this
            );

            patternController.ProcessCurrentPattern();

            return;
        }

        ExecuteBasicTurn(playerCombat);
    }

    /// <summary>
    /// CSV 패턴 컨트롤러가 없는 적의
    /// 기본 공격 행동을 실행합니다.
    /// </summary>
    private void ExecuteBasicTurn(
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

        playerCombat.ReceiveAttackDamage(
            finalDamage
        );

        ProcessDToxinSwitch(playerCombat);
        ProcessFFesteredSkin(playerCombat);
    }

    /// <summary>
    /// 적에게 적용된 힘과 약화를 반영하여
    /// 플레이어에게 줄 최종 피해를 계산합니다.
    ///
    /// 계산 순서:
    /// 1. 기본 피해
    /// 2. 힘 수치 추가
    /// 3. 약화가 있다면 40% 감소
    /// </summary>
    public int CalculateOutgoingDamage(
        int baseDamage)
    {
        int finalDamage =
            Mathf.Max(
                0,
                baseDamage
            );

        StatusEffectHandler statusEffectHandler =
            GetComponent<StatusEffectHandler>();

        if (statusEffectHandler == null)
        {
            return finalDamage;
        }

        /*
         * 힘은 각각의 공격 피해에 더해집니다.
         *
         * 예:
         * 기본 피해 7 + 힘 1 = 8
         * 기본 피해 9 × 3 + 힘 1 =
         * 각 타격이 10이 되어 10 × 3
         */
        int mightValue =
            statusEffectHandler.GetStatusValue(
                StatusEffectType.Might
            );

        if (mightValue > 0)
        {
            int damageBeforeMight =
                finalDamage;

            finalDamage += mightValue;

            Debug.Log(
                $"[Enemy] 힘 적용 : " +
                $"{damageBeforeMight} + {mightValue} " +
                $"= {finalDamage}",
                this
            );
        }

        /*
         * 약화는 힘까지 포함된 공격 피해를
         * 최종적으로 40% 감소시킵니다.
         */
        if (statusEffectHandler.HasStatusEffect(
                StatusEffectType.Weaken
            ))
        {
            int damageBeforeWeaken =
                finalDamage;

            finalDamage =
                Mathf.FloorToInt(
                    finalDamage * 0.6f
                );

            Debug.Log(
                $"[Enemy] 약화 적용 : " +
                $"{damageBeforeWeaken} → {finalDamage}",
                this
            );
        }

        return Mathf.Max(
            0,
            finalDamage
        );
    }

    /// <summary>
    /// 플레이어에게 적 공격 피해를 적용합니다.
    /// 적에게 걸린 약화를 자동으로 반영합니다.
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
            CalculateOutgoingDamage(
                baseDamage
            );

        Debug.Log(
            $"[Enemy] 플레이어 공격 : " +
            $"{finalDamage} 피해",
            this
        );

        playerCombat.ReceiveAttackDamage(
            finalDamage
        );
    }

    /// <summary>
    /// 플레이어에게 적 공격 피해를 여러 번 적용합니다.
    /// 각 타격은 독립된 공격으로 처리됩니다.
    /// </summary>
    public void DealRepeatedDamageToPlayer(
        PlayerCombat playerCombat,
        int baseDamage,
        int repeatCount)
    {
        if (playerCombat == null)
        {
            Debug.LogWarning(
                "[Enemy] PlayerCombat이 없어 " +
                "다단 피해를 적용할 수 없습니다.",
                this
            );

            return;
        }

        if (repeatCount <= 0)
        {
            return;
        }

        for (int i = 0;
             i < repeatCount;
             i++)
        {
            DealDamageToPlayer(
                playerCombat,
                baseDamage
            );
        }
    }

    /// <summary>
    /// UnderWater 상태라면 침몰 피해를 실행한 뒤 사망합니다.
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
    /// 독오름 패시브를 처리합니다.
    /// 공격 후 마비와 중독을 번갈아 부여합니다.
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

        applyParalyzeNext =
            !applyParalyzeNext;
    }

    /// <summary>
    /// 불어터진 피부 패시브를 처리합니다.
    /// 공격 후 미끄러짐과 부러짐을 번갈아 부여합니다.
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

        applyNoBlockNext =
            !applyNoBlockNext;
    }

    /// <summary>
    /// 모르바엘에게 장송의 원혼이 존재한다면
    /// 받는 피해를 25% 감소시킵니다.
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
    /// </summary>
    public int TakeDamage(int damage)
    {
        if (isDeathProcessed ||
            currentHP <= 0)
        {
            return 0;
        }

        damage =
            Mathf.Max(
                0,
                damage
            );

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

        damage =
            ApplyFuneralProtection(damage);

        int remainingDamage =
            damage;

        bool hadBlockBeforeDamage =
            currentBlock > 0;

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

            NotifyBlockChanged();

            Debug.Log(
                $"[Enemy] 방어도 피해 흡수 : " +
                $"{actualBlockDamage} / " +
                $"남은 방어도 {currentBlock} / " +
                $"남은 피해 {remainingDamage}",
                this
            );
        }

        if (hadBlockBeforeDamage &&
            currentBlock <= 0)
        {
            TryActivateKShellguard();
            TryActivateProfanedHalo();
        }

        int actualHealthDamage =
            Mathf.Min(
                currentHP,
                remainingDamage
            );

        currentHP -= actualHealthDamage;

        currentHP =
            Mathf.Max(
                0,
                currentHP
            );

        NotifyHealthChanged();



        Debug.Log(
            $"[Enemy] 체력 피해 : {actualHealthDamage} / " +
            $"현재 체력 : {currentHP}",
            this
        );

        if (currentHP <= 0)
        {
            bool preventedDeath =
                TryPreventDeathWithUnderGround();

            if (!preventedDeath)
            {
                preventedDeath =
                    TryPreventDeathWithRIP();
            }

            if (!preventedDeath)
            {
                Die();
            }
        }

        return actualHealthDamage;
    }

    /// <summary>
    /// 아스피도켈의 사망 방지와
    /// UnderWater 상태 전환을 처리합니다.
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

        Debug.Log(
            $"[Enemy] {name} UnderWater 상태 전환 성공 / " +
            $"예정 침몰 피해 " +
            $"{underGroundController.CurrentSinkValue}",
            this
        );

        return true;
    }

    /// <summary>
    /// 모르바엘의 안식 사망 방지를 처리합니다.
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

        currentHP = 1;

        NotifyHealthChanged();

        Debug.Log(
            $"[Enemy] 안식 발동으로 사망 방지 : {name} / " +
            $"현재 체력 {currentHP}",
            this
        );

        return true;
    }

    /// <summary>
    /// 모르바엘 안식으로 예약된 회복을 처리합니다.
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
    /// 등껍질 방패 패시브를 발동합니다.
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
    /// 모독받은 후광 패시브를 발동합니다.
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
    /// 생성 시점에 적의 최대 체력과 현재 체력을 설정합니다.
    /// </summary>
    public void InitializeHealth(
        int health)
    {
        if (health <= 0)
        {
            Debug.LogWarning(
                $"[Enemy] 잘못된 초기 체력입니다: {health}",
                this
            );

            return;
        }

        maxHP = health;
        currentHP = health;
        currentBlock = 0;
        isDeathProcessed = false;

        NotifyHealthChanged();
        NotifyBlockChanged();

        Debug.Log(
            $"[Enemy] 체력 초기화 : " +
            $"{currentHP}/{maxHP}",
            this
        );
    }

    /// <summary>
    /// 적의 체력을 회복하고
    /// 실제 회복량을 반환합니다.
    /// </summary>
    public int Heal(
        int amount)
    {
        if (amount <= 0)
        {
            return 0;
        }

        if (currentHP <= 0 ||
            isDeathProcessed)
        {
            Debug.LogWarning(
                $"[Enemy] 사망한 적은 회복할 수 없습니다: {name}",
                this
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

        NotifyHealthChanged();

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
    public void GainBlock(
        int amount)
    {
        if (amount <= 0 ||
            isDeathProcessed)
        {
            return;
        }

        currentBlock += amount;

        NotifyBlockChanged();

        Debug.Log(
            $"[Enemy] 방어도 획득 : " +
            $"+{amount} / 현재 방어도 {currentBlock}",
            this
        );
    }

    /// <summary>
    /// 현재 체력 정보를 UI 등 외부 시스템에 알립니다.
    /// </summary>
    private void NotifyHealthChanged()
    {
        HealthChanged?.Invoke(
            currentHP,
            maxHP
        );
    }

    /// <summary>
    /// 현재 방어도 정보를 UI 등 외부 시스템에 알립니다.
    /// </summary>
    private void NotifyBlockChanged()
    {
        BlockChanged?.Invoke(
            currentBlock
        );
    }

    /// <summary>
    /// 적의 최종 사망 처리를 실행합니다.
    ///
    /// 장송의 원혼은 FuneralSpiritSpawner에 사망을 알리고,
    /// 메인 적은 자신을 생성한 EnemySpawner에 사망을 알립니다.
    /// </summary>
    private void Die()
    {
        if (isDeathProcessed)
        {
            return;
        }

        isDeathProcessed = true;

        Debug.Log(
            $"[Enemy] 적 사망 : {name}",
            this
        );

        FuneralSpirit funeralSpirit =
            GetComponent<FuneralSpirit>();

        /*
         * 장송의 원혼은 메인 적 목록에 포함되지 않습니다.
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
         * 먼저 오브젝트를 비활성화하여
         * 전투 종료 검사에서 살아 있는 적으로 검색되지 않게 합니다.
         */
        gameObject.SetActive(false);

        /*
         * EnemyBattleData를 통해 생성된 메인 적은
         * 자신을 생성한 스포너에 사망 사실을 전달합니다.
         */
        if (ownerSpawner != null)
        {
            ownerSpawner.NotifyEnemyDefeated(this);
            return;
        }

        /*
         * 씬에 직접 배치된 적처럼 Owner Spawner가 없는 경우를 위한
         * 기존 호환 처리입니다.
         */
        if (battleManager == null)
        {
            battleManager =
                FindFirstObjectByType<BattleManager>();
        }

        if (battleManager != null)
        {
            Debug.LogWarning(
                $"[Enemy] Owner Spawner가 없어 " +
                $"BattleManager에서 직접 전투 종료를 검사합니다: {name}",
                this
            );

            battleManager.CheckBattleEnd();
        }
        else
        {
            Debug.LogError(
                "[Enemy] BattleManager와 EnemySpawner를 모두 " +
                "찾지 못해 전투 종료를 처리할 수 없습니다.",
                this
            );
        }
    }
}