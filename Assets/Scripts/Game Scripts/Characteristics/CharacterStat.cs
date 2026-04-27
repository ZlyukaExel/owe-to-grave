using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterStat
{
    [SerializeField]
    private int baseValue; //база

    // Список всех активных баффов/дебаффов
    private List<int> modifiers = new List<int>();

    public CharacterStat(int startingValue)
    {
        baseValue = startingValue;
    }

    public int BaseValue 
    {
        get { return baseValue; }
    }

    public int Value
    {
        get
        {
            int finalValue = baseValue;
            foreach (int mod in modifiers)
            {
                finalValue += mod;
            }
            return finalValue;
        }
    }

    //пороговая проверка
    public bool MeetsThreshold(int requiredLevel)
    {
        return Value >= requiredLevel;
    }

    //управление модификаторами
    public void AddModifier(int modifier)
    {
        if (modifier != 0)
            modifiers.Add(modifier);
    }

    public void RemoveModifier(int modifier)
    {
        if (modifier != 0)
            modifiers.Remove(modifier);
    }

    public void SetBaseValue(int newValue)
    {
        baseValue = newValue;
    }
}