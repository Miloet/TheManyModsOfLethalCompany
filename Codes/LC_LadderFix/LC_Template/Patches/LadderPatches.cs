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
public static class LadderPatches
{
    public static LayerMask layer = LayerMask.GetMask("Default", "Room", "Colliders");
    public const float distance = 0.4f;
    public const float distance2 = 0.6f;


    [HarmonyPatch(typeof(ExtensionLadderItem), "DiscardItem")]
    [HarmonyPostfix]
    public static void LadderPatch(ExtensionLadderItem __instance)
    {
        Vector3 forward = StartOfRound.Instance.localPlayerController.gameObject.transform.forward;
        Vector3 vector = (-forward + Vector3.up);

        Transform transform = __instance.transform;

        Ray ray = new Ray(transform.position + vector * distance, -vector);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, distance2, layer))
        {
            __instance.targetFloorPosition = hit.point + __instance.itemProperties.verticalOffset * Vector3.up;
            if (__instance.transform.parent != null)
            {
                __instance.targetFloorPosition = __instance.transform.parent.InverseTransformPoint(__instance.targetFloorPosition);
            }
        }
    }
}

