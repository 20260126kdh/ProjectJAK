using UnityEngine;

/// <summary>
/// 클래스 선택 씬의 키보드 단축키 입력을 관리합니다.
///
/// 단축키
/// - 클래스 목록 화면에서 Esc: 타이틀 씬으로 이동
/// - 클래스 상세 화면에서 Esc: 상세 패널 닫기
/// - 클래스 상세 화면에서 Enter: 클래스 선택 확정
/// </summary>
public class ClassSelectShortcutController : MonoBehaviour
{
    [Header("Class Select Manager")]
    [SerializeField]
    private ClassSelectManager classSelectManager;

    /// <summary>
    /// 매 프레임 클래스 선택 씬의 단축키 입력을 확인합니다.
    /// </summary>
    private void Update()
    {
        if (classSelectManager == null)
        {
            return;
        }

        HandleEscapeInput();
        HandleClassSelectionInput();
        HandleConfirmInput();
    }

    /// <summary>
    /// 숫자키 1, 2, 3으로 클래스를 선택합니다.
    /// </summary>
    private void HandleClassSelectionInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) ||
            Input.GetKeyDown(KeyCode.Keypad1))
        {
            classSelectManager.SelectPhysique();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) ||
                 Input.GetKeyDown(KeyCode.Keypad2))
        {
            classSelectManager.SelectTechnician();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) ||
                 Input.GetKeyDown(KeyCode.Keypad3))
        {
            classSelectManager.SelectCaptain();
        }
    }

    /// <summary>
    /// Esc 입력을 처리합니다.
    ///
    /// 상세 패널이 열려 있으면 상세 패널을 닫고,
    /// 상세 패널이 닫혀 있으면 타이틀 씬으로 이동합니다.
    /// </summary>
    private void HandleEscapeInput()
    {
        if (!Input.GetKeyDown(KeyCode.Escape))
        {
            return;
        }

        if (classSelectManager.IsDetailPanelOpen)
        {
            classSelectManager.CloseDetailPanel();
            return;
        }

        classSelectManager.ReturnToTitle();
    }

    /// <summary>
    /// Enter 입력을 처리합니다.
    ///
    /// 클래스 상세 패널이 열려 있고
    /// 클래스 선택을 확정할 수 있을 때만 전투 씬으로 이동합니다.
    /// </summary>
    private void HandleConfirmInput()
    {
        bool enterPressed =
            Input.GetKeyDown(KeyCode.Return)
            || Input.GetKeyDown(KeyCode.KeypadEnter);

        if (!enterPressed)
        {
            return;
        }

        if (!classSelectManager.CanConfirmSelection)
        {
            return;
        }

        classSelectManager.ConfirmSelection();
    }

#if UNITY_EDITOR

    /// <summary>
    /// Inspector에서 참조 누락 여부를 확인합니다.
    /// </summary>
    private void OnValidate()
    {
        if (classSelectManager == null)
        {
            classSelectManager = FindFirstObjectByType<ClassSelectManager>();
        }
    }

#endif
}
