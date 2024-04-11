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
using System.Collections.Generic;
using System.Linq;
using System;
using RuntimeNetcodeRPCValidator;
using System.CodeDom;

namespace LC_HoardingBugSnacks
{
    [BepInPlugin(modGUID, modName, modVersion)]
    [BepInDependency(RuntimeNetcodeRPCValidator.MyPluginInfo.PLUGIN_GUID, RuntimeNetcodeRPCValidator.MyPluginInfo.PLUGIN_VERSION)]
    public class HoardingBugSnacksMod : BaseUnityPlugin
    {
        private const string modGUID = "Mellowdy.HoardingBugSnacks";
        private const string modName = "HoardingBugSnacks";
        private const string modVersion = "1.0.1";

        private const string assetName = "bug.snack";
        private const string gameObjectName = "BugSnacks.prefab";

        private readonly Harmony harmony = new Harmony(modGUID);
        private NetcodeValidator validator = new NetcodeValidator(modGUID);

        public static ManualLogSource mls;
        public static HoardingBugSnacksMod instance;

        static string Name = "Hoarding Bug Snacks";
        static string Description = "Use to sedate the common hoarding bug by means of peace offering.";

        static ConfigEntry<int> price;
        public static ConfigEntry<int> friendlyTime;
        public static ConfigEntry<int> danceTime;
        public static ConfigEntry<int> bugsToSpawn;
        public static ConfigEntry<int> randomBugsToSpawn;
        public static ConfigEntry<int> shotgunChance;
        public static Item shotgunItem;

        public static EnemyType hoarderType;

        void Awake()
        {
            if (instance == null) instance = this;

            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);

            harmony.PatchAll(typeof(SpawnPatch));
            harmony.PatchAll();
            validator.PatchAll();

            #region Set up item

            //Assign Config Settings

            price = Config.Bind<int>("Item", "Price", 10, "");
            friendlyTime = Config.Bind<int>("Item", "Friendliness", 60, "How long are the bugs friendly for? (in seconds)");
            danceTime = Config.Bind<int>("Item", "Dance", 10, "How long should the bugs dance for? (in seconds)");
            bugsToSpawn = Config.Bind<int>("Bugs", "Spawn Always", 1, "How many bugs should always spawn at the start of a round");
            randomBugsToSpawn = Config.Bind<int>("Bugs", "Spawn Random", 3, "How many bugs extra bugs CAN randomly spawn in addition to the guaranteed bugs");
            shotgunChance = Config.Bind<int>("Bugs", "Shotgun chance", 10, "Chance for bug to spawn with a shotgun (100 = 100%) (they found the US constitution and really liked the second part)");

            //Asset Bundle

            string currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string path = Path.Combine(currentDirectory, assetName).Replace("\\", "/");

            mls.LogMessage(path);

            AssetBundle assets = AssetBundle.LoadFromFile(path);
            Item item = assets.LoadAsset<Item>("BugSnacks.asset");

            GameObject Object = assets.LoadAsset<GameObject>(gameObjectName);
            var bugSnacks = Object.AddComponent<BugSnacks>();

            bugSnacks.useCooldown = 1f;
            bugSnacks.itemProperties = item;
            bugSnacks.grabbable = true;
            bugSnacks.grabbableToEnemies = true;

            bugSnacks.shake = assets.LoadAsset<AudioClip>("shake.wav");

            #endregion

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