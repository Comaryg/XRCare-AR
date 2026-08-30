using UnityEngine;

public class AgeSFX : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] ageClips;

    [SerializeField] private Animator animator;

    //0 = Ingles
    //1 = Español

    void Start()
    {
        if (SeleccionaModo.modoSeleccionado == "Pequeños")
        {
            if (CambioIdioma.currentLocaleIndex == 0)
            {
                audioSource.clip = ageClips[0];
                animator.SetTrigger("Pequeños?");
                audioSource.Play();
            }
            else if (CambioIdioma.currentLocaleIndex == 1)
            {
                audioSource.clip = ageClips[1];
                animator.SetTrigger("Pequeños?");
                audioSource.Play();
            }
            else if (CambioIdioma.currentLocaleIndex == 2)
            {
                audioSource.clip = ageClips[2];
                animator.SetTrigger("Pequeños?");
                audioSource.Play();
            }
            else if (CambioIdioma.currentLocaleIndex == 3)
            {
                audioSource.clip = ageClips[3];
                animator.SetTrigger("Pequeños?");
                audioSource.Play();
            }

        } else if (SeleccionaModo.modoSeleccionado == "Mayores")
        {
            if (CambioIdioma.currentLocaleIndex == 0)
            {
                audioSource.clip = ageClips[4];
                animator.SetTrigger("Mayores?");
                audioSource.Play();
            }
            else if (CambioIdioma.currentLocaleIndex == 1)
            {
                audioSource.clip = ageClips[5];
                animator.SetTrigger("Mayores?");
                audioSource.Play();
            }
            else if (CambioIdioma.currentLocaleIndex == 2)
            {
                audioSource.clip = ageClips[6];
                animator.SetTrigger("Mayores?");
                audioSource.Play();
            }
            else if (CambioIdioma.currentLocaleIndex == 3)
            {
                audioSource.clip = ageClips[7];
                animator.SetTrigger("Mayores?");
                audioSource.Play();
            }
        }
    }
}
