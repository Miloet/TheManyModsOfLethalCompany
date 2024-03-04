using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using LethalLib.Modules;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System;

namespace LC_Sandwich
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class SandwichMod : BaseUnityPlugin
    {

        private const string modGUID = "Mellowdy.YummySandwich";
        private const string modName = "YummySandwich";
        private const string modVersion = "1.0.0";


        private readonly Harmony harmony = new Harmony(modGUID);

        public static ManualLogSource mls;

        private static SandwichMod instance;
        public static AssetBundle assets;

        public static string assetName = "sandwich.asset";
        public static string itemName = "Sandwich.asset";
        public static string prefabName = "Sandwich.prefab";

        public static int rarity = 30;

        void Awake()
        {
            if (instance == null) instance = this;
            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);

            //Load Asset Bundle
            string currentDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string path = Path.Combine(currentDirectory, assetName).Replace("\\", "/");
            assets = AssetBundle.LoadFromFile(path);

            //Item

            GameObject itemPrefab = assets.LoadAsset<GameObject>(prefabName);

            Sandwich comp = itemPrefab.AddComponent<Sandwich>();
            Item sandwichItem = assets.LoadAsset<Item>(itemName);

            Sandwich.eatingSFX = assets.LoadAsset<AudioClip>("Eat.wav");
            Sandwich.finishSFX = assets.LoadAsset<AudioClip>("FinishEating.wav");

            Sandwich.defaultOffset = sandwichItem.positionOffset;
            Sandwich.defaultRotation = sandwichItem.rotationOffset;

            Sandwich.originalProperties = sandwichItem;
            Sandwich.eatingProperties = Instantiate(sandwichItem);

            Sandwich.sandwichSize = Config.Bind<int>("Properties", "Size", 4, "How many times can the sandwich be eaten").Value;
            Sandwich.healing = Config.Bind<int>("Properties", "Healing", 35, "How many hitpoints should be restored when one part is consumed").Value;
            float chance = Config.Bind<float>("Properties", "Heal Critical-injury Chance", -1f, "The chance to heal the critical-injury state with each bite (between 0 - 1)(set to -1 if you want the chance to be based off sandwich size)").Value;
            Sandwich.healCrippleChance = chance == -1 ? 1f / Sandwich.sandwichSize : chance;
            Sandwich.timeToEat = Config.Bind<int>("Properties", "Time", 2, "How long in seconds does it take to eat one part of the sandwich").Value;




            comp.itemProperties = sandwichItem;
            sandwichItem.spawnPrefab = itemPrefab;


            rarity = Config.Bind<int>("Rarity", "Base rarity", 30, "Rarity of the sandwich between 1 - 100 (for context, on march: bottles/large axel/engine: 80 - 100. gold bar/robot/lazer pointer: 1 - 6)").Value;

            NetworkPrefabs.RegisterNetworkPrefab(sandwichItem.spawnPrefab);

            Items.RegisterScrap(sandwichItem, (int)(rarity * 
                Config.Bind<float>("Rarity", "Experimentation", 0.3f, "Rarity multiplier for Experimentation").Value),
                 Levels.LevelTypes.ExperimentationLevel);
            Items.RegisterScrap(sandwichItem, (int)(rarity * 
                Config.Bind<float>("Rarity", "Assurance", 0f, "Rarity multiplier for Assurance").Value),
                Levels.LevelTypes.AssuranceLevel);
            Items.RegisterScrap(sandwichItem, (int)(rarity * 
                Config.Bind<float>("Rarity", "Vow", 1.5f, "Rarity multiplier for Vow").Value),
                Levels.LevelTypes.VowLevel);

            Items.RegisterScrap(sandwichItem, (int)(rarity * 
                Config.Bind<float>("Rarity", "Offense", 1f, "Rarity multiplier for Offense").Value),
                    Levels.LevelTypes.OffenseLevel);
            Items.RegisterScrap(sandwichItem, (int)(rarity * 
                Config.Bind<float>("Rarity", "March", 2f, "Rarity multiplier for March").Value),
                            Levels.LevelTypes.MarchLevel);

            Items.RegisterScrap(sandwichItem, (int)(rarity * 
                Config.Bind<float>("Rarity", "Rend", 1f, "Rarity multiplier for Rend").Value),
                            Levels.LevelTypes.RendLevel);
            Items.RegisterScrap(sandwichItem, (int)(rarity * 
                Config.Bind<float>("Rarity", "Dine", 1f, "Rarity multiplier for Dine").Value),
                            Levels.LevelTypes.DineLevel);
            Items.RegisterScrap(sandwichItem, (int)(rarity * 
                Config.Bind<float>("Rarity", "Titan", .5f, "Rarity multiplier for Titan").Value),
                            Levels.LevelTypes.TitanLevel);



            harmony.PatchAll();
            mls.LogInfo($"{modName} has been loaded");
        }
    }
}
