using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// 시작 덱 확인과 전투 중 카드 더미 확인 UI를 관리합니다.
///
/// 지원 화면:
/// - 게임 시작 시 시작 덱 확인
/// - 현재 전체 덱 확인
/// - 뽑을 패 더미 확인
/// - 버림 패 더미 확인
///
/// 시작 덱 확인이 완료되면
/// 첫 전투 시작 시점의 게임 진행을 자동 저장합니다.
/// </summary>
public class StartingDeckUI : MonoBehaviour
{
    /// <summary>
    /// 현재 카드 목록 패널이 표시 중인 화면 종류입니다.
    /// </summary>
    private enum DeckViewMode
    {
        None,
        StartingDeck,
        CurrentDeck,
        DrawPile,
        DiscardPile
    }

    [Header("Deck Manager")]
    [SerializeField]
    private DeckManager deckManager;

    [Header("Hand Manager")]
    [SerializeField]
    private HandManager handManager;

    [Header("Battle Manager")]
    [SerializeField]
    private BattleManager battleManager;

    [Header("덱 보기 패널")]
    [SerializeField]
    private GameObject startingDeckPanel;

    [Header("전투 UI 패널")]
    [SerializeField]
    private GameObject battlePanel;

    [Header("패널 제목")]
    [SerializeField]
    private TMP_Text titleText;

    [Header("카드가 생성될 부모")]
    [SerializeField]
    private Transform cardGridParent;

    [Header("카드 UI 프리팹")]
    [SerializeField]
    private CardUI cardPrefab;

    [Header("현재 카드 목록 화면")]
    [SerializeField]
    private DeckViewMode currentViewMode =
        DeckViewMode.None;

    /// <summary>
    /// 현재 카드 목록 패널이 열려 있는지 반환합니다.
    /// </summary>
    public bool IsDeckViewOpen =>
        startingDeckPanel != null &&
        startingDeckPanel.activeSelf;

    /// <summary>
    /// 현재 최초 시작 덱 확인 화면인지 반환합니다.
    /// </summary>
    public bool IsStartingDeckConfirmation =>
        currentViewMode == DeckViewMode.StartingDeck;

    /// <summary>
    /// 게임 시작 시 시작 덱을 표시합니다.
    /// 이 화면에서 확인 버튼을 누르면 전투 준비가 진행됩니다.
    /// </summary>
    public void ShowStartingDeck(
        List<CardData> deck)
    {
        currentViewMode =
            DeckViewMode.StartingDeck;

        OpenDeckPanel();

        SetTitle(
            $"시작 덱 ({GetCardCount(deck)}장)"
        );

        ShowDeckCards(deck);

        Debug.Log(
            "[StartingDeckUI] 시작 덱 보기"
        );
    }

    /// <summary>
    /// 플레이어가 현재 보유한 전체 덱을 표시합니다.
    /// </summary>
    public void ShowCurrentDeck()
    {
        if (IsRevelationSelectionPending())
        {
            return;
        }

        if (!ValidateDeckManager())
        {
            return;
        }

        currentViewMode =
            DeckViewMode.CurrentDeck;

        OpenDeckPanel();

        SetTitle(
            $"전체 덱 ({deckManager.CurrentDeck.Count}장)"
        );

        ShowDeckCards(
            deckManager.CurrentDeck
        );

        Debug.Log(
            "[StartingDeckUI] 현재 전체 덱 보기"
        );
    }

    /// <summary>
    /// 현재 뽑을 패 더미에 남아 있는 카드를 표시합니다.
    /// </summary>
    public void ShowDrawPile()
    {
        if (IsRevelationSelectionPending())
        {
            return;
        }

        if (!ValidateDeckManager())
        {
            return;
        }

        currentViewMode =
            DeckViewMode.DrawPile;

        OpenDeckPanel();

        SetTitle(
            $"뽑을 패 더미 ({deckManager.DrawPile.Count}장)"
        );

        ShowDeckCards(
            deckManager.DrawPile
        );

        Debug.Log(
            "[StartingDeckUI] 뽑을 패 더미 보기"
        );
    }

    /// <summary>
    /// 현재 버림 패 더미에 들어 있는 카드를 표시합니다.
    /// </summary>
    public void ShowDiscardPile()
    {
        if (IsRevelationSelectionPending())
        {
            return;
        }

        if (!ValidateDeckManager())
        {
            return;
        }

        currentViewMode =
            DeckViewMode.DiscardPile;

        OpenDeckPanel();

        SetTitle(
            $"버림 패 더미 ({deckManager.DiscardPile.Count}장)"
        );

        ShowDeckCards(
            deckManager.DiscardPile
        );

        Debug.Log(
            "[StartingDeckUI] 버림 패 더미 보기"
        );
    }

    /// <summary>
    /// 계시 선택이 완료되기 전에는 다른 전투 패널을 열지 않도록 확인합니다.
    /// </summary>
    private bool IsRevelationSelectionPending()
    {
        HRevelationPanelUI revelationPanel =
            FindFirstObjectByType<HRevelationPanelUI>();
        if (revelationPanel == null || !revelationPanel.IsPanelOpen)
        {
            return false;
        }

        Debug.LogWarning(
            "[StartingDeckUI] 계시를 먼저 선택해야 덱을 확인할 수 있습니다."
        );
        return true;
    }

    /// <summary>
    /// 확인 버튼 입력을 처리합니다.
    ///
    /// 시작 덱 화면:
    /// - 드로우 파일 생성
    /// - 첫 손패 4장 드로우
    /// - 전투 UI 표시
    /// - 첫 전투 시작 시점 자동 저장
    ///
    /// 전투 중 카드 목록 화면:
    /// - 카드 목록 패널 닫기
    /// </summary>
    public void ConfirmStartingDeck()
    {
        Debug.Log(
            "[StartingDeckUI] 확인 버튼 클릭됨"
        );

        if (currentViewMode !=
            DeckViewMode.StartingDeck)
        {
            CloseDeckView();
            return;
        }

        if (deckManager == null)
        {
            Debug.LogError(
                "[StartingDeckUI] DeckManager가 연결되지 않았습니다."
            );

            return;
        }

        if (handManager == null)
        {
            Debug.LogError(
                "[StartingDeckUI] HandManager가 연결되지 않았습니다."
            );

            return;
        }

        /*
         * 현재 전체 덱을 기준으로
         * 첫 전투용 드로우 파일을 만들고 섞습니다.
         */
        if (GameManager.Instance == null)
        {
            Debug.LogError(
                "[StartingDeckUI] GameManager.Instance가 없어 " +
                "튜토리얼 첫 손패를 준비하지 못했습니다."
            );
            return;
        }

        bool tutorialDrawPilePrepared =
            deckManager.PrepareTutorialDrawPile(
                GameManager.Instance.PlayerData.PlayerClass
            );

        if (!tutorialDrawPilePrepared)
        {
            Debug.LogError(
                "[StartingDeckUI] 튜토리얼 드로우 파일 준비에 실패했습니다."
            );
            return;
        }

        currentViewMode =
            DeckViewMode.None;

        ClearCards();

        if (startingDeckPanel != null)
        {
            startingDeckPanel.SetActive(false);
        }

        if (battlePanel != null)
        {
            battlePanel.SetActive(true);
        }

        /*
         * 비활성 패널에서 드로우 목적 좌표를 저장하면 Canvas가 활성화될 때
         * 월드 좌표가 달라질 수 있으므로, 전투 UI의 최종 좌표를 먼저 확정합니다.
         */
        Canvas.ForceUpdateCanvases();

        /*
         * 기존 손패가 남아 있을 가능성에 대비해 초기화한 뒤
         * 활성화된 전투 UI 기준으로 첫 손패 4장을 드로우합니다.
         */
        handManager.ClearHand();
        handManager.DrawCards(4);

        bool battleStartSucceeded =
            StartPreparedBattle();

        if (!battleStartSucceeded)
        {
            return;
        }

        /*
         * 클래스, 체력, 현재 스테이지,
         * 전투 진행도, 전체 덱과 강화 상태를 저장합니다.
         *
         * 손패와 드로우 순서는 저장하지 않으므로
         * 이어하기 시 현재 전투를 처음부터 다시 시작합니다.
         */
        SaveManager.Instance.CaptureBattleStartSnapshot();

        TutorialManager tutorialManager =
            FindFirstObjectByType<TutorialManager>();

        if (tutorialManager != null)
        {
            tutorialManager.BeginInitialTutorial();
        }
        else
        {
            Debug.LogWarning(
                "[StartingDeckUI] TutorialManager가 없어 " +
                "첫 전투 튜토리얼을 시작하지 못했습니다."
            );
        }

        Debug.Log(
            "[StartingDeckUI] 시작 덱 확인 완료"
        );
    }

    /// <summary>
    /// 이어하기로 BattleScene에 들어왔을 때
    /// 저장된 첫 손패와 드로우 파일 순서를 복원합니다.
    ///
    /// 처리 내용:
    /// - 저장된 CurrentDeck은 DeckManager에서 먼저 복원
    /// - 저장된 인덱스로 첫 손패 복원
    /// - 저장된 인덱스로 남은 드로우 파일 복원
    /// - 버림 더미 초기화
    /// - 시작 덱 확인창 생략
    /// - 전투 UI 표시
    /// - 저장된 전투를 처음부터 시작
    /// </summary>
    /// <returns>전투 준비 성공 여부</returns>
    public bool PrepareBattleAfterContinue()
    {
        if (deckManager == null)
        {
            Debug.LogError(
                "[StartingDeckUI] DeckManager가 연결되지 않았습니다."
            );

            return false;
        }

        if (handManager == null)
        {
            Debug.LogError(
                "[StartingDeckUI] HandManager가 연결되지 않았습니다."
            );

            return false;
        }

        if (!ContinueLoadContext.TryGetPendingSaveData(
                out GameSaveData saveData))
        {
            Debug.LogError(
                "[StartingDeckUI] 이어하기 저장 데이터를 가져오지 못했습니다."
            );

            return false;
        }

        if (deckManager.CurrentDeck == null ||
            deckManager.CurrentDeck.Count <= 0)
        {
            Debug.LogError(
                "[StartingDeckUI] 복원된 현재 덱이 비어 있습니다."
            );

            return false;
        }

        bool handRestoreSucceeded =
            handManager.RestoreOpeningHand(
                deckManager.CurrentDeck,
                saveData.openingHandCardIndices
            );

        if (!handRestoreSucceeded)
        {
            Debug.LogError(
                "[StartingDeckUI] 첫 손패 복원에 실패했습니다."
            );

            return false;
        }

        bool drawPileRestoreSucceeded =
            deckManager.RestoreDrawPile(
                saveData.remainingDrawPileCardIndices
            );

        if (!drawPileRestoreSucceeded)
        {
            Debug.LogError(
                "[StartingDeckUI] 드로우 파일 복원에 실패했습니다."
            );

            return false;
        }

        currentViewMode =
            DeckViewMode.None;

        ClearCards();

        if (startingDeckPanel != null)
        {
            startingDeckPanel.SetActive(false);
        }

        if (battlePanel != null)
        {
            battlePanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning(
                "[StartingDeckUI] BattlePanel이 연결되지 않았습니다."
            );
        }

        bool battleStartSucceeded =
            StartPreparedBattle();

        if (!battleStartSucceeded)
        {
            return false;
        }

        /*
         * 이어하기로 복원된 전투 시작 상태를
         * 다시 메모리 스냅샷으로 등록합니다.
         *
         * 이후 플레이어가 다시 저장 후 종료를 누르면
         * 같은 전투 시작 상태를 저장할 수 있습니다.
         */
        if (SaveManager.Instance != null)
        {
            bool captureSucceeded =
                SaveManager.Instance.CaptureBattleStartSnapshot();

            if (!captureSucceeded)
            {
                Debug.LogWarning(
                    "[StartingDeckUI] 이어하기 후 전투 시작 " +
                    "스냅샷 재생성에 실패했습니다."
                );
            }
        }

        Debug.Log(
            $"[StartingDeckUI] 이어하기 전투 준비 완료 / " +
            $"현재 덱: {deckManager.CurrentDeck.Count}장 / " +
            $"첫 손패: {handManager.HandCards.Count}장 / " +
            $"드로우 파일: {deckManager.DrawPile.Count}장"
        );

        return true;
    }

    /// <summary>
    /// 덱과 첫 손패 준비가 끝난 이후
    /// BattleManager에 첫 전투 시작을 요청합니다.
    /// </summary>
    /// <returns>전투 시작 성공 여부</returns>
    private bool StartPreparedBattle()
    {
        if (battleManager == null)
        {
            Debug.LogError(
                "[StartingDeckUI] BattleManager가 연결되지 않았습니다."
            );

            return false;
        }

        bool battleStartSucceeded =
            battleManager.StartInitialBattle();

        if (!battleStartSucceeded)
        {
            Debug.LogError(
                "[StartingDeckUI] 첫 전투 시작에 실패했습니다."
            );

            return false;
        }

        return true;
    }

    /// <summary>
    /// 현재 게임 진행을 이어하기 파일에 저장합니다.
    /// </summary>
    private void SaveCurrentProgress()
    {
        if (SaveManager.Instance == null)
        {
            Debug.LogWarning(
                "[StartingDeckUI] SaveManager.Instance가 없어 " +
                "첫 전투 자동 저장을 처리하지 못했습니다."
            );

            return;
        }

        bool saveSucceeded =
            SaveManager.Instance.SaveCurrentGame();

        if (!saveSucceeded)
        {
            Debug.LogWarning(
                "[StartingDeckUI] 첫 전투 시작 시점 자동 저장에 실패했습니다."
            );

            return;
        }

        Debug.Log(
            "[StartingDeckUI] 첫 전투 시작 시점 자동 저장 완료"
        );
    }

    /// <summary>
    /// 현재 열려 있는 카드 목록 패널을 닫습니다.
    /// 시작 덱 최초 확인 화면은 이 메서드로 닫지 않습니다.
    /// </summary>
    public void CloseDeckView()
    {
        if (currentViewMode ==
            DeckViewMode.StartingDeck)
        {
            Debug.LogWarning(
                "[StartingDeckUI] 시작 덱 화면은 " +
                "확인 버튼으로 진행해야 합니다."
            );

            return;
        }

        currentViewMode =
            DeckViewMode.None;

        ClearCards();

        if (startingDeckPanel != null)
        {
            startingDeckPanel.SetActive(false);
        }

        if (battlePanel != null)
        {
            battlePanel.SetActive(true);
        }

        Debug.Log(
            "[StartingDeckUI] 카드 목록 보기 닫기"
        );
    }

    /// <summary>
    /// 카드 목록 패널을 표시하고 전투 UI를 숨깁니다.
    /// </summary>
    private void OpenDeckPanel()
    {
        if (startingDeckPanel == null)
        {
            Debug.LogError(
                "[StartingDeckUI] StartingDeckPanel이 연결되지 않았습니다."
            );

            return;
        }

        startingDeckPanel.SetActive(true);

        if (battlePanel != null)
        {
            battlePanel.SetActive(false);
        }
    }

    /// <summary>
    /// 전달받은 카드 목록을 패널에 생성합니다.
    /// </summary>
    private void ShowDeckCards(
        List<CardData> deck)
    {
        ClearCards();

        if (deck == null)
        {
            Debug.LogWarning(
                "[StartingDeckUI] 표시할 카드 목록이 없습니다."
            );

            return;
        }

        if (cardGridParent == null)
        {
            Debug.LogError(
                "[StartingDeckUI] CardGridParent가 연결되지 않았습니다."
            );

            return;
        }

        if (cardPrefab == null)
        {
            Debug.LogError(
                "[StartingDeckUI] CardPrefab이 연결되지 않았습니다."
            );

            return;
        }

        for (int i = 0;
             i < deck.Count;
             i++)
        {
            CardData card =
                deck[i];

            if (card == null)
            {
                continue;
            }

            CardUI cardUI =
                Instantiate(
                    cardPrefab,
                    cardGridParent
                );

            cardUI.SetCard(card);
        }

        Debug.Log(
            $"[StartingDeckUI] 카드 목록 생성 완료: " +
            $"{deck.Count}장"
        );
    }

    /// <summary>
    /// 현재 생성된 카드 UI를 모두 제거합니다.
    /// </summary>
    private void ClearCards()
    {
        if (cardGridParent == null)
        {
            return;
        }

        foreach (Transform child
                 in cardGridParent)
        {
            Destroy(
                child.gameObject
            );
        }
    }

    /// <summary>
    /// 패널 제목을 변경합니다.
    /// </summary>
    private void SetTitle(
        string title)
    {
        if (titleText == null)
        {
            Debug.LogWarning(
                "[StartingDeckUI] TitleText가 연결되지 않았습니다."
            );

            return;
        }

        titleText.text =
            title;
    }

    /// <summary>
    /// DeckManager 연결 여부를 확인합니다.
    /// </summary>
    private bool ValidateDeckManager()
    {
        if (deckManager != null)
        {
            return true;
        }

        Debug.LogError(
            "[StartingDeckUI] DeckManager가 연결되지 않았습니다."
        );

        return false;
    }

    /// <summary>
    /// 카드 목록의 장수를 안전하게 반환합니다.
    /// </summary>
    private int GetCardCount(
        List<CardData> deck)
    {
        if (deck == null)
        {
            return 0;
        }

        return deck.Count;
    }

#if UNITY_EDITOR

    /// <summary>
    /// Inspector 참조가 비어 있으면
    /// 현재 씬에서 자동 탐색합니다.
    /// </summary>
    private void OnValidate()
    {
        if (deckManager == null)
        {
            deckManager =
                FindFirstObjectByType<DeckManager>();
        }

        if (handManager == null)
        {
            handManager =
                FindFirstObjectByType<HandManager>();
        }

        if (battleManager == null)
        {
            battleManager =
                FindFirstObjectByType<BattleManager>();
        }
    }

#endif
}
