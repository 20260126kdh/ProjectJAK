using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// BattleScene의 일시정지 상태와 Esc 입력을 관리합니다.
///
/// 담당 기능:
/// - 일반 전투에서 일시정지 열기 및 닫기
/// - 카드 목록 화면 닫기
/// - 환경 설정 화면 전환
/// - 전투 포기 확인 화면 전환
/// - 전투 포기 후 진행 데이터 초기화 및 타이틀 이동
/// - Time.timeScale 정지 및 복구
/// </summary>
public class PauseManager : MonoBehaviour
{
    [Header("Scene 설정")]
    [Tooltip("전투 포기 후 이동할 타이틀 씬 이름입니다.")]
    [SerializeField]
    private string titleSceneName = "TitleScene";

    [Header("Battle UI Manager")]
    [SerializeField]
    private BattleUIManager battleUIManager;

    [Header("Starting Deck UI")]
    [SerializeField]
    private StartingDeckUI startingDeckUI;

    [Header("Hand Manager")]
    [SerializeField]
    private HandManager handManager;

    /// <summary>
    /// 현재 게임이 일시정지 상태인지 저장합니다.
    /// 설정 및 전투 포기 확인 화면에서도 true를 유지합니다.
    /// </summary>
    private bool isPaused;

    /// <summary>
    /// 현재 게임이 일시정지 상태인지 반환합니다.
    /// </summary>
    public bool IsPaused => isPaused;

    /// <summary>
    /// 시간 배율과 일시정지 상태를 초기화합니다.
    /// </summary>
    private void Awake()
    {
        isPaused = false;
        Time.timeScale = 1f;
    }

    /// <summary>
    /// BattleUIManager 초기화 후
    /// 일시정지 관련 패널을 닫습니다.
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

        battleUIManager.ClosePanel(
            BattleUIPanelType.Settings
        );

        battleUIManager.ClosePanel(
            BattleUIPanelType.SurrenderConfirm
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
    /// Esc 입력을 현재 UI 상태에 맞춰 처리합니다.
    ///
    /// 우선순위:
    /// 1. 전투 포기 확인 화면 → Pause 메뉴 복귀
    /// 2. 환경 설정 화면 → Pause 메뉴 복귀
    /// 3. Pause 메뉴 → 게임 재개
    /// 4. 덱 보기 화면 → 덱 보기 닫기
    /// 5. 보존 모드 → 보존 취소
    /// 6. 다른 주요 패널 → 입력 무시
    /// 7. 일반 전투 → Pause 열기
    /// </summary>
    private void HandleEscapeInput()
    {
        if (battleUIManager != null &&
            battleUIManager.IsPanelOpen(
                BattleUIPanelType.SurrenderConfirm
            ))
        {
            CancelSurrender();
            return;
        }

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

        if (handManager != null &&
            handManager.IsPreserveMode)
        {
            handManager.CancelPreserveMode();
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
    /// 일시정지 관련 패널을 닫고 게임을 재개합니다.
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
                BattleUIPanelType.SurrenderConfirm
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
    /// 게임은 계속 정지 상태를 유지합니다.
    /// </summary>
    public void OpenSettings()
    {
        if (!ValidatePausedUIState())
        {
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

        battleUIManager.ClosePanel(
            BattleUIPanelType.SurrenderConfirm
        );

        battleUIManager.OpenPanel(
            BattleUIPanelType.Pause
        );

        Time.timeScale = 0f;

        Debug.Log(
            "[PauseManager] Pause 메뉴 복귀"
        );
    }

    /// <summary>
    /// Pause 메뉴에서 전투 포기 확인 패널을 엽니다.
    ///
    /// PausePanel은 숨기고,
    /// 게임은 계속 정지 상태를 유지합니다.
    /// </summary>
    public void OpenSurrenderConfirm()
    {
        if (!ValidatePausedUIState())
        {
            return;
        }

        battleUIManager.ClosePanel(
            BattleUIPanelType.Pause
        );

        battleUIManager.OpenPanel(
            BattleUIPanelType.SurrenderConfirm
        );

        Time.timeScale = 0f;

        Debug.Log(
            "[PauseManager] 전투 포기 확인 창 열기"
        );
    }

    /// <summary>
    /// 전투 포기를 취소하고 Pause 메뉴로 돌아갑니다.
    ///
    /// 아니오 버튼과 Esc 입력에서 공통으로 사용합니다.
    /// </summary>
    public void CancelSurrender()
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
            BattleUIPanelType.SurrenderConfirm
        );

        battleUIManager.OpenPanel(
            BattleUIPanelType.Pause
        );

        Time.timeScale = 0f;

        Debug.Log(
            "[PauseManager] 전투 포기 취소"
        );
    }

    /// <summary>
    /// 현재 전투와 게임 진행을 포기하고
    /// 모든 진행 데이터를 초기화한 뒤 타이틀 씬으로 이동합니다.
    ///
    /// 예 버튼에서 호출합니다.
    /// </summary>
    public void ConfirmSurrender()
    {
        if (!isPaused)
        {
            Debug.LogWarning(
                "[PauseManager] 일시정지 상태가 아니므로 " +
                "전투 포기를 처리하지 않습니다."
            );

            return;
        }

        if (string.IsNullOrWhiteSpace(titleSceneName))
        {
            Debug.LogError(
                "[PauseManager] Title Scene Name이 비어 있습니다."
            );

            return;
        }

        /*
         * 씬을 이동하기 전에 반드시 시간 배율을 복구합니다.
         * 그렇지 않으면 타이틀 씬도 Time.timeScale 0 상태로 시작합니다.
         */
        Time.timeScale = 1f;
        isPaused = false;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.InitializeGame();
        }
        else
        {
            Debug.LogWarning(
                "[PauseManager] GameManager.Instance가 없어 " +
                "플레이어 데이터를 초기화하지 못했습니다."
            );
        }

        if (StageManager.Instance != null)
        {
            StageManager.Instance.ResetProgress();
        }
        else
        {
            Debug.LogWarning(
                "[PauseManager] StageManager.Instance가 없어 " +
                "스테이지 진행을 초기화하지 못했습니다."
            );
        }

        if (SFXManager.Instance != null)
        {
            SFXManager.Instance.StopAllSFX();
        }

        /*
        * 전투 포기는 이어하기를 남기지 않습니다.
        * 메모리 스냅샷과 저장 파일을 모두 제거합니다.
        */
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.ClearCapturedBattleSnapshot();
            SaveManager.Instance.DeleteSaveData();
        }

        Debug.Log(
            "[PauseManager] 전투 포기 - 진행 초기화 후 타이틀 이동"
        );

        SceneManager.LoadScene(
            titleSceneName
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
    /// Pause 메뉴의 하위 화면을 열 수 있는 상태인지 확인합니다.
    /// </summary>
    private bool ValidatePausedUIState()
    {
        if (!isPaused)
        {
            Debug.LogWarning(
                "[PauseManager] 현재 일시정지 상태가 아닙니다."
            );

            return false;
        }

        if (battleUIManager == null)
        {
            Debug.LogError(
                "[PauseManager] BattleUIManager가 연결되지 않았습니다."
            );

            return false;
        }

        return true;
    }

    /// <summary>
    /// 현재 전투 시작 스냅샷을 저장한 뒤
    /// 타이틀 화면으로 이동합니다.
    /// </summary>
    public void SaveAndQuit()
    {
        if (!isPaused)
        {
            Debug.LogWarning(
                "[PauseManager] 일시정지 상태가 아닙니다."
            );

            return;
        }

        if (SaveManager.Instance == null)
        {
            Debug.LogError(
                "[PauseManager] SaveManager가 없습니다."
            );

            return;
        }

        bool saveSucceeded =
            SaveManager.Instance.SaveCapturedBattleSnapshot();

        if (!saveSucceeded)
        {
            Debug.LogError(
                "[PauseManager] 저장 후 종료에 실패했습니다."
            );

            return;
        }

        Time.timeScale = 1f;
        isPaused = false;

        if (SFXManager.Instance != null)
        {
            SFXManager.Instance.StopAllSFX();
        }

        Debug.Log(
            "[PauseManager] 저장 후 종료"
        );

        SceneManager.LoadScene(
            titleSceneName
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
    /// 오브젝트가 파괴될 때 시간 배율을 복구합니다.
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
    /// 현재 씬에서 자동으로 탐색합니다.
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

        if (handManager == null)
        {
            handManager =
                FindFirstObjectByType<HandManager>();
        }
    }

#endif
}
