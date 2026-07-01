using System;
using System.Collections.Generic;
using UnityEngine;

public class CardCSVLoader : MonoBehaviour
{
    [Header("CSV 파일 이름")]
    [SerializeField]
    private string csvFileName = "Data/cards";

    public List<CardData> LoadCards()
    {
        List<CardData> cards = new List<CardData>();

        TextAsset csvFile = Resources.Load<TextAsset>(csvFileName);

        if (csvFile == null)
        {
            Debug.LogError($"CSV 파일을 찾을 수 없습니다: Resources/{csvFileName}");
            return cards;
        }

        string[] lines = csvFile.text.Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();

            if (string.IsNullOrEmpty(line))
                continue;

            string[] values = line.Split(',');

            if (values.Length < 9)
            {
                Debug.LogWarning($"CSV 데이터가 부족합니다. {i + 1}번째 줄: {line}");
                continue;
            }

            CardData card = new CardData();

            card.cardID = values[0];
            card.cardName = values[1];
            card.description = values[2];

            card.ownerClass = Enum.Parse<PlayerClass>(values[3]);
            card.cardType = Enum.Parse<CardType>(values[4]);
            card.cardRarity = Enum.Parse<CardRarity>(values[5]);

            card.artworkPath = values[6];

            card.isStartingCard = values[7].ToUpper() == "TRUE";
            card.startingCount = int.Parse(values[8]);

            cards.Add(card);
        }

        Debug.Log($"카드 CSV 로드 완료: {cards.Count}장");

        return cards;
    }
}