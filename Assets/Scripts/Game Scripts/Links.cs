using Mirror;
using UnityEngine;

[RequireComponent(typeof(StateManager))]
[RequireComponent(typeof(MovementManager))]
[RequireComponent(typeof(NetworkHitpoints))]
[RequireComponent(typeof(HitPointsSet))]
[RequireComponent(typeof(CarTrigger))]
[RequireComponent(typeof(NetworkCharacterConfig))]
[RequireComponent(typeof(NetworkRigidbodyReliable))]
public class Links : NetworkBehaviour
{
    public StateManager stateManager { get; private set; }
    public MovementManager movement { get; private set; }
    public CarTrigger carTrigger { get; private set; }
    public Animator animator { get; private set; }
    public NetworkHitpoints hitpoints { get; private set; }
    public InputManager input { get; protected set; }
    public Weapon weapon => netConfig.configManager.GetWeapon().GetComponent<Weapon>();
    public NetworkCharacterConfig netConfig { get; private set; }

    [SerializeField]
    private MonoBehaviour[] localScripts;

    public virtual void Awake()
    {
        input = GetComponent<InputManager>();
        movement = GetComponent<MovementManager>();
        stateManager = GetComponent<StateManager>();
        animator = transform.Find("Armature").GetComponent<Animator>();
        carTrigger = GetComponent<CarTrigger>();
        hitpoints = GetComponent<NetworkHitpoints>();
        netConfig = GetComponent<NetworkCharacterConfig>();

        GetComponent<NetworkRigidbodyReliable>().isKinematic = false;
    }

    public virtual void Start()
    {
        if (!(netIdentity.connectionToClient == null && isServer || isOwned))
            return;

        // Enable local scripts & components
        foreach (var script in localScripts)
        {
            script.enabled = true;
        }
    }
}
