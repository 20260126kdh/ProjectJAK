using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

/// <summary>
/// 공개 전투 스냅샷만 사용해 일반적인 숙련 플레이어 수준의 한 행동을 선택합니다.
/// 완전 탐색이나 미래 정보 예측 없이 생존, 처치와 현재 카드 설명을 휴리스틱으로 평가합니다.
/// </summary>
public class PolishHumanDecisionEngine
{
    private static readonly Regex NumberRegex = new Regex(@"\d+", RegexOptions.Compiled);
    private static readonly Regex MultiHitRegex = new Regex(
        @"(?<damage>\d+)\s*x\s*(?<count>\d+)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly System.Random decisionRandom;

    /// <summary>
    /// 게임 RNG와 분리된 결정용 Seed로 판단 엔진을 생성합니다.
    /// </summary>
    /// <param name="seed">합리적인 동점 선택을 재현할 결정용 Seed</param>
    public PolishHumanDecisionEngine(int seed)
    {
        decisionRandom = new System.Random(seed ^ 0x5A17C3);
    }

    /// <summary>
    /// 현재 공개 정보에서 한 장의 카드 사용 또는 턴 종료를 선택합니다.
    /// </summary>
    /// <param name="snapshot">플레이어에게 공개된 현재 전투 상태</param>
    /// <returns>실행 전 검증이 필요한 단일 행동 계획</returns>
    public PolishHumanDecision Decide(PolishVisibleBattleSnapshot snapshot)
    {
        if (snapshot == null || !snapshot.isPlayerTurn)
        {
            return CreateEndTurnDecision(
                PolishDangerLevel.Safe,
                0,
                "플레이어 행동 가능 상태가 아닙니다.");
        }

        int incomingDamage = EstimateIncomingDamage(snapshot);
        PolishDangerLevel dangerLevel = EvaluateDanger(snapshot, incomingDamage);
        List<CardCandidate> candidates = BuildCandidates(snapshot, dangerLevel, incomingDamage);

        if (candidates.Count == 0)
        {
            return CreateEndTurnDecision(
                dangerLevel,
                incomingDamage,
                "현재 사용할 수 있는 카드가 없습니다.");
        }

        candidates.Sort((left, right) => right.score.CompareTo(left.score));
        int bestScore = candidates[0].score;
        List<CardCandidate> nearBest = candidates.FindAll(
            candidate => bestScore - candidate.score <= 2);
        CardCandidate selected = nearBest[decisionRandom.Next(nearBest.Count)];

        return new PolishHumanDecision
        {
            decisionType = PolishDecisionType.UseCard,
            target = selected.target,
            handIndex = selected.handIndex,
            enemyIndex = selected.enemyIndex,
            crewOrder = selected.crewOrder,
            dangerLevel = dangerLevel,
            expectedIncomingDamage = incomingDamage,
            reason = selected.reason
        };
    }

    /// <summary>
    /// Intent UI 문자열에서 이번 적 턴의 총 예상 피해를 계산합니다.
    /// </summary>
    /// <param name="snapshot">현재 공개 전투 상태</param>
    /// <returns>모든 살아 있는 적의 화면 표시 피해 합계</returns>
    public static int EstimateIncomingDamage(PolishVisibleBattleSnapshot snapshot)
    {
        if (snapshot == null || snapshot.enemies == null)
        {
            return 0;
        }

        int totalDamage = 0;
        foreach (PolishVisibleEnemySnapshot enemy in snapshot.enemies)
        {
            if (enemy == null || enemy.intents == null)
            {
                continue;
            }

            foreach (string intent in enemy.intents)
            {
                if (string.IsNullOrWhiteSpace(intent) ||
                    !intent.StartsWith("Damage:", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string expression = intent.Substring("Damage:".Length);
                foreach (string part in expression.Split('+'))
                {
                    Match multiHit = MultiHitRegex.Match(part);
                    if (multiHit.Success)
                    {
                        totalDamage += int.Parse(multiHit.Groups["damage"].Value) *
                                       int.Parse(multiHit.Groups["count"].Value);
                        continue;
                    }

                    Match number = NumberRegex.Match(part);
                    if (number.Success)
                    {
                        totalDamage += int.Parse(number.Value);
                    }
                }
            }
        }

        return totalDamage;
    }

    private static PolishDangerLevel EvaluateDanger(
        PolishVisibleBattleSnapshot snapshot,
        int incomingDamage)
    {
        if (incomingDamage >= snapshot.playerHp + snapshot.playerBlock)
        {
            return PolishDangerLevel.Lethal;
        }

        if (incomingDamage > snapshot.playerBlock ||
            snapshot.playerHp * 100 <= snapshot.playerMaxHp * 35)
        {
            return PolishDangerLevel.High;
        }

        if (incomingDamage > 0)
        {
            return PolishDangerLevel.Caution;
        }

        return PolishDangerLevel.Safe;
    }

    private List<CardCandidate> BuildCandidates(
        PolishVisibleBattleSnapshot snapshot,
        PolishDangerLevel dangerLevel,
        int incomingDamage)
    {
        List<CardCandidate> candidates = new List<CardCandidate>();
        for (int index = 0; index < snapshot.hand.Count; index++)
        {
            PolishVisibleCardSnapshot card = snapshot.hand[index];
            if (card == null || !card.isUsable)
            {
                continue;
            }

            CardCandidate candidate = ScoreCard(
                snapshot,
                card,
                index,
                dangerLevel,
                incomingDamage);
            if (candidate != null)
            {
                candidates.Add(candidate);
            }
        }

        return candidates;
    }

    private CardCandidate ScoreCard(
        PolishVisibleBattleSnapshot snapshot,
        PolishVisibleCardSnapshot card,
        int handIndex,
        PolishDangerLevel dangerLevel,
        int incomingDamage)
    {
        string description = card.description ?? string.Empty;
        string cardType = card.cardType ?? string.Empty;
        int displayedValue = ExtractFirstNumber(description);
        int score = decisionRandom.Next(-1, 2);
        PolishDecisionTarget target = InferTarget(cardType, description);
        int enemyIndex = target == PolishDecisionTarget.Enemy
            ? FindLowestHpEnemyIndex(snapshot)
            : -1;

        if (cardType.Equals("Defense", StringComparison.OrdinalIgnoreCase) ||
            description.Contains("방어도"))
        {
            score += dangerLevel >= PolishDangerLevel.High ? 28 : 8;
            score += Math.Min(displayedValue, Math.Max(0, incomingDamage - snapshot.playerBlock));
        }

        if (description.Contains("회복"))
        {
            int missingHp = Math.Max(0, snapshot.playerMaxHp - snapshot.playerHp);
            score += missingHp > 0 ? 12 + Math.Min(displayedValue, missingHp) : -6;
            target = PolishDecisionTarget.Player;
        }

        if (cardType.Equals("Attack", StringComparison.OrdinalIgnoreCase) ||
            description.Contains("피해"))
        {
            score += 10 + displayedValue;
            if (dangerLevel == PolishDangerLevel.Lethal)
            {
                score -= 12;
            }

            if (enemyIndex >= 0 &&
                displayedValue > 0 &&
                snapshot.enemies[enemyIndex].hp <= displayedValue)
            {
                score += 30;
            }
        }

        if (cardType.Equals("Skill", StringComparison.OrdinalIgnoreCase))
        {
            score += 6;
        }

        if (description.Contains("사용할 수 없"))
        {
            return null;
        }

        return new CardCandidate
        {
            handIndex = handIndex,
            enemyIndex = enemyIndex,
            target = target,
            score = score,
            reason = $"{dangerLevel} 위험에서 '{card.displayName}'을(를) " +
                     $"화면 설명 기준 점수 {score}로 선택했습니다."
        };
    }

    private static PolishDecisionTarget InferTarget(string cardType, string description)
    {
        if (description.Contains("선원 한 명을") && description.Contains("희생"))
        {
            return PolishDecisionTarget.Crew;
        }

        if (cardType.Equals("Attack", StringComparison.OrdinalIgnoreCase) ||
            description.Contains("적에게") ||
            description.Contains("모든 적") ||
            description.Contains("작살"))
        {
            return PolishDecisionTarget.Enemy;
        }

        return PolishDecisionTarget.Player;
    }

    private static int FindLowestHpEnemyIndex(PolishVisibleBattleSnapshot snapshot)
    {
        int selectedIndex = -1;
        int lowestHp = int.MaxValue;
        for (int index = 0; index < snapshot.enemies.Count; index++)
        {
            PolishVisibleEnemySnapshot enemy = snapshot.enemies[index];
            if (enemy != null && enemy.hp > 0 && enemy.hp < lowestHp)
            {
                selectedIndex = index;
                lowestHp = enemy.hp;
            }
        }

        return selectedIndex;
    }

    private static int ExtractFirstNumber(string text)
    {
        Match match = NumberRegex.Match(text ?? string.Empty);
        return match.Success ? int.Parse(match.Value) : 0;
    }

    private static PolishHumanDecision CreateEndTurnDecision(
        PolishDangerLevel dangerLevel,
        int incomingDamage,
        string reason)
    {
        return new PolishHumanDecision
        {
            decisionType = PolishDecisionType.EndTurn,
            target = PolishDecisionTarget.None,
            dangerLevel = dangerLevel,
            expectedIncomingDamage = incomingDamage,
            reason = reason
        };
    }

    private sealed class CardCandidate
    {
        public int handIndex;
        public int enemyIndex;
        public int crewOrder = -1;
        public PolishDecisionTarget target;
        public int score;
        public string reason;
    }
}
