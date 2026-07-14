using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 타락한 계시에서 선택할 수 있는 계시 종류입니다.
/// </summary>
public enum HRevelationType
{
    DrownedMemory,
    DeepWhisper,
    BrokenWill
}

/// <summary>
/// 타락한 계시의 선택 상태를 관리합니다.
/// 선택한 계시는 현재 전투 동안 다시 선택할 수 없습니다.
/// </summary>
public class HRevelationController : MonoBehaviour
{
    [Header("선택 완료된 계시 목록")]
    [SerializeField]
    private List<HRevelationType> selectedRevelations =
        new List<HRevelationType>();

    /// <summary>
    /// 선택된 계시 목록을 반환합니다.
    /// </summary>
    public IReadOnlyList<HRevelationType> SelectedRevelations =>
        selectedRevelations;

    /// <summary>
    /// 현재 선택한 계시 수를 반환합니다.
    /// </summary>
    public int SelectedRevelationCount =>
        selectedRevelations.Count;

    /// <summary>
    /// 모든 계시를 선택했는지 반환합니다.
    /// </summary>
    public bool HasSelectedAllRevelations =>
        selectedRevelations.Count >= 3;

    private void Awake()
    {
        ResetRevelations();
    }

    /// <summary>
    /// 전달받은 계시가 이미 선택됐는지 확인합니다.
    /// </summary>
    public bool HasRevelation(
        HRevelationType revelationType)
    {
        return selectedRevelations.Contains(
            revelationType
        );
    }

    /// <summary>
    /// 새로운 계시를 선택합니다.
    /// 이미 선택한 계시라면 false를 반환합니다.
    /// </summary>
    public bool SelectRevelation(
        HRevelationType revelationType)
    {
        if (HasSelectedAllRevelations)
        {
            Debug.LogWarning(
                "[HRevelationController] " +
                "모든 계시를 이미 선택했습니다."
            );

            return false;
        }

        if (HasRevelation(revelationType))
        {
            Debug.LogWarning(
                $"[HRevelationController] " +
                $"이미 선택한 계시입니다 : {revelationType}"
            );

            return false;
        }

        selectedRevelations.Add(revelationType);

        Debug.Log(
            $"[HRevelationController] 계시 선택 : " +
            $"{revelationType} / " +
            $"{selectedRevelations.Count}/3"
        );

        return true;
    }

    /// <summary>
    /// 현재 전투의 계시 선택 기록을 초기화합니다.
    /// </summary>
    public void ResetRevelations()
    {
        selectedRevelations.Clear();

        Debug.Log(
            "[HRevelationController] 계시 선택 기록 초기화"
        );
    }
}