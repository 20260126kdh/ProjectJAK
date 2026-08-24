#if UNITY_EDITOR || POLISH_SIMULATION_BUILD

using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// F7 캠페인의 시작 덱 확인과 첫 전투 튜토리얼을 실제 공개 입력 경로로 진행합니다.
/// </summary>
public sealed class PolishTutorialAutomationController : MonoBehaviour
{
    private float actionInterval = 0.2f;
    private const float SetupTimeout = 12f;
    private const float TutorialTimeout = 120f;

    private PolishHumanActionExecutor actionExecutor;
    private Action completedCallback;
    private Action<string> failedCallback;
    private Coroutine automationRoutine;

    /// <summary>
    /// 자동 테스트 모드에 맞춰 튜토리얼 입력 간격을 설정합니다.
    /// </summary>
    /// <param name="fastMode">30분 목표 고속 모드 여부</param>
    public void ConfigureExecutionMode(bool fastMode)
    {
        actionInterval = fastMode ? 0f : 0.2f;
    }

    /// <summary>
    /// 튜토리얼 완료와 실패 결과를 받을 콜백을 연결합니다.
    /// </summary>
    /// <param name="onCompleted">튜토리얼 정상 완료 콜백</param>
    /// <param name="onFailed">자동 진행 실패 원인을 받을 콜백</param>
    public void Initialize(
        Action onCompleted,
        Action<string> onFailed)
    {
        actionExecutor = GetComponent<PolishHumanActionExecutor>();
        completedCallback = onCompleted;
        failedCallback = onFailed;
    }

    /// <summary>
    /// BattleScene의 시작 덱 확인부터 튜토리얼 완료까지 자동 진행합니다.
    /// </summary>
    public void BeginBattleEntryAutomation()
    {
        if (automationRoutine != null)
        {
            return;
        }

        automationRoutine = StartCoroutine(RunAutomation());
    }

    private IEnumerator RunAutomation()
    {
        float setupStartedAt = Time.realtimeSinceStartup;
        StartingDeckUI startingDeckUI = null;

        while (startingDeckUI == null ||
               !startingDeckUI.IsStartingDeckConfirmation)
        {
            startingDeckUI = FindFirstObjectByType<StartingDeckUI>();
            if (Time.realtimeSinceStartup - setupStartedAt >= SetupTimeout)
            {
                Fail("시작 덱 확인 화면 준비 시간 초과");
                yield break;
            }

            yield return null;
        }

        startingDeckUI.ConfirmStartingDeck();
        yield return WaitForActionInterval();

        TutorialManager tutorialManager =
            FindFirstObjectByType<TutorialManager>();
        HandManager handManager = FindFirstObjectByType<HandManager>();
        if (tutorialManager == null || handManager == null ||
            actionExecutor == null)
        {
            Fail("튜토리얼 자동 진행에 필요한 전투 구성요소 탐색 실패");
            yield break;
        }

        float tutorialStartedAt = Time.realtimeSinceStartup;
        bool tutorialStarted = tutorialManager.IsTutorialRunning;

        while (true)
        {
            if (Time.realtimeSinceStartup - tutorialStartedAt >=
                TutorialTimeout)
            {
                Fail("튜토리얼 자동 진행 시간 초과");
                yield break;
            }

            tutorialStarted |= tutorialManager.IsTutorialRunning;
            if (tutorialStarted && !tutorialManager.IsTutorialRunning)
            {
                automationRoutine = null;
                completedCallback?.Invoke();
                yield break;
            }

            if (handManager.IsCardFlowBusy)
            {
                yield return null;
                continue;
            }

            if (tutorialManager.IsTutorialActive)
            {
                tutorialManager.ShowNextDialogue();
                yield return WaitForActionInterval();
                continue;
            }

            if (tutorialManager.CanStartPreserve)
            {
                handManager.StartPreserveMode();
                yield return WaitForActionInterval();
                continue;
            }

            if (tutorialManager.CanHandlePreserveInput)
            {
                if (!TrySelectAllowedCard(
                        tutorialManager,
                        handManager,
                        true,
                        true,
                        out _))
                {
                    Fail("튜토리얼 보존 대상 카드를 찾지 못했습니다");
                    yield break;
                }

                yield return WaitForActionInterval();
                handManager.ConfirmPreserveCard();
                yield return WaitForActionInterval();
                continue;
            }

            if (tutorialManager.CanEndTurnAfterPreserve)
            {
                // 보존 확정의 버림 연출이 끝나면 HandManager가 실제 턴 종료를 호출합니다.
                yield return null;
                continue;
            }

            if (tutorialManager.IsTutorialRunning &&
                TrySelectAllowedCard(
                    tutorialManager,
                    handManager,
                    false,
                    false,
                    out int handIndex))
            {
                CardData cardData = handManager.HandCards[handIndex];
                PolishDecisionTarget target =
                    PolishBattleObserver.ResolveRequiredTarget(cardData);
                PolishActionExecutionResult result =
                    actionExecutor.Execute(new PolishHumanDecision
                    {
                        decisionType = PolishDecisionType.UseCard,
                        target = target,
                        handIndex = handIndex,
                        enemyIndex = target == PolishDecisionTarget.Enemy
                            ? 0
                            : -1,
                        crewOrder = target == PolishDecisionTarget.Crew
                            ? 1
                            : -1,
                        reason = "튜토리얼 지정 카드 사용"
                    });
                if (result != PolishActionExecutionResult.Success)
                {
                    Fail($"튜토리얼 카드 사용 실패: {result}");
                    yield break;
                }

                yield return WaitForActionInterval();
                continue;
            }

            yield return null;
        }
    }

    private IEnumerator WaitForActionInterval()
    {
        if (actionInterval <= 0f)
        {
            yield return null;
            yield break;
        }

        yield return new WaitForSecondsRealtime(actionInterval);
    }

    private static bool TrySelectAllowedCard(
        TutorialManager tutorialManager,
        HandManager handManager,
        bool isPreserveMode,
        bool selectCard,
        out int handIndex)
    {
        for (int index = 0; index < handManager.HandCards.Count; index++)
        {
            CardData cardData = handManager.HandCards[index];
            if (!tutorialManager.CanSelectCard(cardData, isPreserveMode))
            {
                continue;
            }

            if (selectCard)
            {
                handManager.SelectCardByIndex(index);
            }
            handIndex = index;
            return true;
        }

        handIndex = -1;
        return false;
    }

    private void Fail(string message)
    {
        automationRoutine = null;
        failedCallback?.Invoke(message);
    }
}

#endif
