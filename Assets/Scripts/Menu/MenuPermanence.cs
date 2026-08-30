using Unity.XR.CoreUtils;
using UnityEngine;

public class MenuPermanence : MonoBehaviour
{
    [SerializeField] private GameObject menuPrincipal;
    [SerializeField] private GameObject menuPequeños;
    [SerializeField] private GameObject menuMayores;

    void Start()
    {
        if (SeleccionaModo.modoSeleccionado == "Pequeños")
        {
            menuPrincipal.SetActive(false);
            menuPequeños.SetActive(true);
            menuMayores.SetActive(false);
        } else if (SeleccionaModo.modoSeleccionado == "Mayores")
        {
            menuPrincipal.SetActive(false);
            menuPequeños.SetActive(false);
            menuMayores.SetActive(true);
        } else
        {
            menuPrincipal.SetActive(true);
            menuPequeños.SetActive(false);
            menuMayores.SetActive(false);
        }
    }
}
