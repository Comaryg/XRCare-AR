using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class NewIndoorNav_Floor : MonoBehaviour {
    [Header("Navegación")]
    [SerializeField] private Transform player;
    [SerializeField] private LineRenderer line;

    [Header("AR Trackers")]
    [SerializeField] private ARTrackedImageManager m_TrackedImageManager;
    [SerializeField] private ARAnchorManager m_AnchorManager;
    [SerializeField] private ARSession arSession;
    [SerializeField] private ARPlaneManager planeManager;

    [Header("Prefabs")]
    [SerializeField] private GameObject trackedImagePrefab;
    [SerializeField] private GameObject solPrefab;

    [Header("UI / Debug")]
    [SerializeField] private TMP_Text statusText;

    [Header("Flujo de estabilización")]
    [SerializeField] private bool requireHorizontalPlaneBeforeScanning = true;
    [SerializeField] private float requiredSessionTrackingWarmup = 0.75f;
    [SerializeField] private float imageAnchorDelay = 0.3f;

    private List<NavigationTarget> navigationTargets = new List<NavigationTarget>();
    private NavMeshSurface navMeshSurface;
    private NavMeshPath navMeshPath;

    private GameObject navigationLine;
    private GameObject navigationBase;
    private GameObject sol;

    private bool canStartImageTracking = false;
    private bool m_IsAnchoring = false;
    private bool m_Anchored = false;
    private bool lineActive = false;
    private Coroutine m_AnchorRoutine;
    private float sessionTrackingTimer = 0f;

    private void Start() {
        navMeshPath = new NavMeshPath();

        // disable screen dimming
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        navigationLine = GameObject.Find("NavigationLine");
        if (navigationLine == null)
        {
            Debug.LogError("Navigation line not found in the scene.");
        }
        navigationLine.SetActive(false);

        
        if (m_TrackedImageManager != null)
            m_TrackedImageManager.enabled = false;

        SetStatus("Iniciando AR...");
    }

    private void Update() {

        if (!m_Anchored && !canStartImageTracking){
            UpdateLocalizationFlow();
        }

        if (navigationBase != null && navigationTargets.Count > 0 && navMeshSurface != null) {
            //navMeshSurface.BuildNavMesh();
            if (NavigationController.locationStep <= 1 && lineActive){
                navigationLine.SetActive(true);
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

private void UpdateLocalizationFlow()
    {
        switch (ARSession.state)
        {
            case ARSessionState.SessionInitializing:
                sessionTrackingTimer = 0f;
                if (m_TrackedImageManager.enabled)
                    m_TrackedImageManager.enabled = false;

                SetStatus("Mueve el móvil alrededor de la sala para ubicar el espacio");
                break;

            case ARSessionState.SessionTracking:
                sessionTrackingTimer += Time.deltaTime;

                if (sessionTrackingTimer < requiredSessionTrackingWarmup)
                {
                    if (m_TrackedImageManager.enabled)
                        m_TrackedImageManager.enabled = false;

                    SetStatus("Espacio localizado. Sigue moviendo un poco el móvil...");
                    return;
                }

                bool horizontalPlaneReady = !requireHorizontalPlaneBeforeScanning || HasHorizontalPlane();

                if (!horizontalPlaneReady)
                {
                    if (m_TrackedImageManager.enabled)
                        m_TrackedImageManager.enabled = false;

                    SetStatus("Buscando el suelo...");
                    return;
                }

                canStartImageTracking = true;
                if (!m_TrackedImageManager.enabled)
                    m_TrackedImageManager.enabled = true;

                SetStatus("Espacio localizado. Ahora mira al suelo para escanear la imagen");
                break;

            case ARSessionState.Ready:
                sessionTrackingTimer = 0f;
                if (m_TrackedImageManager.enabled)
                    m_TrackedImageManager.enabled = false;

                SetStatus("Preparando la sesión AR...");
                break;

            case ARSessionState.CheckingAvailability:
                SetStatus("Comprobando compatibilidad AR...");
                break;

            case ARSessionState.NeedsInstall:
                SetStatus("Se necesita instalar software AR en el dispositivo");
                break;

            case ARSessionState.Installing:
                SetStatus("Instalando componentes AR...");
                break;

            case ARSessionState.Unsupported:
                SetStatus("Este dispositivo no es compatible con AR");
                break;

            default:
                sessionTrackingTimer = 0f;
                if (m_TrackedImageManager.enabled)
                    m_TrackedImageManager.enabled = false;

                SetStatus("Iniciando AR...");
                break;
        }
    }
    
    private bool HasHorizontalPlane()
    {
        if (planeManager == null)
            return false;

        foreach (var plane in planeManager.trackables)
        {
            if (plane == null) continue;

            if (plane.alignment == PlaneAlignment.HorizontalUp ||
                plane.alignment == PlaneAlignment.HorizontalDown)
            {
                return true;
            }
        }

        return false;
    }


    private void OnEnable() => m_TrackedImageManager.trackablesChanged.AddListener(OnChanged);

    private void OnDisable() => m_TrackedImageManager.trackablesChanged.RemoveListener(OnChanged);

    private void OnChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs) {
        if (!canStartImageTracking) return;
        if (m_Anchored || m_IsAnchoring) return;

        var addedSnapshot = eventArgs.added.ToList();
        var updatedSnapshot = eventArgs.updated.ToList();

        ARTrackedImage candidate =
            addedSnapshot.FirstOrDefault(img => img != null && img.trackingState == TrackingState.Tracking)
            ?? updatedSnapshot.FirstOrDefault(img => img != null && img.trackingState == TrackingState.Tracking);

        if (candidate == null) return;

        if (m_AnchorRoutine == null)
            m_AnchorRoutine = StartCoroutine(AnclajeYPrefab(candidate));
    }

    
    private IEnumerator AnclajeYPrefab(ARTrackedImage image)
    {
        if (m_Anchored || m_IsAnchoring) yield break;
        if (image == null) yield break;

        m_IsAnchoring = true;
        SetStatus("Imagen detectada. Mantén el móvil quieto...");

        yield return new WaitForSeconds(imageAnchorDelay);

        
        if (image == null || image.trackingState != TrackingState.Tracking)
        {
            SetStatus("Se perdió la imagen. Vuelve a apuntar al suelo.");
            m_IsAnchoring = false;
            m_AnchorRoutine = null;
            yield break;
        }


        Pose pose = new Pose(image.transform.position, image.transform.rotation);
        Quaternion yawOnly = Quaternion.Euler(0f, pose.rotation.eulerAngles.y, 0f);

        GameObject anchorGO = new GameObject("IndoorNavAnchor");
        anchorGO.transform.SetPositionAndRotation(pose.position, yawOnly);

        ARAnchor anchor = anchorGO.AddComponent<ARAnchor>();

        navigationBase = Instantiate(trackedImagePrefab, anchor.transform);
        sol = Instantiate(solPrefab, anchor.transform);

        StartCoroutine(WaitAndActivateLine());

        navigationBase.transform.localPosition = Vector3.zero;
        navigationBase.transform.localRotation = Quaternion.identity;

        sol.transform.localPosition = Vector3.zero;
        sol.transform.localRotation = Quaternion.identity;

        navigationTargets.Clear();
        navigationTargets = navigationBase.transform.GetComponentsInChildren<NavigationTarget>().ToList();

        navMeshSurface = navigationBase.transform.GetComponentInChildren<NavMeshSurface>();

        m_TrackedImageManager.enabled = false;

        
        m_Anchored = true;
        m_IsAnchoring = false;
        m_AnchorRoutine = null;

        SetStatus("¡Listo!");
        statusText.enabled = false;
    }
    
    private void SetStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;
    }

    private IEnumerator WaitAndActivateLine()
    {
        yield return new WaitForSeconds(11f);
        navigationLine.SetActive(true);
        lineActive = true;
    }

}