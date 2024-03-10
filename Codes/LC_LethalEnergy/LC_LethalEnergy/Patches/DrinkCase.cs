using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using GameNetcodeStuff;
using HarmonyLib;
using Unity.Netcode;
using System.Collections;

namespace LC_LethalEnergy
{
    public class DrinkCase : GrabbableObject
    {
        public static GameObject canPrefab;

        public int cansHeld;



        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);

            if (buttonDown)
            {
				SpawnCanServerRpc();
            }
        }


		public void UpdateCans()
        {

        }



		[ServerRpc(RequireOwnership = false)]
		public void SpawnCanServerRpc()
        {
			cansHeld--;
			UpdateCans();

			#region Networking Stuff

			Transform parent = ((((!(playerHeldBy != null) || !playerHeldBy.isInElevator) && !StartOfRound.Instance.inShipPhase) || !(RoundManager.Instance.spawnedScrapContainer != null)) ? StartOfRound.Instance.elevatorTransform : RoundManager.Instance.spawnedScrapContainer);
			var vector = base.transform.position + Vector3.up * 0.25f;
			var gameObject = UnityEngine.Object.Instantiate(canPrefab, vector, Quaternion.identity, parent);
			GrabbableObject component = gameObject.GetComponent<GrabbableObject>();
			component.targetFloorPosition = component.GetItemFloorPosition(base.transform.position);
			if (playerHeldBy != null && playerHeldBy.isInHangarShipRoom)
			{
				playerHeldBy.SetItemInElevator(droppedInShipRoom: true, droppedInElevator: true, component);
			}
			component.NetworkObject.Spawn();

			#endregion

            SpawnCanClientRpc(gameObject.GetComponent<NetworkObject>(), vector, cansHeld);
		}

		[ClientRpc]
		public void SpawnCanClientRpc(NetworkObjectReference netObjectRef, Vector3 vector, int cans)
        {
			if (!base.IsServer)
			{
				cansHeld = cans;
				UpdateCans();
				StartCoroutine(waitForCanToSpawnOnClient(netObjectRef, vector));
			}
		}


		private IEnumerator waitForCanToSpawnOnClient(NetworkObjectReference netObjectRef, Vector3 vector)
		{
            #region Networking Stuff

            NetworkObject netObject = null;
			float startTime = Time.realtimeSinceStartup;
			while (Time.realtimeSinceStartup - startTime < 8f && !netObjectRef.TryGet(out netObject))
			{
				yield return new WaitForSeconds(0.03f);
			}
			if (netObject == null)
			{
				Debug.Log("No network object found");
				yield break;
			}
			yield return new WaitForEndOfFrame();
			GrabbableObject component = netObject.GetComponent<GrabbableObject>();
			RoundManager.Instance.totalScrapValueInLevel -= scrapValue;
			RoundManager.Instance.totalScrapValueInLevel += component.scrapValue;
			component.startFallingPosition = vector;
			component.fallTime = 0f;
			component.hasHitGround = false;
			component.reachedFloorTarget = false;
			if (playerHeldBy != null && playerHeldBy.isInHangarShipRoom)
			{
				playerHeldBy.SetItemInElevator(droppedInShipRoom: true, droppedInElevator: true, component);
			}

            #endregion 
        }
    }

}
