using UnityEngine;

public class NeverSleep : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }
}
