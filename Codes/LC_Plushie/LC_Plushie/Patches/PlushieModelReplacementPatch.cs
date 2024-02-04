using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace LC_Plushie.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal class PlushieModelReplacement
    {
        [HarmonyPatch("Awake")]
        [HarmonyPrefix]
        public static void ReplaceModel(UnlockablesList ___unlockablesList)
        {
            
            var objs = ___unlockablesList.unlockables;
            foreach (UnlockableItem obj in objs)
            {
                if (obj.unlockableName == BugPlushieMod.PlushieName)
                {
                    BugPlushieMod.PlushiePrefab = obj.prefabObject;
                    break;
                }
            }
            if (BugPlushieMod.PlushiePrefab != null)
            {
                Mesh plushieMesh = Mesh.Instantiate(BugPlushieMod.assets.LoadAsset<Mesh>(BugPlushieMod.meshName));
                MeshFilter mesh = BugPlushieMod.PlushiePrefab.GetComponentInChildren<MeshFilter>();

                Vector3[] vertices = plushieMesh.vertices;
                Vector3 center = plushieMesh.bounds.center;
                for (int i = 0; i < vertices.Length; i++)
                {
                    vertices[i] = center + (vertices[i] - center) * BugPlushieMod.PlushieSize - new Vector3(0,0,0.25f);
                }

                plushieMesh.vertices = vertices;
                plushieMesh.RecalculateBounds();

                mesh.mesh = plushieMesh;
                float size = BugPlushieMod.PlushieSize;
                

                
                BugPlushieMod.mls.LogMessage("Plush man loaded and replaced");
            }
            else BugPlushieMod.mls.LogMessage("Plush man could not be found");
        }
    }
}
