using Mirror;
using UnityEngine;

[RequireComponent(typeof(NetworkIdentity))]
public class NetworkCharacteristics : NetworkBehaviour
{
    [Header("Characteristics")]
    [SyncVar(hook = nameof(OnStatsChangedHook))]
    public Characteristics syncStats = new(1, 1, 1, 1);

    private void OnStatsChangedHook(Characteristics oldStats, Characteristics newStats)
    {
        if (newStats.eloquence.level > oldStats.eloquence.level)
        {
            Debug.Log($"[Client] Skill up eloquence {newStats.eloquence.level}!");
        }

        if (newStats.strength.level > oldStats.strength.level)
        {
            Debug.Log($"[Client] Skill up strength {newStats.strength.level}!");
        }

        if (newStats.agility.level > oldStats.agility.level)
        {
            Debug.Log($"[Client] Skill up agility {newStats.agility.level}!");
        }

        if (newStats.marksmanship.level > oldStats.marksmanship.level)
        {
            Debug.Log($"[Client] Skill up marksmanship {newStats.marksmanship.level}!");
        }
    }

    [Command]
    public void CmdAddSkillExperience(string skillName, int amount)
    {
        Characteristics updatedStats = syncStats;

        switch (skillName.ToLower())
        {
            case "eloquence":
                updatedStats.eloquence = ProcessExperience(updatedStats.eloquence, amount);
                break;
            case "strength":
                updatedStats.strength = ProcessExperience(updatedStats.strength, amount);
                break;
            case "agility":
                updatedStats.agility = ProcessExperience(updatedStats.agility, amount);
                break;
            case "marksmanship":
                updatedStats.marksmanship = ProcessExperience(updatedStats.marksmanship, amount);
                break;
        }

        syncStats = updatedStats;
    }

    private CharacteristicStat ProcessExperience(CharacteristicStat stat, int addedXP)
    {
        stat.currentXP += addedXP;

        while (stat.currentXP >= stat.XPRequiredForNextLevel)
        {
            stat.currentXP -= stat.XPRequiredForNextLevel;
            stat.level++;

            Debug.Log($"[Server] Level up! {stat.level}");
        }

        return stat;
    }
}

// TODO
public enum PlayerStatType
{
    Marksmanship,
    Agility,
    Strength,
    Eloquence,
}
