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
    [HideInInspector]
    public StateManager stateManager { get; private set; }

    [HideInInspector]
    public MovementManager movement { get; private set; }

    [HideInInspector]
    public ItemGrabber itemGrabber { get; private set; }

    [HideInInspector]
    public CarTrigger carTrigger { get; private set; }

    [HideInInspector]
    public Animator animator { get; private set; }

    [HideInInspector]
    public NetworkHitpoints hitpoints { get; private set; }

    public Weapon weapon => netConfig.configManager.GetWeapon().GetComponent<Weapon>();
    public NetworkCharacterConfig netConfig { get; private set; }

    [SerializeField]
    private MonoBehaviour[] localScripts;

    public virtual void Start()
    {
        if (!(netIdentity.connectionToClient == null && isServer || isOwned))
            return;

        // ui = Instantiate(uiPrefab).transform.Find("Game Ui");
        // minimap = Instantiate(minimapPrefab).GetComponent<Minimap>();
        // cameraController = Instantiate(cameraPrefab).GetComponentInChildren<CameraController>();
        // minimap.SetTarget(transform);

        // // Camera init
        // cameraController.Initialize(transform);
        // Transform firstPersonCameraPivot = transform.Find("First Person Camera Pivot");
        // Transform thirdPersonCameraPivot = transform.Find("Third Person Camera Pivot");
        // cameraController.ChangeTarget(
        //     transform,
        //     this,
        //     firstPersonCameraPivot,
        //     thirdPersonCameraPivot
        // );

        // Enable local scripts & components
        foreach (var script in localScripts)
        {
            script.enabled = true;
        }

        movement = GetComponent<MovementManager>();
        stateManager = GetComponent<StateManager>();
        itemGrabber = GetComponent<ItemGrabber>();
        animator = transform.Find("Armature").GetComponent<Animator>();
        carTrigger = GetComponent<CarTrigger>();
        hitpoints = GetComponent<NetworkHitpoints>();
        netConfig = GetComponent<NetworkCharacterConfig>();

        GetComponent<NetworkRigidbodyReliable>().isKinematic = false;

        // hitpoints.SetUi(ui.Find("Hitpoints").GetComponent<Slider>());
        // minimap.PlayerMarker = ui.Find("Minimap/Player marker");
    }
}
