using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// BattleScene에서 사용하는 주요 UI 패널의 등록과 상태 확인을 관리합니다.
///
/// 각 패널의 실제 기능은 기존 UI 스크립트가 계속 담당합니다.
///
/// 예:
/// - RewardPanelUI: 보상 카드 생성 및 획득
/// - RestPanelUI: 휴식과 강화 진입
/// - StartingDeckUI: 덱 및 카드 더미 표시
/// - PauseManager: 일시정지 처리
///
/// BattleUIManager는 다음 기능만 담당합니다.
/// - 패널 종류와 GameObject 연결
/// - 패널 열림 여부 확인
/// - 다른 팝업이 열려 있는지 확인
/// - 공통 패널 열기 및 닫기
/// </summary>
public class BattleUIManager : MonoBehaviour
{
    /// <summary>
    /// 패널 종류와 실제 GameObject의 연결 정보입니다.
    /// </summary>
    [Serializable]
    private class PanelEntry
    {
        [Tooltip("등록할 UI 패널 종류")]
        public BattleUIPanelType panelType;

        [Tooltip("실제 UI 패널의 최상위 GameObject")]
        public GameObject panelObject;
    }

    [Header("Battle UI 패널 목록")]
    [SerializeField]
    private List<PanelEntry> panelEntries =
        new List<PanelEntry>();

    /// <summary>
    /// 실행 중 패널을 빠르게 조회하기 위한 사전입니다.
    /// </summary>
    private readonly Dictionary<
        BattleUIPanelType,
        GameObject
    > panelDictionary =
        new Dictionary<
            BattleUIPanelType,
            GameObject
        >();

    /// <summary>
    /// 현재 등록된 패널 정보를 사전에 구성합니다.
    /// </summary>
    private void Awake()
    {
        BuildPanelDictionary();
    }

    /// <summary>
    /// Inspector에 등록된 패널 목록을 바탕으로
    /// 패널 조회용 Dictionary를 생성합니다.
    /// </summary>
    private void BuildPanelDictionary()
    {
        panelDictionary.Clear();

        if (panelEntries == null ||
            panelEntries.Count == 0)
        {
            Debug.LogWarning(
                "[BattleUIManager] 등록된 UI 패널이 없습니다."
            );

            return;
        }

        for (int i = 0;
             i < panelEntries.Count;
             i++)
        {
            PanelEntry entry =
                panelEntries[i];

            if (entry == null)
            {
                continue;
            }

            if (entry.panelType ==
                BattleUIPanelType.None)
            {
                Debug.LogWarning(
                    $"[BattleUIManager] {i}번 패널 항목의 " +
                    "Panel Type이 None입니다."
                );

                continue;
            }

            if (entry.panelObject == null)
            {
                Debug.LogWarning(
                    $"[BattleUIManager] " +
                    $"{entry.panelType} 패널 오브젝트가 " +
                    "연결되지 않았습니다."
                );

                continue;
            }

            if (panelDictionary.ContainsKey(
                entry.panelType
            ))
            {
                Debug.LogWarning(
                    $"[BattleUIManager] " +
                    $"{entry.panelType} 패널이 중복 등록되었습니다. " +
                    "먼저 등록된 항목을 유지합니다."
                );

                continue;
            }

            panelDictionary.Add(
                entry.panelType,
                entry.panelObject
            );
        }

        Debug.Log(
            $"[BattleUIManager] UI 패널 등록 완료: " +
            $"{panelDictionary.Count}개"
        );
    }

    /// <summary>
    /// 지정한 패널을 표시합니다.
    ///
    /// 패널의 고유 초기화가 필요한 경우에는
    /// 기존 UI 스크립트의 Show 메서드를 사용해야 합니다.
    ///
    /// 예:
    /// RewardPanelUI.ShowRewardPanel()
    /// RestPanelUI.ShowRestPanel()
    /// StartingDeckUI.ShowCurrentDeck()
    /// </summary>
    public void OpenPanel(
        BattleUIPanelType panelType)
    {
        GameObject panel =
            GetPanelObject(panelType);

        if (panel == null)
        {
            return;
        }

        panel.SetActive(true);

        Debug.Log(
            $"[BattleUIManager] 패널 열기: " +
            $"{panelType}"
        );
    }

    /// <summary>
    /// 지정한 패널을 숨깁니다.
    ///
    /// 패널을 닫을 때 데이터 정리나 상태 변경이 필요한 경우에는
    /// 해당 UI 스크립트의 기존 닫기 메서드를 사용해야 합니다.
    /// </summary>
    public void ClosePanel(
        BattleUIPanelType panelType)
    {
        GameObject panel =
            GetPanelObject(panelType);

        if (panel == null)
        {
            return;
        }

        panel.SetActive(false);

        Debug.Log(
            $"[BattleUIManager] 패널 닫기: " +
            $"{panelType}"
        );
    }

    /// <summary>
    /// 지정한 패널의 활성 상태를 전환합니다.
    /// 단순한 표시 전환이 필요한 패널에서만 사용합니다.
    /// </summary>
    public void TogglePanel(
        BattleUIPanelType panelType)
    {
        GameObject panel =
            GetPanelObject(panelType);

        if (panel == null)
        {
            return;
        }

        bool shouldOpen =
            !panel.activeSelf;

        panel.SetActive(shouldOpen);

        Debug.Log(
            $"[BattleUIManager] 패널 전환: " +
            $"{panelType} / " +
            $"활성화: {shouldOpen}"
        );
    }

    /// <summary>
    /// 지정한 패널이 현재 열려 있는지 반환합니다.
    /// 부모 오브젝트가 비활성화된 경우까지 포함하기 위해
    /// activeInHierarchy를 사용합니다.
    /// </summary>
    public bool IsPanelOpen(
        BattleUIPanelType panelType)
    {
        GameObject panel =
            GetPanelObject(
                panelType,
                false
            );

        return panel != null &&
               panel.activeInHierarchy;
    }

    /// <summary>
    /// 지정한 패널을 제외하고
    /// 등록된 다른 패널이 하나라도 열려 있는지 반환합니다.
    ///
    /// Pause 패널을 열기 전에
    /// 다른 팝업이 열려 있는지 검사할 때 사용합니다.
    /// </summary>
    public bool IsAnyPanelOpenExcept(
        BattleUIPanelType excludedPanelType)
    {
        foreach (
            KeyValuePair<
                BattleUIPanelType,
                GameObject
            > pair in panelDictionary
        )
        {
            if (pair.Key ==
                excludedPanelType)
            {
                continue;
            }

            if (pair.Value == null)
            {
                continue;
            }

            if (pair.Value.activeInHierarchy)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 등록된 주요 UI 패널 중
    /// 하나라도 열려 있는지 반환합니다.
    /// </summary>
    public bool IsAnyPanelOpen()
    {
        foreach (
            KeyValuePair<
                BattleUIPanelType,
                GameObject
            > pair in panelDictionary
        )
        {
            if (pair.Value == null)
            {
                continue;
            }

            if (pair.Value.activeInHierarchy)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 전달한 종류들 중
    /// 하나라도 열린 패널이 있는지 반환합니다.
    /// </summary>
    public bool IsAnyPanelOpen(
        params BattleUIPanelType[] panelTypes)
    {
        if (panelTypes == null ||
            panelTypes.Length == 0)
        {
            return false;
        }

        for (int i = 0;
             i < panelTypes.Length;
             i++)
        {
            if (IsPanelOpen(
                panelTypes[i]
            ))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 모든 등록 패널을 닫습니다.
    ///
    /// 주의:
    /// 각 UI의 상태 초기화 메서드를 호출하지 않고
    /// GameObject만 비활성화합니다.
    /// 씬 전환이나 강제 UI 초기화 상황에서만 사용합니다.
    /// </summary>
    public void CloseAllPanels()
    {
        foreach (
            KeyValuePair<
                BattleUIPanelType,
                GameObject
            > pair in panelDictionary
        )
        {
            if (pair.Value == null)
            {
                continue;
            }

            pair.Value.SetActive(false);
        }

        Debug.Log(
            "[BattleUIManager] 모든 등록 패널 닫기"
        );
    }

    /// <summary>
    /// 지정한 패널 종류에 연결된 GameObject를 반환합니다.
    /// </summary>
    public GameObject GetPanel(
        BattleUIPanelType panelType)
    {
        return GetPanelObject(panelType);
    }

    /// <summary>
    /// Dictionary에서 패널 오브젝트를 조회합니다.
    /// </summary>
    private GameObject GetPanelObject(
        BattleUIPanelType panelType,
        bool logWarning = true)
    {
        if (panelType ==
            BattleUIPanelType.None)
        {
            if (logWarning)
            {
                Debug.LogWarning(
                    "[BattleUIManager] None 타입의 패널은 " +
                    "조회할 수 없습니다."
                );
            }

            return null;
        }

        if (panelDictionary.TryGetValue(
            panelType,
            out GameObject panel
        ))
        {
            return panel;
        }

        if (logWarning)
        {
            Debug.LogWarning(
                $"[BattleUIManager] " +
                $"{panelType} 패널이 등록되지 않았습니다."
            );
        }

        return null;
    }

#if UNITY_EDITOR

    /// <summary>
    /// Inspector에서 패널 종류가 중복 등록되었는지 확인합니다.
    /// </summary>
    private void OnValidate()
    {
        if (panelEntries == null)
        {
            return;
        }

        HashSet<BattleUIPanelType>
            registeredTypes =
                new HashSet<BattleUIPanelType>();

        for (int i = 0;
             i < panelEntries.Count;
             i++)
        {
            PanelEntry entry =
                panelEntries[i];

            if (entry == null ||
                entry.panelType ==
                BattleUIPanelType.None)
            {
                continue;
            }

            if (!registeredTypes.Add(
                entry.panelType
            ))
            {
                Debug.LogWarning(
                    $"[BattleUIManager] " +
                    $"{entry.panelType} 패널이 " +
                    "Inspector에 중복 등록되어 있습니다.",
                    this
                );
            }
        }
    }

#endif
}