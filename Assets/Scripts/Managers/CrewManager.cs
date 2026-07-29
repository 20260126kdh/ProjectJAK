using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 전투 중 소환된 선원들을 관리합니다.
/// 소환, 생성 순서, 성장, 제거를 담당합니다.
/// </summary>
public class CrewManager : MonoBehaviour
{
    [Header("소환수 프리팹")]
    [SerializeField]
    private Crew crewPrefab;

    [Header("소환 위치")]
    [SerializeField]
    private Transform[] spawnPoints;

    [Header("최대 소환 수")]
    [SerializeField]
    private int maxCrewCount = 3;

    [Header("현재 소환된 선원 목록")]
    [SerializeField]
    private List<Crew> crews = new List<Crew>();

    /// <summary>
    /// 현재 소환된 선원 목록입니다.
    /// 목록의 앞쪽이 먼저 소환된 선원입니다.
    /// </summary>
    public IReadOnlyList<Crew> Crews => crews;

    /// <summary>
    /// 현재 소환된 선원 수입니다.
    /// </summary>
    public int CrewCount => crews.Count;

    /// <summary>
    /// 새로운 선원을 소환할 수 있는지 반환합니다.
    /// </summary>
    public bool CanSummon =>
        crews.Count < maxCrewCount &&
        crews.Count < spawnPoints.Length;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0) ||
            Input.GetKeyDown(KeyCode.Keypad0))
        {
            SummonCrewFromButton();
        }
    }

    /// <summary>
    /// 비어 있는 다음 소환 위치에 선원을 생성합니다.
    /// 소환에 성공하면 true를 반환합니다.
    /// </summary>
    public bool SummonCrew()
    {
        RemoveNullCrews();

        if (crewPrefab == null)
        {
            Debug.LogError(
                "[CrewManager] Crew Prefab이 연결되지 않았습니다."
            );

            return false;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError(
                "[CrewManager] 소환 위치가 연결되지 않았습니다."
            );

            return false;
        }

        if (!CanSummon)
        {
            Debug.LogWarning(
                $"[CrewManager] 최대 소환 수에 도달했습니다. " +
                $"현재 {crews.Count}/{maxCrewCount}"
            );

            return false;
        }

        int spawnIndex = crews.Count;
        Transform spawnPoint = spawnPoints[spawnIndex];

        if (spawnPoint == null)
        {
            Debug.LogError(
                $"[CrewManager] Spawn Point {spawnIndex}가 비어 있습니다."
            );

            return false;
        }

        Crew newCrew = Instantiate(
            crewPrefab,
            spawnPoint.position,
            spawnPoint.rotation,
            transform
        );

        newCrew.Initialize(this);
        crews.Add(newCrew);

        Debug.Log(
            $"[CrewManager] 선원 소환 완료 : " +
            $"{crews.Count}/{maxCrewCount}"
        );

        return true;
    }

    /// <summary>
    /// Unity 테스트 버튼에서 선원 소환을 호출합니다.
    /// </summary>
    public void SummonCrewFromButton()
    {
        SummonCrew();
    }

    /// <summary>
    /// 사망한 선원을 현재 목록에서 제거합니다.
    /// Crew.Die()에서 호출됩니다.
    /// </summary>
    public void RemoveCrew(Crew crew)
    {
        if (crew == null)
        {
            return;
        }

        if (!crews.Remove(crew))
        {
            return;
        }

        Debug.Log(
            $"[CrewManager] 선원 목록 제거 : " +
            $"현재 {crews.Count}/{maxCrewCount}"
        );

        RearrangeCrewPositions();
    }

    /// <summary>
    /// 선원이 사망한 뒤 남은 선원들을 앞쪽 소환 위치부터 재배치합니다.
    /// 소환 순서는 유지됩니다.
    /// </summary>
    private void RearrangeCrewPositions()
    {
        for (int i = 0; i < crews.Count; i++)
        {
            Crew crew = crews[i];

            if (crew == null)
            {
                continue;
            }

            if (i >= spawnPoints.Length)
            {
                break;
            }

            Transform spawnPoint = spawnPoints[i];

            if (spawnPoint == null)
            {
                continue;
            }

            crew.transform.SetPositionAndRotation(
                spawnPoint.position,
                spawnPoint.rotation
            );
        }
    }

    /// <summary>
    /// 살아있는 모든 선원을 성장시킵니다.
    /// 플레이어 턴 시작 시 호출할 예정입니다.
    /// </summary>
    public void GrowAllCrews()
    {
        RemoveNullCrews();

        foreach (Crew crew in crews)
        {
            if (crew == null || !crew.IsAlive)
            {
                continue;
            }

            crew.GrowAtPlayerTurnStart();
        }

        Debug.Log(
            $"[CrewManager] 전체 선원 성장 처리 : {crews.Count}명"
        );
    }

    /// <summary>
    /// 살아있는 모든 선원의 체력을 지정된 수치만큼 회복합니다.
    /// 전체 실제 회복량을 반환합니다.
    /// </summary>
    public int HealAllCrews(int amount)
    {
        RemoveNullCrews();

        if (amount <= 0)
        {
            return 0;
        }

        int totalHealedAmount = 0;

        foreach (Crew crew in crews)
        {
            if (crew == null || !crew.IsAlive)
            {
                continue;
            }

            totalHealedAmount += crew.Heal(amount);
        }

        Debug.Log(
            $"[CrewManager] 모든 선원 회복 : " +
            $"선원 {crews.Count}명 / 총 회복량 {totalHealedAmount}"
        );

        return totalHealedAmount;
    }

    /// <summary>
    /// 먼저 소환된 선원부터 순서대로 피해를 받습니다.
    /// 모든 선원이 피해를 받은 뒤 남은 피해량을 반환합니다.
    /// </summary>
    public int AbsorbDamageWithCrews(int damage)
    {
        RemoveNullCrews();

        if (damage <= 0)
        {
            return 0;
        }

        int remainingDamage = damage;

        /*
         * Crew.TakeDamage() 도중 사망한 선원이
         * 목록에서 제거될 수 있으므로 항상 0번 선원부터 처리합니다.
         */
        while (remainingDamage > 0 && crews.Count > 0)
        {
            Crew firstCrew = crews[0];

            if (firstCrew == null || !firstCrew.IsAlive)
            {
                crews.RemoveAt(0);
                continue;
            }

            remainingDamage =
                firstCrew.TakeDamage(remainingDamage);
        }

        Debug.Log(
            $"[CrewManager] 선원 피해 처리 완료 / " +
            $"플레이어에게 전달될 피해 : {remainingDamage}"
        );

        return remainingDamage;
    }

    /// <summary>
    /// 먼저 소환된 선원부터 지정된 수만큼 희생합니다.
    /// 희생된 선원들의 현재 체력 합계를 반환합니다.
    /// </summary>
    public int SacrificeCrews(int count)
    {
        RemoveNullCrews();

        if (count <= 0)
        {
            return 0;
        }

        if (crews.Count < count)
        {
            Debug.LogWarning(
                $"[CrewManager] 희생할 선원이 부족합니다. " +
                $"필요 {count}명 / 현재 {crews.Count}명"
            );

            return 0;
        }

        int sacrificedHealth = 0;

        for (int i = 0; i < count; i++)
        {
            Crew crew = crews[0];

            if (crew == null)
            {
                crews.RemoveAt(0);
                i--;
                continue;
            }

            sacrificedHealth += crew.CurrentHP;

            crews.RemoveAt(0);
            Destroy(crew.gameObject);
        }

        RearrangeCrewPositions();

        Debug.Log(
            $"[CrewManager] 선원 {count}명 희생 / " +
            $"현재 체력 합계 : {sacrificedHealth}"
        );

        return sacrificedHealth;
    }

    /// <summary>
    /// 현재 소환된 모든 선원을 동시에 희생합니다.
    /// 희생된 모든 선원의 현재 체력 합계를 반환합니다.
    /// </summary>
    public int SacrificeAllCrews()
    {
        RemoveNullCrews();

        if (crews.Count <= 0)
        {
            Debug.LogWarning(
                "[CrewManager] 희생할 선원이 없습니다."
            );

            return 0;
        }

        int sacrificedHealth = 0;
        int sacrificedCount = crews.Count;

        foreach (Crew crew in crews)
        {
            if (crew == null)
            {
                continue;
            }

            sacrificedHealth += crew.CurrentHP;
            Destroy(crew.gameObject);
        }

        crews.Clear();

        Debug.Log(
            $"[CrewManager] 모든 선원 희생 : {sacrificedCount}명 / " +
            $"현재 체력 합계 : {sacrificedHealth}"
        );

        return sacrificedHealth;
    }

    /// <summary>
    /// 현재 소환된 모든 선원을 제거합니다.
    /// 전투 종료 또는 새 전투 준비 시 호출할 예정입니다.
    /// </summary>
    public void ClearAllCrews()
    {
        for (int i = crews.Count - 1; i >= 0; i--)
        {
            Crew crew = crews[i];

            if (crew != null)
            {
                Destroy(crew.gameObject);
            }
        }

        crews.Clear();

        Debug.Log("[CrewManager] 모든 선원 제거");
    }

    /// <summary>
    /// 삭제되었거나 비어 있는 선원 참조를 목록에서 제거합니다.
    /// </summary>
    private void RemoveNullCrews()
    {
        crews.RemoveAll(crew => crew == null);
    }
}
