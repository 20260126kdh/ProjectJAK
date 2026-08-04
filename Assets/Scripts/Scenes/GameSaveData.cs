using System;
using System.Collections.Generic;

/// <summary>
/// 이어하기에 필요한 게임 진행 데이터를 저장합니다.
///
/// 전투 내부에서 진행된 상태는 저장하지 않습니다.
/// 저장된 위치의 전투를 처음부터 다시 시작하는 방식으로 사용합니다.
///
/// 단, 전투 시작 시 생성된 첫 손패와
/// 남은 드로우 파일 순서는 그대로 복원합니다.
/// </summary>
[Serializable]
public class GameSaveData
{
    #region Save Information

    /// <summary>
    /// 저장 데이터 버전입니다.
    ///
    /// 버전 2부터 첫 손패와
    /// 남은 드로우 파일 순서를 저장합니다.
    /// </summary>
    public int saveVersion = 2;

    /// <summary>
    /// 저장 생성 시각입니다.
    /// 화면 표시와 디버깅을 위해 문자열로 저장합니다.
    /// </summary>
    public string savedAt;

    #endregion

    #region Player

    /// <summary>
    /// 선택한 플레이어 클래스입니다.
    /// JsonUtility 호환성을 위해 enum 값을 int로 저장합니다.
    /// </summary>
    public int playerClass;

    /// <summary>
    /// 저장할 전투 시작 시점의 현재 체력입니다.
    /// </summary>
    public int currentHP;

    /// <summary>
    /// 저장할 전투 시작 시점의 최대 체력입니다.
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
    /// 현재 보유한 전체 카드 목록입니다.
    ///
    /// 같은 카드 ID가 여러 장이어도
    /// 각각 독립된 항목으로 저장합니다.
    /// 카드마다 강화 상태가 다를 수 있기 때문입니다.
    /// </summary>
    public List<SavedCardData> cards =
        new List<SavedCardData>();

    /// <summary>
    /// 전투 시작 시 첫 손패로 뽑힌 카드들의
    /// CurrentDeck 기준 인덱스 목록입니다.
    ///
    /// 저장된 순서가 실제 손패 순서입니다.
    ///
    /// 예:
    /// 2, 5, 0, 7
    /// </summary>
    public List<int> openingHandCardIndices =
        new List<int>();

    /// <summary>
    /// 첫 손패를 뽑고 남은 드로우 파일의
    /// CurrentDeck 기준 인덱스 목록입니다.
    ///
    /// 저장된 순서가 이후 카드 드로우 순서입니다.
    ///
    /// 목록의 0번 카드가 다음에 뽑힐 카드입니다.
    /// </summary>
    public List<int> remainingDrawPileCardIndices =
        new List<int>();

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