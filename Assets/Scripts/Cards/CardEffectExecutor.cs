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

    /// <summary>
    /// 현재 실행 중인 카드가 희생한 선원들의 현재 체력 합계입니다.
    /// 카드 실행이 시작될 때마다 0으로 초기화됩니다.
    /// </summary>
    private int sacrificedHealthThisCard;

    /// <summary>
    /// 카드 효과 목록을 실행합니다.
    /// </summary>
    public void ExecuteEffects(CardData cardData, Enemy targetEnemy)
    {
        if (cardData == null)
        {
            Debug.LogWarning(
                "[CardEffectExecutor] 실행할 카드 데이터가 없습니다."
            );

            return;
        }

        if (cardData.effects == null ||
            cardData.effects.Count == 0)
        {
            Debug.LogWarning(
                $"[CardEffectExecutor] {cardData.cardName} " +
                "카드에 효과가 없습니다."
            );

            return;
        }

        sacrificedHealthThisCard = 0;

        List<CardEffectData> orderedEffects = cardData.effects
            .OrderBy(effect => effect.order)
            .ToList();

        foreach (CardEffectData effect in orderedEffects)
        {
            ExecuteSingleEffect(effect, targetEnemy);
        }
    }

    /// <summary>
    /// 카드 효과 하나를 실행합니다.
    /// </summary>
    private void ExecuteSingleEffect(
        CardEffectData effect,
        Enemy targetEnemy)
    {
        switch (effect.effectType)
        {
            case CardEffectType.DealDamage:
                ExecuteDealDamage(effect, targetEnemy);
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
                ExecuteApplyStatus(effect, targetEnemy);
                break;

            case CardEffectType.HarpoonerStack:
            case CardEffectType.ApplyHarpoon:
                Debug.Log(
                    $"[CardEffectExecutor] 작살 스택 부여 예정 : " +
                    $"{effect.value}"
                );
                break;

            case CardEffectType.DrawCard:
                Debug.Log(
                    $"[CardEffectExecutor] 카드 드로우 예정 : " +
                    $"{effect.value}"
                );
                break;

            case CardEffectType.Summon:
            case CardEffectType.SummonCrew:
                ExecuteSummonCrew(effect);
                break;

            case CardEffectType.CrewDealDamage:
                ExecuteCrewDealDamage(effect, targetEnemy);
                break;

            case CardEffectType.AllCrewsDealDamageRandomEnemy:
                ExecuteAllCrewsDealDamageRandomEnemy(effect);
                break;

            case CardEffectType.AllCrewsDealDamageAllEnemies:
                ExecuteAllCrewsDealDamageAllEnemies(effect);
                break;

            case CardEffectType.GainBlockOnHealthLossThisTurn:
                ExecuteGainBlockOnHealthLossThisTurn(effect);
                break;

            case CardEffectType.DoubleNextAttackDamage:
                Debug.Log(
                    $"[CardEffectExecutor] 다음 공격 강화 예정 : " +
                    $"{effect.value}"
                );
                break;

            case CardEffectType.Sacrifice:
                ExecuteSacrifice(effect);
                break;

            case CardEffectType.SacrificeAll:
                ExecuteSacrificeAll();
                break;

            case CardEffectType.SetMaxHealth:
                Debug.Log(
                    $"[CardEffectExecutor] 최대 체력 설정 예정 : " +
                    $"{effect.value}"
                );
                break;

            case CardEffectType.MightEqualToSacrificedHealth:
                ExecuteMightEqualToSacrificedHealth();
                break;

            case CardEffectType.DealDamageEqualToHarpoonerStack:
                Debug.Log(
                    "[CardEffectExecutor] 작살 스택만큼 피해 예정"
                );
                break;
        }
    }

    /// <summary>
    /// 선원을 소환합니다.
    /// value 수만큼 소환을 시도합니다.
    /// </summary>
    private void ExecuteSummonCrew(CardEffectData effect)
    {
        CrewManager crewManager =
            FindFirstObjectByType<CrewManager>();

        if (crewManager == null)
        {
            Debug.LogWarning(
                "[CardEffectExecutor] CrewManager를 찾지 못했습니다."
            );

            return;
        }

        for (int i = 0; i < effect.value; i++)
        {
            crewManager.SummonCrew();
        }
    }

    /// <summary>
    /// 먼저 소환된 선원부터 value 수만큼 희생하고
    /// 희생된 선원들의 현재 체력을 저장합니다.
    /// </summary>
    private void ExecuteSacrifice(CardEffectData effect)
    {
        CrewManager crewManager =
            FindFirstObjectByType<CrewManager>();

        if (crewManager == null)
        {
            Debug.LogWarning(
                "[CardEffectExecutor] CrewManager를 찾지 못했습니다."
            );

            return;
        }

        int sacrificedHealth =
            crewManager.SacrificeCrews(effect.value);

        sacrificedHealthThisCard += sacrificedHealth;

        Debug.Log(
            $"[CardEffectExecutor] 선원 희생 처리 : " +
            $"{effect.value}명 / 누적 희생 체력 " +
            $"{sacrificedHealthThisCard}"
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
                "[CardEffectExecutor] CrewManager를 찾지 못했습니다."
            );

            return;
        }

        int sacrificedHealth =
            crewManager.SacrificeAllCrews();

        sacrificedHealthThisCard += sacrificedHealth;

        Debug.Log(
            $"[CardEffectExecutor] 모든 선원 희생 처리 / " +
            $"누적 희생 체력 {sacrificedHealthThisCard}"
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

        PlayerCombat playerCombat = FindPlayerCombat();

        if (playerCombat == null)
        {
            Debug.LogWarning(
                "[CardEffectExecutor] PlayerCombat을 찾지 못했습니다."
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
    /// 소환된 선원이 적에게 피해를 줍니다.
    /// 플레이어에게 적용된 힘, 약화, 흡혈은 반영하지 않고
    /// 적에게 적용된 취약만 반영합니다.
    /// </summary>
    private void ExecuteCrewDealDamage(
        CardEffectData effect,
        Enemy targetEnemy)
    {
        if (targetEnemy == null)
        {
            Debug.LogWarning(
                "[CardEffectExecutor] 선원이 공격할 Enemy가 없습니다."
            );

            return;
        }

        CrewManager crewManager =
            FindFirstObjectByType<CrewManager>();

        if (crewManager == null)
        {
            Debug.LogWarning(
                "[CardEffectExecutor] CrewManager를 찾지 못했습니다."
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

        int finalDamage = Mathf.Max(0, effect.value);

        /*
         * 플레이어의 Might, Weaken, Lifesteal은 확인하지 않습니다.
         * 적의 Vulnerable은 Enemy.TakeDamage() 내부에서 적용됩니다.
         */
        int actualDamage =
            targetEnemy.TakeDamage(finalDamage);

        Debug.Log(
            $"[CardEffectExecutor] 선원 공격 : " +
            $"기본 피해 {finalDamage} / 실제 피해 {actualDamage}"
        );
    }

    /// <summary>
    /// 현재 살아있는 모든 선원이 각각 무작위 적 한 명에게
    /// 지정된 피해를 한 번씩 줍니다.
    /// 플레이어의 힘, 약화, 흡혈은 적용되지 않으며
    /// 공격 대상에게 적용된 취약은 반영됩니다.
    /// </summary>
    private void ExecuteAllCrewsDealDamageRandomEnemy(
        CardEffectData effect)
    {
        CrewManager crewManager =
            FindFirstObjectByType<CrewManager>();

        if (crewManager == null)
        {
            Debug.LogWarning(
                "[CardEffectExecutor] CrewManager를 찾지 못했습니다."
            );

            return;
        }

        if (crewManager.CrewCount <= 0)
        {
            Debug.LogWarning(
                "[CardEffectExecutor] 공격할 선원이 없습니다."
            );

            return;
        }

        Enemy[] foundEnemies = FindObjectsByType<Enemy>(
            FindObjectsSortMode.None
        );

        List<Enemy> activeEnemies = new List<Enemy>();

        foreach (Enemy enemy in foundEnemies)
        {
            if (enemy == null)
            {
                continue;
            }

            if (!enemy.gameObject.activeSelf)
            {
                continue;
            }

            if (enemy.CurrentHP <= 0)
            {
                continue;
            }

            activeEnemies.Add(enemy);
        }

        if (activeEnemies.Count <= 0)
        {
            Debug.LogWarning(
                "[CardEffectExecutor] 선원이 공격할 살아있는 적이 없습니다."
            );

            return;
        }

        int damagePerCrew = Mathf.Max(0, effect.value);
        int attackCount = 0;

        /*
         * 현재 선원 목록을 복사합니다.
         * 공격 중 적 사망이나 다른 처리로 목록 상태가 바뀌더라도
         * 안전하게 순회하기 위한 처리입니다.
         */
        List<Crew> attackingCrews =
            crewManager.Crews
                .Where(crew => crew != null && crew.IsAlive)
                .ToList();

        foreach (Crew crew in attackingCrews)
        {
            /*
             * 앞선 선원 공격으로 적이 사망할 수 있으므로
             * 매 공격 전에 살아있는 적 목록을 다시 정리합니다.
             */
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

            int randomIndex = Random.Range(
                0,
                activeEnemies.Count
            );

            Enemy randomEnemy = activeEnemies[randomIndex];

            int actualDamage =
                randomEnemy.TakeDamage(damagePerCrew);

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
    /// 플레이어의 힘, 약화, 흡혈은 적용되지 않고
    /// 적의 취약은 적용됩니다.
    /// </summary>
    private void ExecuteAllCrewsDealDamageAllEnemies(
        CardEffectData effect)
    {
        CrewManager crewManager =
            FindFirstObjectByType<CrewManager>();

        if (crewManager == null)
        {
            Debug.LogWarning(
                "[CardEffectExecutor] CrewManager를 찾지 못했습니다."
            );

            return;
        }

        List<Crew> attackingCrews =
            crewManager.Crews
                .Where(crew => crew != null && crew.IsAlive)
                .ToList();

        if (attackingCrews.Count <= 0)
        {
            Debug.LogWarning(
                "[CardEffectExecutor] 공격할 선원이 없습니다."
            );

            return;
        }

        int damagePerHit = Mathf.Max(0, effect.value);
        int repeatCount = Mathf.Max(1, effect.repeatCount);
        int totalAttackCount = 0;

        for (int repeatIndex = 0;
             repeatIndex < repeatCount;
             repeatIndex++)
        {
            foreach (Crew crew in attackingCrews)
            {
                if (crew == null || !crew.IsAlive)
                {
                    continue;
                }

                Enemy[] enemies = FindObjectsByType<Enemy>(
                    FindObjectsSortMode.None
                );

                foreach (Enemy enemy in enemies)
                {
                    if (enemy == null)
                    {
                        continue;
                    }

                    if (!enemy.gameObject.activeSelf)
                    {
                        continue;
                    }

                    if (enemy.CurrentHP <= 0)
                    {
                        continue;
                    }

                    int actualDamage =
                        enemy.TakeDamage(damagePerHit);

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

            Enemy[] remainingEnemies =
                FindObjectsByType<Enemy>(
                    FindObjectsSortMode.None
                );

            bool hasAliveEnemy = remainingEnemies.Any(
                enemy =>
                    enemy != null &&
                    enemy.gameObject.activeSelf &&
                    enemy.CurrentHP > 0
            );

            if (!hasAliveEnemy)
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
    private void ExecuteHealAllCrews(CardEffectData effect)
    {
        CrewManager crewManager =
            FindFirstObjectByType<CrewManager>();

        if (crewManager == null)
        {
            Debug.LogWarning(
                "[CardEffectExecutor] CrewManager를 찾지 못했습니다."
            );

            return;
        }

        if (crewManager.CrewCount <= 0)
        {
            Debug.LogWarning(
                "[CardEffectExecutor] 회복할 선원이 없습니다."
            );

            return;
        }

        int totalHealedAmount =
            crewManager.HealAllCrews(effect.value);

        Debug.Log(
            $"[CardEffectExecutor] 모든 선원 체력 회복 / " +
            $"총 실제 회복량 {totalHealedAmount}"
        );
    }

    /// <summary>
    /// 단일 적에게 피해를 줍니다.
    /// 힘과 약화를 반영한 후 실제 피해량만큼 흡혈을 처리합니다.
    /// </summary>
    private void ExecuteDealDamage(
        CardEffectData effect,
        Enemy targetEnemy)
    {
        if (targetEnemy == null)
        {
            Debug.LogWarning(
                "[CardEffectExecutor] 공격 대상 Enemy가 " +
                "연결되지 않았습니다."
            );

            return;
        }

        PlayerCombat playerCombat = FindPlayerCombat();

        int finalDamage = effect.value;

        StatusEffectHandler playerStatusEffectHandler = null;

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
                        $"기본 {effect.value} + 힘 {mightValue} " +
                        $"= {finalDamage}"
                    );
                }

                if (playerStatusEffectHandler.HasStatusEffect(
                    StatusEffectType.Weaken))
                {
                    int reducedDamage =
                        Mathf.FloorToInt(finalDamage * 0.6f);

                    Debug.Log(
                        $"[CardEffectExecutor] 약화 적용 : " +
                        $"{finalDamage} → {reducedDamage}"
                    );

                    finalDamage = reducedDamage;
                }
            }
        }

        if (finalDamage < 0)
        {
            finalDamage = 0;
        }

        int actualDamage =
            targetEnemy.TakeDamage(finalDamage);

        ProcessLifesteal(
            playerCombat,
            playerStatusEffectHandler,
            actualDamage
        );
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

        if (statusEffectHandler == null)
        {
            return;
        }

        if (actualDamage <= 0)
        {
            return;
        }

        if (!statusEffectHandler.HasStatusEffect(
            StatusEffectType.Lifesteal
        ))
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
    /// 플레이어가 방어도를 획득합니다.
    /// </summary>
    private void ExecuteGainBlock(CardEffectData effect)
    {
        PlayerCombat playerCombat = FindPlayerCombat();

        if (playerCombat == null)
        {
            Debug.LogWarning(
                "[CardEffectExecutor] PlayerCombat을 찾지 못했습니다."
            );

            return;
        }

        playerCombat.GainBlock(effect.value);
    }

    /// <summary>
    /// 플레이어의 체력을 감소시킵니다.
    /// </summary>
    private void ExecuteLoseHealth(CardEffectData effect)
    {
        PlayerCombat playerCombat = FindPlayerCombat();

        if (playerCombat == null)
        {
            Debug.LogWarning(
                "[CardEffectExecutor] PlayerCombat을 찾지 못했습니다."
            );

            return;
        }

        playerCombat.LoseHealth(effect.value);
    }

    /// <summary>
    /// 플레이어의 체력을 회복합니다.
    /// </summary>
    private void ExecuteHeal(CardEffectData effect)
    {
        PlayerCombat playerCombat = FindPlayerCombat();

        if (playerCombat == null)
        {
            Debug.LogWarning(
                "[CardEffectExecutor] PlayerCombat을 찾지 못했습니다."
            );

            return;
        }

        playerCombat.Heal(effect.value);
    }

    /// <summary>
    /// 이번 턴 체력 손실 여부에 따라 방어도를 획득합니다.
    /// </summary>
    private void ExecuteGainBlockOnHealthLossThisTurn(
        CardEffectData effect)
    {
        PlayerCombat playerCombat = FindPlayerCombat();

        if (playerCombat == null)
        {
            Debug.LogWarning(
                "[CardEffectExecutor] PlayerCombat을 찾지 못했습니다."
            );

            return;
        }

        if (playerCombat.DamagedThisTurn)
        {
            playerCombat.GainBlock(effect.value);

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
        if (effect.target == CardTargetType.Self)
        {
            PlayerCombat playerCombat = FindPlayerCombat();

            if (playerCombat == null)
            {
                Debug.LogWarning(
                    "[CardEffectExecutor] PlayerCombat을 " +
                    "찾지 못했습니다."
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

        if (effect.target == CardTargetType.Enemy)
        {
            if (targetEnemy == null)
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

        if (effect.target == CardTargetType.AllEnemies)
        {
            Enemy[] enemies = FindObjectsByType<Enemy>(
                FindObjectsSortMode.None
            );

            foreach (Enemy enemy in enemies)
            {
                if (enemy == null)
                {
                    continue;
                }

                if (!enemy.gameObject.activeSelf)
                {
                    continue;
                }

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
    /// 대상의 StatusEffectHandler에 상태 효과를 등록합니다.
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
        int remainingTurn = effect.value;

        if (effect.statusEffectType == StatusEffectType.Might ||
    effect.statusEffectType == StatusEffectType.Guard ||
    effect.statusEffectType == StatusEffectType.Resist ||
    effect.statusEffectType == StatusEffectType.Immortal)
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
         * 공격 카드를 사용하지 않으면 턴 종료 시 제거됩니다.
         */
        if (effect.statusEffectType ==
            StatusEffectType.Echo)
        {
            isPermanent = false;
            remainingTurn = 1;
        }

        if (effect.statusEffectType ==
            StatusEffectType.Toxic)
        {
            isPermanent = false;
            remainingTurn = 0;
        }

        statusEffectHandler.AddStatusEffect(
            effect.statusEffectType,
            effect.value,
            remainingTurn,
            isPermanent
        );

        Debug.Log(
            $"[CardEffectExecutor] 상태 효과 부여 : " +
            $"{effect.statusEffectType} / 수치 : {effect.value} / " +
            $"지속 턴 : {remainingTurn}"
        );
    }

    /// <summary>
    /// Player 태그를 가진 오브젝트들 중
    /// PlayerCombat이 붙은 오브젝트를 찾습니다.
    /// </summary>
    private PlayerCombat FindPlayerCombat()
    {
        GameObject[] playerObjects =
            GameObject.FindGameObjectsWithTag(playerTag);

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