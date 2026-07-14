using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// 전투 중 턴 상태와 카드 사용 제한을 관리하는 클래스입니다.
/// 플레이어 턴, 적 턴, 공격/방어 카드 사용 횟수,
/// 상태 효과 처리, 선원 성장과 다음 턴 드로우를 담당합니다.
/// </summary>
public class TurnManager : MonoBehaviour
{
    [Header("현재 플레이어 턴 여부")]
    [SerializeField]
    private bool isPlayerTurn;

    [Header("Hand Manager")]
    [SerializeField]
    private HandManager handManager;

    [Header("손패 최대 장수")]
    [SerializeField]
    private int maxHandCount = 4;

    [Header("공격/방어 카드 최대 사용 횟수")]
    [SerializeField]
    private int maxAttackDefenseCardUseCount = 2;

    [Header("현재 공격/방어 카드 사용 횟수")]
    [SerializeField]
    private int currentAttackDefenseCardUseCount;

    [Header("공격/방어 카드 사용 횟수 UI")]
    [SerializeField]
    private TextMeshProUGUI attackDefenseUseCountText;

    [Header("적 턴 설정")]
    [SerializeField]
    private float enemyTurnDelay = 0.7f;

    /// <summary>
    /// 현재 플레이어 턴인지 반환합니다.
    /// </summary>
    public bool IsPlayerTurn => isPlayerTurn;

    /// <summary>
    /// 현재 공격/방어 카드 사용 횟수입니다.
    /// </summary>
    public int CurrentAttackDefenseCardUseCount =>
        currentAttackDefenseCardUseCount;

    /// <summary>
    /// 공격/방어 카드의 한 턴 최대 사용 횟수입니다.
    /// </summary>
    public int MaxAttackDefenseCardUseCount =>
        maxAttackDefenseCardUseCount;

    private void Start()
    {
        StartPlayerTurn();
    }

    /// <summary>
    /// 플레이어 턴을 시작합니다.
    /// 무감각을 제거하고 방어도를 초기화한 뒤,
    /// 선원을 성장시키고 카드를 드로우합니다.
    /// </summary>
    public void StartPlayerTurn()
    {
        isPlayerTurn = true;
        currentAttackDefenseCardUseCount = 0;

        PlayerCombat playerCombat =
            FindFirstObjectByType<PlayerCombat>();

        if (playerCombat != null)
        {
            StatusEffectHandler statusEffectHandler =
                playerCombat.GetComponent<StatusEffectHandler>();

            if (statusEffectHandler != null)
            {
                statusEffectHandler.RemoveStatusEffect(
                    StatusEffectType.Resist
                );
            }

            playerCombat.ClearBlock();
        }
        else
        {
            Debug.LogWarning(
                "[TurnManager] PlayerCombat을 찾지 못해 " +
                "방어도를 초기화하지 못했습니다."
            );
        }

        GrowAllCrews();
        DrawCardsForNewTurn();

        if (handManager != null)
        {
            handManager.ApplyJinxToRandomCard();
        }

        UpdateAttackDefenseUseCountUI();

        Debug.Log("[TurnManager] 플레이어 턴 시작");
    }

    /// <summary>
    /// 플레이어 턴을 종료하고 적 턴을 시작합니다.
    /// </summary>
    public void EndPlayerTurnAndStartNextTurn()
    {
        if (!isPlayerTurn)
        {
            Debug.LogWarning(
                "[TurnManager] 현재 플레이어 턴이 아닙니다."
            );

            return;
        }

        EndPlayerTurn();
        StartCoroutine(EnemyTurnRoutine());
    }

    /// <summary>
    /// 플레이어 턴 종료 처리를 수행합니다.
    /// 중독 피해를 먼저 처리한 후 지속 턴을 감소시킵니다.
    /// </summary>
    public void EndPlayerTurn()
    {
        if (!isPlayerTurn)
        {
            Debug.LogWarning(
                "[TurnManager] 현재 플레이어 턴이 아닙니다."
            );

            return;
        }

        ProcessPlayerToxic();
        DecreasePlayerStatusDuration();

        PlayerCombat playerCombat =
            FindFirstObjectByType<PlayerCombat>();

        if (playerCombat != null)
        {
            playerCombat.EndTurn();
        }

        isPlayerTurn = false;

        Debug.Log("[TurnManager] 플레이어 턴 종료");
    }

    /// <summary>
    /// 적 턴을 순서대로 진행합니다.
    /// </summary>
    private IEnumerator EnemyTurnRoutine()
    {
        Debug.Log("[TurnManager] 적 턴 시작");

        yield return new WaitForSeconds(enemyTurnDelay);

        ExecuteEnemyTurn();

        ProcessEnemyToxic();
        DecreaseEnemyStatusDuration();

        yield return new WaitForSeconds(enemyTurnDelay);

        Debug.Log("[TurnManager] 적 턴 종료");

        StartPlayerTurn();
    }

    /// <summary>
    /// 모든 활성화된 적의 행동을 실행합니다.
    /// </summary>
    private void ExecuteEnemyTurn()
    {
        PlayerCombat playerCombat =
            FindFirstObjectByType<PlayerCombat>();

        if (playerCombat == null)
        {
            Debug.LogError(
                "[TurnManager] 씬에서 PlayerCombat을 찾지 못했습니다."
            );

            return;
        }

        Enemy[] enemies = FindObjectsByType<Enemy>(
            FindObjectsSortMode.None
        );

        if (enemies.Length == 0)
        {
            Debug.LogWarning(
                "[TurnManager] 행동할 Enemy가 없습니다."
            );

            return;
        }

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

            enemy.TakeTurn(playerCombat);
        }
    }

    /// <summary>
    /// 플레이어의 중독을 발동합니다.
    /// 중독 피해는 선원이 대신 받지 않습니다.
    /// </summary>
    private void ProcessPlayerToxic()
    {
        PlayerCombat playerCombat =
            FindFirstObjectByType<PlayerCombat>();

        if (playerCombat == null)
        {
            Debug.LogWarning(
                "[TurnManager] 플레이어 중독을 처리할 " +
                "PlayerCombat을 찾지 못했습니다."
            );

            return;
        }

        StatusEffectHandler statusEffectHandler =
            playerCombat.GetComponent<StatusEffectHandler>();

        if (statusEffectHandler == null)
        {
            return;
        }

        int toxicDamage =
            statusEffectHandler.ProcessToxic();

        if (toxicDamage <= 0)
        {
            return;
        }

        playerCombat.LoseHealth(toxicDamage);

        Debug.Log(
            $"[TurnManager] 플레이어 중독 피해 처리 : " +
            $"{toxicDamage}"
        );
    }

    /// <summary>
    /// 모든 활성화된 적의 중독을 발동합니다.
    /// </summary>
    private void ProcessEnemyToxic()
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

            if (statusEffectHandler == null)
            {
                continue;
            }

            int toxicDamage =
                statusEffectHandler.ProcessToxic();

            if (toxicDamage <= 0)
            {
                continue;
            }

            enemy.TakeDamage(toxicDamage);

            Debug.Log(
                $"[TurnManager] 적 중독 피해 처리 : " +
                $"{enemy.name} / {toxicDamage}"
            );
        }
    }

    /// <summary>
    /// 플레이어 턴 시작 시 현재 살아있는 모든 선원을 성장시킵니다.
    /// </summary>
    private void GrowAllCrews()
    {
        CrewManager crewManager =
            FindFirstObjectByType<CrewManager>();

        if (crewManager == null)
        {
            return;
        }

        if (crewManager.CrewCount <= 0)
        {
            return;
        }

        crewManager.GrowAllCrews();

        Debug.Log(
            $"[TurnManager] 플레이어 턴 시작 선원 성장 : " +
            $"{crewManager.CrewCount}명"
        );
    }

    /// <summary>
    /// 새 플레이어 턴에 부족한 손패 수만큼 드로우합니다.
    /// </summary>
    private void DrawCardsForNewTurn()
    {
        if (handManager == null)
        {
            Debug.LogError(
                "[TurnManager] HandManager가 연결되지 않았습니다."
            );

            return;
        }

        int currentHandCount =
            handManager.HandCards.Count;

        int drawCount =
            maxHandCount - currentHandCount;

        if (drawCount <= 0)
        {
            Debug.Log(
                "[TurnManager] 손패가 이미 최대 장수입니다."
            );

            return;
        }

        handManager.DrawCards(drawCount);

        Debug.Log(
            $"[TurnManager] 새 턴 드로우 : {drawCount}장"
        );
    }

    /// <summary>
    /// 현재 선택한 카드를 사용할 수 있는지 확인합니다.
    /// 턴 상태, 선원 비용, 선원 필요 효과, 카드 제한을 검사합니다.
    /// </summary>
    public bool CanUseCard(CardData cardData)
    {
        if (!isPlayerTurn)
        {
            Debug.LogWarning(
                "[TurnManager] 플레이어 턴이 아니므로 " +
                "카드를 사용할 수 없습니다."
            );

            return false;
        }

        if (cardData == null)
        {
            Debug.LogWarning(
                "[TurnManager] 확인할 카드 데이터가 없습니다."
            );

            return false;
        }

        /*
         * 희생 카드가 요구하는 선원 수를
         * 현재 보유한 선원으로 지불할 수 있는지 확인합니다.
         */
        if (!CanPayCrewSacrifice(cardData))
        {
            return false;
        }

        /*
         * 선원 공격이나 선원 회복처럼
         * 선원이 있어야 실행 가능한 카드인지 확인합니다.
         */
        if (!CanUseCrewRequiredEffect(cardData))
        {
            return false;
        }

        if (cardData.cardType == CardType.Skill)
        {
            PlayerCombat playerCombat =
                FindFirstObjectByType<PlayerCombat>();

            StatusEffectHandler statusEffectHandler = null;

            if (playerCombat != null)
            {
                statusEffectHandler =
                    playerCombat.GetComponent<StatusEffectHandler>();
            }

            /*
             * 미끄러짐 상태에서는
             * GainBlock 효과가 포함된 스킬 카드도 사용할 수 없습니다.
             */
            if (statusEffectHandler != null &&
                statusEffectHandler.HasStatusEffect(
                    StatusEffectType.NoBlock
                ) &&
                CardHasGainBlockEffect(cardData))
            {
                Debug.LogWarning(
                    "[TurnManager] 미끄러짐 적용 : " +
                    "방어도를 얻는 스킬 카드를 사용할 수 없습니다."
                );

                return false;
            }

            return true;
        }

        if (cardData.cardType == CardType.Attack ||
    cardData.cardType == CardType.Defense)
        {
            PlayerCombat playerCombat =
                FindFirstObjectByType<PlayerCombat>();

            StatusEffectHandler statusEffectHandler = null;

            if (playerCombat != null)
            {
                statusEffectHandler =
                    playerCombat.GetComponent<StatusEffectHandler>();
            }

            /*
             * 부러짐 상태에서는 공격 카드를 사용할 수 없습니다.
             */
            if (cardData.cardType == CardType.Attack &&
                statusEffectHandler != null &&
                statusEffectHandler.HasStatusEffect(
                    StatusEffectType.Broken
                ))
            {
                Debug.LogWarning(
                    "[TurnManager] 부러짐 적용 : " +
                    "공격 카드를 사용할 수 없습니다."
                );

                return false;
            }

            /*
             * 미끄러짐 상태에서는 방어 카드를 사용할 수 없습니다.
             *
             * 사용 전에 false를 반환하므로:
             * - 카드 효과 실행 안 됨
             * - 손패에서 제거 안 됨
             * - 버림 더미 이동 안 됨
             * - 공격/방어 카드 사용 횟수 증가 안 함
             */
            if (cardData.cardType == CardType.Defense &&
                statusEffectHandler != null &&
                statusEffectHandler.HasStatusEffect(
                    StatusEffectType.NoBlock
                ))
            {
                Debug.LogWarning(
                    "[TurnManager] 미끄러짐 적용 : " +
                    "방어 카드를 사용할 수 없습니다."
                );

                return false;
            }

            if (currentAttackDefenseCardUseCount >=
                maxAttackDefenseCardUseCount)
            {
                Debug.LogWarning(
                    "[TurnManager] 공격/방어 카드는 한 턴에 " +
                    $"최대 {maxAttackDefenseCardUseCount}장까지만 " +
                    "사용할 수 있습니다."
                );

                return false;
            }

            return true;
        }

        Debug.LogWarning(
            $"[TurnManager] 알 수 없는 카드 타입입니다 : " +
            $"{cardData.cardType}"
        );

        return false;
    }

    /// <summary>
    /// 카드에 방어도 획득 효과가 포함되어 있는지 확인합니다.
    /// </summary>
    private bool CardHasGainBlockEffect(
        CardData cardData)
    {
        if (cardData == null ||
            cardData.effects == null)
        {
            return false;
        }

        foreach (CardEffectData effect in cardData.effects)
        {
            if (effect == null)
            {
                continue;
            }

            if (effect.effectType ==
                    CardEffectType.GainBlock ||
                effect.effectType ==
                    CardEffectType.GainBlockOnHealthLossThisTurn)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 카드에 포함된 선원 희생 비용을 현재 선원 수로
    /// 지불할 수 있는지 확인합니다.
    /// </summary>
    private bool CanPayCrewSacrifice(CardData cardData)
    {
        if (cardData.effects == null ||
            cardData.effects.Count == 0)
        {
            return true;
        }

        CrewManager crewManager =
            FindFirstObjectByType<CrewManager>();

        int availableCrewCount = 0;

        if (crewManager != null)
        {
            availableCrewCount =
                crewManager.CrewCount;
        }

        foreach (CardEffectData effect in cardData.effects)
        {
            if (effect == null)
            {
                continue;
            }

            if (effect.effectType ==
                CardEffectType.Sacrifice)
            {
                int requiredCount =
                    Mathf.Max(0, effect.value);

                if (availableCrewCount < requiredCount)
                {
                    Debug.LogWarning(
                        $"[TurnManager] 희생할 선원이 부족합니다. " +
                        $"필요 {requiredCount}명 / " +
                        $"현재 {availableCrewCount}명"
                    );

                    return false;
                }

                availableCrewCount -= requiredCount;
            }

            if (effect.effectType ==
                CardEffectType.SacrificeAll)
            {
                if (availableCrewCount <= 0)
                {
                    Debug.LogWarning(
                        "[TurnManager] 전체 희생에 필요한 " +
                        "선원이 없습니다."
                    );

                    return false;
                }

                availableCrewCount = 0;
            }
        }

        return true;
    }

    /// <summary>
    /// 선원이 필요한 카드 효과가 있을 때
    /// 현재 선원이 한 명 이상 존재하는지 확인합니다.
    /// </summary>
    private bool CanUseCrewRequiredEffect(
        CardData cardData)
    {
        if (cardData.effects == null ||
            cardData.effects.Count == 0)
        {
            return true;
        }

        bool requiresCrew = false;

        foreach (CardEffectData effect in cardData.effects)
        {
            if (effect == null)
            {
                continue;
            }

            if (effect.effectType ==
                    CardEffectType.CrewDealDamage ||
                effect.effectType ==
                    CardEffectType.AllCrewsDealDamageRandomEnemy ||
                effect.effectType ==
                    CardEffectType.AllCrewsDealDamageAllEnemies ||
                effect.effectType ==
                    CardEffectType.HealAllCrews)
            {
                requiresCrew = true;
                break;
            }
        }

        if (!requiresCrew)
        {
            return true;
        }

        CrewManager crewManager =
            FindFirstObjectByType<CrewManager>();

        if (crewManager == null ||
            crewManager.CrewCount <= 0)
        {
            Debug.LogWarning(
                "[TurnManager] 선원이 필요한 카드이지만 " +
                "현재 소환된 선원이 없습니다."
            );

            return false;
        }

        return true;
    }

    /// <summary>
    /// 사용한 공격/방어 카드 횟수를 기록합니다.
    /// </summary>
    public void RecordCardUse(CardData cardData)
    {
        if (cardData == null)
        {
            Debug.LogWarning(
                "[TurnManager] 사용 기록할 카드 데이터가 없습니다."
            );

            return;
        }

        if (cardData.cardType == CardType.Attack ||
            cardData.cardType == CardType.Defense)
        {
            currentAttackDefenseCardUseCount++;

            UpdateAttackDefenseUseCountUI();

            Debug.Log(
                $"[TurnManager] 공격/방어 카드 사용 횟수 : " +
                $"{currentAttackDefenseCardUseCount}/" +
                $"{maxAttackDefenseCardUseCount}"
            );
        }
    }

    /// <summary>
    /// 현재 공격/방어 카드 사용 횟수 UI를 갱신합니다.
    /// </summary>
    private void UpdateAttackDefenseUseCountUI()
    {
        if (attackDefenseUseCountText == null)
        {
            Debug.LogWarning(
                "[TurnManager] 공격/방어 카드 사용 횟수 UI가 " +
                "연결되지 않았습니다."
            );

            return;
        }

        attackDefenseUseCountText.text =
            $"{currentAttackDefenseCardUseCount} / " +
            $"{maxAttackDefenseCardUseCount}";
    }

    /// <summary>
    /// 플레이어의 일반 상태효과 지속 턴을 감소시킵니다.
    /// </summary>
    private void DecreasePlayerStatusDuration()
    {
        PlayerCombat playerCombat =
            FindFirstObjectByType<PlayerCombat>();

        if (playerCombat == null)
        {
            Debug.LogWarning(
                "[TurnManager] PlayerCombat을 찾지 못해 " +
                "플레이어 상태효과 턴을 감소시키지 못했습니다."
            );

            return;
        }

        StatusEffectHandler statusEffectHandler =
            playerCombat.GetComponent<StatusEffectHandler>();

        if (statusEffectHandler == null)
        {
            return;
        }

        statusEffectHandler.DecreaseTurnDuration();

        Debug.Log(
            "[TurnManager] 플레이어 상태효과 지속 턴 감소"
        );
    }

    /// <summary>
    /// 모든 활성화된 적의 일반 상태효과 지속 턴을 감소시킵니다.
    /// </summary>
    private void DecreaseEnemyStatusDuration()
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

            if (statusEffectHandler == null)
            {
                continue;
            }

            statusEffectHandler.DecreaseTurnDuration();
        }

        Debug.Log(
            "[TurnManager] 적 상태효과 지속 턴 감소"
        );
    }
}