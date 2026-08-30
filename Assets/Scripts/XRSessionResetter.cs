using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Management;

/// <summary>
/// Limpia el estado global de AR entre cambios de escena. Resuelve dos problemas
/// que aparecen al salir de la escena AR y volver a entrar (sobre todo en el
/// Editor con XR Simulation):
///
/// 1) AssertionException en TrackableSpawner.RegisterCreatedTrackable:
///    TrackableSpawner es un singleton estático interno que registra los trackables
///    por trackableId y NO se limpia entre escenas. Al re-entrar, el subsistema
///    reutiliza un trackableId ya registrado y la aserción falla. -> Reset por reflexión.
///
/// 2) ObjectDisposedException (CancellationTokenSource has been disposed) en
///    SimulationTrackedImageDiscoverer.Stop: el subsistema de XR Simulation se
///    reutiliza en un estado corrupto tras recargar la escena. -> Recrear los
///    subsistemas XR (Deinitialize + Initialize del loader).
///
/// Se auto-crea al iniciar la app; NO hay que colocarlo en ninguna escena.
/// </summary>
public class XRSessionResetter : MonoBehaviour
{
    private static XRSessionResetter _instance;
    private static MethodInfo s_ResetTrackableSpawner;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (_instance != null) return;

        CacheReflection();

        var go = new GameObject("[XRSessionResetter]");
        _instance = go.AddComponent<XRSessionResetter>();
        DontDestroyOnLoad(go);
    }

    private static void CacheReflection()
    {
        // TrackableSpawner es 'internal' dentro del ensamblado de AR Foundation.
        // Lo alcanzamos a través de un tipo público del mismo ensamblado (ARAnchor).
        var spawnerType = typeof(UnityEngine.XR.ARFoundation.ARAnchor)
            .Assembly.GetType("UnityEngine.XR.ARFoundation.TrackableSpawner");

        if (spawnerType == null)
        {
            Debug.LogWarning("[XRSessionResetter] No se encontró TrackableSpawner. " +
                             "¿Ha cambiado la versión de AR Foundation?");
            return;
        }

        s_ResetTrackableSpawner = spawnerType.GetMethod(
            "ResetInstance", BindingFlags.Static | BindingFlags.NonPublic);

        if (s_ResetTrackableSpawner == null)
            Debug.LogWarning("[XRSessionResetter] No se encontró TrackableSpawner.ResetInstance().");
    }

    private void OnEnable()  => SceneManager.sceneUnloaded += OnSceneUnloaded;
    private void OnDisable() => SceneManager.sceneUnloaded -= OnSceneUnloaded;

    private void OnSceneUnloaded(Scene _)
    {
        // sceneUnloaded se dispara cuando la escena anterior ya se destruyó (sus
        // managers AR ya han parado sus subsistemas en OnDisable) y ANTES de que
        // se habiliten los de la nueva escena: momento ideal para limpiar.

        // 1) Limpia el singleton estático de trackables -> arregla el AssertionException.
        try
        {
            s_ResetTrackableSpawner?.Invoke(null, null);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[XRSessionResetter] No se pudo resetear TrackableSpawner: {e.Message}");
        }

        // 2) Recrea los subsistemas XR desde cero -> arregla el estado corrupto de
        //    XR Simulation (CancellationTokenSource disposed, etc.).
        RestartXR();
    }

    private void RestartXR()
    {
        var xr = XRGeneralSettings.Instance != null ? XRGeneralSettings.Instance.Manager : null;
        if (xr == null) return;

        if (xr.isInitializationComplete)
        {
            // Cada paso en su propio try/catch: aunque un subsistema roto lance al
            // pararse, queremos completar la deinicialización y volver a inicializar.
            try { xr.StopSubsystems(); }
            catch (Exception e) { Debug.LogWarning($"[XRSessionResetter] StopSubsystems: {e.Message}"); }

            try { xr.DeinitializeLoader(); }
            catch (Exception e) { Debug.LogWarning($"[XRSessionResetter] DeinitializeLoader: {e.Message}"); }
        }

        try
        {
            // Reinicialización síncrona: deja el loader listo antes de que la nueva
            // escena cargue. NO llamamos a StartSubsystems: los managers AR de la
            // nueva escena (ARSession, ARTrackedImageManager...) los arrancan al
            // habilitarse, así la cámara no queda encendida en el menú.
            xr.InitializeLoaderSync();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[XRSessionResetter] InitializeLoaderSync: {e.Message}");
        }
    }
}
