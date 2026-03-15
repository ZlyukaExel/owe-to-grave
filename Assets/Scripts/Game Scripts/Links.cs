using Mirror;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(StateManager))]
[RequireComponent(typeof(MovementManager))]
[RequireComponent(typeof(NetworkHitpoints))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(HitPointsSet))]
[RequireComponent(typeof(CharacterConfigManager))]
[RequireComponent(typeof(CarTrigger))]
[RequireComponent(typeof(ItemGrabber))]
[RequireComponent(typeof(NetworkCharacterConfig))]
public class Links : NetworkBehaviour
{
    [HideInInspector]
    public Transform ui { get; private set; }

    [HideInInspector]
    public Minimap minimap { get; private set; }

    [HideInInspector]
    public CameraController cameraController { get; private set; }

    [HideInInspector]
    public Transform playerCamera => cameraController.transform;

    [HideInInspector]
    public Transform cameraPivot => playerCamera.parent;

    [HideInInspector]
    public StateManager stateManager { get; private set; }

    [HideInInspector]
    public MovementManager movement { get; private set; }

    [HideInInspector]
    public ItemGrabber itemGrabber { get; private set; }

    [HideInInspector]
    public Rigidbody rb { get; private set; }

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

    [SerializeField]
    private GameObject cameraPrefab,
        uiPrefab,
        minimapPrefab;

    private void Start()
    {
        if (!isOwned)
            return;

        ui = Instantiate(uiPrefab).transform.Find("Game Ui");
        minimap = Instantiate(minimapPrefab).GetComponent<Minimap>();
        cameraController = Instantiate(cameraPrefab).GetComponentInChildren<CameraController>();
        minimap.SetTarget(transform); // TODO: change in movement

        // Camera init
        cameraController.Initialize(transform);
        Transform firstPersonCameraPivot = transform.Find("First Person Camera Pivot");
        Transform thirdPersonCameraPivot = transform.Find("Third Person Camera Pivot");
        cameraController.ChangeTarget(
            transform,
            this,
            firstPersonCameraPivot,
            thirdPersonCameraPivot
        );

        stateManager = GetComponent<StateManager>();
        movement = GetComponent<MovementManager>();
        itemGrabber = GetComponent<ItemGrabber>();
        rb = GetComponent<Rigidbody>();
        animator = transform.Find("Armature").GetComponent<Animator>();
        carTrigger = GetComponent<CarTrigger>();
        hitpoints = GetComponent<NetworkHitpoints>();
        netConfig = GetComponent<NetworkCharacterConfig>();

        // Enable local scripts & components
        foreach (var script in localScripts)
        {
            script.enabled = true;
        }

        rb.isKinematic = false;

        hitpoints.SetUi(ui.Find("Hitpoints").GetComponent<Slider>());
        minimap.PlayerMarker = ui.Find("Minimap/Player marker");
    }
}
