using UnityEngine;

/// <summary>
/// 스테이지의 진행 상태를 관리하는 클래스
///
/// 관리 내용
/// - 현재 스테이지 번호
/// - 일반 전투 횟수 관리
/// - 휴식 진입
/// - 보스 전투 진행
/// - 게임 클리어 판정
///
/// 씬 이동이나 UI 처리는 담당하지 않는다.
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

    [Tooltip("현재 일반 전투 횟수")]
    [SerializeField]
    private int currentBattleCount = 0;

    [Tooltip("스테이지당 일반 전투 횟수")]
    [SerializeField]
    private int maxBattleCount = 5;

    [Space]

    [Tooltip("현재 진행 단계")]
    [SerializeField]
    private StagePhase currentPhase = StagePhase.NormalBattle;

    [Header("Debug")]

    [SerializeField]
    private bool debugMode = true;

    #endregion

    #region Property

    public int CurrentStage => currentStage;
    public int CurrentBattleCount => currentBattleCount;
    public StagePhase CurrentPhase => currentPhase;

    #endregion

    #region Public

    /// <summary>
    /// 게임 시작 시 호출
    /// </summary>
    public void StartGame()
    {
        currentStage = 1;
        currentBattleCount = 0;
        currentPhase = StagePhase.NormalBattle;

        PrintState("게임 시작");
    }

    /// <summary>
    /// 일반 전투 승리
    /// </summary>
    public void BattleWin()
    {
        if (currentPhase != StagePhase.NormalBattle)
        {
            Debug.LogWarning("현재 일반 전투 상태가 아닙니다.");
            return;
        }

        currentBattleCount++;

        // 일반 전투를 모두 완료했다면
        // 휴식 단계로 이동
        if (currentBattleCount >= maxBattleCount)
        {
            currentPhase = StagePhase.Rest;
        }

        PrintState("일반 전투 승리");
    }

    /// <summary>
    /// 휴식 완료
    /// </summary>
    public void RestComplete()
    {
        if (currentPhase != StagePhase.Rest)
        {
            Debug.LogWarning("현재 휴식 단계가 아닙니다.");
            return;
        }

        currentPhase = StagePhase.BossBattle;

        PrintState("휴식 완료");
    }

    /// <summary>
    /// 보스 처치
    /// </summary>
    public void BossWin()
    {
        if (currentPhase != StagePhase.BossBattle)
        {
            Debug.LogWarning("현재 보스 전투가 아닙니다.");
            return;
        }

        currentStage++;

        // 게임 클리어
        if (currentStage > maxStage)
        {
            Debug.Log("★★★★★ GAME CLEAR ★★★★★");
            return;
        }

        currentBattleCount = 0;
        currentPhase = StagePhase.NormalBattle;

        PrintState("보스 처치");
    }

    #endregion

    #region Private

    /// <summary>
    /// 현재 진행 상황 출력
    /// </summary>
    private void PrintState(string action)
    {
        if (!debugMode)
            return;

        Debug.Log(
            $"[{action}]\n" +
            $"Stage : {currentStage}\n" +
            $"Phase : {currentPhase}\n" +
            $"Battle : {currentBattleCount}/{maxBattleCount}"
        );
    }

    #endregion
}