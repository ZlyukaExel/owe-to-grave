using Mirror;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NetworkAudioSource))]
[RequireComponent(typeof(NetworkIdentity))]
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
    private PlayerLinks pLinks;
    private AnyDirectionSlider rotateField;
    private Image takeButtonFiller;
    private bool isHeld = false,
        isHeldCont = false;

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
                pLinks.interactableTrigger.transform.position
                + pLinks.interactableTrigger.transform.forward
                    * pLinks.interactableTrigger.triggerLengthWithOffset
                - rb.worldCenterOfMass;

            // Drag the item
            SetVelocity(dragDirection * 10);

            // Rotate the item
            SetRotation(
                new(
                    pLinks.interactableTrigger.transform.eulerAngles.x + rotateField.y,
                    pLinks.interactableTrigger.transform.eulerAngles.y + rotateField.x,
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
        this.pLinks = pLinks;
        pLinks.interactableTrigger.SetCheckTrigger(false);
        rotateField = pLinks
            .ui.Find("Mobile Ui/Ground Ui/Rotate Field")
            .GetComponent<AnyDirectionSlider>();
        rotateField.gameObject.SetActive(true);
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
            // If is holding
            if (isHeldCont)
            {
                SetVelocity(
                    takeButtonPressedTime
                        * throwingForce
                        * pLinks.interactableTrigger.transform.forward
                );
                StopHolding();
            }
            // If not holding
            else
            {
                // Take in
                if (takeButtonPressedTime < holdButtonTime)
                {
                    SetInteractable(true);
                    StopHolding();
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

    public void StopHolding()
    {
        if (!pLinks)
            return;

        if (!pLinks.stateManager.combatState.isAimingOrShooting)
            pLinks.cameraController.positionOffset.x = 0;
        takeButtonFiller.fillAmount = 0;
        pLinks.interactableTrigger.SetCheckTrigger(true);
        pLinks = null;
        rotateField.gameObject.SetActive(false);
        rotateField = null;
        takeButtonFiller = null;

        takeButtonPressed = isHeld = isHeldCont = false;

        if (this != null && netIdentity != null && NetworkClient.active)
            SetInteractable(true);
    }

    [Command(requiresAuthority = false)]
    public void SetInteractable(bool isInteractable)
    {
        this.isInteractable = isInteractable;
    }

    public bool IsHeldBy(InteractableTrigger trigger) =>
        pLinks && pLinks.interactableTrigger == trigger;
}
