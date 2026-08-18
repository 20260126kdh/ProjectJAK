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
/// 타락한 계시의 선택 상태와 효과 발동 상태를 관리합니다.
/// 선택한 계시는 현재 전투 동안 다시 선택할 수 없습니다.
/// </summary>
public class HRevelationController : MonoBehaviour
{
    [Header("선택 완료된 계시 목록")]
    [SerializeField]
    private List<HRevelationType> selectedRevelations =
        new List<HRevelationType>();

    [Header("이번 턴 무너진 의지 발동 가능 여부")]
    [SerializeField]
    private bool canActivateBrokenWillThisTurn;

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
    /// 플레이어 턴 시작 시 현재 선택된 계시 효과를 처리합니다.
    /// 선택된 계시가 여러 개라면 모두 적용합니다.
    /// </summary>
    public void ProcessPlayerTurnStartEffects()
    {
        PlayerCombat playerCombat =
            FindFirstObjectByType<PlayerCombat>();

        if (playerCombat == null)
        {
            Debug.LogWarning(
                "[HRevelationController] 계시 효과를 처리할 " +
                "PlayerCombat을 찾지 못했습니다."
            );

            return;
        }

        /*
         * 익사한 기억:
         * 플레이어 턴 시작마다 피해 6을 받습니다.
         */
        if (HasRevelation(
            HRevelationType.DrownedMemory
        ))
        {
            playerCombat.LoseHealth(6);

            Debug.Log(
                "[HRevelationController] 익사한 기억 발동 : " +
                "플레이어에게 피해 6"
            );
        }

        /*
         * 심해의 속삭임:
         * 플레이어 턴 시작마다 약화와 손상을 1씩 얻습니다.
         */
        if (HasRevelation(
            HRevelationType.DeepWhisper
        ))
        {
            StatusEffectHandler statusEffectHandler =
                playerCombat.GetComponent<StatusEffectHandler>();

            if (statusEffectHandler == null)
            {
                Debug.LogWarning(
                    "[HRevelationController] 플레이어에게 " +
                    "StatusEffectHandler가 없습니다."
                );
            }
            else
            {
                statusEffectHandler.AddEnemyDebuffWithDurationStack(
                    StatusEffectType.Weaken,
                    1,
                    1
                );

                statusEffectHandler.AddEnemyDebuffWithDurationStack(
                    StatusEffectType.Cripple,
                    1,
                    1
                );

                if (SFXManager.Instance != null)
                {
                    SFXManager.Instance.PlayEnemyEffectSequence(
                        false,
                        false,
                        0,
                        2
                    );
                }

                Debug.Log(
                    "[HRevelationController] 심해의 속삭임 발동 : " +
                    "약화 1, 손상 1 부여"
                );
            }
        }

        /*
         * 무너진 의지:
         * 이번 플레이어 턴의 첫 공격 카드 피해 감소를 활성화합니다.
         */
        canActivateBrokenWillThisTurn =
            HasRevelation(
                HRevelationType.BrokenWill
            );

        if (canActivateBrokenWillThisTurn)
        {
            Debug.Log(
                "[HRevelationController] 무너진 의지 준비 : " +
                "이번 턴 첫 공격 카드의 피해가 4 감소합니다."
            );
        }
    }

    /// <summary>
    /// 이번 턴 무너진 의지가 발동 가능한지 확인하고 소비합니다.
    /// 첫 공격 카드에서 한 번만 true를 반환합니다.
    /// </summary>
    public bool TryConsumeBrokenWill()
    {
        if (!HasRevelation(
            HRevelationType.BrokenWill
        ))
        {
            return false;
        }

        if (!canActivateBrokenWillThisTurn)
        {
            return false;
        }

        canActivateBrokenWillThisTurn = false;

        Debug.Log(
            "[HRevelationController] 무너진 의지 발동 : " +
            "첫 공격 카드 피해 4 감소"
        );

        return true;
    }

    /// <summary>
    /// 현재 전투의 계시 선택 기록과
    /// 턴별 발동 상태를 초기화합니다.
    /// </summary>
    public void ResetRevelations()
    {
        selectedRevelations.Clear();
        canActivateBrokenWillThisTurn = false;

        Debug.Log(
            "[HRevelationController] 계시 선택 기록 초기화"
        );
    }
}
