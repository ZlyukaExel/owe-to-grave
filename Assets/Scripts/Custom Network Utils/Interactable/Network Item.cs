using Mirror;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NetworkAudioSource))]
public class NetworkItem : InteractiveObject
{
    [SyncVar]
    private bool isInteractable = true;
    private NetworkAudioSource audioSource;
    private Rigidbody rb;
    private float lastHitTime = -999,
        hitCooldown = 0.1f;

    [SerializeField]
    private float throwingForce = 5;
    public bool takeButtonPressed;

    [SerializeField]
    private float holdButtonTime = 0.15f,
        takeButtonPressedTime;
    private InteractableTrigger interactableTrigger;
    private AnyDirectionSlider rotateField;
    private GameObject aimButton;
    private Image takeButtonFiller;
    private bool isHeld = false,
        isHeldCont = false;

    [SerializeField]
    private Sprite defaultIcon;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<NetworkAudioSource>();
    }

    void Update()
    {
        if (takeButtonPressed)
        {
            takeButtonPressedTime += Time.deltaTime;
        }

        if (isHeld)
        {
            Vector3 dragDirection =
                interactableTrigger.transform.position
                + interactableTrigger.transform.forward
                    * interactableTrigger.triggerLengthWithOffset
                - rb.worldCenterOfMass;

            // Drag the item
            SetVelocity(dragDirection * 10);

            // Rotate the item
            SetRotation(
                new(
                    interactableTrigger.transform.eulerAngles.x + rotateField.y,
                    interactableTrigger.transform.eulerAngles.y + rotateField.x,
                    0
                )
            );

            if (isHeldCont && takeButtonPressed)
            {
                takeButtonFiller.fillAmount = takeButtonPressedTime;
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!(isOwned || isServer && connectionToClient == null))
            return;

        float velocity =
            rb.linearVelocity.magnitude * 5 /* rb.mass / 100*/
            - 10;

        if (velocity > 0)
        {
            // Damage players
            if (collision.collider.TryGetComponent(out HitPoint hp))
            {
                if (Time.time - lastHitTime > hitCooldown)
                {
                    DamageInfo damageInfo = new(velocity, DamageType.Item, null);
                    hp.Damage(damageInfo);

                    // Audio SFX
                    if (collision.relativeVelocity.magnitude > 3)
                        audioSource.Play();

                    lastHitTime = Time.time;
                }
            }
        }
    }

    [Command(requiresAuthority = false)]
    public void SetVelocity(Vector3 velocity)
    {
        if (rb.isKinematic)
            return;

        rb.linearVelocity = velocity;
    }

    [Command(requiresAuthority = false)]
    public void AddForce(Vector3 velocity)
    {
        if (rb.isKinematic)
            return;

        rb.AddForce(velocity);
    }

    [Command(requiresAuthority = false)]
    public void SetRotation(Vector3 rotation)
    {
        if (rb.isKinematic)
            return;

        rb.transform.eulerAngles = rotation;
    }

    public override bool IsInteractable() => isInteractable;

    public override void OnInteractButtonDown(Transform character)
    {
        takeButtonPressedTime = 0;
        takeButtonPressed = true;

        // If not interactable, return
        if (!isInteractable)
            return;

        // Start dragging
        SetInteractable(false);

        PlayerLinks pLinks = character.GetComponent<PlayerLinks>();
        if (!pLinks.cameraController.isFirstPerson)
            pLinks.cameraController.positionOffset.x = 0.7f;
        interactableTrigger = pLinks.interactableTrigger;
        interactableTrigger.SetCheckTrigger(false);
        rotateField = pLinks
            .ui.Find("Mobile Ui/Ground Ui/Rotate Field")
            .GetComponent<AnyDirectionSlider>();
        rotateField.gameObject.SetActive(true);
        aimButton = pLinks.ui.Find("Mobile Ui/Ground Ui/Aim Button").gameObject;
        aimButton.SetActive(false);
        takeButtonFiller = pLinks
            .ui.Find("Mobile Ui/Ground Ui/Interact Button/Filler")
            .GetComponent<Image>();

        isHeld = true;
    }

    public override void OnInteractButtonUp(Transform character)
    {
        takeButtonPressed = false;

        if (isHeld)
        {
            PlayerLinks pLinks = character.GetComponent<PlayerLinks>();

            // If is holding
            if (isHeldCont)
            {
                if (!pLinks.stateManager.combatState.isAimingOrShooting)
                    pLinks.cameraController.positionOffset.x = 0;

                SetVelocity(
                    takeButtonPressedTime * throwingForce * interactableTrigger.transform.forward
                );
                StopHolding();
            }
            // If not holding
            else
            {
                // Take in
                if (takeButtonPressedTime < holdButtonTime)
                {
                    if (!pLinks.stateManager.combatState.isAimingOrShooting)
                        pLinks.cameraController.positionOffset.x = 0;

                    CleanUpUi();
                    character.GetComponent<Inventory>().TakeIn(netIdentity);
                }
                // Continious holding
                else
                {
                    isHeldCont = true;
                }
            }
        }
    }

    private void CleanUpUi()
    {
        if (!interactableTrigger)
            return;

        takeButtonFiller.fillAmount = 0;
        interactableTrigger.SetCheckTrigger(true);
        interactableTrigger = null;
        rotateField.gameObject.SetActive(false);
        rotateField = null;
        aimButton.SetActive(true);
        aimButton = null;
        takeButtonFiller = null;

        takeButtonPressed = isHeld = isHeldCont = false;
    }

    public void StopHolding()
    {
        CleanUpUi();
        if (this != null && netIdentity != null && NetworkClient.active)
            SetInteractable(true);
    }

    [Command(requiresAuthority = false)]
    public void SetInteractable(bool isInteractable)
    {
        this.isInteractable = isInteractable;
    }

    public bool IsHeldBy(InteractableTrigger trigger) => interactableTrigger == trigger;
}
