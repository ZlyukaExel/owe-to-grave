using System.Collections;
using Mirror;
using UnityEngine;

public class Seat : InteractiveObject
{
    public override bool IsInteractable() => !currentCharacter;

    [SyncVar(hook = nameof(OnCurrentCharacterChanged))]
    public NetworkIdentity currentCharacter = null;

    [SerializeField]
    private Car attachedCarScript;

    [SerializeField]
    private Transform thirdPersonCameraPivot;
    private Transform firstPersonCameraPivot;
    private InputManager input;
    private GameObject dummy;

    [Header("Stand Settings")]
    [SerializeField]
    private Vector3 standOffset = new(0, 0, 1.5f);

    [SerializeField]
    private float characterHeight = 1;

    [SerializeField]
    private float characterRadius = 0.4f;

    [SerializeField]
    private LayerMask obstacleLayers = (1 << 0) | (1 << 3);

    void Awake()
    {
        dummy = transform.GetChild(0).gameObject;
    }

    void Start()
    {
        dummy.SetActive(currentCharacter);
        firstPersonCameraPivot = dummy.transform.Find("First Person Camera Pivot");
    }

    public override void Interact(Transform character)
    {
        Sit(character);
    }

    public void Sit(Transform character)
    {
        if (character == null || !IsInteractable())
            return;

        CmdSetCharacter(character.GetComponent<NetworkIdentity>());

        Links l = character.GetComponent<Links>();

        input = InputManager.Instance;
        l.ui.Find("Ground Ui").gameObject.SetActive(false);

        // Saving camera position
        Vector3 cameraPositionSaver = l.playerCamera.position;

        // If it's a car seat
        if (attachedCarScript != null)
        {
            // Camera ignores car
            int ignoreCameraLayerId = LayerMask.NameToLayer("Ignore Camera");
            foreach (Collider collider in attachedCarScript.GetComponentsInChildren<Collider>())
            {
                if (collider == null)
                    continue;
                collider.gameObject.layer = ignoreCameraLayerId;
            }

            //Change camera settings if it's a car seat
            l.cameraController.ChangeCameraRange(-5, -10);
            l.cameraController.CarScript = attachedCarScript;

            // First person in car uses local coordinates
            if (l.cameraController.isFirstPerson)
                l.cameraController.angleY -= transform.eulerAngles.y;

            l.minimap.target = attachedCarScript.transform;

            input.joystick.gameObject.SetActive(false);
            StartCoroutine(Delay());

            IEnumerator Delay()
            {
                yield return new WaitForSeconds(0.1f);
                l.ui.Find("Car Button").gameObject.SetActive(true);
            }
        }
        else
        {
            InputManager.Instance.onMovement.AddListener(Stand);
            l.minimap.target = transform;
        }

        l.cameraController.ChangeTarget(
            transform,
            l,
            firstPersonCameraPivot,
            thirdPersonCameraPivot
        );

        input.GetAction(KeyCode.F).onUp.AddListener(Stand);

        // Return camera to where it was
        l.playerCamera.position = cameraPositionSaver;

        character.GetComponent<NetworkDisable>().SetEnabled(false);
    }

    private void Stand()
    {
        if (!currentCharacter)
            return;

        currentCharacter.GetComponent<NetworkDisable>().SetEnabled(true);

        Links l = currentCharacter.GetComponent<Links>();
        input.joystick.gameObject.SetActive(true);
        input.GetAction(KeyCode.F).onUp.RemoveListener(Stand);
        InputManager.Instance.onMovement.RemoveListener(Stand);

        // Saving camera position
        Vector3 savedCameraPosition = l.playerCamera.position;

        if (attachedCarScript)
        {
            // Exit car if it's a driver seat
            if (attachedCarScript.seats[0] == this)
                attachedCarScript.ExitCar();

            // Camera doesn't ignore car anymore
            int defaultLayerId = LayerMask.NameToLayer("Default");
            foreach (var collider in attachedCarScript.GetComponentsInChildren<Collider>())
            {
                if (collider == null || collider.GetComponent<Camera>())
                    continue;
                collider.gameObject.layer = defaultLayerId;
            }

            // Changing camera settings to default
            l.cameraController.ChangeCameraRange(-1, -7);
            l.cameraController.positionOffset.y = 0;
            l.cameraController.CarScript = null;

            // First person uses global coordinates
            if (l.cameraController.isFirstPerson)
                l.cameraController.angleY = l.cameraPivot.eulerAngles.y;
        }

        l.minimap.target = currentCharacter.transform;

        l.ui.Find("Ground Ui").gameObject.SetActive(true);

        // Move
        Vector3 standPosition = transform.TransformPoint(standOffset);
        bool canStand = !Physics.CheckCapsule(
            standPosition,
            standPosition + Vector3.up * characterHeight,
            characterRadius,
            obstacleLayers
        );
        if (canStand)
        {
            currentCharacter.transform.position = standPosition;
        }
        else
        {
            Vector3 safePosition = transform.position + Vector3.up * 2f;
            currentCharacter.transform.position = safePosition;
        }

        // Rotate
        currentCharacter.transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);

        // Change target
        l.cameraController.ChangeTarget(
            currentCharacter.transform,
            l,
            currentCharacter.transform.Find("Third Person Camera Pivot"),
            currentCharacter.transform.Find("First Person Camera Pivot")
        );

        // Return camera to where it was
        l.playerCamera.position = savedCameraPosition;

        input = null;
        CmdSetCharacter(null);
    }

    [Command(requiresAuthority = false)]
    private void CmdSetCharacter(NetworkIdentity networkIdentity)
    {
        currentCharacter = networkIdentity;
    }

    private void OnCurrentCharacterChanged(NetworkIdentity oldVar, NetworkIdentity newVar)
    {
        dummy.SetActive(newVar);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 standPos = transform.TransformPoint(standOffset);

        Gizmos.DrawWireSphere(standPos, characterRadius);
        Gizmos.DrawWireSphere(standPos + Vector3.up * characterHeight, characterRadius);
        Gizmos.DrawLine(
            standPos + Vector3.right * characterRadius,
            standPos + Vector3.up * characterHeight + Vector3.right * characterRadius
        );
        Gizmos.DrawLine(
            standPos - Vector3.right * characterRadius,
            standPos + Vector3.up * characterHeight - Vector3.right * characterRadius
        );
        Gizmos.DrawLine(
            standPos + Vector3.forward * characterRadius,
            standPos + Vector3.up * characterHeight + Vector3.forward * characterRadius
        );
        Gizmos.DrawLine(
            standPos - Vector3.forward * characterRadius,
            standPos + Vector3.up * characterHeight - Vector3.forward * characterRadius
        );
    }
}
