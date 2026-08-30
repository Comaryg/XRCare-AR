using UnityEngine;
using UnityEngine.Video;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class CambiaVideoIdioma : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public LocalizedAsset<VideoClip> localizedVideo;

    void Reset()
    {
        videoPlayer = GetComponent<VideoPlayer>();
    }

    void OnEnable()
    {
        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();

        // AssetChanged se dispara al suscribirse, cuando termina la carga
        // y cada vez que cambia el idioma. La propia localización se encarga
        // de liberar el clip anterior al cambiar de idioma.
        localizedVideo.AssetChanged += OnVideoChanged;
    }

    void OnDisable()
    {
        localizedVideo.AssetChanged -= OnVideoChanged;

        if (videoPlayer != null)
        {
            videoPlayer.prepareCompleted -= OnPrepared;
            videoPlayer.errorReceived -= OnVideoError;
        }
    }

    void OnVideoChanged(VideoClip clip)
    {
        string code = LocalizationSettings.SelectedLocale != null
            ? LocalizationSettings.SelectedLocale.Identifier.Code
            : "??";

        if (clip == null)
        {
            // ESTO es lo que hay que vigilar en el dispositivo (adb logcat -s Unity):
            // si aparece, la carga del vídeo localizado ha fallado (normalmente por
            // memoria con vídeos muy grandes) y el VideoPlayer se queda con el clip
            // por defecto asignado en el inspector (el español).
            Debug.LogError(
                $"[CambiaVideoIdioma] El clip localizado llegó NULL (idioma={code}). " +
                "Se mantiene el clip por defecto del VideoPlayer. " +
                "Revisa que el vídeo esté en Addressables y que no sea demasiado " +
                "grande para cargarse en memoria en el dispositivo.", this);
            return;
        }

        Debug.Log($"[CambiaVideoIdioma] Cambiando vídeo a '{clip.name}' (idioma={code}).", this);

        videoPlayer.Stop();
        videoPlayer.source = VideoSource.VideoClip;
        videoPlayer.clip = clip;

        // Preparar antes de reproducir: en Android reproducir un clip recién asignado
        // sin preparar puede fallar o mostrar el frame anterior.
        videoPlayer.prepareCompleted -= OnPrepared;
        videoPlayer.prepareCompleted += OnPrepared;
        videoPlayer.errorReceived -= OnVideoError;
        videoPlayer.errorReceived += OnVideoError;
        videoPlayer.Prepare();
    }

    void OnPrepared(VideoPlayer vp)
    {
        vp.prepareCompleted -= OnPrepared;
        vp.Play();
    }

    void OnVideoError(VideoPlayer vp, string message)
    {
        Debug.LogError($"[CambiaVideoIdioma] Error del VideoPlayer: {message}", this);
    }
}
