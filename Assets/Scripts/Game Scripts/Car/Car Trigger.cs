using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Links))]
public class CarTrigger : MonoBehaviour
{
    private GameObject getInTheCarButton;

    public float carTriggerRadius = 1;
    public Vector3 carTriggerOffset = new(0, 1, 0);
    public LayerMask carTriggerLayers;

    [SerializeField]
    private InputActionReference carAction;

    private void Start()
    {
        Links l = GetComponent<Links>();

        if (l is PlayerLinks pLinks)
            getInTheCarButton = pLinks.ui.Find("Mobile Ui/Car Button").gameObject;

        l.input.GetAction(carAction.action).onUp.AddListener(GetInTheCar);
    }

    // Remove in PC
    public void CarTriggerUpdate()
    {
        if (!getInTheCarButton)
            return;

        bool isCarValid = false;

        foreach (
            Collider col in Physics.OverlapSphere(
                transform.position + carTriggerOffset,
                carTriggerRadius,
                carTriggerLayers.value
            )
        )
        {
            if (col.transform.root.TryGetComponent(out Car car) && car.HasSeat())
            {
                isCarValid = true;
                break;
            }
        }

        getInTheCarButton.SetActive(isCarValid);
    }

    public void GetInTheCar()
    {
        Car closestCar = null;
        float minDistance = Mathf.Infinity;

        foreach (
            Collider col in Physics.OverlapSphere(
                transform.position + carTriggerOffset,
                carTriggerRadius,
                carTriggerLayers.value
            )
        )
        {
            if (col.transform.root.TryGetComponent(out Car car) && car.HasSeat())
            {
                Vector3 closestPoint = col.ClosestPoint(transform.position);

                float distance = Vector3.Distance(closestPoint, transform.position);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestCar = car;
                }
            }
        }

        if (closestCar != null)
            closestCar.EnterCar(transform);
    }
}
