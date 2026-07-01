using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 메인 타이틀 관리
/// </summary>
public class TitleManager : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("클래스 선택 씬 이름")]
    [SerializeField]
    private string classSelectScene = "ClassSelectScene";

    /// <summary>
    /// 게임 시작 버튼
    /// </summary>
    public void StartGame()
    {
        Debug.Log("게임 시작");

        GameManager.Instance.InitializeGame();

        SceneManager.LoadScene(classSelectScene);
    }
}