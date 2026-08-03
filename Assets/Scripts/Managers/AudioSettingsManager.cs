using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// 게임의 오디오 환경 설정을 관리합니다.
///
/// 담당 기능:
/// - 전체 음량 설정
/// - BGM 음량 설정
/// - SFX 음량 설정
/// - PlayerPrefs 저장 및 불러오기
/// - AudioMixer에 실제 볼륨 반영
///
/// 슬라이더 입력값은 0~1 범위를 사용하고,
/// AudioMixer에는 데시벨 값으로 변환해 적용합니다.
/// </summary>
public class AudioSettingsManager : MonoBehaviour
{
    #region PlayerPrefs Keys

    private const string MasterVolumeKey =
        "MasterVolume";

    private const string BGMVolumeKey =
        "BGMVolume";

    private const string SFXVolumeKey =
        "SFXVolume";

    #endregion

    #region AudioMixer Parameters

    private const string MasterVolumeParameter =
        "MasterVolume";

    private const string BGMVolumeParameter =
        "BGMVolume";

    private const string SFXVolumeParameter =
        "SFXVolume";

    #endregion

    #region Inspector

    [Header("Audio Mixer")]
    [SerializeField]
    private AudioMixer audioMixer;

    [Header("기본 볼륨")]

    [Range(0f, 1f)]
    [SerializeField]
    private float defaultMasterVolume = 1f;

    [Range(0f, 1f)]
    [SerializeField]
    private float defaultBGMVolume = 0.5f;

    [Range(0f, 1f)]
    [SerializeField]
    private float defaultSFXVolume = 0.5f;

    [Header("현재 볼륨")]

    [Range(0f, 1f)]
    [SerializeField]
    private float masterVolume;

    [Range(0f, 1f)]
    [SerializeField]
    private float bgmVolume;

    [Range(0f, 1f)]
    [SerializeField]
    private float sfxVolume;

    [Header("Debug")]
    [SerializeField]
    private bool debugMode = true;

    #endregion

    #region Property

    /// <summary>
    /// 현재 전체 음량을 반환합니다.
    /// </summary>
    public float MasterVolume =>
        masterVolume;

    /// <summary>
    /// 현재 BGM 음량을 반환합니다.
    /// </summary>
    public float BGMVolume =>
        bgmVolume;

    /// <summary>
    /// 현재 SFX 음량을 반환합니다.
    /// </summary>
    public float SFXVolume =>
        sfxVolume;

    #endregion

    #region Unity

    /// <summary>
    /// 저장된 볼륨 값을 불러오고
    /// AudioMixer에 반영합니다.
    /// </summary>
    private void Start()
    {
        LoadAudioSettings();
        ApplyAllVolumes();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// 전체 음량을 변경하고 저장합니다.
    /// Slider의 On Value Changed에서 사용합니다.
    /// </summary>
    /// <param name="value">0~1 범위의 음량 값</param>
    public void SetMasterVolume(float value)
    {
        masterVolume =
            Mathf.Clamp01(value);

        ApplyVolume(
            MasterVolumeParameter,
            masterVolume
        );

        PlayerPrefs.SetFloat(
            MasterVolumeKey,
            masterVolume
        );

        PlayerPrefs.Save();

        if (debugMode)
        {
            Debug.Log(
                $"[AudioSettingsManager] " +
                $"전체 음량 변경: {masterVolume:F2}"
            );
        }
    }

    /// <summary>
    /// BGM 음량을 변경하고 저장합니다.
    /// Slider의 On Value Changed에서 사용합니다.
    /// </summary>
    /// <param name="value">0~1 범위의 음량 값</param>
    public void SetBGMVolume(float value)
    {
        bgmVolume =
            Mathf.Clamp01(value);

        ApplyVolume(
            BGMVolumeParameter,
            bgmVolume
        );

        PlayerPrefs.SetFloat(
            BGMVolumeKey,
            bgmVolume
        );

        PlayerPrefs.Save();

        if (debugMode)
        {
            Debug.Log(
                $"[AudioSettingsManager] " +
                $"BGM 음량 변경: {bgmVolume:F2}"
            );
        }
    }

    /// <summary>
    /// SFX 음량을 변경하고 저장합니다.
    /// Slider의 On Value Changed에서 사용합니다.
    /// </summary>
    /// <param name="value">0~1 범위의 음량 값</param>
    public void SetSFXVolume(float value)
    {
        sfxVolume =
            Mathf.Clamp01(value);

        ApplyVolume(
            SFXVolumeParameter,
            sfxVolume
        );

        PlayerPrefs.SetFloat(
            SFXVolumeKey,
            sfxVolume
        );

        PlayerPrefs.Save();

        if (debugMode)
        {
            Debug.Log(
                $"[AudioSettingsManager] " +
                $"SFX 음량 변경: {sfxVolume:F2}"
            );
        }
    }

    /// <summary>
    /// 오디오 설정을 기본값으로 초기화합니다.
    /// </summary>
    public void ResetAudioSettings()
    {
        masterVolume =
            defaultMasterVolume;

        bgmVolume =
            defaultBGMVolume;

        sfxVolume =
            defaultSFXVolume;

        SaveAllVolumes();
        ApplyAllVolumes();

        Debug.Log(
            "[AudioSettingsManager] " +
            "오디오 설정 기본값 복원"
        );
    }

    /// <summary>
    /// 저장된 모든 오디오 설정을 다시 불러옵니다.
    /// </summary>
    public void ReloadAudioSettings()
    {
        LoadAudioSettings();
        ApplyAllVolumes();
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// PlayerPrefs에서 저장된 음량 값을 불러옵니다.
    /// 저장값이 없으면 Inspector의 기본값을 사용합니다.
    /// </summary>
    private void LoadAudioSettings()
    {
        masterVolume =
            PlayerPrefs.GetFloat(
                MasterVolumeKey,
                defaultMasterVolume
            );

        bgmVolume =
            PlayerPrefs.GetFloat(
                BGMVolumeKey,
                defaultBGMVolume
            );

        sfxVolume =
            PlayerPrefs.GetFloat(
                SFXVolumeKey,
                defaultSFXVolume
            );

        masterVolume =
            Mathf.Clamp01(masterVolume);

        bgmVolume =
            Mathf.Clamp01(bgmVolume);

        sfxVolume =
            Mathf.Clamp01(sfxVolume);

        if (debugMode)
        {
            Debug.Log(
                $"[AudioSettingsManager] 설정 불러오기 / " +
                $"Master: {masterVolume:F2} / " +
                $"BGM: {bgmVolume:F2} / " +
                $"SFX: {sfxVolume:F2}"
            );
        }
    }

    /// <summary>
    /// 현재 모든 음량 값을 PlayerPrefs에 저장합니다.
    /// </summary>
    private void SaveAllVolumes()
    {
        PlayerPrefs.SetFloat(
            MasterVolumeKey,
            masterVolume
        );

        PlayerPrefs.SetFloat(
            BGMVolumeKey,
            bgmVolume
        );

        PlayerPrefs.SetFloat(
            SFXVolumeKey,
            sfxVolume
        );

        PlayerPrefs.Save();
    }

    /// <summary>
    /// 현재 모든 볼륨을 AudioMixer에 적용합니다.
    /// </summary>
    private void ApplyAllVolumes()
    {
        ApplyVolume(
            MasterVolumeParameter,
            masterVolume
        );

        ApplyVolume(
            BGMVolumeParameter,
            bgmVolume
        );

        ApplyVolume(
            SFXVolumeParameter,
            sfxVolume
        );
    }

    /// <summary>
    /// 0~1 범위의 선형 볼륨 값을
    /// AudioMixer용 데시벨 값으로 변환해 적용합니다.
    ///
    /// 0일 때 Log10 계산 오류를 피하기 위해
    /// -80dB로 처리합니다.
    /// </summary>
    private void ApplyVolume(
        string parameterName,
        float linearVolume)
    {
        if (audioMixer == null)
        {
            Debug.LogError(
                "[AudioSettingsManager] " +
                "AudioMixer가 연결되지 않았습니다."
            );

            return;
        }

        float decibelValue;

        if (linearVolume <= 0.0001f)
        {
            decibelValue = -80f;
        }
        else
        {
            decibelValue =
                Mathf.Log10(linearVolume) * 20f;
        }

        bool succeeded =
            audioMixer.SetFloat(
                parameterName,
                decibelValue
            );

        if (!succeeded)
        {
            Debug.LogError(
                $"[AudioSettingsManager] " +
                $"AudioMixer 파라미터를 찾지 못했습니다: " +
                $"{parameterName}"
            );
        }
    }

    #endregion
}