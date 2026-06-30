using UnityEngine;

/// <summary>
/// 게임 전체를 관리하는 핵심 매니저
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
    /// 새 게임 시작
    /// </summary>
    public void InitializeGame()
    {
        playerData.SetClass(PlayerClass.None);
        playerData.SetHP(100);

        Debug.Log("새 게임 초기화 완료");
    }
}