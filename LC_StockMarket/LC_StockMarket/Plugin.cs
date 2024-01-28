using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using LethalLib.Modules;
using GameNetcodeStuff;
using System.IO;
using Unity.Netcode;
using LC_StockMarketIndex.Patches;
using UnityEngine.SceneManagement;
using SAM;

namespace LC_StockMarketIndex
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class StockMarketIndexMod : BaseUnityPlugin
    {
        private const string modGUID = "Mellowdy.StockMarketIndex";
        private const string modName = "StockMarketIndex";
        private const string modVersion = "0.0.1";

        private const string assetName = "device.stock";
        private const string gameObjectName = "StockIndex.prefab";

        private readonly Harmony harmony = new Harmony(modGUID);

        public static ManualLogSource mls;
        private static StockMarketIndexMod instance;



        static string Name = "Stock Index";
        static string Description = "Allows you to buy and sell speculative assets.";

        static ConfigEntry<int> price;
        static ConfigEntry<bool> voice;

        static Vector3 positionInHand = new Vector3(-0.08f, -0.05f, 0.07f);
        static Vector3 rotationInHand = new Vector3(-165, -75f, 0);

        static float size = 4f;

        public static string[] Companies = {"Apple","Haldan Electronics","FacePunch","The Company", "Kremmer's crematorium", "Zeekers"}; 


        void Awake()
        {
            if (instance == null) instance = this;

            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);

            harmony.PatchAll(typeof(StockMarketIndexMod));


            //Assign Config Settings

            price = Config.Bind<int>("Price", "DevicePrice", 80, "Credits needed to buy the hand-held device");
            voice = Config.Bind<bool>("Voice", "DeviceVoiceOn", true, "If the device talkt to you or not.");

            //Asset Bundle

            string currentDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string path = Path.Combine(currentDirectory, assetName).Replace("\\", "/");

            mls.LogMessage(path);

            AssetBundle assets = AssetBundle.LoadFromFile(path);

            Item device = ScriptableObject.CreateInstance<Item>();

            GameObject deviceObject = assets.LoadAsset<GameObject>(gameObjectName);


            //Sounds

            device.grabSFX = assets.LoadAsset<AudioClip>("Grab.wav");
            device.pocketSFX = assets.LoadAsset<AudioClip>("Pocket.wav");
            device.dropSFX = assets.LoadAsset<AudioClip>("Drop.wav");
            device.throwSFX = device.dropSFX;

            //Item
            #region

            device.name = Name;
            device.itemName = Name;

            device.restingRotation = new Vector3(0, 90, 0);
            device.canBeGrabbedBeforeGameStart = false;
            device.isConductiveMetal = true;
            device.isScrap = false;
            device.canBeInspected = true;
            device.itemIcon = assets.LoadAsset<Sprite>("Icon.png");

            device.rotationOffset = rotationInHand;
            var positions = positionInHand / 3f * size;
            device.positionOffset = new Vector3(positions.y, positions.z, positions.x);

            device.batteryUsage = 0;
            device.requiresBattery = false;
            device.automaticallySetUsingPower = false;

            device.toolTips = new string[] { "Next : [LMB]", "Buy : [Q]", "Sell : [E]"};

            #endregion

            //GameObject
            #region

            //Assign Components

            deviceObject.transform.localScale = Vector3.one * size;

            deviceObject.AddComponent<NetworkObject>();
            deviceObject.AddComponent<AudioSource>();
            StockMarketIndex smi = deviceObject.AddComponent<StockMarketIndex>();
            
            smi.originalScale = deviceObject.transform.localScale;
            smi.grabbable = true;
            smi.itemProperties = device;
            smi.floorYRot = -1;

            device.spawnPrefab = deviceObject;
            SceneManager.sceneLoaded += StockMarketIndex.FindTerminal;
            #endregion

            Items.RegisterItem(device);
            Items.RegisterShopItem(device, null, null, CreateInfoNode(Name, Description), price.Value);
            mls.LogInfo($"{modName} is active");

            
        }
        private TerminalNode CreateInfoNode(string name, string description)
        {
            TerminalNode val = ScriptableObject.CreateInstance<TerminalNode>();
            val.clearPreviousText = true;
            val.name = name + "InfoNode";
            val.displayText = description + "\n\n";
            return val;
        }
    }
}
