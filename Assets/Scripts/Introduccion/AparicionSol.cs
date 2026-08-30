using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;

public class AparicionSol : MonoBehaviour
{
    [SerializeField] private ARTrackedImageManager m_TrackedImageManager;
    [SerializeField] private GameObject trackedImagePrefab;

    private GameObject sol;

    private void OnEnable() => m_TrackedImageManager.trackablesChanged.AddListener(OnChanged);

    private void OnDisable() => m_TrackedImageManager.trackablesChanged.RemoveListener(OnChanged);

    private void OnChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
    {
        Debug.Log("Imagen?");

        foreach (var newImage in eventArgs.added)
        {
            sol = Instantiate(trackedImagePrefab, newImage.transform);
            //sol.transform.SetPositionAndRotation(newImage.pose.position, Quaternion.Euler(0, newImage.pose.rotation.eulerAngles.y, 0));
            Debug.Log("Sale" + newImage.referenceImage.name);
        }

        foreach (var updatedImage in eventArgs.updated)
        {
            sol.transform.SetPositionAndRotation(updatedImage.pose.position, Quaternion.Euler(0, updatedImage.pose.rotation.eulerAngles.y, 0));
            Debug.Log("Actualiza" + updatedImage.referenceImage.name);
        }

        foreach (var removedImage in eventArgs.removed)
        {

        }
    }
}
