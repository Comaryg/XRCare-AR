using UnityEngine;
using UnityEngine.Events;

public class TriggerZones : MonoBehaviour
{
    [SerializeField] UnityEvent onTriggerEnter;

    void OnTriggerEnter(Collider other)
    {
        onTriggerEnter.Invoke();
    }
}
