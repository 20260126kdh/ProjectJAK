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

    [Header("보존 모드 여부")]
    [SerializeField]
    private bool isPreserveMode;

    [Header("이번 턴 Jinx 카드 인덱스")]
    [SerializeField]
    private int jinxedHandIndex = -1;

    [Header("손패 UI 부모")]
    [SerializeField]
    private Transform handCardParent;

    [Header("버림 더미 도착 위치")]
    [SerializeField]
    private RectTransform discardPileTarget;

    [Header("버림 도착 VFX")]
    [SerializeField]
    private ParticleSystem discardArrivalVfxPrefab;

    private ParticleSystem activeDiscardArrivalVfx;

    [Header("버림 카드 이동 테스트")]
    [SerializeField]
    private float discardMoveDuration = 1f;

    [SerializeField]
    private float discardArrivalHoldDuration = 0.2f;

    [SerializeField]
    private float discardCardMoveInterval = 0.5f;

    private Coroutine discardMoveTestCoroutine;

    [Header("카드 UI 프리팹")]
    [SerializeField]
    private CardUI cardPrefab;

    [Header("턴 종료 버튼 오브젝트")]
    [SerializeField]
    private GameObject endTurnButtonObject;

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
        List<CardUI> testCards = new List<CardUI>();

        foreach (CardUI cardUI in handCardUIs)
        {
            if (cardUI != null)
            {
                testCards.Add(cardUI);
            }
        }

        float moveInterval = Mathf.Max(0f, discardCardMoveInterval);

        for (int i = 0; i < testCards.Count; i++)
        {
            CardUI testCard = testCards[i];

            if (testCard == null)
            {
                continue;
            }

            Coroutine cardMoveCoroutine = StartCoroutine(
                PlayDiscardCardMoveTestCoroutine(testCard)
            );

            bool hasNextCard = i < testCards.Count - 1;

            if (hasNextCard && moveInterval > 0f)
            {
                yield return new WaitForSeconds(moveInterval);
            }
            else if (!hasNextCard)
            {
                yield return cardMoveCoroutine;
            }
        }

        discardMoveTestCoroutine = null;
    }

    /// <summary>
    /// 카드 UI의 현재 화면 위치를 기준으로 버림 더미까지 보간합니다.
    /// 테스트 종료 시 기존 UI 상태를 보존하기 위해 원래 Transform을 복원합니다.
    /// </summary>
    private IEnumerator PlayDiscardCardMoveTestCoroutine(CardUI testCard)
    {
        if (testCard == null)
        {
            yield break;
        }

        RectTransform cardRectTransform =
            testCard.GetComponent<RectTransform>();

        if (cardRectTransform == null)
        {
            Debug.LogError(
                "[HandManager] 이동 테스트 카드에 RectTransform이 없습니다."
            );

            yield break;
        }

        Vector3 startPosition = cardRectTransform.position;
        Quaternion startRotation = cardRectTransform.localRotation;
        Vector3 startScale = cardRectTransform.localScale;
        bool wasCardEnabled = testCard.enabled;

        testCard.enabled = false;

        float elapsedTime = 0f;
        float moveDuration = Mathf.Max(0f, discardMoveDuration);

        while (elapsedTime < moveDuration)
        {
            if (testCard == null)
            {
                yield break;
            }

            if (discardPileTarget == null)
            {
                cardRectTransform.position = startPosition;
                cardRectTransform.localRotation = startRotation;
                cardRectTransform.localScale = startScale;
                testCard.enabled = wasCardEnabled;

                yield break;
            }

            float progress = moveDuration > 0f
                ? Mathf.Clamp01(elapsedTime / moveDuration)
                : 1f;

            cardRectTransform.position = Vector3.Lerp(
                startPosition,
                discardPileTarget.position,
                progress
            );

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (testCard != null && discardPileTarget != null)
        {
            cardRectTransform.position = discardPileTarget.position;
            PlayDiscardArrivalVfx(false);
        }

        float holdDuration = Mathf.Max(0f, discardArrivalHoldDuration);

        if (holdDuration > 0f)
        {
            yield return new WaitForSeconds(holdDuration);
        }

        if (testCard != null)
        {
            cardRectTransform.position = startPosition;
            cardRectTransform.localRotation = startRotation;
            cardRectTransform.localScale = startScale;
            testCard.enabled = wasCardEnabled;
        }

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

        Debug.Log(
            $"[HandManager] 현재 손패: " +
            $"{handCards.Count}장"
        );
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
        isPreserveMode = true;
        selectedPreserveCardUI = null;

        if (endTurnButtonObject != null)
        {
            endTurnButtonObject.SetActive(false);
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
            selectedPreserveCardUI.SetDeselected();
            selectedPreserveCardUI = null;

            Debug.Log(
                "[HandManager] 보존 카드 선택 해제"
            );

            return;
        }

        if (selectedPreserveCardUI != null)
        {
            selectedPreserveCardUI.SetDeselected();
        }

        selectedPreserveCardUI = cardUI;
        selectedPreserveCardUI.SetSelected();

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
        if (!isPreserveMode)
        {
            Debug.LogWarning(
                "[HandManager] 현재 보존 모드가 아닙니다."
            );

            return;
        }

        if (selectedPreserveCardUI != null)
        {
            preservedCard =
                selectedPreserveCardUI.GetCardData();

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

        DiscardUnpreservedCards();

        ClearJinxedCard();

        if (endTurnButtonObject != null)
        {
            endTurnButtonObject.SetActive(true);
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
        handCards.Clear();
        preservedCard = null;
        selectedPreserveCardUI = null;
        isPreserveMode = false;
        jinxedHandIndex = -1;

        ClearHandUI();

        if (endTurnButtonObject != null)
        {
            endTurnButtonObject.SetActive(true);
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
