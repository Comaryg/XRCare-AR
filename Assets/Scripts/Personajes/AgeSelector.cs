using UnityEngine;

public class AgeSelector : MonoBehaviour
{
    [SerializeField] private GameObject delfin;
    [SerializeField] private GameObject virus;

    void Start()
    {
        if (SeleccionaModo.modoSeleccionado == "Pequeños")
        {
            delfin.SetActive(true);
            virus.SetActive(false);
        }

        if (SeleccionaModo.modoSeleccionado == "Mayores")
        {
            virus.SetActive(true);
            delfin.SetActive(false);
        }
    }

}
