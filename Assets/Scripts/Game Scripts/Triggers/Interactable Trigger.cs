using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(BoxCollider))]
public class InteractableTrigger : Trigger<InteractiveObject>
{
    [SerializeField]
    private float triggerLength = 3;

    [HideInInspector]
    public float triggerLengthWithOffset = 8;
    public InteractiveObject closestInteractable { private set; get; }

    private GameObject takeButtonObj,
        attackButton;

    [SerializeField]
    private bool checkTrigger = true;
    private BoxCollider colliderTrigger;
    private PlayerLinks pLinks;

    [SerializeField]
    private InputActionReference interactAction;

    void Awake()
    {
        colliderTrigger = GetComponent<BoxCollider>();
    }

    public void SetTriggerLength(float value)
    {
        value = Mathf.Clamp(value, 0, 7) + triggerLength;

        if (Mathf.Abs(value - triggerLengthWithOffset) < 0.1f)
            return;

        triggerLengthWithOffset = value;

        Vector3 newSize = colliderTrigger.size;
        newSize.z = triggerLengthWithOffset;
        colliderTrigger.size = newSize;

        Vector3 newCenter = colliderTrigger.center;
        newCenter.z = triggerLengthWithOffset / 2f;
        colliderTrigger.center = newCenter;
    }

    public void SetCheckTrigger(bool value)
    {
        checkTrigger = value;
        if (!value)
        {
            // Hide labels
            foreach (var component in triggerList)
            {
                component.ShowLabel(false);
            }

            // Hide buttons if no item selected
            if (!IsHolding())
            {
                takeButtonObj.SetActive(false);
                attackButton.SetActive(true);
            }
        }
    }

    public override void OnTriggerEnter(Collider other)
    {
        if (!other.attachedRigidbody)
            return;

        InteractiveObject[] components = other.attachedRigidbody.GetComponents<InteractiveObject>();
        foreach (var component in components)
        {
            //if the object is not already in the list
            if (!triggerList.Contains(component))
                //add object to the list
                triggerList.Add(component);
        }
    }

    public override void OnTriggerExit(Collider other)
    {
        if (!other.attachedRigidbody)
            return;

        InteractiveObject[] components = other.attachedRigidbody.GetComponents<InteractiveObject>();
        foreach (var component in components)
        {
            //if the object is in the list
            if (triggerList.Contains(component))
            {
                //remove it from the list
                triggerList.Remove(component);
                component.ShowLabel(false);
            }
        }
    }

    public override void Update()
    {
        base.Update();

        if (!checkTrigger)
            return;

        closestInteractable = GetClosest();

        foreach (var component in triggerList)
        {
            bool shouldBeActive = component == closestInteractable;
            component.ShowLabel(shouldBeActive);
        }

        takeButtonObj.SetActive(closestInteractable);
        attackButton.SetActive(!closestInteractable);
    }

    [Header("Get Closest")]
    public float closestToAxisFactor = 1,
        closestToCameraFactor = 3;

    private InteractiveObject GetClosest()
    {
        if (!isTriggered())
            return null;

        InteractiveObject closest = null;
        float minScore = Mathf.Infinity;
        Vector3 endPointWorld = transform.position + transform.forward * triggerLengthWithOffset,
            segment = endPointWorld - transform.position;

        foreach (var component in triggerList)
        {
            if (!component.IsInteractable())
                continue;

            Vector3 dirToComponent = component.transform.position - transform.position;

            // Closest to z axis
            float distToZSqr;
            float segmentLengthSqr = segment.sqrMagnitude;

            if (segmentLengthSqr > 0)
            {
                float t = Vector3.Dot(dirToComponent, segment) / segmentLengthSqr;
                Vector3 closestPointLocal = segment * Mathf.Clamp01(t);
                distToZSqr = (dirToComponent - closestPointLocal).sqrMagnitude;
            }
            else
            {
                distToZSqr = dirToComponent.sqrMagnitude;
            }

            // Closest to the camera
            float distToCamSqr = dirToComponent.sqrMagnitude;

            float currentScore =
                distToZSqr * closestToAxisFactor + distToCamSqr * closestToCameraFactor;
            if (currentScore < minScore)
            {
                minScore = currentScore;
                closest = component;
            }
        }
        return closest;
    }

    public override void OnComponentRemoved(InteractiveObject component)
    {
        if (!component)
            return;
        component.ShowLabel(false);
    }

    public void OnTakeButtonDown()
    {
        if (closestInteractable)
            closestInteractable.OnInteractButtonDown(pLinks.transform);
    }

    public void OnTakeButtonUp()
    {
        if (closestInteractable)
            closestInteractable.OnInteractButtonUp(pLinks.transform);
    }

    void OnEnable()
    {
        PlayerInput.Instance.GetAction(interactAction.action).onDown.AddListener(OnTakeButtonDown);
        PlayerInput.Instance.GetAction(interactAction.action).onUp.AddListener(OnTakeButtonUp);
    }

    public override void OnDisable()
    {
        base.OnDisable();

        PlayerInput
            .Instance.GetAction(interactAction.action)
            .onDown.RemoveListener(OnTakeButtonDown);
        PlayerInput.Instance.GetAction(interactAction.action).onUp.RemoveListener(OnTakeButtonUp);

        StopDraging();
    }

    public void StopDraging()
    {
        if (closestInteractable is NetworkItem item)
        {
            item.StopHolding();
        }
    }

    public void SetPlayerLinks(PlayerLinks pLinks)
    {
        this.pLinks = pLinks;
        takeButtonObj = pLinks.ui.Find("Mobile Ui/Ground Ui/Interact Button").gameObject;
        attackButton = pLinks.ui.Find("Mobile Ui/Ground Ui/Attack Button").gameObject;
        pLinks.cameraController.OnDistChanged += SetTriggerLength;
    }

    public bool IsHolding()
    {
        return closestInteractable
            && closestInteractable is NetworkItem item
            && item.IsHeldBy(this);
    }

    private void OnDrawGizmos()
    {
        Vector3 start = transform.position;
        Vector3 end = transform.position + transform.forward * triggerLengthWithOffset;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(start, end);
        Gizmos.DrawSphere(end, 0.05f);
    }
}
