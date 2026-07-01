using UnityEngine;

/// <summary>
/// 클래스 정보를 저장하는 데이터입니다.
/// </summary>
[System.Serializable]
public class ClassInfo
{
    [Header("클래스")]
    public PlayerClass playerClass;

    [Header("클래스 이름")]
    public string className;

    [Header("최대 체력")]
    public int maxHP;

    [Header("패시브 설명")]
    [TextArea]
    public string passiveDescription;

    [Header("클래스 설명")]
    [TextArea]
    public string description;

    [Header("대표 이미지")]
    public Sprite classImage;
}