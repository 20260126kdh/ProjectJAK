using UnityEngine;

/// <summary>
/// 게임 전체를 관리하는 싱글톤 매니저
/// </summary>
public class GameManager : MonoBehaviour
{
    #region Singleton

    public static GameManager Instance { get; private set; }

    #endregion

    [Header("Player Data")]
    [SerializeField]
    private PlayerData playerData;

    public PlayerData PlayerData => playerData;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// 게임 데이터를 초기화합니다.
    /// </summary>
    public void InitializeGame()
    {
        playerData.SetClass(PlayerClass.None);
        playerData.SetHP(100);

        Debug.Log("게임 데이터 초기화 완료");
    }
}