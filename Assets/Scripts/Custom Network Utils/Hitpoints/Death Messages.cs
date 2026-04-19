public static class DeathMessages
{
    public static string GetDeathMessage(Player player, DamageInfo damageInfo)
    {
        Entity killer = damageInfo.source?.GetComponent<Entity>();
        bool isSuicide = killer == null || damageInfo.source == player;
        string killerName = isSuicide ? "himself" : killer.entityName;

        return damageInfo.type switch
        {
            DamageType.Bullet => $"Player {player.entityName} shot down by {killerName}",
            DamageType.Item => $"Player {player.entityName} was beat up by {killerName}",
            DamageType.Fall => $"Player {player.entityName} fell to his death",
            _ => $"Player {player.entityName} died",
        };
    }
}
