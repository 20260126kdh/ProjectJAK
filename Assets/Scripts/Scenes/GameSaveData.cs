using System;
using System.Collections.Generic;

/// <summary>
/// 이어하기에 필요한 게임 진행 데이터를 저장합니다.
///
/// 전투 내부 상태는 저장하지 않습니다.
/// 저장된 위치의 전투를 처음부터 다시 시작하는 방식으로 사용합니다.
/// </summary>
[Serializable]
public class GameSaveData
{
    #region Save Information

    /// <summary>
    /// 저장 데이터 버전입니다.
    /// 이후 저장 구조가 변경될 때 호환성 검사에 사용합니다.
    /// </summary>
    public int saveVersion = 1;

    /// <summary>
    /// 저장 생성 시각입니다.
    /// 화면 표시와 디버깅을 위해 문자열로 저장합니다.
    /// </summary>
    public string savedAt;

    #endregion

    #region Player

    /// <summary>
    /// 선택한 플레이어 클래스입니다.
    /// JsonUtility 호환성과 안정성을 위해 enum 값을 int로 저장합니다.
    /// </summary>
    public int playerClass;

    /// <summary>
    /// 저장 시점의 현재 체력입니다.
    /// </summary>
    public int currentHP;

    /// <summary>
    /// 저장 시점의 최대 체력입니다.
    /// </summary>
    public int maxHP;

    #endregion

    #region Stage

    /// <summary>
    /// 현재 스테이지입니다.
    /// </summary>
    public int currentStage;

    /// <summary>
    /// 현재 스테이지에서 완료한 일반 전투 횟수입니다.
    /// </summary>
    public int currentBattleCount;

    /// <summary>
    /// 현재 진행 단계입니다.
    /// StagePhase를 int로 저장합니다.
    /// </summary>
    public int currentPhase;

    /// <summary>
    /// Stage 3 보스 진행 순서입니다.
    ///
    /// 0: 모르바엘
    /// 1: 아리엘
    /// </summary>
    public int currentBossSequence;

    /// <summary>
    /// 게임 클리어 여부입니다.
    /// 일반 이어하기 저장에서는 false가 저장됩니다.
    /// </summary>
    public bool isGameClear;

    #endregion

    #region Deck

    /// <summary>
    /// 현재 보유한 카드 목록입니다.
    ///
    /// 같은 카드 ID가 여러 장이어도
    /// 각각 독립된 항목으로 저장합니다.
    /// 카드마다 강화 상태가 다를 수 있기 때문입니다.
    /// </summary>
    public List<SavedCardData> cards =
        new List<SavedCardData>();

    #endregion
}

/// <summary>
/// 저장되는 카드 한 장의 정보를 나타냅니다.
/// </summary>
[Serializable]
public class SavedCardData
{
    /// <summary>
    /// CardDatabase에서 원본 카드를 찾기 위한 카드 ID입니다.
    /// </summary>
    public string cardID;

    /// <summary>
    /// 해당 카드가 강화된 상태인지 여부입니다.
    /// </summary>
    public bool isUpgraded;

    /// <summary>
    /// 빈 카드 저장 데이터를 생성합니다.
    /// JsonUtility 역직렬화를 위해 유지합니다.
    /// </summary>
    public SavedCardData()
    {
    }

    /// <summary>
    /// 카드 저장 데이터를 생성합니다.
    /// </summary>
    /// <param name="newCardID">저장할 카드 ID</param>
    /// <param name="newIsUpgraded">강화 여부</param>
    public SavedCardData(
        string newCardID,
        bool newIsUpgraded)
    {
        cardID = newCardID;
        isUpgraded = newIsUpgraded;
    }
}