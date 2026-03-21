using UnityEngine;

public class SpeedBuff : Buff
{
    public float SpeedModifier;

    public SpeedBuff(string name, string description, Sprite icon, float timeRemaining, float value)
        : base(name, description, icon, timeRemaining)
    {
        SpeedModifier = value;
    }
}
