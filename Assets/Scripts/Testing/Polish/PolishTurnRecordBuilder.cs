using System.Collections.Generic;

/// <summary>
/// 플레이어에게 공개된 전투 스냅샷과 판단 결과를 턴 기록으로 변환합니다.
/// </summary>
public static class PolishTurnRecordBuilder
{
    /// <summary>
    /// 플레이어 턴 시작 시점의 공개 정보로 새 턴 기록을 만듭니다.
    /// </summary>
    /// <param name="snapshot">플레이어 화면에 공개된 전투 정보</param>
    /// <param name="turnNumber">현재 전투의 플레이어 턴 번호</param>
    /// <returns>행동을 누적할 턴 기록</returns>
    public static PolishTurnRecord Create(
        PolishVisibleBattleSnapshot snapshot,
        int turnNumber)
    {
        PolishTurnRecord record = new PolishTurnRecord
        {
            turn = turnNumber,
            hp = snapshot != null ? snapshot.playerHp : 0,
            block = snapshot != null ? snapshot.playerBlock : 0,
            crewCount = snapshot != null && snapshot.crews != null
                ? snapshot.crews.Count
                : 0
        };

        if (snapshot == null)
        {
            record.observation = "공개 전투 스냅샷 없음";
            return record;
        }

        CaptureHand(record, snapshot.hand);
        CaptureEnemies(record, snapshot.enemies);
        CaptureStatuses(record, snapshot.playerStatuses, "플레이어");
        record.observation =
            $"HP {snapshot.playerHp}/{snapshot.playerMaxHp}, " +
            $"Block {snapshot.playerBlock}, " +
            $"Hand {record.hand.Count}, Enemy {record.enemyStates.Count}";
        return record;
    }

    /// <summary>
    /// 같은 플레이어 턴에 수행한 판단과 실제 전달 결과를 기록에 추가합니다.
    /// </summary>
    /// <param name="record">현재 플레이어 턴 기록</param>
    /// <param name="decision">인간형 판단 엔진이 선택한 행동</param>
    /// <param name="executionResult">기존 전투 입력 경로의 전달 결과</param>
    /// <param name="snapshot">행동 선택 직전의 공개 전투 정보</param>
    public static void AppendAction(
        PolishTurnRecord record,
        PolishHumanDecision decision,
        PolishActionExecutionResult executionResult,
        PolishVisibleBattleSnapshot snapshot)
    {
        if (record == null || decision == null)
        {
            return;
        }

        record.dangerLevel = decision.dangerLevel.ToString();
        record.expectedIncomingDamage = decision.expectedIncomingDamage;
        record.actions.Add(FormatAction(decision, snapshot));
        record.executionResults.Add(executionResult.ToString());
        record.decisionReasons.Add(decision.reason ?? string.Empty);
    }

    private static string FormatAction(
        PolishHumanDecision decision,
        PolishVisibleBattleSnapshot snapshot)
    {
        if (decision.decisionType == PolishDecisionType.EndTurn)
        {
            return "EndTurn";
        }

        string cardName = GetCardName(snapshot, decision.handIndex);
        string target = decision.target.ToString();
        if (decision.target == PolishDecisionTarget.Enemy)
        {
            target += $"[{decision.enemyIndex}]";
        }
        else if (decision.target == PolishDecisionTarget.Crew)
        {
            target += $"[{decision.crewOrder}]";
        }

        return $"UseCard:{cardName} -> {target}";
    }

    private static string GetCardName(
        PolishVisibleBattleSnapshot snapshot,
        int handIndex)
    {
        if (snapshot == null || snapshot.hand == null ||
            handIndex < 0 || handIndex >= snapshot.hand.Count)
        {
            return $"Unknown[{handIndex}]";
        }

        PolishVisibleCardSnapshot card = snapshot.hand[handIndex];
        if (card == null)
        {
            return $"Unknown[{handIndex}]";
        }

        return string.IsNullOrWhiteSpace(card.displayName)
            ? card.cardId ?? $"Unknown[{handIndex}]"
            : card.displayName;
    }

    private static void CaptureHand(
        PolishTurnRecord record,
        List<PolishVisibleCardSnapshot> hand)
    {
        if (hand == null)
        {
            return;
        }

        foreach (PolishVisibleCardSnapshot card in hand)
        {
            if (card == null)
            {
                continue;
            }

            record.hand.Add(
                $"{card.cardId}|{card.displayName}|{card.cardType}|" +
                $"Upgraded:{card.isUpgraded}|Usable:{card.isUsable}");
        }
    }

    private static void CaptureEnemies(
        PolishTurnRecord record,
        List<PolishVisibleEnemySnapshot> enemies)
    {
        if (enemies == null)
        {
            return;
        }

        for (int index = 0; index < enemies.Count; index++)
        {
            PolishVisibleEnemySnapshot enemy = enemies[index];
            if (enemy == null)
            {
                continue;
            }

            record.harpoonStack += enemy.harpoonStack;
            record.enemyStates.Add(
                $"Enemy[{index}] {enemy.objectName} " +
                $"HP:{enemy.hp}/{enemy.maxHp} Block:{enemy.block} " +
                $"Harpoon:{enemy.harpoonStack}");

            if (enemy.intents != null)
            {
                foreach (string intent in enemy.intents)
                {
                    record.enemyIntents.Add($"Enemy[{index}] {intent}");
                }
            }

            CaptureStatuses(record, enemy.statuses, $"Enemy[{index}]");
        }
    }

    private static void CaptureStatuses(
        PolishTurnRecord record,
        List<PolishVisibleStatusSnapshot> statuses,
        string owner)
    {
        if (statuses == null)
        {
            return;
        }

        foreach (PolishVisibleStatusSnapshot status in statuses)
        {
            if (status == null)
            {
                continue;
            }

            string text =
                $"{owner} {status.type} Value:{status.value} " +
                $"Turn:{status.remainingTurn} Permanent:{status.isPermanent}";
            if (status.isDebuff)
            {
                record.debuffs.Add(text);
            }
            else
            {
                record.buffs.Add(text);
            }
        }
    }
}
