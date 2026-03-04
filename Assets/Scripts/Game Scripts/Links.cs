using Mirror;
using UnityEngine;

public class Links : NetworkBehaviour
{
    [HideInInspector]
    public Transform ui { get; private set; }

    [HideInInspector]
    public Minimap minimap { get; private set; }

    [HideInInspector]
    public Transform playerCamera;
    public GameObject cameraPrefab;

    [HideInInspector]
    public Transform cameraPivot => playerCamera.parent;

    [HideInInspector]
    public CameraController cameraController { get; private set; }

    [HideInInspector]
    public Humanoid humanoid { get; private set; }

    [HideInInspector]
    public Movement movement { get; private set; }

    [HideInInspector]
    public Dash dash { get; private set; }

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

    [HideInInspector]
    public NetworkCommands networkCommands { get; private set; }

    public Transform rifleInHand; // Must be accesable

    public Weapon weapon; // TODO: get from inventory

    [SerializeField]
    private MonoBehaviour[] localScripts;

    [SerializeField]
    private GameObject uiPrefab,
        minimapPrefab;

    public override void OnStartLocalPlayer()
    {
        ui = Instantiate(uiPrefab).transform.Find("Game Ui");
        minimap = Instantiate(minimapPrefab).GetComponent<Minimap>();
        cameraController = Instantiate(cameraPrefab).GetComponentInChildren<CameraController>();
        playerCamera = cameraController.transform;
        minimap.SetTarget(transform);

        // Getting all of the components
        Transform firstPersonCameraPivot = transform.Find("First Person Camera Pivot");
        Transform thirdPersonCameraPivot = transform.Find("Third Person Camera Pivot");
        cameraController.ChangeTarget(
            transform,
            this,
            firstPersonCameraPivot,
            thirdPersonCameraPivot
        );

        humanoid = GetComponent<Humanoid>();
        movement = new Movement(this);
        dash = GetComponent<Dash>();
        itemGrabber = new ItemGrabber(this);
        rb = GetComponent<Rigidbody>();
        animator = transform.Find("Armature").GetComponent<Animator>();
        carTrigger = new CarTrigger(this);
        hitpoints = GetComponent<NetworkHitpoints>();
        networkCommands = GetComponent<NetworkCommands>();

        hitpoints.ChangeHitPoints(GetComponent<HitPointsSet>());

        // Enable local scripts & components
        foreach (var script in localScripts)
        {
            script.enabled = true;
        }

        rb.isKinematic = false;

        hitpoints.SetUi(ui);
        minimap.PlayerMarker = ui.Find("Minimap/Player marker");
    }
}
