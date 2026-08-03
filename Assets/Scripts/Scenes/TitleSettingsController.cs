using UnityEngine;

/// <summary>
/// TitleScene의 환경 설정 패널 전환을 관리합니다.
///
/// 타이틀 메뉴 버튼은 환경 설정 화면이 열려 있어도
/// 계속 표시된 상태를 유지합니다.
///
/// 실제 음량, 전체 화면, 초기화 기능은
/// SettingsPanelUI가 담당합니다.
/// </summary>
public class TitleSettingsController : MonoBehaviour
{
    [Header("환경 설정 패널")]
    [SerializeField]
    private GameObject settingsPanel;

    /// <summary>
    /// 현재 환경 설정 패널이 열려 있는지 반환합니다.
    /// </summary>
    public bool IsSettingsOpen =>
        settingsPanel != null &&
        settingsPanel.activeInHierarchy;

    /// <summary>
    /// 시작 시 환경 설정 패널을 닫습니다.
    /// 타이틀 메뉴 버튼은 그대로 표시합니다.
    /// </summary>
    private void Start()
    {
        CloseSettings();
    }

    /// <summary>
    /// 환경 설정 패널이 열린 상태에서
    /// Esc를 누르면 패널을 닫습니다.
    /// </summary>
    private void Update()
    {
        if (!IsSettingsOpen)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseSettings();
        }
    }

    /// <summary>
    /// 타이틀 메뉴 버튼을 유지한 상태로
    /// 환경 설정 패널을 표시합니다.
    ///
    /// 환경 설정 버튼에서 호출합니다.
    /// </summary>
    public void OpenSettings()
    {
        if (settingsPanel == null)
        {
            Debug.LogError(
                "[TitleSettingsController] " +
                "SettingsPanel이 연결되지 않았습니다."
            );

            return;
        }

        settingsPanel.SetActive(true);

        Debug.Log(
            "[TitleSettingsController] 환경 설정 열기"
        );
    }

    /// <summary>
    /// 환경 설정 패널만 닫습니다.
    ///
    /// 타이틀 메뉴 버튼은 처음부터 비활성화하지 않았으므로
    /// 별도로 다시 활성화할 필요가 없습니다.
    ///
    /// 뒤로 버튼과 Esc 입력에서 공통으로 사용합니다.
    /// </summary>
    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        Debug.Log(
            "[TitleSettingsController] 환경 설정 닫기"
        );
    }

    /// <summary>
    /// 기존 Button 연결과의 호환성을 위해 유지합니다.
    /// 기존 ShowMainMenu 연결을 바로 바꾸지 않아도
    /// 환경 설정 패널만 닫도록 처리합니다.
    /// </summary>
    public void ShowMainMenu()
    {
        CloseSettings();
    }
}