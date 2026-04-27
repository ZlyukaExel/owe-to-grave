using UnityEngine;

[System.Serializable]
public class SpeedBuff : Buff
{
    public float modifier;

    public SpeedBuff(string name, string description, Sprite icon, float timeRemaining, float value)
        : base(name, description, icon, timeRemaining)
    {
        modifier = value;
    }
}
