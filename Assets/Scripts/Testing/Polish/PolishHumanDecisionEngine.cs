using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

/// <summary>
/// 공개 전투 스냅샷만 사용해 일반적인 숙련 플레이어 수준의 한 행동을 선택합니다.
/// 완전 탐색이나 미래 정보 예측 없이 생존, 처치와 현재 카드 설명을 휴리스틱으로 평가합니다.
/// </summary>
public class PolishHumanDecisionEngine
{
    private const int ComboSearchDepth = 4;
    private const int ComboBeamWidth = 24;
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
                -1,
                "플레이어 행동 가능 상태가 아닙니다.");
        }

        int incomingDamage = EstimateIncomingDamage(snapshot);
        PolishDangerLevel dangerLevel = EvaluateDanger(snapshot, incomingDamage);
        List<CardCandidate> candidates = BuildCandidates(snapshot, dangerLevel, incomingDamage);

        if (candidates.Count == 0)
        {
            int preserveHandIndex = ChoosePreserveHandIndex(
                snapshot,
                incomingDamage,
                decisionRandom);
            return CreateEndTurnDecision(
                dangerLevel,
                incomingDamage,
                preserveHandIndex,
                "현재 사용할 수 있는 카드가 없습니다.");
        }

        CardCandidate comboOpening = FindBestComboOpening(snapshot, candidates);
        if (comboOpening != null)
        {
            return CreateUseCardDecision(
                comboOpening,
                dangerLevel,
                incomingDamage,
                $"'{comboOpening.comboReason}' 조합의 첫 카드입니다.");
        }

        candidates.Sort((left, right) => right.score.CompareTo(left.score));
        int bestScore = candidates[0].score;
        List<CardCandidate> nearBest = candidates.FindAll(
            candidate => bestScore - candidate.score <= 2);
        CardCandidate selected = nearBest[decisionRandom.Next(nearBest.Count)];

        return CreateUseCardDecision(selected, dangerLevel, incomingDamage, selected.reason);
    }

    private static PolishHumanDecision CreateUseCardDecision(
        CardCandidate selected,
        PolishDangerLevel dangerLevel,
        int incomingDamage,
        string reason)
    {
        return new PolishHumanDecision
        {
            decisionType = PolishDecisionType.UseCard,
            target = selected.target,
            handIndex = selected.handIndex,
            enemyIndex = selected.enemyIndex,
            crewOrder = selected.crewOrder,
            dangerLevel = dangerLevel,
            expectedIncomingDamage = incomingDamage,
            reason = reason
        };
    }

    /// <summary>
    /// 현재 손패만 사용해 최대 네 장의 순서를 미리 비교합니다.
    /// 미래 드로우와 RNG는 예측하지 않으며 실제 실행 뒤에는 다시 탐색합니다.
    /// </summary>
    private CardCandidate FindBestComboOpening(
        PolishVisibleBattleSnapshot snapshot,
        List<CardCandidate> immediateCandidates)
    {
        if (immediateCandidates.Count < 2 ||
            !snapshot.hand.Exists(card => card != null && card.effects.Count > 0))
        {
            return null;
        }

        ComboState initial = ComboState.Create(snapshot);
        List<ComboPath> beam = new List<ComboPath> { new ComboPath(initial) };
        ComboPath best = null;

        for (int depth = 0; depth < ComboSearchDepth; depth++)
        {
            List<ComboPath> expanded = new List<ComboPath>();
            foreach (ComboPath path in beam)
            {
                foreach (CardCandidate candidate in immediateCandidates)
                {
                    if (path.usedHandIndices.Contains(candidate.handIndex) ||
                        !path.state.CanUse(snapshot.hand[candidate.handIndex]))
                    {
                        continue;
                    }

                    ComboPath next = path.Clone();
                    next.Apply(snapshot.hand[candidate.handIndex], candidate);
                    if (next.state.playerHp <= 0)
                    {
                        continue;
                    }
                    expanded.Add(next);
                    if (best == null || next.score > best.score)
                    {
                        best = next;
                    }
                }
            }

            expanded.Sort((left, right) => right.score.CompareTo(left.score));
            if (expanded.Count > ComboBeamWidth)
            {
                expanded.RemoveRange(ComboBeamWidth, expanded.Count - ComboBeamWidth);
            }

            beam = expanded;
            if (beam.Count == 0)
            {
                break;
            }
        }

        if (best == null || best.actions.Count < 2)
        {
            return null;
        }

        CardCandidate opening = best.actions[0];
        opening.comboReason = string.Join(" → ", best.cardNames);
        return opening;
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
        int healthCost = GetEffectValue(card, CardEffectType.LoseHealth);
        bool hasStructuredEffects = card.effects != null && card.effects.Count > 0;
        PolishDecisionTarget target = card.hasRequiredTarget
            ? card.requiredTarget
            : InferTarget(cardType, description);
        if (card.hasRequiredTarget && target == PolishDecisionTarget.None)
        {
            return null;
        }

        if (cardType.Equals("Defense", StringComparison.OrdinalIgnoreCase) ||
            description.Contains("방어도"))
        {
            if (incomingDamage <= snapshot.playerBlock &&
                hasStructuredEffects && HasOnlyImmediateBlockEffects(card))
            {
                return null;
            }

            score += dangerLevel >= PolishDangerLevel.High ? 28 : -4;
            score += Math.Min(displayedValue, Math.Max(0, incomingDamage - snapshot.playerBlock));
        }

        if (healthCost > 0)
        {
            if (healthCost >= snapshot.playerHp)
            {
                return null;
            }

            score -= healthCost * 3;
            int hpAfterCost = snapshot.playerHp - healthCost;
            if (incomingDamage > hpAfterCost + snapshot.playerBlock)
            {
                score -= 40;
            }
        }

        if (description.Contains("회복"))
        {
            int missingHp = Math.Max(0, snapshot.playerMaxHp - snapshot.playerHp);
            score += missingHp > 0 ? 12 + Math.Min(displayedValue, missingHp) : -6;
            if (!card.hasRequiredTarget)
            {
                target = PolishDecisionTarget.Player;
            }
        }

        if (cardType.Equals("Attack", StringComparison.OrdinalIgnoreCase) ||
            description.Contains("피해"))
        {
            score += 10 + displayedValue;
            if (dangerLevel == PolishDangerLevel.Lethal)
            {
                score -= 12;
            }

            int lethalEnemyIndex = target == PolishDecisionTarget.Enemy
                ? FindLowestHpEnemyIndex(snapshot)
                : -1;
            if (lethalEnemyIndex >= 0 &&
                displayedValue > 0 &&
                snapshot.enemies[lethalEnemyIndex].hp <= displayedValue)
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

        int enemyIndex = target == PolishDecisionTarget.Enemy
            ? FindLowestHpEnemyIndex(snapshot)
            : -1;
        int crewOrder = target == PolishDecisionTarget.Crew
            ? FindLowestHpCrewOrder(snapshot)
            : -1;
        if ((target == PolishDecisionTarget.Enemy && enemyIndex < 0) ||
            (target == PolishDecisionTarget.Crew && crewOrder < 0))
        {
            return null;
        }

        return new CardCandidate
        {
            handIndex = handIndex,
            enemyIndex = enemyIndex,
            crewOrder = crewOrder,
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

    private static int FindLowestHpCrewOrder(PolishVisibleBattleSnapshot snapshot)
    {
        int selectedOrder = -1;
        int lowestHp = int.MaxValue;
        for (int index = 0; index < snapshot.crews.Count; index++)
        {
            PolishVisibleCrewSnapshot crew = snapshot.crews[index];
            if (crew != null && crew.hp > 0 && crew.hp < lowestHp)
            {
                selectedOrder = crew.order;
                lowestHp = crew.hp;
            }
        }

        return selectedOrder;
    }

    private static int ExtractFirstNumber(string text)
    {
        Match match = NumberRegex.Match(text ?? string.Empty);
        return match.Success ? int.Parse(match.Value) : 0;
    }

    private static int GetEffectValue(
        PolishVisibleCardSnapshot card,
        CardEffectType effectType)
    {
        int total = 0;
        if (card?.effects == null)
        {
            return total;
        }

        foreach (PolishVisibleCardEffectSnapshot effect in card.effects)
        {
            if (effect != null && effect.effectType == effectType)
            {
                total += effect.value * Math.Max(1, effect.repeatCount);
            }
        }
        return total;
    }

    private static bool HasOnlyImmediateBlockEffects(
        PolishVisibleCardSnapshot card)
    {
        if (card?.effects == null || card.effects.Count == 0)
        {
            return false;
        }

        foreach (PolishVisibleCardEffectSnapshot effect in card.effects)
        {
            if (effect != null && effect.effectType != CardEffectType.GainBlock)
            {
                return false;
            }
        }
        return true;
    }

    private static PolishHumanDecision CreateEndTurnDecision(
        PolishDangerLevel dangerLevel,
        int incomingDamage,
        int preserveHandIndex,
        string reason)
    {
        return new PolishHumanDecision
        {
            decisionType = PolishDecisionType.EndTurn,
            target = PolishDecisionTarget.None,
            preserveHandIndex = preserveHandIndex,
            preserveReason = preserveHandIndex >= 0
                ? "절대 보존 점수가 기준 이상인 카드 한 장을 선택했습니다."
                : "보존 기준을 충족한 카드가 없어 빈 보존을 선택했습니다.",
            dangerLevel = dangerLevel,
            expectedIncomingDamage = incomingDamage,
            reason = reason
        };
    }

    /// <summary>
    /// 턴 종료 시 다음 턴에 남길 카드 한 장을 공개 손패 정보로 선택합니다.
    /// </summary>
    /// <param name="snapshot">현재 공개 전투 상태</param>
    /// <param name="incomingDamage">현재 표시된 총 예상 피해</param>
    /// <returns>보존할 손패 인덱스이며 손패가 없으면 -1</returns>
    public static int ChoosePreserveHandIndex(
        PolishVisibleBattleSnapshot snapshot,
        int incomingDamage,
        System.Random tieBreaker = null)
    {
        const int preserveThreshold = 7;
        if (snapshot?.hand == null || snapshot.hand.Count == 0)
        {
            return -1;
        }

        int bestIndex = -1;
        int bestScore = int.MinValue;
        List<int> tiedBestIndices = new List<int>();
        for (int index = 0; index < snapshot.hand.Count; index++)
        {
            PolishVisibleCardSnapshot card = snapshot.hand[index];
            if (card == null)
            {
                continue;
            }

            int score = card.isUpgraded ? 7 : 0;
            if (string.Equals(
                    card.rarity,
                    CardRarity.Epic.ToString(),
                    StringComparison.OrdinalIgnoreCase))
            {
                score += 7;
            }
            else if (string.Equals(
                         card.rarity,
                         CardRarity.Rare.ToString(),
                         StringComparison.OrdinalIgnoreCase))
            {
                score += 4;
            }

            if (string.Equals(
                    card.cardType,
                    "Skill",
                    StringComparison.OrdinalIgnoreCase))
            {
                score += 5;
            }
            else if (string.Equals(
                         card.cardType,
                         "Defense",
                         StringComparison.OrdinalIgnoreCase))
            {
                score += incomingDamage > 0 ? 4 : 1;
            }
            else if (string.Equals(
                         card.cardType,
                         "Attack",
                         StringComparison.OrdinalIgnoreCase))
            {
                score += 1;
            }

            if (score > bestScore)
            {
                bestIndex = index;
                bestScore = score;
                tiedBestIndices.Clear();
                tiedBestIndices.Add(index);
            }
            else if (score == bestScore)
            {
                tiedBestIndices.Add(index);
            }
        }

        if (bestScore < preserveThreshold)
        {
            return -1;
        }

        if (tieBreaker != null && tiedBestIndices.Count > 1)
        {
            return tiedBestIndices[tieBreaker.Next(tiedBestIndices.Count)];
        }

        return bestIndex;
    }

    private sealed class CardCandidate
    {
        public int handIndex;
        public int enemyIndex;
        public int crewOrder = -1;
        public PolishDecisionTarget target;
        public int score;
        public string reason;
        public string comboReason;
    }

    private sealed class ComboPath
    {
        public ComboState state;
        public int score;
        public readonly List<int> usedHandIndices = new List<int>();
        public readonly List<CardCandidate> actions = new List<CardCandidate>();
        public readonly List<string> cardNames = new List<string>();

        public ComboPath(ComboState state)
        {
            this.state = state;
        }

        public ComboPath Clone()
        {
            ComboPath clone = new ComboPath(state.Clone()) { score = score };
            clone.usedHandIndices.AddRange(usedHandIndices);
            clone.actions.AddRange(actions);
            clone.cardNames.AddRange(cardNames);
            return clone;
        }

        public void Apply(PolishVisibleCardSnapshot card, CardCandidate candidate)
        {
            int beforeHp = state.TotalEnemyHp;
            int beforeBlock = state.playerBlock;
            int beforePlayerHp = state.playerHp;
            state.Apply(card, candidate.enemyIndex);
            usedHandIndices.Add(candidate.handIndex);
            actions.Add(candidate);
            cardNames.Add(card.displayName);

            int damage = Math.Max(0, beforeHp - state.TotalEnemyHp);
            int defense = Math.Max(0, state.playerBlock - beforeBlock);
            int healing = Math.Max(0, state.playerHp - beforePlayerHp);
            int healthCost = Math.Max(0, beforePlayerHp - state.playerHp);
            score += damage * 3 + defense * 2 + healing * 2;
            score -= healthCost * 4;
            score += state.defeatedEnemies * 45;
            score += state.playerHp + state.playerBlock >= state.IncomingDamage ? 20 : -20;
            score += state.synergyBonus;
        }
    }

    private sealed class ComboState
    {
        public int playerHp;
        public int playerMaxHp;
        public int playerBlock;
        public int might;
        public int attackDefenseUses;
        public int attackDefenseLimit;
        public int defeatedEnemies;
        public int synergyBonus;
        public readonly List<ComboEnemy> enemies = new List<ComboEnemy>();

        public int TotalEnemyHp
        {
            get
            {
                int total = 0;
                foreach (ComboEnemy enemy in enemies)
                {
                    total += Math.Max(0, enemy.hp);
                }
                return total;
            }
        }

        public int IncomingDamage
        {
            get
            {
                int total = 0;
                foreach (ComboEnemy enemy in enemies)
                {
                    if (enemy.hp > 0)
                    {
                        total += enemy.incomingDamage;
                    }
                }
                return total;
            }
        }

        public static ComboState Create(PolishVisibleBattleSnapshot snapshot)
        {
            ComboState state = new ComboState
            {
                playerHp = snapshot.playerHp,
                playerMaxHp = snapshot.playerMaxHp,
                playerBlock = snapshot.playerBlock,
                attackDefenseUses = snapshot.attackDefenseUseCount,
                attackDefenseLimit = snapshot.attackDefenseUseLimit,
                might = GetStatusValue(snapshot.playerStatuses, StatusEffectType.Might)
            };
            foreach (PolishVisibleEnemySnapshot enemy in snapshot.enemies)
            {
                PolishVisibleBattleSnapshot single = new PolishVisibleBattleSnapshot();
                single.enemies.Add(enemy);
                state.enemies.Add(new ComboEnemy
                {
                    hp = enemy.hp,
                    block = enemy.block,
                    harpoon = enemy.harpoonStack,
                    vulnerable = GetStatusValue(enemy.statuses, StatusEffectType.Vulnerable),
                    incomingDamage = EstimateIncomingDamage(single)
                });
            }
            return state;
        }

        public ComboState Clone()
        {
            ComboState clone = new ComboState
            {
                playerHp = playerHp,
                playerMaxHp = playerMaxHp,
                playerBlock = playerBlock,
                might = might,
                attackDefenseUses = attackDefenseUses,
                attackDefenseLimit = attackDefenseLimit,
                defeatedEnemies = defeatedEnemies,
                synergyBonus = synergyBonus
            };
            foreach (ComboEnemy enemy in enemies)
            {
                clone.enemies.Add(enemy.Clone());
            }
            return clone;
        }

        public bool CanUse(PolishVisibleCardSnapshot card)
        {
            bool limited = card.cardType.Equals("Attack", StringComparison.OrdinalIgnoreCase) ||
                           card.cardType.Equals("Defense", StringComparison.OrdinalIgnoreCase);
            return !limited || attackDefenseLimit <= 0 || attackDefenseUses < attackDefenseLimit;
        }

        public void Apply(PolishVisibleCardSnapshot card, int enemyIndex)
        {
            bool limited = card.cardType.Equals("Attack", StringComparison.OrdinalIgnoreCase) ||
                           card.cardType.Equals("Defense", StringComparison.OrdinalIgnoreCase);
            if (limited)
            {
                attackDefenseUses++;
            }

            foreach (PolishVisibleCardEffectSnapshot effect in card.effects)
            {
                ApplyEffect(effect, enemyIndex);
            }
        }

        private void ApplyEffect(PolishVisibleCardEffectSnapshot effect, int enemyIndex)
        {
            ComboEnemy enemy = enemyIndex >= 0 && enemyIndex < enemies.Count
                ? enemies[enemyIndex]
                : null;
            int repeats = Math.Max(1, effect.repeatCount);
            switch (effect.effectType)
            {
                case CardEffectType.DealDamage:
                    if (enemy != null)
                    {
                        if (might > 0 && repeats > 1)
                        {
                            synergyBonus += might * (repeats - 1) * 2;
                        }
                        DealDamage(enemy, (effect.value + might) * repeats);
                    }
                    break;
                case CardEffectType.GainBlock:
                    playerBlock += effect.value;
                    break;
                case CardEffectType.Heal:
                    playerHp = Math.Min(playerMaxHp, playerHp + effect.value);
                    break;
                case CardEffectType.LoseHealth:
                    playerHp = Math.Max(0, playerHp - effect.value);
                    break;
                case CardEffectType.ApplyHarpoon:
                case CardEffectType.HarpoonerStack:
                    if (enemy != null)
                    {
                        enemy.harpoon += effect.value;
                        synergyBonus += 6;
                    }
                    break;
                case CardEffectType.DealDamageEqualToHarpoonerStack:
                    if (enemy != null)
                    {
                        synergyBonus += enemy.harpoon * 3;
                        DealDamage(enemy, enemy.harpoon);
                    }
                    break;
                case CardEffectType.ApplyStatus:
                    ApplyStatus(effect, enemy);
                    break;
            }
        }

        private void ApplyStatus(PolishVisibleCardEffectSnapshot effect, ComboEnemy enemy)
        {
            if (effect.statusEffectType == StatusEffectType.Might &&
                effect.target == CardTargetType.Self)
            {
                might += effect.value;
                synergyBonus += 8;
            }
            else if (effect.statusEffectType == StatusEffectType.Vulnerable && enemy != null)
            {
                enemy.vulnerable += effect.value;
                synergyBonus += 10;
            }
            else if (effect.statusEffectType == StatusEffectType.Weaken && enemy != null)
            {
                enemy.incomingDamage = (int)Math.Floor(enemy.incomingDamage * 0.6f);
                synergyBonus += 6;
            }
        }

        private void DealDamage(ComboEnemy enemy, int damage)
        {
            if (enemy.hp <= 0)
            {
                return;
            }
            if (enemy.vulnerable > 0)
            {
                damage = (int)Math.Ceiling(damage * 1.4f);
                synergyBonus += 8;
            }
            int absorbed = Math.Min(enemy.block, damage);
            enemy.block -= absorbed;
            enemy.hp -= damage - absorbed;
            if (enemy.hp <= 0)
            {
                defeatedEnemies++;
            }
        }

        private static int GetStatusValue(
            List<PolishVisibleStatusSnapshot> statuses,
            StatusEffectType type)
        {
            if (statuses == null)
            {
                return 0;
            }
            PolishVisibleStatusSnapshot status = statuses.Find(
                item => item != null && item.type == type.ToString());
            return status != null ? status.value : 0;
        }
    }

    private sealed class ComboEnemy
    {
        public int hp;
        public int block;
        public int harpoon;
        public int vulnerable;
        public int incomingDamage;

        public ComboEnemy Clone()
        {
            return (ComboEnemy)MemberwiseClone();
        }
    }
}
