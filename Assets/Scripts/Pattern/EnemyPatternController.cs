using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 적의 CSV 기반 행동 패턴 진행을 관리합니다.
///
/// 적의 고유 ID와 현재 패턴 턴을 저장하며,
/// EnemyPatternCSVLoader에서 현재 턴의 행동 목록을 가져옵니다.
///
/// 실제 피해, 방어도, 상태효과, 소환 실행은
/// 이후 EnemyPatternExecutor에서 담당합니다.
/// </summary>
public class EnemyPatternController : MonoBehaviour
{
    [Header("적 패턴 정보")]
    [SerializeField]
    private string enemyId;

    [Header("현재 패턴 턴")]
    [SerializeField]
    [Min(1)]
    private int currentPatternTurn = 1;

    [Header("전체 패턴 길이")]
    [SerializeField]
    [Min(0)]
    private int patternLength;

    [Header("CSV Loader")]
    [SerializeField]
    private EnemyPatternCSVLoader csvLoader;

    [Header("조건 검사기")]
    [SerializeField]
    private EnemyPatternConditionChecker conditionChecker;

    [Header("패턴 실행기")]
    [SerializeField]
    private EnemyPatternExecutor patternExecutor;

    [Header("초기화 상태")]
    [SerializeField]
    private bool isInitialized;

    /// <summary>
    /// 이 컨트롤러가 사용하는 적 ID입니다.
    /// </summary>
    public string EnemyId => enemyId;

    /// <summary>
    /// 다음에 실행할 패턴 턴입니다.
    /// </summary>
    public int CurrentPatternTurn =>
        currentPatternTurn;

    /// <summary>
    /// CSV에 등록된 전체 패턴 길이입니다.
    /// </summary>
    public int PatternLength =>
        patternLength;

    /// <summary>
    /// 패턴 컨트롤러가 정상적으로 초기화됐는지 여부입니다.
    /// </summary>
    public bool IsInitialized =>
        isInitialized;

    private void Start()
    {
        Initialize();
    }

    /// <summary>
    /// CSV 로더와 적 패턴 정보를 초기화합니다.
    /// </summary>
    public void Initialize()
    {
        isInitialized = false;

        if (string.IsNullOrWhiteSpace(enemyId))
        {
            Debug.LogError(
                "[EnemyPatternController] " +
                "Enemy ID가 지정되지 않았습니다.",
                this
            );

            return;
        }

        TryFindCSVLoader();
        TryFindConditionChecker();
        TryFindPatternExecutor();

        if (csvLoader == null)
        {
            Debug.LogError(
                "[EnemyPatternController] " +
                "EnemyPatternCSVLoader를 찾지 못했습니다.",
                this
            );

            return;
        }

        if (conditionChecker == null)
        {
            Debug.LogError(
                "[EnemyPatternController] " +
                "EnemyPatternConditionChecker를 찾지 못했습니다.",
                this
            );

            return;
        }

        if (patternExecutor == null)
        {
            Debug.LogError(
                "[EnemyPatternController] " +
                "EnemyPatternExecutor를 찾지 못했습니다.",
                this
            );

            return;
        }

        /*
         * 로더가 아직 데이터를 읽지 않은 상태라면
         * CSV를 다시 읽습니다.
         */
        if (csvLoader.AllPatternData.Count == 0)
        {
            csvLoader.LoadCSV();
        }

        patternLength =
            csvLoader.GetPatternLength(enemyId);

        if (patternLength <= 0)
        {
            Debug.LogError(
                $"[EnemyPatternController] " +
                $"Enemy ID '{enemyId}'에 해당하는 " +
                "패턴 데이터를 찾지 못했습니다.",
                this
            );

            return;
        }

        currentPatternTurn = 1;
        isInitialized = true;

        Debug.Log(
            $"[EnemyPatternController] 초기화 완료 / " +
            $"Enemy ID: {enemyId} / " +
            $"패턴 길이: {patternLength}",
            this
        );
    }

    /// <summary>
    /// 현재 패턴 턴에 등록된 행동 목록을 반환합니다.
    ///
    /// 아직 실제 행동은 실행하지 않습니다.
    /// </summary>
    public List<EnemyPatternData> GetCurrentPatternActions()
    {
        if (!EnsureInitialized())
        {
            return new List<EnemyPatternData>();
        }

        List<EnemyPatternData> actions =
            csvLoader.GetPatternActions(
                enemyId,
                currentPatternTurn
            );

        if (actions.Count == 0)
        {
            Debug.LogWarning(
                $"[EnemyPatternController] " +
                $"{enemyId}의 {currentPatternTurn}턴에 " +
                "등록된 행동이 없습니다.",
                this
            );
        }

        return actions;
    }

    /// <summary>
    /// 현재 패턴 턴의 행동을 가져와 조건을 검사한 뒤
    /// 통과한 행동을 실제로 실행합니다.
    ///
    /// 행동 처리가 끝나면 다음 패턴 턴으로 이동합니다.
    /// </summary>
    public void ProcessCurrentPattern()
    {
        List<EnemyPatternData> actions =
            GetCurrentPatternActions();

        if (actions.Count == 0)
        {
            return;
        }

        Debug.Log(
            $"[EnemyPatternController] " +
            $"{enemyId} 패턴 {currentPatternTurn}턴 실행 시작 / " +
            $"등록 행동 수: {actions.Count}",
            this
        );

        int executedActionCount = 0;

        for (int i = 0;
             i < actions.Count;
             i++)
        {
            EnemyPatternData action =
                actions[i];

            if (!conditionChecker.IsConditionMet(action))
            {
                Debug.Log(
                    $"[EnemyPatternController] 조건 불충족 / " +
                    $"행동 {action.actionType} / " +
                    $"조건 {action.conditionType}",
                    this
                );

                continue;
            }

            bool executed =
                patternExecutor.ExecuteAction(action);

            if (executed)
            {
                executedActionCount++;
            }
            else
            {
                Debug.LogWarning(
                    $"[EnemyPatternController] 행동 실행 실패 / " +
                    $"{action.actionType}",
                    this
                );
            }
        }

        Debug.Log(
            $"[EnemyPatternController] " +
            $"{enemyId} {currentPatternTurn}턴 실행 완료 / " +
            $"성공 행동 수: {executedActionCount}",
            this
        );

        AdvancePatternTurn();
    }

    /// <summary>
    /// 다음 패턴 턴으로 이동합니다.
    ///
    /// 마지막 패턴 이후에는 다시 1턴으로 돌아갑니다.
    /// </summary>
    public void AdvancePatternTurn()
    {
        if (!EnsureInitialized())
        {
            return;
        }

        currentPatternTurn++;

        if (currentPatternTurn > patternLength)
        {
            currentPatternTurn = 1;
        }

        Debug.Log(
            $"[EnemyPatternController] " +
            $"{enemyId} 다음 패턴 턴: " +
            $"{currentPatternTurn}/{patternLength}",
            this
        );
    }

    /// <summary>
    /// 패턴을 다시 1턴으로 초기화합니다.
    /// </summary>
    public void ResetPattern()
    {
        currentPatternTurn = 1;

        Debug.Log(
            $"[EnemyPatternController] " +
            $"{enemyId} 패턴을 1턴으로 초기화했습니다.",
            this
        );
    }

    /// <summary>
    /// 적 ID를 지정하고 패턴을 다시 초기화합니다.
    ///
    /// 적 프리팹 생성 후 외부 코드에서 ID를 설정할 때 사용할 수 있습니다.
    /// </summary>
    public void SetEnemyId(string newEnemyId)
    {
        if (string.IsNullOrWhiteSpace(newEnemyId))
        {
            Debug.LogWarning(
                "[EnemyPatternController] " +
                "빈 Enemy ID는 설정할 수 없습니다.",
                this
            );

            return;
        }

        enemyId = newEnemyId;
        Initialize();
    }

    /// <summary>
    /// CSV Loader를 외부에서 연결합니다.
    /// </summary>
    public void SetCSVLoader(
        EnemyPatternCSVLoader loader)
    {
        csvLoader = loader;

        if (csvLoader == null)
        {
            Debug.LogWarning(
                "[EnemyPatternController] " +
                "CSV Loader 연결에 실패했습니다.",
                this
            );

            return;
        }

        Initialize();
    }

    /// <summary>
    /// 패턴 컨트롤러가 실행 가능한 상태인지 확인합니다.
    /// </summary>
    private bool EnsureInitialized()
    {
        if (isInitialized)
        {
            return true;
        }

        Initialize();

        return isInitialized;
    }

    /// <summary>
    /// 현재 씬에서 EnemyPatternCSVLoader를 찾습니다.
    /// </summary>
    private void TryFindCSVLoader()
    {
        if (csvLoader != null)
        {
            return;
        }

        csvLoader =
            FindFirstObjectByType<EnemyPatternCSVLoader>();

        if (csvLoader != null)
        {
            Debug.Log(
                "[EnemyPatternController] " +
                "EnemyPatternCSVLoader 자동 탐색 완료",
                this
            );
        }
    }

    /// <summary>
    /// 현재 씬에서 EnemyPatternConditionChecker를 찾습니다.
    /// </summary>
    private void TryFindConditionChecker()
    {
        if (conditionChecker != null)
        {
            return;
        }

        conditionChecker =
            FindFirstObjectByType<EnemyPatternConditionChecker>();

        if (conditionChecker != null)
        {
            Debug.Log(
                "[EnemyPatternController] " +
                "EnemyPatternConditionChecker 자동 탐색 완료",
                this
            );
        }
    }

    /// <summary>
    /// 같은 적 오브젝트에서 EnemyPatternExecutor를 찾습니다.
    /// </summary>
    private void TryFindPatternExecutor()
    {
        if (patternExecutor != null)
        {
            return;
        }

        patternExecutor =
            GetComponent<EnemyPatternExecutor>();

        if (patternExecutor != null)
        {
            Debug.Log(
                "[EnemyPatternController] " +
                "EnemyPatternExecutor 자동 탐색 완료",
                this
            );
        }
    }

    /// <summary>
    /// 조건 검사기를 외부에서 연결합니다.
    /// </summary>
    public void SetConditionChecker(
        EnemyPatternConditionChecker checker)
    {
        conditionChecker = checker;

        if (conditionChecker == null)
        {
            Debug.LogWarning(
                "[EnemyPatternController] " +
                "조건 검사기 연결에 실패했습니다.",
                this
            );

            return;
        }

        Debug.Log(
            "[EnemyPatternController] " +
            "조건 검사기 연결 완료",
            this
        );
    }

    /// <summary>
    /// Inspector에서 현재 패턴 처리를 테스트합니다.
    /// </summary>
    [ContextMenu("현재 적 패턴 테스트")]
    private void TestCurrentPattern()
    {
        ProcessCurrentPattern();
    }

    /// <summary>
    /// Inspector에서 패턴을 초기화합니다.
    /// </summary>
    [ContextMenu("적 패턴 초기화")]
    private void TestResetPattern()
    {
        ResetPattern();
    }
}