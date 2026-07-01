using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class CardCSVImporter
{
    private const string CardsCsvPath = "Assets/Data/CSV/Cards.csv";
    private const string CardEffectsCsvPath = "Assets/Data/CSV/CardEffects.csv";
    private const string StartingDeckCsvPath = "Assets/Data/CSV/StartingDeck.csv";

    private const string CardAssetFolder = "Assets/Data/ScriptableObjects/Cards";
    private const string StartingDeckAssetPath = "Assets/Data/ScriptableObjects/StartingDeckDatabase.asset";

    [MenuItem("Tools/Card/Import Cards")]
    public static void ImportCards()
    {
        Dictionary<string, CardData> cardMap = ImportCardData();
        ImportCardEffects(cardMap);
        ImportStartingDeck();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("카드 데이터 Import 완료");
    }

    private static Dictionary<string, CardData> ImportCardData()
    {
        Dictionary<string, CardData> cardMap = new Dictionary<string, CardData>();

        string[] lines = File.ReadAllLines(CardsCsvPath);

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();

            if (string.IsNullOrEmpty(line))
                continue;

            string[] values = line.Split(',');

            string cardID = values[0];

            CardData cardData = LoadOrCreateCardData(cardID);

            cardData.cardID = values[0];
            cardData.cardName = values[1];
            cardData.description = values[2];
            cardData.ownerClass = Enum.Parse<PlayerClass>(values[3]);
            cardData.cardType = Enum.Parse<CardType>(values[4]);
            cardData.cardRarity = Enum.Parse<CardRarity>(values[5]);
            cardData.artworkPath = values[6];

            cardData.effects.Clear();

            EditorUtility.SetDirty(cardData);

            cardMap[cardID] = cardData;
        }

        return cardMap;
    }

    private static void ImportCardEffects(Dictionary<string, CardData> cardMap)
    {
        string[] lines = File.ReadAllLines(CardEffectsCsvPath);

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();

            if (string.IsNullOrEmpty(line))
                continue;

            string[] values = line.Split(',');

            string cardID = values[0];

            if (!cardMap.ContainsKey(cardID))
            {
                Debug.LogWarning($"효과를 연결할 CardData를 찾을 수 없습니다: {cardID}");
                continue;
            }

            CardEffectData effect = new CardEffectData
            {
                order = int.Parse(values[1]),
                effectType = Enum.Parse<CardEffectType>(values[2]),
                value = int.Parse(values[3]),
                target = Enum.Parse<CardTargetType>(values[4])
            };

            cardMap[cardID].effects.Add(effect);

            cardMap[cardID].effects.Sort((a, b) => a.order.CompareTo(b.order));

            EditorUtility.SetDirty(cardMap[cardID]);
        }
    }

    private static void ImportStartingDeck()
    {
        StartingDeckDatabase database =
            AssetDatabase.LoadAssetAtPath<StartingDeckDatabase>(StartingDeckAssetPath);

        if (database == null)
        {
            database = ScriptableObject.CreateInstance<StartingDeckDatabase>();
            AssetDatabase.CreateAsset(database, StartingDeckAssetPath);
        }

        database.entries.Clear();

        string[] lines = File.ReadAllLines(StartingDeckCsvPath);

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();

            if (string.IsNullOrEmpty(line))
                continue;

            string[] values = line.Split(',');

            StartingDeckEntry entry = new StartingDeckEntry
            {
                ownerClass = Enum.Parse<PlayerClass>(values[0]),
                cardID = values[1],
                count = int.Parse(values[2])
            };

            database.entries.Add(entry);
        }

        EditorUtility.SetDirty(database);
    }

    private static CardData LoadOrCreateCardData(string cardID)
    {
        string assetPath = $"{CardAssetFolder}/{cardID}.asset";

        CardData cardData = AssetDatabase.LoadAssetAtPath<CardData>(assetPath);

        if (cardData == null)
        {
            cardData = ScriptableObject.CreateInstance<CardData>();
            AssetDatabase.CreateAsset(cardData, assetPath);
        }

        return cardData;
    }
}