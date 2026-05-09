using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Inventory))]
public class PlayerLinks : Links
{
    public Transform ui { get; private set; }
    public Minimap minimap { get; private set; }
    public CameraController cameraController { get; private set; }

    public InteractableTrigger interactableTrigger { get; private set; }
    public Transform playerCamera => cameraController.transform;
    public Transform cameraPivot => playerCamera.parent;

    [SerializeField]
    private GameObject cameraPrefab,
        uiPrefab,
        minimapPrefab;

    public override void Awake()
    {
        base.Awake();
    }

    public override void Start()
    {
        if (!(netIdentity.connectionToClient == null && isServer || isOwned))
            return;

        ui = Instantiate(uiPrefab).transform.Find("Game Ui");
        minimap = Instantiate(minimapPrefab).GetComponent<Minimap>();
        cameraController = Instantiate(cameraPrefab).GetComponentInChildren<CameraController>();
        interactableTrigger = playerCamera.GetComponentInChildren<InteractableTrigger>();
        interactableTrigger.SetPlayerLinks(this);
        minimap.SetTarget(transform); // TODO: change in movement
        input = PlayerInput.Instance;

        Inventory inventory = GetComponent<Inventory>();
        ui.parent.Find("Inventory/Horizontal/Player Inventory")
            .GetComponent<InventoryUi>()
            .SetInventory(inventory);
        inventory.SetTarget(cameraPivot);

        base.Start();

        // Camera init
        Transform firstPersonCameraPivot = transform.Find("First Person Camera Pivot");
        Transform thirdPersonCameraPivot = transform.Find("Third Person Camera Pivot");
        cameraController.ChangeTarget(
            transform,
            this,
            firstPersonCameraPivot,
            thirdPersonCameraPivot
        );

        hitpoints.SetUi(ui.Find("Hitpoints").GetComponent<Slider>());
        minimap.PlayerMarker = ui.Find("Minimap/Player marker");
    }
}
