using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations;

public class SolFollow : MonoBehaviour
{
    [SerializeField] private NavMeshAgent navmesh;
    [SerializeField] private Transform target;
    // [SerializeField] private Animator animator;

    private Vector3 destination;

    void Start()
    {
        target = GameObject.Find("Main Camera").transform;
        if (target == null)
        {
            Debug.LogError("Main Camera not found in the scene.");
        }
    }

    void LateUpdate()
    {
        if (AnimationSequencer.isFollowing)
        {
            navmesh.enabled = true;
            navmesh.SetDestination(target.position);
            // if (navmesh.remainingDistance <= navmesh.stoppingDistance)
            // {
            //     animator.ResetTrigger("fly");
            //     animator.SetTrigger("idle");
            // }
            // else
            // {
            //     animator.ResetTrigger("idle");
            //     animator.SetTrigger("fly");
            // }
        }; 
    }


}