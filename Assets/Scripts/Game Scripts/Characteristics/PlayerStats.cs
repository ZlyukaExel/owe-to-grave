using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Characteristics")]
    public CharacterStat strength = new CharacterStat(1);
    public CharacterStat dexterity = new CharacterStat(1);
    public CharacterStat constitution = new CharacterStat(1);
    public CharacterStat intelligence = new CharacterStat(1);
    public CharacterStat wisdom = new CharacterStat(1);
    public CharacterStat charisma = new CharacterStat(1);

}