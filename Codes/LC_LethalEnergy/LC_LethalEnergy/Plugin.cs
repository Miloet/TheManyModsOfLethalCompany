using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using LethalLib.Modules;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace LC_LethalEnergy
{

    [BepInPlugin(modGUID, modName, modVersion)]
    public class LethalEnergyMod : BaseUnityPlugin
    {
        private const string modGUID = "Mellowdy.LethalEnergy";
        private const string modName = "LethalEnergy";
        private const string modVersion = "0.0.1";

        private readonly Harmony harmony = new Harmony(modGUID);

        public static ManualLogSource mls;

        private static LethalEnergyMod instance;
        public static AssetBundle assets;

        public static string assetName = "monster.energy";
        public static string canPrefabName = "monstercan.prefab";
        public static string casePrefabName = ".prefab";

        void Awake()
        {
            if (instance == null) instance = this;
            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);

            //Load Asset Bundle
            string currentDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string path = Path.Combine(currentDirectory, assetName).Replace("\\", "/");
            assets = AssetBundle.LoadFromFile(path);

            //Plushie Scrap Item
            //GameObject Case = assets.LoadAsset<GameObject>(casePrefabName);

            GameObject Can = assets.LoadAsset<GameObject>(canPrefabName);
            mls.LogMessage(Can != null ? Can.name : "CAN IS NULL. FUCK YOU");

            //DEBUG! REMOVE LATER!!!!!
            Can.layer = LayerMask.NameToLayer("Props");
            Can.tag = "PhysicsProp";

            Can.AddComponent<AudioSource>();


            Item canItem = assets.LoadAsset<Item>("can.asset");
            canItem.spawnPrefab = Can;
            canItem.itemName = "Can";

            LethalCan lethalCan = Can.AddComponent<LethalCan>();
            lethalCan.itemProperties = canItem;
            lethalCan.grabbable = true;

            NetworkPrefabs.RegisterNetworkPrefab(canItem.spawnPrefab);
            Items.RegisterScrap(canItem);


            harmony.PatchAll();
            harmony.PatchAll(typeof(PlayerPatches));
            mls.LogInfo($"{modName} has been loaded");
        }
    }
}
