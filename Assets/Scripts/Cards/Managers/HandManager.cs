using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 전투 중 플레이어의 손패를 관리하는 클래스입니다.
/// 손패 카드 목록 저장, 드로우, 손패 UI 표시, 카드 선택,
/// 보존 카드 선택을 담당합니다.
/// </summary>
public class HandManager : MonoBehaviour
{
    [Header("Deck Manager")]
    [SerializeField]
    private DeckManager deckManager;

    [Header("Battle Manager")]
    [SerializeField]
    private BattleManager battleManager;

    [Header("현재 손패")]
    [SerializeField]
    private List<CardData> handCards = new List<CardData>();

    [Header("현재 생성된 손패 카드 UI")]
    [SerializeField]
    private List<CardUI> handCardUIs = new List<CardUI>();

    [Header("보존된 카드")]
    [SerializeField]
    private CardData preservedCard;

    [Header("현재 보존 선택 중인 카드 UI")]
    [SerializeField]
    private CardUI selectedPreserveCardUI;

    [Header("보존 모드 화면 암전")]
    [SerializeField]
    private GameObject preserveDimOverlay;

    [Header("보존 모드 여부")]
    [SerializeField]
    private bool isPreserveMode;

    [Header("이번 턴 Jinx 카드 인덱스")]
    [SerializeField]
    private int jinxedHandIndex = -1;

    [Header("손패 UI 부모")]
    [SerializeField]
    private Transform handCardParent;

    [Header("뽑을 더미 시작 위치")]
    [SerializeField]
    private RectTransform drawPileTarget;

    [Header("버림 더미 도착 위치")]
    [SerializeField]
    private RectTransform discardPileTarget;

    [Header("버림 도착 VFX")]
    [SerializeField]
    private ParticleSystem discardArrivalVfxPrefab;

    private ParticleSystem activeDiscardArrivalVfx;

    [Header("카드 물빛 변환 VFX")]
    [SerializeField]
    private ParticleSystem discardTransformVfxPrefab;

    [Header("셔플 소용돌이 VFX")]
    [SerializeField]
    private ParticleSystem shuffleVortexVfxPrefab;

    [Header("전용 덱 셔플 VFX")]
    [SerializeField]
    private ShuffleTransferVfx shuffleTransferVfxPrefab;

    [Header("셔플 이동 물살 VFX")]
    [SerializeField]
    private ParticleSystem shuffleFlowVfxPrefab;

    [Header("셔플 도착 VFX")]
    [SerializeField]
    private ParticleSystem shuffleArrivalVfxPrefab;

    [SerializeField]
    private float shuffleVortexLeadDuration = 0.6f;

    [SerializeField]
    private float shuffleFlowMoveDuration = 0.5f;

    [SerializeField]
    private float shuffleFlowSpawnInterval = 0.06f;

    [SerializeField]
    private int shuffleFlowCount = 3;

    [SerializeField]
    private float shuffleFlowArcHeight = 0.6f;

    [SerializeField]
    private float shuffleVortexScale = 0.35f;

    [SerializeField]
    private float shuffleFlowScale = 0.18f;

    [SerializeField]
    private float shuffleArrivalScale = 0.2f;

    [SerializeField]
    private float discardTransformDuration = 0.6f;

    [SerializeField]
    private float discardLiftHeight = 35f;

    [Header("버림 카드 이동 테스트")]
    [SerializeField]
    private float discardMoveDuration = 0.54f;

    [Header("드로우 카드 이동")]
    [SerializeField]
    private float drawMoveDuration = 0.27f;

    [SerializeField]
    private float discardRotationDegrees = 45f;

    [SerializeField]
    private float discardMoveArcHeight = 80f;

    [SerializeField]
    [Range(0f, 1f)]
    private float discardLightPointScale = 0.25f;

    [SerializeField]
    private float discardWaterWrapScale = 0.35f;

    [SerializeField]
    [Range(0f, 1f)]
    private float discardArrivalScale = 0f;

    [SerializeField]
    private float discardArrivalHoldDuration = 0.08f;

    [SerializeField]
    private float discardCardMoveInterval = 0.5f;

    private Coroutine discardMoveTestCoroutine;
    private Coroutine shuffleVfxTestCoroutine;
    private bool isDiscardAnimationPlaying;
    private Coroutine drawAnimationCoroutine;
    private bool isDrawAnimationPlaying;
    private bool applyJinxAfterDraw;
    private bool wasEndTurnButtonActive;
    private Coroutine preserveCardFocusCoroutine;
    private RectTransform focusedPreserveCardRectTransform;
    private Vector3 focusedPreserveCardPosition;
    private Quaternion focusedPreserveCardRotation;
    private Vector3 focusedPreserveCardScale;
    private int focusedPreserveCardSiblingIndex;

    [Header("보존 선택 카드 중앙 표시")]
    [SerializeField]
    private float preserveFocusedCardScale = 1.4f;

    private sealed class DiscardCardTestState
    {
        public CardUI CardUI { get; }
        public RectTransform RectTransform { get; }
        public Vector3 Position { get; }
        public Quaternion Rotation { get; }
        public Vector3 Scale { get; }
        public bool WasCardEnabled { get; }

        public DiscardCardTestState(
            CardUI cardUI,
            RectTransform rectTransform
        )
        {
            CardUI = cardUI;
            RectTransform = rectTransform;
            Position = rectTransform.position;
            Rotation = rectTransform.localRotation;
            Scale = rectTransform.localScale;
            WasCardEnabled = cardUI.enabled;
        }
    }

    [Header("카드 UI 프리팹")]
    [SerializeField]
    private CardUI cardPrefab;

    [Header("턴 종료 버튼 오브젝트")]
    [SerializeField]
    private GameObject endTurnButtonObject;

    [Header("보존 확정 버튼 오브젝트")]
    [SerializeField]
    private GameObject preserveConfirmButtonObject;

    [Header("Turn Manager")]
    [SerializeField]
    private TurnManager turnManager;

    /// <summary>
    /// 현재 손패의 카드 데이터 목록입니다.
    /// </summary>
    public List<CardData> HandCards => handCards;

    /// <summary>
    /// 현재 생성된 손패 카드 UI 목록입니다.
    /// </summary>
    public IReadOnlyList<CardUI> HandCardUIs => handCardUIs;

    /// <summary>
    /// 현재 보존된 카드입니다.
    /// </summary>
    public CardData PreservedCard => preservedCard;

    /// <summary>
    /// 현재 보존 카드 선택 모드인지 반환합니다.
    /// </summary>
    public bool IsPreserveMode => isPreserveMode;

    /// <summary>
    /// 버림 더미 UI 위치를 월드 좌표로 변환하여 도착 VFX를 한 번 재생합니다.
    /// 카드 이동 및 버림 데이터는 변경하지 않습니다.
    /// </summary>
    [ContextMenu("버림 도착 VFX 테스트")]
    public void PlayDiscardArrivalVfxTest()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning(
                "[HandManager] 버림 도착 VFX 테스트는 Play Mode에서만 실행할 수 있습니다."
            );

            return;
        }

        PlayDiscardArrivalVfx(true);
    }

    /// <summary>
    /// 버림 더미 소용돌이 이후 난류가 뽑을 더미로 이동하는 셔플 VFX를 테스트합니다.
    /// 카드 및 덱 데이터는 변경하지 않습니다.
    /// </summary>
    [ContextMenu("셔플 VFX 테스트")]
    public void PlayShuffleVfxTest()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning(
                "[HandManager] 셔플 VFX 테스트는 Play Mode에서만 실행할 수 있습니다."
            );

            return;
        }

        if (shuffleVfxTestCoroutine != null)
        {
            Debug.LogWarning(
                "[HandManager] 셔플 VFX 테스트가 이미 실행 중입니다."
            );

            return;
        }

        shuffleVfxTestCoroutine = StartCoroutine(
            PlayDedicatedShuffleVfxTestCoroutine()
        );
    }

    /// <summary>
    /// 전용 셔플 프리팹으로 응축, 셔플, 이동과 재구성 연출을 테스트합니다.
    /// </summary>
    private IEnumerator PlayDedicatedShuffleVfxTestCoroutine()
    {
        if (
            discardPileTarget == null ||
            drawPileTarget == null ||
            shuffleTransferVfxPrefab == null
        )
        {
            Debug.LogError(
                "[HandManager] 전용 셔플 VFX 테스트 설정이 누락되었습니다."
            );
            shuffleVfxTestCoroutine = null;
            yield break;
        }

        if (
            !TryConvertUiPositionToVfxWorldPosition(
                discardPileTarget.position,
                out Vector3 discardWorldPosition
            ) ||
            !TryConvertUiPositionToVfxWorldPosition(
                drawPileTarget.position,
                out Vector3 drawWorldPosition
            )
        )
        {
            shuffleVfxTestCoroutine = null;
            yield break;
        }

        ShuffleTransferVfx shuffleVfx = Instantiate(
            shuffleTransferVfxPrefab,
            Vector3.zero,
            Quaternion.identity
        );
        bool isCompleted = false;
        shuffleVfx.Play(
            discardWorldPosition,
            drawWorldPosition,
            () => isCompleted = true
        );

        while (!isCompleted && shuffleVfx != null)
        {
            yield return null;
        }

        shuffleVfxTestCoroutine = null;

        Debug.Log(
            "[HandManager] 전용 셔플 VFX 테스트 완료 / 덱 데이터 변경 없음"
        );
    }

    /// <summary>
    /// 셔플 VFX의 순서와 이동 경로만 재생합니다.
    /// </summary>
    private IEnumerator PlayShuffleVfxTestCoroutine()
    {
        if (
            discardPileTarget == null ||
            drawPileTarget == null ||
            shuffleVortexVfxPrefab == null ||
            shuffleFlowVfxPrefab == null ||
            shuffleArrivalVfxPrefab == null
        )
        {
            Debug.LogError(
                "[HandManager] 셔플 VFX 테스트에 필요한 위치 또는 프리팹이 연결되지 않았습니다."
            );
            shuffleVfxTestCoroutine = null;
            yield break;
        }

        if (
            !TryConvertUiPositionToVfxWorldPosition(
                discardPileTarget.position,
                out Vector3 discardWorldPosition
            ) ||
            !TryConvertUiPositionToVfxWorldPosition(
                drawPileTarget.position,
                out Vector3 drawWorldPosition
            )
        )
        {
            shuffleVfxTestCoroutine = null;
            yield break;
        }

        ParticleSystem vortexVfx = Instantiate(
            shuffleVortexVfxPrefab,
            discardWorldPosition,
            Quaternion.identity
        );
        vortexVfx.transform.localScale =
            Vector3.one * Mathf.Max(0f, shuffleVortexScale);
        DestroyParticleVfxAfterPlayback(vortexVfx);

        yield return new WaitForSeconds(
            Mathf.Max(0f, shuffleVortexLeadDuration)
        );

        int flowCount = Mathf.Max(1, shuffleFlowCount);

        for (int i = 0; i < flowCount; i++)
        {
            StartCoroutine(
                AnimateShuffleFlowCoroutine(
                    discardWorldPosition,
                    drawWorldPosition,
                    i,
                    flowCount
                )
            );

            if (i < flowCount - 1)
            {
                yield return new WaitForSeconds(
                    Mathf.Max(0f, shuffleFlowSpawnInterval)
                );
            }
        }

        yield return new WaitForSeconds(
            Mathf.Max(0f, shuffleFlowMoveDuration)
        );

        ParticleSystem arrivalVfx = Instantiate(
            shuffleArrivalVfxPrefab,
            drawWorldPosition,
            Quaternion.identity
        );
        arrivalVfx.transform.localScale =
            Vector3.one * Mathf.Max(0f, shuffleArrivalScale);
        DisableTallShuffleArrivalParts(arrivalVfx.transform);
        DestroyParticleVfxAfterPlayback(arrivalVfx);

        shuffleVfxTestCoroutine = null;

        Debug.Log(
            "[HandManager] 셔플 VFX 테스트 완료 / 덱 데이터 변경 없음"
        );
    }

    /// <summary>
    /// 작은 물살 하나를 서로 다른 곡선으로 이동시켜
    /// 여러 장의 카드가 물살에 섞여 이동하는 느낌을 만듭니다.
    /// </summary>
    private IEnumerator AnimateShuffleFlowCoroutine(
        Vector3 startPosition,
        Vector3 endPosition,
        int flowIndex,
        int flowCount)
    {
        Vector3 travelDirection = endPosition - startPosition;
        Vector3 perpendicular = new Vector3(
            -travelDirection.y,
            travelDirection.x,
            0f
        ).normalized;
        float centeredIndex =
            flowIndex - (flowCount - 1) * 0.5f;
        Vector3 spreadOffset =
            perpendicular * centeredIndex * 0.18f;
        Vector3 flowStartPosition =
            startPosition + spreadOffset;
        Vector3 controlPosition =
            Vector3.Lerp(flowStartPosition, endPosition, 0.5f) +
            Vector3.up *
            Mathf.Max(0f, shuffleFlowArcHeight) *
            (0.8f + flowIndex * 0.2f) +
            spreadOffset * 0.4f;

        ParticleSystem flowVfx = Instantiate(
            shuffleFlowVfxPrefab,
            flowStartPosition,
            Quaternion.identity
        );
        ConfigureShuffleFlowParts(flowVfx.transform);
        flowVfx.transform.localScale =
            Vector3.one * Mathf.Max(0f, shuffleFlowScale);

        float moveDuration =
            Mathf.Max(0f, shuffleFlowMoveDuration);
        float elapsedTime = 0f;

        while (elapsedTime < moveDuration)
        {
            if (flowVfx == null)
            {
                yield break;
            }

            float progress = moveDuration > 0f
                ? Mathf.Clamp01(elapsedTime / moveDuration)
                : 1f;
            float easedProgress =
                1f - Mathf.Pow(1f - progress, 2f);
            Vector3 firstHalf = Vector3.Lerp(
                flowStartPosition,
                controlPosition,
                easedProgress
            );
            Vector3 secondHalf = Vector3.Lerp(
                controlPosition,
                endPosition,
                easedProgress
            );

            flowVfx.transform.position = Vector3.Lerp(
                firstHalf,
                secondHalf,
                easedProgress
            );

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (flowVfx != null)
        {
            flowVfx.transform.position = endPosition;
            flowVfx.Stop(
                true,
                ParticleSystemStopBehavior.StopEmitting
            );
            DestroyParticleVfxAfterPlayback(flowVfx);
        }
    }

    /// <summary>
    /// 이동 물살에서 고정 지면과 큰 폭발 파트를 제외해
    /// 작은 물빛과 입자 꼬리만 남깁니다.
    /// </summary>
    private void ConfigureShuffleFlowParts(
        Transform flowRoot)
    {
        if (flowRoot == null)
        {
            return;
        }

        for (int i = flowRoot.childCount - 1; i >= 0; i--)
        {
            Transform child = flowRoot.GetChild(i);
            string childName = child.name.ToLowerInvariant();
            bool shouldDisable =
                childName == "explosion_highlight" ||
                childName == "splash_ground" ||
                childName == "splash_view";

            if (shouldDisable)
            {
                child.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 셔플 도착 VFX에서 높은 물기둥만 끄고
    /// 낮은 물결과 입자만 표시합니다.
    /// </summary>
    private void DisableTallShuffleArrivalParts(
        Transform arrivalRoot)
    {
        if (arrivalRoot == null)
        {
            return;
        }

        for (int i = arrivalRoot.childCount - 1; i >= 0; i--)
        {
            Transform child = arrivalRoot.GetChild(i);
            string childName = child.name.ToLowerInvariant();

            if (childName.StartsWith("splash_long"))
            {
                child.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 모든 자식 Particle System의 재생 시간을 계산해 VFX를 제거합니다.
    /// </summary>
    private void DestroyParticleVfxAfterPlayback(
        ParticleSystem rootParticleSystem)
    {
        if (rootParticleSystem == null)
        {
            return;
        }

        float cleanupDelay = 0f;
        ParticleSystem[] particleSystems =
            rootParticleSystem.GetComponentsInChildren<ParticleSystem>(true);

        foreach (ParticleSystem particleSystem in particleSystems)
        {
            ParticleSystem.MainModule main = particleSystem.main;
            float playbackDuration =
                main.startDelay.constantMax +
                main.duration +
                main.startLifetime.constantMax;

            cleanupDelay = Mathf.Max(cleanupDelay, playbackDuration);
        }

        Destroy(rootParticleSystem.gameObject, cleanupDelay);
    }

    /// <summary>
    /// 버림 더미 UI 위치에 도착 VFX를 생성하고 재생 종료 후 제거합니다.
    /// </summary>
    private void PlayDiscardArrivalVfx(bool replaceActiveVfx)
    {
        if (discardPileTarget == null)
        {
            Debug.LogError(
                "[HandManager] 버림 더미 도착 위치가 연결되지 않았습니다."
            );

            return;
        }

        if (discardArrivalVfxPrefab == null)
        {
            Debug.LogError(
                "[HandManager] 버림 도착 VFX 프리팹이 연결되지 않았습니다."
            );

            return;
        }

        Camera mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogError(
                "[HandManager] MainCamera 태그가 지정된 카메라를 찾을 수 없습니다."
            );

            return;
        }

        Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(
            null,
            discardPileTarget.position
        );

        Ray screenRay = mainCamera.ScreenPointToRay(screenPosition);
        Plane vfxPlane = new Plane(Vector3.forward, Vector3.zero);

        if (!vfxPlane.Raycast(screenRay, out float enter))
        {
            Debug.LogError(
                "[HandManager] 버림 더미 화면 위치를 VFX 월드 위치로 변환하지 못했습니다."
            );

            return;
        }

        if (replaceActiveVfx && activeDiscardArrivalVfx != null)
        {
            Destroy(activeDiscardArrivalVfx.gameObject);
        }

        Vector3 worldPosition = screenRay.GetPoint(enter);
        ParticleSystem spawnedVfx = Instantiate(
            discardArrivalVfxPrefab,
            worldPosition,
            Quaternion.identity
        );

        if (replaceActiveVfx)
        {
            activeDiscardArrivalVfx = spawnedVfx;
        }

        float cleanupDelay = 0f;
        ParticleSystem[] particleSystems =
            spawnedVfx.GetComponentsInChildren<ParticleSystem>(true);

        foreach (ParticleSystem particleSystem in particleSystems)
        {
            ParticleSystem.MainModule main = particleSystem.main;
            float playbackDuration =
                main.startDelay.constantMax +
                main.duration +
                main.startLifetime.constantMax;

            cleanupDelay = Mathf.Max(cleanupDelay, playbackDuration);
        }

        Destroy(spawnedVfx.gameObject, cleanupDelay);
    }

    /// <summary>
    /// 현재 손패 카드 UI를 일정한 간격으로 버림 더미 위치까지 이동한 뒤 복원합니다.
    /// 카드 데이터와 버림 더미 데이터는 변경하지 않습니다.
    /// </summary>
    [ContextMenu("버림 더미 카드 이동 테스트")]
    public void PlayDiscardCardMoveTest()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning(
                "[HandManager] 버림 카드 이동 테스트는 Play Mode에서만 실행할 수 있습니다."
            );

            return;
        }

        if (discardMoveTestCoroutine != null)
        {
            Debug.LogWarning(
                "[HandManager] 버림 카드 이동 테스트가 이미 실행 중입니다."
            );

            return;
        }

        if (discardPileTarget == null)
        {
            Debug.LogError(
                "[HandManager] 버림 더미 도착 위치가 연결되지 않았습니다."
            );

            return;
        }

        if (discardTransformVfxPrefab == null)
        {
            Debug.LogError(
                "[HandManager] 카드 물빛 변환 VFX 프리팹이 연결되지 않았습니다."
            );

            return;
        }

        if (handCardUIs.Count == 0 || handCardUIs[0] == null)
        {
            Debug.LogWarning(
                "[HandManager] 이동 테스트에 사용할 손패 카드 UI가 없습니다."
            );

            return;
        }

        discardMoveTestCoroutine = StartCoroutine(
            PlayDiscardCardsMoveTestCoroutine()
        );
    }

    /// <summary>
    /// 테스트 시작 시점의 손패 카드 UI를 복사하여 순서대로 이동을 시작합니다.
    /// </summary>
    private IEnumerator PlayDiscardCardsMoveTestCoroutine()
    {
        yield return PlayDiscardCardsMoveCoroutine(
            new List<CardUI>(handCardUIs),
            true
        );

        discardMoveTestCoroutine = null;
    }

    /// <summary>
    /// 전달받은 카드 UI들을 순차 이동하고 필요할 때 원래 상태로 복원합니다.
    /// </summary>
    private IEnumerator PlayDiscardCardsMoveCoroutine(
        List<CardUI> targetCardUIs,
        bool restoreAfterAnimation
    )
    {
        List<DiscardCardTestState> testCardStates =
            new List<DiscardCardTestState>();

        foreach (CardUI cardUI in targetCardUIs)
        {
            if (cardUI == null)
            {
                continue;
            }

            RectTransform cardRectTransform =
                cardUI.GetComponent<RectTransform>();

            if (cardRectTransform == null)
            {
                Debug.LogError(
                    "[HandManager] 이동 테스트 카드에 RectTransform이 없습니다."
                );

                continue;
            }

            testCardStates.Add(
                new DiscardCardTestState(
                    cardUI,
                    cardRectTransform
                )
            );
        }

        float moveInterval = Mathf.Max(0f, discardCardMoveInterval);

        for (int i = 0; i < testCardStates.Count; i++)
        {
            DiscardCardTestState testCardState = testCardStates[i];

            if (testCardState.CardUI == null)
            {
                continue;
            }

            testCardState.CardUI.enabled = false;

            bool isLastCard = i == testCardStates.Count - 1;
            Coroutine cardMoveCoroutine = StartCoroutine(
                PlayDiscardCardMoveTestCoroutine(
                    testCardState,
                    isLastCard
                )
            );

            bool hasNextCard = !isLastCard;

            if (hasNextCard && moveInterval > 0f)
            {
                yield return new WaitForSeconds(moveInterval);
            }
            else if (!hasNextCard)
            {
                yield return cardMoveCoroutine;
            }
        }

        float holdDuration = Mathf.Max(0f, discardArrivalHoldDuration);

        if (holdDuration > 0f)
        {
            yield return new WaitForSeconds(holdDuration);
        }

        if (restoreAfterAnimation)
        {
            foreach (DiscardCardTestState testCardState in testCardStates)
            {
                RestoreDiscardCardTestState(testCardState);
            }
        }
    }

    /// <summary>
    /// 카드 UI를 물빛으로 변환하고 이동 입자를 버림 더미까지 보냅니다.
    /// </summary>
    private IEnumerator PlayDiscardCardMoveTestCoroutine(
        DiscardCardTestState testCardState,
        bool playArrivalVfx
    )
    {
        if (
            testCardState == null ||
            testCardState.CardUI == null ||
            testCardState.RectTransform == null
        )
        {
            yield break;
        }

        float elapsedTime = 0f;
        float transformDuration = Mathf.Max(0f, discardTransformDuration);
        float lightPointScale = Mathf.Clamp01(discardLightPointScale);
        float arrivalScale = Mathf.Clamp01(discardArrivalScale);

        if (
            !TryConvertUiPositionToVfxWorldPosition(
                testCardState.Position,
                out Vector3 transformWorldPosition
            )
        )
        {
            yield break;
        }

        float minimumWaterWrapLifetime =
            transformDuration +
            Mathf.Max(0f, discardMoveDuration) +
            Mathf.Max(0f, discardArrivalHoldDuration);
        ParticleSystem cardWaterWrap = SpawnTemporaryParticleSystem(
            discardTransformVfxPrefab,
            transformWorldPosition,
            minimumWaterWrapLifetime
        );

        if (cardWaterWrap != null)
        {
            ParticleSystem[] waterWrapParticleSystems =
                cardWaterWrap.GetComponentsInChildren<ParticleSystem>(true);

            foreach (ParticleSystem particleSystem in waterWrapParticleSystems)
            {
                ParticleSystem.MainModule main = particleSystem.main;
                main.simulationSpace = ParticleSystemSimulationSpace.Local;
            }

            float waterWrapScale = Mathf.Max(0f, discardWaterWrapScale);
            cardWaterWrap.transform.localScale =
                Vector3.one * waterWrapScale;
        }

        while (elapsedTime < transformDuration)
        {
            if (
                testCardState.CardUI == null ||
                testCardState.RectTransform == null
            )
            {
                yield break;
            }

            float progress = transformDuration > 0f
                ? Mathf.Clamp01(elapsedTime / transformDuration)
                : 1f;
            float easedProgress =
                progress * progress * (3f - 2f * progress);

            testCardState.RectTransform.position =
                testCardState.Position +
                Vector3.up * discardLiftHeight * easedProgress;
            testCardState.RectTransform.localRotation =
                testCardState.Rotation * Quaternion.Euler(
                    0f,
                    0f,
                    discardRotationDegrees * easedProgress
                );
            testCardState.RectTransform.localScale = Vector3.Lerp(
                testCardState.Scale,
                testCardState.Scale * lightPointScale,
                easedProgress
            );

            UpdateDiscardWaterWrapPosition(
                cardWaterWrap,
                testCardState.RectTransform.position
            );

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (
            testCardState.CardUI == null ||
            testCardState.RectTransform == null ||
            discardPileTarget == null
        )
        {
            yield break;
        }

        Vector3 transformedCardUiPosition =
            testCardState.Position + Vector3.up * discardLiftHeight;
        testCardState.RectTransform.position = transformedCardUiPosition;
        testCardState.RectTransform.localRotation =
            testCardState.Rotation * Quaternion.Euler(
                0f,
                0f,
                discardRotationDegrees
            );
        testCardState.RectTransform.localScale =
            testCardState.Scale * lightPointScale;

        if (cardWaterWrap != null)
        {
            cardWaterWrap.transform.localScale =
                Vector3.one * Mathf.Max(0f, discardWaterWrapScale);
        }

        Vector3 controlUiPosition = Vector3.Lerp(
            transformedCardUiPosition,
            discardPileTarget.position,
            0.5f
        ) + Vector3.up * discardMoveArcHeight;

        elapsedTime = 0f;
        float moveDuration = Mathf.Max(0f, discardMoveDuration);

        while (elapsedTime < moveDuration)
        {
            if (
                testCardState.CardUI == null ||
                testCardState.RectTransform == null ||
                discardPileTarget == null
            )
            {
                yield break;
            }

            float progress = moveDuration > 0f
                ? Mathf.Clamp01(elapsedTime / moveDuration)
                : 1f;
            float easedProgress = progress * progress;
            Vector3 firstHalf = Vector3.Lerp(
                transformedCardUiPosition,
                controlUiPosition,
                easedProgress
            );
            Vector3 secondHalf = Vector3.Lerp(
                controlUiPosition,
                discardPileTarget.position,
                easedProgress
            );

            testCardState.RectTransform.position = Vector3.Lerp(
                firstHalf,
                secondHalf,
                easedProgress
            );
            testCardState.RectTransform.localScale = Vector3.Lerp(
                testCardState.Scale * lightPointScale,
                testCardState.Scale * arrivalScale,
                easedProgress
            );

            UpdateDiscardWaterWrapPosition(
                cardWaterWrap,
                testCardState.RectTransform.position
            );

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (
            testCardState.CardUI != null &&
            testCardState.RectTransform != null &&
            discardPileTarget != null
        )
        {
            testCardState.RectTransform.position =
                discardPileTarget.position;
            testCardState.RectTransform.localScale =
                testCardState.Scale * arrivalScale;
        }

        if (cardWaterWrap != null)
        {
            cardWaterWrap.Stop(
                true,
                ParticleSystemStopBehavior.StopEmitting
            );
        }

        if (playArrivalVfx)
        {
            PlayDiscardArrivalVfx(false);
        }
    }

    /// <summary>
    /// UI 월드 위치를 전투 VFX가 사용하는 월드 평면 위치로 변환합니다.
    /// </summary>
    private bool TryConvertUiPositionToVfxWorldPosition(
        Vector3 uiWorldPosition,
        out Vector3 vfxWorldPosition
    )
    {
        vfxWorldPosition = Vector3.zero;
        Camera mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogError(
                "[HandManager] MainCamera 태그가 지정된 카메라를 찾을 수 없습니다."
            );

            return false;
        }

        Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(
            null,
            uiWorldPosition
        );
        Ray screenRay = mainCamera.ScreenPointToRay(screenPosition);
        Plane vfxPlane = new Plane(Vector3.forward, Vector3.zero);

        if (!vfxPlane.Raycast(screenRay, out float enter))
        {
            Debug.LogError(
                "[HandManager] UI 위치를 VFX 월드 위치로 변환하지 못했습니다."
            );

            return false;
        }

        vfxWorldPosition = screenRay.GetPoint(enter);
        return true;
    }

    /// <summary>
    /// 카드 UI 위치를 따라 물 감싸기 VFX 루트를 이동합니다.
    /// </summary>
    private void UpdateDiscardWaterWrapPosition(
        ParticleSystem cardWaterWrap,
        Vector3 cardUiPosition
    )
    {
        if (
            cardWaterWrap == null ||
            !TryConvertUiPositionToVfxWorldPosition(
                cardUiPosition,
                out Vector3 waterWrapWorldPosition
            )
        )
        {
            return;
        }

        cardWaterWrap.transform.position = waterWrapWorldPosition;
    }

    /// <summary>
    /// 일회성 Particle System을 생성하고 전체 자식 재생이 끝난 뒤 제거합니다.
    /// </summary>
    private ParticleSystem SpawnTemporaryParticleSystem(
        ParticleSystem particleSystemPrefab,
        Vector3 worldPosition,
        float minimumLifetime
    )
    {
        if (particleSystemPrefab == null)
        {
            return null;
        }

        ParticleSystem spawnedParticleSystem = Instantiate(
            particleSystemPrefab,
            worldPosition,
            Quaternion.identity
        );
        float cleanupDelay = 0f;
        ParticleSystem[] particleSystems =
            spawnedParticleSystem.GetComponentsInChildren<ParticleSystem>(true);

        foreach (ParticleSystem particleSystem in particleSystems)
        {
            ParticleSystem.MainModule main = particleSystem.main;
            float playbackDuration =
                main.startDelay.constantMax +
                main.duration +
                main.startLifetime.constantMax;

            cleanupDelay = Mathf.Max(cleanupDelay, playbackDuration);
        }

        Destroy(
            spawnedParticleSystem.gameObject,
            Mathf.Max(cleanupDelay, minimumLifetime)
        );
        return spawnedParticleSystem;
    }

    /// <summary>
    /// 이동 테스트가 끝난 카드의 Transform과 입력 상태를 복원합니다.
    /// </summary>
    private void RestoreDiscardCardTestState(
        DiscardCardTestState testCardState
    )
    {
        if (
            testCardState == null ||
            testCardState.CardUI == null ||
            testCardState.RectTransform == null
        )
        {
            return;
        }

        testCardState.RectTransform.position = testCardState.Position;
        testCardState.RectTransform.localRotation = testCardState.Rotation;
        testCardState.RectTransform.localScale = testCardState.Scale;
        testCardState.CardUI.enabled = testCardState.WasCardEnabled;
    }

    /// <summary>
    /// 지정한 수만큼 카드를 드로우합니다.
    /// </summary>
    public void DrawCards(int drawCount)
    {
        if (deckManager == null)
        {
            Debug.LogError(
                "[HandManager] DeckManager가 연결되지 않았습니다."
            );

            return;
        }

        int firstDrawnCardIndex = handCards.Count;

        for (int i = 0; i < drawCount; i++)
        {
            CardData drawnCard =
                deckManager.DrawOneCard();

            if (drawnCard == null)
            {
                return;
            }

            handCards.Add(drawnCard);

            Debug.Log(
                $"[HandManager] 카드 드로우: " +
                $"{drawnCard.cardName}"
            );
        }

        RefreshHandUI();

        int drawnCardCount =
            handCards.Count - firstDrawnCardIndex;

        if (drawnCardCount > 0)
        {
            StartDrawAnimation(
                firstDrawnCardIndex,
                drawnCardCount
            );
        }

        Debug.Log(
            $"[HandManager] 현재 손패: " +
            $"{handCards.Count}장"
        );
    }

    /// <summary>
    /// 새로 뽑은 카드 UI를 뽑을 더미에서 손패로 순차 이동시킵니다.
    /// </summary>
    private void StartDrawAnimation(
        int firstDrawnCardIndex,
        int drawnCardCount)
    {
        if (drawPileTarget == null)
        {
            Debug.LogError(
                "[HandManager] 뽑을 더미 시작 위치가 연결되지 않아 드로우 연출을 생략합니다."
            );

            return;
        }

        if (drawAnimationCoroutine != null)
        {
            Debug.LogWarning(
                "[HandManager] 드로우 연출이 이미 진행 중입니다."
            );

            return;
        }

        drawAnimationCoroutine = StartCoroutine(
            PlayDrawCardsCoroutine(
                firstDrawnCardIndex,
                drawnCardCount
            )
        );
    }

    /// <summary>
    /// 드로우된 카드를 한 장씩 활성화하여 최종 손패 위치로 이동시킵니다.
    /// </summary>
    private IEnumerator PlayDrawCardsCoroutine(
        int firstDrawnCardIndex,
        int drawnCardCount)
    {
        List<CardUI> drawnCardUIs = new List<CardUI>();
        List<Vector3> targetPositions = new List<Vector3>();
        List<Quaternion> targetRotations = new List<Quaternion>();

        int lastDrawnCardIndex = Mathf.Min(
            firstDrawnCardIndex + drawnCardCount,
            handCardUIs.Count
        );

        for (int i = lastDrawnCardIndex - 1;
             i >= firstDrawnCardIndex;
             i--)
        {
            CardUI cardUI = handCardUIs[i];

            if (cardUI == null)
            {
                continue;
            }

            RectTransform cardRectTransform =
                cardUI.GetComponent<RectTransform>();

            if (cardRectTransform == null)
            {
                continue;
            }

            drawnCardUIs.Add(cardUI);
            targetPositions.Add(cardRectTransform.position);
            targetRotations.Add(cardRectTransform.localRotation);
            cardUI.gameObject.SetActive(false);
        }

        isDrawAnimationPlaying = true;

        if (endTurnButtonObject != null)
        {
            wasEndTurnButtonActive = endTurnButtonObject.activeSelf;
            endTurnButtonObject.SetActive(false);
        }

        float moveDuration = Mathf.Max(0f, drawMoveDuration);

        for (int i = 0; i < drawnCardUIs.Count; i++)
        {
            CardUI cardUI = drawnCardUIs[i];

            if (cardUI == null)
            {
                continue;
            }

            RectTransform cardRectTransform =
                cardUI.GetComponent<RectTransform>();

            if (cardRectTransform == null)
            {
                continue;
            }

            cardUI.gameObject.SetActive(true);
            cardRectTransform.position = drawPileTarget.position;
            cardRectTransform.localRotation = Quaternion.identity;

            float elapsedTime = 0f;

            while (elapsedTime < moveDuration)
            {
                if (cardUI == null || drawPileTarget == null)
                {
                    CompleteDrawAnimation(drawnCardUIs);
                    yield break;
                }

                float progress = moveDuration > 0f
                    ? Mathf.Clamp01(elapsedTime / moveDuration)
                    : 1f;
                float easedProgress = progress * progress;

                cardRectTransform.position = Vector3.Lerp(
                    drawPileTarget.position,
                    targetPositions[i],
                    easedProgress
                );
                cardRectTransform.localRotation = Quaternion.Lerp(
                    Quaternion.identity,
                    targetRotations[i],
                    easedProgress
                );

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            cardRectTransform.position = targetPositions[i];
            cardRectTransform.localRotation = targetRotations[i];
        }

        CompleteDrawAnimation(drawnCardUIs);
    }

    /// <summary>
    /// 드로우 연출 상태를 정리하고 대기 중인 Jinx 처리를 실행합니다.
    /// </summary>
    private void CompleteDrawAnimation(List<CardUI> drawnCardUIs)
    {
        foreach (CardUI cardUI in drawnCardUIs)
        {
            if (cardUI != null)
            {
                cardUI.gameObject.SetActive(true);
            }
        }

        isDrawAnimationPlaying = false;
        drawAnimationCoroutine = null;

        if (endTurnButtonObject != null && wasEndTurnButtonActive)
        {
            endTurnButtonObject.SetActive(true);
        }

        if (applyJinxAfterDraw)
        {
            applyJinxAfterDraw = false;
            ApplyJinxToRandomCard();
        }
    }

    /// <summary>
    /// 현재 손패 데이터에 맞춰 카드 UI를 다시 생성합니다.
    /// 생성된 CardUI는 손패 순서대로 handCardUIs에 저장됩니다.
    /// </summary>
    public void RefreshHandUI()
    {
        ClearHandUI();

        if (handCardParent == null)
        {
            Debug.LogError(
                "[HandManager] HandCardParent가 연결되지 않았습니다."
            );

            return;
        }

        if (cardPrefab == null)
        {
            Debug.LogError(
                "[HandManager] CardPrefab이 연결되지 않았습니다."
            );

            return;
        }

        int cardCount =
            handCards.Count;

        float cardSpacing = 180f;
        float rotationSpacing = 8f;
        float curveHeight = 25f;

        for (int i = 0; i < cardCount; i++)
        {
            CardUI cardUI =
                Instantiate(
                    cardPrefab,
                    handCardParent
                );

            if (cardUI == null)
            {
                Debug.LogError(
                    $"[HandManager] {i + 1}번째 카드 UI 생성에 실패했습니다."
                );

                continue;
            }

            RectTransform rect =
                cardUI.GetComponent<RectTransform>();

            if (rect != null)
            {
                float centerIndex =
                    (cardCount - 1) / 2f;

                float offset =
                    i - centerIndex;

                float x =
                    offset * cardSpacing;

                float y =
                    -Mathf.Abs(offset) * curveHeight;

                float zRotation =
                    -offset * rotationSpacing;

                rect.anchoredPosition =
                    new Vector2(x, y);

                rect.localRotation =
                    Quaternion.Euler(
                        0f,
                        0f,
                        zRotation
                    );
            }

            cardUI.Initialize(
                handCards[i],
                this
            );

            cardUI.SetJinxed(
                i == jinxedHandIndex
            );

            /*
             * 숫자키 선택을 위해 생성된 카드 UI를
             * 손패 순서대로 저장합니다.
             *
             * handCardUIs[0] = 첫 번째 카드
             * handCardUIs[1] = 두 번째 카드
             */
            handCardUIs.Add(cardUI);
        }

        Debug.Log(
            $"[HandManager] 손패 UI 갱신 완료 / " +
            $"생성된 UI: {handCardUIs.Count}개"
        );
    }

    /// <summary>
    /// 손패 순서를 기준으로 카드를 선택합니다.
    ///
    /// index는 0부터 시작합니다.
    /// 0 = 첫 번째 카드
    /// 1 = 두 번째 카드
    /// 2 = 세 번째 카드
    /// 3 = 네 번째 카드
    ///
    /// 일반 전투에서는 BattleManager의 카드 선택을 사용하고,
    /// 보존 모드에서는 보존 카드 선택을 사용합니다.
    /// </summary>
    /// <param name="index">선택할 손패 UI 인덱스</param>
    public void SelectCardByIndex(int index)
    {
        if (
            isDrawAnimationPlaying ||
            isDiscardAnimationPlaying ||
            discardMoveTestCoroutine != null
        )
        {
            return;
        }

        if (index < 0 || index >= handCardUIs.Count)
        {
            Debug.Log(
                $"[HandManager] 선택할 수 없는 손패 번호입니다: " +
                $"{index + 1}"
            );

            return;
        }

        CardUI cardUI =
            handCardUIs[index];

        if (cardUI == null)
        {
            Debug.LogWarning(
                $"[HandManager] {index + 1}번째 CardUI가 없습니다."
            );

            return;
        }

        RequestSelectCard(cardUI);
    }

    /// <summary>
    /// 플레이어가 Jinx를 보유했다면 다음 턴 손패가 완성된 후
    /// 보존 카드를 포함한 전체 손패 중 무작위 카드 한 장을
    /// 이번 턴 사용 불가 상태로 지정합니다.
    /// </summary>
    public void ApplyJinxToRandomCard()
    {
        if (isDrawAnimationPlaying)
        {
            applyJinxAfterDraw = true;
            return;
        }

        ClearJinxedCard();

        PlayerCombat playerCombat =
            FindFirstObjectByType<PlayerCombat>();

        if (playerCombat == null)
        {
            Debug.LogWarning(
                "[HandManager] Jinx 처리를 위한 PlayerCombat을 " +
                "찾지 못했습니다."
            );

            return;
        }

        StatusEffectHandler statusEffectHandler =
            playerCombat.GetComponent<StatusEffectHandler>();

        if (statusEffectHandler == null)
        {
            return;
        }

        if (!statusEffectHandler.HasStatusEffect(
                StatusEffectType.Jinx
            ))
        {
            return;
        }

        if (handCards.Count <= 0)
        {
            Debug.LogWarning(
                "[HandManager] Jinx가 발동했지만 손패가 없습니다."
            );

            return;
        }

        jinxedHandIndex =
            Random.Range(
                0,
                handCards.Count
            );

        RefreshHandUI();

        Debug.Log(
            $"[HandManager] Jinx 발동 : " +
            $"{jinxedHandIndex + 1}번째 카드 " +
            $"{handCards[jinxedHandIndex].cardName} 사용 불가"
        );
    }

    /// <summary>
    /// 전달받은 CardUI가 현재 Jinx로 사용 불가인 카드인지 확인합니다.
    /// </summary>
    public bool IsCardJinxed(CardUI cardUI)
    {
        if (cardUI == null)
        {
            return false;
        }

        return cardUI.IsJinxed;
    }

    /// <summary>
    /// 이번 턴의 Jinx 카드 지정을 해제합니다.
    /// </summary>
    public void ClearJinxedCard()
    {
        jinxedHandIndex = -1;
    }

    /// <summary>
    /// 카드 선택 요청을 처리합니다.
    /// 일반 전투와 보존 모드를 구분해 적절한 선택 로직을 호출합니다.
    /// </summary>
    public void RequestSelectCard(CardUI cardUI)
    {
        if (
            isDrawAnimationPlaying ||
            isDiscardAnimationPlaying ||
            discardMoveTestCoroutine != null
        )
        {
            return;
        }

        if (cardUI == null)
        {
            Debug.LogWarning(
                "[HandManager] 선택 요청된 CardUI가 없습니다."
            );

            return;
        }

        if (isPreserveMode)
        {
            SelectPreserveCard(cardUI);
            return;
        }

        if (battleManager == null)
        {
            Debug.LogError(
                "[HandManager] BattleManager가 연결되지 않았습니다."
            );

            return;
        }

        battleManager.SelectCard(cardUI);
    }

    /// <summary>
    /// 보존 모드를 시작합니다.
    /// </summary>
    public void StartPreserveMode()
    {
        if (
            isDrawAnimationPlaying ||
            isDiscardAnimationPlaying ||
            discardMoveTestCoroutine != null
        )
        {
            Debug.LogWarning(
                "[HandManager] 버림 연출 중에는 보존 모드를 시작할 수 없습니다."
            );

            return;
        }

        isPreserveMode = true;
        selectedPreserveCardUI = null;
        SetPreserveDimActive(true);

        if (endTurnButtonObject != null)
        {
            endTurnButtonObject.SetActive(false);
        }

        if (preserveConfirmButtonObject != null)
        {
            preserveConfirmButtonObject.SetActive(true);
        }

        if (battleManager != null)
        {
            battleManager.ClearSelectedCard();
        }

        RefreshHandUI();

        Debug.Log(
            "[HandManager] 보존 모드 시작"
        );
    }

    /// <summary>
    /// 현재 보존 선택을 해제하고 보존 모드 진입 전 상태로 복귀합니다.
    /// 기존에 보존된 카드 데이터와 턴 진행은 변경하지 않습니다.
    /// </summary>
    public void CancelPreserveMode()
    {
        if (!isPreserveMode)
        {
            return;
        }

        RestoreFocusedPreserveCard();

        selectedPreserveCardUI = null;
        isPreserveMode = false;
        SetPreserveDimActive(false);

        if (endTurnButtonObject != null)
        {
            endTurnButtonObject.SetActive(true);
        }

        if (preserveConfirmButtonObject != null)
        {
            preserveConfirmButtonObject.SetActive(false);
        }

        Debug.Log(
            "[HandManager] 보존 모드 취소"
        );
    }

    /// <summary>
    /// 보존 모드에서 보존할 카드 UI를 선택합니다.
    /// 같은 카드 UI를 다시 선택하면 선택을 해제합니다.
    /// </summary>
    private void SelectPreserveCard(CardUI cardUI)
    {
        CardData cardData =
            cardUI.GetCardData();

        if (cardData == null)
        {
            Debug.LogWarning(
                "[HandManager] 보존 선택할 카드 데이터가 없습니다."
            );

            return;
        }

        if (!handCards.Contains(cardData))
        {
            Debug.LogWarning(
                $"[HandManager] 손패에 없는 카드는 " +
                $"보존할 수 없습니다: {cardData.cardName}"
            );

            return;
        }

        if (selectedPreserveCardUI == cardUI)
        {
            RestoreFocusedPreserveCard();
            selectedPreserveCardUI = null;

            Debug.Log(
                "[HandManager] 보존 카드 선택 해제"
            );

            return;
        }

        if (selectedPreserveCardUI != null)
        {
            RestoreFocusedPreserveCard();
        }

        selectedPreserveCardUI = cardUI;
        FocusPreserveCard(cardUI);

        Debug.Log(
            $"[HandManager] 보존 카드 선택: " +
            $"{cardData.cardName}"
        );
    }

    /// <summary>
    /// 현재 선택한 카드를 보존 카드로 확정합니다.
    /// </summary>
    public void ConfirmPreserveCard()
    {
        if (
            isDrawAnimationPlaying ||
            isDiscardAnimationPlaying ||
            discardMoveTestCoroutine != null
        )
        {
            Debug.LogWarning(
                "[HandManager] 버림 연출이 이미 진행 중입니다."
            );

            return;
        }

        if (!isPreserveMode)
        {
            Debug.LogWarning(
                "[HandManager] 현재 보존 모드가 아닙니다."
            );

            return;
        }

        CardUI confirmedPreservedCardUI = selectedPreserveCardUI;

        RestoreFocusedPreserveCard();

        if (confirmedPreservedCardUI != null)
        {
            preservedCard =
                confirmedPreservedCardUI.GetCardData();

            Debug.Log(
                $"[HandManager] 보존 카드 확정: " +
                $"{preservedCard.cardName}"
            );
        }
        else
        {
            preservedCard = null;

            Debug.Log(
                "[HandManager] 보존 카드 없이 진행"
            );
        }

        isPreserveMode = false;
        selectedPreserveCardUI = null;
        SetPreserveDimActive(false);

        if (preserveConfirmButtonObject != null)
        {
            preserveConfirmButtonObject.SetActive(false);
        }

        List<CardUI> cardsToAnimate = new List<CardUI>();

        foreach (CardUI cardUI in handCardUIs)
        {
            if (
                cardUI == null ||
                cardUI == confirmedPreservedCardUI
            )
            {
                continue;
            }

            cardsToAnimate.Add(cardUI);
        }

        if (confirmedPreservedCardUI != null)
        {
            confirmedPreservedCardUI.enabled = false;
        }

        if (cardsToAnimate.Count == 0)
        {
            CompletePreserveConfirmation();
            return;
        }

        if (
            discardPileTarget == null ||
            discardTransformVfxPrefab == null ||
            discardArrivalVfxPrefab == null ||
            Camera.main == null
        )
        {
            Debug.LogError(
                "[HandManager] 버림 연출 설정이 누락되어 연출 없이 턴 종료 처리를 진행합니다."
            );

            CompletePreserveConfirmation();
            return;
        }

        isDiscardAnimationPlaying = true;
        discardMoveTestCoroutine = StartCoroutine(
            PlayConfirmedDiscardCoroutine(cardsToAnimate)
        );
    }

    /// <summary>
    /// 보존하지 않은 카드의 연출을 마친 뒤 실제 버림과 턴 전환을 처리합니다.
    /// </summary>
    private IEnumerator PlayConfirmedDiscardCoroutine(
        List<CardUI> cardsToAnimate
    )
    {
        yield return PlayDiscardCardsMoveCoroutine(
            cardsToAnimate,
            false
        );

        CompletePreserveConfirmation();
    }

    /// <summary>
    /// 기존 손패 데이터 규칙을 적용한 뒤 플레이어 턴을 종료합니다.
    /// </summary>
    private void CompletePreserveConfirmation()
    {
        discardMoveTestCoroutine = null;
        isDiscardAnimationPlaying = false;

        DiscardUnpreservedCards();

        ClearJinxedCard();

        if (endTurnButtonObject != null)
        {
            endTurnButtonObject.SetActive(true);
        }

        if (preserveConfirmButtonObject != null)
        {
            preserveConfirmButtonObject.SetActive(false);
        }

        if (turnManager != null)
        {
            turnManager.EndPlayerTurnAndStartNextTurn();
        }
        else
        {
            Debug.LogError(
                "[HandManager] TurnManager가 연결되지 않았습니다."
            );
        }
    }

    /// <summary>
    /// 보존 모드 전용 화면 암전 오브젝트의 표시 상태를 변경합니다.
    /// </summary>
    private void SetPreserveDimActive(bool isActive)
    {
        if (preserveDimOverlay != null)
        {
            preserveDimOverlay.SetActive(isActive);
        }
    }

    /// <summary>
    /// 보존으로 선택한 카드를 드로우 이동 시간에 맞춰 화면 중앙으로 이동시킵니다.
    /// </summary>
    private void FocusPreserveCard(CardUI cardUI)
    {
        if (cardUI == null || handCardParent == null)
        {
            return;
        }

        RectTransform cardRectTransform =
            cardUI.GetComponent<RectTransform>();
        RectTransform rootRectTransform =
            handCardParent.root as RectTransform;

        if (cardRectTransform == null || rootRectTransform == null)
        {
            return;
        }

        focusedPreserveCardRectTransform = cardRectTransform;
        focusedPreserveCardPosition = cardRectTransform.position;
        focusedPreserveCardRotation = cardRectTransform.localRotation;
        focusedPreserveCardScale = cardRectTransform.localScale;
        focusedPreserveCardSiblingIndex =
            cardRectTransform.GetSiblingIndex();

        cardRectTransform.SetAsLastSibling();

        preserveCardFocusCoroutine = StartCoroutine(
            AnimatePreserveCardToCenterCoroutine(
                cardRectTransform,
                rootRectTransform.position
            )
        );
    }

    /// <summary>
    /// 보존 선택 카드를 화면 중앙으로 이동하고 확대합니다.
    /// </summary>
    private IEnumerator AnimatePreserveCardToCenterCoroutine(
        RectTransform cardRectTransform,
        Vector3 targetPosition)
    {
        Vector3 startPosition = cardRectTransform.position;
        Quaternion startRotation = cardRectTransform.localRotation;
        Vector3 startScale = cardRectTransform.localScale;
        Vector3 targetScale =
            startScale * Mathf.Max(0f, preserveFocusedCardScale);
        float moveDuration = Mathf.Max(0f, drawMoveDuration);
        float elapsedTime = 0f;

        while (elapsedTime < moveDuration)
        {
            if (cardRectTransform == null)
            {
                preserveCardFocusCoroutine = null;
                yield break;
            }

            float progress = moveDuration > 0f
                ? Mathf.Clamp01(elapsedTime / moveDuration)
                : 1f;
            float easedProgress = progress * progress;

            cardRectTransform.position = Vector3.Lerp(
                startPosition,
                targetPosition,
                easedProgress
            );
            cardRectTransform.localRotation = Quaternion.Lerp(
                startRotation,
                Quaternion.identity,
                easedProgress
            );
            cardRectTransform.localScale = Vector3.Lerp(
                startScale,
                targetScale,
                easedProgress
            );

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (cardRectTransform != null)
        {
            cardRectTransform.position = targetPosition;
            cardRectTransform.localRotation = Quaternion.identity;
            cardRectTransform.localScale = targetScale;
        }

        preserveCardFocusCoroutine = null;
    }

    /// <summary>
    /// 중앙에 표시 중인 보존 선택 카드를 원래 손패 배치로 복원합니다.
    /// </summary>
    private void RestoreFocusedPreserveCard()
    {
        if (preserveCardFocusCoroutine != null)
        {
            StopCoroutine(preserveCardFocusCoroutine);
            preserveCardFocusCoroutine = null;
        }

        if (focusedPreserveCardRectTransform != null)
        {
            focusedPreserveCardRectTransform.position =
                focusedPreserveCardPosition;
            focusedPreserveCardRectTransform.localRotation =
                focusedPreserveCardRotation;
            focusedPreserveCardRectTransform.localScale =
                focusedPreserveCardScale;
            focusedPreserveCardRectTransform.SetSiblingIndex(
                focusedPreserveCardSiblingIndex
            );
        }

        focusedPreserveCardRectTransform = null;
    }

    /// <summary>
    /// 보존 카드를 제외한 손패의 모든 카드를 버림 더미로 이동합니다.
    /// 같은 CardData가 여러 장 있어도 1장만 보존합니다.
    /// </summary>
    public void DiscardUnpreservedCards()
    {
        if (deckManager == null)
        {
            Debug.LogError(
                "[HandManager] DeckManager가 연결되지 않았습니다."
            );

            return;
        }

        List<CardData> newHandCards =
            new List<CardData>();

        List<CardData> cardsToDiscard =
            new List<CardData>();

        bool preservedCardKept = false;

        for (int i = 0; i < handCards.Count; i++)
        {
            CardData card =
                handCards[i];

            if (!preservedCardKept &&
                preservedCard != null &&
                card == preservedCard)
            {
                newHandCards.Add(card);
                preservedCardKept = true;

                continue;
            }

            cardsToDiscard.Add(card);
        }

        for (int i = 0; i < cardsToDiscard.Count; i++)
        {
            deckManager.AddToDiscardPile(
                cardsToDiscard[i]
            );

            Debug.Log(
                $"[HandManager] 턴 종료 버림: " +
                $"{cardsToDiscard[i].cardName}"
            );
        }

        handCards.Clear();
        handCards.AddRange(newHandCards);

        RefreshHandUI();

        Debug.Log(
            $"[HandManager] 보존 처리 완료. " +
            $"현재 손패: {handCards.Count}장"
        );
    }

    /// <summary>
    /// 사용한 카드를 손패에서 제거하고,
    /// 소멸 카드가 아니라면 버림 더미로 이동합니다.
    /// </summary>
    public void DiscardUsedCard(CardData cardData)
    {
        if (cardData == null)
        {
            Debug.LogWarning(
                "[HandManager] 버릴 카드 데이터가 없습니다."
            );

            return;
        }

        if (deckManager == null)
        {
            Debug.LogError(
                "[HandManager] DeckManager가 연결되지 않았습니다."
            );

            return;
        }

        int removedIndex =
            handCards.IndexOf(cardData);

        if (removedIndex < 0)
        {
            Debug.LogWarning(
                $"[HandManager] 손패에 해당 카드가 없습니다: " +
                $"{cardData.cardName}"
            );

            return;
        }

        handCards.RemoveAt(removedIndex);

        /*
         * Jinx 카드보다 앞쪽 카드가 제거되면
         * Jinx 대상의 새 손패 인덱스를 한 칸 당깁니다.
         */
        if (jinxedHandIndex >= 0 &&
            removedIndex < jinxedHandIndex)
        {
            jinxedHandIndex--;
        }

        if (ShouldExhaustCard(cardData))
        {
            RefreshHandUI();

            Debug.Log(
                $"[HandManager] 소멸 카드 사용: " +
                $"{cardData.cardName} / 이번 전투에서 제외"
            );

            return;
        }

        deckManager.AddToDiscardPile(cardData);

        RefreshHandUI();

        Debug.Log(
            $"[HandManager] 사용한 카드 버림 더미 이동: " +
            $"{cardData.cardName}"
        );
    }

    /// <summary>
    /// 카드가 사용 후 소멸되는 카드인지 확인합니다.
    /// </summary>
    private bool ShouldExhaustCard(CardData cardData)
    {
        if (cardData == null ||
            cardData.effects == null)
        {
            return false;
        }

        for (int i = 0;
             i < cardData.effects.Count;
             i++)
        {
            if (cardData.effects[i].statusEffectType ==
                StatusEffectType.Exit)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 지정한 카드를 손패에서 제거합니다.
    /// </summary>
    public void RemoveCardFromHand(CardData cardData)
    {
        if (cardData == null)
        {
            Debug.LogWarning(
                "[HandManager] 제거할 카드 데이터가 없습니다."
            );

            return;
        }

        int removedIndex =
            handCards.IndexOf(cardData);

        if (removedIndex < 0)
        {
            Debug.LogWarning(
                $"[HandManager] 손패에 해당 카드가 없습니다: " +
                $"{cardData.cardName}"
            );

            return;
        }

        handCards.RemoveAt(removedIndex);

        if (jinxedHandIndex >= 0 &&
            removedIndex < jinxedHandIndex)
        {
            jinxedHandIndex--;
        }

        RefreshHandUI();

        Debug.Log(
            $"[HandManager] 손패에서 카드 제거: " +
            $"{cardData.cardName}"
        );
    }

    /// <summary>
    /// 손패 데이터와 손패 UI를 모두 제거합니다.
    /// </summary>
    public void ClearHand()
    {
        handCards.Clear();

        ClearHandUI();

        Debug.Log(
            "[HandManager] 손패 초기화 완료"
        );
    }

    /// <summary>
    /// 저장된 CurrentDeck 인덱스 목록을 사용해
    /// 이어하기 첫 손패를 복원합니다.
    ///
    /// 저장된 순서대로 손패에 배치합니다.
    /// </summary>
    /// <param name="currentDeck">
    /// 복원 완료된 현재 전체 덱
    /// </param>
    /// <param name="savedHandIndices">
    /// CurrentDeck 기준 첫 손패 카드 인덱스
    /// </param>
    /// <returns>첫 손패 복원 성공 여부</returns>
    public bool RestoreOpeningHand(
        List<CardData> currentDeck,
        List<int> savedHandIndices)
    {
        if (currentDeck == null ||
            currentDeck.Count <= 0)
        {
            Debug.LogError(
                "[HandManager] CurrentDeck이 비어 있어 " +
                "첫 손패를 복원할 수 없습니다."
            );

            return false;
        }

        if (savedHandIndices == null ||
            savedHandIndices.Count <= 0)
        {
            Debug.LogError(
                "[HandManager] 복원할 첫 손패 인덱스가 없습니다."
            );

            return false;
        }

        if (savedHandIndices.Count > 4)
        {
            Debug.LogError(
                $"[HandManager] 저장된 첫 손패가 4장을 초과합니다: " +
                $"{savedHandIndices.Count}장"
            );

            return false;
        }

        List<CardData> restoredHand =
            new List<CardData>();

        HashSet<int> usedIndices =
            new HashSet<int>();

        for (int i = 0;
             i < savedHandIndices.Count;
             i++)
        {
            int deckIndex =
                savedHandIndices[i];

            if (deckIndex < 0 ||
                deckIndex >= currentDeck.Count)
            {
                Debug.LogError(
                    $"[HandManager] 잘못된 첫 손패 카드 인덱스입니다: " +
                    $"{deckIndex}"
                );

                return false;
            }

            if (!usedIndices.Add(deckIndex))
            {
                Debug.LogError(
                    $"[HandManager] 첫 손패 카드 인덱스가 " +
                    $"중복되었습니다: {deckIndex}"
                );

                return false;
            }

            CardData card =
                currentDeck[deckIndex];

            if (card == null)
            {
                Debug.LogError(
                    $"[HandManager] CurrentDeck의 {deckIndex}번 카드가 null입니다."
                );

                return false;
            }

            restoredHand.Add(card);
        }

        ResetHandForNewBattle();

        handCards.AddRange(restoredHand);

        RefreshHandUI();

        Debug.Log(
            $"[HandManager] 이어하기 첫 손패 복원 완료: " +
            $"{handCards.Count}장"
        );

        return true;
    }

    /// <summary>
    /// 새로운 전투를 위해 손패 상태를 완전히 초기화합니다.
    /// </summary>
    public void ResetHandForNewBattle()
    {
        RestoreFocusedPreserveCard();

        handCards.Clear();
        preservedCard = null;
        selectedPreserveCardUI = null;
        isPreserveMode = false;
        jinxedHandIndex = -1;
        SetPreserveDimActive(false);

        ClearHandUI();

        if (endTurnButtonObject != null)
        {
            endTurnButtonObject.SetActive(true);
        }

        if (preserveConfirmButtonObject != null)
        {
            preserveConfirmButtonObject.SetActive(false);
        }

        Debug.Log(
            "[HandManager] 새 전투용 손패 완전 초기화"
        );
    }

    /// <summary>
    /// 현재 생성된 손패 UI를 제거하고
    /// CardUI 참조 목록도 초기화합니다.
    /// </summary>
    private void ClearHandUI()
    {
        handCardUIs.Clear();

        if (handCardParent == null)
        {
            return;
        }

        foreach (Transform child in handCardParent)
        {
            Destroy(child.gameObject);
        }
    }
}
