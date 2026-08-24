#if UNITY_EDITOR || POLISH_SIMULATION_BUILD
using UnityEngine;

/// <summary>
/// 자동 전투 중 나타나는 필수 선택 패널을 실제 버튼 경로로 처리합니다.
/// </summary>
public sealed class PolishSpecialChoiceAutomationController : MonoBehaviour
{
    /// <summary>
    /// 열린 특수 선택 패널이 있으면 현재 공개 상태를 기준으로 한 번 처리합니다.
    /// </summary>
    /// <param name="snapshot">현재 공개 전투 상태</param>
    /// <returns>특수 패널을 처리했으면 true</returns>
    public bool TryHandleOpenChoice(PolishVisibleBattleSnapshot snapshot)
    {
        HRevelationPanelUI panel = FindFirstObjectByType<HRevelationPanelUI>();
        if (panel == null || !panel.IsPanelOpen)
        {
            return false;
        }

        HRevelationController controller =
            FindFirstObjectByType<HRevelationController>();
        if (controller == null)
        {
            Debug.LogError("[PolishSpecialChoice] 계시 Controller 탐색 실패", this);
            return true;
        }

        HRevelationType selected = ChooseRevelation(snapshot, controller);
        switch (selected)
        {
            case HRevelationType.DrownedMemory:
                panel.OnClickDrownedMemory();
                break;
            case HRevelationType.DeepWhisper:
                panel.OnClickDeepWhisper();
                break;
            default:
                panel.OnClickBrokenWill();
                break;
        }

        Debug.Log(
            $"[PolishSpecialChoice] 계시 선택: {selected} / " +
            $"HP:{snapshot?.playerHp}/{snapshot?.playerMaxHp}",
            this);
        return true;
    }

    /// <summary>
    /// 이미 선택한 계시를 제외하고 현재 생존 상태에 가장 적합한 계시를 고릅니다.
    /// </summary>
    public static HRevelationType ChooseRevelation(
        PolishVisibleBattleSnapshot snapshot,
        HRevelationController controller)
    {
        bool healthy = snapshot != null && snapshot.playerMaxHp > 0 &&
            snapshot.playerHp * 100 >= snapshot.playerMaxHp * 70;
        if (healthy && !controller.HasRevelation(HRevelationType.DrownedMemory))
        {
            return HRevelationType.DrownedMemory;
        }
        if (!controller.HasRevelation(HRevelationType.BrokenWill))
        {
            return HRevelationType.BrokenWill;
        }
        if (!controller.HasRevelation(HRevelationType.DeepWhisper))
        {
            return HRevelationType.DeepWhisper;
        }
        return HRevelationType.DrownedMemory;
    }
}
#endif
