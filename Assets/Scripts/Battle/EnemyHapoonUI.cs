using TMPro;
using UnityEngine;

/// <summary>
/// 적에게 적용된 현재 작살 스택을 표시합니다.
///
/// 작살 스택이 1 이상이면 HarpoonRoot를 표시하고,
/// 0이면 HarpoonRoot만 숨깁니다.
///
/// 부모 오브젝트에서 HarpoonStackController를 자동으로 찾습니다.
/// </summary>
public class EnemyHarpoonUI : MonoBehaviour
{
    [Header("작살 UI 루트")]
    [SerializeField]
    private GameObject harpoonRoot;

    [Header("작살 스택 숫자 텍스트")]
    [SerializeField]
    private TMP_Text stackText;

    private HarpoonStackController harpoonStackController;

    private void Awake()
    {
        harpoonStackController =
            GetComponentInParent<HarpoonStackController>();

        if (harpoonStackController == null)
        {
            Debug.LogWarning(
                "[EnemyHarpoonUI] 부모 오브젝트에서 " +
                "HarpoonStackController를 찾지 못했습니다.",
                this
            );
        }
    }

    private void OnEnable()
    {
        Subscribe();
        RefreshStack();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    /// <summary>
    /// 작살 스택 변경 이벤트를 연결합니다.
    /// </summary>
    private void Subscribe()
    {
        if (harpoonStackController == null)
        {
            return;
        }

        harpoonStackController.HarpoonStackChanged -=
            OnHarpoonStackChanged;

        harpoonStackController.HarpoonStackChanged +=
            OnHarpoonStackChanged;
    }

    /// <summary>
    /// 작살 스택 변경 이벤트 연결을 해제합니다.
    /// </summary>
    private void Unsubscribe()
    {
        if (harpoonStackController == null)
        {
            return;
        }

        harpoonStackController.HarpoonStackChanged -=
            OnHarpoonStackChanged;
    }

    /// <summary>
    /// 현재 작살 스택을 직접 읽어 초기 표시를 갱신합니다.
    /// </summary>
    private void RefreshStack()
    {
        if (harpoonStackController == null)
        {
            SetHarpoonRootActive(false);
            return;
        }

        OnHarpoonStackChanged(
            harpoonStackController.CurrentHarpoonStack
        );
    }

    /// <summary>
    /// 작살 스택 표시를 갱신합니다.
    /// 스택이 없으면 숨기고, 있으면 가장 왼쪽에 표시합니다.
    /// </summary>
    private void OnHarpoonStackChanged(
        int currentStack)
    {
        bool shouldShow =
            currentStack > 0;

        SetHarpoonRootActive(
            shouldShow
        );

        if (!shouldShow)
        {
            return;
        }

        if (harpoonRoot != null)
        {
            harpoonRoot.transform.SetAsFirstSibling();
        }

        if (stackText != null)
        {
            stackText.text =
                currentStack.ToString();
        }
    }

    /// <summary>
    /// 작살 UI 루트의 활성화 상태를 변경합니다.
    /// </summary>
    private void SetHarpoonRootActive(
        bool isActive)
    {
        if (harpoonRoot == null)
        {
            return;
        }

        if (harpoonRoot.activeSelf == isActive)
        {
            return;
        }

        harpoonRoot.SetActive(
            isActive
        );
    }
}