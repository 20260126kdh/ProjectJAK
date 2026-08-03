using UnityEngine;

/// <summary>
/// TitleScene의 게임 종료 확인창과 종료 처리를 관리합니다.
///
/// 담당 기능:
/// - 게임 종료 확인창 열기
/// - 아니오 버튼 또는 Esc로 닫기
/// - 예 버튼으로 게임 종료
/// </summary>
public class TitleQuitController : MonoBehaviour
{
    [Header("메인 메뉴")]
    [SerializeField]
    private GameObject mainMenuPanel;

    [Header("게임 종료 확인 패널")]
    [SerializeField]
    private GameObject quitConfirmPanel;

    /// <summary>
    /// 현재 게임 종료 확인창이 열려 있는지 반환합니다.
    /// </summary>
    public bool IsQuitConfirmOpen =>
        quitConfirmPanel != null &&
        quitConfirmPanel.activeInHierarchy;

    /// <summary>
    /// 시작 시 종료 확인창을 닫습니다.
    /// </summary>
    private void Start()
    {
        CloseQuitConfirm();
    }

    /// <summary>
    /// 종료 확인창이 열린 상태에서 Esc 입력을 처리합니다.
    /// </summary>
    private void Update()
    {
        if (!IsQuitConfirmOpen)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseQuitConfirm();
        }
    }

    /// <summary>
    /// 메인 메뉴를 숨기고 게임 종료 확인창을 엽니다.
    /// 게임 종료 버튼에서 호출합니다.
    /// </summary>
    public void OpenQuitConfirm()
    {
        if (quitConfirmPanel == null)
        {
            Debug.LogError(
                "[TitleQuitController] QuitConfirmPanel이 연결되지 않았습니다."
            );

            return;
        }

        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
        }

        quitConfirmPanel.SetActive(true);

        Debug.Log(
            "[TitleQuitController] 게임 종료 확인창 열기"
        );
    }

    /// <summary>
    /// 게임 종료 확인창을 닫고 메인 메뉴로 돌아갑니다.
    /// 아니오 버튼과 Esc 입력에서 공통으로 사용합니다.
    /// </summary>
    public void CloseQuitConfirm()
    {
        if (quitConfirmPanel != null)
        {
            quitConfirmPanel.SetActive(false);
        }

        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }

        Debug.Log(
            "[TitleQuitController] 게임 종료 취소"
        );
    }

    /// <summary>
    /// 게임을 종료합니다.
    ///
    /// Unity Editor에서는 Play Mode를 종료하고,
    /// 빌드된 게임에서는 Application.Quit을 호출합니다.
    /// </summary>
    public void QuitGame()
    {
        Debug.Log(
            "[TitleQuitController] 게임 종료"
        );

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}