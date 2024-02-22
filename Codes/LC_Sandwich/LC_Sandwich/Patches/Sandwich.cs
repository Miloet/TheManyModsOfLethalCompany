using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using GameNetcodeStuff;

namespace LC_Sandwich
{
    public class Sandwich : PhysicsProp
    {
        //Settings

        public static int sandwichSize = 4; //Amount of times the sandwich can be eaten
        public static float timeToEat = 2f;
        public static int healing = 15;

        public float eatingSpeed = 1f;
        private float eaten = 0; // 0 - 1 value describing how much of it has been eaten
        private int originalValue;

        Material material;
        AudioSource audioSource;
        public static AudioClip eatingSFX;
        public static AudioClip finishSFX;

        public bool eating;

        private PlayerControllerB previousPlayerHeldBy;

        private Coroutine startEating;

        public static Vector3 defaultRotation;
        public static Vector3 defaultOffset;

        public static Item originalProperties;
        public static Item eatingProperties;

        public override void Start()
        {
            base.Start();
            grabbable = true;
            
            useCooldown = 0.3f;
            originalValue = scrapValue;
            material = GetComponent<MeshRenderer>().material;
            material.SetInt("_Size", sandwichSize);

            UpdateEating(0);

            audioSource = GetComponent<AudioSource>();

            eatingSpeed = sandwichSize * timeToEat;

            itemProperties = originalProperties;
            insertedBattery = new Battery(false, 0f);
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);

            if (buttonDown)
            {
                isBeingUsed = true;
                startEating = StartCoroutine(StartEating());
                previousPlayerHeldBy.activatingItem = true;
                previousPlayerHeldBy.playerBodyAnimator.SetBool("useTZPItem", true);
            }
            else
            {
                isBeingUsed = false;
                
                if (startEating != null)
                {
                    StopCoroutine(startEating);
                }
                Stop();
            }
        }
        public override void Update()
        {
            base.Update();

            if(eating)
            {
                if (previousPlayerHeldBy == null || !isHeld || eaten > 1f)
                {
                    eating = false;
                }
                UpdateEating(Mathf.MoveTowards(eaten, 1, Time.deltaTime / eatingSpeed));
                
            }
            if (isHeld && previousPlayerHeldBy == StartOfRound.Instance.localPlayerController)
            {
                insertedBattery.charge = eaten;
                if (base.IsOwner) SyncBatteryServerRpc((int)(insertedBattery.charge * 100f));
            }
            else
            {
                eaten = insertedBattery.charge;
            }
             
            if (eaten >= 1)
            {
                audioSource.PlayOneShot(finishSFX);
                RoundManager.Instance.PlayAudibleNoise(base.transform.position, 15f, 1.5f, 0, isInElevator && StartOfRound.Instance.hangarDoorsClosed);
                if (base.IsOwner) SyncBatteryServerRpc(100);
                DestroyObjectInHand(previousPlayerHeldBy);
                enabled = false;
            }
        }

        public void UpdateEating(float newValue)
        {
            if (Mathf.Floor(newValue * sandwichSize) / sandwichSize != Mathf.Floor(eaten * sandwichSize) / sandwichSize)
            {
                //play funny sound
                Stop();
                SetScrapValue((int)Mathf.Lerp(0, originalValue, eaten));
                previousPlayerHeldBy.health = Mathf.Clamp(previousPlayerHeldBy.health + healing, 0, 100);
                HUDManager.Instance.UpdateHealthUI(previousPlayerHeldBy.health, false);
            }
            
            eaten = newValue;
            material.SetFloat("_Eating", eaten);
        }

        public override void EquipItem()
        {
            base.EquipItem();
            if (playerHeldBy != null)
            {
                previousPlayerHeldBy = playerHeldBy;
            }
        }
        public override void DiscardItem()
        {
            if (startEating != null)
            {
                StopCoroutine(startEating);
            }
            Stop();

            if (previousPlayerHeldBy != null)
            {
                previousPlayerHeldBy.activatingItem = false;
            }
            base.DiscardItem();
        }

        public void Stop()
        {
            eating = false;
            previousPlayerHeldBy.activatingItem = false;
            audioSource.Stop();
            previousPlayerHeldBy.playerBodyAnimator.SetBool("useTZPItem", false);
            itemProperties = originalProperties;
            eatingProperties.rotationOffset = defaultRotation;
            eatingProperties.positionOffset = defaultOffset;
        }


        public static Vector3 fullSandwichEatingPosition = 
            new Vector3(-0.2f, 0.3f, 0.2f);
        public static Vector3 OneBiteLeftSandwichEatingPosition = 
            new Vector3(0.5f, 0.2f, -0.4f);
        public static Vector3 SandwichEatingRotation = 
            new Vector3(0, -45, 0);


        public IEnumerator<WaitForEndOfFrame> StartEating()
        {
            float time = 0;

            float duration = .5f;

            Vector3 position = Vector3.Lerp(fullSandwichEatingPosition, OneBiteLeftSandwichEatingPosition, Mathf.Floor(eaten * sandwichSize) / sandwichSize);
            itemProperties = eatingProperties;
            while (time < duration)
            {
                float t = Mathf.InverseLerp(0, duration, time);

                eatingProperties.rotationOffset = Vector3.Lerp(defaultRotation, SandwichEatingRotation, t);
                eatingProperties.positionOffset = Vector3.Lerp(defaultOffset, position, t);

                time += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            eating = true;
            audioSource.PlayOneShot(eatingSFX);
            audioSource.pitch = eatingSFX.length / timeToEat;
            RoundManager.Instance.PlayAudibleNoise(base.transform.position, 10f, 1f, 0, isInElevator && StartOfRound.Instance.hangarDoorsClosed);
        }


    }
}
