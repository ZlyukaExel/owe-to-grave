using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class ListEnemyDetector : EnemyDetector
{
    public readonly List<NetworkIdentityEntity> enemies = new();

    public override bool IsEnemy(NetworkIdentity character) => true;

    public override void AddEnemy(NetworkIdentity character)
    {
        if (character == null || !IsEnemy(character))
            return;

        if (enemies.Exists(e => e.networkIdentity == character))
            return;

        enemies.Add(new NetworkIdentityEntity(character, character.GetComponent<Entity>()));
    }

    public override void RemoveEnemy(NetworkIdentity character)
    {
        if (character == null)
            return;

        enemies.RemoveAll(enemy => character.Equals(enemy.networkIdentity));
    }

    public override NetworkIdentityEntity GetClosestEnemy()
    {
        NetworkIdentityEntity closestEnemyEntity = default;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = transform.position;

        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            var enemyIdentity = enemies[i].networkIdentity;
            if (enemyIdentity == null)
            {
                enemies.RemoveAt(i);
                continue;
            }

            float currentDistanceSqr = (
                currentPosition - enemyIdentity.transform.position
            ).sqrMagnitude;

            if (currentDistanceSqr < closestDistanceSqr)
            {
                closestDistanceSqr = currentDistanceSqr;
                closestEnemyEntity = enemies[i];
            }
        }

        return closestEnemyEntity;
    }

    public override List<NetworkIdentityEntity> GetEnemies() => enemies;
}
