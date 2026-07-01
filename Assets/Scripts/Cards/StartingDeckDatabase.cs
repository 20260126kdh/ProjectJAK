using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StartingDeckDatabase", menuName = "Game/Starting Deck Database")]
public class StartingDeckDatabase : ScriptableObject
{
    public List<StartingDeckEntry> entries = new List<StartingDeckEntry>();
}