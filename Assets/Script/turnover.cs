using UnityEngine;
using UnityEngine.UI;

// turn over UI 버튼에 붙임
[RequireComponent(typeof(Button))]
public class TurnOverButton : MonoBehaviour
{
    Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        // 플레이어 턴일 때만 반응
        if (TurnManager.Instance.CurrentTurn == Turn.Player)
            TurnManager.Instance.EndPlayerTurn();
    }

    void OnDestroy()
    {
        button.onClick.RemoveListener(OnClick);
    }
}