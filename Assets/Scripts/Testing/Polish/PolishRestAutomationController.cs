#if UNITY_EDITOR || POLISH_SIMULATION_BUILD

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// F7 캠페인의 휴식, 카드 강화와 보스전 진입을 실제 UI 경로로 진행합니다.
/// </summary>
public sealed class PolishRestAutomationController : MonoBehaviour
{
    private const float PanelTimeout = 15f;
    private const float BossBattleTimeout = 20f;

    private Action bossBattleReadyCallback;
    private Action<string> failedCallback;
    private Coroutine automationRoutine;

    /// <summary>
    /// 휴식 처리 결과 콜백을 연결합니다.
    /// </summary>
    /// <param name="onBossBattleReady">보스전 준비 완료 콜백</param>
    /// <param name="onFailed">휴식 자동화 실패 콜백</param>
    public void Initialize(
        Action onBossBattleReady,
        Action<string> onFailed)
    {
        bossBattleReadyCallback = onBossBattleReady;
        failedCallback = onFailed;
    }

    /// <summary>
    /// 현재 휴식 패널의 회복과 강화를 처리하고 보스전으로 이동합니다.
    /// </summary>
    public void BeginRestAutomation()
    {
        if (automationRoutine == null)
        {
            automationRoutine = StartCoroutine(RunRestAutomation());
        }
    }

    /// <summary>
    /// 강화 가능한 효과를 가진 후보 중 희귀도가 가장 높은 카드 인덱스를 반환합니다.
    /// </summary>
    /// <param name="cards">강화 패널에 공개된 카드 데이터</param>
    /// <returns>선택할 인덱스이며 강화 가능한 카드가 없으면 -1</returns>
    public static int ChooseUpgradeIndex(IReadOnlyList<CardData> cards)
    {
        int bestIndex = -1;
        int bestRarity = int.MinValue;
        if (cards == null)
        {
            return bestIndex;
        }

        for (int index = 0; index < cards.Count; index++)
        {
            CardData card = cards[index];
            if (!HasUpgradeableEffect(card) ||
                (int)card.cardRarity <= bestRarity)
            {
                continue;
            }

            bestIndex = index;
            bestRarity = (int)card.cardRarity;
        }

        return bestIndex;
    }

    private IEnumerator RunRestAutomation()
    {
        RestPanelUI restPanelUI = null;
        float startedAt = Time.realtimeSinceStartup;
        while (restPanelUI == null || !restPanelUI.IsRestPanelOpen)
        {
            restPanelUI = FindFirstObjectByType<RestPanelUI>();
            if (Time.realtimeSinceStartup - startedAt >= PanelTimeout)
            {
                Fail("휴식 패널 준비 시간 초과");
                yield break;
            }

            yield return null;
        }

        PlayerData playerData = GameManager.Instance != null
            ? GameManager.Instance.PlayerData
            : null;
        if (playerData == null)
        {
            Fail("휴식 판단에 필요한 PlayerData 탐색 실패");
            yield break;
        }

        if (playerData.CurrentHP < playerData.MaxHP && !restPanelUI.HasRested)
        {
            restPanelUI.OnClickRest();
            while (restPanelUI.IsRestSequencePlaying)
            {
                yield return null;
            }
        }

        restPanelUI.OnClickUpgrade();
        UpgradePanelUI upgradePanelUI = FindFirstObjectByType<UpgradePanelUI>();
        startedAt = Time.realtimeSinceStartup;
        while (upgradePanelUI == null || !upgradePanelUI.IsUpgradePanelOpen)
        {
            upgradePanelUI = FindFirstObjectByType<UpgradePanelUI>();
            if (Time.realtimeSinceStartup - startedAt >= PanelTimeout)
            {
                Fail("강화 패널 준비 시간 초과");
                yield break;
            }

            yield return null;
        }

        yield return null;
        List<CardData> upgradeCards = new List<CardData>();
        foreach (CardUI cardUI in upgradePanelUI.UpgradeCardUIs)
        {
            upgradeCards.Add(cardUI != null ? cardUI.CardData : null);
        }

        int upgradeIndex = ChooseUpgradeIndex(upgradeCards);
        if (upgradeIndex >= 0 &&
            upgradeIndex < upgradePanelUI.UpgradeCardUIs.Count)
        {
            upgradePanelUI.SelectUpgradeCard(
                upgradePanelUI.UpgradeCardUIs[upgradeIndex]);
            yield return null;
            upgradePanelUI.OnClickConfirm();
            while (upgradePanelUI.IsUpgradeVfxPlaying)
            {
                yield return null;
            }
        }
        else
        {
            upgradePanelUI.OnClickCancel();
        }

        startedAt = Time.realtimeSinceStartup;
        while (!restPanelUI.IsRestPanelOpen)
        {
            if (Time.realtimeSinceStartup - startedAt >= PanelTimeout)
            {
                Fail("강화 이후 휴식 패널 복귀 시간 초과");
                yield break;
            }

            yield return null;
        }

        restPanelUI.OnClickNextBattle();
        startedAt = Time.realtimeSinceStartup;
        while (true)
        {
            EnemySpawner enemySpawner = FindFirstObjectByType<EnemySpawner>();
            if (StageManager.Instance != null &&
                StageManager.Instance.CurrentPhase == StagePhase.BossBattle &&
                enemySpawner != null &&
                enemySpawner.CurrentBattleData != null &&
                enemySpawner.GetActiveEnemies().Count > 0)
            {
                automationRoutine = null;
                bossBattleReadyCallback?.Invoke();
                yield break;
            }

            if (Time.realtimeSinceStartup - startedAt >= BossBattleTimeout)
            {
                Fail("보스전 준비 시간 초과");
                yield break;
            }

            yield return null;
        }
    }

    private static bool HasUpgradeableEffect(CardData card)
    {
        if (card == null || card.IsUpgraded || card.effects == null)
        {
            return false;
        }

        foreach (CardEffectData effect in card.effects)
        {
            if (effect == null)
            {
                continue;
            }

            switch (effect.effectType)
            {
                case CardEffectType.DealDamage:
                case CardEffectType.AllCrewsDealDamageAllEnemies:
                case CardEffectType.GainBlock:
                case CardEffectType.ApplyHarpoon:
                case CardEffectType.HarpoonerStack:
                    return true;
                case CardEffectType.ApplyStatus:
                    switch (effect.statusEffectType)
                    {
                        case StatusEffectType.Might:
                        case StatusEffectType.Guard:
                        case StatusEffectType.Resist:
                        case StatusEffectType.Lifesteal:
                        case StatusEffectType.Echo:
                        case StatusEffectType.Immortal:
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
                    }
                    break;
            }
        }

        return false;
    }

    private void Fail(string message)
    {
        automationRoutine = null;
        failedCallback?.Invoke(message);
    }
}

#endif
