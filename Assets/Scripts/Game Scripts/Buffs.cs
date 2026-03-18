using System.Collections.Generic;
using UnityEngine;

public class Buffs : MonoBehaviour
{
    [SerializeField]
    private List<Buff> _activeBuffs = new();

    public void AddBuff(Buff newBuff)
    {
        _activeBuffs.Add(newBuff);
    }

    public IReadOnlyList<Buff> GetActiveBuffs() // ООП
    {
        return _activeBuffs;
    }

    private void Update()
    {
        for (int i = _activeBuffs.Count - 1; i >= 0; i--)
        {
            _activeBuffs[i].TimeRemaining -= Time.deltaTime;

            if (_activeBuffs[i].TimeRemaining <= 0)
            {
                _activeBuffs.RemoveAt(i);
                // Debug.Log("Бафф истек и был удален.");
            }
        }
    }
}
