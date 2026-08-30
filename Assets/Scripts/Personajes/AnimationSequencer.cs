using UnityEngine;

public class AnimationSequencer : MonoBehaviour
{
    [SerializeField] bool isLooking = false;
    [SerializeField] public static bool isFollowing = false;

    [SerializeField] private Transform target;

    [SerializeField] private GameObject plane;

    [SerializeField] private int x;
    [SerializeField] private int y;
    [SerializeField] private int z;

    void Start()
    {
        target = GameObject.Find("Main Camera").transform;
        if (target == null)
        {
            Debug.LogError("Main Camera not found in the scene.");
        }
    }

    public void StartMoving()
    {
        isFollowing = true;
    }

    public void StartLooking()
    {
        isLooking = true;
        plane.SetActive(false);
    }

    void LateUpdate()
    {
        if (isLooking)
        {
            transform.LookAt(target);
            transform.Rotate(x, y, z);
        }
    }
}