using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(PlayerLinks))]
public class ItemGrabber : NetworkBehaviour
{
    public float itemGrabberLength = 3;
    public float itemThrowingForce = 20;
    public LayerMask itemGrabberLayers;
    public bool takeButtonPressed;
    private GameObject takeButtonObj,
        rotateItemButton,
        aimButton,
        attackButton;
    private AnyDirectionSlider rotateField;
    private Image takeButtonFiller;
    private bool holdingItem;
    private int itemsLayerId;
    private InteractiveObject interactive;
    public Rigidbody itemRigidbody { private set; get; }
    private float takeButtonPressedTime;
    private float holdButtonTime = 0.15f;
    private TMP_Text interactableLabel;
    private PlayerLinks l;

    [SerializeField]
    private InputActionReference interactAction;

    public void Start()
    {
        l = GetComponent<PlayerLinks>();

        itemsLayerId = LayerMask.NameToLayer("Items");
        interactableLabel = l.ui.Find("Interactable Label").GetComponent<TMP_Text>();
        takeButtonObj = l.ui.Find("Mobile Ui/Ground Ui/Interact Button").gameObject;
        takeButtonFiller = takeButtonObj.transform.Find("Filler").GetComponent<Image>();

        rotateItemButton = takeButtonObj.transform.parent.Find("RotateField").gameObject;
        rotateField = rotateItemButton.GetComponent<AnyDirectionSlider>();

        aimButton = l.ui.Find("Mobile Ui/Ground Ui/Aim Button").gameObject;
        attackButton = l.ui.Find("Mobile Ui/Ground Ui/Attack Button").gameObject;

        PlayerInput.Instance.GetAction(interactAction.action).onDown.AddListener(OnTakeButtonDown);
        PlayerInput.Instance.GetAction(interactAction.action).onUp.AddListener(OnTakeButtonUp);
    }

    public void ItemGrabbingUpdate()
    {
        CheckGrabRay();

        if (takeButtonPressed)
        {
            takeButtonPressedTime = Mathf.Clamp01(takeButtonPressedTime + Time.deltaTime);

            if (holdingItem)
                takeButtonFiller.fillAmount = takeButtonPressedTime;
        }

        DraggingItem();
    }

    private void CheckGrabRay()
    {
        // If already holding an item, then no need to update
        if (holdingItem || takeButtonPressed)
            return;

        // Debug ray
        Ray grabRay = new(l.playerCamera.position, l.playerCamera.forward);
        Debug.DrawRay(
            l.playerCamera.position,
            l.playerCamera.forward * (itemGrabberLength - l.cameraController.positionOffset.z),
            Color.yellow
        );

        if (
            //Raycast hit
            Physics.Raycast(
                grabRay,
                out RaycastHit hit,
                itemGrabberLength - l.cameraController.positionOffset.z,
                itemGrabberLayers
            )
            // Is intaractive object
            && hit.transform.TryGetComponent(out InteractiveObject interactiveObject)
            // Is interactable
            && interactiveObject.IsInteractable()
        )
            interactive = interactiveObject;
        // else
        //     interactive = null;

        // if (interactive)
        //     interactableLabel.text =
        //         interactive.gameObject.name + "\n" + interactive.InteractionText();
        // else
        //     interactableLabel.text = "";

        attackButton.SetActive(!interactive);
        takeButtonObj.SetActive(interactive);
    }

    private void DraggingItem()
    {
        if (holdingItem || takeButtonPressed && (takeButtonPressedTime > holdButtonTime))
        {
            // Changing layer so other players can't grab the item
            if (itemRigidbody.gameObject.layer != LayerMask.NameToLayer("Projectiles"))
            {
                NetworkIdentity itemNetId =
                    itemRigidbody.gameObject.GetComponent<NetworkIdentity>();
                CmdRequestOwnership(itemNetId);
                CmdRequestChangeLayer(itemNetId, LayerMask.NameToLayer("Projectiles"));
                if (!l.cameraController.isFirstPerson)
                    l.cameraController.positionOffset.x = 1;
            }

            if (!itemRigidbody.isKinematic)
            {
                Vector3 dragDirection =
                    l.cameraPivot.position
                    + l.cameraPivot.forward * itemGrabberLength
                    - itemRigidbody.worldCenterOfMass;

                // Drag the item
                itemRigidbody.linearVelocity = dragDirection * 10;

                // Rotate the item
                //#if UNITY_ANDROID
                itemRigidbody.transform.eulerAngles = new Vector3(
                    l.cameraPivot.eulerAngles.x + rotateField.y,
                    l.cameraPivot.eulerAngles.y + rotateField.x,
                    0
                );
                //#endif
            }
        }
    }

    public void OnTakeButtonDown()
    {
        // Grab only items
        if (
            interactive
                && !interactive.TryGetComponent(out CarDoor _)
                && !interactive.TryGetComponent(out Seat _)
                && !interactive.TryGetComponent(out DialogueManager _)
            || holdingItem
        )
        {
            takeButtonPressed = true;

            // Grab the item
            if (!holdingItem)
            {
                itemRigidbody = interactive.GetComponent<Rigidbody>();

                // Set buttons

                //#if UNITY_ANDROID
                rotateItemButton.SetActive(true);
                aimButton.SetActive(false);
                //#endif
            }
        }
    }

    public void OnTakeButtonUp()
    {
        if (!interactive)
            return;

        // If interactable object
        if (itemRigidbody == null)
        {
            if (interactive.TryGetComponent(out InteractiveObject interactiveObject))
                interactiveObject.Interact(transform);
        }
        else
        {
            takeButtonPressed = false;

            // Throw / drop item
            if (holdingItem)
            {
                holdingItem = false;

                if (takeButtonPressedTime > holdButtonTime)
                    itemRigidbody.linearVelocity =
                        takeButtonPressedTime * itemThrowingForce * l.cameraPivot.forward;

                StopDragging();
            }
            else // Pick up item
            {
                // If it's a resourse and button clicked - move it to inventory
                if (itemRigidbody.CompareTag("Resourse") && takeButtonPressedTime < holdButtonTime)
                {
                    //#if UNITY_ANDROID
                    rotateItemButton.SetActive(false);
                    aimButton.SetActive(true);
                    //#endif

                    // Move item to inventory
                    CmdDestroy(itemRigidbody.GetComponent<NetworkIdentity>());
                    if (l.stateManager.state is not CombatState)
                        l.cameraController.positionOffset.x = 0;
                    itemRigidbody = null;
                }
                else // Start dragging the resourse
                    holdingItem = true;
            }

            takeButtonPressedTime = takeButtonFiller.fillAmount = 0;
        }
    }

    [Command]
    private void CmdDestroy(NetworkIdentity target)
    {
        NetworkServer.Destroy(target.gameObject);
    }

    public void StopDragging()
    {
        takeButtonPressed = holdingItem = false;
        takeButtonPressedTime = takeButtonFiller.fillAmount = 0;

        // Aim button appears, replacing rotate field
        aimButton.SetActive(true);
        rotateField.gameObject.SetActive(false);

        // Attack button appears, replacing take button
        attackButton.SetActive(true);
        takeButtonObj.SetActive(false);

        // Set layer back
        if (itemRigidbody != null)
        {
            CmdRequestChangeLayer(
                itemRigidbody.transform.GetComponent<NetworkIdentity>(),
                itemsLayerId
            );
            if (l.stateManager.state is not CombatState)
                l.cameraController.positionOffset.x = 0;
            itemRigidbody = null;
        }
    }

    [Command]
    private void CmdRequestOwnership(NetworkIdentity itemNetId)
    {
        if (itemNetId != null && itemNetId.connectionToClient != connectionToClient)
        {
            itemNetId.RemoveClientAuthority();
            itemNetId.AssignClientAuthority(connectionToClient);
        }
    }

    [Command]
    private void CmdRequestChangeLayer(NetworkIdentity itemNetId, int layer)
    {
        itemNetId.ServerChangeLayer(layer);
        RpcChangeLayer(itemNetId, layer);
    }

    [ClientRpc]
    private void RpcChangeLayer(NetworkIdentity targetObject, int newLayer)
    {
        targetObject.gameObject.layer = newLayer;
    }
}
