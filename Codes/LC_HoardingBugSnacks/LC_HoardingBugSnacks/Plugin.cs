using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using LethalLib.Modules;
using GameNetcodeStuff;
using System.IO;
using UnityEngine.SceneManagement;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Numerics;
using LC_HoardingBugSnacks.Patches;


namespace LC_HoardingBugSnacks
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class HoardingBugSnacksMod : BaseUnityPlugin
    {
        private const string modGUID = "Mellowdy.HoardingBugSnacks";
        private const string modName = "HoardingBugSnacks";
        private const string modVersion = "0.0.1";

        private const string assetName = "bug.snack";
        private const string gameObjectName = "BugSnacks.prefab";

        private readonly Harmony harmony = new Harmony(modGUID);

        public static ManualLogSource mls;
        private static HoardingBugSnacksMod instance;

        static string Name = "Hoarding Bug Snacks";
        static string Description = "";

        static ConfigEntry<int> price;

        public static Item shotgunItem;

        void Awake()
        {
            if (instance == null) instance = this;

            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);

            harmony.PatchAll(typeof(SpawnPatch));
            harmony.PatchAll();

            #region Set up item

            //Assign Config Settings

            price = Config.Bind<int>("Price", "Snacks Price", 5, "Seals needed to buy the Seals");

            //Asset Bundle

            string currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string path = Path.Combine(currentDirectory, assetName).Replace("\\", "/");

            mls.LogMessage(path);

            AssetBundle assets = AssetBundle.LoadFromFile(path);
            Item item = assets.LoadAsset<Item>("BugSnacks.asset");

            GameObject Object = assets.LoadAsset<GameObject>(gameObjectName);
            var bugSnacks = Object.AddComponent<BugSnacks>();

            bugSnacks.itemProperties = item;
            bugSnacks.grabbable = true;
            bugSnacks.grabbableToEnemies = true;



            #endregion

            foreach(Item maybeShotgun in Resources.FindObjectsOfTypeAll(typeof(Item)))
            {
                if(maybeShotgun.itemName == "Shotgun") shotgunItem = maybeShotgun;
            }

            


            //Add to shop
            Items.RegisterItem(item);
            Items.RegisterShopItem(item, null, null, CreateInfoNode(Name, Description), price.Value);

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