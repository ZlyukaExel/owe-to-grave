using System.Collections;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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

    [SerializeField]
    private InputActionReference carAction;

    public override void Awake()
    {
        base.Awake();
        dummy = transform.GetChild(0).gameObject;
    }

    void Start()
    {
        dummy.SetActive(currentCharacter);
        firstPersonCameraPivot = dummy.transform.Find("First Person Camera Pivot");
    }

    public override void OnInteractButtonUp(Transform character)
    {
        Sit(character);
    }

    public void Sit(Transform character)
    {
        if (character == null || !IsInteractable())
            return;

        CmdSetCharacter(character.GetComponent<NetworkIdentity>());

        // Change dummy config
        CharacterConfig dummyCfg = new(character.GetComponent<NetworkCharacterConfig>().config)
        {
            inCombat = false,
        };
        dummy.GetComponent<NetworkCharacterConfig>().CmdSetConfig(dummyCfg);

        Links l = character.GetComponent<Links>();
        PlayerLinks pLinks = l as PlayerLinks;

        l.stateManager.SetState(EnumState.Freezed);

        input = l.input;

        // If it's a car seat
        if (attachedCarScript != null)
        {
            // Camera ignores car collision now
            int ignoreCameraLayerId = LayerMask.NameToLayer("Ignore Camera");
            foreach (Collider collider in attachedCarScript.GetComponentsInChildren<Collider>(true))
            {
                if (collider == null)
                    continue;
                collider.gameObject.layer = ignoreCameraLayerId;
            }

            //Change camera settings if it's a car seat
            if (pLinks)
            {
                pLinks.cameraController.ChangeCameraRange(-5, -10);
                pLinks.cameraController.CarScript = attachedCarScript;

                // First person in car uses local coordinates
                if (pLinks.cameraController.isFirstPerson)
                    pLinks.cameraController.angleY -= transform.eulerAngles.y;

                pLinks.minimap.target = attachedCarScript.transform;

                PlayerInput pInput = input as PlayerInput;
                pInput.joystick.gameObject.SetActive(false);
                StartCoroutine(Delay());

                IEnumerator Delay()
                {
                    yield return new WaitForSeconds(0.1f);
                    pLinks.ui.Find("Mobile Ui/Car Button").gameObject.SetActive(true);
                }
            }
        }
        else if (pLinks)
        {
            PlayerInput pInput = input as PlayerInput;
            pInput.onMovement.AddListener(Stand);
            pLinks.minimap.target = transform;
        }

        if (pLinks)
        {
            pLinks.ui.Find("Mobile Ui/Ground Ui").gameObject.SetActive(false);
            Image crosshair = pLinks.ui.Find("Crosshair").GetComponent<Image>();
            Color newColor = crosshair.color;
            newColor.a = 0;
            crosshair.color = newColor;
            pLinks.cameraController.ChangeTarget(
                dummy.transform,
                pLinks,
                firstPersonCameraPivot,
                thirdPersonCameraPivot
            );
        }

        input.GetAction(carAction.action).onUp.AddListener(Stand);

        l.hitpoints.ChangeHitPoints(dummy.GetComponent<HitPointsSet>());

        character.GetComponent<NetworkDisable>().SetEnabled(false);
    }

    private void Stand()
    {
        if (!currentCharacter)
            return;

        currentCharacter.GetComponent<NetworkDisable>().SetEnabled(true);

        Links l = currentCharacter.GetComponent<Links>();
        PlayerLinks pLinks = l as PlayerLinks;
        input.GetAction(carAction.action).onUp.RemoveListener(Stand);

        l.stateManager.SetState(EnumState.Default);

        if (attachedCarScript)
        {
            // Exit car if it's a driver seat
            if (attachedCarScript.seats[0] == this)
                attachedCarScript.ExitCar();

            // Camera doesn't ignore car anymore
            int defaultLayerId = LayerMask.NameToLayer("Items");
            foreach (var collider in attachedCarScript.GetComponentsInChildren<Collider>())
            {
                if (collider == null || collider.GetComponent<Camera>())
                    continue;
                collider.gameObject.layer = defaultLayerId;
            }

            // Changing camera settings to default
            if (pLinks)
            {
                pLinks.cameraController.ChangeCameraRange(-1, -7);
                pLinks.cameraController.positionOffset.y = 0;
                pLinks.cameraController.CarScript = null;

                // First person uses global coordinates
                if (pLinks.cameraController.isFirstPerson)
                    pLinks.cameraController.angleY = pLinks.cameraPivot.eulerAngles.y;
            }
        }

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

        if (pLinks)
        {
            pLinks.minimap.target = currentCharacter.transform;
            pLinks.ui.Find("Mobile Ui/Ground Ui").gameObject.SetActive(true);

            PlayerInput pInput = input as PlayerInput;
            pInput.onMovement.RemoveListener(Stand);
            pInput.joystick.gameObject.SetActive(true);

            // Change target
            pLinks.cameraController.ChangeTarget(
                currentCharacter.transform,
                pLinks,
                currentCharacter.transform.Find("Third Person Camera Pivot"),
                currentCharacter.transform.Find("First Person Camera Pivot")
            );
        }

        l.hitpoints.ChangeHitPoints(currentCharacter.GetComponent<HitPointsSet>());

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
