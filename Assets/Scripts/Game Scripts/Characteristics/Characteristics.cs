using System;

[Serializable]
public struct CharacteristicStat
{
    public int level;
    public int currentXP;
    public int totalModifier;

    public readonly int Value => level + totalModifier;

    public readonly int XPRequiredForNextLevel => level * 100;

    public CharacteristicStat(int startingLevel)
    {
        level = Math.Max(1, startingLevel);
        currentXP = 0;
        totalModifier = 0;
    }
}

[Serializable]
public struct Characteristics
{
    public CharacteristicStat eloquence;
    public CharacteristicStat strength;
    public CharacteristicStat agility;
    public CharacteristicStat marksmanship;

    public Characteristics(
        int eloquence = 1,
        int strength = 1,
        int agility = 1,
        int marksmanship = 1
    )
    {
        this.eloquence = new CharacteristicStat(Math.Max(1, eloquence));
        this.strength = new CharacteristicStat(Math.Max(1, strength));
        this.agility = new CharacteristicStat(Math.Max(1, agility));
        this.marksmanship = new CharacteristicStat(Math.Max(1, marksmanship));
    }
}
