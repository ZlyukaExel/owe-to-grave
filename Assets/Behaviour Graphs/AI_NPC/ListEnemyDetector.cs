using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class ListEnemyDetector : EnemyDetector
{
    public readonly List<NetworkIdentity> enemies = new();

    public override bool IsEnemy(NetworkIdentity character) => true;

    public override void AddEnemy(NetworkIdentity character)
    {
        if (!IsEnemy(character))
            return;

        enemies.Add(character);
    }

    public override void RemoveEnemy(NetworkIdentity character)
    {
        enemies.Remove(character);
    }

    public override NetworkIdentity GetClosestEnemy()
    {
        NetworkIdentity closestEnemy = null;
        float closestDistance = Mathf.Infinity;
        foreach (var enemy in enemies)
        {
            var currentDistance = Vector3.Distance(transform.position, enemy.transform.position);
            if (currentDistance < closestDistance)
            {
                closestDistance = currentDistance;
                closestEnemy = enemy;
            }
        }

        return closestEnemy;
    }
}
