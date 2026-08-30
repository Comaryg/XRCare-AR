using System;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.XR.ARFoundation;

public class NewIndoorNav_OLD : MonoBehaviour {
    [SerializeField] private Transform player;
    [SerializeField] private ARTrackedImageManager m_TrackedImageManager;
    [SerializeField] private GameObject trackedImagePrefab;
    [SerializeField] private GameObject solPrefab;
    [SerializeField] private LineRenderer line;

    private List<NavigationTarget> navigationTargets = new List<NavigationTarget>();
    private NavMeshSurface navMeshSurface;
    private NavMeshPath navMeshPath;

    private GameObject navigationLine;

    private GameObject navigationBase;
    private GameObject sol;

    private void Start() {
        navMeshPath = new NavMeshPath();

        // disable screen dimming
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        navigationLine = GameObject.Find("NavigationLine");
        if (navigationLine == null)
        {
            Debug.LogError("Navigation line not found in the scene.");
        }
    }

    private void Update() {
        if (navigationBase != null && navigationTargets.Count > 0 && navMeshSurface != null) {
            //navMeshSurface.BuildNavMesh();
            if (NavigationController.locationStep <= 1){
                NavMesh.CalculatePath(player.position, navigationTargets[NavigationController.locationStep].transform.position, NavMesh.AllAreas, navMeshPath);

                if (navMeshPath.status == NavMeshPathStatus.PathComplete) {
                    line.positionCount = navMeshPath.corners.Length;
                    line.SetPositions(navMeshPath.corners);
                } else {
                    line.positionCount = 0;
                }
            } else
            {
                navigationLine.SetActive(false);
            }
        }
    }

    private void OnEnable() => m_TrackedImageManager.trackablesChanged.AddListener(OnChanged);

    private void OnDisable() => m_TrackedImageManager.trackablesChanged.RemoveListener(OnChanged);

    private void OnChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs) {
        foreach (var newImage in eventArgs.added) {
            navigationBase = GameObject.Instantiate(trackedImagePrefab);

            navigationTargets.Clear();
            navigationTargets = navigationBase.transform.GetComponentsInChildren<NavigationTarget>().ToList();
            navMeshSurface = navigationBase.transform.GetComponentInChildren<NavMeshSurface>();

            sol = GameObject.Instantiate(solPrefab);
        }

        foreach (var updatedImage in eventArgs.updated) {
            navigationBase.transform.SetPositionAndRotation(updatedImage.pose.position, Quaternion.Euler(0, updatedImage.pose.rotation.eulerAngles.y, 0));
            if (!AnimationSequencer.isFollowing)
                sol.transform.SetPositionAndRotation(updatedImage.pose.position, Quaternion.Euler(updatedImage.pose.rotation.eulerAngles.x, updatedImage.pose.rotation.eulerAngles.y, updatedImage.pose.rotation.eulerAngles.z));
        }

        foreach (var removedImage in eventArgs.removed) {
        }
    }
}
