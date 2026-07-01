using System;

[Serializable]
public class CardEffectData
{
    public int order;
    public CardEffectType effectType;
    public int value;
    public CardTargetType target;
}