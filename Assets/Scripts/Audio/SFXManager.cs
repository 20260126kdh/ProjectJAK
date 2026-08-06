using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// 게임 전체의 효과음 재생을 관리합니다.
///
/// 담당 기능:
/// - 단일 효과음 재생
/// - 여러 효과음 동시 재생
/// - 효과음별 개별 볼륨 적용
/// - 필요 시 랜덤 피치 적용
/// - AudioMixer의 SFX 그룹으로 출력
///
/// 각 기능 스크립트는 AudioSource를 직접 제어하지 않고
/// SFXManager를 통해 효과음을 재생합니다.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class SFXManager : MonoBehaviour
{
    #region Singleton

    public static SFXManager Instance { get; private set; }

    #endregion

    #region Inspector

    [Header("Audio Mixer Group")]

    [Tooltip("GameAudioMixer의 SFX 그룹을 연결합니다.")]
    [SerializeField]
    private AudioMixerGroup sfxMixerGroup;

    [Header("기본 재생 설정")]

    [Range(0f, 1f)]
    [Tooltip("각 효과음에 기본적으로 적용할 개별 볼륨입니다.")]
    [SerializeField]
    private float defaultVolume = 1f;

    [Tooltip("재생할 때마다 약간의 랜덤 피치를 적용할지 여부입니다.")]
    [SerializeField]
    private bool useRandomPitch;

    [Range(0f, 0.5f)]
    [Tooltip("기본 피치 1을 기준으로 변화할 최대 범위입니다.")]
    [SerializeField]
    private float randomPitchRange = 0.05f;

    [Header("Debug")]

    [SerializeField]
    private bool debugMode;

    #endregion

    #region Private

    private AudioSource audioSource;

    #endregion

    #region Unity

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        audioSource =
            GetComponent<AudioSource>();

        ConfigureAudioSource();

        DontDestroyOnLoad(gameObject);
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// 기본 설정으로 효과음을 한 번 재생합니다.
    /// </summary>
    /// <param name="clip">재생할 효과음</param>
    public void PlaySFX(AudioClip clip)
    {
        PlaySFX(
            clip,
            defaultVolume,
            useRandomPitch
        );
    }

    /// <summary>
    /// 지정한 개별 볼륨으로 효과음을 한 번 재생합니다.
    /// </summary>
    /// <param name="clip">재생할 효과음</param>
    /// <param name="volumeScale">0~1 범위의 개별 볼륨</param>
    public void PlaySFX(
        AudioClip clip,
        float volumeScale)
    {
        PlaySFX(
            clip,
            volumeScale,
            useRandomPitch
        );
    }

    /// <summary>
    /// 지정한 볼륨과 랜덤 피치 설정으로 효과음을 재생합니다.
    ///
    /// PlayOneShot을 사용하므로 앞의 효과음이 끝나지 않았더라도
    /// 다른 효과음을 동시에 재생할 수 있습니다.
    /// </summary>
    /// <param name="clip">재생할 효과음</param>
    /// <param name="volumeScale">0~1 범위의 개별 볼륨</param>
    /// <param name="applyRandomPitch">랜덤 피치 적용 여부</param>
    public void PlaySFX(
        AudioClip clip,
        float volumeScale,
        bool applyRandomPitch)
    {
        if (clip == null)
        {
            Debug.LogWarning(
                "[SFXManager] 재생할 AudioClip이 없습니다."
            );

            return;
        }

        if (audioSource == null)
        {
            Debug.LogError(
                "[SFXManager] AudioSource를 찾지 못했습니다."
            );

            return;
        }

        float previousPitch =
            audioSource.pitch;

        if (applyRandomPitch)
        {
            audioSource.pitch =
                Random.Range(
                    1f - randomPitchRange,
                    1f + randomPitchRange
                );
        }
        else
        {
            audioSource.pitch = 1f;
        }

        audioSource.PlayOneShot(
            clip,
            Mathf.Clamp01(volumeScale)
        );

        /*
         * 다음 효과음에 현재 피치가 남지 않도록
         * 기본 피치로 되돌립니다.
         */
        audioSource.pitch =
            previousPitch;

        if (debugMode)
        {
            Debug.Log(
                $"[SFXManager] 효과음 재생: " +
                $"{clip.name} / " +
                $"볼륨: {Mathf.Clamp01(volumeScale):F2}"
            );
        }
    }

    /// <summary>
    /// 전달받은 효과음 목록 중 하나를 무작위로 선택해 재생합니다.
    /// 반복 공격이나 여러 종류의 타격음에 사용할 수 있습니다.
    /// </summary>
    public void PlayRandomSFX(
        AudioClip[] clips,
        float volumeScale = 1f,
        bool applyRandomPitch = true)
    {
        if (clips == null ||
            clips.Length == 0)
        {
            Debug.LogWarning(
                "[SFXManager] 무작위 재생할 효과음 목록이 비어 있습니다."
            );

            return;
        }

        int randomIndex =
            Random.Range(
                0,
                clips.Length
            );

        AudioClip selectedClip =
            clips[randomIndex];

        PlaySFX(
            selectedClip,
            volumeScale,
            applyRandomPitch
        );
    }

    /// <summary>
    /// 현재 AudioSource에서 재생 중인 효과음을 정지합니다.
    ///
    /// PlayOneShot으로 재생된 모든 효과음이 함께 정지됩니다.
    /// 씬 초기화나 강제 종료 상황에서만 사용합니다.
    /// </summary>
    public void StopAllSFX()
    {
        if (audioSource == null)
        {
            return;
        }

        audioSource.Stop();

        if (debugMode)
        {
            Debug.Log(
                "[SFXManager] 모든 효과음 정지"
            );
        }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// 효과음 재생에 맞게 AudioSource를 초기화합니다.
    /// </summary>
    private void ConfigureAudioSource()
    {
        if (audioSource == null)
        {
            return;
        }

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.volume = 1f;
        audioSource.pitch = 1f;
        audioSource.spatialBlend = 0f;

        if (sfxMixerGroup != null)
        {
            audioSource.outputAudioMixerGroup =
                sfxMixerGroup;
        }
        else
        {
            Debug.LogWarning(
                "[SFXManager] SFX AudioMixerGroup이 연결되지 않았습니다."
            );
        }
    }

#if UNITY_EDITOR

    /// <summary>
    /// Inspector 값이 변경될 때 AudioSource 설정을 동기화합니다.
    /// </summary>
    private void OnValidate()
    {
        defaultVolume =
            Mathf.Clamp01(defaultVolume);

        randomPitchRange =
            Mathf.Clamp(
                randomPitchRange,
                0f,
                0.5f
            );

        AudioSource source =
            GetComponent<AudioSource>();

        if (source == null)
        {
            return;
        }

        source.playOnAwake = false;
        source.loop = false;
        source.volume = 1f;
        source.spatialBlend = 0f;

        if (sfxMixerGroup != null)
        {
            source.outputAudioMixerGroup =
                sfxMixerGroup;
        }
    }

#endif

    #endregion
}