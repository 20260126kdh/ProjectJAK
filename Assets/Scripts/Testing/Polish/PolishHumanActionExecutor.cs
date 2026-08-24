using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 인간형 자동 플레이 판단의 실제 실행 결과입니다.
/// </summary>
public enum PolishActionExecutionResult
{
    Success,
    InvalidDecision,
    MissingBattleComponent,
    InvalidHandIndex,
    InvalidTarget,
    CardUseRejected
}

/// <summary>
/// 인간형 자동 플레이 판단을 기존 전투 입력 경로에 전달합니다.
/// 카드 규칙을 직접 실행하지 않고 실제 플레이어 입력과 같은 공개 API를 사용합니다.
/// </summary>
public class PolishHumanActionExecutor : MonoBehaviour
{
    /// <summary>
    /// 판단 하나를 현재 전투에 적용합니다.
    /// </summary>
    /// <param name="decision">공개 전투 정보만으로 생성된 판단</param>
    /// <returns>행동 전달 결과</returns>
    public PolishActionExecutionResult Execute(PolishHumanDecision decision)
    {
        if (decision == null)
        {
            return PolishActionExecutionResult.InvalidDecision;
        }

        if (decision.decisionType == PolishDecisionType.EndTurn)
        {
            HandManager preserveHandManager =
                FindFirstObjectByType<HandManager>();
            if (preserveHandManager == null)
            {
                return PolishActionExecutionResult.MissingBattleComponent;
            }

            if (preserveHandManager.IsCardFlowBusy)
            {
                return PolishActionExecutionResult.CardUseRejected;
            }

            if (decision.preserveHandIndex >=
                preserveHandManager.HandCardUIs.Count)
            {
                return PolishActionExecutionResult.InvalidHandIndex;
            }

            preserveHandManager.StartPreserveMode();
            if (decision.preserveHandIndex >= 0)
            {
                preserveHandManager.SelectCardByIndex(
                    decision.preserveHandIndex);
            }

            preserveHandManager.ConfirmPreserveCard();
            return PolishActionExecutionResult.Success;
        }

        HandManager handManager = FindFirstObjectByType<HandManager>();
        BattleManager battleManager = FindFirstObjectByType<BattleManager>();
        if (handManager == null || battleManager == null)
        {
            return PolishActionExecutionResult.MissingBattleComponent;
        }

        if (decision.handIndex < 0 ||
            decision.handIndex >= handManager.HandCardUIs.Count)
        {
            return PolishActionExecutionResult.InvalidHandIndex;
        }

        CardData selectedCard = handManager.HandCards[decision.handIndex];
        handManager.SelectCardByIndex(decision.handIndex);

        switch (decision.target)
        {
            case PolishDecisionTarget.Enemy:
                Enemy enemy = FindAliveEnemy(decision.enemyIndex);
                if (enemy == null)
                {
                    battleManager.ClearSelectedCard();
                    return PolishActionExecutionResult.InvalidTarget;
                }

                battleManager.UseSelectedCardOnEnemy(enemy);
                return GetCardUseResult(
                    handManager,
                    battleManager,
                    selectedCard);

            case PolishDecisionTarget.Player:
            case PolishDecisionTarget.None:
                PlayerCombat player = FindFirstObjectByType<PlayerCombat>();
                if (player == null)
                {
                    battleManager.ClearSelectedCard();
                    return PolishActionExecutionResult.InvalidTarget;
                }

                battleManager.UseSelectedCardOnPlayer(player);
                return GetCardUseResult(
                    handManager,
                    battleManager,
                    selectedCard);

            case PolishDecisionTarget.Crew:
                Crew crew = FindAliveCrew(decision.crewOrder);
                if (crew == null)
                {
                    battleManager.ClearSelectedCard();
                    return PolishActionExecutionResult.InvalidTarget;
                }

                battleManager.UseSelectedCardOnCrew(crew);
                return GetCardUseResult(
                    handManager,
                    battleManager,
                    selectedCard);

            default:
                battleManager.ClearSelectedCard();
                return PolishActionExecutionResult.InvalidTarget;
        }
    }

    private static PolishActionExecutionResult GetCardUseResult(
        HandManager handManager,
        BattleManager battleManager,
        CardData selectedCard)
    {
        if (selectedCard != null && handManager.HandCards.Contains(selectedCard))
        {
            battleManager.ClearSelectedCard();
            return PolishActionExecutionResult.CardUseRejected;
        }

        return PolishActionExecutionResult.Success;
    }

    private static Enemy FindAliveEnemy(int enemyIndex)
    {
        if (enemyIndex < 0)
        {
            return null;
        }

        Enemy[] enemies = FindObjectsByType<Enemy>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None);
        int aliveIndex = 0;

        foreach (Enemy enemy in enemies)
        {
            if (enemy == null || enemy.CurrentHP <= 0)
            {
                continue;
            }

            if (aliveIndex == enemyIndex)
            {
                return enemy;
            }

            aliveIndex++;
        }

        return null;
    }

    private static Crew FindAliveCrew(int crewOrder)
    {
        if (crewOrder <= 0)
        {
            return null;
        }

        CrewManager crewManager = FindFirstObjectByType<CrewManager>();
        if (crewManager == null)
        {
            return null;
        }

        IReadOnlyList<Crew> crews = crewManager.Crews;
        int index = crewOrder - 1;
        if (index < 0 || index >= crews.Count)
        {
            return null;
        }

        Crew crew = crews[index];
        return crew != null && crew.IsAlive ? crew : null;
    }
}
