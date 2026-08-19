using UnityEngine;

/// <summary>
/// 스테이지의 진행 상태를 관리하는 클래스입니다.
///
/// 관리 내용
/// - 현재 스테이지 번호
/// - 일반 전투 횟수 관리
/// - 휴식 진입
/// - 보스 전투 진행
/// - Stage 3 연속 보스 진행
/// - 게임 클리어 판정
///
/// 씬 이동이나 UI 표시는 직접 담당하지 않습니다.
/// </summary>
public class StageManager : MonoBehaviour
{
    #region Singleton

    public static StageManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #endregion

    #region Inspector

    [Header("Stage Settings")]

    [Tooltip("현재 스테이지")]
    [SerializeField]
    private int currentStage = 1;

    [Tooltip("최대 스테이지")]
    [SerializeField]
    private int maxStage = 3;

    [Space]

    [Tooltip("현재 일반 전투 완료 횟수")]
    [SerializeField]
    private int currentBattleCount = 0;

    [Tooltip("스테이지당 일반 전투 횟수")]
    [SerializeField]
    private int maxBattleCount = 5;

    [Space]

    [Tooltip("현재 진행 단계")]
    [SerializeField]
    private StagePhase currentPhase =
        StagePhase.NormalBattle;

    [Header("Stage 3 Boss Sequence")]

    [Tooltip(
        "Stage 3 보스 진행 순서입니다.\n" +
        "0: 모르바엘\n" +
        "1: 아리엘"
    )]
    [SerializeField]
    private int currentBossSequence = 0;

    [Header("Selected Boss")]

    [Tooltip("Stage 1, 2에서 선택된 보스 Battle ID")]
    [SerializeField]
    private string selectedBossBattleID;

    [Header("Game Clear")]

    [Tooltip("최종 보스 처치 여부")]
    [SerializeField]
    private bool isGameClear;

    [Header("Debug")]

    [SerializeField]
    private bool debugMode = true;

    #endregion

    #region Property

    /// <summary>
    /// Stage 1, 2에서 선택된 보스 Battle ID입니다.
    /// </summary>
    public string SelectedBossBattleID =>
        selectedBossBattleID;

    public int CurrentStage => currentStage;

    /// <summary>
    /// 현재 게임에서 진행할 수 있는 최대 스테이지입니다.
    /// </summary>
    public int MaxStage => maxStage;

    public int CurrentBattleCount =>
        currentBattleCount;

    public StagePhase CurrentPhase =>
        currentPhase;

    /// <summary>
    /// Stage 3 보스 진행 순서입니다.
    ///
    /// 0: 모르바엘
    /// 1: 아리엘
    /// </summary>
    public int CurrentBossSequence =>
        currentBossSequence;

    public bool IsGameClear =>
        isGameClear;

    /// <summary>
    /// 현재 전투가 Stage 3 모르바엘 전투인지 반환합니다.
    /// </summary>
    public bool IsStage3MorvaelBattle =>
        currentStage == 3 &&
        currentPhase == StagePhase.BossBattle &&
        currentBossSequence == 0;

    /// <summary>
    /// 현재 전투가 Stage 3 아리엘 전투인지 반환합니다.
    /// </summary>
    public bool IsStage3ArielBattle =>
        currentStage == 3 &&
        currentPhase == StagePhase.BossBattle &&
        currentBossSequence == 1;

    #endregion

    #region Public

    /// <summary>
    /// 게임 시작 시 모든 진행 데이터를 초기화합니다.
    /// </summary>
    public void StartGame()
    {
        ResetProgress();

        PrintState("게임 시작");

        if (BGMManager.Instance != null)
        {
            BGMManager.Instance.RefreshBGM();
        }
    }

    /// <summary>
    /// 저장 데이터에서 스테이지 진행 상태를 복원합니다.
    ///
    /// 저장된 스테이지, 전투 횟수, 진행 단계,
    /// 보스 순서, 게임 클리어 상태를 그대로 적용합니다.
    /// 씬 이동과 전투 시작은 처리하지 않습니다.
    /// </summary>
    /// <param name="savedStage">복원할 현재 스테이지</param>
    /// <param name="savedBattleCount">복원할 일반 전투 완료 횟수</param>
    /// <param name="savedPhase">복원할 진행 단계</param>
    /// <param name="savedBossSequence">복원할 Stage 3 보스 순서</param>
    /// <param name="savedIsGameClear">복원할 게임 클리어 여부</param>
    /// <returns>복원 성공 여부</returns>
    public bool RestoreProgress(
        int savedStage,
        int savedBattleCount,
        StagePhase savedPhase,
        int savedBossSequence,
        string savedBossBattleID,
        bool savedIsGameClear)
    {
        if (savedStage < 1 ||
            savedStage > maxStage)
        {
            Debug.LogError(
                $"[StageManager] 복원할 스테이지가 올바르지 않습니다: " +
                $"{savedStage}"
            );

            return false;
        }

        if (savedBattleCount < 0 ||
            savedBattleCount > maxBattleCount)
        {
            Debug.LogError(
                $"[StageManager] 복원할 전투 횟수가 올바르지 않습니다: " +
                $"{savedBattleCount}"
            );

            return false;
        }

        if (!System.Enum.IsDefined(
                typeof(StagePhase),
                savedPhase))
        {
            Debug.LogError(
                $"[StageManager] 복원할 StagePhase가 올바르지 않습니다: " +
                $"{savedPhase}"
            );

            return false;
        }

        if (savedBossSequence < 0 ||
            savedBossSequence > 1)
        {
            Debug.LogError(
                $"[StageManager] 복원할 보스 순서가 올바르지 않습니다: " +
                $"{savedBossSequence}"
            );

            return false;
        }

        /*
         * 일반 전투 단계에서는 완료 전투 횟수가
         * 스테이지 최대 전투 횟수보다 작아야 합니다.
         *
         * 최대 횟수에 도달하면 Rest 단계여야 합니다.
         */
        if (savedPhase == StagePhase.NormalBattle &&
            savedBattleCount >= maxBattleCount)
        {
            Debug.LogError(
                "[StageManager] 일반 전투 단계인데 " +
                "일반 전투 완료 횟수가 최대치 이상입니다."
            );

            return false;
        }

        /*
         * Stage 1, 2에서는 보스 순서 0만 사용합니다.
         */
        if (savedStage < 3 &&
            savedBossSequence != 0)
        {
            Debug.LogError(
                $"[StageManager] Stage {savedStage}에서는 " +
                $"Boss Sequence {savedBossSequence}을 사용할 수 없습니다."
            );

            return false;
        }

        /*
         * 아리엘 진행 상태는
         * Stage 3 보스 전투에서만 사용할 수 있습니다.
         */
        if (savedBossSequence == 1 &&
            (savedStage != 3 ||
             savedPhase != StagePhase.BossBattle))
        {
            Debug.LogError(
                "[StageManager] 아리엘 보스 순서는 " +
                "Stage 3 보스 전투에서만 복원할 수 있습니다."
            );

            return false;
        }

        currentStage =
            savedStage;

        currentBattleCount =
            savedBattleCount;

        currentPhase =
            savedPhase;

        currentBossSequence =
            savedBossSequence;

        selectedBossBattleID =
            savedBossBattleID ?? string.Empty;

        isGameClear =
            savedIsGameClear;

        PrintState(
            "저장 진행도 복원"
        );

        if (BGMManager.Instance != null)
        {
            BGMManager.Instance.RefreshBGM();
        }

        return true;
    }

    /// <summary>
    /// 현재 게임의 스테이지 진행 데이터를
    /// 최초 상태로 초기화합니다.
    ///
    /// 씬 이동이나 BGM 변경은 처리하지 않습니다.
    /// 새 게임 시작, 전투 포기, 게임 오버 초기화에서 공통으로 사용합니다.
    /// </summary>
    public void ResetProgress()
    {
        currentStage = 1;
        currentBattleCount = 0;
        currentPhase = StagePhase.NormalBattle;

        selectedBossBattleID =
            string.Empty;

        currentBossSequence = 0;
        isGameClear = false;

        PrintState("진행 데이터 초기화");
    }

    /// <summary>
    /// 일반 전투 승리를 처리합니다.
    /// </summary>
    public void BattleWin()
    {
        if (currentPhase != StagePhase.NormalBattle)
        {
            Debug.LogWarning(
                "[StageManager] 현재 일반 전투 상태가 아닙니다."
            );

            return;
        }

        currentBattleCount++;

        /*
         * 해당 스테이지의 일반 전투를 모두 완료하면
         * 휴식 단계로 전환합니다.
         */
        if (currentBattleCount >= maxBattleCount)
        {
            currentPhase = StagePhase.Rest;
        }

        PrintState("일반 전투 승리");
    }

    /// <summary>
    /// 리워드 패널의 Next 버튼 이후
    /// 현재 진행 상태에 맞는 다음 전투를 시작합니다.
    /// </summary>
    public void ProceedAfterReward(
        BattleManager battleManager)
    {
        if (battleManager == null)
        {
            Debug.LogError(
                "[StageManager] BattleManager가 없습니다."
            );

            return;
        }

        if (isGameClear)
        {
            Debug.LogWarning(
                "[StageManager] 이미 게임 클리어 상태입니다."
            );

            return;
        }

        /*
         * 일반 전투 상태라면
         * 다음 일반 전투 또는 다음 스테이지 일반 전투를 시작합니다.
         */
        if (currentPhase == StagePhase.NormalBattle)
        {
            Debug.Log(
                "[StageManager] 다음 일반 전투로 진행"
            );

            RefreshBGM();

            battleManager.StartNextBattle();
            return;
        }

        /*
         * 휴식 단계는 RewardPanelUI가 RestPanelUI를 열기 때문에
         * 이 메서드에서는 전투를 시작하지 않습니다.
         */
        if (currentPhase == StagePhase.Rest)
        {
            Debug.Log(
                "[StageManager] 현재 휴식 단계입니다."
            );

            return;
        }

        /*
         * Stage 3 모르바엘 처치 후에는
         * currentBossSequence가 1로 변경되어 있습니다.
         *
         * 리워드 패널에서 Next를 누르면
         * 휴식 없이 아리엘 전투를 시작합니다.
         */
        if (currentPhase == StagePhase.BossBattle)
        {
            if (IsStage3ArielBattle)
            {
                Debug.Log(
                    "[StageManager] 모르바엘 리워드 완료 - " +
                    "아리엘 전투로 진행"
                );

                RefreshBGM();

                battleManager.StartNextBattle();
                return;
            }

            Debug.LogWarning(
                "[StageManager] 현재 시작할 수 있는 " +
                "추가 보스 전투가 없습니다."
            );
        }
    }

    /// <summary>
    /// 휴식 완료 후 보스 전투 단계로 전환합니다.
    /// Stage 3에서는 첫 보스를 모르바엘로 설정합니다.
    /// </summary>
    public void RestComplete()
    {
        if (currentPhase != StagePhase.Rest)
        {
            Debug.LogWarning(
                "[StageManager] 현재 휴식 단계가 아닙니다."
            );

            return;
        }

        currentPhase = StagePhase.BossBattle;

        /*
         * 모든 스테이지의 보스전 시작 시
         * 보스 진행 순서를 처음 상태로 초기화합니다.
         *
         * Stage 3에서는 0이 모르바엘을 뜻합니다.
         */
        currentBossSequence = 0;

        PrintState("휴식 완료");

        RefreshBGM();
    }

    /// <summary>
    /// 보스 전투 승리를 처리합니다.
    ///
    /// Stage 1·2:
    /// 다음 스테이지 일반 전투로 전환합니다.
    ///
    /// Stage 3 모르바엘:
    /// 스테이지를 종료하지 않고 아리엘 전투 대기 상태로 전환합니다.
    ///
    /// Stage 3 아리엘:
    /// 게임을 클리어합니다.
    /// </summary>
    public void BossBattleWin()
    {
        if (currentPhase != StagePhase.BossBattle)
        {
            Debug.LogWarning(
                "[StageManager] 현재 보스 전투가 아닙니다."
            );

            return;
        }

        /*
         * Stage 3 첫 번째 보스인 모르바엘을 처치한 경우입니다.
         *
         * 스테이지와 Phase는 변경하지 않고
         * 두 번째 보스인 아리엘 전투 대기 상태로 변경합니다.
         */
        if (IsStage3MorvaelBattle)
        {
            currentBossSequence = 1;

            PrintState(
                "모르바엘 처치 - 아리엘 전투 대기"
            );

            return;
        }

        /*
         * Stage 3 두 번째 보스인 아리엘을 처치한 경우입니다.
         */
        if (IsStage3ArielBattle)
        {
            isGameClear = true;

            Debug.Log(
                "★★★★★ 아리엘 처치 - GAME CLEAR ★★★★★"
            );

            return;
        }

        /*
         * Stage 1 또는 Stage 2 보스를 처치한 경우
         * 다음 스테이지로 진행합니다.
         */
        currentStage++;

        selectedBossBattleID =
            string.Empty;

        if (currentStage > maxStage)
        {
            isGameClear = true;

            Debug.Log(
                "★★★★★ GAME CLEAR ★★★★★"
            );

            return;
        }

        currentBattleCount = 0;
        currentPhase = StagePhase.NormalBattle;
        currentBossSequence = 0;

        PrintState("보스 처치");
    }

    /// <summary>
    /// 기존 코드와의 호환성을 위해 남겨둔 메서드입니다.
    /// 보스 승리 처리 시 BossBattleWin을 호출합니다.
    /// </summary>
    public void BossWin()
    {
        BossBattleWin();
    }

    /// <summary>
    /// 현재 선택된 보스 Battle ID를 저장합니다.
    /// </summary>
    public void SetSelectedBossBattleID(
        string battleID)
    {
        selectedBossBattleID =
            battleID;

        Debug.Log(
            $"[StageManager] 선택된 보스 저장 : {battleID}"
        );
    }

    /// <summary>
    /// 저장된 보스 Battle ID를 초기화합니다.
    /// </summary>
    public void ClearSelectedBossBattleID()
    {
        selectedBossBattleID =
            string.Empty;
    }

    #endregion

    #region Private

    /// <summary>
    /// 현재 진행 상황에 맞는 BGM으로 갱신합니다.
    /// </summary>
    private void RefreshBGM()
    {
        if (BGMManager.Instance == null)
        {
            return;
        }

        BGMManager.Instance.RefreshBGM();
    }

    /// <summary>
    /// 현재 진행 상황을 Console에 출력합니다.
    /// </summary>
    private void PrintState(string action)
    {
        if (!debugMode)
        {
            return;
        }

        Debug.Log(
            $"[{action}] " +
            $"Stage: {currentStage} / " +
            $"Phase: {currentPhase} / " +
            $"Battle: {currentBattleCount}/{maxBattleCount} / " +
            $"Boss Sequence: {currentBossSequence} / " +
            $"Game Clear: {isGameClear}"
        );
    }

    #endregion
}
