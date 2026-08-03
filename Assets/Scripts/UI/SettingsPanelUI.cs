using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 환경 설정 패널의 UI 입력과 화면 표시를 관리합니다.
///
/// 담당 기능:
/// - 전체 음량 슬라이더
/// - BGM 음량 슬라이더
/// - SFX 음량 슬라이더
/// - 음량 퍼센트 텍스트
/// - 전체 화면 설정
/// - 오디오 설정 초기화
/// - 일시정지 메뉴로 복귀
///
/// 실제 오디오 값의 저장과 AudioMixer 반영은
/// AudioSettingsManager가 담당합니다.
/// </summary>
public class SettingsPanelUI : MonoBehaviour
{
    #region PlayerPrefs Key

    private const string FullscreenKey =
        "Fullscreen";

    #endregion

    #region Inspector

    [Header("Audio Settings Manager")]
    [SerializeField]
    private AudioSettingsManager audioSettingsManager;

    [Header("Pause Manager")]
    [SerializeField]
    private PauseManager pauseManager;

    [Header("Master Volume")]

    [SerializeField]
    private Slider masterSlider;

    [SerializeField]
    private TMP_Text masterValueText;

    [Header("BGM Volume")]

    [SerializeField]
    private Slider bgmSlider;

    [SerializeField]
    private TMP_Text bgmValueText;

    [Header("SFX Volume")]

    [SerializeField]
    private Slider sfxSlider;

    [SerializeField]
    private TMP_Text sfxValueText;

    [Header("Fullscreen")]

    [SerializeField]
    private Toggle fullscreenToggle;

    [Header("Debug")]
    [SerializeField]
    private bool debugMode = true;

    #endregion

    #region Unity

    /// <summary>
    /// 설정 패널이 활성화될 때마다
    /// 현재 저장된 설정값을 UI에 반영합니다.
    /// </summary>
    private void OnEnable()
    {
        RefreshUI();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// 전체 음량 슬라이더 변경을 처리합니다.
    /// Slider의 On Value Changed에 연결합니다.
    /// </summary>
    public void OnMasterVolumeChanged(float value)
    {
        if (audioSettingsManager == null)
        {
            Debug.LogError(
                "[SettingsPanelUI] AudioSettingsManager가 연결되지 않았습니다."
            );

            return;
        }

        audioSettingsManager.SetMasterVolume(value);

        UpdateVolumeText(
            masterValueText,
            value
        );
    }

    /// <summary>
    /// BGM 음량 슬라이더 변경을 처리합니다.
    /// </summary>
    public void OnBGMVolumeChanged(float value)
    {
        if (audioSettingsManager == null)
        {
            Debug.LogError(
                "[SettingsPanelUI] AudioSettingsManager가 연결되지 않았습니다."
            );

            return;
        }

        audioSettingsManager.SetBGMVolume(value);

        UpdateVolumeText(
            bgmValueText,
            value
        );
    }

    /// <summary>
    /// SFX 음량 슬라이더 변경을 처리합니다.
    /// </summary>
    public void OnSFXVolumeChanged(float value)
    {
        if (audioSettingsManager == null)
        {
            Debug.LogError(
                "[SettingsPanelUI] AudioSettingsManager가 연결되지 않았습니다."
            );

            return;
        }

        audioSettingsManager.SetSFXVolume(value);

        UpdateVolumeText(
            sfxValueText,
            value
        );
    }

    /// <summary>
    /// 전체 화면 Toggle 변경을 처리합니다.
    /// </summary>
    public void OnFullscreenChanged(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;

        PlayerPrefs.SetInt(
            FullscreenKey,
            isFullscreen ? 1 : 0
        );

        PlayerPrefs.Save();

        if (debugMode)
        {
            Debug.Log(
                $"[SettingsPanelUI] 전체 화면 변경: " +
                $"{isFullscreen}"
            );
        }
    }

    /// <summary>
    /// 환경 설정을 기본값으로 초기화합니다.
    ///
    /// 오디오:
    /// AudioSettingsManager의 Inspector 기본값 사용
    ///
    /// 전체 화면:
    /// 전체 화면 활성화
    /// </summary>
    public void ResetSettings()
    {
        if (audioSettingsManager == null)
        {
            Debug.LogError(
                "[SettingsPanelUI] AudioSettingsManager가 연결되지 않았습니다."
            );

            return;
        }

        audioSettingsManager.ResetAudioSettings();

        bool defaultFullscreen = true;

        Screen.fullScreen =
            defaultFullscreen;

        PlayerPrefs.SetInt(
            FullscreenKey,
            defaultFullscreen ? 1 : 0
        );

        PlayerPrefs.Save();

        RefreshUI();

        Debug.Log(
            "[SettingsPanelUI] 환경 설정 기본값 복원"
        );
    }

    /// <summary>
    /// 환경 설정 패널을 닫고
    /// 일시정지 메뉴로 돌아갑니다.
    ///
    /// 뒤로 버튼에서 호출합니다.
    /// </summary>
    public void BackToPauseMenu()
    {
        if (pauseManager == null)
        {
            Debug.LogError(
                "[SettingsPanelUI] PauseManager가 연결되지 않았습니다."
            );

            return;
        }

        pauseManager.ReturnToPauseMenu();
    }

    /// <summary>
    /// 현재 설정값을 Slider, Text, Toggle에 반영합니다.
    /// </summary>
    public void RefreshUI()
    {
        if (audioSettingsManager == null)
        {
            Debug.LogWarning(
                "[SettingsPanelUI] AudioSettingsManager가 연결되지 않아 " +
                "오디오 UI를 갱신할 수 없습니다."
            );

            return;
        }

        SetSliderWithoutEvent(
            masterSlider,
            audioSettingsManager.MasterVolume
        );

        SetSliderWithoutEvent(
            bgmSlider,
            audioSettingsManager.BGMVolume
        );

        SetSliderWithoutEvent(
            sfxSlider,
            audioSettingsManager.SFXVolume
        );

        UpdateVolumeText(
            masterValueText,
            audioSettingsManager.MasterVolume
        );

        UpdateVolumeText(
            bgmValueText,
            audioSettingsManager.BGMVolume
        );

        UpdateVolumeText(
            sfxValueText,
            audioSettingsManager.SFXVolume
        );

        bool savedFullscreen =
            PlayerPrefs.GetInt(
                FullscreenKey,
                Screen.fullScreen ? 1 : 0
            ) == 1;

        Screen.fullScreen =
            savedFullscreen;

        if (fullscreenToggle != null)
        {
            fullscreenToggle.SetIsOnWithoutNotify(
                savedFullscreen
            );
        }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Slider 이벤트를 발생시키지 않고
    /// 현재 값을 설정합니다.
    /// </summary>
    private void SetSliderWithoutEvent(
        Slider slider,
        float value)
    {
        if (slider == null)
        {
            return;
        }

        slider.SetValueWithoutNotify(
            Mathf.Clamp01(value)
        );
    }

    /// <summary>
    /// 0~1 값을 0~100 퍼센트 텍스트로 표시합니다.
    /// </summary>
    private void UpdateVolumeText(
        TMP_Text valueText,
        float value)
    {
        if (valueText == null)
        {
            return;
        }

        int percent =
            Mathf.RoundToInt(
                Mathf.Clamp01(value) * 100f
            );

        valueText.text =
            $"{percent}%";
    }

    #endregion

#if UNITY_EDITOR

    /// <summary>
    /// Inspector 참조가 비어 있으면
    /// 현재 씬에서 자동 탐색합니다.
    /// </summary>
    private void OnValidate()
    {
        if (audioSettingsManager == null)
        {
            audioSettingsManager =
                FindFirstObjectByType<AudioSettingsManager>();
        }

        if (pauseManager == null)
        {
            pauseManager =
                FindFirstObjectByType<PauseManager>();
        }
    }

#endif
}