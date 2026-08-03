/// <summary>
/// BattleScene에서 관리하는 주요 UI 패널 종류입니다.
///
/// 각 패널의 고유 기능을 처리하는 용도가 아니라,
/// 현재 어떤 팝업이 열려 있는지 구분하기 위해 사용합니다.
/// </summary>
public enum BattleUIPanelType
{
    /// <summary>
    /// 등록되지 않았거나 열린 패널이 없는 상태입니다.
    /// </summary>
    None,

    /// <summary>
    /// 시작 덱 및 전투 중 카드 더미 확인 패널입니다.
    /// </summary>
    DeckView,

    /// <summary>
    /// 전투 승리 후 카드 보상 패널입니다.
    /// </summary>
    Reward,

    /// <summary>
    /// 전투 사이의 휴식 패널입니다.
    /// </summary>
    Rest,

    /// <summary>
    /// 카드 강화 패널입니다.
    /// </summary>
    Upgrade,

    /// <summary>
    /// 타락한 계시 선택 패널입니다.
    /// </summary>
    Revelation,

    /// <summary>
    /// 전투 일시정지 패널입니다.
    /// </summary>
    Pause,

    /// <summary>
    /// 환경 설정 패널입니다.
    /// 이후 설정 UI 구현 시 사용합니다.
    /// </summary>
    Settings,

    /// <summary>
    /// 전투 포기 확인 패널입니다.
    /// 이후 확인 팝업 구현 시 사용합니다.
    /// </summary>
    SurrenderConfirm
}