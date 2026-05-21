using System.Collections.Generic;
using Mirror;
using UnityEngine;

public abstract class EnemyDetector : MonoBehaviour
{
    public abstract bool IsEnemy(NetworkIdentity character);
    public abstract void AddEnemy(NetworkIdentity character);
    public abstract void RemoveEnemy(NetworkIdentity character);
    public abstract List<NetworkIdentityEntity> GetEnemies();
    public abstract NetworkIdentityEntity GetClosestEnemy();
}

public struct NetworkIdentityEntity
{
    public NetworkIdentity networkIdentity;
    public Entity entity;

    public NetworkIdentityEntity(NetworkIdentity networkIdentity = null, Entity entity = null)
    {
        this.networkIdentity = networkIdentity;
        this.entity = entity;
    }
}
