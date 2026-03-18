using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ItemGrabber))]
public class PlayerLinks : Links
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

    [SerializeField]
    private GameObject cameraPrefab,
        uiPrefab,
        minimapPrefab;

    public override void Start()
    {
        if (!(netIdentity.connectionToClient == null && isServer || isOwned))
            return;

        base.Start();

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

        hitpoints.SetUi(ui.Find("Hitpoints").GetComponent<Slider>());
        minimap.PlayerMarker = ui.Find("Minimap/Player marker");
    }
}
