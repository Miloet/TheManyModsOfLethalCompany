using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using LethalLib.Modules;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using GameNetcodeStuff;
using Unity.Netcode;

[BepInPlugin(modGUID, modName, modVersion)]
//[BepInDependency(MyPluginInfo.PLUGIN_GUID,MyPluginInfo.PLUGIN_VERSION)]
public class LC_TemplateMod : BaseUnityPlugin
{
    private const string modGUID = "Mellowdy." + modName;
    private const string modName = "Template";
    private const string modVersion = "0.0.0";

    private readonly Harmony harmony = new Harmony(modGUID);

    public static ManualLogSource mls;

    private static LC_TemplateMod instance;
    public static AssetBundle assets;
    public const string assetName = "asset.asset";

    void Awake()
    {
        if (instance == null) instance = this;
        mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);

        // Load Assets

        string currentDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        string path = Path.Combine(currentDirectory, assetName).Replace("\\", "/");
        assets = AssetBundle.LoadFromFile(path);


        //Register Items

        //LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab();
        //Items.RegisterItem();
        //Items.RegisterShopItem(, );


        //Apply patches

        harmony.PatchAll();
        harmony.PatchAll(typeof(Patch));

        mls.LogInfo($"{modName} has been loaded");
    }

}
