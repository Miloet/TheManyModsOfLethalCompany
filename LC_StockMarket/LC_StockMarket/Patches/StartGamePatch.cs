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
        public void OnGameStart()
        {
            List<Stock> list = new List<Stock>();
            foreach (string company in StockMarketIndexMod.Companies)
            {
                list.Add(new Stock(company, new Stock.StockType(Stock.StockType.RandomType())));
            }
            StockMarketIndex.stocks = list.ToArray();
        }
    }
}
