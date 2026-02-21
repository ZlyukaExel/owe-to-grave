using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class ItemGrabber
{
    public bool takeButtonPressed;
    private readonly GameObject takeButtonObj,
        rotateItemButton,
        aimButton,
        attackButton;
    private readonly AnyDirectionSlider rotateField;
    private readonly Image takeButtonFiller;
    private bool isDragging;
    private readonly int itemsLayerId;
    private Transform itemTransform;
    private Rigidbody itemRigidbody;
    private const float distanceToGrabbedObj = 4;
    private float takeButtonPressedTime;
    private float holdButtonTime = 0.15f;
    private readonly Links l;

    public ItemGrabber(Links links)
    {
        l = links;

        itemsLayerId = LayerMask.NameToLayer("Items");
        takeButtonObj = l.ui.Find("Ground Ui/Interact Button").gameObject;
        takeButtonFiller = takeButtonObj.transform.Find("Filler").GetComponent<Image>();

        rotateItemButton = takeButtonObj.transform.parent.Find("RotateField").gameObject;
        rotateField = rotateItemButton.GetComponent<AnyDirectionSlider>();

        aimButton = l.ui.Find("Ground Ui/Aim Button").gameObject;
        attackButton = l.ui.Find("Ground Ui/Attack Button").gameObject;

        InputManager.Instance.GetAction(KeyCode.E).onDown.AddListener(OnTakeButtonDown);
        InputManager.Instance.GetAction(KeyCode.E).onUp.AddListener(OnTakeButtonUp);
    }

    public void ItemGrabbingUpdate()
    {
        CheckGrabRay();

        if (takeButtonPressed)
        {
            takeButtonPressedTime = Mathf.Clamp01(takeButtonPressedTime + Time.deltaTime);

            if (isDragging)
                takeButtonFiller.fillAmount = takeButtonPressedTime;
        }

        DraggingItem();
    }

    private void CheckGrabRay()
    {
        // If already holding an item, then no need to update
        if (isDragging || takeButtonPressed)
            return;

        // Grab ray on hit enables button
        Ray grabRay = new(l.playerCamera.position, l.playerCamera.forward);
        Debug.DrawRay(
            l.playerCamera.position,
            l.playerCamera.forward
                * (l.humanoid.itemGrabberLength - l.cameraController.positionOffset.z),
            Color.yellow
        );

        bool validItem = false;
        if (
            Physics.Raycast(
                grabRay,
                out RaycastHit hit,
                l.humanoid.itemGrabberLength - l.cameraController.positionOffset.z,
                l.humanoid.itemGrabberLayers
            )
        )
        {
            // Checking item
            itemTransform = hit.transform;

            if (itemTransform.TryGetComponent(out InteractiveObject interactiveObject))
            {
                validItem = interactiveObject.IsInteractable();
            }
            else
                validItem =
                    itemTransform.CompareTag("Interactable")
                    || itemTransform.CompareTag("Resourse");
        }

        if (validItem)
        {
            takeButtonObj.SetActive(true);
            attackButton.SetActive(false);
        }
        else
        {
            attackButton.SetActive(true);
            takeButtonObj.SetActive(false);
            itemTransform = null;
        }
    }

    private void DraggingItem()
    {
        if (isDragging || takeButtonPressed && (takeButtonPressedTime > holdButtonTime))
        {
            // Changing layer so other players can't grab the item
            if (itemRigidbody.gameObject.layer != LayerMask.NameToLayer("Projectiles"))
            {
                NetworkIdentity itemNetId =
                    itemRigidbody.gameObject.GetComponent<NetworkIdentity>();
                l.networkCommands.CmdRequestOwnership(itemNetId);
                l.networkCommands.CmdRequestChangeLayer(
                    itemNetId,
                    LayerMask.NameToLayer("Projectiles")
                );
                if (!l.cameraController.isFirstPerson)
                    l.cameraController.positionOffset.x = 1;
            }

            if (!itemRigidbody.isKinematic)
            {
                Vector3 dragDirection =
                    l.cameraPivot.position
                    + l.cameraPivot.forward * distanceToGrabbedObj
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
            itemTransform != null
                && !itemTransform.TryGetComponent(out CarDoor _)
                && !itemTransform.TryGetComponent(out Seat _)
            || isDragging
        )
        {
            takeButtonPressed = true;

            // Grab the item
            if (!isDragging)
            {
                itemRigidbody = itemTransform.GetComponent<Rigidbody>();

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
        if (itemTransform == null)
            return;

        // If interactable object
        if (itemRigidbody == null)
        {
            if (itemTransform.TryGetComponent(out InteractiveObject interactiveObject))
                interactiveObject.Interact(l.transform);
        }
        else
        {
            takeButtonPressed = false;

            // Throw / drop item
            if (isDragging)
            {
                isDragging = false;

                if (takeButtonPressedTime > holdButtonTime)
                    itemRigidbody.linearVelocity =
                        takeButtonPressedTime
                        * l.humanoid.itemThrowingForce
                        * l.cameraPivot.forward;

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
                    l.networkCommands.CmdDestroy(itemRigidbody.gameObject);
                    l.cameraController.positionOffset.x = 0;
                    itemRigidbody = null;
                }
                else // Start dragging the resourse
                    isDragging = true;
            }

            takeButtonPressedTime = takeButtonFiller.fillAmount = 0;
        }
    }

    public void StopDragging()
    {
        takeButtonPressed = isDragging = false;
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
            l.networkCommands.CmdRequestChangeLayer(
                itemRigidbody.transform.GetComponent<NetworkIdentity>(),
                itemsLayerId
            );
            l.cameraController.positionOffset.x = 0;
            itemRigidbody = null;
        }
    }
}
