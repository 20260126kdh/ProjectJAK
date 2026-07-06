using UnityEngine;

/// <summary>
/// 휴식 단계에서 표시되는 임시 Rest 패널입니다.
/// 현재는 표시/숨김만 담당합니다.
/// </summary>
public class RestPanelUI : MonoBehaviour
{
    [Header("Rest Panel")]
    [SerializeField]
    private GameObject restPanel;

    private void Awake()
    {
        HideRestPanel();
    }

    public void ShowRestPanel()
    {
        if (restPanel == null)
        {
            Debug.LogError("[RestPanelUI] Rest Panel이 연결되지 않았습니다.");
            return;
        }

        restPanel.SetActive(true);

        Debug.Log("[RestPanelUI] 휴식 패널 표시");
    }

    public void HideRestPanel()
    {
        if (restPanel == null)
            return;

        restPanel.SetActive(false);
    }
}