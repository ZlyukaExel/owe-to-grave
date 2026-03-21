using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ItemGrabber))]
public class PlayerLinks : Links
{
    public Transform ui { get; private set; }
    public Minimap minimap { get; private set; }
    public CameraController cameraController { get; private set; }
    public ItemGrabber itemGrabber { get; private set; }
    public Transform playerCamera => cameraController.transform;
    public Transform cameraPivot => playerCamera.parent;

    [SerializeField]
    private GameObject cameraPrefab,
        uiPrefab,
        minimapPrefab;

    public override void Awake()
    {
        base.Awake();

        itemGrabber = GetComponent<ItemGrabber>();
    }

    public override void Start()
    {
        if (!(netIdentity.connectionToClient == null && isServer || isOwned))
            return;

        ui = Instantiate(uiPrefab).transform.Find("Game Ui");
        minimap = Instantiate(minimapPrefab).GetComponent<Minimap>();
        cameraController = Instantiate(cameraPrefab).GetComponentInChildren<CameraController>();
        minimap.SetTarget(transform); // TODO: change in movement
        input = PlayerInput.Instance;

        base.Start();

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

        hitpoints.SetUi(ui.Find("Hitpoints").GetComponent<Slider>());
        minimap.PlayerMarker = ui.Find("Minimap/Player marker");
    }
}
