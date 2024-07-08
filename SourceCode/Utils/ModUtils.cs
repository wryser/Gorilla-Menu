using UnityEngine;
using TMPro;

namespace GorillaMenu.Utils
{
    public static class ModUtils
    {
        public static void AddMod(string name, GameObject ui)
        {
            Debug.Log(ui.name);
            Plugin.instance.AddMod(name, ui);
        }
    }
}