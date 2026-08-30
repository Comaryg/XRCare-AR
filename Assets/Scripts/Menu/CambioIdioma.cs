using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class CambioIdioma : MonoBehaviour
{
    [SerializeField] private List<string> localeCodes;
    public static int currentLocaleIndex = 1;


    public void SetLanguage()
    {
        Locale newLocale = LocalizationSettings.AvailableLocales.GetLocale(localeCodes[currentLocaleIndex]);
        LocalizationSettings.SelectedLocale = newLocale;
        currentLocaleIndex = (currentLocaleIndex + 1) % localeCodes.Count;
        print(currentLocaleIndex);
    }
}
