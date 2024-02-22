using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using LethalLib.Modules;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System;

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

    public static string assetName = "plushie.asset";
    public static string prefabName = "Plushie.prefab";
    public static string meshName = "Plushie.mesh";

    public static GameObject PlushiePrefab;

    public static int rarity = 10;
    //public static ConfigEntry<int> a;

    void Awake()
    {
        if (instance == null) instance = this;
        mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);

        //Load Asset Bundle
        string currentDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        string path = Path.Combine(currentDirectory, assetName).Replace("\\", "/");
        assets = AssetBundle.LoadFromFile(path);

        //Plushie Scrap Item
        Item plushieItem = assets.LoadAsset<GameObject>(prefabName).GetComponent<GrabbableObject>().itemProperties;

        plushieItem.weight = Config.Bind<float>("Raccoon Properties", "Weight", plushieItem.weight, "5 lb = 1.05, 100 lb = 2").Value;
        plushieItem.minValue = Config.Bind<int>("Raccoon Properties", "Min Value", plushieItem.minValue, "").Value;
        plushieItem.maxValue = Config.Bind<int>("Raccoon Properties", "Max Value", plushieItem.maxValue, "").Value;



        NetworkPrefabs.RegisterNetworkPrefab(plushieItem.spawnPrefab);
        Items.RegisterScrap(plushieItem, rarity, Levels.LevelTypes.All);


        harmony.PatchAll();
        mls.LogInfo($"{modName} has been loaded");
    }
}