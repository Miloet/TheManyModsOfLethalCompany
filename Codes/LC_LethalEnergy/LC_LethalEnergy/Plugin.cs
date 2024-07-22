using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using LethalLib.Modules;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using RuntimeNetcodeRPCValidator;
using GameNetcodeStuff;
using Unity.Netcode;

namespace LC_LethalEnergy
{

    [BepInPlugin(modGUID, modName, modVersion)]
    [BepInDependency(MyPluginInfo.PLUGIN_GUID,MyPluginInfo.PLUGIN_VERSION)]
    public class LethalEnergyMod : BaseUnityPlugin
    {
        private const string modGUID = "Mellowdy.LethalEnergy";
        private const string modName = "LethalEnergy";
        private const string modVersion = "1.0.2";

        private readonly Harmony harmony = new Harmony(modGUID);
        private readonly NetcodeValidator validator = new NetcodeValidator(modGUID);

        public static ManualLogSource mls;

        private static LethalEnergyMod instance;
        public static AssetBundle assets;

        public static string assetName = "monster.energy";
        public static string canPrefabName = "monstercan.prefab";
        public static string casePrefabName = "monstercase.prefab";
        public static string drinkSoundName = "drinking.wav";


        public static int rarity = 30;


        void Awake()
        {
            if (instance == null) instance = this;
            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);

            #region Load Assets

            //Load Asset Bundle
            string currentDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string path = Path.Combine(currentDirectory, assetName).Replace("\\", "/");
            assets = AssetBundle.LoadFromFile(path);
            

            //Case

            GameObject Case = assets.LoadAsset<GameObject>(casePrefabName);
            Item caseItem = assets.LoadAsset<Item>("case.asset");
            caseItem.spawnPrefab = Case;

            DrinkCase drinkCase = Case.AddComponent<DrinkCase>();
            drinkCase.itemProperties = caseItem;
            drinkCase.grabbable = true;
            drinkCase.useCooldown = 0.5f;

            drinkCase.canHandler = Case.AddComponent<CanHandler>();
            drinkCase.canHandler.drinkCase = drinkCase;

            //Can

            Item canItem = assets.LoadAsset<Item>("Can.asset");
            GameObject Can = GetCan(assets.LoadAsset<GameObject>(canPrefabName), canItem);
            canItem.spawnPrefab = Can;
            LethalCan.drinkingSFX = assets.LoadAsset<AudioClip>(drinkSoundName);
            LethalCan.drinkingProp = assets.LoadAsset<Item>("Drinking.asset");
            DrinkCase.canPrefab = Can;
            

            //Store Can
            Item buyableCan = assets.LoadAsset<Item>("BuyCan.asset");
            buyableCan.spawnPrefab = GetCan(assets.LoadAsset<GameObject>("boughtcan.prefab"), buyableCan);
            LethalCan.storeProp = buyableCan;

            LethalCan bc = buyableCan.spawnPrefab.GetComponent<LethalCan>();
            bc.store = true;
            
            int itemPrice = Config.Bind<int>("Shop", "Can Price", 10, "Price for a can of monster in the shop").Value;

            #endregion

            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(caseItem.spawnPrefab);
            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(canItem.spawnPrefab);
            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(buyableCan.spawnPrefab);

            Items.RegisterItem(buyableCan);
            Items.RegisterShopItem(buyableCan, itemPrice);

            #region Settings


            Levels.LevelTypes[] AllLevels =
            {
                Levels.LevelTypes.ExperimentationLevel,
                Levels.LevelTypes.AssuranceLevel,
                Levels.LevelTypes.VowLevel,

                Levels.LevelTypes.MarchLevel,
                Levels.LevelTypes.OffenseLevel,

                Levels.LevelTypes.TitanLevel,
                Levels.LevelTypes.DineLevel,
                Levels.LevelTypes.RendLevel
            };

            int[] AllLevelsRarity =
            {
                0,
                10,
                8,

                10,
                5,

                2,
                15,
                20,
            };

            rarity = Config.Bind<int>("Rarity", "Base Rarity", 20, "Rarity for all moons not otherwise listed").Value;

            for (int i = 0; i < AllLevels.Length; i++)
            {
                string levelName = AllLevels[i].ToString().Replace("Level", "");
                AllLevelsRarity[i] = Config.Bind<int>("Rarity", $"{levelName}", AllLevelsRarity[i], $"Rarity for {levelName}").Value;

                Items.RegisterScrap(caseItem, AllLevelsRarity[i], AllLevels[i]);
            }


            #endregion
            #region Register Objects

            Items.RegisterScrap(canItem,0);

            Items.RegisterScrap(caseItem, rarity, Levels.LevelTypes.Modded);

            #endregion

            harmony.PatchAll();
            harmony.PatchAll(typeof(PlayerPatches));

            validator.PatchAll();
            
            mls.LogInfo($"{modName} has been loaded");
        }

        public static GameObject GetCan(GameObject g, Item itemProp)
        {
            LethalCan lethalCan = g.AddComponent<LethalCan>();
            lethalCan.itemProperties = itemProp;
            lethalCan.grabbable = true;
            
            lethalCan.fillHandler = g.AddComponent<FillHandler>();
            lethalCan.fillHandler.lethalCan = lethalCan;

            return g;
        }
    }



    
}
