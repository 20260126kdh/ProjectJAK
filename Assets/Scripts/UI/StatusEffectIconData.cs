using System;
using UnityEngine;

/// <summary>
/// 상태 효과 종류와
/// 해당 상태 효과에 사용할 아이콘 Sprite를 연결합니다.
/// </summary>
[Serializable]
public class StatusEffectIconData
{
    [Header("상태 효과 종류")]
    public StatusEffectType statusEffectType;

    [Header("상태 효과 아이콘")]
    public Sprite iconSprite;
}