using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LC_HoardingBugSnacks.Patches
{
    [HarmonyPatch(typeof(RoundManager))]
    internal class HoarderBugAIPatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        public void SpawnWithShotgun()
        {
            float chance = 0.01f;
            if(chance < Random.Range(0f,1f))
            {
                //Give the bug a shotgun
            }
        }

    }
}
