using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UiChanger : MonoBehaviour
{
    [SerializeField] private GameObject[] defaultObjects;
    [SerializeField] private GameObject[] combatObjects;
    [SerializeField] private GameObject[] carObjects;

    public void SetState(UiState state)
    {
        HashSet<GameObject> allObjects = defaultObjects.Concat(combatObjects).Concat(carObjects).ToHashSet();
        foreach (GameObject obj in allObjects)
        {
            obj.SetActive(false);
        }

        GameObject[] currentObjects = state switch
        {
            UiState.Combat => combatObjects,
            UiState.Car => carObjects,
            _ => defaultObjects
        };

        foreach (var obj in currentObjects)
        {
            obj.SetActive(true);
        }
    }
}

public enum UiState
{
    Default,
    Combat,
    Car
}
