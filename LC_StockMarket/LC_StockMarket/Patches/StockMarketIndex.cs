﻿using System;
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
        public static Stock[] stocks;

        public AudioSource audioSource;
        public TextMeshPro stockText;
        public PlayerControllerB previousPlayerHeldBy;

        public static Terminal terminal;

        static float updateTime = 0;

        public override void Start()
        {
            base.Start();



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
            if (terminal == null) terminal = FindObjectOfType<Terminal>();

                UpdateText();
        }
        public static void FindTerminal(Scene scene, LoadSceneMode mode)
        {
            terminal = FindObjectOfType<Terminal>();
        }



        public override void Update()
        {
            base.Update();

            if (NetworkObjectManager.GetIsHostOrServer())
            {

                if (updateTime <= 0)
                {
                    for (int i = 0; i < stocks.Length; i++)
                    {
                        int change = stocks[i].GetCurrentValue();
                        stocks[i].UpdatePrice(Time.time);
                        if (change != stocks[i].GetCurrentValue()) NetworkObjectManager.SendValueToClients(i, stocks[i].GetCurrentValue());
                    }
                    updateTime = 1;
                }
                else updateTime -= Time.deltaTime;
            }
        }

        public override void ItemInteractLeftRight(bool right)
        {
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
            //NextStock();
        }
        public void SellStock()
        {
            if (stocks[id].owned > 0)
            {
                stocks[id].owned--;
                terminal.groupCredits += stocks[id].GetCurrentValue();
                terminal.SyncGroupCreditsServerRpc(terminal.groupCredits, terminal.numberOfItemsInDropship);
                //Do server call to update the stocks owned value
                NetworkObjectManager.SendOwnedToClients(id, stocks[id].owned);
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
                //Do server call to update the stocks owned value
                NetworkObjectManager.SendOwnedToClients(id, stocks[id].owned);
            }
            else
            {
                PlaySAM.SayString($"You are too broke to buy {stocks[id].name}. Broke bitch", audioSource);
            }
            UpdateText();
        }

        public void UpdateText()
        {
            string color = stocks[id].GetDailyGrowth() > 0 ? "green" : "red";

            stockText.text = $"{stocks[id].name}  {stocks[id].GetCurrentValue()}$  <color={color}>{stocks[id].WriteDailyGrowth()}</color>  ({stocks[id].owned})"   +"   " + terminal.groupCredits;
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
        }
        public override void PocketItem()
        {
            base.PocketItem();
            previousPlayerHeldBy.equippedUsableItemQE = false;
        }
    }
}
