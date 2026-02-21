using UnityEngine;
using Unity;

public class MinWindowSixeSetter : MonoBehaviour
{
    void Start()
    {
        int minWidth = 600, minHeight = 600;
        MinimumWindowSize.Set(minWidth, minHeight);
    }

    private void OnApplicationQuit()
    {
        MinimumWindowSize.Reset();
    }
}