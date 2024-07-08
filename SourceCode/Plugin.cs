using BepInEx;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using TMPro;
using GorillaMenu.Utils;
using GorillaLocomotion;
using HarmonyLib;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GorillaMenu
{
    /// <summary>
    /// This is your mod's main class.
    /// </summary>

    /* This attribute tells Utilla to look for [ModdedGameJoin] and [ModdedGameLeave] */
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        GameObject menuasset;
        GameObject menu;
        GameObject modButtonAsset;
        public GameObject ModList;
        public GameObject ModPages;
        public GameObject mainPage;
        public GameObject currentMenu;
        bool isMenuOpen;
        bool leftSec;
        Transform rHand;
        GameObject testCube;
        public LineRenderer lr;
        int layer_mask;
        Vector3 uiHitPos;
        bool trigger;
        bool canPress = true;
        public static Plugin instance;
        AudioSource clickSfx;
        public bool hasModInitiated = false;
        bool playedHaptic = false;
        float ypos;
        GameObject menuParent;

        void Awake()
        {
            instance = this;
            Debug.Log("Instance Defined");
        }

        public void Start()
        {
            GorillaTagger.OnPlayerSpawned(PlayerSpawned);
        }

        void PlayerSpawned()
        {
            /* Code here runs after the game initializes (i.e. GorillaLocomotion.Player.Instance != null) */
            Stream str = Assembly.GetExecutingAssembly().GetManifestResourceStream("GorillaMenu.Assets.gorillamenu");
            AssetBundle bundle = AssetBundle.LoadFromStream(str);
            menuasset = bundle.LoadAsset<GameObject>("GorillaMenu");
            modButtonAsset = bundle.LoadAsset<GameObject>("Mod");
            menu = Instantiate(menuasset);
            ModList = GameObject.Find("GorillaMenu(Clone)/Canvas/Mods");
            ModPages = GameObject.Find("GorillaMenu(Clone)/Canvas/ModPages");
            mainPage = GameObject.Find("GorillaMenu(Clone)/MainMenu");
            lr = Player.Instance.gameObject.AddComponent<LineRenderer>();
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = Color.white;
            lr.endColor = Color.white;
            lr.startWidth = 0.02f;
            lr.endWidth = 0.02f;
            rHand = GameObject.Find("Player Objects/Local VRRig/Local Gorilla Player/rig/body/shoulder.R/upper_arm.R/forearm.R/hand.R").GetComponent<Transform>();
            testCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            testCube.GetComponent<BoxCollider>().enabled = false;
            testCube.GetComponent<Transform>().localScale = new Vector3(0.3f, 0.3f, 0.3f);
            testCube.transform.localPosition = new Vector3(1f, 0f, 0f);
            testCube.transform.position = rHand.position;
            testCube.transform.localRotation = Quaternion.Euler(0f, 270f, 270f);
            testCube.GetComponent<Transform>().SetParent(rHand);
            layer_mask = LayerMask.GetMask("UI");
            clickSfx = menu.GetComponent<AudioSource>();
            menuParent = new GameObject("menuparent");
            menu.transform.SetParent(menuParent.transform, false);
            menu.transform.localPosition = new Vector3(menu.transform.position.x, menu.transform.position.y, 0.7f);
            hasModInitiated = true;
        }

        void Update()
        {
            leftSec = ControllerInputPoller.instance.leftControllerSecondaryButton;
            if (leftSec && !isMenuOpen)
            {
                menu.SetActive(true);
                menuParent.transform.position = Player.Instance.headCollider.transform.position;
                menuParent.transform.eulerAngles = new Vector3(0, Player.Instance.headCollider.transform.eulerAngles.y, 0);
                ypos = Player.Instance.headCollider.transform.position.y;
                isMenuOpen = true;
            }
            if (leftSec)
            {
                menuParent.transform.position = Vector3.Lerp(menuParent.transform.position, Player.Instance.bodyCollider.transform.position, 8 * Time.deltaTime);
                menuParent.transform.position = new Vector3(menuParent.transform.position.x, ypos, menuParent.transform.position.z);
            }
            else if (!leftSec)
            {
                mainPage.SetActive(true);
                ModList.SetActive(true);
                menu.SetActive(false);
                if(currentMenu != null)
                {
                    currentMenu.SetActive(false);
                }
                isMenuOpen = false;
            }
            if (ControllerInputPoller.instance.rightControllerIndexFloat > 0.5f) //right trigger
            {
                trigger = true;
            }
            else
            {
                trigger = false;
            }
            RaycastHit hit;
            if (Physics.Raycast(testCube.transform.position, testCube.transform.right, out hit, 100f, layer_mask))
            {
                uiHitPos = hit.point;

                if (hit.collider.gameObject.layer == 5 && hit.collider.gameObject.name != "BG")
                {
                    lr.positionCount = 2;
                    DrawLine();
                    if (hit.collider.gameObject.GetComponent<Button>() && canPress && trigger)
                    {
                        hit.collider.gameObject.GetComponent<Button>().onClick.Invoke();
                        clickSfx.Play();
                        canPress = false;
                    }
                    if (!trigger)
                    {
                        canPress = true;
                    }
                }
                else
                {
                    RemoveLine();
                }
            }
            else
            {
                RemoveLine();
            }
        }

        void DrawLine()
        {
            if (!playedHaptic)
            {
                GorillaTagger.Instance.StartVibration(false, GorillaTagger.Instance.taggedHapticStrength / 3, GorillaTagger.Instance.tapHapticDuration);
                playedHaptic = true;
            }
            lr.SetPosition(0, testCube.transform.position);
            lr.SetPosition(1, uiHitPos);
        }

        void RemoveLine()
        {
            playedHaptic = false;
            lr.positionCount = 0;
        }

        public void AddMod(string name, GameObject ui)
        {
            Debug.Log("UI LOADED NAME IS: " + ui.name);
            GameObject modButton = Instantiate(modButtonAsset);
            modButton.transform.SetParent(ModList.transform, false);
            modButton.name = name;
            modButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = name.ToUpper();
            Mod modButtonMod = modButton.AddComponent<Mod>();
            ui.transform.SetParent(ModPages.transform, false);
            modButtonMod.uiSettings = ui;
            modButtonMod.uiButton = modButton.GetComponent<Button>();
            ui.SetActive(false);
        }
    }
}
