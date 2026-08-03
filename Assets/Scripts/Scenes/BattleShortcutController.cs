using UnityEngine;

/// <summary>
/// BattleScene의 전투 관련 키보드 단축키를 관리합니다.
///
/// 지원 단축키:
/// - 1~4: 손패 카드 선택 및 선택 해제
/// - E: 보존 모드 시작 또는 보존 확정
/// - D: 전체 덱 보기
/// - A: 뽑을 패 더미 보기
/// - S: 버림 패 더미 보기
///
/// Esc 입력은 이 클래스에서 처리하지 않습니다.
/// 덱 화면 닫기와 일시정지는 PauseManager가 전담합니다.
/// </summary>
public class BattleShortcutController : MonoBehaviour
{
    [Header("Hand Manager")]
    [SerializeField]
    private HandManager handManager;

    [Header("Turn Manager")]
    [SerializeField]
    private TurnManager turnManager;

    [Header("Starting Deck UI")]
    [SerializeField]
    private StartingDeckUI startingDeckUI;

    [Header("Pause Manager")]
    [SerializeField]
    private PauseManager pauseManager;

    /// <summary>
    /// 매 프레임 전투 단축키 입력을 확인합니다.
    /// </summary>
    private void Update()
    {
        if (handManager == null ||
            turnManager == null ||
            startingDeckUI == null ||
            pauseManager == null)
        {
            return;
        }

        /*
         * 일시정지 중에는 전투 관련 단축키를
         * 모두 처리하지 않습니다.
         *
         * Esc 입력은 PauseManager가 계속 감지하여
         * 일시정지를 해제할 수 있습니다.
         */
        if (pauseManager.IsPaused)
        {
            return;
        }

        /*
         * 최초 시작 덱 확인 화면에서는
         * 전투 단축키를 처리하지 않습니다.
         *
         * 기존 Confirm 버튼으로 전투를 시작합니다.
         */
        if (startingDeckUI.IsStartingDeckConfirmation)
        {
            return;
        }

        /*
         * 카드 목록 화면이 열려 있다면
         * D, A, S를 이용한 목록 전환만 처리합니다.
         *
         * Esc를 이용한 닫기는 PauseManager가 담당합니다.
         * 손패 선택과 턴 종료 입력은 차단합니다.
         */
        if (startingDeckUI.IsDeckViewOpen)
        {
            HandleOpenedDeckViewInput();
            return;
        }

        /*
         * 카드 목록 화면이 닫혀 있다면
         * D, A, S로 목록을 열 수 있습니다.
         */
        if (HandleDeckViewOpenInput())
        {
            return;
        }

        /*
         * 적 턴에는 손패 선택과
         * 턴 종료 입력을 처리하지 않습니다.
         */
        if (!turnManager.IsPlayerTurn)
        {
            return;
        }

        HandleCardSelectionInput();
        HandleEndTurnInput();
    }

    /// <summary>
    /// 카드 목록 화면이 닫혀 있을 때
    /// D, A, S 입력으로 각 카드 목록을 엽니다.
    ///
    /// 카드 목록을 열었다면 true를 반환합니다.
    /// </summary>
    private bool HandleDeckViewOpenInput()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            startingDeckUI.ShowCurrentDeck();
            return true;
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            startingDeckUI.ShowDrawPile();
            return true;
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            startingDeckUI.ShowDiscardPile();
            return true;
        }

        return false;
    }

    /// <summary>
    /// 카드 목록 화면이 열린 상태에서
    /// D, A, S 입력으로 표시 중인 카드 목록을 전환합니다.
    ///
    /// Esc 입력은 PauseManager가 처리합니다.
    /// </summary>
    private void HandleOpenedDeckViewInput()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            startingDeckUI.ShowCurrentDeck();
            return;
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            startingDeckUI.ShowDrawPile();
            return;
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            startingDeckUI.ShowDiscardPile();
        }
    }

    /// <summary>
    /// 숫자키 1~4로 손패 카드를 선택합니다.
    ///
    /// 키보드 상단 숫자키와 숫자패드를 모두 지원합니다.
    /// 같은 번호를 다시 누르면 기존 카드 선택 로직에 따라
    /// 선택이 해제됩니다.
    /// </summary>
    private void HandleCardSelectionInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) ||
            Input.GetKeyDown(KeyCode.Keypad1))
        {
            handManager.SelectCardByIndex(0);
            return;
        }

        if (Input.GetKeyDown(KeyCode.Alpha2) ||
            Input.GetKeyDown(KeyCode.Keypad2))
        {
            handManager.SelectCardByIndex(1);
            return;
        }

        if (Input.GetKeyDown(KeyCode.Alpha3) ||
            Input.GetKeyDown(KeyCode.Keypad3))
        {
            handManager.SelectCardByIndex(2);
            return;
        }

        if (Input.GetKeyDown(KeyCode.Alpha4) ||
            Input.GetKeyDown(KeyCode.Keypad4))
        {
            handManager.SelectCardByIndex(3);
        }
    }

    /// <summary>
    /// E 키 입력을 처리합니다.
    ///
    /// 일반 플레이어 턴:
    /// 보존 모드를 시작합니다.
    ///
    /// 보존 모드:
    /// 선택한 카드를 보존하고 다음 턴으로 진행합니다.
    /// 선택한 카드가 없다면 보존 없이 진행합니다.
    /// </summary>
    private void HandleEndTurnInput()
    {
        if (!Input.GetKeyDown(KeyCode.E))
        {
            return;
        }

        if (handManager.IsPreserveMode)
        {
            handManager.ConfirmPreserveCard();
            return;
        }

        handManager.StartPreserveMode();
    }

#if UNITY_EDITOR

    /// <summary>
    /// Inspector에서 참조가 비어 있다면
    /// 현재 씬에서 자동으로 탐색합니다.
    /// </summary>
    private void OnValidate()
    {
        if (handManager == null)
        {
            handManager =
                FindFirstObjectByType<HandManager>();
        }

        if (turnManager == null)
        {
            turnManager =
                FindFirstObjectByType<TurnManager>();
        }

        if (startingDeckUI == null)
        {
            startingDeckUI =
                FindFirstObjectByType<StartingDeckUI>();
        }

        if (pauseManager == null)
        {
            pauseManager =
                FindFirstObjectByType<PauseManager>();
        }
    }

#endif
}