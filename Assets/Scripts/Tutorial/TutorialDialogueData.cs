using System;

/// <summary>
/// 튜토리얼 대화 한 단계를 표현합니다.
/// 이후 행동 조건 단계에서도 같은 데이터 형식을 사용합니다.
/// </summary>
[Serializable]
public sealed class TutorialDialogueData
{
    public string dialogue;

    public string requiredCardID;

    /// <summary>
    /// 표시할 대사를 생성합니다.
    /// </summary>
    /// <param name="dialogue">화면에 표시할 대사</param>
    public TutorialDialogueData(
        string dialogue,
        string requiredCardID = null)
    {
        this.dialogue = dialogue;
        this.requiredCardID = requiredCardID;
    }
}
