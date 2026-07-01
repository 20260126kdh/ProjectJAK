using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    [Header("현재 덱")]
    [SerializeField]
    private List<CardInfo> currentDeck = new List<CardInfo>();

    [Header("시작 덱 UI")]
    [SerializeField]
    private StartingDeckUI startingDeckUI;

    public List<CardInfo> CurrentDeck => currentDeck;

    private void Start()
    {
        CreateStartingDeck();

        if (startingDeckUI != null)
        {
            startingDeckUI.ShowStartingDeck(currentDeck);
        }
        else
        {
            Debug.LogError("StartingDeckUI가 연결되지 않았습니다.");
        }
    }

    private void CreateStartingDeck()
    {
        currentDeck.Clear();

        PlayerClass playerClass = GameManager.Instance.PlayerData.PlayerClass;

        switch (playerClass)
        {
            case PlayerClass.Physique:
                CreatePhysiqueDeck();
                break;

            case PlayerClass.Technician:
                CreateTechnicianDeck();
                break;

            case PlayerClass.Captain:
                CreateCaptainDeck();
                break;

            default:
                Debug.LogError("선택된 클래스가 없습니다.");
                break;
        }
    }

    private void CreatePhysiqueDeck()
    {
        AddCard("강타", "적에게 피해를 줍니다.", CardType.Attack, 4);
        AddCard("버티기", "방어도를 얻습니다.", CardType.Defense, 4);
        AddCard("분노", "다음 공격을 강화합니다.", CardType.Skill, 1);
        AddCard("전력 공격", "강력한 공격을 준비합니다.", CardType.Skill, 1);
    }

    private void CreateTechnicianDeck()
    {
        AddCard("작살 찌르기", "적에게 피해를 주고 작살 스택을 부여합니다.", CardType.Attack, 4);
        AddCard("회피", "방어도를 얻습니다.", CardType.Defense, 4);
        AddCard("정밀 조준", "작살 스택 활용을 준비합니다.", CardType.Skill, 1);
        AddCard("연속 준비", "다단히트 공격을 준비합니다.", CardType.Skill, 1);
    }

    private void CreateCaptainDeck()
    {
        AddCard("지휘 공격", "적에게 피해를 줍니다.", CardType.Attack, 4);
        AddCard("엄호", "방어도를 얻습니다.", CardType.Defense, 4);
        AddCard("소환 명령", "소환수를 부릅니다.", CardType.Skill, 1);
        AddCard("집중 지휘", "소환수 행동을 준비합니다.", CardType.Skill, 1);
    }

    private void AddCard(string cardName, string description, CardType cardType, int count)
    {
        for (int i = 0; i < count; i++)
        {
            CardInfo card = new CardInfo
            {
                cardName = cardName,
                description = description,
                cardType = cardType,
                cardRarity = CardRarity.Common
            };

            currentDeck.Add(card);
        }
    }
}