using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using LethalLib.Modules;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using RuntimeNetcodeRPCValidator;
using System;

namespace LC_LethalEnergy
{

    [BepInPlugin(modGUID, modName, modVersion)]
    [BepInDependency(MyPluginInfo.PLUGIN_GUID,MyPluginInfo.PLUGIN_VERSION)]
    public class LethalEnergyMod : BaseUnityPlugin
    {
        private const string modGUID = "Mellowdy.LethalEnergy";
        private const string modName = "LethalEnergy";
        private const string modVersion = "0.0.1";

        private readonly Harmony harmony = new Harmony(modGUID);
        private readonly NetcodeValidator validator = new NetcodeValidator(modGUID);

        public static ManualLogSource mls;

        private static LethalEnergyMod instance;
        public static AssetBundle assets;

        public static string assetName = "monster.energy";
        public static string canPrefabName = "monstercan.prefab";
        public static string casePrefabName = "monstercase.prefab";

        void Awake()
        {
            if (instance == null) instance = this;
            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);

            //Load Asset Bundle
            string currentDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string path = Path.Combine(currentDirectory, assetName).Replace("\\", "/");
            assets = AssetBundle.LoadFromFile(path);
            /*
            foreach(string s in assets.GetAllAssetNames())
                mls.LogMessage(s);
            */


            //Case

            GameObject Case = assets.LoadAsset<GameObject>(casePrefabName);
            Item caseItem = assets.LoadAsset<Item>("case.asset");
            caseItem.spawnPrefab = Case;

            DrinkCase drinkCase = Case.AddComponent<DrinkCase>();
            drinkCase.itemProperties = caseItem;
            drinkCase.grabbable = true;

            //Can

            GameObject Can = assets.LoadAsset<GameObject>(canPrefabName);
            Item canItem = assets.LoadAsset<Item>("Can.asset");
            canItem.spawnPrefab = Can;

            LethalCan lethalCan = Can.AddComponent<LethalCan>();
            lethalCan.itemProperties = canItem;
            lethalCan.grabbable = true;
            lethalCan.drinkingProp = assets.LoadAsset<Item>("Drinking.asset");

            DrinkCase.canPrefab = Can;

            NetworkPrefabs.RegisterNetworkPrefab(caseItem.spawnPrefab);
            Items.RegisterScrap(caseItem,100);
            NetworkPrefabs.RegisterNetworkPrefab(canItem.spawnPrefab);
            Items.RegisterScrap(canItem,0);

            harmony.PatchAll();
            harmony.PatchAll(typeof(PlayerPatches));
            validator.PatchAll();
            mls.LogInfo($"{modName} has been loaded");
        }
    }
}
