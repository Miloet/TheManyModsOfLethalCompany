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
public class LC_LadderFix : BaseUnityPlugin
{
    private const string modGUID = "Mellowdy." + modName;
    private const string modName = "LadderFix";
    private const string modVersion = "0.0.1";

    private readonly Harmony harmony = new Harmony(modGUID);

    public static ManualLogSource mls;
    private static LC_LadderFix instance;


    void Awake()
    {
        if (instance == null) instance = this;
        mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);


        //Apply patches

        harmony.PatchAll();
        harmony.PatchAll(typeof(LadderPatches));

        mls.LogInfo($"{modName} has been loaded");
    }

}
