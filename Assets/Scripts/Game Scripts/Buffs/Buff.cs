using UnityEngine;

[System.Serializable]
public abstract class Buff
{
    public string Name;
    public string Description;
    public Sprite Icon;
    public float TimeRemaining;

    public Buff(string name, string description, Sprite icon, float timeRemaining)
    {
        Name = name;
        Description = description;
        Icon = icon;
        TimeRemaining = timeRemaining;
    }
}
