using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace LC_StockMarketIndex.Patches
{
    [HarmonyPatch]
    public class NetworkObjectManager
    {
        [HarmonyPostfix, HarmonyPatch(typeof(GameNetworkManager), nameof(GameNetworkManager.StartHost))]
        public static void Init()
        {
            if (networkPrefab != null)
                return;

            networkPrefab = StockMarketIndexMod.networkObject;
            networkPrefab.AddComponent<MellowdyNetworkHandler>();

            NetworkManager.Singleton.AddNetworkPrefab(networkPrefab);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(StartOfRound))]
        [HarmonyPatch("Awake")]
        public static void SpawnNetworkHandler()
        {
            if (GetIsHostOrServer())
            {
                var networkHandlerHost = GameObject.Instantiate(networkPrefab, Vector3.zero, Quaternion.identity);
    
                networkHandlerHost.GetComponent<NetworkObject>().Spawn();
            }
        }

        public static bool GetIsHostOrServer()
        {
            return NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer
        }

        static GameObject networkPrefab;


        [HarmonyPostfix, HarmonyPatch(typeof(RoundManager), nameof(RoundManager.LoadNewLevel))]
        static void SubscribeToHandler()
        {
            MellowdyNetworkHandler.StockValue += ReceivedValueFromServer;
            MellowdyNetworkHandler.StockOwned += ReceivedOwnedFromServer;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(RoundManager), nameof(RoundManager.DespawnPropsAtEndOfRound))]
        static void UnsubscribeFromHandler()
        {
            MellowdyNetworkHandler.StockValue -= ReceivedValueFromServer;
            MellowdyNetworkHandler.StockOwned -= ReceivedOwnedFromServer;
        }

        static void ReceivedValueFromServer(int id, int newValue)
        {
            StockMarketIndex.stocks[id].value = newValue;
        }
        static void ReceivedOwnedFromServer(int id, int owned)
        {
            StockMarketIndex.stocks[id].owned = owned;
        }

        public static void SendValueToClients(int id, int newValue)
        {
            if (!(NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer))
                return;

            MellowdyNetworkHandler.Instance.UpdateStockValueClientRpc(id, newValue);
        }
        public static void SendOwnedToClients(int id, int owned)
        {
            //if (!(NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer))
            //    return;

            MellowdyNetworkHandler.Instance.UpdateStockOwnedClientRpc(id, owned);
        }
    }

}
