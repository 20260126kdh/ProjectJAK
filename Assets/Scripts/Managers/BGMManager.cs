using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 게임 전체의 BGM 재생과 전환을 관리합니다.
///
/// - 메인 타이틀과 클래스 선택 씬에서는 타이틀 BGM을 유지합니다.
/// - 전투 씬에서는 현재 스테이지와 진행 단계에 맞는 BGM을 재생합니다.
/// - 같은 BGM이 재생 중이라면 처음부터 다시 재생하지 않습니다.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class BGMManager : MonoBehaviour
{
    #region Singleton

    public static BGMManager Instance { get; private set; }

    #endregion

    #region Inspector

    [Header("Scene Names")]

    [Tooltip("메인 타이틀 씬 이름")]
    [SerializeField]
    private string titleSceneName = "TitleScene";

    [Tooltip("클래스 선택 씬 이름")]
    [SerializeField]
    private string classSelectSceneName = "ClassSelectScene";

    [Tooltip("전투 씬 이름")]
    [SerializeField]
    private string battleSceneName = "BattleScene";

    [Header("Main BGM")]

    [Tooltip("메인 타이틀 및 클래스 선택에서 사용할 BGM")]
    [SerializeField]
    private AudioClip titleBGM;

    [Header("Stage 1 BGM")]

    [SerializeField]
    private AudioClip stage1NormalBGM;

    [SerializeField]
    private AudioClip stage1BossBGM;

    [Header("Stage 2 BGM")]

    [SerializeField]
    private AudioClip stage2NormalBGM;

    [SerializeField]
    private AudioClip stage2BossBGM;

    [Header("Stage 3 BGM")]

    [SerializeField]
    private AudioClip stage3NormalBGM;

    [Tooltip("Stage 3 첫 번째 보스 모르바엘 BGM")]
    [SerializeField]
    private AudioClip stage3MorvaelBGM;

    [Tooltip("Stage 3 최종 보스 아리엘 BGM")]
    [SerializeField]
    private AudioClip stage3ArielBGM;

    [Header("Audio Settings")]

    [Range(0f, 1f)]
    [SerializeField]
    private float bgmVolume = 0.5f;

    [SerializeField]
    private bool debugMode = true;

    #endregion

    #region Private

    private AudioSource audioSource;

    #endregion

    #region Unity

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        audioSource = GetComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = true;
        audioSource.volume = bgmVolume;

        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        RefreshBGM();
    }

    #endregion

    #region Public

    /// <summary>
    /// 현재 씬과 StageManager 진행도에 맞는 BGM을 확인하고 재생합니다.
    /// </summary>
    public void RefreshBGM()
    {
        string currentSceneName =
            SceneManager.GetActiveScene().name;

        if (currentSceneName == titleSceneName ||
            currentSceneName == classSelectSceneName)
        {
            PlayBGM(titleBGM);
            return;
        }

        if (currentSceneName == battleSceneName)
        {
            PlayCurrentStageBGM();
            return;
        }

        if (debugMode)
        {
            Debug.Log(
                $"[BGMManager] 등록되지 않은 씬입니다: " +
                $"{currentSceneName}"
            );
        }
    }

    /// <summary>
    /// BGM 볼륨을 변경합니다.
    /// 입력 범위는 0부터 1까지입니다.
    /// </summary>
    public void SetVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);

        if (audioSource != null)
        {
            audioSource.volume = bgmVolume;
        }
    }

    /// <summary>
    /// 현재 BGM을 정지합니다.
    /// </summary>
    public void StopBGM()
    {
        if (audioSource == null)
        {
            return;
        }

        audioSource.Stop();
        audioSource.clip = null;
    }

    #endregion

    #region Private

    /// <summary>
    /// 씬이 로드되었을 때 현재 씬에 맞는 BGM을 재생합니다.
    /// </summary>
    private void OnSceneLoaded(
        Scene scene,
        LoadSceneMode loadSceneMode)
    {
        RefreshBGM();
    }

    /// <summary>
    /// 현재 스테이지와 진행 단계에 맞는 BGM을 재생합니다.
    /// 휴식 단계에서는 해당 스테이지의 일반 BGM을 유지합니다.
    /// </summary>
    private void PlayCurrentStageBGM()
    {
        StageManager stageManager =
            StageManager.Instance;

        if (stageManager == null)
        {
            stageManager =
                FindFirstObjectByType<StageManager>();
        }

        if (stageManager == null)
        {
            Debug.LogWarning(
                "[BGMManager] StageManager를 찾지 못했습니다."
            );

            return;
        }

        int currentStage =
            stageManager.CurrentStage;

        StagePhase currentPhase =
            stageManager.CurrentPhase;

        bool isBossBattle =
            currentPhase == StagePhase.BossBattle;

        AudioClip targetBGM =
            GetStageBGM(
                currentStage,
                isBossBattle,
                stageManager.CurrentBossSequence
            );

        PlayBGM(targetBGM);
    }

    /// <summary>
    /// 스테이지 번호, 보스 여부, 보스 진행 순서에 맞는
    /// BGM을 반환합니다.
    /// </summary>
    private AudioClip GetStageBGM(
        int stage,
        bool isBossBattle,
        int bossSequence)
    {
        switch (stage)
        {
            case 1:
                return isBossBattle
                    ? stage1BossBGM
                    : stage1NormalBGM;

            case 2:
                return isBossBattle
                    ? stage2BossBGM
                    : stage2NormalBGM;

            case 3:
                /*
                 * Stage 3 일반 전투와 휴식에서는
                 * Stage 3 일반 BGM을 사용합니다.
                 */
                if (!isBossBattle)
                {
                    return stage3NormalBGM;
                }

                /*
                 * Stage 3 보스 순서
                 *
                 * 0: 모르바엘
                 * 1: 아리엘
                 */
                if (bossSequence == 0)
                {
                    return stage3MorvaelBGM;
                }

                if (bossSequence == 1)
                {
                    return stage3ArielBGM;
                }

                Debug.LogWarning(
                    $"[BGMManager] 지원하지 않는 Stage 3 " +
                    $"Boss Sequence입니다: {bossSequence}"
                );

                return null;

            default:
                Debug.LogWarning(
                    $"[BGMManager] 지원하지 않는 스테이지입니다: " +
                    $"{stage}"
                );

                return null;
        }
    }

    /// <summary>
    /// 지정한 BGM을 재생합니다.
    /// 같은 곡이 이미 재생 중이라면 다시 시작하지 않습니다.
    /// </summary>
    private void PlayBGM(AudioClip targetClip)
    {
        if (targetClip == null)
        {
            Debug.LogWarning(
                "[BGMManager] 재생할 AudioClip이 없습니다."
            );

            return;
        }

        if (audioSource.clip == targetClip)
        {
            if (debugMode)
            {
                Debug.Log(
                    $"[BGMManager] 같은 BGM 유지: " +
                    $"{targetClip.name}"
                );
            }

            return;
        }

        audioSource.Stop();
        audioSource.clip = targetClip;
        audioSource.loop = true;
        audioSource.volume = bgmVolume;
        audioSource.Play();

        if (debugMode)
        {
            Debug.Log(
                $"[BGMManager] BGM 재생: " +
                $"{targetClip.name}"
            );
        }
    }

    #endregion
}