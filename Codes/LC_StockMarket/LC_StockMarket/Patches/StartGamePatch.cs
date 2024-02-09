using HarmonyLib;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LC_StockMarketIndex.Patches;

namespace LC_StockMarketIndex.Patches
{
    [HarmonyPatch(typeof(RoundManager))]
    internal class StartGamePatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        public static void OnGameStart()
        {
            CreateStocks();
        }

        public static void CreateStocks()
        {
            StockMarketIndex.stocks = Stock.GetStocks();
        }

        [HarmonyPatch(nameof(RoundManager.DespawnPropsAtEndOfRound))]
        [HarmonyPostfix]
        public static void NewDay()
        {
            var rm = RoundManager.Instance;

            GrabbableObject[] scrap = Object.FindObjectsOfType<GrabbableObject>();
            List<GrabbableObject> NoneCollectedScrap = new List<GrabbableObject>();
            List<GrabbableObject> CollectedScrap = rm.scrapCollectedThisRound;
            foreach (GrabbableObject s in scrap)
            {
                if (!s.scrapPersistedThroughRounds && !(s.isInElevator || s.isInShipRoom || s.isPocketed)) NoneCollectedScrap.Add(s);
            }

            List<string> collectedScrap = new List<string>();
            List<string> noneCollectedScrap = new List<string>();

            foreach (GrabbableObject g in CollectedScrap)
            {
                collectedScrap.Add(g.itemProperties.itemName);
            }
            foreach (GrabbableObject g in NoneCollectedScrap)
            {
                noneCollectedScrap.Add(g.itemProperties.itemName);
            }

            foreach (Stock stock in StockMarketIndex.stocks)
            {
                float growth = 0;
                
                if (stock.name == "Kremmer's crematorium LLC") stock.products = StartOfRound.Instance.allPlayerScripts.Select(player => player.playerUsername).ToArray();

                foreach (string product in stock.products)
                {
                    float value1 = collectedScrap.Contains(product) ? CollectedScrap.First(g => g.itemProperties.itemName == product).scrapValue : 0;
                    float value2 = noneCollectedScrap.Contains(product) ? NoneCollectedScrap.First(g => g.itemProperties.itemName == product).scrapValue : 0;
                    growth += collectedScrap.Count(s => s == product) * value1 / stock.moneyToReachBaseGrowth;
                    growth -= noneCollectedScrap.Count(s => s == product) * value2 / stock.moneyToReachBaseGrowth;
                }

                StockMarketIndexMod.mls.LogMessage($"{stock.name} is experiencing a growth of {growth}. This means an effective growth of");

                stock.NextDay(growth);
            }
        }


    }
}
