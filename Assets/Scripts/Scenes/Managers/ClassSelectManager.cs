using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 클래스 선택 씬의 클래스 선택과 상세 정보 UI를 관리합니다.
///
/// 담당 기능
/// - 클래스 선택
/// - 클래스 상세 정보 표시
/// - 상세 정보 패널 열기 및 닫기
/// - 클래스 선택 확정
/// - 타이틀 씬으로 이동
/// </summary>
public class ClassSelectManager : MonoBehaviour
{
    [Header("Scene 설정")]
    [Tooltip("메인 타이틀 씬의 이름입니다.")]
    [SerializeField]
    private string titleSceneName = "TitleScene";

    [Tooltip("전투 씬의 이름입니다.")]
    [SerializeField]
    private string battleSceneName = "BattleScene";

    [Header("클래스 상세 정보 패널")]
    [SerializeField]
    private GameObject classDetailPanel;

    [Header("클래스 정보들")]
    [SerializeField]
    private ClassInfo[] classInfos;

    [Header("클래스 이미지")]
    [SerializeField]
    private Image characterImage;

    [Header("버튼")]
    [SerializeField]
    private Button confirmButton;

    [Header("텍스트")]
    [SerializeField]
    private TMP_Text classNameText;

    [SerializeField]
    private TMP_Text hpText;

    [SerializeField]
    private TMP_Text passiveText;

    [SerializeField]
    private TMP_Text descriptionText;

    /// <summary>
    /// 현재 선택된 클래스입니다.
    /// </summary>
    private PlayerClass selectedClass = PlayerClass.None;

    /// <summary>
    /// 현재 선택된 클래스의 상세 정보입니다.
    /// </summary>
    private ClassInfo selectedClassInfo;

    /// <summary>
    /// 현재 선택된 클래스를 반환합니다.
    /// </summary>
    public PlayerClass SelectedClass => selectedClass;

    /// <summary>
    /// 클래스 상세 정보 패널이 열려 있는지 반환합니다.
    /// </summary>
    public bool IsDetailPanelOpen
    {
        get
        {
            return classDetailPanel != null && classDetailPanel.activeSelf;
        }
    }

    /// <summary>
    /// 현재 클래스 선택을 확정할 수 있는 상태인지 반환합니다.
    /// </summary>
    public bool CanConfirmSelection
    {
        get
        {
            return IsDetailPanelOpen
                && selectedClass != PlayerClass.None
                && selectedClassInfo != null;
        }
    }

    /// <summary>
    /// 시작 시 상세 패널과 선택 상태를 초기화합니다.
    /// </summary>
    private void Start()
    {
        ResetSelection();
    }

    /// <summary>
    /// 피지크 클래스를 선택합니다.
    /// Unity Button의 On Click에서 사용합니다.
    /// </summary>
    public void SelectPhysique()
    {
        SelectClass(PlayerClass.Physique);
    }

    /// <summary>
    /// 테크니션 클래스를 선택합니다.
    /// Unity Button의 On Click에서 사용합니다.
    /// </summary>
    public void SelectTechnician()
    {
        SelectClass(PlayerClass.Technician);
    }

    /// <summary>
    /// 캡틴 클래스를 선택합니다.
    /// Unity Button의 On Click에서 사용합니다.
    /// </summary>
    public void SelectCaptain()
    {
        SelectClass(PlayerClass.Captain);
    }

    /// <summary>
    /// 클래스를 선택하고 상세 정보 패널을 표시합니다.
    /// </summary>
    /// <param name="playerClass">선택할 플레이어 클래스</param>
    public void SelectClass(PlayerClass playerClass)
    {
        if (playerClass == PlayerClass.None || playerClass == PlayerClass.All)
        {
            Debug.LogWarning(
                $"[ClassSelectManager] 선택할 수 없는 클래스입니다: {playerClass}"
            );

            return;
        }

        ClassInfo classInfo = FindClassInfo(playerClass);

        if (classInfo == null)
        {
            Debug.LogError(
                $"[ClassSelectManager] {playerClass} 클래스 정보를 찾을 수 없습니다."
            );

            return;
        }

        selectedClass = playerClass;
        selectedClassInfo = classInfo;

        ShowClassInfo(classInfo);
        OpenDetailPanel();
        SetConfirmButtonInteractable(true);
    }

    /// <summary>
    /// 클래스 상세 정보 패널을 표시합니다.
    /// </summary>
    public void OpenDetailPanel()
    {
        if (classDetailPanel == null)
        {
            Debug.LogError(
                "[ClassSelectManager] Class Detail Panel이 연결되지 않았습니다."
            );

            return;
        }

        classDetailPanel.SetActive(true);
    }

    /// <summary>
    /// 클래스 상세 정보 패널을 닫고 선택 상태를 초기화합니다.
    /// Back 버튼과 Esc 단축키에서 공통으로 사용합니다.
    /// </summary>
    public void CloseDetailPanel()
    {
        ResetSelection();
    }

    /// <summary>
    /// 현재 선택된 클래스를 반환합니다.
    /// 기존 코드와의 호환을 위해 유지합니다.
    /// </summary>
    public PlayerClass GetSelectedClass()
    {
        return selectedClass;
    }

    /// <summary>
    /// 클래스 선택을 확정하고 전투 씬으로 이동합니다.
    /// Confirm 버튼과 Enter 단축키에서 공통으로 사용합니다.
    /// </summary>
    public void ConfirmSelection()
    {
        if (!CanConfirmSelection)
        {
            Debug.LogWarning(
                "[ClassSelectManager] 확정할 수 있는 클래스가 선택되지 않았습니다."
            );

            return;
        }

        if (GameManager.Instance == null)
        {
            Debug.LogError(
                "[ClassSelectManager] GameManager.Instance를 찾을 수 없습니다."
            );

            return;
        }

        if (GameManager.Instance.PlayerData == null)
        {
            Debug.LogError(
                "[ClassSelectManager] GameManager에 PlayerData가 연결되지 않았습니다."
            );

            return;
        }

        if (string.IsNullOrWhiteSpace(battleSceneName))
        {
            Debug.LogError(
                "[ClassSelectManager] Battle Scene Name이 비어 있습니다."
            );

            return;
        }

        GameManager.Instance.PlayerData.SetClass(selectedClass);
        GameManager.Instance.PlayerData.SetHP(selectedClassInfo.maxHP);

        Debug.Log(
            $"[ClassSelectManager] 클래스 선택 확정: {selectedClass}, " +
            $"최대 체력: {selectedClassInfo.maxHP}"
        );

        SceneManager.LoadScene(battleSceneName);
    }

    /// <summary>
    /// 메인 타이틀 씬으로 이동합니다.
    /// 클래스 목록 화면의 Esc 단축키에서 사용합니다.
    /// </summary>
    public void ReturnToTitle()
    {
        if (string.IsNullOrWhiteSpace(titleSceneName))
        {
            Debug.LogError(
                "[ClassSelectManager] Title Scene Name이 비어 있습니다."
            );

            return;
        }

        SceneManager.LoadScene(titleSceneName);
    }

    /// <summary>
    /// 지정한 클래스의 정보를 찾습니다.
    /// </summary>
    /// <param name="playerClass">찾을 플레이어 클래스</param>
    /// <returns>찾은 클래스 정보, 없으면 null</returns>
    private ClassInfo FindClassInfo(PlayerClass playerClass)
    {
        if (classInfos == null || classInfos.Length == 0)
        {
            Debug.LogError(
                "[ClassSelectManager] Class Infos가 비어 있습니다."
            );

            return null;
        }

        foreach (ClassInfo info in classInfos)
        {
            if (info == null)
            {
                continue;
            }

            if (info.playerClass == playerClass)
            {
                return info;
            }
        }

        return null;
    }

    /// <summary>
    /// 선택한 클래스 정보를 UI에 표시합니다.
    /// </summary>
    /// <param name="classInfo">표시할 클래스 정보</param>
    private void ShowClassInfo(ClassInfo classInfo)
    {
        if (classInfo == null)
        {
            Debug.LogError(
                "[ClassSelectManager] 표시할 ClassInfo가 없습니다."
            );

            return;
        }

        if (characterImage != null)
        {
            characterImage.sprite = classInfo.classImage;
        }

        if (classNameText != null)
        {
            classNameText.text = classInfo.className;
        }

        if (hpText != null)
        {
            hpText.text = $"{classInfo.maxHP} / {classInfo.maxHP}";
        }

        if (passiveText != null)
        {
            passiveText.text = classInfo.passiveDescription;
        }

        if (descriptionText != null)
        {
            descriptionText.text = classInfo.description;
        }
    }

    /// <summary>
    /// 클래스 선택 상태와 상세 패널을 초기화합니다.
    /// </summary>
    private void ResetSelection()
    {
        selectedClass = PlayerClass.None;
        selectedClassInfo = null;

        if (classDetailPanel != null)
        {
            classDetailPanel.SetActive(false);
        }

        SetConfirmButtonInteractable(false);
    }

    /// <summary>
    /// Confirm 버튼의 활성화 상태를 설정합니다.
    /// </summary>
    /// <param name="isInteractable">버튼 활성화 여부</param>
    private void SetConfirmButtonInteractable(bool isInteractable)
    {
        if (confirmButton != null)
        {
            confirmButton.interactable = isInteractable;
        }
    }
}