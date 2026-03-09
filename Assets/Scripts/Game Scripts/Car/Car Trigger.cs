using UnityEngine;

[RequireComponent(typeof(Links))]
public class CarTrigger : MonoBehaviour
{
    private GameObject getInTheCarButton;

    public float carTriggerRadius = 1;
    public Vector3 carTriggerOffset = new(0, 1, 0);
    public LayerMask carTriggerLayers;

    private void Start()
    {
        Links l = GetComponent<Links>();
        getInTheCarButton = l.ui.Find("Car Button").gameObject;
        InputManager.Instance.GetAction(KeyCode.F).onUp.AddListener(GetInTheCar);
    }

    // Remove in PC
    public void CarTriggerUpdate()
    {
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
