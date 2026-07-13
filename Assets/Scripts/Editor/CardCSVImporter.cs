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

            List<string> values = ParseCSVLine(line);

            string cardID = values[0];

            CardData cardData = LoadOrCreateCardData(cardID);

            cardData.cardID = values[0];
            cardData.cardName = values[1];
            cardData.description = values[2];
            cardData.ownerClass = Enum.Parse<PlayerClass>(values[3]);
            cardData.cardType = Enum.Parse<CardType>(values[4]);
            cardData.cardRarity = Enum.Parse<CardRarity>(values[5]);
            cardData.artworkPath = values[6];

            if (!string.IsNullOrEmpty(cardData.artworkPath))
            {
                Sprite artworkSprite = AssetDatabase.LoadAssetAtPath<Sprite>(cardData.artworkPath);

                if (artworkSprite != null)
                {
                    cardData.artwork = artworkSprite;
                }
                else
                {
                    Debug.LogWarning($"[CardCSVImporter] 카드 이미지를 찾지 못했습니다: {cardData.cardID} / 경로: {cardData.artworkPath}");
                    cardData.artwork = null;
                }
            }
            else
            {
                cardData.artwork = null;
            }

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

            List<string> values = ParseCSVLine(line);

            if (values.Count < 6)
            {
                Debug.LogError($"[CardCSVImporter] CSV 열 개수가 부족합니다. (줄 {i + 1})");
                continue;
            }

            string cardID = values[0];

            if (!cardMap.ContainsKey(cardID))
            {
                Debug.LogWarning($"효과를 연결할 CardData를 찾을 수 없습니다: {cardID}");
                continue;
            }

            if (!int.TryParse(values[1], out int order))
            {
                Debug.LogError($"[CardCSVImporter] Order 파싱 실패 (줄 {i + 1}) : {values[1]}");
                continue;
            }

            if (!Enum.TryParse(values[2], out CardEffectType effectType))
            {
                Debug.LogError($"[CardCSVImporter] EffectType 파싱 실패 (줄 {i + 1}) : {values[2]}");
                continue;
            }

            if (!Enum.TryParse(values[3], out StatusEffectType statusEffectType))
            {
                Debug.LogError($"[CardCSVImporter] StatusEffectType 파싱 실패 (줄 {i + 1}) : {values[3]}");
                continue;
            }

            if (!int.TryParse(values[4], out int value))
            {
                Debug.LogError($"[CardCSVImporter] Value 파싱 실패 (줄 {i + 1}) : {values[4]}");
                continue;
            }

            int repeatCount = 1;
            int targetIndex = 5;

            /*
             * RepeatCount 열이 존재하는 새 CSV 구조라면
             * 6번째 값을 반복 횟수로 사용합니다.
             *
             * 기존 6열 CSV도 임시로 읽을 수 있도록
             * 열이 7개 이상일 때만 처리합니다.
             */
            if (values.Count >= 7)
            {
                if (!string.IsNullOrWhiteSpace(values[5]) &&
                    !int.TryParse(values[5], out repeatCount))
                {
                    Debug.LogError(
                        $"[CardCSVImporter] RepeatCount 파싱 실패 " +
                        $"(줄 {i + 1}) : {values[5]}"
                    );

                    continue;
                }

                repeatCount = Mathf.Max(1, repeatCount);
                targetIndex = 6;
            }

            if (!Enum.TryParse(
                values[targetIndex],
                out CardTargetType target))
            {
                Debug.LogError(
                    $"[CardCSVImporter] Target 파싱 실패 " +
                    $"(줄 {i + 1}) : {values[targetIndex]}"
                );

                continue;
            }

            CardEffectData effect = new CardEffectData
            {
                order = order,
                effectType = effectType,
                statusEffectType = statusEffectType,
                value = value,
                repeatCount = repeatCount,
                target = target
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

            List<string> values = ParseCSVLine(line);

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

    /// <summary>
    /// CSV 한 줄을 쉼표 기준으로 나누되, 따옴표 안의 쉼표는 무시합니다.
    /// </summary>
    private static List<string> ParseCSVLine(string line)
    {
        List<string> result = new List<string>();
        bool insideQuote = false;
        string currentValue = "";

        for (int i = 0; i < line.Length; i++)
        {
            char currentChar = line[i];

            if (currentChar == '"')
            {
                insideQuote = !insideQuote;
                continue;
            }

            if (currentChar == ',' && !insideQuote)
            {
                result.Add(currentValue.Trim());
                currentValue = "";
                continue;
            }

            currentValue += currentChar;
        }

        result.Add(currentValue.Trim());

        return result;
    }
}