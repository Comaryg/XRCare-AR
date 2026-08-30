using System.Collections;
using UnityEngine;

public class NavigationController : MonoBehaviour
{
    public static int locationStep = 0;

    public void advanceStep()
    {
        StartCoroutine(step());
    }

    public IEnumerator step()
    {
        locationStep += 2;
        yield return new WaitForSeconds(9f);
        locationStep -= 2;
        locationStep += 1;
    }

}
