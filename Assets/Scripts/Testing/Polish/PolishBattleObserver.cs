using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 실제 전투 씬에서 플레이어가 UI로 확인할 수 있는 현재 정보만 수집합니다.
/// AutoPlayer가 미래 정보나 내부 RNG에 접근하지 못하도록 단일 관찰 진입점을 제공합니다.
/// </summary>
public class PolishBattleObserver : MonoBehaviour
{
    /// <summary>
    /// 현재 화면에 공개된 전투 상태의 복사본을 생성합니다.
    /// </summary>
    /// <returns>현재 시점의 공개 정보 스냅샷</returns>
    public PolishVisibleBattleSnapshot Capture()
    {
        PolishVisibleBattleSnapshot snapshot = new PolishVisibleBattleSnapshot();

        PlayerCombat playerCombat = FindFirstObjectByType<PlayerCombat>();
        PlayerData playerData = GameManager.Instance != null
            ? GameManager.Instance.PlayerData
            : null;
        TurnManager turnManager = FindFirstObjectByType<TurnManager>();
        HandManager handManager = FindFirstObjectByType<HandManager>();

        snapshot.playerHp = playerData != null ? playerData.CurrentHP : 0;
        snapshot.playerMaxHp = playerData != null ? playerData.MaxHP : 0;
        snapshot.playerBlock = playerCombat != null ? playerCombat.CurrentBlock : 0;
        snapshot.isPlayerTurn = turnManager != null && turnManager.IsPlayerTurn;
        snapshot.attackDefenseUseCount = turnManager != null
            ? turnManager.CurrentAttackDefenseCardUseCount
            : 0;
        snapshot.attackDefenseUseLimit = turnManager != null
            ? turnManager.MaxAttackDefenseCardUseCount
            : 0;

        CaptureCards(snapshot, handManager, turnManager);
        CaptureStatuses(snapshot.playerStatuses,
            playerCombat != null ? playerCombat.GetComponent<StatusEffectHandler>() : null);
        CaptureEnemies(snapshot);
        CaptureCrews(snapshot);
        return snapshot;
    }

    private static void CaptureCards(
        PolishVisibleBattleSnapshot snapshot,
        HandManager handManager,
        TurnManager turnManager)
    {
        if (handManager == null || handManager.HandCards == null)
        {
            return;
        }

        foreach (CardData card in handManager.HandCards)
        {
            if (card == null)
            {
                continue;
            }

            snapshot.hand.Add(new PolishVisibleCardSnapshot
            {
                cardId = card.cardID,
                displayName = card.GetDisplayName(),
                description = card.DisplayDescription,
                cardType = card.cardType.ToString(),
                isUpgraded = card.IsUpgraded,
                isUsable = turnManager == null || turnManager.CanUseCard(card)
            });
        }
    }

    private static void CaptureEnemies(PolishVisibleBattleSnapshot snapshot)
    {
        Enemy[] enemies = FindObjectsByType<Enemy>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None);

        foreach (Enemy enemy in enemies)
        {
            if (enemy == null || enemy.CurrentHP <= 0)
            {
                continue;
            }

            PolishVisibleEnemySnapshot enemySnapshot = new PolishVisibleEnemySnapshot
            {
                objectName = enemy.gameObject.name,
                hp = enemy.CurrentHP,
                maxHp = enemy.MaxHP,
                block = enemy.CurrentBlock
            };

            HarpoonStackController harpoon = enemy.GetComponent<HarpoonStackController>();
            enemySnapshot.harpoonStack = harpoon != null ? harpoon.CurrentHarpoonStack : 0;

            EnemyIntentUI intentUI = enemy.GetComponentInChildren<EnemyIntentUI>(true);
            if (intentUI != null)
            {
                enemySnapshot.intents.AddRange(intentUI.VisibleIntentTexts);
            }

            CaptureStatuses(
                enemySnapshot.statuses,
                enemy.GetComponent<StatusEffectHandler>());
            snapshot.enemies.Add(enemySnapshot);
        }
    }

    private static void CaptureCrews(PolishVisibleBattleSnapshot snapshot)
    {
        CrewManager crewManager = FindFirstObjectByType<CrewManager>();
        if (crewManager == null)
        {
            return;
        }

        IReadOnlyList<Crew> crews = crewManager.Crews;
        for (int index = 0; index < crews.Count; index++)
        {
            Crew crew = crews[index];
            if (crew == null || !crew.IsAlive)
            {
                continue;
            }

            snapshot.crews.Add(new PolishVisibleCrewSnapshot
            {
                order = index + 1,
                hp = crew.CurrentHP,
                maxHp = crew.MaxHP
            });
        }
    }

    private static void CaptureStatuses(
        List<PolishVisibleStatusSnapshot> target,
        StatusEffectHandler handler)
    {
        if (handler == null || handler.StatusEffects == null)
        {
            return;
        }

        foreach (StatusEffectData status in handler.StatusEffects)
        {
            if (status == null)
            {
                continue;
            }

            target.Add(new PolishVisibleStatusSnapshot
            {
                type = status.statusEffectType.ToString(),
                value = status.value,
                remainingTurn = status.remainingTurn,
                isPermanent = status.isPermanent,
                isDebuff = IsDebuff(status.statusEffectType)
            });
        }
    }

    private static bool IsDebuff(StatusEffectType statusType)
    {
        switch (statusType)
        {
            case StatusEffectType.Weaken:
            case StatusEffectType.Vulnerable:
            case StatusEffectType.Cripple:
            case StatusEffectType.NoBlock:
            case StatusEffectType.Broken:
            case StatusEffectType.Jinx:
            case StatusEffectType.Paralyze:
            case StatusEffectType.Toxic:
            case StatusEffectType.Exit:
            case StatusEffectType.MightReduction:
                return true;
            default:
                return false;
        }
    }
}
