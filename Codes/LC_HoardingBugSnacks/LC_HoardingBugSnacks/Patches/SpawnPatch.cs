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
			float x = Random.Range(-25f,25f);
			float z = Random.Range(-25f, 25f);

			Vector3 spawnPos = new Vector3(
							-4.3f + Random.Range(-5f, 5f) + x,
							-219.5f,
							66f + Random.Range(-5f, 5) + z);
			Vector3 realSpawnPos = ChooseClosestNodeToPosition(spawnPos).position;

			int numToSpawn = HoardingBugSnacksMod.bugsToSpawn.Value + Random.Range(0, HoardingBugSnacksMod.randomBugsToSpawn.Value);
            for (int i = 0; i < numToSpawn; i++)
			{
				instance.SpawnEnemyOnServer(realSpawnPos,
						0f,
						2); //,HoardingBugSnacksMod.hoarderType);
			}
		}

        public static Transform ChooseClosestNodeToPosition(Vector3 pos)
        {
            GameObject[] allAINodes = GameObject.FindGameObjectsWithTag("AINode");
            var nodesTempArray = allAINodes.OrderBy((GameObject x) => Vector3.Distance(pos, x.transform.position)).ToArray();
            Transform result = nodesTempArray[0].transform;
            return result;
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
			float chance = (float)HoardingBugSnacksMod.shotgunChance.Value / 100f;
			if (chance > Random.Range(0f, 1f))
			{
				var g = GameObject.Instantiate(HoardingBugSnacksMod.shotgunItem.spawnPrefab, __instance.transform.position, Quaternion.identity);
				NetworkObject component = g.GetComponent<NetworkObject>();
				var shotgun = g.GetComponent<ShotgunItem>();
				var item = shotgun.itemProperties;
                shotgun.SetScrapValue(Random.Range(item.minValue, item.maxValue)/4);
				shotgun.shellsLoaded = 2;
				shotgun.safetyOn = false;
				component.Spawn();
				__instance.SwitchToBehaviourStateOnLocalClient(1);
				__instance.GrabItemServerRpc(component);
			}
		}
		public static HoarderBugItem tempShotgun;

		[HarmonyPatch(typeof(HoarderBugAI), "DropItem")]
		[HarmonyPrefix]
		public static void DontDropIfShotgun(HoarderBugAI __instance)
		{
            if (__instance.heldItem != null && (__instance.heldItem.itemGrabbableObject is ShotgunItem || __instance.heldItem.itemGrabbableObject is BugSnacks))
			{
				tempShotgun = __instance.heldItem;
			}
		}

        [HarmonyPatch(typeof(HoarderBugAI), "DropItem")]
        [HarmonyPostfix]
        public static void PickBackUpIfShotgun(HoarderBugAI __instance)
        {
			if (__instance.heldItem == null && tempShotgun != null)
			{
				__instance.GrabItemServerRpc(tempShotgun.itemGrabbableObject.NetworkObject);
				tempShotgun = null;
			}
        }


        [HarmonyPatch(typeof(HoarderBugAI), "IsHoarderBugAngry")]
        [HarmonyPrefix]
		public static bool MakeNotAngryWhenSnacks(HoarderBugAI __instance)
		{
			if(BugSnacks.BugHappiness > 0)
			{
                BugSnacks.BugHappiness -= Time.deltaTime;
                return false;
            }
			else
			{
                if (__instance.stunNormalizedTimer > 0f)
                {
                    __instance.angryTimer = 4f;
                    if (__instance.stunnedByPlayer)
                    {
                        __instance.angryAtPlayer = __instance.stunnedByPlayer;
                    }
                    return true;
                }
                int num = 0;
                int num2 = 0;
                for (int i = 0; i < HoarderBugAI.HoarderBugItems.Count; i++)
                {
                    if (HoarderBugAI.HoarderBugItems[i].status == HoarderBugItemStatus.Stolen)
                    {
                        num2++;
                    }
                    else if (HoarderBugAI.HoarderBugItems[i].status == HoarderBugItemStatus.Returned)
                    {
                        num++;
                    }
                }
                if (!(__instance.angryTimer > 0f))
                {
                    return num2 > 0;
                }
                return true;
            }

		}



    }
}
