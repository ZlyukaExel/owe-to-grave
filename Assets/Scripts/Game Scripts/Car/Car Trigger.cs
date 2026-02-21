using UnityEngine;

public class CarTrigger
{
    private readonly GameObject getInTheCarButton;

    private readonly Links l;

    public CarTrigger(Links links)
    {
        l = links;
        getInTheCarButton = l.ui.Find("Car Button").gameObject;
        InputManager.Instance.GetAction(KeyCode.F).onUp.AddListener(GetInTheCar);
    }

    // Remove in PC
    public void CarTriggerUpdate()
    {
        bool isCarValid = false;

        foreach (
            Collider col in Physics.OverlapSphere(
                l.transform.position + l.humanoid.carTriggerOffset,
                l.humanoid.carTriggerRadius,
                l.humanoid.carTriggerLayers.value
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
                l.transform.position + l.humanoid.carTriggerOffset,
                l.humanoid.carTriggerRadius,
                l.humanoid.carTriggerLayers.value
            )
        )
        {
            if (col.transform.root.TryGetComponent(out Car car) && car.HasSeat())
            {
                Vector3 closestPoint = col.ClosestPoint(l.transform.position);

                float distance = Vector3.Distance(closestPoint, l.transform.position);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestCar = car;
                }
            }
        }

        if (closestCar != null)
            closestCar.EnterCar(l.transform);
    }
}
