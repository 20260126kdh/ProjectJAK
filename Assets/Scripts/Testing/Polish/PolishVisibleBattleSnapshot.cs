using System;
using System.Collections.Generic;

/// <summary>
/// 한 시점에 플레이어 화면으로 확인할 수 있는 전투 정보만 담는 스냅샷입니다.
/// 미래 드로우, 보상, RNG와 다음 패턴 정보는 포함하지 않습니다.
/// </summary>
[Serializable]
public class PolishVisibleBattleSnapshot
{
    public bool isPlayerTurn;
    public int playerHp;
    public int playerMaxHp;
    public int playerBlock;
    public int attackDefenseUseCount;
    public int attackDefenseUseLimit;
    public List<PolishVisibleCardSnapshot> hand = new List<PolishVisibleCardSnapshot>();
    public List<PolishVisibleStatusSnapshot> playerStatuses = new List<PolishVisibleStatusSnapshot>();
    public List<PolishVisibleEnemySnapshot> enemies = new List<PolishVisibleEnemySnapshot>();
    public List<PolishVisibleCrewSnapshot> crews = new List<PolishVisibleCrewSnapshot>();
}

/// <summary>
/// 현재 손패에 표시된 카드 정보입니다.
/// </summary>
[Serializable]
public class PolishVisibleCardSnapshot
{
    public string cardId;
    public string displayName;
    public string description;
    public string cardType;
    public bool isUpgraded;
    public bool isUsable;
}

/// <summary>
/// 화면에서 확인 가능한 상태 효과 정보입니다.
/// </summary>
[Serializable]
public class PolishVisibleStatusSnapshot
{
    public string type;
    public int value;
    public int remainingTurn;
    public bool isPermanent;
    public bool isDebuff;
}

/// <summary>
/// 살아 있는 적의 공개 정보와 현재 Intent입니다.
/// </summary>
[Serializable]
public class PolishVisibleEnemySnapshot
{
    public string objectName;
    public int hp;
    public int maxHp;
    public int block;
    public int harpoonStack;
    public List<string> intents = new List<string>();
    public List<PolishVisibleStatusSnapshot> statuses = new List<PolishVisibleStatusSnapshot>();
}

/// <summary>
/// 현재 소환된 선원의 공개 체력 정보입니다.
/// </summary>
[Serializable]
public class PolishVisibleCrewSnapshot
{
    public int order;
    public int hp;
    public int maxHp;
}
