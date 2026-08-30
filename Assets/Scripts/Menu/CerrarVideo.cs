using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class CerrarVideo : MonoBehaviour
{
    [SerializeField] private VideoPlayer videoPlayer;

    void Awake()
    {
        videoPlayer.loopPointReached += OnVideoEnd;
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        Debug.Log("Video has ended.");
        SceneManager.LoadScene("IndoorNav");
    }

    void OnDestroy()
    {
        videoPlayer.loopPointReached -= OnVideoEnd;
    }
}
