using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using GameNetcodeStuff;
using HarmonyLib;
using Unity.Netcode;
using System.Collections;
using RuntimeNetcodeRPCValidator;
using BepInEx.Logging;
using System;

namespace LC_LethalEnergy
{
    public class DrinkCase : GrabbableObject 
	{
        public static GameObject canPrefab;

        public int cansHeld;

		public Transform[] cans;

		public ManualLogSource mls;

		int minimumCans = 5;
		int value = 0;
		public bool loaded = false;

		public CanHandler canHandler;
		public override void Start()
        { 
			base.Start();
			mls = LethalEnergyMod.mls;

			GameObject g = transform.Find("DrinkContainer").gameObject;
			

			cans = new Transform[g.transform.childCount];
			for (int i = 0; i < g.transform.childCount; i++)
			{
				// Add each child's GameObject to the list
				cans[i] = g.transform.GetChild(i);
			}
			StartCoroutine(SetValue());

			if (base.IsServer)
			{
				mls.LogMessage("SetCans was called");
				StartCoroutine(SendOutData());
			}
			else StartCoroutine(RequestData());
		}

		public IEnumerator SendOutData()
        {
			int cans = Mathf.Clamp(UnityEngine.Random.Range(1, 7) + UnityEngine.Random.Range(1, 7), minimumCans, 12);

			canHandler.SendCansClientRpc(cans);
			yield return new WaitForSeconds(2f);
			canHandler.SendCansServerRpc(cans);
		}

		public IEnumerator RequestData()
        {
			yield return new WaitForSeconds(8f);
			if(!loaded) canHandler.RequestCanDataServerRpc();
        }

		public IEnumerator SetValue()
        {
			yield return new WaitForSeconds(3f);

			value = UnityEngine.Random.Range(itemProperties.minValue, itemProperties.maxValue+1);

			UpdateScrapValue();
		}

		public void UpdateScrapValue()
        {
			SetScrapValue(value + cansHeld * 10);
		}

		public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
			mls.LogMessage("CansHeld: " + cansHeld);
            if (buttonDown && cansHeld > 0)
            {
                cansHeld--;
                UpdateCans();
				canHandler.SendCansServerRpc(cansHeld);
				canHandler.SpawnCanServerRpc();
            }
        }

		public void UpdateCans()
        {
			if (value != 0)
				UpdateScrapValue();

			cansHeld = Mathf.Clamp(cansHeld, 0, cans.Length);
			for(int i = 0; i < cans.Length; i++)
			{
				cans[i].gameObject.SetActive(i < cansHeld);
			}
		}

		public IEnumerator waitForCanToSpawnOnClient(NetworkObjectReference netObjectRef, Vector3 vector)
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


	public class CanHandler : NetworkBehaviour
    {
		public DrinkCase drinkCase;

		[ServerRpc(RequireOwnership = false)]
		public void SendCansServerRpc(int cans)
		{
			SendCansClientRpc(cans);
		}
		[ClientRpc]
		public void SendCansClientRpc(int cans)
		{
			drinkCase.mls.LogMessage("SetCans was called AND went through with " + cans + " cans");
			drinkCase.cansHeld = cans;
			drinkCase.UpdateCans();

			drinkCase.loaded = true;
		}


		[ServerRpc(RequireOwnership = false)]
		public void SpawnCanServerRpc()
		{
			#region Networking Stuff

			Transform parent = (((!(drinkCase.playerHeldBy != null) || !drinkCase.playerHeldBy.isInElevator) && !StartOfRound.Instance.inShipPhase) || !(RoundManager.Instance.spawnedScrapContainer != null)) ? StartOfRound.Instance.elevatorTransform : RoundManager.Instance.spawnedScrapContainer;
			var vector = base.transform.position + Vector3.up * 0.25f;
			var gameObject = UnityEngine.Object.Instantiate(DrinkCase.canPrefab, vector, Quaternion.identity, parent);
			GrabbableObject component = gameObject.GetComponent<GrabbableObject>();
			component.targetFloorPosition = component.GetItemFloorPosition(base.transform.position);
			if (drinkCase.playerHeldBy != null && drinkCase.playerHeldBy.isInHangarShipRoom)
			{
				drinkCase.playerHeldBy.SetItemInElevator(droppedInShipRoom: true, droppedInElevator: true, component);
			}
			component.NetworkObject.Spawn();

			#endregion

			SpawnCanClientRpc(gameObject.GetComponent<NetworkObject>(), vector.x, vector.y, vector.z, drinkCase.cansHeld);
		}

		[ClientRpc]
		public void SpawnCanClientRpc(NetworkObjectReference netObjectRef, float x, float y, float z, int cans)
		{
			drinkCase.cansHeld = cans;
			drinkCase.UpdateCans();
			StartCoroutine(drinkCase.waitForCanToSpawnOnClient(netObjectRef, new Vector3(x,y,z)));
		}



		[ServerRpc(RequireOwnership = false)]
		public void RequestCanDataServerRpc()
        {
			RequestCanDataClientRpc();
		}

		[ClientRpc]
		public void RequestCanDataClientRpc()
		{
			if (IsServer) SendCansServerRpc(drinkCase.cansHeld);
		}
	}
}
