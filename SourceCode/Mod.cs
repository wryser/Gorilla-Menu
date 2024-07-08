using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace GorillaMenu
{
    internal class Mod : MonoBehaviour
    {
        public GameObject uiSettings;
        public Button uiButton;

        void Start()
        {
            Debug.Log("Start Loaded in Mod.cs");
            uiButton.onClick.AddListener(OpenUI);
        }

        public void OpenUI()
        {
            Debug.Log("OpenUI Loaded in Mod.cs");
            uiSettings.SetActive(true);
            Plugin.instance.ModList.SetActive(false);
            Plugin.instance.mainPage.SetActive(false);
            Plugin.instance.currentMenu = uiSettings;
        }
    }
}
