using System;
using System.Collections;
using UnityEngine;

public class SecondaryCharController : MonoBehaviour
{
    [SerializeField] private Animator animator_delfin;
    [SerializeField] private GameObject delfin;
    [SerializeField] private Animator animator_virus;
    [SerializeField] private GameObject virus;

    void Start()
    {
        StartCoroutine(Animacion());
    }

    IEnumerator Animacion()
    {
        if (delfin.activeInHierarchy == true)
        {
            print("Delfin activo");
            yield return new WaitForSeconds(8f);
            animator_delfin.SetTrigger("Acaba?");
            yield return new WaitForSeconds(7f);
            delfin.SetActive(false);
        }

        if (virus.activeInHierarchy == true)
        {
            print("Virus activo");
            yield return new WaitForSeconds(6f);
            animator_virus.SetTrigger("Acaba?");
            yield return new WaitForSeconds(5f);
            virus.SetActive(false);
        }
    }
}
