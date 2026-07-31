using UnityEngine;

/// <summary>
/// 전투 씬의 키보드 단축키 입력을 관리합니다.
///
/// 현재 구현된 단축키
/// - 1: 첫 번째 손패 카드 선택
/// - 2: 두 번째 손패 카드 선택
/// - 3: 세 번째 손패 카드 선택
/// - 4: 네 번째 손패 카드 선택
///
/// 같은 번호를 다시 누르면
/// 기존 카드 선택 로직에 따라 선택이 해제됩니다.
/// </summary>
public class BattleShortcutController : MonoBehaviour
{
    [Header("Hand Manager")]
    [SerializeField]
    private HandManager handManager;

    /// <summary>
    /// 매 프레임 전투 단축키 입력을 확인합니다.
    /// </summary>
    private void Update()
    {
        if (handManager == null)
        {
            return;
        }

        HandleCardSelectionInput();
        HandleEndTurnInput();
    }

    /// <summary>
    /// 숫자키 1~4를 이용한 손패 카드 선택을 처리합니다.
    ///
    /// 키보드 상단 숫자키와 숫자패드를 모두 지원합니다.
    /// </summary>
    private void HandleCardSelectionInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) ||
            Input.GetKeyDown(KeyCode.Keypad1))
        {
            handManager.SelectCardByIndex(0);
            return;
        }

        if (Input.GetKeyDown(KeyCode.Alpha2) ||
            Input.GetKeyDown(KeyCode.Keypad2))
        {
            handManager.SelectCardByIndex(1);
            return;
        }

        if (Input.GetKeyDown(KeyCode.Alpha3) ||
            Input.GetKeyDown(KeyCode.Keypad3))
        {
            handManager.SelectCardByIndex(2);
            return;
        }

        if (Input.GetKeyDown(KeyCode.Alpha4) ||
            Input.GetKeyDown(KeyCode.Keypad4))
        {
            handManager.SelectCardByIndex(3);
        }
    }

    /// <summary>
    /// E 키 입력을 처리합니다.
    /// 일반 턴에서는 보존 모드를 시작하고,
    /// 보존 모드에서는 보존을 확정합니다.
    /// </summary>
    private void HandleEndTurnInput()
    {
        if (!Input.GetKeyDown(KeyCode.E))
        {
            return;
        }

        if (handManager.IsPreserveMode)
        {
            handManager.ConfirmPreserveCard();
        }
        else
        {
            handManager.StartPreserveMode();
        }
    }

#if UNITY_EDITOR

    /// <summary>
    /// Inspector에서 HandManager 참조가 비어 있으면
    /// 현재 씬에서 자동으로 찾습니다.
    /// </summary>
    private void OnValidate()
    {
        if (handManager == null)
        {
            handManager =
                FindFirstObjectByType<HandManager>();
        }
    }

#endif
}