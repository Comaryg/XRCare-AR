using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;

public class CambioEscenas : MonoBehaviour
{
    public void CambiarEscena(string nombreEscena)
    {
        StartCoroutine(CambiarEscenaRoutine(nombreEscena));
    }

    private IEnumerator CambiarEscenaRoutine(string nombreEscena)
    {
        // Destruye los ARAnchor creados en runtime ANTES de descargar la escena.
        // Así su OnDisable llama a TryRemoveAnchor mientras el ARAnchorManager
        // sigue vivo, evitando el MissingReferenceException por orden de destrucción.
        foreach (var anchor in FindObjectsByType<ARAnchor>(FindObjectsSortMode.None))
        {
            if (anchor != null)
                Destroy(anchor.gameObject);
        }

        // Deja pasar un frame para que se procese la destrucción de los anchors
        // con el manager todavía presente.
        yield return null;

        SceneManager.LoadScene(nombreEscena);
    }
}
