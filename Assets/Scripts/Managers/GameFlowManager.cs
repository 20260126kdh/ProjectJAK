using UnityEngine;
using UnityEngine.Events;

public class GameFlowManager : MonoBehaviour
{
    #region Singleton

    public static GameFlowManager Instance { get; private set; }

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

    //========================================================
    // Inspector
    //========================================================

    [Header("Current State")]
    [Tooltip("현재 게임 상태")]
    [SerializeField]
    private GameState currentState = GameState.MainMenu;

    [Header("Debug")]
    [SerializeField]
    private bool debugMode = true;

    [SerializeField]
    private bool printStateLog = true;

    [Header("Events")]
    [Tooltip("상태가 변경되면 호출됩니다.")]
    [SerializeField]
    private UnityEvent<GameState> onStateChanged;

    //========================================================
    // Properties
    //========================================================

    public GameState CurrentState => currentState;

    //========================================================
    // Unity
    //========================================================

    private void Start()
    {
        ChangeState(currentState);
    }

    //========================================================
    // Public
    //========================================================

    /// <summary>
    /// 게임 시작
    /// </summary>
    public void StartGame()
    {
        ChangeState(GameState.ClassSelect);
    }

    /// <summary>
    /// 전투 시작
    /// </summary>
    public void EnterBattle()
    {
        ChangeState(GameState.Battle);
    }

    /// <summary>
    /// 보상 화면
    /// </summary>
    public void EnterReward()
    {
        ChangeState(GameState.Reward);
    }

    /// <summary>
    /// 휴식
    /// </summary>
    public void EnterRest()
    {
        ChangeState(GameState.Rest);
    }

    /// <summary>
    /// 보스 전투
    /// </summary>
    public void EnterBossBattle()
    {
        ChangeState(GameState.BossBattle);
    }

    /// <summary>
    /// 게임 오버
    /// </summary>
    public void GameOver()
    {
        ChangeState(GameState.GameOver);
    }

    /// <summary>
    /// 게임 클리어
    /// </summary>
    public void GameClear()
    {
        ChangeState(GameState.GameClear);
    }

    //========================================================
    // Core
    //========================================================

    public void ChangeState(GameState newState)
    {
        if (currentState == newState)
            return;

        currentState = newState;

        if (printStateLog)
        {
            Debug.Log($"[GameFlow] State Changed : {currentState}");
        }

        HandleState(currentState);

        onStateChanged?.Invoke(currentState);
    }

    //========================================================
    // State Handler
    //========================================================

    private void HandleState(GameState state)
    {
        switch (state)
        {
            case GameState.MainMenu:

                if (debugMode)
                    Debug.Log("메인 메뉴");

                break;

            case GameState.ClassSelect:

                if (debugMode)
                    Debug.Log("클래스 선택");

                break;

            case GameState.Battle:

                if (debugMode)
                    Debug.Log("전투 시작");

                break;

            case GameState.Reward:

                if (debugMode)
                    Debug.Log("보상 화면");

                break;

            case GameState.Rest:

                if (debugMode)
                    Debug.Log("휴식");

                break;

            case GameState.BossBattle:

                if (debugMode)
                    Debug.Log("보스 전투");

                break;

            case GameState.GameOver:

                if (debugMode)
                    Debug.Log("게임 오버");

                break;

            case GameState.GameClear:

                if (debugMode)
                    Debug.Log("게임 클리어");

                break;
        }
    }
}