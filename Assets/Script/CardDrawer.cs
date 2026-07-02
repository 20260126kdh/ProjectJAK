using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CardDrawer : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] GameObject[] cardPrefabs;      // UI 카드 프리팹 4종 (CardType 순서)
    [SerializeField] RectTransform deckPosition;     // HandDeckPos (덱 위치)
    [SerializeField] RectTransform[] slots;          // HandSlot1~4 (도착 자리)
    [SerializeField] RectTransform handParent;       // 카드가 생성될 부모 (보통 Canvas나 손패 전용 오브젝트)

    [Header("타이밍")]
    [SerializeField] float startDelay = 1f;
    [SerializeField] float moveDuration = 1f;
    [SerializeField] float betweenCards = 0.15f;

    [Header("버림패")]
    [SerializeField] RectTransform wasteDeckPosition; // waste deck 위치
    [SerializeField] float discardDuration = 0.6f;

    // 기존 List<GameObject> handCards 를 아래로 교체
    List<(GameObject go, CardData data)> handCards = new();
    RectTransform[] orderedSlots;

    void Awake()
    {
        // 슬롯을 화면 x좌표 기준 오른쪽부터 정렬
        orderedSlots = slots.OrderByDescending(s => s.anchoredPosition.x).ToArray();
    }

    public void DrawHand()
    {
        StartCoroutine(DrawRoutine());
    }

    IEnumerator DrawRoutine()
    {
        ClearHand();
        CardManager.Instance.ClearHand();

        yield return new WaitForSeconds(startDelay);

        int order = 0;
        foreach (var slot in orderedSlots)
        {
            if (!CardManager.Instance.CanDraw())
            {
                Debug.Log("상한 도달, 드로우 중단");
                break;
            }

            CardData card = CardDatabase.Instance.DrawCard();
            if (card == null)
            {
                Debug.Log("뽑을 카드가 없음");
                break;
            }

            CardManager.Instance.RegisterCard();
            StartCoroutine(MoveCardToSlot(slot, order, card));
            order++;
            yield return new WaitForSeconds(betweenCards);
        }
    }

    IEnumerator MoveCardToSlot(RectTransform slot, int order, CardData card)
    {
        GameObject prefab = cardPrefabs[(int)card.type];
        GameObject go = Instantiate(prefab, handParent);
        RectTransform rt = go.GetComponent<RectTransform>();
        handCards.Add((go, card));  // 카드와 데이터를 짝지어 저장

        go.transform.SetSiblingIndex(order);

        Vector3 start = deckPosition.position;
        Vector3 end = slot.position;
        rt.position = start;

        float t = 0f;
        while (t < moveDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.SmoothStep(0f, 1f, t / moveDuration);
            rt.position = Vector3.Lerp(start, end, p);
            yield return null;
        }
        rt.position = end;
    }

    void ClearHand()
    {
        foreach (var c in handCards)
            if (c.go != null) Destroy(c.go);
        handCards.Clear();
    }

    public void DiscardHand()
    {
        StartCoroutine(DiscardRoutine());
    }

    IEnumerator DiscardRoutine()
    {
        var toDiscard = new List<(GameObject go, CardData data)>(handCards);

        foreach (var c in toDiscard)
        {
            // 데이터를 손패 → 버림패로 이동
            CardDatabase.Instance.DiscardFromHand(c.data);
            StartCoroutine(MoveCardToWaste(c.go));
            yield return new WaitForSeconds(0.1f);
        }
        handCards.Clear();
    }

    IEnumerator MoveCardToWaste(GameObject go)
    {
        RectTransform rt = go.GetComponent<RectTransform>();
        Vector3 start = rt.position;
        Vector3 end = wasteDeckPosition.position;

        float t = 0f;
        while (t < discardDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.SmoothStep(0f, 1f, t / discardDuration);
            rt.position = Vector3.Lerp(start, end, p);
            yield return null;
        }
        Destroy(go);
    }
}