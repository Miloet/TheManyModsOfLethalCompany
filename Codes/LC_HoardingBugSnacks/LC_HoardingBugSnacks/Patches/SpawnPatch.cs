using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;

namespace LC_HoardingBugSnacks.Patches
{
    internal class SpawnPatch
	{
		[HarmonyPatch(typeof(StartOfRound), "StartGame")]
		[HarmonyPostfix]
		public static void SpawnExtraHoardingBugs()
		{
			int bugId = 2;
			if (RoundManager.Instance.NetworkManager.IsHost || RoundManager.Instance.NetworkManager.IsServer)
			{
				for (int i = 0; i < 1 + Random.Range(0, 10); i++)
				{
					RoundManager.Instance.SpawnEnemyServerRpc(
							new Vector3(
								-4.3f + (float)(Random.Range(-500, 500) / 100),
								-219.5f,
								66f + (float)(Random.Range(-500, 500) / 100)),
							0f,
							bugId);
				}

			}
		}
	

		[HarmonyPatch(typeof(HoarderBugAI), "Start")]
		[HarmonyPostfix]
		public static void SpawnWithShotgun(Transform ___animationContainer)
		{
			HoarderBugAI bug = ___animationContainer.parent.GetComponentInParent<HoarderBugAI>();
			
			if (bug == null) return;
            float chance = 1f;
			if (chance > Random.Range(0f, 1f))
			{
				var g = GameObject.Instantiate(HoardingBugSnacksMod.shotgunItem.spawnPrefab);
                NetworkObject component = g.GetComponent<NetworkObject>();
                bug.SwitchToBehaviourStateOnLocalClient(1);
                bug.GrabItemServerRpc(component);
            }
		}
	}
}
