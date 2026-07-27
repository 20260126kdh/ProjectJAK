using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 모든 상태 효과 아이콘 정보를 보관하고,
/// 상태 효과 종류에 맞는 Sprite를 반환합니다.
/// </summary>
[CreateAssetMenu(
    fileName = "StatusEffectIconDatabase",
    menuName = "Status Effect/Status Effect Icon Database"
)]
public class StatusEffectIconDatabase : ScriptableObject
{
    [Header("상태 효과 아이콘 목록")]
    [SerializeField]
    private List<StatusEffectIconData> iconDataList =
        new List<StatusEffectIconData>();

    /// <summary>
    /// 지정된 상태 효과에 해당하는 아이콘을 반환합니다.
    /// 등록된 아이콘이 없다면 null을 반환합니다.
    /// </summary>
    public Sprite GetIcon(
        StatusEffectType statusEffectType)
    {
        StatusEffectIconData iconData =
            iconDataList.Find(
                data =>
                    data != null &&
                    data.statusEffectType ==
                    statusEffectType
            );

        if (iconData == null)
        {
            Debug.LogWarning(
                $"[StatusEffectIconDatabase] " +
                $"{statusEffectType} 아이콘이 등록되지 않았습니다."
            );

            return null;
        }

        return iconData.iconSprite;
    }

    /// <summary>
    /// 동일한 상태 효과가 중복 등록됐는지 검사합니다.
    /// </summary>
    private void OnValidate()
    {
        HashSet<StatusEffectType> registeredTypes =
            new HashSet<StatusEffectType>();

        foreach (StatusEffectIconData iconData
                 in iconDataList)
        {
            if (iconData == null)
            {
                continue;
            }

            if (iconData.statusEffectType ==
                StatusEffectType.None)
            {
                continue;
            }

            if (!registeredTypes.Add(
                    iconData.statusEffectType))
            {
                Debug.LogWarning(
                    $"[StatusEffectIconDatabase] " +
                    $"{iconData.statusEffectType}이 " +
                    "중복 등록되어 있습니다.",
                    this
                );
            }
        }
    }
}