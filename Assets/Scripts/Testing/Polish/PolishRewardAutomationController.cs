#if UNITY_EDITOR || POLISH_SIMULATION_BUILD

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// F7 캠페인의 전투 보상 카드를 선택하고 일반 다음 전투 진입을 기다립니다.
/// </summary>
public sealed class PolishRewardAutomationController : MonoBehaviour
{
    private const float RewardTimeout = 15f;
    private const float NextBattleTimeout = 20f;

    private Action nextBattleReadyCallback;
    private Action<StagePhase> progressionPausedCallback;
    private Action<string> failedCallback;
    private Coroutine automationRoutine;

    /// <summary>
    /// 보상 처리 이후 결과 콜백을 연결합니다.
    /// </summary>
    /// <param name="onNextBattleReady">다음 일반 전투 준비 완료 콜백</param>
    /// <param name="onProgressionPaused">휴식 또는 보스 분기 대기 콜백</param>
    /// <param name="onFailed">자동 보상 처리 실패 콜백</param>
    public void Initialize(
        Action onNextBattleReady,
        Action<StagePhase> onProgressionPaused,
        Action<string> onFailed)
    {
        nextBattleReadyCallback = onNextBattleReady;
        progressionPausedCallback = onProgressionPaused;
        failedCallback = onFailed;
    }

    /// <summary>
    /// 현재 승리 보상 패널을 기다린 뒤 카드 선택과 확정을 시작합니다.
    /// </summary>
    public void BeginRewardAutomation()
    {
        if (automationRoutine == null)
        {
            automationRoutine = StartCoroutine(RunRewardAutomation());
        }
    }

    /// <summary>
    /// 진행 중인 보상 자동화를 중단하고 다음 전투에서 다시 시작할 수 있도록 상태를 초기화합니다.
    /// </summary>
    public void StopRewardAutomation()
    {
        if (automationRoutine != null)
        {
            StopCoroutine(automationRoutine);
            automationRoutine = null;
        }
    }

    /// <summary>
    /// 후보 중 희귀도가 가장 높은 카드의 인덱스를 반환합니다.
    /// 같은 희귀도에서는 먼저 표시된 카드를 선택합니다.
    /// </summary>
    /// <param name="cards">화면에 공개된 보상 카드 데이터</param>
    /// <returns>선택할 인덱스이며 유효한 카드가 없으면 -1</returns>
    public static int ChooseBestRewardIndex(IReadOnlyList<CardData> cards)
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
            if (card == null || (int)card.cardRarity <= bestRarity)
            {
                continue;
            }

            bestIndex = index;
            bestRarity = (int)card.cardRarity;
        }

        return bestIndex;
    }

    /// <summary>
    /// 보상 확정 후 새 적 생성을 기다려야 하는 진행 단계인지 반환합니다.
    /// Stage 3 모르바엘 이후 아리엘은 BossBattle 상태를 유지합니다.
    /// </summary>
    /// <param name="phase">보상 확정 직후의 스테이지 단계</param>
    /// <returns>일반 전투 또는 연속 보스 전투라면 true</returns>
    public static bool ShouldWaitForNextBattle(StagePhase phase)
    {
        return phase == StagePhase.NormalBattle ||
               phase == StagePhase.BossBattle;
    }

    private IEnumerator RunRewardAutomation()
    {
        float startedAt = Time.realtimeSinceStartup;
        RewardPanelUI rewardPanelUI = null;
        while (rewardPanelUI == null ||
               !rewardPanelUI.IsRewardPanelOpen ||
               rewardPanelUI.RewardCardUIs.Count == 0)
        {
            rewardPanelUI = FindFirstObjectByType<RewardPanelUI>();
            if (Time.realtimeSinceStartup - startedAt >= RewardTimeout)
            {
                Fail("보상 패널 또는 보상 카드 준비 시간 초과 / " +
                     BuildRewardDiagnostic(rewardPanelUI));
                yield break;
            }

            yield return null;
        }

        List<CardData> rewardCards = new List<CardData>();
        foreach (CardUI cardUI in rewardPanelUI.RewardCardUIs)
        {
            rewardCards.Add(cardUI != null ? cardUI.CardData : null);
        }

        int selectedIndex = ChooseBestRewardIndex(rewardCards);
        if (selectedIndex < 0 ||
            selectedIndex >= rewardPanelUI.RewardCardUIs.Count)
        {
            Fail("선택할 수 있는 보상 카드가 없습니다");
            yield break;
        }

        EnemySpawner enemySpawner = FindFirstObjectByType<EnemySpawner>();
        rewardPanelUI.SelectRewardCard(
            rewardPanelUI.RewardCardUIs[selectedIndex]);
        yield return null;
        rewardPanelUI.OnClickContinue();

        StageManager stageManager = StageManager.Instance;
        if (stageManager == null)
        {
            Fail("보상 처리 이후 StageManager를 찾지 못했습니다");
            yield break;
        }

        if (!ShouldWaitForNextBattle(stageManager.CurrentPhase))
        {
            automationRoutine = null;
            progressionPausedCallback?.Invoke(stageManager.CurrentPhase);
            yield break;
        }

        float nextBattleStartedAt = Time.realtimeSinceStartup;
        while (true)
        {
            enemySpawner = FindFirstObjectByType<EnemySpawner>();
            if (enemySpawner != null &&
                enemySpawner.CurrentBattleData != null &&
                enemySpawner.GetActiveEnemies().Count > 0)
            {
                automationRoutine = null;
                nextBattleReadyCallback?.Invoke();
                yield break;
            }

            if (Time.realtimeSinceStartup - nextBattleStartedAt >=
                NextBattleTimeout)
            {
                Fail("다음 일반 또는 연속 보스 전투 준비 시간 초과 / " +
                     BuildRewardDiagnostic(rewardPanelUI));
                yield break;
            }

            yield return null;
        }
    }

    private void Fail(string message)
    {
        automationRoutine = null;
        failedCallback?.Invoke(message);
    }

    private static string BuildRewardDiagnostic(RewardPanelUI rewardPanelUI)
    {
        StageManager stageManager = StageManager.Instance;
        EnemySpawner enemySpawner = FindFirstObjectByType<EnemySpawner>();
        BattleManager battleManager = FindFirstObjectByType<BattleManager>();
        return $"Phase:{(stageManager != null ? stageManager.CurrentPhase.ToString() : "Missing")}, " +
               $"Panel:{(rewardPanelUI != null)}, " +
               $"Open:{(rewardPanelUI != null && rewardPanelUI.IsRewardPanelOpen)}, " +
               $"Cards:{(rewardPanelUI != null ? rewardPanelUI.RewardCardUIs.Count : -1)}, " +
               $"BattleRunning:{(battleManager != null && battleManager.IsBattleStarted)}, " +
               $"Encounter:{(enemySpawner?.CurrentBattleData != null ? enemySpawner.CurrentBattleData.BattleId : "Missing")}";
    }
}

#endif
