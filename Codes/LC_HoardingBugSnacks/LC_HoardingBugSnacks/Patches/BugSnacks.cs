﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;


namespace LC_HoardingBugSnacks.Patches
{
    public class BugSnacks : GrabbableObject
    {
        public static float BugHappiness;
        AudioSource audio;
        public AudioClip shake;
        public override void Start()
        {
            base.Start();
            audio = GetComponent<AudioSource>();
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
            if (buttonDown)
            {
                audio.PlayOneShot(shake);
                playerHeldBy.playerBodyAnimator.SetTrigger("shakeItem");
                MakeBugsDanceServerRpc(transform, 1, 5);
            }
        }

        public override void Update()
        {
            base.Update();
        }

        public override void GrabItemFromEnemy(EnemyAI enemy)
        {
            base.GrabItemFromEnemy(enemy);
            if (enemy != null && enemy is HoarderBugAI)
            {
                var bug = (HoarderBugAI)enemy;
                MakeBugsDanceServerRpc(bug.transform, HoardingBugSnacksMod.danceTime.Value, 100);
                BecomeFriends(bug, HoardingBugSnacksMod.friendlyTime.Value, true, this, playerHeldBy);
            }
        }

        public static void BecomeFriends(HoarderBugAI mainBug, float friendAmount, bool allBugs = true, GrabbableObject held = null, PlayerControllerB player = null)
        {
            mainBug.BecomeFriends(friendAmount, held, player);

            if (allBugs)
                foreach (HoarderBugAI bug in FindObjectsOfType<HoarderBugAI>())
                {
                    bug.BecomeFriends(friendAmount, held, player);
                }
        }
        
        [ServerRpc]
        public static void MakeBugsDanceServerRpc(Transform pos, int time, float range)
        {
            MakeBugsDanceClientRpc(pos, time, range);
        }
        [ClientRpc]
        public static void MakeBugsDanceClientRpc(Transform pos, int time, float range)
        {
            MakeBugsDance(pos, time, range);
        }




        public static void MakeBugsDance(Transform pos, int time, float range)
        {
            if(range > 0.5f)
            {
                var allBugs = FindObjectsOfType<HoarderBugAI>();
                foreach(var bug in allBugs)
                {
                    if (Vector3.Distance(bug.transform.position, pos.position) < range) bug.StartDance(time);
                    //StartCoroutine(bug.Dance(time));
                }
            }
        }
    }

    public static class ExtensionMethod
    {
        public static void BecomeFriends(this HoarderBugAI original, float amount, GrabbableObject held = null,  PlayerControllerB player = null)
        {
            Type type = typeof(HoarderBugAI);

            // Get the private field
            FieldInfo field = type.GetField("annoyanceMeter", BindingFlags.NonPublic | BindingFlags.Instance);

            if (field != null)
            {
                // Get the value of the private field
                float currentValue = (float)field.GetValue(original);

                // Modify the value of the private field
                field.SetValue(original, currentValue - amount);
            }

            original.targetItem = held;
            original.angryAtPlayer = null;
            if(player != null) original.watchingPlayer = player;

            BugSnacks.BugHappiness += amount;
        }

        public static void StartDance(this HoarderBugAI original, float duration)
        {
            float startingY = 2;
            original.StopAllCoroutines();
            original.BecomeFriends(.5f);

            original.StartCoroutine(original.Dance(duration, startingY));
        }

        public static IEnumerator<WaitForEndOfFrame> Dance(this HoarderBugAI original, float duration, float startY)
        {
            float Amplitude = 0.4f;
            float Frequency = Mathf.PI * 10f;
            Transform chestTrans = original.animationContainer.Find("Armature").Find("Abdomen").Find("Chest");
            float time = 0;
            while (time < duration)
            {
                chestTrans.localPosition = new Vector3(
                    chestTrans.localPosition.x, 
                    startY + Mathf.Sin(time * Frequency) * Amplitude,
                    chestTrans.localPosition.z);
                time += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            chestTrans.localPosition = new Vector3(chestTrans.localPosition.x, startY, chestTrans.localPosition.z);

        }
    }
}
