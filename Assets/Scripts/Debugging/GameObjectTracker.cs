using UnityEngine;

public class GameObjectTracker : MonoBehaviour
{

    [SerializeField] private GameObject _objectToTrack;

    void Update()
    {
        Debug.Log($"El Objeto: {_objectToTrack.name} está en: {_objectToTrack.transform.position}");
    }
}
