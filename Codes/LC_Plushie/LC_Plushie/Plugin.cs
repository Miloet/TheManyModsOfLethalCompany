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
public class BugPlushieMod : BaseUnityPlugin
{
    private const string modGUID = "Mellowdy.BugPlushie";
    private const string modName = "BugPlushie";
    private const string modVersion = "0.0.1";


    private readonly Harmony harmony = new Harmony(modGUID);

    public static ManualLogSource mls;

    private static BugPlushieMod instance;
    public static AssetBundle assets;

    public static string assetName = "plushie.asset";
    public static string prefabName = "Plushie.prefab";
    public static string meshName = "Plushie.mesh";
        
    public static string PlushieName = "Plushie pajama man";

    public static GameObject PlushiePrefab;

    public static float PlushieSize = 200;


    public static int rarity = 10;

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
        plushieItem.spawnPrefab.transform.localScale = Vector3.one * 25f; 
        NetworkPrefabs.RegisterNetworkPrefab(plushieItem.spawnPrefab);
        Items.RegisterScrap(plushieItem, rarity, Levels.LevelTypes.All);
        
        
        harmony.PatchAll();
        mls.LogInfo("The bugs have infested the ship. They are in the walls");
    }
}