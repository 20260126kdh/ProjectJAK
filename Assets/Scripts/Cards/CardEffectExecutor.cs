using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 카드에 들어있는 효과 목록을 순서대로 실행하는 클래스입니다.
/// 피해, 방어도, 체력 감소, 회복, 상태 효과 부여를 처리합니다.
/// </summary>
public class CardEffectExecutor : MonoBehaviour
{
    [Header("플레이어 태그")]
    [SerializeField]
    private string playerTag = "Player";

    [Header("플레이어 애니메이션")]
    [SerializeField]
    private PlayerAnimationController playerAnimationController;

    /// <summary>
    /// 클래스 기본 패시브를 관리합니다.
    /// </summary>
    private ClassPassiveController classPassiveController;

    /// <summary>
    /// 현재 실행 중인 카드가 희생한
    /// 선원들의 현재 체력 합계입니다.
    /// 카드 실행이 시작될 때마다 0으로 초기화됩니다.
    /// </summary>
    private int sacrificedHealthThisCard;

    /// <summary>
    /// 현재 실행 중인 공격 카드에 적용할 피해 보정값입니다.
    /// 무너진 의지는 -4를 전달합니다.
    /// </summary>
    private int currentCardDamageModifier;

    /// <summary>
    /// 현재 실행 중인 공격 카드에 적용할 피해 배율입니다.
    /// 악마의 힘이 적용되면 2가 됩니다.
    /// </summary>
    private int currentCardDamageMultiplier = 1;

    private bool currentCardHadAttack;
    private bool currentCardGainedBlock;
    private bool currentCardAppliedHarpoon;
    private bool currentCardHasImmediateLifesteal;
    private readonly HashSet<StatusEffectType> currentCardBuffTypes =
        new HashSet<StatusEffectType>();
    private readonly HashSet<StatusEffectType> currentCardDebuffTypes =
        new HashSet<StatusEffectType>();

    /// <summary>
    /// 카드 효과 목록을 실행합니다.
    /// </summary>
    public void ExecuteEffects(
    CardData cardData,
    Enemy targetEnemy,
    int damageModifier = 0,
    int damageMultiplier = 1,
    Crew targetCrew = null)
    {
        if (cardData == null)
        {
            Debug.LogWarning(
                "[CardEffectExecutor] " +
                "실행할 카드 데이터가 없습니다."
            );

            return;
        }

        if (cardData.effects == null ||
            cardData.effects.Count == 0)
        {
            Debug.LogWarning(
                $"[CardEffectExecutor] " +
                $"{cardData.cardName} 카드에 효과가 없습니다."
            );

            return;
        }

        sacrificedHealthThisCard = 0;
        currentCardDamageModifier = damageModifier;
        currentCardDamageMultiplier =
            Mathf.Max(1, damageMultiplier);
        currentCardHadAttack = false;
        currentCardGainedBlock = false;
        currentCardAppliedHarpoon = false;
        currentCardBuffTypes.Clear();
        currentCardDebuffTypes.Clear();

        List<CardEffectData> orderedEffects =
            cardData.effects
                .OrderBy(effect => effect.order)
                .ToList();

        currentCardHasImmediateLifesteal =
            HasImmediateLifestealCombination(
                orderedEffects
            );

        foreach (CardEffectData effect in orderedEffects)
        {
            ExecuteSingleEffect(
                effect,
                targetEnemy,
                targetCrew
            );
        }

        if (SFXManager.Instance != null)
        {
            SFXManager.Instance.PlayCardEffectSequence(
                currentCardHadAttack,
                currentCardGainedBlock,
                currentCardBuffTypes.Count,
                currentCardDebuffTypes.Count,
                currentCardAppliedHarpoon
            );
        }
    }

    /// <summary>
    /// 카드 효과 하나를 실행합니다.
    /// </summary>
    private void ExecuteSingleEffect(
        CardEffectData effect,
        Enemy targetEnemy,
        Crew targetCrew)
    {
        switch (effect.effectType)
        {
            case CardEffectType.DealDamage:
                ExecuteDealDamage(
                    effect,
                    targetEnemy
                );
                break;

            case CardEffectType.HealAllCrews:
                ExecuteHealAllCrews(effect);
                break;

            case CardEffectType.GainBlock:
                ExecuteGainBlock(effect);
                break;

            case CardEffectType.LoseHealth:
                ExecuteLoseHealth(effect);
                break;

            case CardEffectType.Heal:
                ExecuteHeal(effect);
                break;

            case CardEffectType.ApplyStatus:
                if (IsImmediateLifestealEffect(effect))
                {
                    Debug.Log(
                        "[CardEffectExecutor] " +
                        "현재 카드 피해 한정 흡혈로 처리합니다."
                    );
                    break;
                }

                ExecuteApplyStatus(
                    effect,
                    targetEnemy
                );
                RecordStatusSfx(effect.statusEffectType);
                break;

            case CardEffectType.HarpoonerStack:
            case CardEffectType.ApplyHarpoon:
                ExecuteApplyHarpoon(
                    effect,
                    targetEnemy
                );
                break;

            case CardEffectType.DrawCard:
                Debug.Log(
                    $"[CardEffectExecutor] " +
                    $"카드 드로우 예정 : {effect.value}"
                );
                break;

            case CardEffectType.Summon:
            case CardEffectType.SummonCrew:
                ExecuteSummonCrew(effect);
                break;

            case CardEffectType.CrewDealDamage:
                ExecuteCrewDealDamage(
                    effect,
                    targetEnemy
                );
                break;

            case CardEffectType.AllCrewsDealDamageRandomEnemy:
                ExecuteAllCrewsDealDamageRandomEnemy(
                    effect
                );
                break;

            case CardEffectType.AllCrewsDealDamageAllEnemies:
                ExecuteAllCrewsDealDamageAllEnemies(
                    effect
                );
                break;

            case CardEffectType.GainBlockOnHealthLossThisTurn:
                ExecuteGainBlockOnHealthLossThisTurn(
                    effect
                );
                break;

            case CardEffectType.DoubleNextAttackDamage:
                ExecuteDoubleNextAttackDamage();
                RecordStatusSfx(StatusEffectType.DevilPower);
                break;

            case CardEffectType.Sacrifice:
                ExecuteSacrifice(effect, targetCrew);
                break;

            case CardEffectType.SacrificeAll:
                ExecuteSacrificeAll();
                break;

            case CardEffectType.SetMaxHealth:
                ExecuteSetCrewHealth(effect);
                break;

            case CardEffectType.MightEqualToSacrificedHealth:
                ExecuteMightEqualToSacrificedHealth();
                if (sacrificedHealthThisCard > 0)
                {
                    RecordStatusSfx(StatusEffectType.Might);
                }
                break;

            case CardEffectType.DealDamageEqualToHarpoonerStack:
                ExecuteDamageEqualToHarpoonStack(
                    targetEnemy
                );
                break;
        }
    }

    /// <summary>
    /// 지정된 대상에게 작살 스택을 부여합니다.
    /// 단일 적, 무작위 적, 모든 적 대상을 지원합니다.
    /// </summary>
    private void ExecuteApplyHarpoon(
        CardEffectData effect,
        Enemy targetEnemy)
    {
        int stackAmount =
            Mathf.Max(
                0,
                effect.value
            );

        if (stackAmount <= 0)
        {
            Debug.LogWarning(
                "[CardEffectExecutor] " +
                "부여할 작살 스택이 0 이하입니다."
            );

            return;
        }

        switch (effect.target)
        {
            case CardTargetType.Enemy:
                ApplyHarpoonToEnemy(
                    targetEnemy,
                    stackAmount
                );
                break;

            case CardTargetType.RandomEnemy:
                ApplyHarpoonToRandomEnemy(
                    stackAmount
                );
                break;

            case CardTargetType.AllEnemies:
                ApplyHarpoonToAllEnemies(
                    stackAmount
                );
                break;

            default:
                Debug.LogWarning(
                    $"[CardEffectExecutor] " +
                    $"지원하지 않는 작살 대상입니다: " +
                    $"{effect.target}"
                );
                break;
        }
    }

    /// <summary>
    /// 지정된 적 한 명에게 작살 스택을 부여합니다.
    /// </summary>
    private void ApplyHarpoonToEnemy(
        Enemy targetEnemy,
        int stackAmount)
    {
        if (!IsEnemyAlive(targetEnemy))
        {
            Debug.LogWarning(
                "[CardEffectExecutor] " +
                "작살을 부여할 적이 없습니다."
            );

            return;
        }

        HarpoonStackController harpoonController =
            targetEnemy.GetComponent<HarpoonStackController>();

        if (harpoonController == null)
        {
            Debug.LogWarning(
                $"[CardEffectExecutor] " +
                $"{targetEnemy.name}에 " +
                "HarpoonStackController가 없습니다.",
                targetEnemy
            );

            return;
        }

        int actualAddedAmount =
            harpoonController.AddHarpoonStack(
                stackAmount
            );

        if (actualAddedAmount > 0)
        {
            currentCardAppliedHarpoon = true;
        }

        Debug.Log(
            $"[CardEffectExecutor] 작살 스택 부여 : " +
            $"{targetEnemy.name} / " +
            $"+{actualAddedAmount} / " +
            $"현재 {harpoonController.CurrentHarpoonStack}",
            targetEnemy
        );
    }

    /// <summary>
    /// 살아 있는 적 중 한 명을 무작위로 선택하여
    /// 작살 스택을 부여합니다.
    /// </summary>
    private void ApplyHarpoonToRandomEnemy(
        int stackAmount)
    {
        List<Enemy> activeEnemies =
            FindActiveEnemies();

        if (activeEnemies.Count <= 0)
        {
            Debug.LogWarning(
                "[CardEffectExecutor] " +
                "작살을 부여할 살아 있는 적이 없습니다."
            );

            return;
        }

        int randomIndex =
            Random.Range(
                0,
                activeEnemies.Count
            );

        ApplyHarpoonToEnemy(
            activeEnemies[randomIndex],
            stackAmount
        );
    }

    /// <summary>
    /// 현재 살아 있는 모든 적에게
    /// 작살 스택을 부여합니다.
    /// </summary>
    private void ApplyHarpoonToAllEnemies(
        int stackAmount)
    {
        List<Enemy> activeEnemies =
            FindActiveEnemies();

        if (activeEnemies.Count <= 0)
        {
            Debug.LogWarning(
                "[CardEffectExecutor] " +
                "작살을 부여할 살아 있는 적이 없습니다."
            );

            return;
        }

        foreach (Enemy enemy in activeEnemies)
        {
            ApplyHarpoonToEnemy(
                enemy,
                stackAmount
            );
        }
    }

    /// <summary>
    /// 지정된 대상 방식에 따라 일반 공격 피해를
    /// repeatCount만큼 반복해서 적용합니다.
    /// </summary>
    private void ExecuteDealDamage(
        CardEffectData effect,
        Enemy targetEnemy)
    {
        int repeatCount =
            Mathf.Max(1, effect.repeatCount);

        switch (effect.target)
        {
            case CardTargetType.Enemy:
                ExecuteDealDamageToEnemy(
                    effect,
                    targetEnemy,
                    repeatCount
                );
                break;

            case CardTargetType.RandomEnemy:
                ExecuteDealDamageToRandomEnemy(
                    effect,
                    repeatCount
                );
                break;

            case CardTargetType.AllEnemies:
                ExecuteDealDamageToAllEnemies(
                    effect,
                    repeatCount
                );
                break;

            default:
                Debug.LogWarning(
                    $"[CardEffectExecutor] " +
                    $"처리되지 않은 공격 대상 : {effect.target}"
                );
                break;
        }
    }

    /// <summary>
    /// 선택된 단일 적에게 repeatCount만큼 피해를 줍니다.
    /// 대상이 사망하면 남은 타격을 중단합니다.
    /// </summary>
    private void ExecuteDealDamageToEnemy(
        CardEffectData effect,
        Enemy targetEnemy,
        int repeatCount)
    {
        if (!IsEnemyAlive(targetEnemy))
        {
            Debug.LogWarning(
                "[CardEffectExecutor] " +
                "공격할 단일 적이 없습니다."
            );

            return;
        }

        for (int hitIndex = 0;
             hitIndex < repeatCount;
             hitIndex++)
        {
            if (!IsEnemyAlive(targetEnemy))
            {
                break;
            }

            int actualDamage =
                ApplyPlayerAttackDamage(
                    effect.value,
                    targetEnemy
                );

            Debug.Log(
                $"[CardEffectExecutor] 단일 적 공격 : " +
                $"{targetEnemy.name} / " +
                $"{hitIndex + 1}/{repeatCount}타 / " +
                $"실제 피해 {actualDamage}"
            );
        }
    }

    /// <summary>
    /// 매 타격마다 살아 있는 적 중 한 명을
    /// 무작위로 다시 선택하여 피해를 줍니다.
    /// </summary>
    private void ExecuteDealDamageToRandomEnemy(
        CardEffectData effect,
        int repeatCount)
    {
        for (int hitIndex = 0;
             hitIndex < repeatCount;
             hitIndex++)
        {
            List<Enemy> activeEnemies =
                FindActiveEnemies();

            if (activeEnemies.Count <= 0)
            {
                break;
            }

            int randomIndex =
                Random.Range(
                    0,
                    activeEnemies.Count
                );

            Enemy randomEnemy =
                activeEnemies[randomIndex];

            int actualDamage =
                ApplyPlayerAttackDamage(
                    effect.value,
                    randomEnemy
                );

            Debug.Log(
                $"[CardEffectExecutor] 무작위 적 공격 : " +
                $"{randomEnemy.name} / " +
                $"{hitIndex + 1}/{repeatCount}타 / " +
                $"실제 피해 {actualDamage}"
            );
        }
    }

    /// <summary>
    /// 모든 살아 있는 적에게 피해를 주는 과정을
    /// repeatCount만큼 반복합니다.
    /// </summary>
    private void ExecuteDealDamageToAllEnemies(
        CardEffectData effect,
        int repeatCount)
    {
        for (int hitIndex = 0;
             hitIndex < repeatCount;
             hitIndex++)
        {
            List<Enemy> activeEnemies =
                FindActiveEnemies();

            if (activeEnemies.Count <= 0)
            {
                break;
            }

            foreach (Enemy enemy in activeEnemies)
            {
                if (!IsEnemyAlive(enemy))
                {
                    continue;
                }

                int actualDamage =
                    ApplyPlayerAttackDamage(
                        effect.value,
                        enemy
                    );

                Debug.Log(
                    $"[CardEffectExecutor] 모든 적 공격 : " +
                    $"{enemy.name} / " +
                    $"{hitIndex + 1}/{repeatCount}타 / " +
                    $"실제 피해 {actualDamage}"
                );
            }
        }
    }

    /// <summary>
    /// 플레이어의 힘과 약화를 계산한 뒤
    /// 적에게 한 번의 공격 피해를 적용합니다.
    /// 실제 피해량을 기준으로 흡혈을 처리합니다.
    /// </summary>
    private int ApplyPlayerAttackDamage(
    int baseDamage,
    Enemy targetEnemy)
    {
        if (!IsEnemyAlive(targetEnemy))
        {
            return 0;
        }

        if (playerAnimationController == null)
        {
            playerAnimationController =
                FindPlayerAnimationController();
        }

        if (playerAnimationController != null)
        {
            playerAnimationController.PlayAttack();
        }

        PlayerCombat playerCombat =
            FindPlayerCombat();

        StatusEffectHandler playerStatusEffectHandler =
            null;

        int modifiedBaseDamage =
    Mathf.Max(
        0,
        baseDamage + currentCardDamageModifier
    );

        int finalDamage = modifiedBaseDamage;

        if (currentCardDamageModifier != 0)
        {
            Debug.Log(
                $"[CardEffectExecutor] 카드 피해 보정 : " +
                $"{baseDamage} → {modifiedBaseDamage}"
            );
        }

        if (playerCombat != null)
        {
            playerStatusEffectHandler =
                playerCombat.GetComponent<StatusEffectHandler>();

            if (playerStatusEffectHandler != null)
            {
                int mightValue =
                    playerStatusEffectHandler.GetStatusValue(
                        StatusEffectType.Might
                    );

                finalDamage += mightValue;

                if (mightValue > 0)
                {
                    Debug.Log(
                        $"[CardEffectExecutor] 힘 적용 : " +
                        $"기본 {modifiedBaseDamage} + 힘 {mightValue} " +
                        $"= {finalDamage}"
                    );
                }

                if (playerStatusEffectHandler.HasStatusEffect(
                    StatusEffectType.Weaken
                ))
                {
                    int reducedDamage =
                        Mathf.FloorToInt(
                            finalDamage * 0.6f
                        );

                    Debug.Log(
                        $"[CardEffectExecutor] 약화 적용 : " +
                        $"{finalDamage} → {reducedDamage}"
                    );

                    finalDamage = reducedDamage;
                }
            }
        }

        finalDamage =
            Mathf.Max(
                0,
                finalDamage
            );

        if (currentCardDamageMultiplier > 1)
        {
            int multipliedDamage =
                finalDamage *
                currentCardDamageMultiplier;

            Debug.Log(
                $"[CardEffectExecutor] 악마의 힘 적용 : " +
                $"{finalDamage} → {multipliedDamage}"
            );

            finalDamage =
                multipliedDamage;
        }

        int harpoonBonusDamage = 0;

        HarpoonStackController harpoonController =
            targetEnemy.GetComponent<HarpoonStackController>();

        bool hadHarpoonStack =
            harpoonController != null &&
            harpoonController.HasHarpoonStack;

        if (hadHarpoonStack)
        {
            /*
             * 작살 추가 피해는 피해를 적용하기 전에
             * 현재 스택을 기준으로 계산합니다.
             *
             * 계산과 동시에 스택 1을 소비합니다.
             */
            harpoonBonusDamage =
                harpoonController
                    .CalculateBonusDamageAndConsume();

            Debug.Log(
                $"[CardEffectExecutor] 작살 추가 피해 적용 : " +
                $"기본 공격 피해 {finalDamage} + " +
                $"작살 추가 피해 {harpoonBonusDamage} = " +
                $"{finalDamage + harpoonBonusDamage}",
                targetEnemy
            );
        }

        int totalDamage =
            finalDamage + harpoonBonusDamage;

        if (totalDamage > 0)
        {
            BattleCameraMotion.PlayPlayerAttack();
        }

        /*
         * 적의 취약과 장송의 가호는
         * Enemy.TakeDamage() 내부에서 적용됩니다.
         */
        int blockBeforeDamage = targetEnemy.CurrentBlock;

        int actualDamage =
            targetEnemy.TakeDamage(
                totalDamage
            );

        PlayEnemyAttackSfx(
            targetEnemy,
            totalDamage,
            blockBeforeDamage,
            actualDamage,
            hadHarpoonStack,
            false
        );

        ProcessLifesteal(
            playerCombat,
            playerStatusEffectHandler,
            actualDamage
        );

        return actualDamage;
    }

    /// <summary>
    /// 캡틴 패시브에 의해
    /// 선원 공격 시 작살 스택 2를 부여합니다.
    /// </summary>
    private void ApplyCaptainCrewPassive(
        Enemy targetEnemy)
    {
        if (!IsEnemyAlive(targetEnemy))
        {
            return;
        }

        if (classPassiveController == null)
        {
            classPassiveController =
                FindFirstObjectByType<ClassPassiveController>();
        }

        if (classPassiveController == null)
        {
            return;
        }

        if (!classPassiveController.IsCaptain())
        {
            return;
        }

        HarpoonStackController harpoonController =
            targetEnemy.GetComponent<HarpoonStackController>();

        if (harpoonController == null)
        {
            return;
        }

        int addedAmount =
            harpoonController.AddHarpoonStack(2);

        if (addedAmount > 0)
        {
            currentCardAppliedHarpoon = true;
        }

        Debug.Log(
            $"[CardEffectExecutor] 캡틴 패시브 발동 : " +
            $"{targetEnemy.name} / " +
            $"작살 +{addedAmount}",
            targetEnemy
        );
    }

    /// <summary>
    /// 현재 활성화되어 있고
    /// 체력이 남아 있는 적 목록을 반환합니다.
    /// </summary>
    private List<Enemy> FindActiveEnemies()
    {
        Enemy[] foundEnemies =
            FindObjectsByType<Enemy>(
                FindObjectsSortMode.None
            );

        return foundEnemies
            .Where(enemy =>
                enemy != null &&
                enemy.gameObject.activeSelf &&
                enemy.CurrentHP > 0
            )
            .ToList();
    }

    /// <summary>
    /// 해당 적이 현재 공격 가능한 상태인지 확인합니다.
    /// </summary>
    private bool IsEnemyAlive(Enemy enemy)
    {
        return
            enemy != null &&
            enemy.gameObject.activeSelf &&
            enemy.CurrentHP > 0;
    }

    /// <summary>
    /// 플레이어가 흡혈 상태라면
    /// 실제 피해량만큼 체력을 회복합니다.
    /// </summary>
    private void ProcessLifesteal(
        PlayerCombat playerCombat,
        StatusEffectHandler statusEffectHandler,
        int actualDamage)
    {
        if (playerCombat == null)
        {
            return;
        }

        if (actualDamage <= 0)
        {
            return;
        }

        bool hasTurnLifesteal =
            statusEffectHandler != null &&
            statusEffectHandler.HasStatusEffect(
                StatusEffectType.Lifesteal
            );

        if (!currentCardHasImmediateLifesteal &&
            !hasTurnLifesteal)
        {
            return;
        }

        playerCombat.Heal(actualDamage);

        Debug.Log(
            $"[CardEffectExecutor] 흡혈 발동 : " +
            $"실제 피해 {actualDamage}만큼 회복"
        );
    }

    /// <summary>
    /// 공격과 자기 흡혈 부여가 같은 카드에 함께 있으면
    /// 해당 카드가 실제로 입힌 피해만 회복하는 효과로 해석합니다.
    /// </summary>
    private bool HasImmediateLifestealCombination(
        List<CardEffectData> orderedEffects)
    {
        bool hasDamage = orderedEffects.Any(effect =>
            effect != null &&
            effect.effectType == CardEffectType.DealDamage
        );

        bool hasSelfLifesteal = orderedEffects.Any(effect =>
            effect != null &&
            effect.effectType == CardEffectType.ApplyStatus &&
            effect.statusEffectType == StatusEffectType.Lifesteal &&
            effect.target == CardTargetType.Self
        );

        return hasDamage && hasSelfLifesteal;
    }

    /// <summary>
    /// 현재 효과가 카드 한정 흡혈을 표현하기 위한 데이터인지 확인합니다.
    /// 이 효과는 지속 상태로 부여하지 않습니다.
    /// </summary>
    private bool IsImmediateLifestealEffect(
        CardEffectData effect)
    {
        return
            currentCardHasImmediateLifesteal &&
            effect != null &&
            effect.effectType == CardEffectType.ApplyStatus &&
            effect.statusEffectType == StatusEffectType.Lifesteal &&
            effect.target == CardTargetType.Self;
    }

    /// <summary>
    /// 선원을 소환합니다.
    /// value 수만큼 소환을 시도합니다.
    /// </summary>
    private void ExecuteSummonCrew(
        CardEffectData effect)
    {
        CrewManager crewManager =
            FindFirstObjectByType<CrewManager>();

        if (crewManager == null)
        {
            Debug.LogWarning(
                "[CardEffectExecutor] " +
                "CrewManager를 찾지 못했습니다."
            );

            return;
        }

        for (int i = 0;
             i < effect.value;
             i++)
        {
            if (!crewManager.CanSummon)
            {
                break;
            }

            crewManager.SummonCrew();
        }
    }

    /// <summary>
    /// 현재 소환된 모든 선원의 현재 체력과 최대 체력을
    /// 지정된 값으로 설정합니다.
    /// </summary>
    private void ExecuteSetCrewHealth(
        CardEffectData effect)
    {
        if (effect.target != CardTargetType.AllUndeads)
        {
            Debug.LogWarning(
                $"[CardEffectExecutor] 지원하지 않는 선원 체력 대상 : " +
                $"{effect.target}"
            );

            return;
        }

        CrewManager crewManager =
            FindFirstObjectByType<CrewManager>();

        if (crewManager == null)
        {
            Debug.LogWarning(
                "[CardEffectExecutor] 선원 체력 설정을 위한 " +
                "CrewManager를 찾지 못했습니다."
            );

            return;
        }

        int health = Mathf.Max(1, effect.value);
        List<Crew> crews = crewManager.Crews.ToList();

        foreach (Crew crew in crews)
        {
            if (crew == null || !crew.IsAlive)
            {
                continue;
            }

            crew.SetHealth(health, health);
        }

        Debug.Log(
            $"[CardEffectExecutor] 모든 선원 체력 설정 : " +
            $"{health}/{health} / {crews.Count}명"
        );
    }

    /// <summary>
    /// 선택한 적이 현재 보유한 작살 스택과 같은 수치의 피해를 줍니다.
    /// 이 카드 효과는 작살 스택을 소비하지 않습니다.
    /// </summary>
    private void ExecuteDamageEqualToHarpoonStack(
        Enemy targetEnemy)
    {
        if (!IsEnemyAlive(targetEnemy))
        {
            Debug.LogWarning(
                "[CardEffectExecutor] 작살 스택 피해를 줄 적이 없습니다."
            );

            return;
        }

        HarpoonStackController harpoonController =
            targetEnemy.GetComponent<HarpoonStackController>();

        if (harpoonController == null)
        {
            Debug.LogWarning(
                "[CardEffectExecutor] 대상 적에게 " +
                "HarpoonStackController가 없습니다.",
                targetEnemy
            );

            return;
        }

        int stackDamage =
            Mathf.Max(0, harpoonController.CurrentHarpoonStack);

        if (stackDamage > 0)
        {
            BattleCameraMotion.PlayPlayerAttack();
        }

        int blockBeforeDamage = targetEnemy.CurrentBlock;
        int actualDamage = targetEnemy.TakeDamage(stackDamage);

        PlayEnemyAttackSfx(
            targetEnemy,
            stackDamage,
            blockBeforeDamage,
            actualDamage,
            harpoonController.HasHarpoonStack,
            false
        );

        Debug.Log(
            $"[CardEffectExecutor] 작살 스택 동일 피해 : " +
            $"스택 {stackDamage} / 실제 피해 {actualDamage} / " +
            $"남은 스택 {harpoonController.CurrentHarpoonStack}",
            targetEnemy
        );
    }

    /// <summary>
    /// 먼저 소환된 선원부터 value 수만큼 희생하고
    /// 희생된 선원들의 현재 체력을 저장합니다.
    /// </summary>
    private void ExecuteSacrifice(
        CardEffectData effect,
        Crew targetCrew)
    {
        CrewManager crewManager =
            FindFirstObjectByType<CrewManager>();

        if (crewManager == null)
        {
            Debug.LogWarning(
                "[CardEffectExecutor] " +
                "CrewManager를 찾지 못했습니다."
            );

            return;
        }

        int sacrificedHealth;

        if (effect.value == 1 && targetCrew != null)
        {
            sacrificedHealth =
                crewManager.SacrificeCrew(targetCrew);
        }
        else
        {
            sacrificedHealth =
                crewManager.SacrificeCrews(effect.value);
        }

        sacrificedHealthThisCard +=
            sacrificedHealth;

        Debug.Log(
            $"[CardEffectExecutor] 선원 희생 처리 : " +
            $"{effect.value}명 / " +
            $"누적 희생 체력 {sacrificedHealthThisCard}"
        );
    }

    /// <summary>
    /// 현재 소환된 모든 선원을 동시에 희생하고
    /// 희생된 선원들의 현재 체력 합계를 저장합니다.
    /// </summary>
    private void ExecuteSacrificeAll()
    {
        CrewManager crewManager =
            FindFirstObjectByType<CrewManager>();

        if (crewManager == null)
        {
            Debug.LogWarning(
                "[CardEffectExecutor] " +
                "CrewManager를 찾지 못했습니다."
            );

            return;
        }

        int sacrificedHealth =
            crewManager.SacrificeAllCrews();

        sacrificedHealthThisCard +=
            sacrificedHealth;

        Debug.Log(
            $"[CardEffectExecutor] 모든 선원 희생 처리 / " +
            $"누적 희생 체력 {sacrificedHealthThisCard}"
        );
    }

    /// <summary>
    /// 플레이어에게 악마의 힘을 부여합니다.
    /// 다음에 사용하는 공격 카드의 피해가 두 배가 되며,
    /// 해당 공격 카드를 사용할 때 BattleManager에서 소비합니다.
    /// </summary>
    private void ExecuteDoubleNextAttackDamage()
    {
        PlayerCombat playerCombat =
            FindPlayerCombat();

        if (playerCombat == null)
        {
            Debug.LogWarning(
                "[CardEffectExecutor] 악마의 힘을 부여할 " +
                "PlayerCombat을 찾지 못했습니다."
            );

            return;
        }

        StatusEffectHandler statusEffectHandler =
            playerCombat.GetComponent<StatusEffectHandler>();

        if (statusEffectHandler == null)
        {
            Debug.LogWarning(
                "[CardEffectExecutor] 플레이어에게 " +
                "StatusEffectHandler가 없습니다."
            );

            return;
        }

        /*
         * 악마의 힘은 수치 중첩이 필요한 상태가 아니라
         * 다음 공격 카드 1회를 기다리는 조건부 상태입니다.
         */
        if (!statusEffectHandler.HasStatusEffect(
                StatusEffectType.DevilPower))
        {
            statusEffectHandler.AddStatusEffect(
                StatusEffectType.DevilPower,
                1,
                0,
                true
            );
        }

        Debug.Log(
            "[CardEffectExecutor] 악마의 힘 부여 : " +
            "다음 공격 카드 피해 2배"
        );
    }

    /// <summary>
    /// 현재 카드로 희생한 선원들의 현재 체력 합계만큼
    /// 플레이어에게 힘을 부여합니다.
    /// </summary>
    private void ExecuteMightEqualToSacrificedHealth()
    {
        if (sacrificedHealthThisCard <= 0)
        {
            Debug.LogWarning(
                "[CardEffectExecutor] 저장된 희생 체력이 없어 " +
                "힘을 획득하지 않습니다."
            );

            return;
        }

        PlayerCombat playerCombat =
            FindPlayerCombat();

        if (playerCombat == null)
        {
            Debug.LogWarning(
                "[CardEffectExecutor] " +
                "PlayerCombat을 찾지 못했습니다."
            );

            return;
        }

        StatusEffectHandler statusEffectHandler =
            playerCombat.GetComponent<StatusEffectHandler>();

        if (statusEffectHandler == null)
        {
            Debug.LogWarning(
                "[CardEffectExecutor] 플레이어에게 " +
                "StatusEffectHandler가 없습니다."
            );

            return;
        }

        statusEffectHandler.AddStatusEffect(
            StatusEffectType.Might,
            sacrificedHealthThisCard,
            0,
            true
        );

        Debug.Log(
            $"[CardEffectExecutor] 희생 체력만큼 힘 획득 : " +
            $"{sacrificedHealthThisCard}"
        );
    }

    /// <summary>
    /// 소환된 선원이 선택한 적에게 피해를 줍니다.
    /// 플레이어의 힘, 약화, 흡혈은 반영하지 않고
    /// 적에게 적용된 취약만 반영합니다.
    /// </summary>
    private void ExecuteCrewDealDamage(
        CardEffectData effect,
        Enemy targetEnemy)
    {
        if (!IsEnemyAlive(targetEnemy))
        {
            Debug.LogWarning(
                "[CardEffectExecutor] " +
                "선원이 공격할 Enemy가 없습니다."
            );

            return;
        }

        CrewManager crewManager =
            FindFirstObjectByType<CrewManager>();

        if (crewManager == null)
        {
            Debug.LogWarning(
                "[CardEffectExecutor] " +
                "CrewManager를 찾지 못했습니다."
            );

            return;
        }

        if (crewManager.CrewCount <= 0)
        {
            Debug.LogWarning(
                "[CardEffectExecutor] 소환된 선원이 없어 " +
                "선원 공격을 실행할 수 없습니다."
            );

            return;
        }

        int finalDamage =
            Mathf.Max(0, effect.value);

        int blockBeforeDamage = targetEnemy.CurrentBlock;
        bool hadHarpoonStack = HasHarpoonStack(targetEnemy);
        int actualDamage = targetEnemy.TakeDamage(finalDamage);

        PlayEnemyAttackSfx(
            targetEnemy,
            finalDamage,
            blockBeforeDamage,
            actualDamage,
            hadHarpoonStack,
            true
        );

        ApplyCaptainCrewPassive(
            targetEnemy
        );

        Debug.Log(
            $"[CardEffectExecutor] 선원 공격 : " +
            $"기본 피해 {finalDamage} / " +
            $"실제 피해 {actualDamage}"
        );
    }

    /// <summary>
    /// 현재 살아있는 모든 선원이 각각
    /// 무작위 적 한 명에게 지정된 피해를 한 번씩 줍니다.
    /// </summary>
    private void ExecuteAllCrewsDealDamageRandomEnemy(
        CardEffectData effect)
    {
        CrewManager crewManager =
            FindFirstObjectByType<CrewManager>();

        if (crewManager == null)
        {
            Debug.LogWarning(
                "[CardEffectExecutor] " +
                "CrewManager를 찾지 못했습니다."
            );

            return;
        }

        if (crewManager.CrewCount <= 0)
        {
            Debug.LogWarning(
                "[CardEffectExecutor] " +
                "공격할 선원이 없습니다."
            );

            return;
        }

        List<Enemy> activeEnemies =
            FindActiveEnemies();

        if (activeEnemies.Count <= 0)
        {
            Debug.LogWarning(
                "[CardEffectExecutor] 선원이 공격할 " +
                "살아있는 적이 없습니다."
            );

            return;
        }

        int damagePerCrew =
            Mathf.Max(0, effect.value);

        int attackCount = 0;

        List<Crew> attackingCrews =
            crewManager.Crews
                .Where(crew =>
                    crew != null &&
                    crew.IsAlive
                )
                .ToList();

        foreach (Crew crew in attackingCrews)
        {
            activeEnemies.RemoveAll(
                enemy =>
                    enemy == null ||
                    !enemy.gameObject.activeSelf ||
                    enemy.CurrentHP <= 0
            );

            if (activeEnemies.Count <= 0)
            {
                break;
            }

            int randomIndex =
                Random.Range(
                    0,
                    activeEnemies.Count
                );

            Enemy randomEnemy =
                activeEnemies[randomIndex];

            int blockBeforeDamage = randomEnemy.CurrentBlock;
            bool hadHarpoonStack = HasHarpoonStack(randomEnemy);
            int actualDamage = randomEnemy.TakeDamage(damagePerCrew);

            PlayEnemyAttackSfx(
                randomEnemy,
                damagePerCrew,
                blockBeforeDamage,
                actualDamage,
                hadHarpoonStack,
                true
            );

            ApplyCaptainCrewPassive(
                randomEnemy
            );

            attackCount++;

            Debug.Log(
                $"[CardEffectExecutor] 전체 선원 무작위 공격 : " +
                $"{crew.name} → {randomEnemy.name} / " +
                $"기본 피해 {damagePerCrew} / " +
                $"실제 피해 {actualDamage}"
            );
        }

        Debug.Log(
            $"[CardEffectExecutor] 전체 선원 공격 종료 : " +
            $"총 {attackCount}회 공격"
        );
    }

    /// <summary>
    /// 모든 살아있는 선원이 모든 살아있는 적에게
    /// 지정된 피해를 repeatCount만큼 반복해서 줍니다.
    /// </summary>
    private void ExecuteAllCrewsDealDamageAllEnemies(
        CardEffectData effect)
    {
        CrewManager crewManager =
            FindFirstObjectByType<CrewManager>();

        if (crewManager == null)
        {
            Debug.LogWarning(
                "[CardEffectExecutor] " +
                "CrewManager를 찾지 못했습니다."
            );

            return;
        }

        List<Crew> attackingCrews =
            crewManager.Crews
                .Where(crew =>
                    crew != null &&
                    crew.IsAlive
                )
                .ToList();

        if (attackingCrews.Count <= 0)
        {
            Debug.LogWarning(
                "[CardEffectExecutor] " +
                "공격할 선원이 없습니다."
            );

            return;
        }

        int damagePerHit =
            Mathf.Max(0, effect.value);

        int repeatCount =
            Mathf.Max(1, effect.repeatCount);

        int totalAttackCount = 0;

        for (int repeatIndex = 0;
             repeatIndex < repeatCount;
             repeatIndex++)
        {
            foreach (Crew crew in attackingCrews)
            {
                if (crew == null ||
                    !crew.IsAlive)
                {
                    continue;
                }

                List<Enemy> activeEnemies =
                    FindActiveEnemies();

                foreach (Enemy enemy in activeEnemies)
                {
                    if (!IsEnemyAlive(enemy))
                    {
                        continue;
                    }

                    int blockBeforeDamage = enemy.CurrentBlock;
                    bool hadHarpoonStack = HasHarpoonStack(enemy);
                    int actualDamage = enemy.TakeDamage(damagePerHit);

                    PlayEnemyAttackSfx(
                        enemy,
                        damagePerHit,
                        blockBeforeDamage,
                        actualDamage,
                        hadHarpoonStack,
                        true
                    );

                    ApplyCaptainCrewPassive(
                        enemy
                    );

                    totalAttackCount++;

                    Debug.Log(
                        $"[CardEffectExecutor] 전원 던져라 공격 : " +
                        $"{crew.name} → {enemy.name} / " +
                        $"{repeatIndex + 1}/{repeatCount}타 / " +
                        $"기본 피해 {damagePerHit} / " +
                        $"실제 피해 {actualDamage}"
                    );
                }
            }

            if (FindActiveEnemies().Count <= 0)
            {
                break;
            }
        }

        Debug.Log(
            $"[CardEffectExecutor] 전원 던져라 종료 : " +
            $"총 공격 횟수 {totalAttackCount}"
        );
    }

    /// <summary>
    /// 현재 소환된 모든 선원의 체력을 회복합니다.
    /// </summary>
    private void ExecuteHealAllCrews(
        CardEffectData effect)
    {
        CrewManager crewManager =
            FindFirstObjectByType<CrewManager>();

        if (crewManager == null)
        {
            Debug.LogWarning(
                "[CardEffectExecutor] " +
                "CrewManager를 찾지 못했습니다."
            );

            return;
        }

        if (crewManager.CrewCount <= 0)
        {
            Debug.LogWarning(
                "[CardEffectExecutor] " +
                "회복할 선원이 없습니다."
            );

            return;
        }

        int totalHealedAmount =
            crewManager.HealAllCrews(
                effect.value
            );

        Debug.Log(
            $"[CardEffectExecutor] 모든 선원 체력 회복 / " +
            $"총 실제 회복량 {totalHealedAmount}"
        );
    }

    /// <summary>
    /// 플레이어가 방어도를 획득합니다.
    /// </summary>
    private void ExecuteGainBlock(
        CardEffectData effect)
    {
        PlayerCombat playerCombat =
            FindPlayerCombat();

        if (playerCombat == null)
        {
            Debug.LogWarning(
                "[CardEffectExecutor] " +
                "PlayerCombat을 찾지 못했습니다."
            );

            return;
        }

        int blockBeforeGain = playerCombat.CurrentBlock;

        playerCombat.GainBlock(
            effect.value
        );

        if (playerCombat.CurrentBlock > blockBeforeGain)
        {
            currentCardGainedBlock = true;
        }
    }

    /// <summary>
    /// 플레이어의 체력을 감소시킵니다.
    /// </summary>
    private void ExecuteLoseHealth(
        CardEffectData effect)
    {
        PlayerCombat playerCombat =
            FindPlayerCombat();

        if (playerCombat == null)
        {
            Debug.LogWarning(
                "[CardEffectExecutor] " +
                "PlayerCombat을 찾지 못했습니다."
            );

            return;
        }

        playerCombat.LoseHealth(
            effect.value
        );
    }

    /// <summary>
    /// 플레이어의 체력을 회복합니다.
    /// </summary>
    private void ExecuteHeal(
        CardEffectData effect)
    {
        PlayerCombat playerCombat =
            FindPlayerCombat();

        if (playerCombat == null)
        {
            Debug.LogWarning(
                "[CardEffectExecutor] " +
                "PlayerCombat을 찾지 못했습니다."
            );

            return;
        }

        playerCombat.Heal(
            effect.value
        );
    }

    /// <summary>
    /// 이번 턴 체력 손실 여부에 따라
    /// 방어도를 획득합니다.
    /// </summary>
    private void ExecuteGainBlockOnHealthLossThisTurn(
        CardEffectData effect)
    {
        PlayerCombat playerCombat =
            FindPlayerCombat();

        if (playerCombat == null)
        {
            Debug.LogWarning(
                "[CardEffectExecutor] " +
                "PlayerCombat을 찾지 못했습니다."
            );

            return;
        }

        if (playerCombat.DamagedThisTurn)
        {
            int blockBeforeGain = playerCombat.CurrentBlock;

            playerCombat.GainBlock(
                effect.value
            );

            if (playerCombat.CurrentBlock > blockBeforeGain)
            {
                currentCardGainedBlock = true;
            }

            Debug.Log(
                $"[CardEffectExecutor] 체력 손실 조건 " +
                $"방어도 획득 : {effect.value}"
            );
        }
        else
        {
            Debug.Log(
                "[CardEffectExecutor] 이번 턴 체력 손실이 없어 " +
                "추가 방어도를 얻지 않습니다."
            );
        }
    }

    /// <summary>
    /// 상태 효과를 지정된 대상에게 부여합니다.
    /// </summary>
    private void ExecuteApplyStatus(
        CardEffectData effect,
        Enemy targetEnemy)
    {
        if (effect.target ==
            CardTargetType.Self)
        {
            PlayerCombat playerCombat =
                FindPlayerCombat();

            if (playerCombat == null)
            {
                Debug.LogWarning(
                    "[CardEffectExecutor] " +
                    "PlayerCombat을 찾지 못했습니다."
                );

                return;
            }

            StatusEffectHandler statusEffectHandler =
                playerCombat.GetComponent<StatusEffectHandler>();

            ApplyStatusToHandler(
                statusEffectHandler,
                effect
            );

            return;
        }

        if (effect.target ==
            CardTargetType.Enemy)
        {
            if (!IsEnemyAlive(targetEnemy))
            {
                Debug.LogWarning(
                    "[CardEffectExecutor] 상태 효과를 부여할 " +
                    "Enemy가 없습니다."
                );

                return;
            }

            StatusEffectHandler statusEffectHandler =
                targetEnemy.GetComponent<StatusEffectHandler>();

            ApplyStatusToHandler(
                statusEffectHandler,
                effect
            );

            return;
        }

        if (effect.target ==
            CardTargetType.AllEnemies)
        {
            List<Enemy> activeEnemies =
                FindActiveEnemies();

            foreach (Enemy enemy in activeEnemies)
            {
                StatusEffectHandler statusEffectHandler =
                    enemy.GetComponent<StatusEffectHandler>();

                ApplyStatusToHandler(
                    statusEffectHandler,
                    effect
                );
            }

            Debug.Log(
                $"[CardEffectExecutor] 모든 적에게 상태 효과 부여 : " +
                $"{effect.statusEffectType}"
            );

            return;
        }

        Debug.LogWarning(
            $"[CardEffectExecutor] 처리되지 않은 상태 효과 대상 : " +
            $"{effect.target}"
        );
    }

    /// <summary>
    /// 대상의 StatusEffectHandler에
    /// 상태 효과를 등록합니다.
    /// </summary>
    private void ApplyStatusToHandler(
        StatusEffectHandler statusEffectHandler,
        CardEffectData effect)
    {
        if (statusEffectHandler == null)
        {
            Debug.LogWarning(
                "[CardEffectExecutor] 대상에 " +
                "StatusEffectHandler가 없습니다."
            );

            return;
        }

        bool isPermanent = false;
        int remainingTurn =
            effect.value;

        /*
         * 전투 종료까지 유지되는 상태 효과입니다.
         */
        if (effect.statusEffectType == StatusEffectType.Might ||
    effect.statusEffectType == StatusEffectType.Guard ||
    effect.statusEffectType == StatusEffectType.Resist ||
    effect.statusEffectType == StatusEffectType.Immortal ||
    effect.statusEffectType == StatusEffectType.Jinx)
        {
            isPermanent = true;
            remainingTurn = 0;
        }

        /*
         * Lifesteal은 부여된 현재 턴에만 유지됩니다.
         */
        if (effect.statusEffectType ==
            StatusEffectType.Lifesteal)
        {
            isPermanent = false;
            remainingTurn = 1;
        }

        /*
         * Echo는 부여된 현재 턴에만 유지됩니다.
         * 공격 카드 사용 시 BattleManager에서 즉시 제거됩니다.
         */
        if (effect.statusEffectType ==
            StatusEffectType.Echo)
        {
            isPermanent = false;
            remainingTurn = 1;
        }

        /*
         * 힘 감소는 적이 이번 턴에 행동한 뒤 제거됩니다.
         */
        if (effect.statusEffectType ==
            StatusEffectType.MightReduction)
        {
            isPermanent = false;
            remainingTurn = 1;
        }

        /*
         * Toxic은 remainingTurn이 아닌 value가 감소합니다.
         */
        if (effect.statusEffectType ==
            StatusEffectType.Toxic)
        {
            isPermanent = false;
            remainingTurn = 0;
        }

        if (IsFixedStrengthDurationDebuff(effect.statusEffectType))
        {
            statusEffectHandler.AddStatusEffectWithDurationStack(
                effect.statusEffectType,
                effect.value,
                remainingTurn
            );
        }
        else
        {
            statusEffectHandler.AddStatusEffect(
                effect.statusEffectType,
                effect.value,
                remainingTurn,
                isPermanent
            );
        }

        Debug.Log(
            $"[CardEffectExecutor] 상태 효과 부여 : " +
            $"{effect.statusEffectType} / " +
            $"수치 : {effect.value} / " +
            $"지속 턴 : {remainingTurn} / " +
            $"영구 여부 : {isPermanent}"
        );
    }

    /// <summary>
    /// 효과 강도는 고정되고 남은 지속 턴만 중첩되는 디버프인지 반환합니다.
    /// </summary>
    private bool IsFixedStrengthDurationDebuff(
        StatusEffectType statusEffectType)
    {
        switch (statusEffectType)
        {
            case StatusEffectType.Weaken:
            case StatusEffectType.Vulnerable:
            case StatusEffectType.Cripple:
            case StatusEffectType.NoBlock:
            case StatusEffectType.Broken:
                return true;

            default:
                return false;
        }
    }

    /// <summary>
    /// 적에게 적용된 공격 결과를 전투 SFX 규칙에 전달합니다.
    /// </summary>
    private void PlayEnemyAttackSfx(
        Enemy targetEnemy,
        int requestedDamage,
        int blockBeforeDamage,
        int actualHealthDamage,
        bool hadHarpoonStack,
        bool isCrewAttack)
    {
        if (SFXManager.Instance == null || requestedDamage <= 0)
        {
            return;
        }

        currentCardHadAttack = true;

        bool wasFullyBlocked =
            blockBeforeDamage > 0 &&
            actualHealthDamage <= 0;

        SFXManager.Instance.PlayAttackHitSequence(
            actualHealthDamage,
            wasFullyBlocked,
            hadHarpoonStack,
            isCrewAttack
        );
    }

    private bool HasHarpoonStack(Enemy targetEnemy)
    {
        if (targetEnemy == null)
        {
            return false;
        }

        HarpoonStackController harpoonController =
            targetEnemy.GetComponent<HarpoonStackController>();

        return
            harpoonController != null &&
            harpoonController.HasHarpoonStack;
    }

    private void RecordStatusSfx(StatusEffectType statusEffectType)
    {
        if (statusEffectType == StatusEffectType.None ||
            statusEffectType == StatusEffectType.Exit)
        {
            return;
        }

        if (IsDebuffStatus(statusEffectType))
        {
            currentCardDebuffTypes.Add(statusEffectType);
            return;
        }

        currentCardBuffTypes.Add(statusEffectType);
    }

    private bool IsDebuffStatus(StatusEffectType statusEffectType)
    {
        switch (statusEffectType)
        {
            case StatusEffectType.Weaken:
            case StatusEffectType.Vulnerable:
            case StatusEffectType.Cripple:
            case StatusEffectType.NoBlock:
            case StatusEffectType.Broken:
            case StatusEffectType.Jinx:
            case StatusEffectType.Paralyze:
            case StatusEffectType.Toxic:
            case StatusEffectType.MightReduction:
                return true;

            default:
                return false;
        }
    }

    /// <summary>
    /// 현재 전투에 생성된 플레이어의
    /// PlayerAnimationController를 찾아 반환합니다.
    /// </summary>
    private PlayerAnimationController FindPlayerAnimationController()
    {
        PlayerCombat playerCombat =
            FindPlayerCombat();

        if (playerCombat == null)
        {
            Debug.LogWarning(
                "[CardEffectExecutor] " +
                "PlayerCombat을 찾지 못해 " +
                "공격 애니메이션을 실행할 수 없습니다."
            );

            return null;
        }

        PlayerAnimationController animationController =
            playerCombat.GetComponent<PlayerAnimationController>();

        if (animationController == null)
        {
            animationController =
                playerCombat.GetComponentInChildren<PlayerAnimationController>();
        }

        if (animationController == null)
        {
            Debug.LogWarning(
                $"[CardEffectExecutor] " +
                $"{playerCombat.name}에 " +
                "PlayerAnimationController가 없습니다.",
                playerCombat
            );
        }

        return animationController;
    }

    /// <summary>
    /// Player 태그를 가진 오브젝트들 중
    /// PlayerCombat이 붙은 오브젝트를 찾습니다.
    /// </summary>
    private PlayerCombat FindPlayerCombat()
    {
        GameObject[] playerObjects =
            GameObject.FindGameObjectsWithTag(
                playerTag
            );

        foreach (GameObject playerObject in playerObjects)
        {
            PlayerCombat playerCombat =
                playerObject.GetComponent<PlayerCombat>();

            if (playerCombat != null)
            {
                return playerCombat;
            }
        }

        return null;
    }
}
