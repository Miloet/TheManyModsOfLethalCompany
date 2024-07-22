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

public class Patch
{
    [HarmonyPatch(typeof(RoundManager), "LoadNewLevelWait")]
    [HarmonyPostfix]
    public static void PatchExample(RoundManager __instance)
    {

    }
}

