using System.Collections.Generic;
using UnityEngine;

public class Buffs : MonoBehaviour
{
    [SerializeReference]
    private List<Buff> _activeBuffs = new();

    public void AddBuff(Buff newBuff)
    {
        if (_activeBuffs.Contains(newBuff))
            return;

        _activeBuffs.Add(newBuff);
    }

    public void RemoveBuff(Buff buff)
    {
        _activeBuffs.Remove(buff);
    }

    public IReadOnlyList<Buff> GetActiveBuffs()
    {
        return _activeBuffs;
    }

    private void Update()
    {
        for (int i = _activeBuffs.Count - 1; i >= 0; i--)
        {
            Buff buff = _activeBuffs[i];

            if (buff.TimeRemaining < 0)
                continue;

            buff.TimeRemaining -= Time.deltaTime;
            if (buff.TimeRemaining <= 0)
            {
                _activeBuffs.RemoveAt(i);
            }
        }
    }
}
