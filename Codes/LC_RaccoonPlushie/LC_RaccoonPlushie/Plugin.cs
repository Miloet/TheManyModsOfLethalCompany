using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using LethalLib.Modules;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;

[BepInPlugin(modGUID, modName, modVersion)]
public class RaccoonPlushieMod : BaseUnityPlugin
{
    private const string modGUID = "Mellowdy.RaccoonPlushie";
    private const string modName = "RaccoonPlushie";
    private const string modVersion = "0.0.1";


    private readonly Harmony harmony = new Harmony(modGUID);

    public static ManualLogSource mls;

    private static RaccoonPlushieMod instance;
    public static AssetBundle assets;

    public static string assetName = "raccoon.plush";
    public static string itemName = "RaccoonPlush.asset";

    public static GameObject PlushiePrefab;

    public static int rarity = 30;
    //public static ConfigEntry<int> a;

    void Awake()
    {
        if (instance == null) instance = this;
        mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);

        harmony.PatchAll();

        //Load Asset Bundle
        string currentDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        string path = Path.Combine(currentDirectory, assetName).Replace("\\", "/");
        assets = AssetBundle.LoadFromFile(path);

        //Plushie Scrap Item
        Item plushieItem = assets.LoadAsset<Item>(itemName);

        plushieItem.weight = Config.Bind<float>("Raccoon Properties", "Weight", plushieItem.weight, "5 lb = 1.05, 100 lb = 2").Value;
        plushieItem.minValue = Config.Bind<int>("Raccoon Properties", "Min Value", plushieItem.minValue, "").Value;
        plushieItem.maxValue = Config.Bind<int>("Raccoon Properties", "Max Value", plushieItem.maxValue, "").Value;
        
        



        var rac = plushieItem.spawnPrefab.AddComponent<Raccoon>();
        rac.grabbable = true;
        rac.grabbableToEnemies = true;
        rac.useCooldown = 0.25f;
        rac.sounds = new AudioClip[] {
            assets.LoadAsset<AudioClip>("Squeak1.mp3"), 
            assets.LoadAsset<AudioClip>("Squeak2.mp3")};

        rac.itemProperties = plushieItem;

        NetworkPrefabs.RegisterNetworkPrefab(plushieItem.spawnPrefab);


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

        float[] AllLevelsRarity =
        {
            0.2f,
            0f,
            0f,

            0f,
            0.1f,

            1f,
            2f,
            2f,
        };

        rarity = Config.Bind<int>("Rarity", "Base Rarity", 10, "Base rarity for the plushie between 1 - 100(for context, on march: bottles / large axel / engine: 80 - 100.gold bar / robot / lazer pointer: 1 - 6)").Value;

        for (int i = 0; i < AllLevels.Length; i++)
        {
            string levelName = AllLevels[i].ToString().Replace("Level", ""); ;
            AllLevelsRarity[i] = Config.Bind<float>("Rarity", $"{levelName}", AllLevelsRarity[i], $"Rarity multiplier for {levelName}").Value;

            Items.RegisterScrap(plushieItem, Mathf.FloorToInt(rarity * AllLevelsRarity[i]), AllLevels[i]);
        }
        Items.RegisterScrap(plushieItem, rarity, Levels.LevelTypes.Modded);

        mls.LogInfo($"{modName} has been loaded");
    }
}

