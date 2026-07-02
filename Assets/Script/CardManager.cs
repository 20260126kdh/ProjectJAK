using UnityEngine;

public class CardManager : MonoBehaviour
{
    public static CardManager Instance { get; private set; }

    [SerializeField] int maxHandSize = 4;
    public int CurrentCount { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    public bool CanDraw() => CurrentCount < maxHandSize;

    public bool RegisterCard()
    {
        if (!CanDraw())
        {
            Debug.Log($"카드 상한({maxHandSize}장) 도달");
            return false;
        }
        CurrentCount++;
        return true;
    }

    public void RemoveCard() => CurrentCount = Mathf.Max(0, CurrentCount - 1);
    public void ClearHand() => CurrentCount = 0;
}
