using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using SAM;
using UnityEngine.SceneManagement;
using GameNetcodeStuff;

namespace LC_StockMarketIndex.Patches
{
    public class StockMarketIndex : GrabbableObject
    {
        public AudioSource audioSource;
        public TextMeshPro stockText;
        public PlayerControllerB previousPlayerHeldBy;

        int num;

        public static Terminal terminal;

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
            if(terminal == null) terminal = FindObjectOfType<Terminal>();

            UpdateText();
        }
        public static void FindTerminal(Scene scene, LoadSceneMode mode)
        {
            terminal = FindObjectOfType<Terminal>();
        }



        public override void Update()
        {
            base.Update();


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
            num--;
            PlaySAM.SayString("You gained 5$, Big W", audioSource);
            terminal.groupCredits = Mathf.Max(terminal.groupCredits + 5, 0);
            terminal.SyncGroupCreditsServerRpc(terminal.groupCredits, terminal.numberOfItemsInDropship);

            UpdateText();
        }
        public void BuyStock()
        {
            num++;
            PlaySAM.SayString("You Lost 5$, L bozo", audioSource);
            terminal.groupCredits = Mathf.Max(terminal.groupCredits - 5, 0);
            terminal.SyncGroupCreditsServerRpc(terminal.groupCredits, terminal.numberOfItemsInDropship);

            UpdateText();
        }

        public void UpdateText()
        {
            stockText.text = num.ToString() + "   " + terminal.groupCredits + "   " + previousPlayerHeldBy.equippedUsableItemQE.ToString();
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
