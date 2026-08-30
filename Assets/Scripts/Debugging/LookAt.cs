using Unity.VisualScripting;
using UnityEngine;

public class LookAt : MonoBehaviour
{
    [SerializeField] private Transform target;

    void Start()
    {
        target = GameObject.Find("Main Camera").transform;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.LookAt(target);
    }
}
