using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 타락한 계시 선택 패널을 관리합니다.
/// 패널 표시, 계시 선택 버튼, 중복 선택 차단을 담당합니다.
/// </summary>
public class HRevelationPanelUI : MonoBehaviour
{
    [Header("계시 선택 패널")]
    [SerializeField]
    private GameObject revelationPanel;

    [Header("익사한 기억 버튼")]
    [SerializeField]
    private Button drownedMemoryButton;

    [Header("심해의 속삭임 버튼")]
    [SerializeField]
    private Button deepWhisperButton;

    [Header("무너진 의지 버튼")]
    [SerializeField]
    private Button brokenWillButton;

    /// <summary>
    /// 현재 계시를 관리하는 컨트롤러입니다.
    /// 패널을 열 때 전달받습니다.
    /// </summary>
    private HRevelationController currentController;

    /// <summary>
    /// 계시 선택 패널이 현재 열려 있는지 반환합니다.
    /// </summary>
    public bool IsPanelOpen =>
        revelationPanel != null &&
        revelationPanel.activeSelf;

    private void Awake()
    {
        HidePanel();
    }

    /// <summary>
    /// 전달받은 HRevelationController를 기준으로
    /// 계시 선택 패널을 표시합니다.
    /// </summary>
    public void ShowPanel(
        HRevelationController controller)
    {
        if (controller == null)
        {
            Debug.LogWarning(
                "[HRevelationPanelUI] " +
                "HRevelationController가 없습니다."
            );

            return;
        }

        if (controller.HasSelectedAllRevelations)
        {
            Debug.Log(
                "[HRevelationPanelUI] " +
                "모든 계시가 이미 선택되어 패널을 열지 않습니다."
            );

            return;
        }

        if (revelationPanel == null)
        {
            Debug.LogError(
                "[HRevelationPanelUI] " +
                "Revelation Panel이 연결되지 않았습니다."
            );

            return;
        }

        currentController = controller;

        RefreshButtonStates();

        revelationPanel.SetActive(true);

        Debug.Log(
            "[HRevelationPanelUI] 계시 선택 패널 표시"
        );
    }

    /// <summary>
    /// 계시 선택 패널을 숨깁니다.
    /// </summary>
    public void HidePanel()
    {
        if (revelationPanel != null)
        {
            revelationPanel.SetActive(false);
        }

        currentController = null;
    }

    /// <summary>
    /// 익사한 기억 버튼에서 호출합니다.
    /// </summary>
    public void OnClickDrownedMemory()
    {
        SelectRevelation(
            HRevelationType.DrownedMemory
        );
    }

    /// <summary>
    /// 심해의 속삭임 버튼에서 호출합니다.
    /// </summary>
    public void OnClickDeepWhisper()
    {
        SelectRevelation(
            HRevelationType.DeepWhisper
        );
    }

    /// <summary>
    /// 무너진 의지 버튼에서 호출합니다.
    /// </summary>
    public void OnClickBrokenWill()
    {
        SelectRevelation(
            HRevelationType.BrokenWill
        );
    }

    /// <summary>
    /// 선택한 계시를 현재 컨트롤러에 등록합니다.
    /// 선택 성공 시 패널을 닫습니다.
    /// </summary>
    private void SelectRevelation(
        HRevelationType revelationType)
    {
        if (currentController == null)
        {
            Debug.LogWarning(
                "[HRevelationPanelUI] " +
                "현재 연결된 HRevelationController가 없습니다."
            );

            return;
        }

        bool wasSelected =
            currentController.SelectRevelation(
                revelationType
            );

        if (!wasSelected)
        {
            RefreshButtonStates();
            return;
        }

        Debug.Log(
            $"[HRevelationPanelUI] 계시 선택 완료 : " +
            $"{revelationType}"
        );

        HidePanel();
    }

    /// <summary>
    /// 이미 선택된 계시의 버튼을 비활성화합니다.
    /// </summary>
    private void RefreshButtonStates()
    {
        if (currentController == null)
        {
            return;
        }

        if (drownedMemoryButton != null)
        {
            drownedMemoryButton.interactable =
                !currentController.HasRevelation(
                    HRevelationType.DrownedMemory
                );
        }

        if (deepWhisperButton != null)
        {
            deepWhisperButton.interactable =
                !currentController.HasRevelation(
                    HRevelationType.DeepWhisper
                );
        }

        if (brokenWillButton != null)
        {
            brokenWillButton.interactable =
                !currentController.HasRevelation(
                    HRevelationType.BrokenWill
                );
        }
    }
}