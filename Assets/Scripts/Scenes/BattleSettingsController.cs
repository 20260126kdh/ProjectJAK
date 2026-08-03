using UnityEngine;

/// <summary>
/// BattleScene의 환경 설정 패널 화면 전환을 관리합니다.
///
/// 담당 기능:
/// - 환경 설정 패널에서 Pause 메뉴로 복귀
///
/// 실제 음량, 전체화면, 초기화 기능은
/// SettingsPanelUI가 담당합니다.
/// </summary>
public class BattleSettingsController : MonoBehaviour
{
    [Header("Pause Manager")]
    [SerializeField]
    private PauseManager pauseManager;

    /// <summary>
    /// 환경 설정 패널을 닫고
    /// Pause 메뉴로 돌아갑니다.
    ///
    /// SettingsPanel의 Back 버튼에서 호출합니다.
    /// </summary>
    public void BackToPauseMenu()
    {
        if (pauseManager == null)
        {
            Debug.LogError(
                "[BattleSettingsController] " +
                "PauseManager가 연결되지 않았습니다."
            );

            return;
        }

        pauseManager.ReturnToPauseMenu();
    }

#if UNITY_EDITOR

    /// <summary>
    /// Inspector 참조가 비어 있으면
    /// 현재 씬에서 PauseManager를 자동으로 찾습니다.
    /// </summary>
    private void OnValidate()
    {
        if (pauseManager == null)
        {
            pauseManager =
                FindFirstObjectByType<PauseManager>();
        }
    }

#endif
}