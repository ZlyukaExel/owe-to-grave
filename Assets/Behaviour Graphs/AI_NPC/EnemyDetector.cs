using Mirror;
using UnityEngine;

public abstract class EnemyDetector : MonoBehaviour
{
    public abstract bool IsEnemy(NetworkIdentity character);
    public abstract void AddEnemy(NetworkIdentity character);
    public abstract void RemoveEnemy(NetworkIdentity character);
    public abstract NetworkIdentity GetClosestEnemy();
}
