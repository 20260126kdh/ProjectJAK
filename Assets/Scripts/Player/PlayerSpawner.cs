using UnityEngine;

/// <summary>
/// BattleScene 시작 시 선택한 플레이어 클래스를 생성하는 컴포넌트입니다.
/// </summary>
public class PlayerSpawner : MonoBehaviour
{
    [Header("Spawn Point")]
    [SerializeField]
    private Transform spawnPoint;

    [Header("Player Prefabs")]
    [SerializeField]
    private GameObject physiquePrefab;

    [SerializeField]
    private GameObject technicianPrefab;

    [SerializeField]
    private GameObject captainPrefab;

    /// <summary>
    /// 생성된 플레이어
    /// </summary>
    public GameObject SpawnedPlayer { get; private set; }

    private void Start()
    {
        SpawnPlayer();
    }

    /// <summary>
    /// 선택한 클래스를 생성합니다.
    /// </summary>
    private void SpawnPlayer()
    {
        PlayerClass playerClass = GameManager.Instance.PlayerData.PlayerClass;

        GameObject prefab = GetPlayerPrefab(playerClass);

        if (prefab == null)
        {
            Debug.LogError("생성할 플레이어 프리팹이 없습니다.");
            return;
        }

        SpawnedPlayer = Instantiate(
            prefab,
            spawnPoint.position,
            Quaternion.identity);
    }

    /// <summary>
    /// 클래스에 맞는 프리팹을 반환합니다.
    /// </summary>
    private GameObject GetPlayerPrefab(PlayerClass playerClass)
    {
        switch (playerClass)
        {
            case PlayerClass.Physique:
                return physiquePrefab;

            case PlayerClass.Technician:
                return technicianPrefab;

            case PlayerClass.Captain:
                return captainPrefab;

            default:
                return null;
        }
    }
}