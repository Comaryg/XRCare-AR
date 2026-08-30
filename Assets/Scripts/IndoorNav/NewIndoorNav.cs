using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class NewIndoorNav : MonoBehaviour {
    [SerializeField] private Transform player;
    [SerializeField] private ARTrackedImageManager m_TrackedImageManager;
    [SerializeField] private ARAnchorManager m_AnchorManager;
    [SerializeField] private GameObject trackedImagePrefab;
    [SerializeField] private GameObject solPrefab;
    [SerializeField] private LineRenderer line;

    private List<NavigationTarget> navigationTargets = new List<NavigationTarget>();
    private NavMeshSurface navMeshSurface;
    private NavMeshPath navMeshPath;

    private GameObject navigationLine;
    private GameObject navigationBase;
    private GameObject sol;
    
    private bool m_IsAnchoring = false;
    private bool m_Anchored = false;
    private Coroutine m_AnchorRoutine;

    [SerializeField] float requiredStableTime = 0.6f;   // tiempo mínimo estable
    [SerializeField] float maxYawJitterDeg = 2.0f;      // jitter permitido en grados
    [SerializeField] int   sampleWindow = 20;           // nº de muestras para mediana

    private readonly Queue<float> yawSamples = new Queue<float>();
    
    [SerializeField] TMPro.TextMeshProUGUI statusText;
    [SerializeField] TMPro.TextMeshProUGUI debugText;
    [SerializeField] UnityEngine.UI.Image stabilityMeter;

    private void Start() {
        statusText.text = "Buscando marcador...";
        stabilityMeter.fillAmount = 0f;
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
        if (m_Anchored || m_IsAnchoring) return;

        var addedSnapshot = eventArgs.added.ToList();
        var updatedSnapshot = eventArgs.updated.ToList();
        
        ARTrackedImage candidate =
            addedSnapshot.FirstOrDefault(img => img != null && img.trackingState == TrackingState.Tracking)
            ?? updatedSnapshot.FirstOrDefault(img => img != null && img.trackingState == TrackingState.Tracking);

        if (candidate == null) return;
        m_AnchorRoutine = StartCoroutine(AnclajeYPrefab(candidate));
    }

    private IEnumerator AnclajeYPrefab(ARTrackedImage image) {
            
        if (m_Anchored || m_IsAnchoring) yield break;
        if (image == null) yield break;

        m_IsAnchoring = true;

        while (image != null && image.trackingState != TrackingState.Tracking)
            {
                statusText.text = "Buscando marcador...";
                stabilityMeter.fillAmount = 0f;
                yield return null;
            }

        if (image == null)
        {
            m_IsAnchoring = false;
            yield break;
        }

        yawSamples.Clear();
        float stableTimer = 0f;

        float lastYaw = GetYaw(image.transform.rotation);

        while (stableTimer < requiredStableTime)
        {
            if (image == null || image.trackingState != TrackingState.Tracking)
            {
                m_IsAnchoring = false;
                yield break;
            }

            float yaw = GetYaw(image.transform.rotation);

            // Detecta saltos grandes (típico “me ha cambiado de orientación”)
            float delta = Mathf.DeltaAngle(lastYaw, yaw);
            lastYaw = yaw;

            if (Mathf.Abs(delta) > 15f) // umbral anti “flip/spin”
            {
                stableTimer = 0f;
                yawSamples.Clear();
                statusText.text = "Mantén el móvil quieto y apunta al marcador";
                yield return null;
                continue;
            }

            EnqueueYaw(yaw);

            float medianYaw = MedianYaw(yawSamples);

            // ¿Cuánto se está moviendo respecto a la mediana?
            float err = Mathf.Abs(Mathf.DeltaAngle(medianYaw, yaw));

            if (err <= maxYawJitterDeg)
                stableTimer += Time.deltaTime;
            else
                stableTimer = 0f;

            statusText.text = "Marcador detectado. Estabilizando...";
            stabilityMeter.fillAmount = Mathf.Clamp01(stableTimer / requiredStableTime);

            debugText.text =
            $"Tracking: {image.trackingState}\n" +
            $"Yaw: {yaw:0.0}°\n" +
            $"Estable: {stableTimer:0.00}/{requiredStableTime:0.00}s";

            yield return null;
        }

        // Ya tenemos yaw estable (usa la mediana como “yaw final”)
        float finalYaw = MedianYaw(yawSamples);
        finalYaw = Mathf.Round(finalYaw / 90f) * 90f;
        Quaternion yawOnly = Quaternion.Euler(0f, finalYaw, 0f);

        GameObject anchorNavigation = new GameObject("IndoorNavAnchor");
        anchorNavigation.transform.SetPositionAndRotation(image.transform.position, yawOnly); //yawOnly

        ARAnchor anchor = anchorNavigation.AddComponent<ARAnchor>();

        navigationBase = Instantiate(trackedImagePrefab, anchor.transform);
        sol = Instantiate(solPrefab, anchor.transform);

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
    }

    float GetYaw(Quaternion q) => q.eulerAngles.y;

    void EnqueueYaw(float yaw)
    {
        yawSamples.Enqueue(yaw);
        while (yawSamples.Count > sampleWindow)
            yawSamples.Dequeue();
    }

    float MedianYaw(IEnumerable<float> angles)
    {
        // Para mediana circular simple: convertimos a deltas respecto al primero
        // (suficiente para ventanas pequeñas donde no cruzas 0/360 constantemente)
        var list = angles.ToList();
        if (list.Count == 0) return 0f;
        float refA = list[0];
        var deltas = list.Select(a => Mathf.DeltaAngle(refA, a)).OrderBy(x => x).ToList();
        float medDelta = deltas[deltas.Count / 2];
        return Mathf.Repeat(refA + medDelta, 360f);
    }

}
