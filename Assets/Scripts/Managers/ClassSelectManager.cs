using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 클래스 선택 씬을 관리하는 매니저입니다.
/// 플레이어의 클래스 선택과 상세 정보 패널을 관리합니다.
/// </summary>
public class ClassSelectManager : MonoBehaviour
{
    [Header("클래스 상세 정보 패널")]
    [SerializeField]
    private GameObject classDetailPanel;

    [Header("클래스 데이터")]
    [SerializeField]
    private ClassInfo[] classInfos;

    [Header("UI")]
    [SerializeField]
    private Image characterImage;

    [Header("버튼")]
    [SerializeField]
    private Button confirmButton;

    [SerializeField]
    private TMP_Text classNameText;

    [SerializeField]
    private TMP_Text hpText;

    [SerializeField]
    private TMP_Text passiveText;

    [SerializeField]
    private TMP_Text descriptionText;



    /// <summary>
    /// 현재 선택한 클래스
    /// </summary>
    private PlayerClass selectedClass = PlayerClass.None;

    private ClassInfo selectedClassInfo;

    public void SelectPhysique()
    {
        SelectClass(PlayerClass.Physique);
    }

    public void SelectTechnician()
    {
        SelectClass(PlayerClass.Technician);
    }

    public void SelectCaptain()
    {
        SelectClass(PlayerClass.Captain);
    }

    /// <summary>
    /// 시작 시 상세 패널을 숨깁니다.
    /// </summary>
    private void Start()
    {
        classDetailPanel.SetActive(false);

        confirmButton.interactable = false;
    }

    /// <summary>
    /// 클래스 버튼을 눌렀을 때 호출됩니다.
    /// 선택한 클래스를 저장하고 상세 패널을 표시합니다.
    /// </summary>
    /// <param name="playerClass">선택한 클래스</param>
    public void SelectClass(PlayerClass playerClass)
    {
        selectedClass = playerClass;

        ShowClassInfo(playerClass);

        OpenDetailPanel();

        confirmButton.interactable = true;
    }

    /// <summary>
    /// 클래스 상세 패널을 표시합니다.
    /// </summary>
    private void OpenDetailPanel()
    {
        classDetailPanel.SetActive(true);
    }

    /// <summary>
    /// 클래스 상세 패널을 닫습니다.
    /// </summary>
    public void CloseDetailPanel()
    {
        classDetailPanel.SetActive(false);
    }

    /// <summary>
    /// 현재 선택된 클래스를 반환합니다.
    /// </summary>
    public PlayerClass GetSelectedClass()
    {
        return selectedClass;
    }

    private void ShowClassInfo(PlayerClass playerClass)
    {
        foreach (ClassInfo info in classInfos)
        {
            if (info.playerClass != playerClass)
                continue;

            selectedClassInfo = info;

            characterImage.sprite = info.classImage;

            classNameText.text = info.className;

            hpText.text = $"체력 : {info.maxHP}";

            passiveText.text = info.passiveDescription;

            descriptionText.text = info.description;

            return;
        }

        Debug.LogWarning($"{playerClass} 정보를 찾을 수 없습니다.");
    }

    /// <summary>
    /// 클래스 선택을 확정합니다.
    /// (다음 단계에서 구현 예정)
    /// </summary>
    public void ConfirmSelection()
    {
        Debug.Log("===== Confirm =====");

        Debug.Log($"GameManager : {GameManager.Instance}");

        Debug.Log($"PlayerData : {GameManager.Instance.PlayerData}");

        Debug.Log($"선택 클래스 : {selectedClass}");

        GameManager.Instance.PlayerData.SetClass(selectedClass);

        Debug.Log($"저장 후 클래스 : {GameManager.Instance.PlayerData.PlayerClass}");

        GameManager.Instance.PlayerData.SetHP(selectedClassInfo.maxHP);

        SceneManager.LoadScene("BattleScene");
    }
}