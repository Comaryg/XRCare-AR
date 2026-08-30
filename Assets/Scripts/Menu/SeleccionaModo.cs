using UnityEngine;

public class SeleccionaModo : MonoBehaviour
{
    public static string modoSeleccionado;

    public void SeleccionarModo(string modo)
    {
        modoSeleccionado = modo;
        Debug.Log("Modo seleccionado: " + modoSeleccionado);
    }
}
