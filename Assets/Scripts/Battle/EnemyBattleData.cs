using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 한 번의 전투에서 등장할 적 구성을 저장합니다.
/// 최대 3마리까지 적 프리팹을 등록할 수 있습니다.
/// </summary>
[CreateAssetMenu(
    fileName = "EnemyBattleData",
    menuName = "Battle/Enemy Battle Data"
)]
public class EnemyBattleData : ScriptableObject
{
    [Header("전투 식별 정보")]
    [SerializeField]
    private string battleId;

    [Header("등장 적 목록")]
    [SerializeField]
    private List<GameObject> enemyPrefabs =
        new List<GameObject>();

    public string BattleId => battleId;

    public IReadOnlyList<GameObject> EnemyPrefabs =>
        enemyPrefabs;

    public int EnemyCount
    {
        get
        {
            int count = 0;

            for (int i = 0; i < enemyPrefabs.Count; i++)
            {
                if (enemyPrefabs[i] != null)
                {
                    count++;
                }
            }

            return count;
        }
    }

    public GameObject GetEnemyPrefab(int index)
    {
        if (index < 0 || index >= enemyPrefabs.Count)
        {
            return null;
        }

        return enemyPrefabs[index];
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (enemyPrefabs.Count > 3)
        {
            enemyPrefabs.RemoveRange(
                3,
                enemyPrefabs.Count - 3
            );

            Debug.LogWarning(
                $"[EnemyBattleData] {name}의 적 목록이 " +
                "3개를 초과하여 잘랐습니다.",
                this
            );
        }

        if (string.IsNullOrWhiteSpace(battleId))
        {
            battleId = name;
        }
    }
#endif
}