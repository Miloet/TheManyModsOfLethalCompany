using HarmonyLib;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;

namespace LC_HoardingBugSnacks.Patches
{
    internal class SpawnPatch
	{
		[HarmonyPatch(typeof(RoundManager), "Awake")]
		[HarmonyPostfix]
		public static void FindShotgun()
		{
			List<Item> AllItems = StartOfRound.Instance.allItemsList.itemsList;
			HoardingBugSnacksMod.shotgunItem = AllItems.FirstOrDefault(i => i.name.Equals("Shotgun"));
		}


		[HarmonyPatch(typeof(RoundManager), "LoadNewLevelWait")]
		[HarmonyPostfix]
		public static void SpawnExtraHoardingBugs(RoundManager __instance)
		{
			int bugId = 2;
			if (__instance.NetworkManager.IsHost || __instance.NetworkManager.IsServer)
			{
				__instance.StartCoroutine(FinishedLoading(__instance));
			}
		}

		public static IEnumerator FinishedLoading(RoundManager instance)
        {
			yield return new WaitUntil(() => instance.dungeonCompletedGenerating);
			yield return null;
			yield return new WaitUntil(() => instance.playersFinishedGeneratingFloor.Count >= GameNetworkManager.Instance.connectedPlayers);
			yield return new WaitForSeconds(5f);
			for (int i = 0; i < 10; i++)
			{
				instance.SpawnEnemyOnServer(
						new Vector3(
							-4.3f + Random.Range(-5f, 5f),
							-219.5f,
							66f + Random.Range(-5f, 5)),
						0f,
						2); //,HoardingBugSnacksMod.hoarderType);
			}
		}

		[HarmonyPatch(typeof(StartOfRound), "Awake")]
		[HarmonyPostfix]
		private static void SetHoarderType(StartOfRound __instance)
		{
			SelectableLevel[] levels = __instance.levels;
			foreach (SelectableLevel val in levels)
			{
				foreach (SpawnableEnemyWithRarity enemy in val.Enemies)
				{
					if (!(enemy.enemyType.name == "HoarderBug"))
					{
						continue;
					}
					if (HoardingBugSnacksMod.hoarderType == null)
					{
						enemy.enemyType.isOutsideEnemy = false;
						HoardingBugSnacksMod.hoarderType = enemy.enemyType;
					}
					break;
				}
			}
		}

		[HarmonyPatch(typeof(HoarderBugAI), "Start")]
		[HarmonyPostfix]
		public static void SpawnWithShotgun(HoarderBugAI __instance)
		{
			float chance = 1f;
			if (chance > Random.Range(0f, 1f))
			{
				var g = GameObject.Instantiate(HoardingBugSnacksMod.shotgunItem.spawnPrefab, __instance.transform.position, Quaternion.identity);
				NetworkObject component = g.GetComponent<NetworkObject>();
				var shotgun = g.GetComponent<ShotgunItem>();
				shotgun.shellsLoaded = 2;
				shotgun.safetyOn = false;
				component.Spawn();
				__instance.SwitchToBehaviourStateOnLocalClient(1);
				__instance.GrabItemServerRpc(component);
			}
		}

		[HarmonyPatch(typeof(HoarderBugAI), "DropItem")]
		[HarmonyPrefix]
		public static void DontDropIfShotgun(HoarderBugAI __instance)
		{
			if (__instance.heldItem.itemGrabbableObject is ShotgunItem) return;
		}

	}
}
