using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 클래스 선택 관리
/// </summary>
public class ClassSelectManager : MonoBehaviour
{
    [Header("Battle Scene")]
    [SerializeField]
    private string battleScene = "BattleScene";

    [Header("Start HP")]

    [SerializeField]
    private int physiqueHP = 120;

    [SerializeField]
    private int technicianHP = 100;

    [SerializeField]
    private int captainHP = 110;

    public void SelectPhysique()
    {
        SelectClass(PlayerClass.Physique, physiqueHP);
    }

    public void SelectTechnician()
    {
        SelectClass(PlayerClass.Technician, technicianHP);
    }

    public void SelectCaptain()
    {
        SelectClass(PlayerClass.Captain, captainHP);
    }

    private void SelectClass(PlayerClass playerClass, int hp)
    {
        GameManager.Instance.PlayerData.SetClass(playerClass);
        GameManager.Instance.PlayerData.SetHP(hp);

        Debug.Log($"{playerClass} 선택");

        // TODO
        // 시작 덱 지급
        // A가 DeckManager 제작 후 연결 예정

        SceneManager.LoadScene(battleScene);
    }
}