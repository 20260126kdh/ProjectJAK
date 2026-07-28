using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 스테이지와 전투 번호별 EnemyBattleData를 관리합니다.
///
/// StageManager가 현재 스테이지와 전투 정보를 전달하면,
/// 해당 전투에 사용할 EnemyBattleData를 반환합니다.
/// </summary>
[CreateAssetMenu(
    fileName = "BattleDatabase",
    menuName = "Battle/Battle Database"
)]
public class BattleDatabase : ScriptableObject
{
    [Header("일반 전투 데이터")]
    [SerializeField]
    private List<BattleEntry> normalBattleEntries =
        new List<BattleEntry>();

    [Header("보스 전투 데이터")]
    [SerializeField]
    private List<BossBattleEntry> bossBattleEntries =
        new List<BossBattleEntry>();

    /// <summary>
    /// 지정한 스테이지와 일반 전투 번호에 해당하는
    /// EnemyBattleData를 반환합니다.
    /// </summary>
    public EnemyBattleData GetNormalBattleData(
        int stage,
        int battleCount)
    {
        for (int i = 0;
             i < normalBattleEntries.Count;
             i++)
        {
            BattleEntry entry =
                normalBattleEntries[i];

            if (entry == null)
            {
                continue;
            }

            if (entry.Stage != stage)
            {
                continue;
            }

            if (entry.BattleCount != battleCount)
            {
                continue;
            }

            return entry.BattleData;
        }

        Debug.LogWarning(
            $"[BattleDatabase] 일반 전투 데이터를 찾지 못했습니다. " +
            $"Stage: {stage} / Battle: {battleCount}",
            this
        );

        return null;
    }

    /// <summary>
    /// 지정한 스테이지의 보스 전투 데이터를 반환합니다.
    ///
    /// Stage 1, 2:
    /// 등록된 보스 중 하나를 무작위로 반환합니다.
    ///
    /// Stage 3:
    /// bossSequence에 맞는 보스를 확정 반환합니다.
    /// 0: 모르바엘
    /// 1: 아리엘
    /// </summary>
    public EnemyBattleData GetBossBattleData(
        int stage,
        int bossSequence)
    {
        /*
         * Stage 3은 연속 보스 구조이므로
         * 순서에 맞는 보스 데이터를 확정 반환합니다.
         */
        if (stage == 3)
        {
            for (int i = 0;
                 i < bossBattleEntries.Count;
                 i++)
            {
                BossBattleEntry entry =
                    bossBattleEntries[i];

                if (entry == null)
                {
                    continue;
                }

                if (entry.Stage != stage)
                {
                    continue;
                }

                if (entry.BossSequence != bossSequence)
                {
                    continue;
                }

                if (entry.BattleData == null)
                {
                    Debug.LogWarning(
                        $"[BattleDatabase] Stage {stage} / " +
                        $"Boss Sequence {bossSequence}의 " +
                        "보스 전투 데이터가 비어 있습니다.",
                        this
                    );

                    return null;
                }

                Debug.Log(
                    $"[BattleDatabase] Stage 3 보스 확정 선택 / " +
                    $"Sequence: {bossSequence} / " +
                    $"선택: {entry.BattleData.name}",
                    entry.BattleData
                );

                return entry.BattleData;
            }

            Debug.LogWarning(
                $"[BattleDatabase] Stage 3 보스 데이터를 찾지 못했습니다. " +
                $"Boss Sequence: {bossSequence}",
                this
            );

            return null;
        }

        /*
         * Stage 1과 Stage 2는 기존처럼
         * 같은 스테이지에 등록된 보스 중 무작위 선택합니다.
         */
        List<EnemyBattleData> matchingBossBattleData =
            new List<EnemyBattleData>();

        for (int i = 0;
             i < bossBattleEntries.Count;
             i++)
        {
            BossBattleEntry entry =
                bossBattleEntries[i];

            if (entry == null)
            {
                continue;
            }

            if (entry.Stage != stage)
            {
                continue;
            }

            if (entry.BattleData == null)
            {
                Debug.LogWarning(
                    $"[BattleDatabase] Stage {stage}의 " +
                    $"보스 전투 데이터가 비어 있습니다. " +
                    $"Element Index: {i}",
                    this
                );

                continue;
            }

            matchingBossBattleData.Add(
                entry.BattleData
            );
        }

        if (matchingBossBattleData.Count == 0)
        {
            Debug.LogWarning(
                $"[BattleDatabase] 보스 전투 데이터를 찾지 못했습니다. " +
                $"Stage: {stage}",
                this
            );

            return null;
        }

        int randomIndex =
            UnityEngine.Random.Range(
                0,
                matchingBossBattleData.Count
            );

        EnemyBattleData selectedBattleData =
            matchingBossBattleData[randomIndex];

        Debug.Log(
            $"[BattleDatabase] 보스 무작위 선택 완료 / " +
            $"Stage: {stage} / " +
            $"후보 수: {matchingBossBattleData.Count} / " +
            $"선택: {selectedBattleData.name}",
            selectedBattleData
        );

        return selectedBattleData;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        ValidateNormalEntries();
        ValidateBossEntries();
    }

    /// <summary>
    /// 일반 전투 데이터의 잘못된 값을 보정합니다.
    /// </summary>
    private void ValidateNormalEntries()
    {
        for (int i = 0;
             i < normalBattleEntries.Count;
             i++)
        {
            BattleEntry entry =
                normalBattleEntries[i];

            if (entry == null)
            {
                continue;
            }

            entry.ClampValues();
        }
    }

    /// <summary>
    /// 보스 전투 데이터의 잘못된 값을 보정합니다.
    /// </summary>
    private void ValidateBossEntries()
    {
        for (int i = 0;
             i < bossBattleEntries.Count;
             i++)
        {
            BossBattleEntry entry =
                bossBattleEntries[i];

            if (entry == null)
            {
                continue;
            }

            entry.ClampValues();
        }
    }
#endif
}

/// <summary>
/// 일반 전투 하나의 스테이지, 전투 번호,
/// EnemyBattleData 연결 정보를 저장합니다.
/// </summary>
[Serializable]
public class BattleEntry
{
    [SerializeField]
    [Min(1)]
    private int stage = 1;

    [SerializeField]
    [Min(1)]
    private int battleCount = 1;

    [SerializeField]
    private EnemyBattleData battleData;

    public int Stage => stage;
    public int BattleCount => battleCount;
    public EnemyBattleData BattleData => battleData;

#if UNITY_EDITOR
    public void ClampValues()
    {
        stage =
            Mathf.Max(
                1,
                stage
            );

        battleCount =
            Mathf.Max(
                1,
                battleCount
            );
    }
#endif
}

/// <summary>
/// 보스 전투 하나의 스테이지와
/// EnemyBattleData 연결 정보를 저장합니다.
/// </summary>
[Serializable]
public class BossBattleEntry
{
    [SerializeField]
    [Min(1)]
    private int stage = 1;

    [SerializeField]
    [Min(0)]
    private int bossSequence = 0;

    [SerializeField]
    private EnemyBattleData battleData;

    public int Stage => stage;
    public int BossSequence => bossSequence;
    public EnemyBattleData BattleData => battleData;

#if UNITY_EDITOR
    public void ClampValues()
    {
        stage =
            Mathf.Max(
                1,
                stage
            );

        bossSequence =
            Mathf.Max(
                0,
                bossSequence
            );
    }
#endif
}