using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SAM;
using UnityEngine.SceneManagement;
using GameNetcodeStuff;
using Unity.Netcode;


namespace LC_StockMarketIndex.Patches
{
    public class StockMarketIndex : GrabbableObject
    {
        //Stocks

        public int id;
        public static Stock[] stocks = new Stock[0];

        public AudioSource audioSource;
        public TextMeshPro stockText;
        public PlayerControllerB previousPlayerHeldBy;

        public static Terminal terminal;
        public static TimeOfDay timeScript;


        public static int MarketOpen = 8;
        public static int MarketClose = 17;

        float updateTime = 0;

        public override void Start()
        {
            base.Start();

            //Write all Existing 
            foreach(var g in StartOfRound.Instance.allItemsList.itemsList)
            {
                StockMarketIndexMod.mls.LogMessage($"{g.itemName}");
            }

            //Assure correct spawning

            if (isInShipRoom && isInElevator)
            {
                transform.SetParent(StartOfRound.Instance.elevatorTransform, false);
                targetFloorPosition = transform.localPosition - transform.parent.position;
            }

            //Getting components

            mainObjectRenderer = GetComponent<MeshRenderer>();
            audioSource = GetComponent<AudioSource>();
            stockText = GetComponentInChildren<TextMeshPro>();
            stockText.font = FindObjectOfType<TextMeshProUGUI>().font;
            if (terminal == null) terminal = FindObjectOfType<Terminal>();
            if(timeScript == null) timeScript = FindObjectOfType<TimeOfDay>();

            if (stocks.Length <= 0)
                StartGamePatch.CreateStocks();

                UpdateText();
        }
        public static void FindTerminal(Scene scene, LoadSceneMode mode)
        {
            terminal = FindObjectOfType<Terminal>();
        }



        public override void Update()
        {
            base.Update();
            
            if (updateTime <= 0)
            {
                updateTime = 1;
                UpdateText();
                if (!CanTrade()) return;
                for (int i = 0; i < stocks.Length; i++)
                {
                    int change = stocks[i].GetCurrentValue();
                    stocks[i].UpdatePrice(Time.time);
                }
            }
            else updateTime -= Time.deltaTime;
        }

        public override void ItemInteractLeftRight(bool right)
        {
            if(!CanTrade()) return;
            base.ItemInteractLeftRight(right);
            if(right)
            {
                SellStock();
            }
            else
            {
                BuyStock();
            }

        }
        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
            if (!CanTrade()) 
            {
                PlaySAM.SayString("The market isnt open yet, impatient idiot.", audioSource);
                return;
            }
            NextStock();
        }
        public void SellStock()
        {
            if (stocks[id].owned > 0)
            {
                stocks[id].owned--;
                terminal.groupCredits += stocks[id].GetCurrentValue();
                terminal.SyncGroupCreditsServerRpc(terminal.groupCredits, terminal.numberOfItemsInDropship);
            }
            else
            {
                PlaySAM.SayString($"You do not own any shares in {stocks[id].name}. L bozo", audioSource);
            }
            UpdateText();
        }
        public void BuyStock()
        {
            if (stocks[id].GetCurrentValue() <= terminal.groupCredits)
            {
                stocks[id].owned++;
                terminal.groupCredits -= stocks[id].GetCurrentValue();
                terminal.SyncGroupCreditsServerRpc(terminal.groupCredits, terminal.numberOfItemsInDropship);
            }
            else
            {
                PlaySAM.SayString($"You are too broke to buy {stocks[id].name}. Broke bitch", audioSource);
            }
            UpdateText();
        }

        public void UpdateText()
        {
            if (CanTrade())
            {
                string color = stocks[id].GetDailyGrowth() > 0 ? "green" : "red";

                stockText.text = $"{stocks[id].name}  {stocks[id].GetCurrentValue()}$  <color={color}>{stocks[id].WriteDailyGrowth()}</color>  {stocks[id].WriteValue()} ({stocks[id].owned}) ";
            }
            else stockText.text = CantTradeReason();
        }

        public void NextStock()
        {
            id = (int)Mathf.Repeat(id + 1, stocks.Length);
            PlaySAM.SayString(stocks[id].name, audioSource);
            UpdateText();
        }

        public bool CanTrade()
        {
            int num = (int)(timeScript.normalizedTimeOfDay * (60f * timeScript.numberOfHours)) + 360;
            int num2 = (int)Mathf.Floor(num / 60);

            int lastValidTime = (int)Mathf.Clamp(num2, MarketOpen,MarketClose);

            return (num2 == lastValidTime) && playerHeldBy != null && !(playerHeldBy.isInHangarShipRoom || playerHeldBy.isInsideFactory);
        }
        public string CantTradeReason()
        {
            int num = (int)(timeScript.normalizedTimeOfDay * (60f * timeScript.numberOfHours)) + 360;
            int num2 = (int)Mathf.Floor(num / 60);

            int lastValidTime = (int)Mathf.Clamp(num2, MarketOpen, MarketClose);

            if (num2 != lastValidTime) return $"Market is currently closed.\nComeback between {TimeToClock(MarketOpen)} and {TimeToClock(MarketClose)}";
            if (playerHeldBy != null && (playerHeldBy.isInHangarShipRoom || playerHeldBy.isInsideFactory)) return "Weak signal, cannot get market prices. Go outside.";
            return "I should work... if not i am broken :<";
        }


        public string TimeToClock(int time)
        {
            string period = (time < 12) ? "AM" : "PM";
            int hour12 = (time % 12 == 0) ? 12 : time % 12;

            return string.Format("{0:D2}:{1:D2} {2}", hour12, 0, period);
        }

        public override void EquipItem()
        {
            base.EquipItem();
            previousPlayerHeldBy = playerHeldBy;
            previousPlayerHeldBy.equippedUsableItemQE = true;
            UpdateText();
        }
        public override void GrabItem()
        {
            base.GrabItem();
            previousPlayerHeldBy.equippedUsableItemQE = true;
            UpdateText();
        }
        public override void DiscardItem()
        {
            base.DiscardItem();
            previousPlayerHeldBy.equippedUsableItemQE = false;
            previousPlayerHeldBy = null;
        }
        public override void PocketItem()
        {
            base.PocketItem();
            previousPlayerHeldBy.equippedUsableItemQE = false;
        }
    }
}
