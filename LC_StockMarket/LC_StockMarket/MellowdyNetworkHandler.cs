using System;
using Unity.Netcode;
using UnityEngine;
using Object = UnityEngine.Object;
using LC_StockMarketIndex.Patches;

namespace LC_StockMarketIndex.Patches
{
    public class MellowdyNetworkHandler : NetworkBehaviour
    {
        public static event Action<int, int> StockValue;
        public static event Action<int, int> StockOwned;
        [ClientRpc]
        public void UpdateStockValueClientRpc(int id, int newValue)
        {
            StockValue?.Invoke(id, newValue);
        }
        public void UpdateStockOwnedClientRpc(int id, int owned)
        {
            StockOwned?.Invoke(id, owned);
        }

        public override void OnNetworkSpawn()
        {
            StockValue = null;
            StockOwned = null;

            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
                Instance?.gameObject.GetComponent<NetworkObject>().Despawn();
            Instance = this;

            base.OnNetworkSpawn();
        }

        public static MellowdyNetworkHandler Instance { get; private set; }
    }
}