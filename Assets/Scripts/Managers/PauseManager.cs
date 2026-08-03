using UnityEngine;

/// <summary>
/// BattleScene의 일시정지 상태와 Esc 입력을 관리합니다.
///
/// 담당 기능:
/// - 일반 전투 화면에서 Esc로 일시정지 열기
/// - 일시정지 화면에서 Esc로 게임 재개
/// - 카드 목록 화면에서 Esc로 카드 목록 닫기
/// - 환경 설정 화면에서 Esc로 일시정지 메뉴 복귀
/// - 주요 팝업이 열린 상태에서는 일시정지 열기 차단
/// - Time.timeScale 정지 및 복구
///
/// 실제 패널 활성 상태는 BattleUIManager를 통해 확인합니다.
/// </summary>
public class PauseManager : MonoBehaviour
{
    [Header("Battle UI Manager")]
    [SerializeField]
    private BattleUIManager battleUIManager;

    [Header("Starting Deck UI")]
    [SerializeField]
    private StartingDeckUI startingDeckUI;

    /// <summary>
    /// 현재 게임이 일시정지 상태인지 저장합니다.
    /// 환경 설정 화면이 열려 있어도 true를 유지합니다.
    /// </summary>
    private bool isPaused;

    /// <summary>
    /// 현재 게임이 일시정지 상태인지 반환합니다.
    /// </summary>
    public bool IsPaused => isPaused;

    /// <summary>
    /// 시작 시 시간 배율과 일시정지 상태를 초기화합니다.
    /// </summary>
    private void Awake()
    {
        isPaused = false;
        Time.timeScale = 1f;
    }

    /// <summary>
    /// BattleUIManager 초기화 이후
    /// Pause와 Settings 패널을 닫습니다.
    /// </summary>
    private void Start()
    {
        if (battleUIManager == null)
        {
            Debug.LogError(
                "[PauseManager] BattleUIManager가 연결되지 않았습니다."
            );

            return;
        }

        battleUIManager.ClosePanel(
            BattleUIPanelType.Pause
        );

        /*
         * Settings 패널이 등록되어 있을 때만 닫습니다.
         * 아직 Inspector 등록 전에는 미등록 경고가 출력될 수 있으므로
         * Unity 설정까지 완료한 뒤 테스트합니다.
         */
        battleUIManager.ClosePanel(
            BattleUIPanelType.Settings
        );
    }

    /// <summary>
    /// 매 프레임 Esc 입력을 확인합니다.
    /// </summary>
    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape))
        {
            return;
        }

        HandleEscapeInput();
    }

    /// <summary>
    /// Esc 입력을 현재 UI 상태에 맞게 처리합니다.
    ///
    /// 우선순위:
    /// 1. 환경 설정 화면 → Pause 메뉴 복귀
    /// 2. Pause 메뉴 → 게임 재개
    /// 3. 덱 보기 화면 → 덱 보기 닫기
    /// 4. 다른 주요 팝업 → 입력 무시
    /// 5. 일반 전투 → Pause 열기
    /// </summary>
    private void HandleEscapeInput()
    {
        if (battleUIManager != null &&
            battleUIManager.IsPanelOpen(
                BattleUIPanelType.Settings
            ))
        {
            ReturnToPauseMenu();
            return;
        }

        if (isPaused)
        {
            ResumeGame();
            return;
        }

        if (startingDeckUI != null &&
            startingDeckUI.IsDeckViewOpen)
        {
            if (!startingDeckUI.IsStartingDeckConfirmation)
            {
                startingDeckUI.CloseDeckView();
            }

            return;
        }

        if (!CanOpenPause())
        {
            return;
        }

        PauseGame();
    }

    /// <summary>
    /// 일시정지 패널을 열고 게임 시간을 정지합니다.
    /// </summary>
    public void PauseGame()
    {
        if (isPaused)
        {
            return;
        }

        if (!CanOpenPause())
        {
            return;
        }

        if (battleUIManager == null)
        {
            Debug.LogError(
                "[PauseManager] BattleUIManager가 없어 " +
                "일시정지를 열 수 없습니다."
            );

            return;
        }

        isPaused = true;

        battleUIManager.OpenPanel(
            BattleUIPanelType.Pause
        );

        Time.timeScale = 0f;

        Debug.Log(
            "[PauseManager] 게임 일시정지"
        );
    }

    /// <summary>
    /// 일시정지와 환경 설정 패널을 닫고
    /// 게임 시간을 정상 속도로 복구합니다.
    /// </summary>
    public void ResumeGame()
    {
        if (!isPaused)
        {
            return;
        }

        isPaused = false;
        Time.timeScale = 1f;

        if (battleUIManager != null)
        {
            battleUIManager.ClosePanel(
                BattleUIPanelType.Settings
            );

            battleUIManager.ClosePanel(
                BattleUIPanelType.Pause
            );
        }
        else
        {
            Debug.LogError(
                "[PauseManager] BattleUIManager가 없어 " +
                "일시정지 UI를 닫지 못했습니다."
            );
        }

        Debug.Log(
            "[PauseManager] 게임 계속하기"
        );
    }

    /// <summary>
    /// Pause 메뉴에서 환경 설정 패널로 이동합니다.
    ///
    /// 게임의 일시정지 상태와 Time.timeScale은 유지합니다.
    /// </summary>
    public void OpenSettings()
    {
        if (!isPaused)
        {
            Debug.LogWarning(
                "[PauseManager] 일시정지 상태가 아니므로 " +
                "환경 설정을 열 수 없습니다."
            );

            return;
        }

        if (battleUIManager == null)
        {
            Debug.LogError(
                "[PauseManager] BattleUIManager가 연결되지 않았습니다."
            );

            return;
        }

        battleUIManager.ClosePanel(
            BattleUIPanelType.Pause
        );

        battleUIManager.OpenPanel(
            BattleUIPanelType.Settings
        );

        Debug.Log(
            "[PauseManager] 환경 설정 열기"
        );
    }

    /// <summary>
    /// 환경 설정 패널을 닫고 Pause 메뉴로 돌아갑니다.
    ///
    /// 뒤로 버튼과 Settings 화면의 Esc 입력에서 사용합니다.
    /// 게임은 계속 일시정지 상태를 유지합니다.
    /// </summary>
    public void ReturnToPauseMenu()
    {
        if (!isPaused)
        {
            return;
        }

        if (battleUIManager == null)
        {
            Debug.LogError(
                "[PauseManager] BattleUIManager가 연결되지 않았습니다."
            );

            return;
        }

        battleUIManager.ClosePanel(
            BattleUIPanelType.Settings
        );

        battleUIManager.OpenPanel(
            BattleUIPanelType.Pause
        );

        Time.timeScale = 0f;

        Debug.Log(
            "[PauseManager] 환경 설정 닫기 - Pause 메뉴 복귀"
        );
    }

    /// <summary>
    /// 현재 일시정지 패널을 열 수 있는 상태인지 확인합니다.
    /// </summary>
    private bool CanOpenPause()
    {
        if (battleUIManager == null)
        {
            Debug.LogError(
                "[PauseManager] BattleUIManager가 연결되지 않았습니다."
            );

            return false;
        }

        if (startingDeckUI != null &&
            startingDeckUI.IsStartingDeckConfirmation)
        {
            Debug.Log(
                "[PauseManager] 시작 덱 확인 중에는 " +
                "일시정지를 열지 않습니다."
            );

            return false;
        }

        if (battleUIManager.IsPanelOpen(
            BattleUIPanelType.Reward))
        {
            Debug.Log(
                "[PauseManager] Reward 패널이 열려 있어 " +
                "일시정지를 열지 않습니다."
            );

            return false;
        }

        if (battleUIManager.IsPanelOpen(
            BattleUIPanelType.Rest))
        {
            Debug.Log(
                "[PauseManager] Rest 패널이 열려 있어 " +
                "일시정지를 열지 않습니다."
            );

            return false;
        }

        if (battleUIManager.IsPanelOpen(
            BattleUIPanelType.Upgrade))
        {
            Debug.Log(
                "[PauseManager] Upgrade 패널이 열려 있어 " +
                "일시정지를 열지 않습니다."
            );

            return false;
        }

        if (battleUIManager.IsPanelOpen(
            BattleUIPanelType.Revelation))
        {
            Debug.Log(
                "[PauseManager] Revelation 패널이 열려 있어 " +
                "일시정지를 열지 않습니다."
            );

            return false;
        }

        return true;
    }

    /// <summary>
    /// 전투 포기 버튼에서 호출합니다.
    /// 실제 기능은 이후 구현합니다.
    /// </summary>
    public void SurrenderBattle()
    {
        Debug.Log(
            "[PauseManager] 전투 포기 기능은 아직 연결되지 않았습니다."
        );
    }

    /// <summary>
    /// 저장 후 종료 버튼에서 호출합니다.
    /// 저장 시스템 구현 후 실제 기능을 연결합니다.
    /// </summary>
    public void SaveAndQuit()
    {
        Debug.Log(
            "[PauseManager] 저장 후 종료 기능은 아직 연결되지 않았습니다."
        );
    }

    /// <summary>
    /// 오브젝트가 비활성화될 때 시간 배율을 복구합니다.
    /// </summary>
    private void OnDisable()
    {
        RestoreTimeScale();
    }

    /// <summary>
    /// 오브젝트가 파괴되거나 씬이 변경될 때
    /// 시간 배율을 복구합니다.
    /// </summary>
    private void OnDestroy()
    {
        RestoreTimeScale();
    }

    /// <summary>
    /// 게임 시간과 일시정지 상태를 정상으로 복구합니다.
    /// </summary>
    private void RestoreTimeScale()
    {
        Time.timeScale = 1f;
        isPaused = false;
    }

#if UNITY_EDITOR

    /// <summary>
    /// Inspector 참조가 비어 있으면
    /// 현재 씬에서 자동 탐색합니다.
    /// </summary>
    private void OnValidate()
    {
        if (battleUIManager == null)
        {
            battleUIManager =
                FindFirstObjectByType<BattleUIManager>();
        }

        if (startingDeckUI == null)
        {
            startingDeckUI =
                FindFirstObjectByType<StartingDeckUI>();
        }
    }

#endif
}