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
    /// 이후 단축키 입력 차단 및 Esc 닫기에 사용합니다.
    /// </summary>
    public bool IsDeckViewOpen =>
        startingDeckPanel != null &&
        startingDeckPanel.activeSelf;

    /// <summary>
    /// 현재 최초 시작 덱 확인 화면인지 반환합니다.
    /// 최초 시작 덱 화면에서는 전투 중 단축키를 사용하지 않습니다.
    /// </summary>
    public bool IsStartingDeckConfirmation =>
        currentViewMode == DeckViewMode.StartingDeck;

    /// <summary>
    /// 게임 시작 시 시작 덱을 표시합니다.
    /// 이 화면에서 확인 버튼을 누르면 전투 준비가 진행됩니다.
    /// </summary>
    public void ShowStartingDeck(List<CardData> deck)
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
    /// 현재 뽑을 패 더미에 남아 있는 카드들을 표시합니다.
    /// 현재 리스트 순서대로 표시되며,
    /// 첫 번째 카드가 다음에 뽑힐 카드입니다.
    /// </summary>
    public void ShowDrawPile()
    {
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
    /// 현재 버림 패 더미에 들어 있는 카드들을 표시합니다.
    /// </summary>
    public void ShowDiscardPile()
    {
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
    /// 확인 버튼 입력을 처리합니다.
    ///
    /// 시작 덱 화면에서는 전투를 준비하고,
    /// 전투 중 카드 목록 화면에서는 패널만 닫습니다.
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
         * 시작 덱을 기준으로 뽑을 패를 만들고 섞습니다.
         */
        deckManager.PrepareDrawPileForBattle();

        /*
         * 혹시 기존 손패가 남아 있다면 제거한 뒤
         * 전투 시작 손패 4장을 드로우합니다.
         */
        handManager.ClearHand();
        handManager.DrawCards(4);

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
            "[StartingDeckUI] 시작 덱 확인 완료"
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

        for (int i = 0; i < deck.Count; i++)
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

        foreach (Transform child in cardGridParent)
        {
            Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// 패널 제목을 변경합니다.
    /// </summary>
    private void SetTitle(string title)
    {
        if (titleText == null)
        {
            Debug.LogWarning(
                "[StartingDeckUI] TitleText가 연결되지 않았습니다."
            );

            return;
        }

        titleText.text = title;
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
    /// Inspector에서 참조가 비어 있으면
    /// 현재 씬에서 자동으로 탐색합니다.
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
    }

#endif
}